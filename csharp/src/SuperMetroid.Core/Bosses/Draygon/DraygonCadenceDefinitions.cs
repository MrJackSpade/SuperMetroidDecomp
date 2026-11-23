namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The counter cadences and phases of Draygon's fight and death (#1606): his shocks, turrets, bubbles, foam, smoke and flash.</summary>
internal static partial class DraygonCadenceDefinitions
{
    /// <summary>
    /// The updates on which a hurt Draygon holding Samus's connected grapple takes its shock, counted on his body's own
    /// update count at the period's first place.
    /// </summary>
    internal static readonly UpdateCadence GrappleShock = new(8, 0);

    /// <summary>
    /// The phase of a hurt Draygon's flash timer on which his body shows the white flash: for two updates of every
    /// four, while his own colours show for the other two.
    /// </summary>
    internal static readonly UpdateAlternation HurtWhiteFlashShown = new(2);

    /// <summary>
    /// The updates on which the wall turrets take their turn to fire, counted on the frame counter at the period's first
    /// place. Each turn draws a random number even when its turret is destroyed.
    /// </summary>
    internal static readonly UpdateCadence TurretTurn = new(64, 0);

    /// <summary>
    /// The updates on which a swimming Draygon breathes a bubble, counted on his body's own update count at the period's
    /// first place.
    /// </summary>
    internal static readonly UpdateCadence BreathBubble = new(128, 0);

    /// <summary>
    /// The updates on which Draygon's eye breathes its long bubbles, counted on the eye's own update count at the period's
    /// first place, whether or not the eye turns.
    /// </summary>
    internal static readonly UpdateCadence EyeBreathBubble = new(128, 0);

    /// <summary>
    /// The updates on which Draygon spinning Samus in his grip leaves spiral foam beside him, counted on his body's own
    /// update count at the period's first place.
    /// </summary>
    internal static readonly UpdateCadence GrabFoam = new(8, 0);

    /// <summary>
    /// The updates on which a dying Draygon drifting to his burial point leaves spiral foam beside him, counted on his
    /// body's own update count at the period's first place.
    /// </summary>
    internal static readonly UpdateCadence DyingFoam = new(16, 0);

    /// <summary>
    /// The updates on which a dying Draygon raises smoke at a random spot, counted on the frame counter at the period's
    /// first place. Each one draws a random number.
    /// </summary>
    internal static readonly UpdateCadence DyingSmoke = new(8, 0);
}
