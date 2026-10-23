Add-Type @"
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class Win32Capture {
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public static List<IntPtr> GetAllVisibleWindows() {
        List<IntPtr> hwnds = new List<IntPtr>();
        EnumWindows((hWnd, lParam) => {
            if (IsWindowVisible(hWnd)) {
                hwnds.Add(hWnd);
            }
            return true;
        }, IntPtr.Zero);
        return hwnds;
    }
}
"@

$snapcodeProcs = Get-Process -Name "Snapcode*" -ErrorAction SilentlyContinue
Write-Host "Running Snapcode processes:"
$snapcodeProcs | ForEach-Object { Write-Host "  PID: $($_.Id) | Memory: $([math]::Round($_.WorkingSet64/1MB, 2)) MB" }

$hwnds = [Win32Capture]::GetAllVisibleWindows()
Write-Host "Total visible desktop windows: $($hwnds.Count)"

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

$found = $false
foreach ($hwnd in $hwnds) {
    uint $wPid = 0
    [Win32Capture]::GetWindowThreadProcessId($hwnd, [ref]$wPid) | Out-Null
    
    $sb = New-Object System.Text.StringBuilder 256
    [Win32Capture]::GetWindowText($hwnd, $sb, 256) | Out-Null
    $title = $sb.ToString()

    if ($snapcodeProcs.Id -contains $wPid -or $title -like "*Snapcode*" -or $title -like "*Setup*" -or $title -like "*newapp*") {
        $rect = New-Object Win32Capture+RECT
        [Win32Capture]::GetWindowRect($hwnd, [ref]$rect) | Out-Null
        $w = $rect.Right - $rect.Left
        $h = $rect.Bottom - $rect.Top

        Write-Host ">>> Found Match: HWND $hwnd | Title: '$title' | PID: $wPid | Size: ${w}x${h}" -ForegroundColor Green

        if ($w -gt 100 -and $h -gt 100) {
            $bmp = New-Object System.Drawing.Bitmap $w, $h
            $g = [System.Drawing.Graphics]::FromImage($bmp)
            $g.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size($w, $h)))
            $savePath = ".\helpers\live_snapcode_app.png"
            $bmp.Save($savePath, [System.Drawing.Imaging.ImageFormat]::Png)
            $g.Dispose()
            $bmp.Dispose()
            Write-Host ">>> Screenshot saved to $savePath" -ForegroundColor Cyan
            $found = $true
        }
    }
}

if (-not $found) {
    Write-Host "No matching window captured yet." -ForegroundColor Yellow
}
