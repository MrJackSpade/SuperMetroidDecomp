using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Enemies;

/// <summary>One typed enemy-projectile family's oracle parameters.</summary>
internal sealed class NativeEnemyProjectileProgramFamily<TProgram, TFrame, TCue>(
    (TProgram Program, ushort Entry)[] entries,
    AnimationProgramSet<TProgram, TFrame, TCue> programs,
    Func<TFrame, ushort> presentationKey,
    NativeEnemyProjectileCueRules<TCue> rules,
    Func<TFrame, ushort?>? reflectedPresentationKey) : NativeEnemyProjectileFamily
    where TProgram : struct, Enum
    where TFrame : struct
    where TCue : notnull
{
    /// <inheritdoc/>
    internal override int CompareAll(IImportCartridgeSource cartridge) =>
        NativeEnemyProjectileProgramOracle.CompareAll(cartridge, entries, programs, presentationKey, rules, reflectedPresentationKey);

    /// <inheritdoc/>
    internal override Func<EnemyProjectileAnimation, ushort?> NativePointers(IImportCartridgeSource cartridge)
    {
        Dictionary<TProgram, ushort[]> addresses =
            NativeEnemyProjectileProgramOracle.StepAddresses(cartridge, entries, programs, presentationKey, rules, reflectedPresentationKey);
        return animation => animation is EnemyProjectileProgramAnimation<TProgram, TFrame, TCue> typed
            ? addresses[typed.Cursor.Program][typed.Cursor.Position]
            : null;
    }
}
