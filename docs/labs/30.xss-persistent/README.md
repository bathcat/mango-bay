# Lab 30: Persistent XSS Mitigation

## Summary

Persistent (Stored) XSS is one of the most dangerous web vulnerabilities because malicious scripts are permanently stored in the database and execute for every user who views the content. Unlike DOM-based XSS that affects individual victims through malicious links, persistent XSS creates a persistent attack vector that compromises all users automatically. In this lab, you'll fix a vulnerability in the pilot review feature where unsanitized HTML in review notes executes scripts for everyone viewing the review.

## Requirements

1. Switch to the vulnerable branch and reproduce the vulnerability

   - Check out `vulnerability/xss-persistent`
   - Complete a delivery so you can submit a review
   - In the review form, toggle to "Raw HTML" mode
   - Submit one of the basic XSS payloads from `xss-persistent.README.md`
   - View the review and observe script execution

2. Understand the attack surface

   - Test submitting malicious payloads through the web form
   - Try bypassing client-side validation by calling the API directly (browser console or PowerShell)
   - Understand why client-side validation alone is insufficient
   - Identify where the vulnerability exists: storage, retrieval, or rendering

3. Fix the vulnerability with defense in depth

   - **Server-side**: Sanitize or validate review notes before storing in the database
   - **Client-side**: Safely render review notes when displaying them to users
   - Consider whether you need to support any legitimate HTML (formatting, links) or strip it entirely
   - Ensure the fix applies to both the web form submission and direct API calls

4. Validate your fix

   - Test all XSS payloads from `xss-persistent.README.md`
   - Try bypassing the fix with direct API calls
   - Confirm that:
     - Scripts in review notes don't execute
     - Token exfiltration attempts fail
     - Keyloggers don't install
     - Legitimate review content still displays correctly

## Acceptance Criteria

- Normal reviews with plain text or safe formatting display correctly
- All XSS payloads from `xss-persistent.README.md` are neutralized:
  - Basic alert payloads don't execute
  - Session hijacking attempts fail
  - Keyloggers don't install
  - Backdoor attempts are blocked
- The fix works regardless of how the review is submitted (web form, browser console, or PowerShell script)
- Existing malicious reviews in the database are safely rendered (the fix applies to display, not just input)

## Tips

- **Server-side sanitization is critical**: Client-side validation can always be bypassed. The API must sanitize or validate before storing data.
- **Defense in depth**: Sanitize on both input (server-side) and output (client-side rendering). This provides multiple layers of protection.
- **Angular's built-in protection**: Using Angular template binding automatically sanitizes content. The vulnerability likely exists because code is using `innerHTML` or similar unsafe APIs.
- **Consider your HTML policy**: Do you need to support formatted text in reviews? If not, strip all HTML. If yes, use a whitelist-based sanitizer that allows only safe tags.
- **Test the API directly**: Don't just test through the UI. Use browser console or PowerShell to verify the server-side fix works.

## Learning Resources

1. [OWASP XSS Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html) - Comprehensive XSS defense strategies
2. [Angular Security Guide](https://angular.io/guide/security) - Built-in XSS protection and DomSanitizer
3. [HTML Sanitization Libraries](https://github.com/cure53/DOMPurify) - Client-side HTML sanitization
4. [.NET HTML Encoding](https://learn.microsoft.com/en-us/dotnet/api/system.web.httputility.htmlencode) - Server-side sanitization options
