using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks the old Tourian escape shaft wall PLM's trigger, explosion, and cleared blocks.</summary>
internal static class OldTourianEscapeShaftWallVerification
{
    /// <summary>Confirms escape setup $8F:91A9 spawns the wall PLM, which opens two columns after Samus crosses its strict position gate.</summary>
    /// <remarks>#1269: pre-instruction $84:B927 spawns explosion $86:B4B1 at ($110,$888). The missing setup had omitted that explosion in the 100% movie.</remarks>
    internal static void VerifyOldTourianEscapeShaftWall()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        VerifyWallExplosionProgramAgainstRom(bus);
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Climb);
        AssertTrue(runtime.Plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.OldTourianEscapeShaftFakeWall),
            "the escape state's setup spawns the fake-wall PLM");

        int Explosions() => runtime.Enemies.Projectiles.EnemyProjectiles.Count(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Position = samus.Position.WithWhole(new(0x00e8, 0x088b));
        runtime.StepFrame(0);
        AssertEqual(0, Explosions(), "Samus left of X $F0 leaves the wall asleep");

        samus.Position = samus.Position.WithWhole(new(0x00f1, 0x088b));
        runtime.StepFrame(0);
        AssertEqual(1, Explosions(), "Samus below and right of ($F0,$820) blows the wall");
        RoomEnemyProjectileSlot explosion = runtime.Enemies.Projectiles.EnemyProjectiles.First(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion);
        AssertEqual(0x0110, explosion.Variable0, "the explosion is centred on the wall's X");
        AssertEqual(0x0888, explosion.Variable1, "the explosion is centred on the wall's Y");
        AssertTrue(explosion.Animation is OldTourianWallExplosionAnimation, "the explosion plays its typed program");
        AssertTrue(explosion.Animation is not null, "the explosion runs no native list");
        RoomLevelData level = runtime.Room.LoadedLevelData;
        for (int column = 0; column < 2; column++)
        for (int row = 0; row < 3; row++)
        {
            ushort word = level.GetCollisionBlockByIndex(level.GetBlockIndex(new(checked((ushort)(0x10 + column)), checked((ushort)(0x87 + row))))).PackedWord.Raw;
            AssertEqual((ushort)0x00ff, word, $"wall block ({column},{row}) opens to $00FF");
        }
        Console.WriteLine("Old Tourian escape shaft wall: setup spawns the PLM; Samus past the target blows the wall.");
    }

    /// <summary>
    /// The typed wall-explosion programs against the retail list at $86:B443-$B46F, word by word: the cleared
    /// movement, the burst count, the scatter masks, the sound, each pose's duration and operand position, the
    /// counted repeat back to $86:B449 and the delete.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the old Tourian wall explosion list")]
    private static void VerifyWallExplosionProgramAgainstRom(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address);
        var programs = OldTourianWallExplosionAnimation.AllPrograms;

        var start = programs.Steps(OldTourianWallExplosionProgram.Start);
        AssertEqual(3, start.Count, "the setup has three steps");
        AssertEqual(OldTourianEscapeShaftWallExplosionInstructionProgramDefinitionsConstants.InitialInstructionList, (ushort)0xb443, "the setup starts the list");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.ClearPreInstruction, Word(0xb443), "the list first clears the movement");
        AssertEqual(OldTourianWallExplosionCue.ClearPreInstruction, AssertIs<AnimationProgramStep<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>.CueStep>(start[0], "the setup publishes a cue").Cue, "the setup clears the movement");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.TimerInY, Word(0xb445), "the list then loads the timer");
        var setCounter = AssertIs<AnimationProgramStep<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>.SetCounterStep>(start[1], "the setup loads the counter");
        AssertEqual(Word(0xb447), setCounter.Count, "the burst count");
        var fallInto = AssertIs<AnimationProgramStep<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>.FallIntoStep>(start[2], "the setup runs on into the burst");
        AssertEqual(OldTourianWallExplosionProgram.Burst, fallInto.Target, "the setup's next program");

        var burst = programs.Steps(OldTourianWallExplosionProgram.Burst);
        AssertEqual(OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.Loop, (ushort)0xb449, "the burst starts at the loop label");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius, Word(0xb449), "the burst first scatters");
        AssertEqual(OldTourianWallExplosionCue.Scatter, AssertIs<AnimationProgramStep<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>.CueStep>(burst[0], "the burst publishes a cue").Cue, "the burst scatters");
        EnemyProjectileScatter scatter = OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.Scatter;
        AssertEqual(SnesLittleEndian.Word(checked((byte)scatter.Range.X), checked((byte)scatter.Center.X)), Word(0xb44b), "the X scatter mask and centre");
        AssertEqual(SnesLittleEndian.Word(checked((byte)scatter.Range.Y), checked((byte)scatter.Center.Y)), Word(0xb44d), "the Y scatter mask and centre");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max6, Word(0xb44f), "the burst then sounds");
        AssertEqual(OldTourianWallExplosionCue.ExplosionSound, AssertIs<AnimationProgramStep<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>.CueStep>(burst[1], "the burst publishes a cue").Cue, "the burst sounds");

        int cursor = 0xb452;
        for (int frame = 0; frame < 6; frame++, cursor += 4)
        {
            var step = burst[2 + frame];
            var show = AssertIs<AnimationProgramStep<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>.ShowStep>(step, $"pose {frame} is shown");
            AssertEqual(frame, show.Frame, $"pose {frame} order");
            AssertEqual(Word(cursor), show.Duration, $"pose {frame} duration");
            AssertEqual((ushort)(cursor + 2), OldTourianEscapeShaftWallExplosionInstructionProgramDefinitionsTooling.PresentationWordAddress(frame),
                $"pose {frame} operand follows its duration");
        }

        AssertEqual((ushort)NativeEnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero, Word(cursor), "the burst then counts down");
        var repeat = AssertIs<AnimationProgramStep<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>.RepeatWhileCountingStep>(burst[8], "the burst repeats while counting");
        AssertEqual(OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.Loop, Word(cursor + 2), "the repeat returns to the loop label");
        AssertEqual(OldTourianWallExplosionProgram.Burst, repeat.Target, "the repeat returns to the burst");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.Delete, Word(cursor + 4), "the list ends by deleting the explosion");
        AssertEqual(OldTourianWallExplosionCue.Delete, AssertIs<AnimationProgramStep<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>.CueStep>(burst[9], "the burst publishes a cue").Cue, "the burst deletes the explosion");
        AssertTrue(burst[10] is AnimationProgramStep<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>.HoldStep, "the deleted explosion holds");
        AssertEqual(11, burst.Count, "the burst has eleven steps");
    }

    /// <summary>Runs the focused old Tourian escape shaft wall verification command.</summary>
    [VerificationCommand("--old-tourian-escape-shaft-wall")]
    internal static int OldTourianEscapeShaftWallCommand()
    {
        VerifyOldTourianEscapeShaftWall();
        return 0;
    }
}
