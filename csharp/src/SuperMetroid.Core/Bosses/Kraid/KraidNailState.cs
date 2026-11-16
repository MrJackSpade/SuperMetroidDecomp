namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's fingernails' own state beside what every part holds: the phase it runs, its velocity and which spawn
/// its last launch took, which its sibling reads.
/// </summary>
[ArchitectureReview("back-reference", "A part of Kraid's encounter (#2218): the encounter owns this part's state for the room and the part reads the encounter it joined at setup, so the two share one lifetime.")]
public sealed class KraidNailState : KraidPartState
{
    /// <summary>Makes a fingernail of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the fingernail is part of.</param>
    internal KraidNailState(KraidEnemyState kraid)
        : base(kraid)
    {
    }

    /// <summary>The phase the fingernail runs; its setup has it wait before its first launch.</summary>
    public KraidNailPhase Phase { get; internal set; }

    /// <summary>
    /// Gets whether this fingernail's last launch was the fixed-left spawn rather than the body-side one. A nail
    /// launches body-side whenever its sibling's last launch was fixed-left.
    /// </summary>
    public bool AlternateSpawn { get; internal set; }

    /// <summary>A fingernail's signed 1/65536-pixel velocity in pixels per update.</summary>
    public FixedVector<RoomPixels> NailVelocity { get; internal set; }
}
