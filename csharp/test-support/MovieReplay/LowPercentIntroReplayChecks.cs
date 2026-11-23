using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// Intro checks the 13% movie replay adds to its per-update comparison: every intro phase
/// handoff on the update its bsnes capture shows bank $8B's matching cinematic function
/// installed, no other phase change, and the Samus values the capture records at the intro
/// updates owned by one fix each. One instance follows one replay.
/// </summary>
internal sealed class LowPercentIntroReplayChecks : IConvertedMovieChecks
{
    /// <summary>
    /// Every phase handoff, keyed by the update after which the capture shows bank $8B's
    /// matching cinematic function installed. Between them no handoff occurs.
    /// </summary>
    private static readonly Dictionary<int, LowPercentIntroReplayPhase> phases = new()
    {
        [214] = new LowPercentIntroReplayPhase("Initial", "$A395"),
        [215] = new LowPercentIntroReplayPhase("WaitForInitialMusicQueue", "$A5A7"),
        [232] = new LowPercentIntroReplayPhase("FadeInFirstNarration", "$A5BD"),
        [262] = new LowPercentIntroReplayPhase("LastMetroidIsInCaptivity", "$A5F8"),
        [322] = new LowPercentIntroReplayPhase("GalaxyIsAtPeace", "$A613"),
        [522] = new LowPercentIntroReplayPhase("WaitForSecondMusicQueue", "$A639"),
        [554] = new LowPercentIntroReplayPhase("HoldFirstNarration", "$A64C"),
        [794] = new LowPercentIntroReplayPhase("FadeOutFirstNarration", "$A663"),
        [824] = new LowPercentIntroReplayPhase("SetupFirstPage", "$A66F"),
        [825] = new LowPercentIntroReplayPhase("WaitForFirstPageMusicQueue", "$A82B"),
        [857] = new LowPercentIntroReplayPhase("FadeInFirstPage", "$A84A"),
        [887] = new LowPercentIntroReplayPhase("PageText/ZebesBattle", "$A391"),
        [1718] = new LowPercentIntroReplayPhase("PageAwaitingInput/ZebesBattle", "$AEB8"),
        [1719] = new LowPercentIntroReplayPhase("MotherBrainCrossfade", "$B250"),
        [1847] = new LowPercentIntroReplayPhase("MotherBrainFlashback", "$A391"),
        [2235] = new LowPercentIntroReplayPhase("PageCrossfade/HatchedLarva", "$B3F4"),
        [2363] = new LowPercentIntroReplayPhase("PageText/HatchedLarva", "$A391"),
        [3011] = new LowPercentIntroReplayPhase("PageAwaitingInput/HatchedLarva", "$AF6C"),
        [3012] = new LowPercentIntroReplayPhase("BabyDiscoveryCrossfade", "$B250"),
        [3140] = new LowPercentIntroReplayPhase("BabyDiscovery", "$A391"),
        [3816] = new LowPercentIntroReplayPhase("PageCrossfade/DeliveryToCeres", "$B3F4"),
        [3944] = new LowPercentIntroReplayPhase("PageText/DeliveryToCeres", "$A391"),
        [4432] = new LowPercentIntroReplayPhase("PageAwaitingInput/DeliveryToCeres", "$B0F2"),
        [4433] = new LowPercentIntroReplayPhase("BabyMetroidDeliveryCrossfade", "$B2D2"),
        [4561] = new LowPercentIntroReplayPhase("BabyMetroidDelivery", "$A391"),
        [4834] = new LowPercentIntroReplayPhase("PageCrossfade/ScientistsFindings", "$B458"),
        [4962] = new LowPercentIntroReplayPhase("PageText/ScientistsFindings", "$A391"),
        [5500] = new LowPercentIntroReplayPhase("PageAwaitingInput/ScientistsFindings", "$B123"),
        [5501] = new LowPercentIntroReplayPhase("BabyMetroidExaminationCrossfade", "$B2D2"),
        [5629] = new LowPercentIntroReplayPhase("BabyMetroidExamination", "$A391"),
        [5902] = new LowPercentIntroReplayPhase("PageCrossfade/DistressSignal", "$B458"),
        [6030] = new LowPercentIntroReplayPhase("PageText/DistressSignal", "$A391"),
        [6683] = new LowPercentIntroReplayPhase("PageAwaitingInput/DistressSignal", "$B1DA"),
        [6684] = new LowPercentIntroReplayPhase("PageText/CeresUnderAttack", "$A390"),
        [6968] = new LowPercentIntroReplayPhase("IntroFadeOut", "$B72F"),
        [6998] = new LowPercentIntroReplayPhase("CeresFlight/Initial", "$BCA0"),
        [6999] = new LowPercentIntroReplayPhase("CeresFlight/WaitForMusicQueue", "$BDE4"),
        [7023] = new LowPercentIntroReplayPhase("CeresFlight/FlyingIntoCamera", "$BDF9"),
        [7055] = new LowPercentIntroReplayPhase("CeresFlight/FlyingTowardCeres", "$BFDA"),
        [7407] = new LowPercentIntroReplayPhase("CeresFlight/SpaceColonyTitle", "$A38F"),
        [7711] = new LowPercentIntroReplayPhase("CeresFlight/FadeOut", "$C0C5"),
        [7741] = new LowPercentIntroReplayPhase("CeresFlight/StartGameAtCeres", "$C100"),
    };

    /// <inheritdoc/>
    /// <remarks>The intro checks read only the port's state after each update.</remarks>
    IEnumerable<string> IConvertedMovieChecks.Compare(SuperMetroidGame game, int update, NativeCheckpointMemory native) => Compare(game, update, native);

    /// <inheritdoc/>
    IEnumerable<string> IConvertedMovieChecks.Finish() => [];

    /// <summary>The intro phase after the previous intro update, or null before the intro.</summary>
    private string? previous;

    /// <summary>Checks the port intro phase and Samus milestone after an intro update.</summary>
    internal IEnumerable<string> Compare(SuperMetroidGame game, int update, NativeCheckpointMemory native)
    {
        if (game.GameState != SuperMetroidGameState.IntroCinematic || game.Session.ScreenAs<IntroCinematicState>() is not { } intro)
            return [];
        string phase = intro.Phase.ToString();
        // The page phases are one flow parameterized by the page they show (#2489).
        if (intro.Phase is IntroCinematicPhase.PageText or IntroCinematicPhase.PageAwaitingInput or IntroCinematicPhase.PageCrossfade)
            phase += "/" + intro.Page;
        if (intro.CurrentCeresFlight is { } flight)
            phase += "/" + flight.Phase;
        var mismatches = new List<string>();
        if (phases.TryGetValue(update, out var expected))
        {
            if (phase != expected.Phase)
                mismatches.Add($"Intro phase: native installs $8B:{expected.Native[1..]} ({expected.Phase}), port is in {phase}");
        }
        else if (previous is not null && phase != previous)
            mismatches.Add($"Intro phase: port entered {phase}, where native installs no new cinematic function");
        previous = phase;
        CheckSamusMilestone(game, intro, update, native);
        return mismatches;
    }

    /// <summary>
    /// The Samus the 13% capture records at intro updates owned by one fix each: in the Mother Brain flashback, the Samus the
    /// scene draws is the one the capture holds (#2554).
    /// </summary>
    private static void CheckSamusMilestone(SuperMetroidGame game, IntroCinematicState intro, int update, NativeCheckpointMemory native)
    {
        SamusState? discovery = intro.CurrentBabyDiscovery is { } scene
            ? scene.Samus : null;
        switch (update)
        {
            case 1809:
                // $90:A3BA: the demo's held shot restarts and holds the standing animation's first frame.
            case 1884:
                // $8B:B90B: Rinka zero, stepped in its spawn pass, strikes Samus on native's update: she shows knocked back.
            case 2111:
                // $91:874B: the demo-end lock keeps the running animation of an unchanged pose.
                CheckDrawnFlashbackSamus(game, intro, update, native);
                break;
            case 2050:
                // $90:EB02 cleared the run's movement records, so the missile inherits nothing.
                var slots = intro.flashbackProjectiles;
                SamusProjectile missile = slots.Pool.Shots[0];
                VerificationAssertions.AssertEqual(-256, missile.Velocity.X.InTwoHundredFiftySixthsOfPixel, "update 2050 third missile launch velocity: a pixel leftward");
                VerificationAssertions.AssertEqual((ushort)0x0088, missile.Position.X.Whole, "update 2050 third missile X");
                break;
            case 2400:
                // $8B:B842 cleared $1A57, so Samus is not processed during the page-two text: Mother Brain's deletion hid her,
                // and the discovery carries on the animation the freeze kept (update 3012).
                VerificationAssertions.AssertTrue(intro.ComposeScene().DrawnSamusBody is null, "update 2400: the page-two text draws no Samus");
                break;
            case 3012:
                // $8B:AF99: pose two was already set, so the discovery keeps her animation running.
                VerificationAssertions.AssertEqual((ushort)0x0002, (discovery ?? throw new InvalidOperationException("Verification requires discovery.")).AnimationFrameTimer.RawValue, "update 3012 discovery Samus animation timer");
                VerificationAssertions.AssertEqual(0, discovery.AnimationNumber, "update 3012 discovery Samus animation frame");
                break;
            case 3990:
                // $91:8682 pointed both state handlers at an RTL during update 3943.
                VerificationAssertions.AssertEqual((ushort)0x0028, (discovery ?? throw new InvalidOperationException("Verification requires discovery.")).Position.X.Whole, "update 3990 stopped Samus X");
                VerificationAssertions.AssertEqual((ushort)0x5000, discovery.Kinematics.Position.X.Fraction, "update 3990 stopped Samus X fraction");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// The flashback draws Samus's body as the capture's Samus shows at <paramref name="update"/>: her recorded pose at her
    /// recorded animation frame, where the recording places her, drawn by the flashback's own artwork.
    /// </summary>
    private static void CheckDrawnFlashbackSamus(SuperMetroidGame game, IntroCinematicState intro, int update, NativeCheckpointMemory native)
    {
        ImageSpriteDraw drawn = intro.ComposeScene().DrawnSamusBody
            ?? throw new InvalidOperationException($"Update {update}: the Mother Brain flashback draws no Samus.");
        var recorded = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(),
            SamusCompatibility.ForNewSamus(), new BeamChargeState(), game.Session.Artwork.FlashbackSamusArtwork())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(native.Word(NativeSnapshotMemory.X), native.Word(NativeSnapshotMemory.Y))),
        };
        recorded.PlaceInPose(SamusPoseNumbering.FromNumber(native.Word(NativeSnapshotMemory.Pose)));
        SamusPoseMetadata.RefreshCollisionRadii(recorded);
        SamusPresentation.InitializeAnimation(recorded);
        SamusPresentation.SetAnimationFrameFromMovementOwner(recorded, recorded.AnimationFrameNumbered(native.Word(NativeSnapshotMemory.Animation)),
            native.Word(NativeSnapshotMemory.AnimationTimer));
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        recorded.Presentation.Draw(recorded, sprites, Position16<RoomPixels>.Zero, frameCounter: 0);
        sprites.FinalizeFrame();
        ImageSpriteDraw expected = sprites.Draws.Single(draw => draw.Source.Kind == SpriteSourceKind.Samus).Published(PictureShading.Standard);
        VerificationAssertions.AssertEqual(expected.Image, drawn.Image, $"update {update}: the drawn Samus shows the recorded pose at the recorded animation frame");
        VerificationAssertions.AssertEqual(expected.Origin, drawn.Origin, $"update {update}: the drawn Samus stands where the recording places her");
    }
}
