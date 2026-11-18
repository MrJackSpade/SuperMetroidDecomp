namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Where the intro flashback's Mother Brain stands, from taking Samus's missiles to her deletion behind page two.</summary>
internal enum IntroMotherBrainPhase
{
    /// <summary>Pulsing and taking missile hits; the fourth hit starts her explosion.</summary>
    TakingHits,

    /// <summary>Exploding: the room shakes and the explosion counts its updates; no missile hits her.</summary>
    Exploding,

    /// <summary>The page-two animation has started; its cues ask for the page and start the crossfade.</summary>
    StartingNextPage,

    /// <summary>Page two's crossfade runs: the room shakes on, and her hurt flash no longer alternates.</summary>
    CrossfadingToNextPage,

    /// <summary>Deleted as page two's crossfade finished: she draws nothing, and the room shakes on.</summary>
    Deleted,
}
