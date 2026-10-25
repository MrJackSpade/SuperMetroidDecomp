using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Verification's native import of enemy projectiles: the frame a recorded list operand shows.</summary>
internal static class EnemyProjectileNativeImportAccess
{
    /// <summary>
    /// Shows the recorded frame at bank-$86 operand <paramref name="operandAddress"/>: the installed program frame when
    /// the operand names one, otherwise the composition frame drawing the bank-$8D spritemap the cartridge selects there.
    /// </summary>
    internal static void ShowNativeOperand(RoomEnemyProjectileSlot projectile, ushort operandAddress) =>
        projectile.Frame = EnemyProjectileProgramFrameSources.Contains(operandAddress)
            ? EnemyProjectileProgramFrameSources.Frame(operandAddress)
            : EnemyProjectileSlotFrameAccess.CompositionAt(
                CompiledEnemyVisualSelectors.TryGet(SnesLittleEndianConstants.BankByte(EnemyProjectileCodePointers.BankBase), operandAddress, out ushort selector)
                    ? selector
                    : throw new InvalidDataException($"Enemy projectile visual operand $86:{operandAddress:X4} has no compiled selector."));
}
