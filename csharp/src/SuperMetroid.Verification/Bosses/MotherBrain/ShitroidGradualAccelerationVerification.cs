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
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.BigBoy);
        RoomEnemySlot shitroid = runtime.Enemies.SlotTable.Slots.First(slot =>
            slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid);

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
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.BigBoy);
        RoomEnemySlot shitroid = runtime.Enemies.SlotTable.Slots.First(slot =>
            slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid);
        var samus = runtime.Player.IntroducedSamus;
        ShitroidEnemyState state = shitroid.SpeciesState<ShitroidEnemyState>()
            ?? throw new InvalidOperationException("The Big Boy room's Shitroid has no state.");
        foreach ((ushort ySpeed, ushort expectedX, ushort expectedSpeed) in new VerifyShitroidDrainCarryCase[]
            { new VerifyShitroidDrainCarryCase(5, 0x01c4, 2), new VerifyShitroidDrainCarryCase(1, 0x01c3, 1) })
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
