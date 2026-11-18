namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The random choices of Draygon's fight and death: its attack, its goop, its turrets and its dying smoke.</summary>
internal static class DraygonRandomDefinitions
{
    /// <summary>Whether Draygon swoops rather than spits goop when it reaches a side (#2479): an odd sample swoops, one time in two.</summary>
    internal static readonly RandomChance SwoopsRatherThanSpitsGoop = RandomChance.Highest(1, new RandomSampleField(0, 1));

    /// <summary>
    /// The goop volley's step (#1619): one random number in sixteen counts the volley down and fires the next goop.
    /// </summary>
    internal static readonly RandomChance GoopVolleySteps = RandomChance.Lowest(1, new RandomSampleField(0, 4));

    /// <summary>A goop's angle within its quarter turn, 0 through 63 steps (256ths of a turn).</summary>
    internal static readonly RandomSampleField GoopAngleSpread = new(0, 6);

    /// <summary>The part of the sample that chooses the cannon a turret cadence fires from (<see cref="DraygonCannonData.FiringCannon"/>).</summary>
    internal static readonly RandomSampleField TurretChoice = new(0, 2);

    /// <summary>A sinking Draygon's smoke X across its 128-pixel strip from X 192.</summary>
    internal static readonly RandomSampleField DeathSmokeX = new(0, 7);

    /// <summary>A sinking Draygon's smoke Y down its 64-pixel band from Y 400.</summary>
    internal static readonly RandomSampleField DeathSmokeY = new(8, 6);

    /// <summary>A dying Draygon's sprite scatter across 128 pixels centred on its body's X.</summary>
    internal static readonly RandomSampleField DyingScatterX = new(0, 7);

    /// <summary>
    /// A dying Draygon's sprite scatter down 128 pixels centred on its body's Y.
    /// </summary>
    internal static readonly RandomSampleField DyingScatterY = new(8, 7);

    /// <summary>
    /// The tail whip Draygon inserts while he holds Samus (#1619): one roll in 256, the lowest, whips.
    /// </summary>
    internal static readonly RandomChance TailWhips = RandomChance.Lowest(1, new RandomSampleField(0, 8));
}
