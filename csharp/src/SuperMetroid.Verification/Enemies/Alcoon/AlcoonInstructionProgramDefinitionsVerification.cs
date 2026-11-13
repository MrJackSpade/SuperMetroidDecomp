using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Enemies.Alcoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;

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
        SuperMetroidAddressSpace rom, ImportEnemyArtwork? installedArt = null)
    {
        SelectedRoom room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(rom, AlcoonInstructionAuditRoom);
        var installation = RepositoryInstallation.Installation;
        LoadedRoomAssets assets = LoadFixtureRoomAssets(room, installation.LoadRoomVisualLayouts());
        var vram = new SnesVram();
        var samusColours = new SamusColours();
        assets.LoadGraphics(BgArtworkFixtures.For(vram));
        var random = new SharedGameState(new SharedRandom());
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices((installedArt ?? InstalledFixtures.EnemyArtwork).Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(random.NextRandom) { ReadRandom = () => random.RandomNumber, SeedRandom = random.SetRandomNumber }) { BossArtwork = (installedArt ?? InstalledFixtures.EnemyArtwork).Bosses };
        enemies.Load(new RoomEnemyLoadRequest(room.State.EnemyPopulation, BgArtworkFixtures.For(vram)) { Level = assets.LevelData });

        RoomEnemySlot actor = enemies.SlotTable.Slots[0];
        AlcoonEnemyState state = enemies.AlcoonStates[0] ??
            throw new InvalidDataException("Real Alcoon initializer omitted slot-zero state.");
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(actor.Position.X.Whole + 1)), state.LandingPosition.Y)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);

        bool sawLeftFireball = false;
        bool sawRightFireball = false;
        var reachedPoses = new HashSet<ReachedPose>();
        for (int frame = 0; frame < 4_096 &&
             (installedArt is not null || reachedPoses.Count < 42 ||
              !sawLeftFireball || !sawRightFireball); frame++)
        {
            int maximumX = Math.Max(0, room.Size.X * 256 - 256);
            ushort cameraX = unchecked((ushort)Math.Clamp(actor.Position.X.Whole - 128, 0, maximumX));
            enemies.StepStandaloneFrame(enemies.StandaloneFrame(assets.LevelData) with { Camera = new(cameraX, 0), TimeIsFrozen = false, Samus = samus });
            foreach (RoomEnemySlot slot in enemies.SlotTable.Slots)
            {
                // A playback that has just passed a frame beat is showing that pose; one resting on a held frame stands past
                // its phase's last beat, the held frame.
                if (slot.DefinitionId == EnemyDefinitionId.Alcoon &&
                    enemies.AlcoonStates[slot.SlotIndex] is { Playback: { Place.Beat: > 0 } playback } &&
                    playback.Animation.Phases[playback.Place.Phase] is var phase &&
                    phase.FirstBeat + Math.Min(playback.Place.Beat, phase.BeatCount) - 1 is var shownBeat &&
                    AlcoonAnimations.Segments.Beats(phase.Segment)[shownBeat].ShownFrame is not null)
                    reachedPoses.Add(new ReachedPose(phase.Segment, shownBeat));
            }

            samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(actor.Position.X.Whole +
                (state.Velocity.X.WholePixels < 0 ? -32 : 32))), state.LandingPosition.Y));
            foreach (EnemyProjectile projectile in enemies.Projectiles.EnemyProjectiles.Where(
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
            .SelectMany(program => AlcoonAnimations.Segments.Beats(program)
                .Select((beat, index) => new AlcoonInstructionProgramDefinitionsPose(Program:program, Step:index, Shows:beat.ShownFrame is not null)))
            .Where(step => step.Shows)
            .Select(step => new ReachedPose(step.Program, step.Step))
            .ToHashSet();
        string missed = string.Join(", ", poses.Where(pose => !reachedPoses.Contains(pose)).Select(pose => $"{pose.Program}[{pose.Step}]"));
        AssertEqual(44, poses.Count, "the Alcoon's programs author 44 poses");
        AssertEqual(42, reachedPoses.Count, $"real Alcoon reaches every reachable pose; missing={missed}");
        AssertTrue(poses.Except(reachedPoses).All(pose =>
                pose.Program is AlcoonProgram.FireLeft or AlcoonProgram.FireRight &&
                AlcoonAnimations.Segments.Beats(pose.Program)[pose.Step - 1] is AnimationBeat<AlcoonFrame, AlcoonCue>.Event cue &&
                cue.Raised.Kind == AlcoonCueKind.StartWalking),
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
