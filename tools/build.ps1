# Builds, removing the unused usings each build reports, until a build reports none it could remove (#1530).
# Unused usings (IDE0005) are build errors here, and the command-line build reports only the first in each file; this
# deletes the reported lines from the build's own diagnostics, with no extra project load, and builds again while a pass
# still removed something. Whatever errors remain need a decision and are printed once.
# Usage: pwsh -File tools/build.ps1 [project or solution] [extra dotnet build arguments...]
param([string] $Target = 'csharp/SuperMetroid.Full.slnx', [Parameter(ValueFromRemainingArguments)] [string[]] $BuildArguments = @())
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$log = Join-Path ([IO.Path]::GetTempPath()) "supermetroid-build-$PID.log"
Push-Location $repo
try {
    $pass = 0
    while ($true) {
        $pass++
        $started = Get-Date
        dotnet build $Target -nologo -v:q @BuildArguments *> $log
        $code = $LASTEXITCODE
        Write-Output "build pass $pass exit $code after $([int]((Get-Date) - $started).TotalSeconds) s"
        if ($code -eq 0) { break }
        $removal = python (Join-Path $PSScriptRoot 'remove-unused-usings.py') $log
        if ($LASTEXITCODE -ne 0) { throw "Removing unused usings failed: $removal" }
        Write-Output $removal
        if ($removal -match '^removed 0 ') { break }
    }
    if ($code -ne 0) {
        Select-String -Path $log -Pattern ': error ' |
            ForEach-Object { $_.Line -replace ' \[.*', '' -replace '.*SuperMetroidDecomp\\csharp\\', '' -replace '\(https[^)]*\)', '' } |
            Sort-Object -Unique
    }
    exit $code
}
finally {
    Pop-Location
}
