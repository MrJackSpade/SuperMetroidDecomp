namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The counter cadences and phases of Draygon's fight and death (#1606): his shocks, turrets, bubbles, foam, smoke and flash.</summary>
internal static class DraygonCadenceDefinitions
{
    /// <summary>
    /// The updates on which a hurt Draygon holding Samus's connected grapple takes its shock: every eighth update of his
    /// body's own update count, on the period's first place.
    /// </summary>
    internal static readonly UpdateCadence GrappleShockEveryEighthUpdate = new(8, 0);

    /// <summary>
    /// The phase of a hurt Draygon's flash timer on which his body shows the white flash: set for two updates of every
    /// four, while his own colours show for the other two.
    /// </summary>
    internal static readonly UpdateCounterField HurtWhiteFlashShown = new(1, 1);

    /// <summary>
    /// The updates on which the wall turrets take their turn to fire: every 64th update of the frame counter, on the
    /// period's first place. Each turn draws a random number even when its turret is destroyed.
    /// </summary>
    internal static readonly UpdateCadence TurretTurnEverySixtyFourthUpdate = new(64, 0);

    /// <summary>
    /// The updates on which a swimming Draygon breathes a bubble: every 128th update of his body's own update count, on
    /// the period's first place.
    /// </summary>
    internal static readonly UpdateCadence BreathBubbleEvery128thUpdate = new(128, 0);

    /// <summary>
    /// The updates on which Draygon's eye breathes its long bubbles: every 128th update of the eye's own update count,
    /// on the period's first place, whether or not the eye turns.
    /// </summary>
    internal static readonly UpdateCadence EyeBreathBubbleEvery128thUpdate = new(128, 0);

    /// <summary>
    /// The updates on which Draygon spinning Samus in his grip leaves spiral foam beside him: every eighth update of his
    /// body's own update count, on the period's first place.
    /// </summary>
    internal static readonly UpdateCadence GrabFoamEveryEighthUpdate = new(8, 0);

    /// <summary>
    /// The updates on which a dying Draygon drifting to his burial point leaves spiral foam beside him: every sixteenth
    /// update of his body's own update count, on the period's first place.
    /// </summary>
    internal static readonly UpdateCadence DyingFoamEverySixteenthUpdate = new(16, 0);

    /// <summary>
    /// The updates on which a dying Draygon raises smoke at a random spot: every eighth update of the frame counter, on
    /// the period's first place. Each one draws a random number.
    /// </summary>
    internal static readonly UpdateCadence DyingSmokeEveryEighthUpdate = new(8, 0);
}
