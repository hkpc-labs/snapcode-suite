Add-Type @"
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class Win32 {
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

    public static List<IntPtr> GetWindowsForProcess(uint pid) {
        List<IntPtr> hwnds = new List<IntPtr>();
        EnumWindows((hWnd, lParam) => {
            uint windowPid;
            GetWindowThreadProcessId(hWnd, out windowPid);
            if (windowPid == pid && IsWindowVisible(hWnd)) {
                hwnds.Add(hWnd);
            }
            return true;
        }, IntPtr.Zero);
        return hwnds;
    }
}
"@

$exe = Resolve-Path ".\src\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\Snapcode.exe"
$project = Resolve-Path "$HOME\Documents\Snapcode\repos\newapp"
$p = Start-Process -FilePath $exe -ArgumentList "--project `"$project`"" -PassThru

try {
    Write-Host "Launched Snapcode PID: $($p.Id). Waiting 5 seconds..."
    Start-Sleep -Seconds 5

    $hwnds = [Win32]::GetWindowsForProcess($p.Id)
    Write-Host "Found $($hwnds.Count) visible window(s) for PID $($p.Id):"

    Add-Type -AssemblyName System.Drawing
    Add-Type -AssemblyName System.Windows.Forms

    foreach ($hwnd in $hwnds) {
        $sb = New-Object System.Text.StringBuilder 256
        [Win32]::GetWindowText($hwnd, $sb, 256) | Out-Null
        $title = $sb.ToString()
        
        $rect = New-Object Win32+RECT
        [Win32]::GetWindowRect($hwnd, [ref]$rect) | Out-Null
        $w = $rect.Right - $rect.Left
        $h = $rect.Bottom - $rect.Top

        Write-Host "  HWND: $hwnd | Title: '$title' | Size: ${w}x${h} at ($($rect.Left), $($rect.Top))"

        if ($w -gt 200 -and $h -gt 200) {
            $bmp = New-Object System.Drawing.Bitmap $w, $h
            $g = [System.Drawing.Graphics]::FromImage($bmp)
            $g.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size($w, $h)))
            $screenshotPath = ".\helpers\live_snapcode_window.png"
            $bmp.Save($screenshotPath, [System.Drawing.Imaging.ImageFormat]::Png)
            $g.Dispose()
            $bmp.Dispose()
            Write-Host ">>> Captured live window screenshot to: $screenshotPath" -ForegroundColor Green
        }
    }
} finally {
    Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
}
