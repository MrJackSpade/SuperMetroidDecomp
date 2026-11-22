namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The random choices of Draygon's fight and death: its attack, its goop, its turrets and its dying smoke.</summary>
internal static partial class DraygonRandomDefinitions
{
    /// <summary>Whether Draygon swoops rather than spits goop when it reaches a side (#2479): one time in two.</summary>
    internal static readonly RandomChance SwoopsRatherThanSpitsGoop = RandomChance.Highest(1, RandomSampleField.OneOf(2));

    /// <summary>
    /// The goop volley's step (#1619): one random number in sixteen counts the volley down and fires the next goop.
    /// </summary>
    internal static readonly RandomChance GoopVolleySteps = RandomChance.Lowest(1, RandomSampleField.OneOf(16));

    /// <summary>A goop's angle within its quarter turn, in steps of the turn.</summary>
    internal static readonly RandomSampleField GoopAngleSpread = RandomSampleField.OneOf(64);

    /// <summary>The part of the sample that chooses the cannon a turret cadence fires from (<see cref="DraygonCannonData.FiringCannon"/>).</summary>
    internal static readonly RandomSampleField TurretChoice = RandomSampleField.OneOf(4);

    /// <summary>A sinking Draygon's smoke X across its strip from <see cref="DraygonArenaDefinitions.DeathSmokeOrigin"/>.</summary>
    internal static readonly RandomSampleField DeathSmokeX = RandomSampleField.OneOf(128);

    /// <summary>A sinking Draygon's smoke Y down its band from <see cref="DraygonArenaDefinitions.DeathSmokeOrigin"/>.</summary>
    internal static readonly RandomSampleField DeathSmokeY = RandomSampleField.SecondOf(64);

    /// <summary>A dying Draygon's sprite scatter across 128 pixels centred on its body's X.</summary>
    internal static readonly RandomSampleField DyingScatterX = RandomSampleField.OneOf(128);

    /// <summary>
    /// A dying Draygon's sprite scatter down 128 pixels centred on its body's Y.
    /// </summary>
    internal static readonly RandomSampleField DyingScatterY = RandomSampleField.SecondOf(128);

    /// <summary>
    /// The tail whip Draygon inserts while he holds Samus (#1619): one roll in 256, the lowest, whips.
    /// </summary>
    internal static readonly RandomChance TailWhips = RandomChance.Lowest(1, RandomSampleField.OneOf(256));
}
