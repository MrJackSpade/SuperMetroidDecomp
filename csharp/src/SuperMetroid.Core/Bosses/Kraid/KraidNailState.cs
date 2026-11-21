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
    /// His other fingernail as this one's launch reads it in its place (#2298), whatever the place holds
    /// (<see cref="RoomEnemyPlace.FoundState{T}"/>): the state it is attached as, the <see cref="KraidPartState"/> every
    /// Kraid part is attached as, or null when the place reads as holding none, as a killed sibling's cleared place does.
    /// Only his other fingernail joins that place, so a part found there is a fingernail.
    /// </summary>
    internal KraidNailState? FoundSibling => (KraidNailState?)sibling.Place.FoundState<KraidPartState>();

    /// <summary>The phase the fingernail runs; its setup has it wait before its first launch.</summary>
    public KraidNailPhase Phase { get; internal set; }

    /// <summary>
    /// The side this fingernail's last launch took (#2560): the body-side spawn, or the fixed-left one. A nail launches
    /// body-side whenever its sibling's last launch was fixed-left.
    /// </summary>
    public KraidNailLaunchSide LaunchSide { get; internal set; }

    /// <summary>A fingernail's velocity in pixels per update, positive right and down.</summary>
    public FixedVector<RoomPixels> NailVelocity { get; internal set; }
}
