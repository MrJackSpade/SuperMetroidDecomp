namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>How the newly hatched, confused Baby Metroid moves (#1903).</summary>
internal enum ConfusedBabyMotion
{
    /// <summary>Waiting in the egg for it to hatch.</summary>
    WaitingForHatch,

    /// <summary>Rising out of the hatched egg toward Samus.</summary>
    Rising,

    /// <summary>Hovering at the top of its rise before it dances.</summary>
    Idling,

    /// <summary>Dancing around Samus until the cross-fade ends.</summary>
    Dancing,
}
