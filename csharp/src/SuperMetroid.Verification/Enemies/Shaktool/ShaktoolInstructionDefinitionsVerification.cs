using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Shaktool.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Shaktool;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Shaktool orientation and collision selectors alongside compiled segment instruction programs.</summary>
internal static class ShaktoolInstructionDefinitionsVerification
{
    /// <summary>Compares orientation and collision lists with the cartridge and checks live Shaktool handoffs.</summary>
    internal static void VerifyShaktoolInstructionDefinitions(CartridgeImportAddressSpace rom)
    {

        // The cartridge keys the head's lists by bucket, the bearing of the direction it aims; each shows the picture the
        // head's orientation chooses (#2284).
        for (ushort bucket = 0; bucket <= 0x00e0; bucket += 0x0020)
        {
            CompassDirection aim = AimAngleConstants.FromClockwiseSteps(bucket).DirectionOfEighth();
            AssertEqual(ShaktoolNativeListAddresses.AimedSpritemap(rom, bucket),
                ShaktoolFrameToolingDefinitions.Spritemap(ShaktoolHeadPictures.Aiming(aim)),
                $"Shaktool orientation picture {bucket:X2}");
        }
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(rom.ReadCartridgeWord((0xaadf13 + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(ShaktoolNativeListAddresses.Entries, ShaktoolSegmentDefinitions.Segments[index].CollisionAnimation),
                $"Shaktool collision list {index}");
        }

        (RoomEnemySystem enemies, ShaktoolChain chain) = CreateCompiledShaktoolGroup();
        RoomEnemyPlace center = enemies.Places.InUpdateOrder[3];
        ShaktoolSegmentState centerState = chain.Segments[3].State;
        ShaktoolSegmentState nextState = chain.Segments[4].State;
        // The centre's main AI orbits it and turns it to face between its neighbours, the chain curling clockwise as it was
        // placed: the centre's orbit never changes how the chain moves.
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        for (ushort bucket = 0; bucket <= 0x00e0; bucket += 0x0020)
        {
            ushort midpoint = unchecked((ushort)(bucket << 8));
            centerState.OrbitAngle = RotationAngleConstants.FromRaw(unchecked((ushort)(midpoint ^ 0x8000)));
            centerState.AngularVelocity = RotationAngle.Zero;
            centerState.ResetOrientation();
            nextState.OrbitAngle = RotationAngleConstants.FromRaw(midpoint);
            center.RunMainAi(samus);
            AssertEqual(AimAngleConstants.FromClockwiseSteps(bucket).DirectionOfEighth(), centerState.Orientation,
                $"Shaktool production orientation bucket {bucket:X2}");
            AssertEqual<EnemyFrameId?>(ShaktoolHeadPictures.Aiming(AimAngleConstants.FromClockwiseSteps(bucket).DirectionOfEighth()), center.Frame,
                $"Shaktool production orientation picture {bucket:X2}");
            AssertTrue(center.AnimationClock.IsStopped, $"the head's animation takes no step while its orientation {bucket:X2} shows its picture");
        }

        RoomEnemyPlace tail = enemies.Places.InUpdateOrder[6];
        DriveTailIntoWall(tail);
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(new AnimationPlayback<ShaktoolSegment>(ShaktoolAnimations.StartingWith(ShaktoolSegmentDefinitions.Segments[index].CollisionAnimation), AnimationPlace.StartOfAnimation),
                chain.Segments[index].State.Playback,
                $"Shaktool production collision list {index}");
        }

        Console.WriteLine(
            "Shaktool instruction definitions: 22 native selectors and all orientation and collision-reversal production handoffs pass with source tables forbidden.");
    }

    /// <summary>Loads the retail cartridge and runs Shaktool instruction program checks.</summary>
    internal static void VerifyShaktoolInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyShaktoolInstructionProgramDefinitions), () => VerifyShaktoolInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Runs every steady and collision program on the real seven-segment group; the program oracle matches bank $AA.</summary>
    internal static void VerifyShaktoolInstructionProgramDefinitions(CartridgeImportAddressSpace rom)
    {
        (RoomEnemySystem enemies, ShaktoolChain chain) = CreateCompiledShaktoolGroup();
        ShaktoolSegment[] steadyPrograms =
        [
            ShaktoolSegment.SawHandPrimaryPiece,
            ShaktoolSegment.ArmPieceNormal,
            ShaktoolSegment.HeadAimingLeft,
            ShaktoolSegment.HeadAimingDown,
        ];
        for (int index = 0; index < steadyPrograms.Length; index++)
        {
            int slotIndex = Math.Min(index, 6);
            chain.Segments[slotIndex].State.Playback =
                new(ShaktoolAnimations.StartingWith(steadyPrograms[index]), AnimationPlace.StartOfAnimation);
            RunForcedShaktoolInstructions(enemies.Places.InUpdateOrder[slotIndex], 5);
        }

        RoomEnemyPlace tail = enemies.Places.InUpdateOrder[6];
        DriveTailIntoWall(tail);
        for (int index = 0; index < 7; index++)
            RunForcedShaktoolInstructions(enemies.Places.InUpdateOrder[index], 8);

        Suite(nameof(ShaktoolAnimationsVerification.VerifyShaktoolAnimations), () => ShaktoolAnimationsVerification.VerifyShaktoolAnimations(rom));
        Console.WriteLine("Shaktool: every steady and collision program runs on the real seven-segment group.");
    }

    /// <summary>
    /// Runs the trailing saw's main AI once through solid terrain, away from where its orbit last put it, so its move meets a
    /// wall and the chain reverses as the saw's collision has it.
    /// </summary>
    internal static void DriveTailIntoWall(RoomEnemyPlace tail)
    {
        tail.Position = tail.Position.WithWhole(new(unchecked((ushort)(tail.Position.X.Whole + 0x40)), tail.Position.Y.Whole));
        tail.RunMainAi(PlacedEnemyAccess.AiFrame(RoomEnemyFrameFixtures.StandaloneSamus(), level: PlacedEnemyAccess.SolidRoom(new(64, 64))));
    }

    /// <summary>Forces successive instruction steps and reports whether they moved the segment.</summary>
    private static bool RunForcedShaktoolInstructions(RoomEnemyPlace segment,
        int steps)
    {
        bool moved = false;
        object?[] arguments =
            [segment, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
        for (int step = 0; step < steps; step++)
        {
            MotherBrainRinkaDoorSpawnExpected before = new MotherBrainRinkaDoorSpawnExpected(segment.Position.X.Whole, segment.Position.Y.Whole);
            segment.AnimationClock.StepOnNextUpdate();
            EnemyInstructionRunner.Process(arguments);
            moved |= segment.Position.X.Whole != before.X || segment.Position.Y.Whole != before.Y;
        }
        return moved;
    }

    /// <summary>
    /// Places all seven Shaktool segments with the selector tables forbidden, each set up in the chain the check holds
    /// (<see cref="ShaktoolChainHolding"/>), so the check steers their orbits while their own entities run their AI.
    /// </summary>
    private static (RoomEnemySystem Enemies, ShaktoolChain Chain) CreateCompiledShaktoolGroup()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        for (int index = 0; index < 7; index++)
        {
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[index];
            place.Bind(EnemyDefinitionId.Shaktool, place.KeptDefinition);
            place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            // The chain is placed curling clockwise, as its population entry names how it starts moving.
            place.SetSpawnSettings(new ShaktoolSpawnSettings(ShaktoolChainMotion.CurlingClockwise, ShaktoolSegmentDefinitions.Segments[index]));
            place.Position = place.Position.WithWhole(new(unchecked((ushort)(0x0100 + index * 16)), 0x0200));
        }
        return (enemies, enemies.HoldShaktoolChain());
    }

    /// <summary>Runs the Shaktool orientation and collision animation-selection check.</summary>
    [VerificationCommand("--shaktool-instruction-definitions")]
    internal static int ShaktoolInstructionDefinitionsCommand()
    {
        Suite(nameof(VerifyShaktoolInstructionDefinitions), () => VerifyShaktoolInstructionDefinitions(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the standalone Shaktool instruction mechanics verification command.</summary>
    [VerificationCommand("--shaktool-instruction-mechanics")]
    internal static int ShaktoolInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyShaktoolInstructionProgramDefinitions), () => VerifyShaktoolInstructionProgramDefinitions());
        return 0;
    }
}
