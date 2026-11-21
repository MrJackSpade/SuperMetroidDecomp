using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Samus.Drawing;
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
using SuperMetroid.Core.Enemies.Artwork;
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
        CartridgeImportAddressSpace rom, ImportEnemyArtwork? installedArt = null)
    {
        SelectedRoom room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(rom, AlcoonInstructionAuditRoom);
        var installation = RepositoryInstallation.Installation;
        LoadedRoomAssets assets = LoadFixtureRoomAssets(room, installation.LoadRoomVisualLayouts());
        var vram = new SnesVram();
        var samusColours = new SamusColours();
        assets.LoadGraphics(BgArtworkFixtures.For(vram));
        var random = new SharedGameState(new SharedRandom());
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices((installedArt ?? InstalledFixtures.EnemyArtwork).Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(random.NextRandom) { ReadRandom = () => random.RandomNumber, SeedRandom = random.SetRandomNumber }, installedBosses: (installedArt ?? InstalledFixtures.EnemyArtwork).Bosses);
        enemies.Load(RoomEnemyFrameFixtures.StandaloneLoad(room.State.EnemyPopulation, BgArtworkFixtures.For(vram), assets.LevelData, RoomEnemyFrameFixtures.StandaloneSamus()));

        RoomEnemyPlace actor = enemies.Places.InUpdateOrder[0];
        // The floor the slot-zero Alcoon lands on, which its setup finds; Samus stands on it.
        int landingY = enemies.AlcoonBehavior.InitializeAlcoon(actor, assets.LevelData).LandingY;
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(actor.Position.X.Whole + 1)), checked((ushort)landingY))),
        };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);

        bool sawLeftFireball = false;
        bool sawRightFireball = false;
        var shownPictures = new HashSet<EnemyFrameId>();
        // The way the slot-zero Alcoon walks, as its steps show it; Samus waits ahead of it.
        int walking = -1;
        for (int frame = 0; frame < 4_096 &&
             (installedArt is not null || shownPictures.Count < AuthoredPictures().Count ||
              !sawLeftFireball || !sawRightFireball); frame++)
        {
            int maximumX = Math.Max(0, room.Size.X * 256 - 256);
            ushort cameraX = unchecked((ushort)Math.Clamp(actor.Position.X.Whole - 128, 0, maximumX));
            enemies.StepStandaloneFrame(enemies.StandaloneFrame(assets.LevelData) with { Camera = new(cameraX, 0), TimeIsFrozen = false, Samus = samus });
            // Each Alcoon's shown picture is the pose its animation reached.
            foreach (RoomEnemyPlace place in enemies.Places.InUpdateOrder)
            {
                if (place.Holds(EnemyDefinitionId.Alcoon) && place.Frame is { } picture)
                    shownPictures.Add(picture);
            }
            // The slot-zero Alcoon faces the way it walks, as the picture it shows says.
            if (actor.Frame is { } facing)
                walking = facing.ToString().Contains("FacingLeft", StringComparison.Ordinal) ? -1
                    : facing.ToString().Contains("FacingRight", StringComparison.Ordinal) ? 1 : walking;

            samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(actor.Position.X.Whole + 32 * walking)), checked((ushort)landingY)));
            foreach (EnemyProjectile projectile in enemies.Projectiles.EnemyProjectiles.Where(
                         projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball))
            {
                sawLeftFireball |= unchecked((short)projectile.Velocity.X.RawValue) < 0;
                sawRightFireball |= unchecked((short)projectile.Velocity.X.RawValue) > 0;
            }
            enemies.StepEnemyProjectiles(assets.LevelData, samus: RoomEnemyFrameFixtures.StandaloneSamus(), camera: Position16<RoomPixels>.Zero);
        }

        AssertTrue(sawLeftFireball && sawRightFireball,
            "real Alcoon programs execute fire callbacks in both facings");
        HashSet<EnemyFrameId> pictures = AuthoredPictures();
        string missed = string.Join(", ", pictures.Where(picture => !shownPictures.Contains(picture)));
        // The fire animations end at their start-walking cue (#2061): every authored pose is reachable, so the room shows
        // every picture the programs author.
        AssertTrue(pictures.SetEquals(shownPictures), $"real Alcoon shows every authored picture; missing={missed}");
        Suite(nameof(AlcoonAnimationsVerification.VerifyAlcoonAnimations), () => AlcoonAnimationsVerification.VerifyAlcoonAnimations(rom));

        Console.WriteLine(installedArt is null
            ? "Alcoon: a real room reaches all 42 reachable poses of the ten programs, fires in both facings, and hands " +
              "off to walking as the native callbacks do."
            : "Installed Alcoon: real-room movement and both fire directions pass with the installed artwork.");
    }

    /// <summary>Every picture the Alcoon's programs show.</summary>
    private static HashSet<EnemyFrameId> AuthoredPictures() =>
        [.. Enum.GetValues<AlcoonSegment>()
            .SelectMany(program => AlcoonAnimations.Segments.Beats(program))
            .Where(beat => beat.ShownFrame is not null)
            .Select(beat => (EnemyFrameId)beat.ShownFrame!)];

    /// <summary>Runs the standalone Alcoon instruction mechanics verification command.</summary>
    [VerificationCommand("--alcoon-instruction-mechanics")]
    internal static int AlcoonInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyAlcoonInstructionProgramDefinitions), () => VerifyAlcoonInstructionProgramDefinitions());
        return 0;
    }
}
