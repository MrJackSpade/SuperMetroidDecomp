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
    /// <param name="place">The place of the fingernail, which his body found as his room lists it.</param>
    /// <param name="sibling">The place of his other fingernail.</param>
    internal KraidNailState(KraidEnemyState kraid, RoomEnemyPlace place, RoomEnemyPlace sibling)
        : base(kraid, place, KraidPartSegment.NailLoop) => SiblingPlace = Ensure.NotNull(sibling);

    /// <summary>
    /// The place his other fingernail occupies (#2298), whose last launch this one's reads in that place, whatever the
    /// place holds.
    /// </summary>
    internal RoomEnemyPlace SiblingPlace { get; }

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
