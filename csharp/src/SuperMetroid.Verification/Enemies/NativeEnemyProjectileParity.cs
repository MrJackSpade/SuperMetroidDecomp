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
            enemies.Projectiles.AdvanceEnemyProjectileAnimations(projectile, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()), Position16<RoomPixels>.Zero, shortCounter: SuperMetroid.Core.Compatibility.ShortFrameCount.Zero);
            native.Step(cartridge);
            string at = $"{label} update {update}";
            AssertEqual(!native.Deleted, projectile.IsActive, $"{at}: deletion");
            if (native.Deleted)
                return new CompareResult(true, update + 1);
            AssertFrameHold(native.Timer, projectile, at);
            // An operand that stores a pose's picture again shows that pose (#2273).
            AssertEqual(SuperMetroid.AssetExtraction.Enemies.EnemyProjectileProgramFrameOperands.OwnOperand(native.Operand),
                projectile.PresentationOperandAddress, $"{at}: frame");
            if (native.FrameBehaviorCleared)
                AssertEqual<Enum?>(null, projectile.MotionKind, $"{at}: cleared pre-instruction");
            else if (native.FrameBehavior is { } installed)
                AssertEqual(projectile.MotionOfWord(installed), projectile.Motion, $"{at}: installed pre-instruction");
        }
        return new CompareResult(false, update);
    }

    /// <summary>
    /// Whether <paramref name="hold"/>, a projectile's frame hold, is the timer of a frame the typed animation holds for
    /// good (#1607) beside a native list at <paramref name="nativeTimer"/>: the held frame rests on the hold, counting on
    /// from zero, while the native list counts its longest frame down before resting on the same frame, so within the
    /// updates a check runs the two stay the longest frame's duration apart.
    /// </summary>
    internal static bool RestsOnHeldFrame(ushort nativeTimer, WrappingCountdown hold) =>
        hold.RawValue == unchecked((ushort)(nativeTimer - NativeHeldFrameDuration));

    /// <summary>
    /// Compares <paramref name="projectile"/>'s frame hold with the native list's timer, <paramref name="nativeTimer"/>,
    /// while the hold runs. A stopped hold (#2380), where the typed animation shows a frame for good or rests, counts
    /// nothing, while the native list's timer counts down its longest frame, toward its sleep, or toward the frame it shows
    /// again and again, none of which shows anything new; the frame is compared on its own.
    /// </summary>
    internal static void AssertFrameHold(ushort nativeTimer, EnemyProjectile projectile, string at)
    {
        if (!projectile.IsFrameHoldStopped)
            AssertEqual(nativeTimer, projectile.FrameHold.RawValue, $"{at}: timer");
    }

    /// <summary>
    /// Compares an enemy's animation <paramref name="clock"/> with the native list's timer, <paramref name="nativeTimer"/>,
    /// while the clock runs. A stopped clock (#2454), where the typed animation shows a frame for good or rests, counts
    /// nothing, while the native list's timer counts down its longest frame or on from its sleep toward a step that shows
    /// nothing new; the frame is compared on its own.
    /// </summary>
    internal static void AssertClockHold(ushort nativeTimer, EnemyAnimationClock clock, string at)
    {
        ArgumentNullException.ThrowIfNull(clock);
        if (!clock.IsStopped)
            AssertEqual(nativeTimer, clock.Hold, $"{at}: timer");
    }
}
