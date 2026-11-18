namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>What the intro Mother Brain's animations have her owner do (#2197).</summary>
internal enum IntroMotherBrainCue
{
    /// <summary>Starts the narration page the flashback leads into.</summary>
    StartNextPage,
    /// <summary>Starts Mother Brain's cross-fade into the next page: the room shakes, and she goes once the cross-fade ends.</summary>
    StartCrossFade,
}
