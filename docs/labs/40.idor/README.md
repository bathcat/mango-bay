# Lab 40: IDOR and Authorization Hardening

## Summary

Insecure Direct Object Reference (IDOR) vulnerabilities occur when applications expose internal identifiers and fail to verify that users are authorized to access those resources. This vulnerability is particularly dangerous when combined with information disclosure that leaks those identifiers to unauthorized parties. In this lab, you'll fix a multi-layered vulnerability where review endpoints leak customer IDs, and customer endpoints lack both authentication and authorization checks, allowing anyone to read and modify customer data.

## Requirements

1. Switch to the vulnerable branch and reproduce the vulnerability

   - Check out `vulnerability/idor`
   - Follow the manual browser-based attack from `idor.README.md`
   - Use browser console to fetch pilot reviews and observe exposed customer IDs
   - Use those IDs to access customer data without authentication
   - Optionally, run the automated PowerShell script to see mass exploitation

2. Identify the vulnerability chain

   - **Information Disclosure**: Review endpoints return full entities with internal IDs
   - **Missing Authentication**: Customer endpoints accessible without login
   - **Missing Authorization**: No verification that the requester owns the resource
   - Map out which endpoints are vulnerable and what data they expose

3. Implement defense in depth

   - **Reduce information disclosure**: Use DTOs that don't expose sensitive internal IDs in review endpoints
   - **Add authentication**: Require valid JWT tokens for customer data access
   - **Add authorization**: Verify that authenticated users can only access their own data (or have admin privileges)
   - Consider which fields truly need to be public vs. private

4. Validate your fix

   - Test the browser-based attack from `idor.README.md`
   - Try the automated PowerShell script
   - Confirm that:
     - Unauthenticated requests to customer endpoints are rejected
     - Authenticated users can only access their own data
     - Customer IDs are no longer leaked through review endpoints
     - Legitimate operations (users viewing their own profiles) still work

## Acceptance Criteria

- Unauthenticated requests to customer endpoints return 401 Unauthorized
- Authenticated users trying to access other users' data receive 403 Forbidden
- Review endpoints no longer expose customer IDs or other sensitive identifiers
- The browser-based attack from `idor.README.md` fails at multiple stages
- The automated PowerShell script cannot harvest customer IDs or deface profiles
- Users can still access and update their own customer profiles when properly authenticated

## Tips

- **Multiple layers of defense**: This vulnerability has several weaknesses. Fix all of them—reducing information disclosure, adding authentication, and enforcing authorization.
- **DTOs are your friend**: Never return raw database entities through public APIs. Use Data Transfer Objects that expose only the fields intended for public consumption.
- **Authorization is not authentication**: Even authenticated users shouldn't access resources they don't own. Check ownership or roles before returning data.
- **Think like an attacker**: Even if you fix the customer endpoints, are there other places where customer IDs leak? Check all public endpoints.
- **Test from multiple angles**: Try accessing data as an unauthenticated user, as a different customer, and as the correct customer. All three scenarios should behave differently.

## Learning Resources

1. [OWASP IDOR Guide](https://owasp.org/www-project-web-security-testing-guide/latest/4-Web_Application_Security_Testing/05-Authorization_Testing/04-Testing_for_Insecure_Direct_Object_References) - Understanding and preventing IDOR
2. [ASP.NET Core Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction) - Implementing authorization in .NET
3. [JWT Authentication in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/) - Token-based authentication
4. [API Security Best Practices](https://owasp.org/www-project-api-security/) - OWASP API Security Top 10

