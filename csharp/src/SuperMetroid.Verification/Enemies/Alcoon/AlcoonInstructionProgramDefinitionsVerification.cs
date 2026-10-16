using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Alcoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Alcoon's typed animations through a retail room's movement and fire cues.</summary>
internal static class AlcoonInstructionProgramDefinitionsVerification
{
    /// <summary>Retail room containing the Alcoon population used by the instruction audit.</summary>
    private const ushort AlcoonInstructionAuditRoom = 0x93aa;

    /// <summary>Loads the retail cartridge and runs the Alcoon instruction suite.</summary>
    internal static void VerifyAlcoonInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyAlcoonInstructionProgramDefinitions), () => VerifyAlcoonInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Runs a real Alcoon room until reachable frames and both fire directions execute without source reads.</summary>
    internal static void VerifyAlcoonInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog? installedArt = null)
    {
        CartridgeRoomHeader room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(rom, AlcoonInstructionAuditRoom);
        var installation = RepositoryInstallation.Installation;
        CartridgeRoomAssets assets = LoadFixtureRoomAssets(room,
            installation.LoadRoomCharacters(), installation.LoadRoomPalettes(),
            installation.LoadRoomMetatiles(), installation.LoadRoomVisualLayouts());
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem { TileArtwork = installedArt ?? InstalledFixtures.EnemyTiles };
        enemies.Load(new RoomEnemyLoadRequest(rom, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer, vram, cgram, new StandaloneRoomEnemyServices(random.NextRandom) { ReadRandom = () => random.RandomNumber, SeedRandom = random.SetRandomNumber }) { Level = assets.LevelData });

        RoomEnemySlot actor = enemies.SlotTable.Slots[0];
        AlcoonEnemyState state = enemies.AlcoonStates[0] ??
            throw new InvalidDataException("Real Alcoon initializer omitted slot-zero state.");
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(actor.Position.X.Whole + 1)), state.LandingYPosition)),
        };
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation(rom);

        bool sawLeftFireball = false;
        bool sawRightFireball = false;
        var reachedPoses = new HashSet<(AlcoonProgram Program, int Step)>();
        for (int frame = 0; frame < 4_096 &&
             (installedArt is not null || reachedPoses.Count < 42 ||
              !sawLeftFireball || !sawRightFireball); frame++)
        {
            int maximumX = Math.Max(0, room.Size.X * 256 - 256);
            ushort cameraX = unchecked((ushort)Math.Clamp(actor.Position.X.Whole - 128, 0, maximumX));
            enemies.StepFrame(new RoomEnemyFrame { Camera = new(cameraX, 0), TimeIsFrozen = false, Samus = samus, Level = assets.LevelData });
            foreach (RoomEnemySlot slot in enemies.SlotTable.Slots)
            {
                // A cursor that has just passed a pose step is showing that pose.
                if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Alcoon &&
                    enemies.AlcoonStates[slot.SlotIndex] is { Program: { Position: > 0 } cursor } &&
                    AlcoonAnimations.Programs.Steps(cursor.Program)[cursor.Position - 1].Kind == AnimationProgramStepKind.Show)
                    reachedPoses.Add((cursor.Program, cursor.Position - 1));
            }

            samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(actor.Position.X.Whole +
                (state.Velocity.X.WholePixels < 0 ? -32 : 32))), state.LandingYPosition));
            foreach (RoomEnemyProjectileSlot projectile in enemies.Projectiles.EnemyProjectiles.Where(
                         projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball))
            {
                sawLeftFireball |= unchecked((short)projectile.Velocity.X.RawValue) < 0;
                sawRightFireball |= unchecked((short)projectile.Velocity.X.RawValue) > 0;
            }
            enemies.StepEnemyProjectiles(assets.LevelData, samus: null, camera: Position16<RoomPixels>.Zero);
        }

        AssertTrue(sawLeftFireball && sawRightFireball,
            "real Alcoon programs execute fire callbacks in both facings");
        var poses = Enum.GetValues<AlcoonProgram>()
            .SelectMany(program => AlcoonAnimations.Programs.Steps(program)
                .Select((step, index) => (Program: program, Step: index, step.Kind)))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => (step.Program, step.Step))
            .ToHashSet();
        string missed = string.Join(", ", poses.Where(pose => !reachedPoses.Contains(pose)).Select(pose => $"{pose.Program}[{pose.Step}]"));
        AssertEqual(44, poses.Count, "the Alcoon's programs author 44 poses");
        AssertEqual(42, reachedPoses.Count, $"real Alcoon reaches every reachable pose; missing={missed}");
        AssertTrue(poses.Except(reachedPoses).All(pose =>
                pose.Program is AlcoonProgram.FireLeft or AlcoonProgram.FireRight &&
                AlcoonAnimations.Programs.Steps(pose.Program)[pose.Step - 1] is { Kind: AnimationProgramStepKind.Cue } cue &&
                cue.PublishedCue.Kind == AlcoonCueKind.StartWalking),
            "native start-walking cues skip the two trailing unreachable poses");
        Suite(nameof(AlcoonAnimationsVerification.VerifyAlcoonAnimations), () => AlcoonAnimationsVerification.VerifyAlcoonAnimations(rom));

        Console.WriteLine(installedArt is null
            ? "Alcoon: a real room reaches all 42 reachable poses of the ten programs, fires in both facings, and hands " +
              "off to walking as the native callbacks do."
            : "Installed Alcoon: real-room movement and both fire directions pass with the installed artwork.");
    }

    /// <summary>Runs the standalone Alcoon instruction mechanics verification command.</summary>
    [VerificationCommand("--alcoon-instruction-mechanics")]
    internal static int AlcoonInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyAlcoonInstructionProgramDefinitions), () => VerifyAlcoonInstructionProgramDefinitions());
        return 0;
    }
}
