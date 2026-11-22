# Renders the supplied movies' replays as captioned MP4s for visual review: every frame as the desktop host presents it,
# with a band below the picture naming the video frame, the converted update, the original movie frame and the area and
# room (or the screen outside gameplay). The verifier compares every frame with the native recording while it records,
# so a video only exists for a replay that matched.
#
# A verifier run may take at most 30 minutes, so each movie is recorded in segments of -SegmentFrames frames, each in a
# run of its own; -Parallel runs go at once, at Idle priority. Each segment replays the movie from its start (without
# rasterizing) up to its first frame, so the game reaches it exactly as the whole replay does. A movie's segments are then
# joined: the pictures without re-encoding, the sound as one continuous stream encoded once, so nothing drifts at the
# joins. Everything runs on the CPU (the WARP software rasterizer and libx264).
#
# Usage: pwsh -File tools/render-movie-videos.ps1 [-OutputDirectory test-temp/movie-videos] [-Movies Lowc,Full100]
#        [-SegmentFrames 60000] [-Parallel 8]
param(
    [string] $OutputDirectory = 'test-temp/movie-videos',
    [ValidateSet('MotherBrainGlitch', 'LowPercent13', 'Full100', 'Drunk', 'Lowc', 'Ridley')]
    [string[]] $Movies = @('Ridley', 'MotherBrainGlitch', 'LowPercent13', 'Lowc', 'Full100', 'Drunk'),
    [ValidateRange(600, 1000000)][int] $SegmentFrames = 60000,
    [ValidateRange(1, 64)][int] $Parallel = 8)
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'project-executable.ps1')

# Starts one verifier run of $Arguments at Idle priority with its output in $Log.out and $Log.err.
function Start-Run([string] $Executable, [string] $Arguments, [string] $Log) {
    $process = Start-Process -FilePath $Executable -ArgumentList $Arguments -WorkingDirectory $repo -NoNewWindow -PassThru `
        -RedirectStandardOutput "$Log.out" -RedirectStandardError "$Log.err"
    $null = $process.Handle
    $process.PriorityClass = 'Idle'
    return $process
}

# Waits for every run in $Runs (name -> process), and throws naming each that failed with its error log.
function Wait-Runs([hashtable] $Runs, [string] $LogDirectory) {
    $failed = @()
    foreach ($name in $Runs.Keys) {
        $Runs[$name].WaitForExit()
        if ($Runs[$name].ExitCode -ne 0) { $failed += $name }
    }
    if ($failed.Count -gt 0) {
        $details = $failed | ForEach-Object { "$_ (exit $($Runs[$_].ExitCode)):`n" + (Get-Content (Join-Path $LogDirectory "$_.err") -Raw) }
        throw "These runs failed:`n$($details -join "`n")"
    }
}

# Runs ffmpeg with $Arguments and throws on a nonzero exit.
function Invoke-Ffmpeg([string[]] $Arguments) {
    & ffmpeg -hide_banner -loglevel error -y @Arguments
    if ($LASTEXITCODE -ne 0) { throw "ffmpeg $($Arguments -join ' ') failed with exit code $LASTEXITCODE." }
}

$watch = [Diagnostics.Stopwatch]::StartNew()
$output = [IO.Path]::GetFullPath((Join-Path $repo $OutputDirectory))
$work = Join-Path $output 'segments'
New-Item -ItemType Directory -Force $work | Out-Null

# Build once and run a private copy, so a later build cannot replace the binary under running segments.
$project = Join-Path $repo 'csharp/src/SuperMetroid.Verification/SuperMetroid.Verification.csproj'
& pwsh -File (Join-Path $PSScriptRoot 'build.ps1') $project
if ($LASTEXITCODE -ne 0) { throw "The build of SuperMetroid.Verification failed (exit $LASTEXITCODE); nothing was rendered." }
$built = Get-Item -LiteralPath (Resolve-ProjectExecutable $project)
$bin = Join-Path $repo 'test-temp/bin-movie-video'
if (Test-Path $bin) { Remove-Item -Recurse -Force $bin }
Copy-Item -Recurse $built.DirectoryName $bin
$executable = Join-Path $bin $built.Name
Write-Output "Rendering $($Movies -join ', ') with $executable (built $($built.LastWriteTime))."

# Each movie's length decides its segments. The first run installs the shared verification content alone, so the
# other runs reuse it instead of installing it at the same time.
$counts = @{}
$countRuns = [ordered]@{}
foreach ($movie in $Movies) {
    $countRuns["count-$movie"] = Start-Run $executable "--count-movie-video-frames $movie" (Join-Path $work "count-$movie")
    if ($countRuns.Count -eq 1) { Wait-Runs @{ "count-$movie" = $countRuns["count-$movie"] } $work }
}
Wait-Runs $countRuns $work
foreach ($movie in $Movies) {
    $line = Select-String -Path (Join-Path $work "count-$movie.out") -Pattern '^Video frames: (\d+)$'
    if ($null -eq $line) { throw "The frame count of $movie printed no 'Video frames:' line; see $(Join-Path $work "count-$movie.out")." }
    $counts[$movie] = [long]$line.Matches[0].Groups[1].Value
    $rate = Select-String -Path (Join-Path $work "count-$movie.out") -Pattern '^Sound sample rate: (\d+)$'
    if ($null -eq $rate) { throw "The frame count of $movie printed no 'Sound sample rate:' line." }
    $sampleRate = $rate.Matches[0].Groups[1].Value
    Write-Output ("{0}: {1} frames ({2:N1} min of video)" -f $movie, $counts[$movie], ($counts[$movie] / 3600.0))
}

# Plan every segment, longest replay first so the runs that replay furthest start early.
$segments = foreach ($movie in $Movies) {
    for ([long]$first = 0; $first -lt $counts[$movie]; $first += $SegmentFrames) {
        [pscustomobject]@{ Movie = $movie; First = $first; Count = [Math]::Min($SegmentFrames, $counts[$movie] - $first)
            Name = '{0}-{1:D7}' -f $movie, $first }
    }
}
$segments = @($segments | Sort-Object -Property First -Descending)
Write-Output "$($segments.Count) segments of up to $SegmentFrames frames, $Parallel at a time."

$running = @{}
$done = 0
$failures = @()
foreach ($segment in $segments) {
    while ($running.Count -ge $Parallel) {
        $finished = @($running.Keys | Where-Object { $running[$_].HasExited })
        if ($finished.Count -eq 0) { Start-Sleep -Seconds 5; continue }
        foreach ($name in $finished) {
            if ($running[$name].ExitCode -ne 0) { $failures += $name }
            $running.Remove($name)
            $done++
        }
        Write-Output ("[{0:hh\:mm\:ss}] {1}/{2} segments done" -f $watch.Elapsed, $done, $segments.Count)
    }
    if ($failures.Count -gt 0) { break }
    $video = Join-Path $work "$($segment.Name).mp4"
    $running[$segment.Name] = Start-Run $executable "--export-movie-video $($segment.Movie) `"$video`" $($segment.First) $($segment.Count)" (Join-Path $work $segment.Name)
}
foreach ($name in @($running.Keys)) {
    $running[$name].WaitForExit()
    if ($running[$name].ExitCode -ne 0) { $failures += $name }
}
if ($failures.Count -gt 0) {
    $details = $failures | ForEach-Object { "${_}:`n" + (Get-Content (Join-Path $work "$_.err") -Raw) }
    throw "These segments failed; nothing was joined:`n$($details -join "`n")"
}

# Join each movie: the pictures as they were encoded, the sound as one stream.
foreach ($movie in $Movies) {
    $parts = @($segments | Where-Object Movie -EQ $movie | Sort-Object -Property First)
    $list = Join-Path $work "$movie-segments.txt"
    Set-Content -LiteralPath $list -Encoding ascii -Value ($parts | ForEach-Object { "file '$((Join-Path $work "$($_.Name).mp4") -replace '\\', '/')'" })
    $sound = Join-Path $work "$movie.pcm"
    $stream = [IO.File]::Create($sound)
    try {
        foreach ($part in $parts) {
            $piece = [IO.File]::OpenRead((Join-Path $work "$($part.Name).mp4.pcm"))
            try { $piece.CopyTo($stream) } finally { $piece.Dispose() }
        }
    }
    finally { $stream.Dispose() }
    $final = Join-Path $output "$movie.mp4"
    Invoke-Ffmpeg @('-f', 'concat', '-safe', '0', '-i', $list, '-f', 's16le', '-ar', $sampleRate, '-ac', '2', '-i', $sound,
        '-map', '0:v', '-map', '1:a', '-c:v', 'copy', '-c:a', 'aac', '-b:a', '256k', '-movflags', '+faststart', $final)
    $frames = [long](& ffprobe -v error -select_streams v:0 -count_packets -show_entries stream=nb_read_packets -of csv=p=0 $final)
    if ($frames -ne $counts[$movie]) { throw "$final holds $frames frames; the replay ran $($counts[$movie])." }
    foreach ($part in $parts) {
        Remove-Item -LiteralPath (Join-Path $work "$($part.Name).mp4"), (Join-Path $work "$($part.Name).mp4.pcm")
    }
    Remove-Item -LiteralPath $list, $sound
    Write-Output "Wrote $final ($frames frames)."
}
Write-Output ("Done in {0:hh\:mm\:ss}." -f $watch.Elapsed)
