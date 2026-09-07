param()
$ErrorActionPreference = 'Stop'

# Isolated MSBuild intermediates are essential: publishing from the usual obj tree
# would not demonstrate that shaders are rebuilt from their checked-in sources.
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$artifacts = Join-Path $repository ('csharp/test-temp/render-publish/' + [Guid]::NewGuid().ToString('N'))
$published = Join-Path $artifacts 'published'
New-Item -ItemType Directory -Path $artifacts | Out-Null
$project = Join-Path $repository 'csharp/src/SuperMetroid.RenderVerification/SuperMetroid.RenderVerification.csproj'
dotnet publish $project -c Release --artifacts-path $artifacts --output $published -p:RestoreLockedMode=true
if ($LASTEXITCODE -ne 0) { throw "Isolated renderer publish failed; evidence retained at $artifacts" }

$notice = Join-Path $published 'D3D11_THIRD_PARTY_NOTICES.txt'
if (-not (Test-Path -LiteralPath $notice)) { throw 'Published renderer is missing dependency notices.' }
$entry = Join-Path $published 'SuperMetroid.RenderVerification.dll'

# Run with the published directory as the working directory, with no ROM, loose
# shaders or source tree beside the application. Tests use constructed fixtures.
Push-Location $published
try {
    foreach ($test in @('--solid-smoke', '--ordinary-smoke', '--display-smoke')) {
        dotnet $entry $test
        if ($LASTEXITCODE -ne 0) { throw "Published renderer failed $test; evidence retained at $artifacts" }
    }
} finally { Pop-Location }

Write-Output "Isolated shader build/publish and hardware/WARP execution passed: $published"
Write-Output 'Outputs retained for inspection; no existing build outputs or player files were changed.'
