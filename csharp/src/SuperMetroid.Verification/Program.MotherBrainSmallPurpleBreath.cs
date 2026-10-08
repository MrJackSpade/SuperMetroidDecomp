using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    // #1269: DrawMotherBrainHead ($A9:9357) spawns the small purple breath ($86:CB3D) whenever
    // generation is enabled, none is active and the sampled RNG has bit 15 clear. Its
    // initializer marks it active; its last instruction ($86:CAEE) clears the flag before it
    // deletes itself. The port never spawned it, so in the 100% movie a projectile slot native
    // filled stayed empty.
    private static void VerifyMotherBrainSmallPurpleBreath()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        RoomEnemySlot head = state.Head ?? throw new InvalidOperationException("Mother Brain has no head enemy.");
        state.DrawBrain = true;
        state.SmallPurpleBreathGenerationEnabled = true;
        head.XPosition = 0x007c;
        head.YPosition = 0x0058;
        int Breaths() => runtime.Enemies.EnemyProjectiles.Count(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall);
        void Draw() => runtime.Enemies.DrawLayers(new OamBuffer(), 0, 0, 5, 5);

        runtime.System.SetRandomNumber(0x8000);
        Draw();
        AssertEqual(0, Breaths(), "an RNG with bit 15 set spawns no breath");

        runtime.System.SetRandomNumber(0x7fff);
        Draw();
        AssertEqual(1, Breaths(), "an RNG with bit 15 clear spawns the small breath");
        AssertTrue(state.SmallPurpleBreathActive, "the initializer marks the small breath active");
        RoomEnemyProjectileSlot breath = runtime.Enemies.EnemyProjectiles.First(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall);
        AssertEqual((ushort)0x0082, breath.XPosition, "the breath starts six pixels right of the head");
        AssertEqual((ushort)0x0068, breath.YPosition, "the breath starts sixteen pixels below the head");
        AssertEqual((ushort)0x7fff, runtime.System.RandomNumber, "the spawn only samples the RNG");

        Draw();
        AssertEqual(1, Breaths(), "an active small breath blocks another");

        // 8+8+10+10+11+11+12+12 frames, then $86:CAEE clears the flag and Delete removes it.
        for (int frame = 0; frame < 120 && Breaths() != 0; frame++)
            runtime.Enemies.StepEnemyProjectileInstructions(runtime.LevelData!, runtime.Samus,
                runtime.Camera!.XPosition, runtime.Camera.YPosition);
        AssertEqual(0, Breaths(), "the small breath deletes itself after its eight poses");
        AssertTrue(!state.SmallPurpleBreathActive, "its last instruction clears the active flag");

        // The brain-list opcode $A9:9F8E also clears generation. Publishing the rainbow body's
        // state must not restore the flag unless the body itself wrote it (#1269).
        state.SmallPurpleBreathGenerationEnabled = false;
        typeof(RoomEnemySystem).GetMethod("StartLiveMotherBrainRainbowBeam",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(runtime.Enemies, [state, runtime.Samus!, null]);
        AssertTrue(!state.SmallPurpleBreathGenerationEnabled,
            "an unwritten rainbow-body flag leaves the brain-list clear in place");
        Console.WriteLine("Mother Brain small purple breath: RNG-gated spawn, one at a time, flag cleared on its last instruction.");
    }
}
