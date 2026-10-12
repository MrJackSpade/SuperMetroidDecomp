using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static RetailRuntimeFixtureVerification;

internal static class MotherBrainSmallPurpleBreathVerification
{
    // #1269: DrawMotherBrainHead ($A9:9357) spawns the small purple breath ($86:CB3D) whenever
    // generation is enabled, none is active and the sampled RNG has bit 15 clear. Its
    // initializer marks it active; its last instruction ($86:CAEE) clears the flag before it
    // deletes itself. The port never spawned it, so in the 100% movie a projectile slot native
    // filled stayed empty.
    internal static void VerifyMotherBrainSmallPurpleBreath()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        RoomEnemySlot head = state.Head ?? throw new InvalidOperationException("Mother Brain has no head enemy.");
        state.DrawBrain = true;
        state.SmallPurpleBreathGenerationEnabled = true;
        head.Position = head.Position.WithWhole(new(0x007c, 0x0058));
        int Breaths() => runtime.Enemies.Projectiles.EnemyProjectiles.Count(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall);
        void Draw() => runtime.Enemies.Drawing.DrawLayers(new OamBuffer(), new(0, 0), 5, 5);

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
            runtime.Enemies.StepEnemyProjectileInstructions(runtime.Room.LevelData!, runtime.Player.Samus,
                new(runtime.Room.Camera!.PixelPosition.X, runtime.Room.Camera.PixelPosition.Y));
        AssertEqual(0, Breaths(), "the small breath deletes itself after its eight poses");
        AssertTrue(!state.SmallPurpleBreathActive, "its last instruction clears the active flag");

        // The brain-list opcode $A9:9F8E also clears generation. Publishing the rainbow body's
        // state must not restore the flag unless the body itself wrote it (#1269).
        state.SmallPurpleBreathGenerationEnabled = false;
        runtime.Enemies.MotherBrainBehavior.StartLiveMotherBrainRainbowBeam(state, runtime.Player.Samus!, null);
        AssertTrue(!state.SmallPurpleBreathGenerationEnabled,
            "an unwritten rainbow-body flag leaves the brain-list clear in place");
        Console.WriteLine("Mother Brain small purple breath: RNG-gated spawn, one at a time, flag cleared on its last instruction.");
    }

    [VerificationCommand("--mother-brain-small-purple-breath")]
    internal static int MotherBrainSmallPurpleBreathCommand()
    {
        VerifyMotherBrainSmallPurpleBreath();
        return 0;
    }
}
