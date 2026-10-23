# SnapCode Comprehensive System Verification & Gap Analysis Script
$ErrorActionPreference = "Continue"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " SNAPCODE COMPREHENSIVE AUTOMATED VERIFICATION SUITE" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""

$results = [System.Collections.Generic.List[PSCustomObject]]::new()
$total = 0
$passed = 0
$failed = 0

function Assert-Test {
    param(
        [string]$Name,
        [bool]$Condition,
        [string]$Detail = ""
    )
    $script:total++
    if ($Condition) {
        $script:passed++
        Write-Host "  [PASS] $Name" -ForegroundColor Green
        $script:results.Add([PSCustomObject]@{ Name = $Name; Status = "PASS"; Detail = "" })
    } else {
        $script:failed++
        Write-Host "  [FAIL] $Name - $Detail" -ForegroundColor Red
        $script:results.Add([PSCustomObject]@{ Name = $Name; Status = "FAIL"; Detail = $Detail })
    }
}

# ----------------------------------------------------
# 1. C# COMPILATION
# ----------------------------------------------------
Write-Host ">>> 1. Verifying C# Solution Compilation (SnapCode.csproj)..." -ForegroundColor Yellow
$buildOutput = dotnet build "src\Snapcode.csproj" -c Debug 2>&1
$buildSuccess = ($LASTEXITCODE -eq 0)
Assert-Test -Name "C# SnapCode.csproj compiles with 0 errors" -Condition $buildSuccess -Detail ($buildOutput | Select-String "error" | Out-String)

# ----------------------------------------------------
# 2. FLUTTER SDK & TOOLCHAIN
# ----------------------------------------------------
Write-Host "`n>>> 2. Verifying Flutter SDK & CLI Toolchain..." -ForegroundColor Yellow
$flutterVersion = flutter --version 2>&1
Assert-Test -Name "Flutter CLI available and responsive" -Condition ($LASTEXITCODE -eq 0)

# Check flutter devices
$devicesJson = flutter devices --machine 2>&1
$hasDevices = $false
try {
    $parsedDevices = $devicesJson | ConvertFrom-Json
    $hasDevices = ($parsedDevices.Count -gt 0)
} catch {
    $hasDevices = $false
}
Assert-Test -Name "flutter devices --machine returns valid JSON devices" -Condition $hasDevices -Detail "Devices count: $($parsedDevices.Count)"

# Check Android emulators
$emuOutput = flutter emulators 2>&1
$emuFound = ($emuOutput -match "available emulator") -or ($emuOutput -match "Pixel")
Assert-Test -Name "Android Emulator discovery via Flutter CLI" -Condition $emuFound -Detail "Output: $emuOutput"

# ----------------------------------------------------
# 3. DART CODE GENERATION & COMPILATION INTEGRITY
# ----------------------------------------------------
Write-Host "`n>>> 3. Verifying Generated Flutter Dart Code Compilation..." -ForegroundColor Yellow
$testRepoPath = "C:\Users\hardi\Documents\snapcode\repos\newapp\flutter"
if (Test-Path $testRepoPath) {
    # Run dart analyze on the actual generated flutter application
    $dartAnalyze = dart analyze "$testRepoPath" 2>&1
    $analyzeSuccess = ($LASTEXITCODE -eq 0) -and ($dartAnalyze -match "No issues found")
    Assert-Test -Name "dart analyze reports 0 issues on generated main.dart" -Condition $analyzeSuccess -Detail ($dartAnalyze | Out-String)

    # Verify key code generation patterns in main.dart
    $mainDartContent = Get-Content "$testRepoPath\lib\main.dart" -Raw

    Assert-Test -Name "main.dart has auto-generated TextEditingController" -Condition ($mainDartContent -match "final TextEditingController _ctrl")
    Assert-Test -Name "main.dart disposes controllers in dispose()" -Condition ($mainDartContent -match "\.dispose\(\);")
    Assert-Test -Name "main.dart has reactive setState on interactive widgets" -Condition ($mainDartContent -match "setState\(")
    Assert-Test -Name "main.dart does NOT have naked showDialog outside method body" -Condition (-not ($mainDartContent -match "//\s*EVENT_BINDING:.*\r?\n\s*showDialog"))
    Assert-Test -Name "main.dart has properly scoped event handler method" -Condition ($mainDartContent -match "void\s+on\w+\s*\([^)]*\)\s*\{")
    Assert-Test -Name "main.dart has preservation markers for CLASS_MEMBERS" -Condition ($mainDartContent -match "USER_CODE_START: CLASS_MEMBERS")
    Assert-Test -Name "main.dart has preservation markers for CUSTOM_METHODS" -Condition ($mainDartContent -match "USER_CODE_START: CUSTOM_METHODS")
} else {
    Assert-Test -Name "Flutter test repository exists at $testRepoPath" -Condition $false -Detail "Path not found"
}

# ----------------------------------------------------
# 4. SAFETY & COMPLIANCE (No double.NaN on MinWidth)
# ----------------------------------------------------
Write-Host "`n>>> 4. Verifying WinUI 3 MinWidth/MinHeight Safety Constraints..." -ForegroundColor Yellow
$doubleNanMatches = Select-String -Path "src\**\*.cs" -Pattern "(MinWidth|MinHeight)\s*=\s*double\.NaN"
Assert-Test -Name "Zero occurrences of double.NaN assigned to MinWidth/MinHeight (WinRT constraint)" -Condition ($doubleNanMatches.Count -eq 0) -Detail "Found $($doubleNanMatches.Count) occurrences"

# ----------------------------------------------------
# 5. NEW FILES & SERVICES INTEGRITY
# ----------------------------------------------------
Write-Host "`n>>> 5. Verifying New Services & UI Controls..." -ForegroundColor Yellow
$requiredFiles = @(
    "src\Models\FlutterDevice.cs",
    "src\Models\DebugLogEntry.cs",
    "src\Services\FlutterExecutionService.cs",
    "src\Views\DebugConsoleControl.xaml",
    "src\Views\DebugConsoleControl.xaml.cs",
    "src\Views\DebugLogsWindow.xaml",
    "src\Views\DebugLogsWindow.xaml.cs",
    "src\Views\EmulatorManagerDialog.xaml",
    "src\Views\EmulatorManagerDialog.xaml.cs"
)

foreach ($f in $requiredFiles) {
    Assert-Test -Name "File exists: $f" -Condition (Test-Path $f)
}

# ----------------------------------------------------
# 6. SUMMARY & METRICS
# ----------------------------------------------------
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " VERIFICATION RESULTS: $passed / $total PASSED ($([math]::Round(($passed/$total)*100, 1))%)" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
Write-Host "==========================================================" -ForegroundColor Cyan

# Output markdown report
$reportMd = @"
# Automated Comprehensive System Verification Report
*Generated at: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")*

## Summary
- **Total Tests**: $total
- **Passed**: $passed ($([math]::Round(($passed/$total)*100, 1))%)
- **Failed**: $failed

$($results | ForEach-Object { 
    $check = if ($_.Status -eq 'PASS') { 'x' } else { ' ' }
    $detailText = if ($_.Detail) { " - *$($_.Detail)*" } else { "" }
    "- [$check] **$($_.Status)**: $($_.Name)$detailText"
} | Out-String)
"@

$reportPath = "comprehensive_system_verification_report.md"
Set-Content -Path $reportPath -Value $reportMd -Encoding utf8
Write-Host "Full Markdown Report saved to: $reportPath" -ForegroundColor Cyan
