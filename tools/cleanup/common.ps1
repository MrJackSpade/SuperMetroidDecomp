# Shared helpers of the asynchronous cleanup (tools/cleanup/README.md). Dot-sourced by every cleanup script; it only
# defines functions. It stays parseable by Windows PowerShell 5.1, which runs notify.ps1.

# SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX | SEM_NOOPENFILEERRORBOX, as the C# entry points install it.
$script:NoDialogsErrorMode = [uint32]0x8003

# The scripts the install copies into <cleanup directory>\bin; the service refuses to run when they differ from
# tools/cleanup in the tree it cleans.
$script:InstalledScripts = @('common.ps1', 'notify.ps1', 'trigger.ps1', 'service.ps1')

function Disable-ErrorDialogs {
    # Installs the Windows no-dialog process policy; every process this one starts inherits it. Emitted directly
    # rather than compiled with Add-Type, so the git hook that starts the cleanup stays fast.
    $assembly = [System.Reflection.Emit.AssemblyBuilder]::DefineDynamicAssembly(
        [System.Reflection.AssemblyName]::new('CleanupErrorMode'), [System.Reflection.Emit.AssemblyBuilderAccess]::Run)
    $type = $assembly.DefineDynamicModule('CleanupErrorMode').DefineType('ErrorMode', [System.Reflection.TypeAttributes]'Public, Abstract, Sealed')
    $method = $type.DefinePInvokeMethod('SetErrorMode', 'kernel32.dll', [System.Reflection.MethodAttributes]'Public, Static, PinvokeImpl',
        [System.Reflection.CallingConventions]::Standard, [uint32], [type[]]@([uint32]),
        [System.Runtime.InteropServices.CallingConvention]::Winapi, [System.Runtime.InteropServices.CharSet]::Auto)
    $method.SetImplementationFlags([System.Reflection.MethodImplAttributes]::PreserveSig)
    $created = $type.CreateType()
    $null = $created::SetErrorMode($script:NoDialogsErrorMode)
}

function Get-CleanupPaths([string]$Directory) {
    # Every file and folder the cleanup keeps in its directory.
    $root = [IO.Path]::GetFullPath($Directory)
    return @{
        Root = $root
        Config = Join-Path $root 'config.json'
        Status = Join-Path $root 'STATUS'
        State = Join-Path $root 'state.json'
        Requests = Join-Path $root 'requests.log'
        Lock = Join-Path $root 'service.lock'
        ServiceLog = Join-Path $root 'service.log'
        TriggerLog = Join-Path $root 'trigger.log'
        Logs = Join-Path $root 'logs'
        Temp = Join-Path $root 'temp'
        Bin = Join-Path $root 'bin'
        Repository = Join-Path $root 'repo'
    }
}

function Write-AtomicText([string]$Path, [string]$Text) {
    # Replaces a file in one step, so a reader never sees half of it.
    $temporary = "$Path.$([guid]::NewGuid().ToString('N')).tmp"
    [IO.File]::WriteAllText($temporary, $Text, [Text.UTF8Encoding]::new($false))
    [IO.File]::Move($temporary, $Path, $true)
}

function Write-LogLine([string]$Path, [string]$Text) {
    # Appends one timestamped line.
    [IO.File]::AppendAllText($Path, "$([DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ')) $Text`r`n", [Text.UTF8Encoding]::new($false))
}

function Read-CleanupConfig($Paths) {
    # The install's configuration: the clone, the remote and the branch it cleans.
    if (-not (Test-Path -LiteralPath $Paths.Config)) { throw "$($Paths.Config) is missing; run tools/cleanup/install.ps1." }
    $config = Get-Content -LiteralPath $Paths.Config -Raw | ConvertFrom-Json
    if ($config.version -ne 1) { throw "$($Paths.Config) has unsupported version $($config.version)." }
    foreach ($name in 'remote', 'branch', 'mainCheckout') {
        if (-not $config.$name) { throw "$($Paths.Config) has no $name." }
    }
    return $config
}

function Write-CleanupStatus($Paths, [string]$State, [string]$Result, [string]$Message, [string]$Tip, [string]$Log, [string]$Commit) {
    # The STATUS file the developer's tools print: state is idle, running, failed or stopped.
    $status = [ordered]@{
        state = $State
        result = $Result
        message = $Message
        tip = $Tip
        commit = $Commit
        log = $Log
        updated = [DateTime]::UtcNow.ToString('o')
    }
    Write-AtomicText $Paths.Status ($status | ConvertTo-Json)
}

function Send-CleanupNotification($Paths, [string]$Title, [string]$Text) {
    # Raises a Windows toast through Windows PowerShell, which can reach the toast API; a toast never takes focus.
    $notifier = Join-Path $Paths.Bin 'notify.ps1'
    $output = & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $notifier -Title $Title -Text $Text 2>&1
    if ($LASTEXITCODE -ne 0) { throw "The notification failed (exit $LASTEXITCODE): $($output -join "`n")" }
}

function Test-SharingViolation([System.Exception]$Exception) {
    # Whether an open failed only because another process holds the file exclusively.
    return $Exception -is [System.IO.IOException] -and ($Exception.HResult -band 0xFFFF) -eq 32
}

function Open-ServiceLock($Paths) {
    # Opens the service lock exclusively, or returns null when a running service holds it. Windows releases it when
    # the holder exits, however it exits, so a lock is never left stale.
    try {
        return [IO.File]::Open($Paths.Lock, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    }
    catch [System.Management.Automation.MethodInvocationException] {
        if (Test-SharingViolation $_.Exception.InnerException) { return $null }
        throw
    }
}

function Get-RequestLength($Paths) {
    # How much has been appended to the request log: each trigger appends one line.
    if (-not (Test-Path -LiteralPath $Paths.Requests)) { return [long]0 }
    return (Get-Item -LiteralPath $Paths.Requests).Length
}

function Get-ScriptHash([string]$Path) {
    # The SHA-256 of a script's text with line endings normalized, since checkouts may hold CRLF or LF.
    $text = [IO.File]::ReadAllText($Path).Replace("`r`n", "`n")
    $bytes = [Text.UTF8Encoding]::new($false).GetBytes($text)
    return [BitConverter]::ToString([Security.Cryptography.SHA256]::Create().ComputeHash($bytes))
}
