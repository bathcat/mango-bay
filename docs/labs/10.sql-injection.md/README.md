# Lab 10: SQL Injection Hardening

## Summary

SQL injection vulnerabilities can be catastrophic, especially when attackers can modify or exfiltrate data from critical tables. In the vulnerable branch, the delivery search feature uses a stored procedure in a way that allows malicious input to alter the SQL command. In this lab, you'll harden that search so that cargo description queries are no longer exploitable.

## Requirements

1. Run the vulnerable branch with SQL Server enabled

   - Switch to the `vulnerability/sql-injection` branch
   - Configure the API to use SQL Server in `appsettings.json` (set the provider and connection string appropriately)
   - Make sure the app starts cleanly with SQL Server
   - If you don't have SQL Server locally, either:
     - Pair with someone who does and share their API, or
     - Use Docker / Codespaces, which already support SQL Server for this workshop (appsettings should already be set up proprly)

2. Reproduce the vulnerability

   - Use the SQL injection examples from `sql-injection.README.md`
   - Trigger the search endpoint via the UI at `http://localhost:4200/search-cargo?q=...`
   - Observe how certain payloads can:
     - Destroy or modify data (DML/DDL attacks), and/or
     - Change behavior through timing or error-based attacks

3. Analyze the attack surface

   - Inspect `DeliveryStore.SearchByCargoDescriptionSproc` and how it builds and executes the stored procedure call
   - Understand where user-controlled `searchTerm` flows into the query
   - Confirm that the SQL Server code path is actually used (the EF path is not the vulnerable one)

4. Fix the vulnerability

   - Make `SearchByCargoDescription` safe against SQL injection
   - Possible approaches include:
     - Correctly parameterizing the stored procedure call
     - Refactoring to use a parameterized EF Core query instead of raw command text
     - Adding server-side validation/sanitization to reject obviously dangerous `searchTerm` inputs
   - You do not need a perfect real-world solution here; a reasonable, effective mitigation that closes the lab exploits is sufficient

5. Validate your fix

   - Re-run the attack strings from `sql-injection.README.md` against the search-cargo feature
   - Confirm that:
     - SQL payloads no longer execute as additional commands
     - Data is not destroyed or modified by those inputs
     - Timing / error-based tricks no longer behave as a side-channel into the database

## Acceptance Criteria

- The search page at `http://localhost:4200/search-cargo?q=rocks` still works as expected for normal input
- Replaying the sample SQL injection payloads from `sql-injection.README.md` no longer results in:
  - Dropped or deleted tables
  - Mass data deletion or modification
  - Time-based or error-based proof of SQL injection
- In other words, searching by cargo description is no longer vulnerable to SQL injection.

## Tips

- **You already know the schema**: Unlike real attackers doing blind SQL injection, you have the table and column shapes in front of you. Use that to reason about what an exploit would do and how to neutralize it.
- **Start by confirming the SQL Server path**: If the app is still using Sqlite, the vulnerable code path in `DeliveryStore.SearchByCargoDescriptionSproc` will never run.
- **Don't stress about breaking the database**: If you trash the data with an attack, you can delete the database in SQL Server Management Studio and let the app recreate and reseed it on the next startup.
- **Think about defense in depth**: Parameterization is the gold standard, but server-side input validation and conservative stored procedure interfaces also help reduce risk.

## Learning Resources

1. [OWASP SQL Injection Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html)
2. [EF Core Raw SQL Queries Documentation](https://learn.microsoft.com/en-us/ef/core/querying/raw-sql)
3. [SQL Server Stored Procedures and Parameters](https://learn.microsoft.com/en-us/sql/relational-databases/stored-procedures)