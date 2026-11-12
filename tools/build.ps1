# The one sanctioned build entry point (tools/require-build-script.py refuses a direct `dotnet build`): builds once,
# keeps the full output in a log and prints the errors once, each on one line.
# Nothing a tool fixes deterministically is a build error: the automatically fixed rules are set to none in
# csharp/.editorconfig and the asynchronous cleanup fixes them after each push (tools/cleanup/README.md).
# Usage: pwsh -File tools/build.ps1 [project or solution] [extra dotnet build arguments...]
param([string] $Target = 'csharp/SuperMetroid.Full.slnx', [Parameter(ValueFromRemainingArguments)] [string[]] $BuildArguments = @())
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$log = Join-Path ([IO.Path]::GetTempPath()) "supermetroid-build-$PID.log"
Push-Location $repo
try {
    $started = Get-Date
    dotnet build $Target -nologo -v:q @BuildArguments *> $log
    $code = $LASTEXITCODE
    Write-Output "build exit $code after $([int]((Get-Date) - $started).TotalSeconds) s; full output $log"
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
