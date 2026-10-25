using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// Verification's native view of the FuneNamihe identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class FuneNamiheNativeWords
{
    /// <summary>The routine word naming each <see cref="FuneNamiheEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<FuneNamiheEnemyFunction> FuneNamiheEnemyFunctions = new(new Dictionary<FuneNamiheEnemyFunction, ushort>
    {
        [FuneNamiheEnemyFunction.FuneWaitForCooldown] = 0x9737,
        [FuneNamiheEnemyFunction.NamiheWaitForSamus] = 0x975c,
        [FuneNamiheEnemyFunction.FuneActivityNoOp] = 0x978e,
        [FuneNamiheEnemyFunction.NamiheActivityNoOp] = 0x978f,
    });

    /// <summary>The routine word naming each <see cref="NamiFuneFireballFunction"/>.</summary>
    internal static readonly NativeWordIdentities<NamiFuneFireballFunction> NamiFuneFireballFunctions = new(new Dictionary<NamiFuneFireballFunction, ushort>
    {
        [NamiFuneFireballFunction.MovingLeft] = 0xdf40,
        [NamiFuneFireballFunction.MovingRight] = 0xdf6a,
    });
}
