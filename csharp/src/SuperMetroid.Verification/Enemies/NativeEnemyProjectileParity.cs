using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.MovieReplay.Verification;

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
    internal static CompareResult Compare(IImportCartridgeSource cartridge, RoomEnemySystem enemies,
        RoomEnemyProjectileSlot projectile, ushort entry, int updates, string label)
    {
        var native = new NativeEnemyProjectileListReader(entry, projectile.FrameHold.RawValue);
        int update = 0;
        for (; update < updates; update++)
        {
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), Position16<RoomPixels>.Zero);
            native.Step(cartridge);
            string at = $"{label} update {update}";
            AssertEqual(!native.Deleted, projectile.IsActive, $"{at}: deletion");
            if (native.Deleted)
                return new CompareResult(true, update + 1);
            AssertEqual(native.Timer, projectile.FrameHold.RawValue, $"{at}: timer");
            AssertEqual(native.Operand, projectile.PresentationOperandAddress, $"{at}: frame");
            if (native.FrameBehaviorCleared)
                AssertEqual(EnemyProjectileFrameBehavior.Stationary, projectile.FrameBehavior, $"{at}: cleared pre-instruction");
            else if (native.FrameBehavior is { } installed)
                AssertEqual(EnemyProjectileFrameBehaviorAccess.FromWord(installed), projectile.FrameBehavior, $"{at}: installed pre-instruction");
        }
        return new CompareResult(false, update);
    }
}
