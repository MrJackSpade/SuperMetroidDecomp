namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>The supplied movies, by the names the replay commands use.</summary>
internal enum ReplayMovieId
{
    /// <summary>The #1275 snapshot-start recording paused in Mother Brain's room (<c>--mother-brain-glitch-movie</c>).</summary>
    MotherBrainGlitch,

    /// <summary>The 13% Speed Booster lsnes recording, captured on bsnes v085 (<c>--lsmv-playthrough-movie</c>).</summary>
    LowPercent13,

    /// <summary>The 100% Snes9x power-on playthrough (<c>--full-playthrough-movie</c>).</summary>
    Full100,

    /// <summary>The #1275 "Samus drunk" Snes9x 1.60 power-on playthrough (<c>--drunk-playthrough-movie</c>).</summary>
    Drunk,

    /// <summary>The contributed "lowc" Snes9x 1.60 power-on playthrough (<c>--lowc-playthrough-movie</c>).</summary>
    Lowc,

    /// <summary>The #1266 Ridley showcase snapshot-start recording (<c>--ridley-full-movie</c>).</summary>
    Ridley,
}
