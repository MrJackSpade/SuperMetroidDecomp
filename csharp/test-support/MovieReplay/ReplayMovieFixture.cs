namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// One supplied movie's repository fixtures: the immutable recording, its expected digest, and the committed compact
/// trace of its native input-consumption timeline and WRAM checkpoints. Paths are relative to the repository root, the
/// working directory every replay runs from.
/// </summary>
/// <param name="Name">The short label used in replay messages.</param>
/// <param name="MoviePath">The recording.</param>
/// <param name="MovieSha256">The expected SHA-256 of the recording.</param>
/// <param name="TraceDirectory">The committed compact trace of the recording.</param>
internal sealed record ReplayMovieFixture(string Name, string MoviePath, string MovieSha256, string TraceDirectory)
{
    /// <summary>The 100% Snes9x playthrough.</summary>
    internal static ReplayMovieFixture Full100 { get; } = new("100%",
        "csharp/test-fixtures/full-100-percent/Super Metroid 100%.smv",
        "4D0E6E671E11BD99439AE498210943711135E6CBE8F57D97B2C56815315AE07E",
        "csharp/test-fixtures/replay-traces/full-100-percent");

    /// <summary>The 13% Speed Booster lsnes recording.</summary>
    internal static ReplayMovieFixture LowPercent13 { get; } = new("13%",
        "csharp/test-fixtures/low-13-percent/13_speedbooster.lsmv",
        "1D217BB073309AE44EA3D972C5EC3201BE0EC434C60F034E1E958F94FD1BCDA8",
        "csharp/test-fixtures/replay-traces/low-13-percent");

    /// <summary>The power-on recording used to check the issue 1275 Samus movement divergence.</summary>
    internal static ReplayMovieFixture Drunk { get; } = new("Samus drunk",
        "csharp/test-fixtures/issue-1275-movies/Samus drunk.smv",
        "6822B5D0F2D5CD81F7DB3E48E05E8EFE7F0EC2395669E0DDF80B7F5E8FC4DF17",
        "csharp/test-fixtures/replay-traces/samus-drunk");

    /// <summary>The snapshot-start recording used to check the issue 1275 Mother Brain divergence.</summary>
    internal static ReplayMovieFixture MotherBrainGlitch { get; } = new("Mother Brain glitch",
        "csharp/test-fixtures/issue-1275-movies/mother brain glitch.smv",
        "C63BF68D4F24F378B2E3E3591281ABC158DDBAEBA113D35A63E3307A32D6E505",
        "csharp/test-fixtures/replay-traces/mother-brain-glitch");

    /// <summary>The contributed Snes9x 1.60 power-on low-percent playthrough.</summary>
    internal static ReplayMovieFixture Lowc { get; } = new("lowc",
        "csharp/test-fixtures/contributed-movies/lowc.smv",
        "77BA6E6E58A4C416FD070A6B61AF942D661E675D2D32C82F85E8DE17C6991F7C",
        "csharp/test-fixtures/replay-traces/lowc");

    /// <summary>The supplied Ridley showcase movie (#1266).</summary>
    internal static ReplayMovieFixture Ridley { get; } = new("Ridley",
        "csharp/test-fixtures/issue-1266-ridley/Ridley fight showcase.smv",
        "7E12861DC56C5ABED12C2BFA2B00D24BFA418F49F2CE4C027D930CE9A3663F66",
        "csharp/test-fixtures/replay-traces/ridley");

    /// <summary>The fixtures of <paramref name="movie"/>.</summary>
    internal static ReplayMovieFixture Of(ReplayMovieId movie) => movie switch
    {
        ReplayMovieId.MotherBrainGlitch => MotherBrainGlitch,
        ReplayMovieId.LowPercent13 => LowPercent13,
        ReplayMovieId.Full100 => Full100,
        ReplayMovieId.Drunk => Drunk,
        ReplayMovieId.Lowc => Lowc,
        ReplayMovieId.Ridley => Ridley,
        _ => throw new ArgumentOutOfRangeException(nameof(movie), movie, "Undefined replay movie."),
    };

    /// <summary>Loads the recording after checking it is exactly the expected one.</summary>
    internal ReplayMovie Load() => ReplayMovie.Load(Name, MoviePath, MovieSha256);
}
