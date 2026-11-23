namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where Mother Brain's glass is in its course (<see cref="MotherBrainGlassObject"/>).</summary>
internal abstract partial record MotherBrainGlassPhase
{
    /// <summary>Allows only the phases below.</summary>
    private MotherBrainGlassPhase()
    {
    }

    /// <summary>Spawned: its first update picks the cleared glass, the shattered glass or its first stage.</summary>
    internal sealed partial record Starting : MotherBrainGlassPhase;

    /// <summary>Holding a stage until the glass has taken that stage's hits.</summary>
    internal sealed partial record Holding : MotherBrainGlassPhase;

    /// <summary>
    /// <paramref name="Stage"/> broke and threw its first shards: the image after it shows before its second shards.
    /// </summary>
    /// <param name="Stage">The stage that burst.</param>
    internal sealed partial record Bursting(MotherBrainGlassStage.Bursts Stage) : MotherBrainGlassPhase;

    /// <summary>The last stage broke: the shattered glass shows before the destroyed event.</summary>
    internal sealed partial record Shattering : MotherBrainGlassPhase;

    /// <summary>The cleared or already-shattered glass was drawn: it leaves on its next update.</summary>
    internal sealed partial record Resolved : MotherBrainGlassPhase;
}
