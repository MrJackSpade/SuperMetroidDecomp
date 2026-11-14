using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.OldTourianWall;
using SuperMetroid.Core.Areas.Tourian;

/// <summary>Checks the old Tourian escape shaft wall PLM's trigger, explosion, and cleared blocks.</summary>
internal static class OldTourianEscapeShaftWallVerification
{
    /// <summary>Confirms escape setup $8F:91A9 spawns the wall PLM, which opens two columns after Samus crosses its strict position gate.</summary>
    /// <remarks>#1269: pre-instruction $84:B927 spawns explosion $86:B4B1 at ($110,$888). The missing setup had omitted that explosion in the 100% movie.</remarks>
    internal static void VerifyOldTourianEscapeShaftWall()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        VerifyWallExplosionProgramAgainstRom(bus);
        VerifyWallListAgainstRom(bus);
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(StoryEvent.ZebesTimebombSet);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Climb);
        AssertTrue(runtime.RoomObjects.PopulationSlots.Any(slot => slot.Behaviour is OldTourianWallObject),
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
        EnemyProjectile explosion = runtime.Enemies.Projectiles.EnemyProjectiles.First(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion);
        AssertEqual(0x0110, explosion.Position.X.Whole, "the explosion is centred on the wall's X");
        AssertEqual(0x0888, explosion.Position.Y.Whole, "the explosion is centred on the wall's Y");
        AssertTrue(explosion.Animation is OldTourianWallExplosionAnimation, "the explosion plays its typed program");
        AssertTrue(explosion.Animation is not null, "the explosion runs no native list");
        RoomLevelData level = runtime.Room.LoadedLevelData;
        for (int column = 0; column < 2; column++)
        for (int row = 0; row < 3; row++)
        {
            RoomTerrainBlock word = level.GetCollisionBlockByIndex(level.GetBlockIndex(new(checked((ushort)(0x10 + column)), checked((ushort)(0x87 + row))))).Terrain;
            AssertEqual(BlankAirBlock.Value, word, $"wall block ({column},{row}) opens to blank air");
        }
        Console.WriteLine("Old Tourian escape shaft wall: setup spawns the PLM; Samus past the target blows the wall.");
    }

    /// <summary>
    /// The typed wall-explosion segments and animation against the retail list at $86:B443-$B46F, word by word: the
    /// cleared movement, the burst count, the scatter masks, the sound, each pose's duration and operand position, the
    /// counted repeat back to $86:B449 and the delete, after which the explosion holds.
    /// </summary>
    private static void VerifyWallExplosionProgramAgainstRom(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address);
        var segments = OldTourianWallExplosionAnimation.AllSegments;
        Animation<OldTourianWallExplosionSegment> animation = OldTourianWallExplosionAnimation.AllAnimations[OldTourianWallExplosionSegment.Start];
        AssertEqual(3, animation.Phases.Count, "the animation plays the setup, the counted burst and the delete");

        var start = segments.Beats(OldTourianWallExplosionSegment.Start);
        AssertEqual(1, start.Count, "the setup has one beat");
        AssertEqual(new AnimationPhase<OldTourianWallExplosionSegment>(OldTourianWallExplosionSegment.Start, 0, 1, 1), animation.Phases[0],
            "the animation first plays the setup");
        AssertEqual(OldTourianEscapeShaftWallExplosionInstructionProgramDefinitionsConstants.InitialAnimationScript, (ushort)0xb443, "the setup starts the list");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.ClearFrameBehavior, Word(0xb443), "the list first clears the movement");
        AssertEqual(OldTourianWallExplosionCue.ClearFrameBehavior, AssertIs<AnimationBeat<EnemyProjectileFrameId, OldTourianWallExplosionCue>.Event>(start[0], "the setup raises an event").Raised, "the setup clears the movement");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.TimerInY, Word(0xb445), "the list then loads the timer");
        AnimationPhase<OldTourianWallExplosionSegment> counted = animation.Phases[1];
        AssertTrue(counted.CountsRepeats, "the burst counts its plays down the timer the list loads");
        AssertEqual(Word(0xb447), counted.Plays, "the burst count");
        AssertEqual(OldTourianWallExplosionSegment.Burst, counted.Segment, "the setup runs on into the burst");
        AssertEqual(0, counted.FirstBeat, "the counted burst plays from the burst's start");

        var burst = segments.Beats(OldTourianWallExplosionSegment.Burst);
        AssertEqual(OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.Loop, (ushort)0xb449, "the burst starts at the loop label");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius, Word(0xb449), "the burst first scatters");
        AssertEqual(OldTourianWallExplosionCue.Scatter, AssertIs<AnimationBeat<EnemyProjectileFrameId, OldTourianWallExplosionCue>.Event>(burst[0], "the burst raises an event").Raised, "the burst scatters");
        EnemyProjectileScatter scatter = OldTourianEscapeShaftWallExplosionAnimationDefinitions.Scatter;
        AssertEqual(LittleEndianBytes.Word(checked((byte)((scatter.XRange.Outcomes - 1) << scatter.XRange.Shift)), checked((byte)scatter.Center.X)), Word(0xb44b), "the X scatter mask and centre");
        AssertEqual(LittleEndianBytes.Word(checked((byte)((scatter.YRange.Outcomes - 1) << scatter.YRange.Shift)), checked((byte)scatter.Center.Y)), Word(0xb44d), "the Y scatter mask and centre");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max6, Word(0xb44f), "the burst then sounds");
        AssertEqual(OldTourianWallExplosionCue.ExplosionSound, AssertIs<AnimationBeat<EnemyProjectileFrameId, OldTourianWallExplosionCue>.Event>(burst[1], "the burst raises an event").Raised, "the burst sounds");

        int cursor = 0xb452;
        for (int frame = 0; frame < 6; frame++, cursor += 4)
        {
            var show = AssertIs<AnimationBeat<EnemyProjectileFrameId, OldTourianWallExplosionCue>.Frame>(burst[2 + frame], $"pose {frame} is shown");
            AssertEqual(OldTourianEscapeShaftWallExplosionAnimationDefinitions.Frames[frame], show.Shown, $"pose {frame} order");
            AssertEqual(Word(cursor), show.Duration, $"pose {frame} duration");
            AssertEqual((ushort)(cursor + 2), OldTourianEscapeShaftWallExplosionInstructionProgramDefinitionsTooling.PresentationWordAddress(frame),
                $"pose {frame} operand follows its duration");
        }

        AssertEqual((ushort)NativeEnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero, Word(cursor), "the burst then counts down");
        AssertEqual(OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.Loop, Word(cursor + 2), "the repeat returns to the loop label");
        AssertEqual(8, counted.BeatCount, "the counted burst ends at the count-down, after its six poses");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.Delete, Word(cursor + 4), "the list ends by deleting the explosion");
        AssertEqual(OldTourianWallExplosionCue.Delete, AssertIs<AnimationBeat<EnemyProjectileFrameId, OldTourianWallExplosionCue>.Event>(burst[8], "the burst raises an event").Raised, "the burst deletes the explosion");
        AssertEqual(9, burst.Count, "the burst has nine beats");
        AssertEqual(new AnimationPhase<OldTourianWallExplosionSegment>(OldTourianWallExplosionSegment.Burst, 8, 1, 1), animation.Phases[2],
            "after its last burst the animation plays the delete");
        AssertEqual(AnimationEnd.Holds, animation.End, "the deleted explosion holds");
    }

    /// <summary>
    /// The wall's native list does what <see cref="OldTourianWallObject"/> does (#1554): it installs its wait for Samus
    /// and sleeps; past the sleep it clears the wait, draws the open wall for one update and ends.
    /// </summary>
    private static void VerifyWallListAgainstRom(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(0x840000 | address);
        RoomObjectInstruction InstructionWord(int address) => (RoomObjectInstruction)Word(address);
        ushort entry = Word(RoomObjectNativeWords.Word(RoomObjectKind.OldTourianEscapeShaftFakeWall) + 2);
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, InstructionWord(entry), "the wall first installs its wait");
        AssertEqual(OldTourianEscapeShaftWallObjectDefinitionsConstants.WaitForSamusFrameBehavior, Word(entry + 2), "the wall waits for Samus");
        AssertEqual(RoomObjectInstruction.Sleep, InstructionWord(entry + 4), "the wall sleeps while it waits");
        AssertEqual(RoomObjectInstruction.ClearFrameBehavior, InstructionWord(entry + 6), "past its sleep the wall stops waiting");
        AssertEqual(1, (int)Word(entry + 8), "the open wall shows for one update");
        AssertEqual(SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawAddressCodec.Of(OldTourianEscapeShaftWallObjectDefinitions.OpenWallDraw),
            (int)Word(entry + 10), "the open wall's draw");
        AssertEqual(RoomObjectInstruction.Delete, InstructionWord(entry + 12), "the wall ends after it opens");
    }

    /// <summary>Runs the focused old Tourian escape shaft wall verification command.</summary>
    [VerificationCommand("--old-tourian-escape-shaft-wall")]
    internal static int OldTourianEscapeShaftWallCommand()
    {
        VerifyOldTourianEscapeShaftWall();
        return 0;
    }
}
