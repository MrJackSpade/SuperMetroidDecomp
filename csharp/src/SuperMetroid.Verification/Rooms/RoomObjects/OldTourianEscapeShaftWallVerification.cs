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
        WallExplosionList explosionList = VerifyWallExplosionListAgainstRom(bus);
        VerifyWallListAgainstRom(bus);
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.System.SetEvent(StoryEvent.ZebesTimebombSet);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Climb);
        AssertTrue(runtime.RoomObjects.PopulationPlaces.Any(place => place.Behaviour is OldTourianWallObject),
            "the escape state's setup spawns the fake-wall PLM");

        int Explosions() => runtime.Enemies.Projectiles.EnemyProjectiles.Count(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion);
        SamusState samus = runtime.Player.Samus;
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
        RoomLevelData level = runtime.Room.LoadedLevelData;
        for (int column = 0; column < 2; column++)
        for (int row = 0; row < 3; row++)
        {
            RoomTerrainBlock word = level.GetCollisionBlockByIndex(level.GetBlockIndex(new(checked((ushort)(0x10 + column)), checked((ushort)(0x87 + row))))).Terrain;
            AssertEqual(BlankAirBlock.Value, word, $"wall block ({column},{row}) opens to blank air");
        }
        VerifyWallExplosionPlays(runtime, explosion, explosionList);
        Console.WriteLine("Old Tourian escape shaft wall: setup spawns the PLM; Samus past the target blows the wall.");
    }

    /// <summary>
    /// The retail wall-explosion list at $86:B443-$B46F, word by word: it clears the movement, loads the burst count, and
    /// each burst scatters, sounds and shows six poses, each for the duration its word holds with its operand after it,
    /// counts down back to $86:B449 and finally deletes the explosion. Returns what the list shows: each burst's poses with
    /// their durations, and how many bursts it plays, which the explosion the game spawns is observed showing (#2554).
    /// </summary>
    private static WallExplosionList VerifyWallExplosionListAgainstRom(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = rom;
        ushort Word(int address) => cartridge.ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address);
        AssertEqual(OldTourianEscapeShaftWallExplosionInstructionProgramDefinitionsConstants.InitialAnimationScript, (ushort)0xb443, "the setup starts the list");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.ClearFrameBehavior, Word(0xb443), "the list first clears the movement");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.TimerInY, Word(0xb445), "the list then loads the timer");
        int bursts = Word(0xb447);
        AssertEqual(OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.Loop, (ushort)0xb449, "the burst starts at the loop label");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius, Word(0xb449), "the burst first scatters");
        EnemyProjectileScatter scatter = OldTourianEscapeShaftWallExplosionAnimationDefinitions.Scatter;
        AssertEqual(LittleEndianBytes.Word(checked((byte)(scatter.Span.X - 1)), checked((byte)scatter.Center.X)), Word(0xb44b), "the X scatter mask and centre");
        AssertEqual(LittleEndianBytes.Word(checked((byte)(scatter.Span.Y - 1)), checked((byte)scatter.Center.Y)), Word(0xb44d), "the Y scatter mask and centre");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max6, Word(0xb44f), "the burst then sounds");

        var poses = new List<(EnemyProjectileFrameId Frame, int Duration)>();
        int cursor = 0xb452;
        for (int frame = 0; frame < 6; frame++, cursor += 4)
        {
            AssertEqual((ushort)(cursor + 2), OldTourianEscapeShaftWallExplosionInstructionProgramDefinitionsTooling.PresentationWordAddress(frame),
                $"pose {frame} operand follows its duration");
            poses.Add((OldTourianEscapeShaftWallExplosionAnimationDefinitions.Beats[frame].Frame, Word(cursor)));
        }

        AssertEqual((ushort)NativeEnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero, Word(cursor), "the burst then counts down");
        AssertEqual(OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.Loop, Word(cursor + 2), "the repeat returns to the loop label");
        AssertEqual((ushort)NativeEnemyProjectileInstruction.Delete, Word(cursor + 4), "the list ends by deleting the explosion");
        return new WallExplosionList(poses, bursts);
    }

    /// <summary>What the retail wall-explosion list shows: one burst's poses, each with its duration, and how many bursts play.</summary>
    /// <param name="Poses">One burst's poses in order, each with the updates it shows for.</param>
    /// <param name="Bursts">How many times the burst plays before the explosion is deleted.</param>
    private sealed record WallExplosionList(IReadOnlyList<(EnemyProjectileFrameId Frame, int Duration)> Poses, int Bursts);

    /// <summary>
    /// Runs the room on from the update that spawned <paramref name="explosion"/> until it is deleted, sampling the frame it
    /// shows after each update (#2554): it shows <paramref name="expected"/>'s poses in order, each for its duration, as many
    /// bursts as the list counts, and is then deleted, the room never reusing its place meanwhile.
    /// </summary>
    private static void VerifyWallExplosionPlays(SuperMetroidRuntime runtime, EnemyProjectile explosion, WallExplosionList expected)
    {
        var shown = new List<(EnemyProjectileFrameId Frame, int Duration)>();
        int totalUpdates = expected.Bursts * expected.Poses.Sum(pose => pose.Duration);
        void Sample()
        {
            if (explosion.Frame is not { } frame)
                return;
            if (shown.Count > 0 && shown[^1].Frame == frame)
                shown[^1] = (frame, shown[^1].Duration + 1);
            else
                shown.Add((frame, 1));
        }

        Sample();
        for (int update = 0; explosion.IsActive && update <= totalUpdates; update++)
        {
            AssertEqual(RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion, explosion.Kind, "the explosion holds its place while it plays");
            runtime.StepFrame(0);
            if (explosion.IsActive)
                Sample();
        }

        AssertTrue(!explosion.IsActive, "the explosion is deleted after its last burst");
        (EnemyProjectileFrameId Frame, int Duration)[] played = [.. Enumerable.Repeat(expected.Poses, expected.Bursts).SelectMany(poses => poses)];
        AssertEqual(played.Length, shown.Count, "the explosion shows each burst's six poses, burst after burst");
        for (int pose = 0; pose < played.Length; pose++)
        {
            AssertEqual(played[pose].Frame, shown[pose].Frame, $"shown pose {pose} is the list's pose {pose % expected.Poses.Count}");
            AssertEqual(played[pose].Duration, shown[pose].Duration, $"shown pose {pose} shows for the list's duration");
        }
    }

    /// <summary>
    /// The wall's native list does what <see cref="OldTourianWallObject"/> does (#1554): it installs its wait for Samus
    /// and sleeps; past the sleep it clears the wait, draws the open wall for one update and ends.
    /// </summary>
    private static void VerifyWallListAgainstRom(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = rom;
        ushort Word(int address) => cartridge.ReadCartridgeWord(0x840000 | address);
        RoomObjectInstruction InstructionWord(int address) => (RoomObjectInstruction)Word(address);
        ushort entry = Word(NativeWallHeader + 2);
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, InstructionWord(entry), "the wall first installs its wait");
        AssertEqual(OldTourianEscapeShaftWallObjectDefinitionsConstants.WaitForSamusFrameBehavior, Word(entry + 2), "the wall waits for Samus");
        AssertEqual(RoomObjectInstruction.Sleep, InstructionWord(entry + 4), "the wall sleeps while it waits");
        AssertEqual(RoomObjectInstruction.ClearFrameBehavior, InstructionWord(entry + 6), "past its sleep the wall stops waiting");
        AssertEqual(1, (int)Word(entry + 8), "the open wall shows for one update");
        AssertEqual(SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawAddressCodec.Of(OldTourianEscapeShaftWallObjectDefinitions.OpenWallDraw),
            (int)Word(entry + 10), "the open wall's draw");
        AssertEqual(RoomObjectInstruction.Delete, InstructionWord(entry + 12), "the wall ends after it opens");
    }

    /// <summary>
    /// The original game's header of the wall, through which verification reads its native list; the port's room setup
    /// places the wall by what it does and gives it no identity (#2126).
    /// </summary>
    private const ushort NativeWallHeader = 0xb964;

    /// <summary>Runs the focused old Tourian escape shaft wall verification command.</summary>
    [VerificationCommand("--old-tourian-escape-shaft-wall")]
    internal static int OldTourianEscapeShaftWallCommand()
    {
        VerifyOldTourianEscapeShaftWall();
        return 0;
    }
}
