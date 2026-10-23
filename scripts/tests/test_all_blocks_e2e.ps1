# SnapCode Comprehensive End-to-End Block Testing & Verification Suite
$ErrorActionPreference = "Continue"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " SNAPCODE E2E BLOCK SYSTEM TEST & VERIFICATION" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""

$testsPassed = 0
$testsFailed = 0

function Assert-Check {
    param(
        [string]$Title,
        [bool]$Condition,
        [string]$Details = ""
    )
    if ($Condition) {
        $script:testsPassed++
        Write-Host "  [PASS] $Title" -ForegroundColor Green
    } else {
        $script:testsFailed++
        Write-Host "  [FAIL] $Title - $Details" -ForegroundColor Red
    }
}

# 1. Load SnapCode compiled assembly via reflection
Write-Host ">>> 1. Loading SnapCode Assembly for Verification..." -ForegroundColor Yellow
$snapcodeDll = "c:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\vs files\bin\Debug\net8.0-windows10.0.19041.0\SnapCode.dll"
if (-not (Test-Path $snapcodeDll)) {
    Write-Host "SnapCode DLL not found at $snapcodeDll. Building project first..." -ForegroundColor Yellow
    dotnet build "vs files\SnapCode.csproj" -c Debug
}

Assert-Check -Title "SnapCode.dll exists" -Condition (Test-Path $snapcodeDll) -Details $snapcodeDll

Add-Type -Path $snapcodeDll

# 2. Test UI JSON from the user's project
Write-Host "`n>>> 2. Loading Project UI and Event Workspaces..." -ForegroundColor Yellow
$projectRoot = "C:\Users\hardi\Documents\Snapcode\repos\new_project_test"
$mainUiPath = "$projectRoot\.snapcode\pages\main_ui.json"
$workspacePath = "$projectRoot\.snapcode\workspace.json"
$buttonClickPath = "$projectRoot\.snapcode\pages\main_button_09ac_onClick.json"

Assert-Check -Title "main_ui.json exists" -Condition (Test-Path $mainUiPath)
Assert-Check -Title "workspace.json exists" -Condition (Test-Path $workspacePath)
Assert-Check -Title "main_button_09ac_onClick.json exists" -Condition (Test-Path $buttonClickPath)

$mainUiJson = Get-Content $mainUiPath -Raw

# 3. Simulate Combined Event Code from Multiple Workspaces
Write-Host "`n>>> 3. Testing Multi-Event Code Generation & Aggregation..." -ForegroundColor Yellow

# Workspace 1: Column click -> change text
$columnClickEvent = @"
// EVENT_BINDING: onClick -> column_1
setState(() {
  _textF2c6Text = 'This is the button click text change!!!';
});
"@

# Workspace 2: Button click -> Show Alert Dialog
$buttonDialogEvent = @"
// EVENT_BINDING: onClick -> button_09ac
showDialog(
  context: context,
  builder: (ctx) => AlertDialog(
    title: const Text('Alert'),
    content: const Text('Are you sure?'),
    actions: [
      TextButton(
        onPressed: () => Navigator.of(ctx).pop(),
        child: const Text('OK'),
      ),
    ],
  ),
);
"@

# Workspace 3: Button long press -> Show SnackBar
$buttonSnackbarEvent = @"
// EVENT_BINDING: onLongPress -> button_09ac
ScaffoldMessenger.of(context).showSnackBar(
  const SnackBar(content: Text('Button long-pressed successfully!')),
);
"@

# Workspace 4: Slider changed -> update dynamic value
$sliderChangedEvent = @"
// EVENT_BINDING: onChanged(double value) -> slider_b124
setState(() {
  _sliderB124Value = value;
  sliderB124Value = value;
});
"@

# Workspace 5: Layout blocks -> change layout color and padding dynamically
$layoutEvents = @"
// EVENT_BINDING: onClick -> row_431c
setState(() {
  _row431cBgColor = const Color(0xFF4CAF50);
  _column1Padding = 24.0;
  _column1BgColor = const Color(0xFFF5F5F5);
});
"@

$combinedEventCode = "$columnClickEvent`n`n$buttonDialogEvent`n`n$buttonSnackbarEvent`n`n$sliderChangedEvent`n`n$layoutEvents"

# 4. Generate Complete Flutter Page Dart via Compiler Service
Write-Host "`n>>> 4. Compiling Complete Flutter Page Dart via FlutterCompilerService..." -ForegroundColor Yellow
$pageMeta = [Snapcode.Models.PageMetadata]::new()
$pageMeta.Title = "Hardik Prajapati"
$pageMeta.ShowAppBar = $true
$pageMeta.ShowDrawer = $true

$generatedDart = [SnapCode.Services.FlutterCompilerService]::GenerateCompletePageDart(
    "main",
    $mainUiJson,
    $combinedEventCode,
    $pageMeta
)

Assert-Check -Title "Dart source code was generated" -Condition (![string]::IsNullOrWhiteSpace($generatedDart))

# 5. Verify Critical Requirements in Generated Dart Code
Write-Host "`n>>> 5. Verifying Architecture & Runtime Fixes in Generated Code..." -ForegroundColor Yellow

# (a) HitTestBehavior.opaque on GestureDetector for Column
$hasOpaqueGesture = $generatedDart -match "GestureDetector\(behavior:\s*HitTestBehavior\.opaque,\s*onTap:\s*\(\)\s*=>\s*onClickColumn1\(context\)"
Assert-Check -Title "Column GestureDetector has behavior: HitTestBehavior.opaque" -Condition $hasOpaqueGesture -Details "Found in code: $hasOpaqueGesture"

# (b) Method onClickColumn1 exists and is properly formed
$hasOnClickColumn1 = $generatedDart -match "void\s+onClickColumn1\(BuildContext\s+context\)\s*\{"
Assert-Check -Title "onClickColumn1 method generated with BuildContext context" -Condition $hasOnClickColumn1

# (c) Method onClickButton09ac exists with showDialog
$hasOnClickButton = $generatedDart -match "void\s+onClickButton09ac\(BuildContext\s+context\)\s*\{"
Assert-Check -Title "onClickButton09ac method generated with BuildContext context" -Condition $hasOnClickButton

$hasShowDialog = $generatedDart -match "showDialog\(\s*context:\s*context,"
Assert-Check -Title "showDialog has valid context parameter" -Condition $hasShowDialog

# (d) Method onLongPressButton09ac exists with ScaffoldMessenger
$hasOnLongPress = $generatedDart -match "void\s+onLongPressButton09ac\(BuildContext\s+context\)\s*\{"
Assert-Check -Title "onLongPressButton09ac method generated" -Condition $hasOnLongPress

$hasSnackBar = $generatedDart -match "ScaffoldMessenger\.of\(context\)\.showSnackBar\("
Assert-Check -Title "ScaffoldMessenger.of(context).showSnackBar has valid context" -Condition $hasSnackBar

# (e) ElevatedButton wires both onClick and onLongPress
$hasBtnClickWire = $generatedDart -match "onClickButton09ac\(context\);"
Assert-Check -Title "ElevatedButton wires onClickButton09ac in onPressed" -Condition $hasBtnClickWire

$hasBtnLongPressWire = $generatedDart -match "onLongPressButton09ac\(context\);"
Assert-Check -Title "ElevatedButton wires onLongPressButton09ac in onLongPress" -Condition $hasBtnLongPressWire

# (f) Slider wires onChanged callback with context and value
$hasSliderWire = $generatedDart -match "onChangedSliderB124\(context,\s*value\);"
Assert-Check -Title "Slider wires onChangedSliderB124 callback" -Condition $hasSliderWire

# (g) Auto-generated state variables exist
$hasTextVar = $generatedDart -match "String\?\s+_textF2c6Text;"
Assert-Check -Title "Dynamic variable _textF2c6Text declared" -Condition $hasTextVar

$hasBgVar = $generatedDart -match "Color\?\s+_row431cBgColor;"
Assert-Check -Title "Dynamic variable _row431cBgColor declared" -Condition $hasBgVar

$hasPadVar = $generatedDart -match "double\?\s+_column1Padding;"
Assert-Check -Title "Dynamic variable _column1Padding declared" -Condition $hasPadVar

# (h) Text widget binds reactive dynamic variable
$hasTextBinding = $generatedDart -match "_textF2c6Text\s*\?\?"
Assert-Check -Title "Text widget displays _textF2c6Text dynamically" -Condition $hasTextBinding

# 6. Write Generated Code to new_project_test and run Flutter Analysis
Write-Host "`n>>> 6. Writing to new_project_test and Running Flutter Analyzer..." -ForegroundColor Yellow
$flutterAppPath = "$projectRoot\flutter"
$mainDartFile = "$flutterAppPath\lib\main.dart"

Set-Content -Path $mainDartFile -Value $generatedDart -Encoding utf8
Write-Host "Updated $mainDartFile with fully aggregated Dart source." -ForegroundColor Green

# Also save the .event.dart files so SnapCode's EditorPage aggregator finds them
$pagesDir = "$projectRoot\.snapcode\pages"
Set-Content -Path "$pagesDir\main_column_1_onClick.event.dart" -Value $columnClickEvent -Encoding utf8
Set-Content -Path "$pagesDir\main_button_09ac_onClick.event.dart" -Value $buttonDialogEvent -Encoding utf8
Set-Content -Path "$pagesDir\main_button_09ac_onLongPress.event.dart" -Value $buttonSnackbarEvent -Encoding utf8
Set-Content -Path "$pagesDir\main_slider_b124_onChanged.event.dart" -Value $sliderChangedEvent -Encoding utf8

# Run flutter analyze
Write-Host "`nRunning 'flutter analyze' on $flutterAppPath..." -ForegroundColor Yellow
$analyzeOutput = & "C:\src\flutter\bin\flutter.bat" analyze "$flutterAppPath" 2>&1
$analyzeSuccess = ($LASTEXITCODE -eq 0) -and ($analyzeOutput -match "No issues found")

Assert-Check -Title "flutter analyze passes with 0 issues" -Condition $analyzeSuccess -Details ($analyzeOutput | Out-String)

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " TEST RESULTS SUMMARY: $testsPassed PASSED, $testsFailed FAILED" -ForegroundColor $(if ($testsFailed -eq 0) { "Green" } else { "Red" })
Write-Host "==========================================================" -ForegroundColor Cyan

if ($testsFailed -gt 0) {
    exit 1
} else {
    exit 0
}
