using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Verification's native view of the Powamp identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class PowampNativeWords
{
    /// <summary>The empty routine the balloon half runs, which Core gives no action (#1837).</summary>
    internal const ushort BalloonIdleWord = 0xc568;

    /// <summary>The routine word naming each <see cref="PowampAction"/>.</summary>
    internal static readonly NativeWordIdentities<PowampAction> PowampActions = new(new Dictionary<PowampAction, ushort>
    {
        [PowampAction.DeflatedResting] = 0xc283,
        [PowampAction.Inflating] = 0xc2a6,
        [PowampAction.InflatedRiseToTargetHeight] = 0xc2cf,
        [PowampAction.InflatedFinishWiggle] = 0xc36b,
        [PowampAction.GrappledRiseToTargetHeight] = 0xc3e1,
        [PowampAction.GrappledFinishWiggle] = 0xc469,
        [PowampAction.GrappledResting] = 0xc4dc,
        [PowampAction.Deflating] = 0xc500,
        [PowampAction.DeflatedSinking] = 0xc51d,
        [PowampAction.FatalDamage] = 0xc569,
        [PowampAction.DeathSequence] = 0xc59f,
    });
}
