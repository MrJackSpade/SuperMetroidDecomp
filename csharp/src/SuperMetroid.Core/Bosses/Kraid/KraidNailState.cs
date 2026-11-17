using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's fingernails' own state beside what every part holds: the phase it runs, its velocity and which spawn
/// its last launch took, which its sibling reads.
/// </summary>
[ArchitectureReview("back-reference", "A part of Kraid's encounter (#2218): the encounter owns this part's state for the room and the part reads the encounter it joined at setup, so the two share one lifetime.")]
public sealed class KraidNailState : KraidAnimatedPartState
{
    /// <summary>Makes a fingernail of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the fingernail is part of.</param>
    /// <param name="reference">Kraid's reference to the fingernail, which it joins as it sets up.</param>
    /// <param name="sibling">Kraid's reference to his other fingernail.</param>
    internal KraidNailState(KraidEnemyState kraid, KraidPartReference reference, KraidPartReference sibling)
        : base(kraid, reference, KraidPartSegment.NailLoop) => this.sibling = Ensure.NotNull(sibling);

    /// <summary>Kraid's reference to his other fingernail.</summary>
    private readonly KraidPartReference sibling;

    /// <summary>
    /// The place his other fingernail occupies (#2298), whose last launch this one's reads in that place, whatever the
    /// place holds.
    /// </summary>
    internal RoomEnemyPlace SiblingPlace => sibling.Place;

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
