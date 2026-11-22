# The application host a .NET project builds, as MSBuild evaluates it: its output directory under its own target
# framework (#1690). Dot-source this file; run-verification.ps1 and render-movie-videos.ps1 share it.

# Evaluates the project as the build just did and returns the full path of the application host it builds.
function Resolve-ProjectExecutable([string] $ProjectFile) {
    $evaluation = & dotnet msbuild $ProjectFile -nologo `
        -getProperty:TargetFramework -getProperty:TargetDir -getProperty:AssemblyName `
        -getProperty:UseAppHost -getProperty:_NativeExecutableExtension
    if ($LASTEXITCODE -ne 0) {
        throw "Evaluating $ProjectFile failed (exit $LASTEXITCODE):`n$($evaluation -join "`n")"
    }
    $properties = ($evaluation -join "`n" | ConvertFrom-Json).Properties
    if ([string]::IsNullOrEmpty($properties.TargetFramework)) {
        throw "$ProjectFile declares no single TargetFramework, so it has no one executable to run."
    }
    if ($properties.UseAppHost -ne 'true') {
        throw "$ProjectFile ($($properties.TargetFramework)) builds no application host (UseAppHost '$($properties.UseAppHost)')."
    }
    return Join-Path $properties.TargetDir "$($properties.AssemblyName)$($properties._NativeExecutableExtension)"
}
