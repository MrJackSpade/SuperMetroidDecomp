param(
    [Parameter(Mandatory)][string] $InstallationRoot,
    [Parameter(Mandatory)][string] $ApkPath,
    [Parameter(Mandatory)][string] $AaptPath,
    [Parameter(Mandatory)][string] $ValidatorPath,
    [Parameter(Mandatory)][string] $DeviceSerial,
    [ValidateRange(10, 180)][int] $StartupTimeoutSeconds = 120
)

# This acceptance tool never updates the player's testing APK or copies saves,
# settings, debugger graphs or a cartridge. The fixed package is disposable.
$ErrorActionPreference = 'Stop'
$gatePackage = 'org.supermetroid.csharp.romfree549'
$gateOwner = $gateStage = $gateRemote = ''
$gateInstalled = $false
$gatePushed = $false
$gateExitCode = 0

function Invoke-GateAdb {
    param([string[]] $AdbArguments)
    $result = & adb -s $DeviceSerial @AdbArguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "adb $($AdbArguments -join ' ') failed with exit ${LASTEXITCODE}:`n$($result -join "`n")"
    }
    return ($result -join "`n").Trim()
}

function Assert-GatePrivateTree {
    $tree = Invoke-GateAdb @('shell', 'run-as', $gatePackage, 'find', 'files', '-type', 'f')
    if ($tree -match '(?im)\.(smc|sfc|srm)$' -or $tree -match '(?im)\.smstate$') {
        throw 'The isolated package unexpectedly contains a ROM, emulator SRAM or debugger state.'
    }
    if ($tree -match '(?m)^files/last-error\.txt$') {
        $failure = Invoke-GateAdb @('shell', 'run-as', $gatePackage, 'cat', 'files/last-error.txt')
        throw "The real Android host stopped:`n$failure"
    }
}

try {
    $gateId = [Guid]::NewGuid().ToString('N')
    $gateWorkspace = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
    $gateOwner = [IO.Path]::GetFullPath((Join-Path $gateWorkspace 'csharp/test-temp'))
    $gateStage = Join-Path $gateOwner ('android-rom-free-549-' + $gateId)
    $gateRemote = '/data/local/tmp/supermetroid-romfree549-' + $gateId
    $gateSource = [IO.Path]::GetFullPath((Join-Path $InstallationRoot 'game'))
    $gateApk = [IO.Path]::GetFullPath($ApkPath)
    if (!(Test-Path -LiteralPath (Join-Path $gateSource 'installation.json')) -or
        !(Test-Path -LiteralPath $gateApk -PathType Leaf) -or
        !(Test-Path -LiteralPath $AaptPath -PathType Leaf) -or
        !(Test-Path -LiteralPath $ValidatorPath -PathType Leaf)) {
        throw 'Supply an extracted installation, diagnostic APK, SDK aapt executable and current IntegrationVerification DLL.'
    }
    & dotnet $ValidatorPath --validate-extracted-installation $InstallationRoot
    if ($LASTEXITCODE -ne 0) { throw 'Source bundle failed the strict current required-resource preflight; device was not modified.' }
    $gateBadging = & $AaptPath dump badging $gateApk 2>&1
    if ($LASTEXITCODE -ne 0 -or ($gateBadging -join "`n") -notmatch "(?m)^package: name='$gatePackage' ") {
        throw "Refusing to install an APK whose manifest is not the isolated package $gatePackage."
    }
    $gateExisting = Invoke-GateAdb @('shell', 'pm', 'list', 'packages', $gatePackage)
    if ($gateExisting -match [regex]::Escape($gatePackage)) {
        throw "The diagnostic package already exists; refusing to overwrite or delete unknown app data."
    }
    [IO.Directory]::CreateDirectory((Join-Path $gateStage 'game')) | Out-Null
    $gateCount = 0
    foreach ($gateFile in Get-ChildItem -LiteralPath $gateSource -Recurse -File) {
        $gateRelative = [IO.Path]::GetRelativePath($gateSource, $gateFile.FullName)
        if ($gateRelative -eq 'SuperMetroid.smc') { continue }
        if ($gateFile.Extension -in @('.smc', '.sfc', '.srm', '.smstate')) {
            throw "Unexpected cartridge/player file in extracted content: $gateRelative"
        }
        $gateDestination = [IO.Path]::GetFullPath((Join-Path (Join-Path $gateStage 'game') $gateRelative))
        if (!$gateDestination.StartsWith($gateStage + [IO.Path]::DirectorySeparatorChar,
            [StringComparison]::OrdinalIgnoreCase)) { throw 'Staged resource escaped the fixture directory.' }
        [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($gateDestination)) | Out-Null
        Copy-Item -LiteralPath $gateFile.FullName -Destination $gateDestination
        $gateCount++
    }
    & dotnet $ValidatorPath --validate-extracted-installation $gateStage
    if ($LASTEXITCODE -ne 0) { throw 'The staged ROM-free copy failed required-resource validation; device was not modified.' }
    $gateInstallResult = Invoke-GateAdb @('install', $gateApk)
    if ($gateInstallResult -notmatch '\bSuccess\b') { throw "Installation was not confirmed: $gateInstallResult" }
    $gateInstalled = $true
    $gatePrivateRoot = Invoke-GateAdb @('shell', 'run-as', $gatePackage, 'pwd')
    if ($gatePrivateRoot -notmatch ('^/data/(user/0|data)/' + [regex]::Escape($gatePackage) + '$')) {
        throw "Unexpected application-private path: $gatePrivateRoot"
    }
    # The generated remote target remains ours even if a transfer fails halfway.
    $gatePushed = $true
    Invoke-GateAdb @('push', (Join-Path $gateStage 'game'), $gateRemote) | Write-Host
    Invoke-GateAdb @('shell', 'run-as', $gatePackage, 'mkdir', '-p', 'files') | Out-Null
    Invoke-GateAdb @('shell', 'run-as', $gatePackage, 'cp', '-r', $gateRemote, 'files/game') | Out-Null
    Assert-GatePrivateTree
    $gateActivity = (Invoke-GateAdb @('shell', 'cmd', 'package', 'resolve-activity', '--brief', $gatePackage)).Split("`n")[-1].Trim()
    if (!$gateActivity.StartsWith($gatePackage + '/', [StringComparison]::Ordinal)) {
        throw "Could not resolve the isolated launcher activity: $gateActivity"
    }
    for ($gateRun = 1; $gateRun -le 2; $gateRun++) {
        Invoke-GateAdb @('shell', 'am', 'force-stop', $gatePackage) | Out-Null
        # Remove only the prior timing output from this newly-created disposable app.
        Invoke-GateAdb @('shell', 'run-as', $gatePackage, 'rm', '-f', 'files/timing.log') | Out-Null
        # A COLD activity launch can succeed behind a sleeping/keyguard screen.
        # The production focus gate correctly refuses to emulate in that case.
        # Wake (never toggle) and dismiss only a nonsecure keyguard; no settings
        # or lock credentials are changed by this acceptance tool.
        Invoke-GateAdb @('shell', 'input', 'keyevent', 'KEYCODE_WAKEUP') | Out-Null
        Invoke-GateAdb @('shell', 'wm', 'dismiss-keyguard') | Out-Null
        Invoke-GateAdb @('shell', 'am', 'start', '-W', '-n', $gateActivity) | Write-Host
        $gateDeadline = [DateTime]::UtcNow.AddSeconds($StartupTimeoutSeconds)
        $gateTiming = ''
        $gateReached = $false
        do {
            Start-Sleep -Milliseconds 500
            Assert-GatePrivateTree
            $gateNames = Invoke-GateAdb @('shell', 'run-as', $gatePackage, 'ls', 'files')
            if ($gateNames.Split("`n") -contains 'timing.log') {
                $gateTiming = Invoke-GateAdb @('shell', 'run-as', $gatePackage, 'cat', 'files/timing.log')
                $gateLast = $gateTiming.Split("`n")[-1]
                if ($gateLast -match 'frame=(\d+)' -and [int]$Matches[1] -ge 600 -and
                    $gateLast -match '(?:^|\s)phase=TitleScreen(?:\s|$)' -and $gateLast -match 'paint ([1-9]\d*(\.\d+)?)') {
                    $gateReached = $true
                    break
                }
            }
        } while ([DateTime]::UtcNow -lt $gateDeadline)
        if (!$gateReached) {
            $gatePower = Invoke-GateAdb @('shell', 'dumpsys', 'power')
            $gateWakefulness = ($gatePower.Split("`n") | Where-Object { $_ -match 'mWakefulness=' }) -join "`n"
            throw "Cold start $gateRun did not reach a rendered title screen before the deadline.`n$gateWakefulness`n$gateTiming"
        }
        Assert-GatePrivateTree
        Write-Host "PASS cold start ${gateRun}: $gateLast"
    }
    Write-Host "PASS on-device extracted-only cold boot: $gateCount files, two fresh processes, no ROM, SRAM or debugger-state payload."
    Write-Host "APK SHA256: $((Get-FileHash -LiteralPath $gateApk -Algorithm SHA256).Hash)"
}
catch {
    [Console]::Error.WriteLine($_.Exception.ToString())
    $gateExitCode = 1
}
finally {
    # Targets are generated here and explicitly checked before any recursive removal.
    if ($gateInstalled) {
        try { Invoke-GateAdb @('uninstall', $gatePackage) | Write-Host }
        catch { [Console]::Error.WriteLine($_.Exception.ToString()); $gateExitCode = 1 }
    }
    if ($gatePushed -and $gateRemote -match '^/data/local/tmp/supermetroid-romfree549-[a-f0-9]{32}$') {
        try { Invoke-GateAdb @('shell', 'rm', '-r', $gateRemote) | Out-Null }
        catch { [Console]::Error.WriteLine($_.Exception.ToString()); $gateExitCode = 1 }
    }
    if ($gateStage.StartsWith($gateOwner + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase) -and
        [IO.Path]::GetFileName($gateStage) -match '^android-rom-free-549-[a-f0-9]{32}$' -and
        (Test-Path -LiteralPath $gateStage)) {
        try { Remove-Item -LiteralPath $gateStage -Recurse -Force }
        catch { [Console]::Error.WriteLine($_.Exception.ToString()); $gateExitCode = 1 }
    }
}
exit $gateExitCode
