using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks Mother Brain's RNG-gated small breath spawn and active-projectile latch.</summary>
internal static class MotherBrainSmallPurpleBreathVerification
{
    /// <summary>
    /// Confirms the head draw hook spawns one breath when RNG bit 15 is clear, then its
    /// final instruction releases the active latch before deletion.
    /// </summary>
    internal static void VerifyMotherBrainSmallPurpleBreath()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        RoomEnemySlot head = state.Head;
        state.DrawBrain = true;
        state.SmallPurpleBreathGenerationEnabled = true;
        head.Position = head.Position.WithWhole(new(0x007c, 0x0058));
        int Breaths() => runtime.Enemies.Projectiles.EnemyProjectiles.Count(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall);
        void Draw() => runtime.Enemies.Drawing.DrawLayersAsOam(new OamBuffer(), new(0, 0), 5, 5);

        runtime.System.SetRandomNumber(0x8000);
        Draw();
        AssertEqual(0, Breaths(), "an RNG with bit 15 set spawns no breath");

        runtime.System.SetRandomNumber(0x7fff);
        Draw();
        AssertEqual(1, Breaths(), "an RNG with bit 15 clear spawns the small breath");
        AssertTrue(state.SmallPurpleBreathActive, "the initializer marks the small breath active");
        RoomEnemyProjectileSlot breath = runtime.Enemies.Projectiles.EnemyProjectiles.First(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall);
        AssertEqual((ushort)0x0082, breath.Position.X.Whole, "the breath starts six pixels right of the head");
        AssertEqual((ushort)0x0068, breath.Position.Y.Whole, "the breath starts sixteen pixels below the head");
        AssertEqual((ushort)0x7fff, runtime.System.RandomNumber, "the spawn only samples the RNG");

        Draw();
        AssertEqual(1, Breaths(), "an active small breath blocks another");

        // 8+8+10+10+11+11+12+12 frames, then $86:CAEE clears the flag and Delete removes it.
        for (int frame = 0; frame < 120 && Breaths() != 0; frame++)
            runtime.Enemies.StepEnemyProjectileInstructions(runtime.Room.LoadedLevelData, runtime.Player.Samus,
                new(runtime.Room.LoadedCamera.PixelPosition.X, runtime.Room.LoadedCamera.PixelPosition.Y));
        AssertEqual(0, Breaths(), "the small breath deletes itself after its eight poses");
        AssertTrue(!state.SmallPurpleBreathActive, "its last instruction clears the active flag");

        // The brain-list opcode $A9:9F8E also clears generation. Publishing the rainbow body's
        // state must not restore the flag unless the body itself wrote it (#1269).
        state.SmallPurpleBreathGenerationEnabled = false;
        runtime.Enemies.MotherBrainBehavior.StartLiveMotherBrainRainbowBeam(state, runtime.Player.IntroducedSamus, null);
        AssertTrue(!state.SmallPurpleBreathGenerationEnabled,
            "an unwritten rainbow-body flag leaves the brain-list clear in place");
        Console.WriteLine("Mother Brain small purple breath: RNG-gated spawn, one at a time, flag cleared on its last instruction.");
    }

    /// <summary>Runs the focused small purple breath lifecycle check.</summary>
    [VerificationCommand("--mother-brain-small-purple-breath")]
    internal static int MotherBrainSmallPurpleBreathCommand()
    {
        VerifyMotherBrainSmallPurpleBreath();
        return 0;
    }
}
