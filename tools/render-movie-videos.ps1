# Renders the supplied movies' replays as captioned MP4s for visual review: every frame as the desktop host presents it,
# with a band below the picture naming the video frame, the converted update, the original movie frame and the area and
# room (or the screen outside gameplay). Each movie renders in one pass from its first frame to its last, every movie in
# a process of its own, all side by side at Idle priority. The replay compares every frame with the native recording as
# it goes, so a video only exists for a replay that matched. Everything runs on the CPU (the WARP software rasterizer and
# libx264).
#
# Usage: pwsh -File tools/render-movie-videos.ps1 [-OutputDirectory test-temp/movie-videos] [-Movies Lowc,Full100]
param(
    [string] $OutputDirectory = 'test-temp/movie-videos',
    [string[]] $Movies = @('Drunk', 'Full100', 'Lowc', 'LowPercent13', 'Ridley', 'MotherBrainGlitch'))
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'project-executable.ps1')

# pwsh -File hands a list over as one comma-joined string, so the names are split here and checked against the movies.
$known = @('MotherBrainGlitch', 'LowPercent13', 'Full100', 'Drunk', 'Lowc', 'Ridley')
$Movies = @($Movies | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$unknown = @($Movies | Where-Object { $known -cnotcontains $_ })
if ($unknown.Count -gt 0) { throw "Unknown movies: $($unknown -join ', '). The movies are $($known -join ', ')." }

$watch = [Diagnostics.Stopwatch]::StartNew()
$output = [IO.Path]::GetFullPath((Join-Path $repo $OutputDirectory))
New-Item -ItemType Directory -Force $output | Out-Null

# Build once and run a private copy, so a later build cannot replace the binary under the running renders.
$project = Join-Path $repo 'tools/movie-video/SuperMetroid.MovieVideo.csproj'
& pwsh -File (Join-Path $PSScriptRoot 'build.ps1') $project
if ($LASTEXITCODE -ne 0) { throw "The build of SuperMetroid.MovieVideo failed (exit $LASTEXITCODE); nothing was rendered." }
$built = Get-Item -LiteralPath (Resolve-ProjectExecutable $project)
$bin = Join-Path $repo 'test-temp/bin-movie-video'
if (Test-Path $bin) { Remove-Item -Recurse -Force $bin }
Copy-Item -Recurse $built.DirectoryName $bin
$executable = Join-Path $bin $built.Name
Write-Output "Rendering $($Movies -join ', ') with $executable (built $($built.LastWriteTime)) into $output."

$runs = [ordered]@{}
foreach ($movie in $Movies) {
    $log = Join-Path $output "$movie.log"
    $process = Start-Process -FilePath $executable -ArgumentList $movie, "`"$(Join-Path $output "$movie.mp4")`"" `
        -WorkingDirectory $repo -NoNewWindow -PassThru -RedirectStandardOutput $log -RedirectStandardError "$log.err"
    $null = $process.Handle
    $process.PriorityClass = 'Idle'
    $runs[$movie] = $process
}

$failed = @()
foreach ($movie in $runs.Keys) {
    $runs[$movie].WaitForExit()
    if ($runs[$movie].ExitCode -eq 0) {
        Write-Output ("[{0:hh\:mm\:ss}] {1}: {2}" -f $watch.Elapsed, $movie, (Get-Content (Join-Path $output "$movie.log") -Tail 1))
    }
    else {
        $failed += $movie
        Write-Output ("[{0:hh\:mm\:ss}] {1} failed (exit {2}); see {3}.err" -f $watch.Elapsed, $movie, $runs[$movie].ExitCode, (Join-Path $output "$movie.log"))
    }
}
if ($failed.Count -gt 0) {
    $details = $failed | ForEach-Object { "${_}:`n" + (Get-Content (Join-Path $output "$_.log.err") -Raw) }
    throw "These renders failed:`n$($details -join "`n")"
}
Write-Output ("Done in {0:hh\:mm\:ss}." -f $watch.Elapsed)
