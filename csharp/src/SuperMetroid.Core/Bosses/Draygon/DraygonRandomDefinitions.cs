namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The random choices of Draygon's fight and death: its attack, its goop, its turrets and its dying smoke.</summary>
internal static class DraygonRandomDefinitions
{
    /// <summary>Whether Draygon swoops rather than spits goop when it reaches a side: the sample's bit zero, set to swoop.</summary>
    internal static readonly RandomSampleField SwoopRatherThanGoop = new(0, 1);

    /// <summary>
    /// The goop volley's step check, the sample's bits 0-3: only a zero, one sample in sixteen, counts the volley down
    /// and fires the next goop.
    /// </summary>
    internal static readonly RandomSampleField GoopVolleyStep = new(0, 4);

    /// <summary>A goop's angle within its quarter turn, 0 through 63 table angles: the sample's bits 0-5.</summary>
    internal static readonly RandomSampleField GoopAngleSpread = new(0, 6);

    /// <summary>Which of the four entries of the turret firing order a turret cadence fires: the sample's bits 0-1.</summary>
    internal static readonly RandomSampleField TurretChoice = new(0, 2);

    /// <summary>A sinking Draygon's smoke X across its 128-pixel strip from X 192: the sample's bits 0-6.</summary>
    internal static readonly RandomSampleField DeathSmokeX = new(0, 7);

    /// <summary>A sinking Draygon's smoke Y down its 64-pixel band from Y 400: the sample's bits 8-13.</summary>
    internal static readonly RandomSampleField DeathSmokeY = new(8, 6);

    /// <summary>A dying Draygon's sprite scatter across 128 pixels centred on its body's X: the sample's bits 0-6.</summary>
    internal static readonly RandomSampleField DyingScatterX = new(0, 7);

    /// <summary>
    /// A dying Draygon's sprite scatter down 128 pixels centred on its body's Y: the sample's bits 8-14, from its upper
    /// random byte.
    /// </summary>
    internal static readonly RandomSampleField DyingScatterY = new(8, 7);

    /// <summary>The roll that inserts a tail whip while Draygon holds Samus, which a zero value does: 0 through 255 from the sample.</summary>
    internal static readonly RandomSampleField TailWhipRoll = new(0, 8);
}
