param(
    [Parameter(Mandatory=$true)][string]$EmulatorDirectory,
    [ValidateRange(1,30)][int]$Seconds = 8
)
$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$captureRoot = Join-Path $repository ('csharp/test-temp/emulator-reference/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $captureRoot | Out-Null
foreach ($folder in @('Screenshots','Saves','Cheats','Patches')) {
    New-Item -ItemType Directory -Path (Join-Path $captureRoot $folder) | Out-Null
}
Copy-Item -LiteralPath (Join-Path $EmulatorDirectory 'snes9x-x64.exe') -Destination $captureRoot
Copy-Item -LiteralPath (Join-Path $repository 'Super Metroid.smc') -Destination (Join-Path $captureRoot 'Reference.smc')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'snes9x-reference.ini') -Destination (Join-Path $captureRoot 'snes9x.conf')
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class ReferenceWindowInput {
    [DllImport("user32.dll", SetLastError=true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool PostMessage(IntPtr window, uint message, IntPtr key, IntPtr data);
    // WinUser.h: WM_KEYDOWN/WM_KEYUP. Send only to the test-owned HWND,
    // never the current foreground application.
    public static void Key(IntPtr window, int key) {
        if (!PostMessage(window, 0x100, (IntPtr)key, (IntPtr)1) ||
            !PostMessage(window, 0x101, (IntPtr)key, (IntPtr)unchecked((int)0xc0000001)))
            throw new System.ComponentModel.Win32Exception();
    }
}
'@
Write-Output "Reference fixture: $captureRoot"
Get-FileHash -LiteralPath (Join-Path $captureRoot 'snes9x-x64.exe'),(Join-Path $captureRoot 'Reference.smc')
# Visible execution is opt-in: obtain user approval before running this script.
$romArgument = '"' + (Join-Path $captureRoot 'Reference.smc') + '"'
$captureProcess = Start-Process -FilePath (Join-Path $captureRoot 'snes9x-x64.exe') -ArgumentList $romArgument -WorkingDirectory $captureRoot -WindowStyle Normal -PassThru
try {
    Start-Sleep -Seconds $Seconds
    $captureProcess.Refresh()
    if ($captureProcess.HasExited -or $captureProcess.MainWindowHandle -eq 0) { throw 'Reference emulator has no live window' }
    [ReferenceWindowInput]::Key($captureProcess.MainWindowHandle, 0x13) # VK_PAUSE
    Start-Sleep -Milliseconds 250
    [ReferenceWindowInput]::Key($captureProcess.MainWindowHandle, 0x7b) # VK_F12: screenshot
    # Snes9x fulfills screenshot requests on its next rendered frame, even paused.
    [ReferenceWindowInput]::Key($captureProcess.MainWindowHandle, 0xdc) # VK_OEM_5: frame advance
    Start-Sleep -Milliseconds 250
    [ReferenceWindowInput]::Key($captureProcess.MainWindowHandle, 0x7a) # VK_F11: isolated slot zero
    Start-Sleep -Seconds 2
    Get-ChildItem -LiteralPath (Join-Path $captureRoot 'Screenshots'),(Join-Path $captureRoot 'Saves') -File
    if (@(Get-ChildItem -LiteralPath (Join-Path $captureRoot 'Screenshots') -Filter '*.png').Count -eq 0) { throw 'Emulator did not produce a native screenshot' }
} finally {
    if (!$captureProcess.HasExited) {
        $null = $captureProcess.CloseMainWindow()
        if (!$captureProcess.WaitForExit(5000)) { throw "Test-owned emulator did not close; PID $($captureProcess.Id), fixture $captureRoot" }
    }
}
