namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Where the intro Baby discovery's scripted input stands (#1554): what its playback watches for before each step.
/// Values are identities only; nothing reads them as numbers.
/// </summary>
internal enum IntroBabyDiscoveryDemoPhase
{
    /// <summary>Samus runs left until she passes her scripted place, where she stops and looks.</summary>
    RunningLeft,

    /// <summary>Samus stands and looks until the crossfade finishes, when the scripted input ends.</summary>
    StoppedAndLooking,

    /// <summary>The scripted input plays its ending and watches for nothing more.</summary>
    Ending,
}
