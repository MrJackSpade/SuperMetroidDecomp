namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Where one of Samus's special suit colourings (Crystal Flash's, or the drained rainbow's) stands for its colouring step
/// (#2562): showing its own colours, or ended, its next colouring update showing her ordinary colours again.
/// </summary>
public enum SpecialColouringStage
{
    /// <summary>The colouring shows its own colours.</summary>
    Showing,
    /// <summary>
    /// The colouring has ended: its next colouring update shows Samus's ordinary colours again, which the colouring step
    /// shows after the steps of the update that ended it.
    /// </summary>
    Ending,
}
