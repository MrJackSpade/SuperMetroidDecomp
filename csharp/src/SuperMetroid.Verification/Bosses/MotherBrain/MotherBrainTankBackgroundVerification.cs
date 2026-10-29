using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainAscentCaptureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;

using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.BossBodies;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
/// <summary>Checks Mother Brain's tank background staging and phase-two BG2 uploads.</summary>
internal static class MotherBrainTankBackgroundVerification
{
    /// <summary>Verifies pipe-room VRAM survives initialization and the crouched body updates only its authored BG2 words.</summary>
    internal static int VerifyMotherBrainTankBackground(bool ascentMaskOnly = false)
    {
        var installation = RepositoryInstallation.Installation;
        var game=new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        var expectedVram = new SnesVram();
        LibraryBackgroundReplay.Execute(expectedVram, LibraryBackgroundProgramDefinitions.Get(0xe48a), FixtureRoomHeaders.UnmatchedDoor, new LibraryBackgroundArtwork { TilemapArt = installation.LoadRoomBackgroundTilemaps() });
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        if (ascentMaskOnly) return VerifyMotherBrainAscentCapture(runtime);
        // The room shows its library background as the image of the pages its list builds (#1406), whatever Mother
        // Brain's initialization stages for her own BG2.
        ushort[] pipePages = [.. Enumerable.Range(0, RoomBackgroundImage.Words)
            .Select(word => expectedVram.ReadWord(GameplayVramLayout.Bg2TilemapWord + word))];
        AssertTrue(RoomShowsPipeBackground(), "Mother Brain's room shows both pages of the actual pipe-room background");
        var state = runtime.Enemies.MotherBrainEncounter.MotherBrain ?? throw new InvalidOperationException("Verification requires state.");
        BossBodySheets sheets = runtime.Enemies.BossArtworkBinding.BodySheets
            ?? throw new InvalidOperationException("The retail runtime installs boss body sheets.");
        AssertTrue(Staged().All(word => word == 0x0338), "Initializer stages her body on a canvas showing only her blank");
        runtime.Enemies.MotherBrainBehavior.Ascent.SetupMotherBrainPhaseTwoGraphics(state);
        // Since #1356 she owns her body layer from the transition: it carries the staged page, and the room's pipe
        // background stays untouched.
        BossBodyCanvas image = runtime.Enemies.Presentation.RequireBossBody();
        AssertTrue(Words(image.Publish(sheets), runtime)[..0x400].All(word => word == 0x0338),
            "Phase-two setup attaches the staged first page as Mother Brain's body layer"); 
        AssertTrue(RoomShowsPipeBackground(), "Phase-two setup leaves the room's library background as loaded");
        var artwork = runtime.Enemies.BossArtwork;
        AssertTrue(((artwork ?? throw new InvalidOperationException("Verification requires boss artwork.")).MotherBrainBodyBg2Frames ?? throw new InvalidOperationException("Verification requires Mother Brain's body BG2 frames.")).TryGet(0xa252, out var writes),
            "Crouched body BG2 artwork is installed");
        state.Body.ExtraProperties = state.Body.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
        runtime.Enemies.Instructions.DrawBossBodyFrame(state.Body, EnemyFrameId.MotherBrainBodyCrouched);
        var expectedImage = Enumerable.Repeat((ushort)0x0338, EnemyBg2FrameLayout.TilemapCellCount).ToArray();
        ushort[] staged = Staged();
        foreach (var write in writes.Span)
        {
            write.Words.CopyTo(expectedImage.AsSpan(write.FirstCell));
            for (int index = 0; index < write.Tiles.Length; index++)
                AssertEqual(write.Tiles.Span[index].PackedWord, staged[write.FirstCell + index],
                    "Body frame draws on her staged body as well as the shown body layer");
        }
        AssertTrue(Words(image.Publish(sheets), runtime).SequenceEqual(expectedImage),
            "The crouched body frame draws exactly its authored BG2 words on the body layer");
        AssertTrue(RoomShowsPipeBackground(), "The body frame leaves the room's library background as loaded");
        Console.WriteLine("Mother Brain tank background: real room pipes retained, native staging clear and later body upload passed.");
        return 0;
        ushort[] Staged() => Words((state.StagedBody ?? throw new InvalidOperationException("Mother Brain stages her body.")).Publish(sheets), runtime);
        bool RoomShowsPipeBackground() => (runtime.Room.Scope?.BackgroundImage
            ?? throw new InvalidOperationException("Mother Brain's room selects no library background image.")).Image.Tiles.SequenceEqual(BackgroundTileWordBytes.Tiles(pipePages));
    }

    /// <summary>Runs the ascent mask capture using the initialized Mother Brain room.</summary>
    [VerificationCommand("--mother-brain-ascent-capture")]
    internal static int MotherBrainAscentCaptureCommand()
    {
        return VerifyMotherBrainTankBackground(ascentMaskOnly: true);
    }

    /// <summary>Runs the tank background staging and VRAM transfer check.</summary>
    [VerificationCommand("--mother-brain-tank-background")]
    internal static int MotherBrainTankBackgroundCommand()
    {
        return VerifyMotherBrainTankBackground();
    }

    /// <summary>The tilemap entry each cell of Mother Brain's <paramref name="layer"/> shows, from the frames the runtime's artwork installs (#1466).</summary>
    private static ushort[] Words(BossBodyLayer layer, SuperMetroidRuntime runtime) =>
        BossBodyWords.Of(layer, BossBodyOwner.MotherBrain, runtime.BossArtwork.Artwork
            ?? throw new InvalidOperationException("The runtime installs boss artwork."));
}
