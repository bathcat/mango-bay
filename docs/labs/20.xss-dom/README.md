# Lab 20: DOM-Based XSS Mitigation

## Summary

DOM-Based XSS vulnerabilities occur when client-side JavaScript code unsafely manipulates the DOM with user-controlled data. Unlike reflected or stored XSS, the malicious payload never reaches the server—it's entirely executed in the browser. In this lab, you'll fix a search results component that directly inserts URL query parameters into the page using `innerHTML`, bypassing Angular's built-in XSS protections.

## Requirements

1. Switch to the vulnerable branch and reproduce the vulnerability

   - Check out `vulnerability/xss-dom`
   - Navigate to the search page: `http://localhost:4200/search?q=test`
   - Try the basic XSS payload from `xss-dom.README.md` to confirm script execution

2. Locate and analyze the vulnerable code

   - Find `SearchResultsComponent.ngAfterViewInit()` in the Angular codebase
   - Identify where user input from the URL query parameter is being inserted into the DOM
   - Understand why using `innerHTML` with unsanitized user input is dangerous
   - Note how this bypasses Angular's automatic XSS protection

3. Fix the vulnerability

   - Refactor the code to safely display the search term
   - Possible approaches:
     - Use Angular template binding (which auto-sanitizes)
     - Use Angular's `DomSanitizer` if raw HTML is legitimately needed
     - Avoid `innerHTML` and use safer DOM APIs like `textContent`
     - Sanitize input before rendering

4. Validate your fix

   - Test all the XSS payloads from `xss-dom.README.md`
   - Confirm that:
     - The basic alert payload no longer executes
     - Token exfiltration attempts fail
     - Keylogger injection is blocked
     - Search functionality still works for legitimate queries

## Acceptance Criteria

- Normal search queries like `http://localhost:4200/search?q=rocks` display correctly
- The XSS payloads from `xss-dom.README.md` no longer execute JavaScript:
  - Alert boxes don't appear
  - Tokens aren't exfiltrated to attacker servers
  - Keyloggers don't get installed
  - Redirects don't occur
- The search term is displayed safely on the page, even if it contains HTML-like characters

## Tips

- **Angular protects you by default**: When you use Angular template binding (`{{ }}` or `[property]`), it automatically sanitizes content. The vulnerability exists because the code uses vanilla JavaScript DOM manipulation.
- **innerHTML is dangerous**: Any time you see `innerHTML` with user input, it's a red flag. There are very few legitimate reasons to use it.
- **Check the Network tab**: When testing token exfiltration payloads, watch the Network tab to verify that requests to the attacker's server (localhost:5155) are blocked after your fix.
- **Test with special characters**: Make sure legitimate searches with characters like `<`, `>`, `&` still display correctly without executing as code.

## Learning Resources

1. [Angular Security Guide](https://angular.io/guide/security) - Built-in XSS protection and sanitization
2. [OWASP DOM-Based XSS](https://owasp.org/www-community/attacks/DOM_Based_XSS) - Understanding DOM-based attacks
3. [Angular DomSanitizer](https://angular.io/api/platform-browser/DomSanitizer) - When and how to use sanitization APIs
4. [Content Security Policy (CSP)](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP) - Defense-in-depth approach to XSS
