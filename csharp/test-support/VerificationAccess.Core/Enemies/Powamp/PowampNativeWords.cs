using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Verification's native view of the Powamp identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class PowampNativeWords
{
    /// <summary>The routine word naming each <see cref="PowampEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<PowampEnemyFunction> PowampEnemyFunctions = new(new Dictionary<PowampEnemyFunction, ushort>
    {
        [PowampEnemyFunction.DeflatedResting] = 0xc283,
        [PowampEnemyFunction.Inflating] = 0xc2a6,
        [PowampEnemyFunction.InflatedRiseToTargetHeight] = 0xc2cf,
        [PowampEnemyFunction.InflatedFinishWiggle] = 0xc36b,
        [PowampEnemyFunction.GrappledRiseToTargetHeight] = 0xc3e1,
        [PowampEnemyFunction.GrappledFinishWiggle] = 0xc469,
        [PowampEnemyFunction.GrappledResting] = 0xc4dc,
        [PowampEnemyFunction.Deflating] = 0xc500,
        [PowampEnemyFunction.DeflatedSinking] = 0xc51d,
        [PowampEnemyFunction.BalloonNoOp] = 0xc568,
        [PowampEnemyFunction.FatalDamage] = 0xc569,
        [PowampEnemyFunction.DeathSequence] = 0xc59f,
    });
}
