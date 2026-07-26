# Unity License Activation Script
# Attempts to get a personal license via Unity API

$ErrorActionPreference = "Continue"

# Step 1: Try Unity ID login
Write-Host "=== Step 1: Unity ID Login ==="
$loginBody = @{
    email = "lvguanbing@gmail.com"
    password = "aA5130778."
} | ConvertTo-Json

try {
    $loginResp = Invoke-RestMethod -Uri "https://api.unity.com/v1/core/login" -Method POST -Body $loginBody -ContentType "application/json" -UseBasicParsing
    Write-Host "Login successful!"
    $loginResp | ConvertTo-Json -Depth 5
} catch {
    Write-Host "Login API error: $($_.Exception.Message)"
    Write-Host "Response: $($_.ErrorDetails.Message)"
}

# Step 2: Try alternative auth endpoint
Write-Host "`n=== Step 2: Alternative Auth ==="
try {
    $authResp = Invoke-WebRequest -Uri "https://id.unity.com/api/login" -Method POST -Body $loginBody -ContentType "application/json" -UseBasicParsing -SessionVariable session
    Write-Host "Auth status: $($authResp.StatusCode)"
    Write-Host "Content: $($authResp.Content.Substring(0, [Math]::Min(500, $authResp.Content.Length)))"
} catch {
    Write-Host "Auth error: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $errBody = $reader.ReadToEnd()
        Write-Host "Error body: $errBody"
    }
}

# Step 3: Check if we can access license API
Write-Host "`n=== Step 3: License API ==="
try {
    $licResp = Invoke-WebRequest -Uri "https://license.unity3d.com/" -UseBasicParsing -TimeoutSec 10
    Write-Host "License API status: $($licResp.StatusCode)"
} catch {
    Write-Host "License API: $($_.Exception.Message)"
}

# Step 4: Check Unity Hub local API (if running)
Write-Host "`n=== Step 4: Unity Hub Local ==="
$hubPaths = @(
    "$env:APPDATA\Unity Hub",
    "$env:LOCALAPPDATA\Programs\Unity Hub",
    "C:\Program Files\Unity Hub"
)
foreach ($p in $hubPaths) {
    if (Test-Path $p) {
        Write-Host "Found: $p"
        Get-ChildItem $p -Recurse -Filter "*.json" -ErrorAction SilentlyContinue | Select-Object -First 5 | ForEach-Object { Write-Host "  $($_.FullName)" }
    }
}

# Check for existing license files
Write-Host "`n=== Checking for existing licenses ==="
$licPaths = @(
    "C:\ProgramData\Unity\Unity_lic.ulf",
    "$env:APPDATA\Unity\Unity_lic.ulf",
    "$env:LOCALAPPDATA\Unity\Unity_lic.ulf"
)
foreach ($p in $licPaths) {
    if (Test-Path $p) {
        Write-Host "FOUND LICENSE: $p"
        Get-Content $p | Select-Object -First 5
    }
}
