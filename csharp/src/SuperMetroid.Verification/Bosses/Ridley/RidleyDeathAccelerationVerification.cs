using static SuperMetroid.Verification.Bosses.Ridley.RidleyInertiaVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks Ridley's two death-phase callers against the native inertia index and reversal boost.</summary>
internal static class RidleyDeathAccelerationVerification
{
    /// <summary>Confirms index zero and boost sixteen from retail code, then checks both axes across 126 distance/velocity cases.</summary>
    internal static void VerifyRidleyDeathAcceleration(CartridgeImportAddressSpace rom)
    {
        // $A6:C60E LDY #0, $C611 LDA #16: index and extra deceleration are
        // distinct native arguments, not an out-of-table index of sixteen.
        AssertEqual((byte)0xa0, rom.ReadCartridgeByte(0xa6c60e), "Death movement LDY immediate opcode");
        AssertEqual((byte)0, rom.ReadCartridgeByte(0xa6c60f), "Death movement native inertia index");
        AssertEqual((byte)0xa9, rom.ReadCartridgeByte(0xa6c611), "Death movement LDA immediate opcode");
        AssertEqual((byte)16, rom.ReadCartridgeByte(0xa6c612), "Death movement native extra deceleration");
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        Action<RoomEnemyPlace, NorfairRidleyState> approaching = NorfairRidleyCombat.TickNorfairRidleyMoveToDeathSpot;
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        Action<RoomEnemyPlace, NorfairRidleyState> roaring = (body, roaringState) => enemies.NorfairRidleyBehavior.Death.TickNorfairRidleyDeathRoar(body, roaringState, samus);
        var place = enemies.Places.InUpdateOrder[0];
        var state = new NorfairRidleyState();
        int[] distances = [-256, -64, -16, -1, 0, 1, 16, 64, 256];
        int[] velocities = [-1280, -256, -1, 0, 1, 256, 1280];
        foreach (var step in new[] { approaching, roaring })
        foreach (int distance in distances)
        foreach (int velocity in velocities)
        {
            place.Position = new(new(unchecked((ushort)(128 + distance)), 0x8123), new(unchecked((ushort)(328 - distance)), 0x8123));
            state.Flight.Velocity = new EightEightVector<RoomPixels>(new SignedEightEight(unchecked((ushort)velocity)), new SignedEightEight(unchecked((ushort)velocity)));
            state.PhaseTimer.LastFor(21);
            step(place, state);
            AssertEqual(ExpectedRidleyNativeAcceleration(unchecked((ushort)velocity), place.Position.X.Whole, 128, 16, 16), state.Flight.Velocity.X.RawValue, "Death phase native X acceleration");
            AssertEqual(ExpectedRidleyNativeAcceleration(unchecked((ushort)velocity), place.Position.Y.Whole, 328, 16, 16), state.Flight.Velocity.Y.RawValue, "Death phase native Y acceleration");
            AssertEqual(unchecked((ushort)(128 + distance)), place.Position.X.Whole, "Death phase leaves integration to shared movement");
            AssertEqual((ushort)0x8123, place.Position.X.Fraction, "Death phase preserves fractional position");
        }
        Console.WriteLine("Ridley death acceleration: both phase callers match native index 0 / reversal boost 16 across 126 cases.");

    }
}
