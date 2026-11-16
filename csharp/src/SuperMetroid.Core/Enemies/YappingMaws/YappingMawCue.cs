namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One cue of the Yapping Maw's animations: what it does, with the values it acts on.</summary>
internal abstract record YappingMawCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private YappingMawCue()
    {
    }

    /// <summary>Holds a grabbed Samus at the mouth offset of the cue's aim (#2071).</summary>
    /// <param name="Aim">The aim whose mouth offset Samus is held at.</param>
    internal sealed record HoldSamus(YappingMawAim Aim) : YappingMawCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(YappingMawCueKind Kind) : YappingMawCue;
}
