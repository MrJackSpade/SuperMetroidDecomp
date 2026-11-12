# Installs the asynchronous cleanup (tools/cleanup/README.md) for this repository's main checkout, or updates an
# existing installation. Safe to run again: it reuses the clone and replaces the installed scripts and configuration.
#
# - Verifies that the main checkout's core.hooksPath is its own absolute .githooks, where the trigger hook lives.
# - Clones origin into <Directory>\repo (a full clone, never a worktree of the developer's checkout), with that clone's
#   own .githooks as its hooks path and the main checkout's commit identity.
# - Writes <Directory>\config.json and copies the service, trigger and notifier into <Directory>\bin.
# - Records cleanup.directory and cleanup.branch in the main checkout's git configuration, which turns the hook on.
# Usage: pwsh -File tools/cleanup/install.ps1 [-Directory <path>] [-Branch <branch>]
param(
    [string]$Directory = 'G:\git\SuperMetroidDecomp-cleanup',
    [string]$Branch = 'main'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'common.ps1')

function Invoke-Git([string]$Repository, [string[]]$Arguments) {
    $output = & git -C $Repository @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) { throw "git -C $Repository $($Arguments -join ' ') failed (exit $LASTEXITCODE): $($output -join "`n")" }
    return (($output | ForEach-Object { "$_" }) -join "`n").Trim()
}

function ConvertTo-GitPath([string]$Path) { return [IO.Path]::GetFullPath($Path).Replace('\', '/').TrimEnd('/') }

try {
    Disable-ErrorDialogs
    $checkout = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
    $mainCheckout = Split-Path (Invoke-Git $checkout @('rev-parse', '--path-format=absolute', '--git-common-dir')) -Parent
    $paths = Get-CleanupPaths $Directory
    if ($paths.Root.StartsWith([IO.Path]::GetFullPath($mainCheckout).TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "The cleanup directory $($paths.Root) is inside the main checkout; it must be its own place."
    }

    $hooksPath = Invoke-Git $mainCheckout @('config', '--get', 'core.hooksPath')
    $expectedHooks = ConvertTo-GitPath (Join-Path $mainCheckout '.githooks')
    if (-not [string]::Equals((ConvertTo-GitPath $hooksPath), $expectedHooks, [StringComparison]::OrdinalIgnoreCase)) {
        throw "The main checkout's core.hooksPath is '$hooksPath', not '$expectedHooks'; set it as tools/discord-updates.md describes."
    }
    $triggerHook = Join-Path $mainCheckout '.githooks/reference-transaction'

    $origin = Invoke-Git $mainCheckout @('remote', 'get-url', 'origin')
    foreach ($folder in $paths.Root, $paths.Logs, $paths.Temp, $paths.Bin) { $null = New-Item -ItemType Directory -Force -Path $folder }
    if (Test-Path -LiteralPath $paths.Repository) {
        $existing = Invoke-Git $paths.Repository @('remote', 'get-url', 'origin')
        if ($existing -ne $origin) { throw "$($paths.Repository) is a clone of $existing, not of $origin." }
        Write-Output "clone: reusing $($paths.Repository)"
    }
    else {
        Write-Output "clone: cloning $origin into $($paths.Repository)"
        $output = & git clone --quiet $origin $paths.Repository 2>&1
        if ($LASTEXITCODE -ne 0) { throw "git clone failed (exit $LASTEXITCODE): $($output -join "`n")" }
    }
    $null = Invoke-Git $paths.Repository @('fetch', '--quiet', '--no-tags', 'origin', "+refs/heads/$($Branch):refs/remotes/origin/$Branch")
    $null = Invoke-Git $paths.Repository @('config', 'core.hooksPath', (ConvertTo-GitPath (Join-Path $paths.Repository '.githooks')))
    foreach ($identity in 'user.name', 'user.email') {
        $null = Invoke-Git $paths.Repository @('config', $identity, (Invoke-Git $mainCheckout @('config', '--get', $identity)))
    }

    $config = [ordered]@{ version = 1; remote = 'origin'; branch = $Branch; origin = $origin; mainCheckout = $mainCheckout }
    Write-AtomicText $paths.Config ($config | ConvertTo-Json)
    foreach ($file in $InstalledScripts) { Copy-Item -LiteralPath (Join-Path $PSScriptRoot $file) -Destination (Join-Path $paths.Bin $file) -Force }

    $null = Invoke-Git $mainCheckout @('config', 'cleanup.directory', (ConvertTo-GitPath $paths.Root))
    $null = Invoke-Git $mainCheckout @('config', 'cleanup.branch', $Branch)

    Write-Output "installed: $($paths.Root) cleans origin/$Branch"
    Write-Output "hooks: $expectedHooks"
    Write-Output ("trigger hook: " + $(if (Test-Path -LiteralPath $triggerHook) { "$triggerHook is present" }
        else { "$triggerHook is not in the main checkout yet; the cleanup starts on pushes once it is" }))
    Write-Output "status: pwsh -File tools/cleanup/status.ps1"
    exit 0
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 1
}
