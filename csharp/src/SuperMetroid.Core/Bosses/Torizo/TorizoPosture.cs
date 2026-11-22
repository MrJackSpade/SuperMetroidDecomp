namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Bomb or Golden Torizo's posture (#2237): the way it faces, the foot its last step left forward, and whether it is
/// turning around. A step sets the facing and the foot and ends any turn; the turning cue sets the turn alone.
/// </summary>
/// <param name="Facing">The way the Torizo faces.</param>
/// <param name="LeadingFoot">The foot the Torizo's last step left forward, the one it stepped on.</param>
internal readonly partial record struct TorizoPosture(HorizontalDirection Facing, HorizontalDirection LeadingFoot)
{
    /// <summary>A new Torizo's posture: facing left on its left foot, not turning.</summary>
    internal static TorizoPosture Initial { get; } = new(HorizontalDirection.Left, HorizontalDirection.Left);

    /// <summary>Whether the Torizo is turning around; its drool then sprays in every direction.</summary>
    internal bool IsTurning { get; private init; }

    /// <summary>The posture a step <paramref name="direction"/> on <paramref name="foot"/> leaves (#2165): facing that way on that foot, not turning.</summary>
    internal static TorizoPosture AfterStep(HorizontalDirection direction, HorizontalDirection foot) => new(direction, foot);

    /// <summary>This posture, turning around.</summary>
    internal TorizoPosture TurningAround => this with { IsTurning = true };

    /// <summary>Whether the leading foot is the one away from the facing side: the right foot facing left, the left facing right.</summary>
    internal bool LeadsWithFarFoot => LeadingFoot != Facing;
}
