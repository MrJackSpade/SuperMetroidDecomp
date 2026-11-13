using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainAscentCaptureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;

using SuperMetroid.Core.Rooms.Artwork;
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
        var game=new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(StoryEvent.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MotherBrain);
        if (ascentMaskOnly) return VerifyMotherBrainAscentCapture(runtime);
        // The room shows its library background as the installed image of the pages its list builds (#1406, #1466),
        // whatever Mother Brain's initialization stages for her own BG2.
        AssertTrue(RoomShowsPipeBackground(), "Mother Brain's room shows both pages of the actual pipe-room background");
        var state = runtime.Enemies.MotherBrainEncounter.MotherBrain ?? throw new InvalidOperationException("Verification requires state.");
        BossBodyParts image = runtime.Enemies.Presentation.RequireBossBody();
        AssertTrue(!image.IsShown && Words(image.Publish()).All(word => word == 0x0338),
            "Initializer attaches her body showing nothing, which her room does not show yet");
        runtime.Enemies.MotherBrainBehavior.Ascent.SetupMotherBrainPhaseTwoGraphics(state);
        // Since #1356 her room shows her body from the transition (#1515), and the room's pipe background stays untouched.
        AssertTrue(image.IsShown && ReferenceEquals(image, runtime.Enemies.Presentation.RequireBossBody())
                && Words(image.Publish())[..0x400].All(word => word == 0x0338),
            "Phase-two setup reveals Mother Brain's body as it stands");
        AssertTrue(RoomShowsPipeBackground(), "Phase-two setup leaves the room's library background as loaded");
        BossSourceArtwork artwork = InstalledFixtures.BossArtwork;
        AssertTrue((artwork.MotherBrainBodyBg2Frames ?? throw new InvalidOperationException("Verification requires Mother Brain's body BG2 frames.")).TryGet(0xa252, out var writes),
            "Crouched body BG2 artwork is installed");
        state.Body.AnimationClock.ShowsNewFrame = true;
        runtime.Enemies.AnimationPlayer.DrawBossBodyFrame(state.Body, EnemyFrameId.MotherBrainBodyCrouched);
        var expectedImage = Enumerable.Repeat((ushort)0x0338, EnemyBg2FrameLayout.TilemapCellCount).ToArray();
        foreach (var write in writes.Span)
            write.Words.CopyTo(expectedImage.AsSpan(write.FirstCell));
        AssertTrue(Words(image.Publish()).SequenceEqual(expectedImage),
            "The crouched body frame shows exactly its authored BG2 words on her body");
        AssertTrue(RoomShowsPipeBackground(), "The body frame leaves the room's library background as loaded");
        Console.WriteLine("Mother Brain tank background: real room pipes retained, native staging clear and later body upload passed.");
        return 0;
        bool RoomShowsPipeBackground() => runtime.Room.Scope is { BackgroundImage: RoomLibraryBackgroundPicture.Drawn { Image: { } shown } } scope
            && scope.Room.State.Background == LibraryBackgroundProgramDefinitions.Get(0xe48a).Background
            && shown.Image.Name == RoomBackgroundImage.NameOf(LibraryBackgroundProgramDefinitions.Get(0xe48a).Background, null, scope.Assets.Tileset);
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

    /// <summary>
    /// The tilemap entry each cell of Mother Brain's <paramref name="layer"/> shows, from the installation's frames her
    /// installed body sheet is composed from (#1466, #1583).
    /// </summary>
    private static ushort[] Words(BossBodyLayer layer) =>
        BossBodyWords.Of(layer, BossBodyOwner.MotherBrain, InstalledFixtures.BossArtwork);
}
