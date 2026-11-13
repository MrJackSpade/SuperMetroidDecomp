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
    /// <summary>The longest duration a native frame command gives: the frame a typed animation holds for good.</summary>
    private const ushort NativeHeldFrameDuration = 0x7fff;

    /// <summary>
    /// Compares <paramref name="projectile"/> with the native list at <paramref name="entry"/> for up to
    /// <paramref name="updates"/> updates or until the list deletes it; returns whether it did and the updates compared.
    /// </summary>
    internal static CompareResult Compare(IImportCartridgeSource cartridge, RoomEnemySystem enemies,
        EnemyProjectile projectile, ushort entry, int updates, string label)
    {
        var native = new NativeEnemyProjectileListReader(entry, projectile.FrameHold.RawValue);
        int update = 0;
        for (; update < updates; update++)
        {
            enemies.Projectiles.AdvanceEnemyProjectileAnimations(projectile, new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()), Position16<RoomPixels>.Zero);
            native.Step(cartridge);
            string at = $"{label} update {update}";
            AssertEqual(!native.Deleted, projectile.IsActive, $"{at}: deletion");
            if (native.Deleted)
                return new CompareResult(true, update + 1);
            if (!RestsOnHeldFrame(native.Timer, projectile.FrameHold))
                AssertEqual(native.Timer, projectile.FrameHold.RawValue, $"{at}: timer");
            AssertEqual(native.Operand, projectile.PresentationOperandAddress, $"{at}: frame");
            if (native.FrameBehaviorCleared)
                AssertEqual(EnemyProjectileFrameBehavior.Stationary, projectile.FrameBehavior, $"{at}: cleared pre-instruction");
            else if (native.FrameBehavior is { } installed)
                AssertEqual(EnemyProjectileFrameBehaviorAccess.FromWord(installed), projectile.FrameBehavior, $"{at}: installed pre-instruction");
        }
        return new CompareResult(false, update);
    }

    /// <summary>
    /// Whether <paramref name="hold"/> is the timer of a frame the typed animation holds for good (#1607) beside a native
    /// list at <paramref name="nativeTimer"/>: the held frame rests on the hold's timer, counting on from zero, while the
    /// native list counts its longest frame down before resting on the same frame, so within the updates a check runs the
    /// two stay the longest frame's duration apart.
    /// </summary>
    internal static bool RestsOnHeldFrame(ushort nativeTimer, WrappingCountdown hold) =>
        hold.RawValue == unchecked((ushort)(nativeTimer - NativeHeldFrameDuration));
}
