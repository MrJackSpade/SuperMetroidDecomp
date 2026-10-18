using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Verification's native import of enemy projectiles: the frame a recorded list operand shows.</summary>
internal static class EnemyProjectileNativeImportAccess
{
    /// <summary>
    /// Shows the recorded frame at bank-$86 operand <paramref name="operandAddress"/>: installed artwork when the operand
    /// names a program frame, otherwise the bank-$8D composition the cartridge selects there.
    /// </summary>
    internal static void ShowNativeOperand(RoomEnemyProjectileSlot projectile, ushort operandAddress)
    {
        if (EnemyProjectilePresentationFrameDefinitions.Contains(operandAddress))
        {
            projectile.PresentationOperandAddress = operandAddress;
            projectile.SpritemapPointer = EnemyProjectileSpritemapDefinitions.BlankSpritemap;
            return;
        }

        projectile.PresentationOperandAddress = 0;
        projectile.SpritemapPointer =
            CompiledEnemyVisualSelectors.TryGet(SnesLittleEndian.BankByte(EnemyProjectileCodePointers.BankBase),
                    operandAddress, out ushort selector)
                ? selector
                : throw new InvalidDataException($"Enemy projectile visual operand $86:{operandAddress:X4} has no compiled selector.");
    }
}
