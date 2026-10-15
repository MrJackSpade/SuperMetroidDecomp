using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Runs a projectile's typed animation through the real projectile interpreter beside its native bank-$86 list (#641),
/// comparing deletion, timer and shown frame each update.
/// </summary>
internal static class NativeEnemyProjectileParity
{
    /// <summary>
    /// Compares <paramref name="projectile"/> with the native list at <paramref name="entry"/> for up to
    /// <paramref name="updates"/> updates or until the list deletes it; returns whether it did and the updates compared.
    /// </summary>
    internal static (bool Deleted, int Updates) Compare(IImportCartridgeSource cartridge, RoomEnemySystem enemies,
        RoomEnemyProjectileSlot projectile, ushort entry, int updates, string label)
    {
        var native = new NativeEnemyProjectileListDefinitions.Reader(entry, projectile.InstructionTimer.RawValue);
        int update = 0;
        for (; update < updates; update++)
        {
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
            native.Step(cartridge);
            string at = $"{label} update {update}";
            AssertEqual(!native.Deleted, projectile.IsActive, $"{at}: deletion");
            if (native.Deleted)
                return (true, update + 1);
            AssertEqual(native.Timer, projectile.InstructionTimer.RawValue, $"{at}: timer");
            AssertEqual(native.Operand, projectile.PresentationOperandAddress, $"{at}: frame");
            if (native.PreInstructionCleared)
                AssertEqual((ushort)EnemyProjectilePreInstruction.RTS_868170, projectile.PreInstruction, $"{at}: cleared pre-instruction");
            else if (native.PreInstruction is { } installed)
                AssertEqual(installed, projectile.PreInstruction, $"{at}: installed pre-instruction");
        }
        return (false, update);
    }
}
