using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Reproduces two issue #1269 Shitroid arithmetic details from the recorded encounter.</summary>
internal static class ShitroidGradualAccelerationVerification
{
    /// <summary>
    /// Checks $A9:F46B's reversal chain <c>SEC; SBC #8; SBC q; SBC q</c>. Starting at
    /// zero velocity, the borrow produces $FFF1; the recorded first rise after draining
    /// the Sidehopper moves Y subpixel by -$F00.
    /// </summary>
    internal static void VerifyShitroidGradualAcceleration()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.BigBoy);
        RoomEnemySlot shitroid = runtime.Enemies.SlotTable.Slots.First(slot =>
            slot.DefinitionId == EnemyDefinitionId.BabyMetroid);

        // The movie's state when the drain ends, entering RiseAfterFeeding with no velocity.
        ShitroidEnemyState state = shitroid.SpeciesState<ShitroidEnemyState>()
            ?? throw new InvalidOperationException("The Big Boy room's Shitroid has no state.");
        state.Function = ShitroidAiFunction.RiseAfterFeeding;
        state.Velocity = default;
        shitroid.Position = shitroid.Position.WithWholeX(0x02d6).WithWholeY(0x009c).WithFractionY(0x6b00);
        runtime.Enemies.ShitroidBehavior.RunShitroidMain(shitroid, runtime.Player.Samus, new Position16<RoomPixels>(0x0200, 0x0000), null);

        AssertEqual((ushort)0xfff1, state.Velocity.Y.RawValue, "the reversal step borrows once, reaching Y velocity $FFF1");
        AssertEqual((ushort)0x009c, shitroid.Position.Y.Whole, "the rise stays within the same pixel");
        AssertEqual((ushort)0x5c00, shitroid.Position.Y.Fraction, "the Y subpixel moves by -$F00");
        Console.WriteLine("Shitroid gradual acceleration: the native SBC borrow chain sets the first rise's velocity.");
    }

    /// <summary>
    /// Checks $A9:F21B's drain X addition without CLC. The carry comes from the
    /// $A9:F238 comparison of Samus's Y speed with four, moving the latched Shitroid
    /// one pixel right on the recorded jump update.
    /// </summary>
    internal static void VerifyShitroidDrainCarry()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.BigBoy);
        RoomEnemySlot shitroid = runtime.Enemies.SlotTable.Slots.First(slot =>
            slot.DefinitionId == EnemyDefinitionId.BabyMetroid);
        var samus = runtime.Player.IntroducedSamus;
        ShitroidEnemyState state = shitroid.SpeciesState<ShitroidEnemyState>()
            ?? throw new InvalidOperationException("The Big Boy room's Shitroid has no state.");
        foreach ((ushort ySpeed, ushort expectedX, ushort expectedSpeed) in new VerifyShitroidDrainCarryCase[]
            {
                new VerifyShitroidDrainCarryCase(5, 0x01c4, 2), new VerifyShitroidDrainCarryCase(1, 0x01c3, 1),
                // A rise slowed past zero (#1562): CMP #4 still sets the carry, but BMI sees a negative difference and keeps the speed.
                new VerifyShitroidDrainCarryCase(0xffff, 0x01c4, 0xffff),
            })
        {
            state.Function = ShitroidAiFunction.DrainSamus;
            state.Velocity = default;
            shitroid.FrameCounter = 0;
            samus.Health = 500;
            samus.Position = samus.Position.WithWhole(new(0x01c3, 0x00b9));
            samus.Kinematics.YSpeed = samus.Kinematics.YSpeed.WithWhole(ySpeed);
            runtime.Enemies.ShitroidBehavior.RunShitroidMain(shitroid, samus, new Position16<RoomPixels>(0x0158, 0x0000), null);
            AssertEqual(expectedX, shitroid.Position.X.Whole, $"Y speed {ySpeed}: the drain's X add takes CMP #4's carry");
            AssertEqual(expectedSpeed, samus.Kinematics.YSpeed.Whole, $"Y speed {ySpeed}: the drain clamps only speeds of 4 or more");
        }
        Console.WriteLine("Shitroid drain carry: a Y speed of 4 or more adds one pixel to the latched X.");
    }

    /// <summary>The original's gradual-acceleration divisor table (<c>$A9:F56A</c>): 16 down to 1.</summary>
    private static readonly int[] OriginalDivisorTable = [16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1];

    /// <summary>The extra speed the original passes every Shitroid flight for an off-screen reversal (<c>$A9:F451</c>).</summary>
    private const int OriginalOffScreenReversalKick = 0x0400;

    /// <summary>
    /// Each Shitroid flight steers with the divisor the original's call selects from its table and the off-screen kick the
    /// original passes (#1684): rushes at table position 15 (<c>$A9:F053</c>, <c>$A9:F07B</c>, <c>$A9:F208</c>), the
    /// stare-down at 10 (<c>$A9:F1EA</c>), the remorseful follow at 8 (<c>$A9:F436</c>) and the drifts at 0
    /// (<c>$A9:F142</c>, <c>$A9:F2CD</c>, <c>$A9:F30C</c>, <c>$A9:F335</c>, <c>$A9:F37A</c>), all through <c>$A9:F451</c>.
    /// The production main runs each flight from the Big Boy room, on and off screen and moving toward and away from its
    /// point, and its new velocity equals the gradual acceleration (checked against the original's chained carries by the
    /// Shitroid seeking acceleration check) with that divisor and kick.
    /// </summary>
    internal static void VerifyShitroidFlightPulls()
    {
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.BigBoy);
        RoomEnemySlot shitroid = runtime.Enemies.SlotTable.Slots.First(slot =>
            slot.DefinitionId == EnemyDefinitionId.BabyMetroid);
        ShitroidEnemyState state = shitroid.SpeciesState<ShitroidEnemyState>()
            ?? throw new InvalidOperationException("The Big Boy room's Shitroid has no state.");
        ShitroidBehavior behavior = runtime.Enemies.ShitroidBehavior;
        var samus = runtime.Player.IntroducedSamus;
        RoomEnemySlot victim = behavior.RequireShitroidVictim(shitroid);

        var flights = new (ShitroidAiFunction Function, int TablePosition, Func<Position16<RoomPixels>> Target)[]
        {
            (ShitroidAiFunction.FlyToSidehopper, 15, () => new(584, 74)),
            (ShitroidAiFunction.ChaseSidehopper, 15, () => victim.Position.Whole + new Offset16<RoomPixels>(0, -32)),
            (ShitroidAiFunction.RiseAfterFeeding, 0, () => new(shitroid.Position.X.Whole, 104)),
            (ShitroidAiFunction.ChaseSamus, 15, () => samus.Position.Whole + new Offset16<RoomPixels>(0, -32)),
            (ShitroidAiFunction.RiseAfterDrainingSamus, 0, () => new(samus.Position.X.Whole, 104)),
            (ShitroidAiFunction.FlyLeft, 0, () => new(CoordinateAxis.Wrap(samus.Position.X.Whole - 64), 100)),
            (ShitroidAiFunction.FlyRight, 0, () => new(CoordinateAxis.Wrap(samus.Position.X.Whole + 96), 104)),
            (ShitroidAiFunction.Exit, 0, () => Position16<RoomPixels>.Wrapped(-128, 64)),
            (ShitroidAiFunction.HoverNearSamus, 10, () => samus.Position.Whole),
            (ShitroidAiFunction.ReleasedFollow, 8, () => samus.Position.Whole - new Offset16<RoomPixels>(0, 18)),
        };
        var velocities = new[]
        {
            new EightEightVector<RoomPixels>(SignedEightEight.FromPixels(2), SignedEightEight.FromPixels(1)),
            new EightEightVector<RoomPixels>(SignedEightEight.FromPixels(-2), SignedEightEight.FromPixels(-1)),
        };
        // From the camera at 512 the Shitroid at 700 is on screen; from the room's corner it is off screen to the right.
        var cameras = new[] { new Position16<RoomPixels>(0x0200, 0x0000), new Position16<RoomPixels>(0x0000, 0x0000) };
        int checks = 0;
        foreach (var flight in flights)
        foreach (EightEightVector<RoomPixels> velocity in velocities)
        foreach (Position16<RoomPixels> camera in cameras)
        {
            state.Function = flight.Function;
            state.Velocity = velocity;
            state.StateTimer = 100;
            state.HoverTimer = 4;
            state.HorizontalLockAccumulator = 0;
            shitroid.Position = shitroid.Position.WithWhole(new(700, 150));
            samus.Position = samus.Position.WithWhole(new(600, 180));
            Position16<RoomPixels> target = flight.Target();
            int divisor = OriginalDivisorTable[flight.TablePosition];
            bool offScreen = camera.X == 0;
            SignedEightEight expectedX = BabyMetroidGradualAcceleration.AccelerateHorizontally(
                700, target.X, velocity.X, divisor, OriginalOffScreenReversalKick, () => offScreen);
            SignedEightEight expectedY = BabyMetroidGradualAcceleration.AccelerateVertically(150, target.Y, velocity.Y, divisor);

            behavior.RunShitroidMain(shitroid, samus, camera, null);

            AssertEqual(expectedX, state.Velocity.X, $"{flight.Function} from {velocity} with the camera at {camera}: X velocity");
            AssertEqual(expectedY, state.Velocity.Y, $"{flight.Function} from {velocity} with the camera at {camera}: Y velocity");
            checks++;
        }
        Console.WriteLine($"  Shitroid flight pulls: {checks} flights steer with the original's divisor and off-screen kick.");
    }

    /// <summary>Runs the focused drain carry reproduction.</summary>
    [VerificationCommand("--shitroid-drain-carry")]
    internal static int ShitroidDrainCarryCommand()
    {
        VerifyShitroidDrainCarry();
        return 0;
    }

    /// <summary>Runs the focused post-feeding acceleration reproduction.</summary>
    [VerificationCommand("--shitroid-gradual-acceleration")]
    internal static int ShitroidGradualAccelerationCommand()
    {
        VerifyShitroidGradualAcceleration();
        return 0;
    }
}
