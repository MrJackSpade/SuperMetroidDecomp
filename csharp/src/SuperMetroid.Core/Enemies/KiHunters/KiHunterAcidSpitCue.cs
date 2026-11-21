namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One cue of the Ki-Hunter acid's animations (#1843): what it does, with the values it acts on.</summary>
internal abstract partial record KiHunterAcidSpitCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private KiHunterAcidSpitCue()
    {
    }

    /// <summary>Launches the acid out of the mouth the cue's way (#2163).</summary>
    /// <param name="Direction">The way the acid leaves.</param>
    internal sealed partial record Launch(HorizontalDirection Direction) : KiHunterAcidSpitCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(KiHunterAcidSpitCueKind Kind) : KiHunterAcidSpitCue;
}
