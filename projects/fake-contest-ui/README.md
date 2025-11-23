# Fake Contest UI (CSRF Attack Demonstration)

This is a malicious web application that demonstrates a **Cross-Site Request Forgery (CSRF)** attack against the Mango Bay Cargo API. It masquerades as a "spin-to-win" contest page but secretly uploads a bogus proof of delivery on behalf of an authenticated pilot.

## Attack Scenario

**The Story:**
A disgruntled competitor wants to sabotage Baloo, one of Mango Bay Cargo's star pilots. They create a fake contest website and send Baloo a link via email: "Congratulations! You won a prize - Click here to claim!"

When Baloo (or any authenticated pilot) visits the malicious site and clicks "Spin to Win," the page:
1. Shows a legitimate-looking spinning wheel animation
2. Behind the scenes, uploads `chicken.webp` as "proof of delivery" for one of Baloo's deliveries
3. Uses Baloo's authentication cookies (sent automatically by the browser)
4. The fake proof replaces the legitimate one, potentially causing disputes or getting Baloo in trouble

## How the Attack Works

### 1. The Malicious Page
- Looks like a typical sleazy marketing site (Temu-style spin-to-win)
- User is prompted to "Spin" for a prize
- Distracts the user with animations and fake urgency ("2,847 people viewing!")

### 2. The Hidden CSRF Attack
When the user clicks spin, the JavaScript code:

```typescript
private async launchAttack() {
  const imageBlob = await fetch('/chicken.webp').then(r => r.blob());
  const file = new File([imageBlob], 'proof-of-delivery.webp', { type: 'image/webp' });
  
  const dataTransfer = new DataTransfer();
  dataTransfer.items.add(file);
  this.fileInput.nativeElement.files = dataTransfer.files;

  this.csrfForm.nativeElement.submit();
}
```

**Critical Detail:** This uses a traditional HTML `<form>` submission, NOT `fetch()` or `XMLHttpRequest`. This is crucial because:
- Form submissions don't trigger CORS preflight
- Browser treats it as a "simple request" and sends cookies automatically
- No Same-Origin Policy check happens before submission
- The attacker can't read the response, but the damage is already done

### 3. Why Traditional CSRF Protections Don't Work Here

**Token-Based Authentication (Current Main Branch):**
- Uses JWT tokens in Authorization headers
- HTML forms cannot set custom headers
- The malicious site cannot access tokens (protected by Same-Origin Policy)
- ❌ **Attack FAILS** on main branch

**Cookie-Based Authentication (Vulnerable Branch):**
- Uses HttpOnly cookies for session management
- Cookies are sent automatically with every request to the API domain
- HTML form submission includes cookies without any JavaScript intervention
- If `SameSite=None` or missing, cookies travel cross-site
- ✅ **Attack SUCCEEDS** on vulnerable branch

## Technical Details

### Target Endpoint
```html
<form 
  action="http://localhost:5110/api/v1/proofs/deliveries/d0000000-0000-0000-0000-ba1000000001/upload"
  method="POST"
  enctype="multipart/form-data">
  <input type="file" name="file" />
</form>
```

This is a standard HTML form - no JavaScript fetch, no custom headers, no CORS preflight. Just a plain form submission that the browser treats as a normal navigation, automatically including any cookies for `localhost:5110`.

### Deterministic Target
The seed data creates a predictable delivery ID for demonstration purposes:
- **Baloo's First Delivery ID:** `d0000000-0000-0000-0000-ba1000000001`
- This ensures the workshop demo works consistently across all environments

### Why `.DisableAntiforgery()` Makes This Possible

The upload endpoint explicitly disables antiforgery validation:

```csharp
proofsGroup.MapPost("/deliveries/{deliveryId}/upload", UploadProofOfDelivery)
    .DisableAntiforgery()  // ← This is the vulnerability!
```

Without antiforgery tokens, there's no way for the server to verify that the request came from the legitimate Mango Bay web app versus an attacker's malicious site.

## Defense Mechanisms

### ✅ What Currently Protects Against This (Main Branch)
1. **Token-based auth** - Attacker can't steal tokens from localStorage/memory
2. **CORS** - Browser blocks reading responses (but doesn't prevent the attack)

### ✅ What Would Protect Against This (Best Practices)
1. **SameSite Cookies** - `SameSite=Lax` or `Strict` prevents cross-site cookie transmission
2. **Antiforgery Tokens** - Server validates a unique token with each request
3. **Custom Headers** - Require headers that HTML forms can't send (triggers CORS preflight)

### ❌ What DOESN'T Protect Against This
1. **CORS alone** - Prevents reading responses but not sending requests
2. **HttpOnly cookies** - Protects from XSS but not CSRF
3. **HTTPS** - Encrypts traffic but doesn't validate request origin

## Running the Attack Demo

### Prerequisites
- Mango Bay API running on `http://localhost:5110`
- User authenticated as a pilot (to have valid cookies)
- Running on a branch with vulnerable cookie configuration

### Steps

1. **Start the fake contest site:**
```bash
cd projects/fake-contest-ui
npm install
npm start
```

2. **Access at:** `http://localhost:6661`

3. **Authenticate as Baloo** in the main app:
   - Email: `baloo@mangobaycargo.com`
   - Password: `One2three`

4. **Visit the fake contest** (in the same browser, different tab)

5. **Click "Spin to Win"** and observe:
   - On main branch: Attack blocked (no cookies or tokens sent)
   - On vulnerable branch: Attack succeeds (cookies sent automatically)

6. **Verify the attack** in the main app:
   - Check Baloo's delivery proof
   - It should now show a chicken instead of the legitimate proof

## Educational Value

This demonstration teaches:
1. **Why SameSite cookies matter** - Not just a "nice to have"
2. **CORS limitations** - It's not a security boundary for authenticated requests
3. **Social engineering** - Real attacks use deception, not just technical exploits
4. **Defense in depth** - Multiple protections work together

## Mitigations in the Workshop

Participants will learn to fix this vulnerability by:
1. Enabling `SameSite=Lax` on authentication cookies
2. Re-enabling antiforgery validation for form endpoints
3. Understanding when to use token-based vs. cookie-based auth
4. Implementing CSRF protection middleware

## Port Configuration

The fake contest runs on **port 6661** to avoid conflicts:
- Main web app: `4200`
- API: `5110`
- Fake file host: `5280`
- **Fake contest: `6661`** ← The "evil" number for an evil purpose 😈

## Files of Interest

- `src/app/app.ts` - Attack logic
- `public/chicken.webp` - The bogus "proof of delivery"
- `../../api/src/MBC.Services/Seeds/Data/SeedIds.cs` - Deterministic delivery ID
- `../../api/src/MBC.Endpoints/Endpoints/DeliveryProofEndpoints.cs` - Vulnerable endpoint

## Disclaimer

This code is for **educational purposes only** as part of the "Securing the Whole Stack" workshop. It demonstrates real vulnerabilities in a controlled environment to teach secure development practices.

**Never use this technique against real applications without explicit authorization.**
