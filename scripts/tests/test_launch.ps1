$exe = Resolve-Path ".\src\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\Snapcode.exe"
$p = Start-Process -FilePath $exe -ArgumentList '--project "C:\Users\hardi\Documents\Snapcode\repos\newapp"' -PassThru
Start-Sleep -Seconds 3
Write-Host "Process ID: $($p.Id)"
Write-Host "HasExited:   $($p.HasExited)"
if ($p.HasExited) {
    Write-Host "ExitCode:    $($p.ExitCode)"
} else {
    Write-Host "MainWindowTitle: $($p.MainWindowTitle)"
    Stop-Process -Id $p.Id -Force
}
