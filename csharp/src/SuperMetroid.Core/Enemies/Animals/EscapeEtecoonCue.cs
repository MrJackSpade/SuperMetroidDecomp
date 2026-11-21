namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One cue of the escaping Etecoons' animations: what it does, with the values it acts on.</summary>
internal abstract partial record EscapeEtecoonCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EscapeEtecoonCue()
    {
    }

    /// <summary>
    /// A low-tide run starts (#2167): once the rising acid is above its low surface (<see cref="EscapeAnimalMotionDefinitions.LowAcidSurfaceY"/>) the owner starts the high-tide run the same way
    /// instead.
    /// </summary>
    /// <param name="Running">The way the Etecoon runs.</param>
    internal sealed partial record LowTideRunStarts(HorizontalDirection Running) : EscapeEtecoonCue;

    /// <summary>Moves the Etecoon horizontally.</summary>
    /// <param name="Offset">The cue's signed pixel offset.</param>
    internal sealed partial record Move(int Offset) : EscapeEtecoonCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(EscapeEtecoonCueKind Kind) : EscapeEtecoonCue;
}
