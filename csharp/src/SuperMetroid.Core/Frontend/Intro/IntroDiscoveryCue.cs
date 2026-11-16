namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>What the discovery scene's egg animations have their owner do (#2197).</summary>
internal enum IntroDiscoveryCue
{
    /// <summary>Spawns the hatching egg's six shell pieces.</summary>
    SpawnShellPieces,
    /// <summary>Starts the intro's third page.</summary>
    StartPageThree,
    /// <summary>Has the egg wait for page three's cross-fade to end and then delete itself.</summary>
    DeleteEggAfterCrossFade,
}
