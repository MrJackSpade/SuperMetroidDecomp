namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>What the discovery scene's egg animations have their owner do (#2197).</summary>
internal enum IntroDiscoveryCue
{
    /// <summary>Spawns the hatching egg's six shell pieces.</summary>
    SpawnShellPieces,
    /// <summary>Starts the narration page the discovery leads into.</summary>
    StartNextPage,
    /// <summary>Has the egg wait for the next page's cross-fade to end and then delete itself.</summary>
    DeleteEggAfterCrossFade,
}
