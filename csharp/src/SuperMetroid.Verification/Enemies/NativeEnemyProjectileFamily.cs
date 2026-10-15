using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// A typed enemy-projectile family beside its native bank-$86 lists: the ROM oracle compares its programs once, and
/// recordings translate its typed cursors to the native instruction pointers they hold.
/// </summary>
internal abstract class NativeEnemyProjectileFamily
{
    /// <summary>The family of <paramref name="programs"/>, whose native lists start at <paramref name="entries"/>.</summary>
    internal static NativeEnemyProjectileProgramFamily<TProgram, TFrame, TCue> Of<TProgram, TFrame, TCue>(
        (TProgram Program, ushort Entry)[] entries,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        Func<TFrame, ushort> presentationKey,
        NativeEnemyProjectileCueRules<TCue> rules,
        Func<TFrame, ushort?>? reflectedPresentationKey = null)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull =>
        new(entries, programs, presentationKey, rules, reflectedPresentationKey);

    /// <summary>Compares every program with its native list and returns the total step count.</summary>
    internal abstract int CompareAll(IImportCartridgeSource cartridge);

    /// <summary>
    /// Walks the family's lists once and returns the native instruction pointer a typed animation of this family stands
    /// for, or null for an animation of another family.
    /// </summary>
    internal abstract Func<EnemyProjectileAnimation, ushort?> NativePointers(IImportCartridgeSource cartridge);

    /// <summary>
    /// Moves <paramref name="animation"/>, when it is this family's, to the typed step a recording's native instruction
    /// pointer <paramref name="nativePointer"/> stands for; false for an animation of another family.
    /// </summary>
    internal abstract bool TryResumeAt(IImportCartridgeSource cartridge, EnemyProjectileAnimation animation, ushort nativePointer);
}
