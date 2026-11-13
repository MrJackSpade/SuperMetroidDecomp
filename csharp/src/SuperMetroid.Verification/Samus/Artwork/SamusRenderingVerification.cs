using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Samus.Palettes.SamusVisorColorsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Samus.Artwork.SamusRenderingFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Verifies Samus body, arm cannon, visor, and hurt palette rendering with cartridge-shaped fixtures.</summary>
internal static class SamusRenderingVerification
{

/// <summary>Samus rendering, arm cannon, visor, hurt palette, and pose fixture verification.</summary>
internal static void VerifySamusRenderingSlice()
{
    var bus = new TestAddressSpace();
    SeedPoseOneSamusData(bus);

    var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        AnimationFrame = 0,
        Position = FixedPosition<RoomPixels>.FromWhole(new(0x0480, 0x0086)),
    };
    var samusColours = new SamusColours();
    var oamFrame = new SpriteFrameBuffer();
    var oam = new OamBuffer();

    InstalledSamusSuitColors suitColors = SamusSuitColorFile.Read(
        new MemoryStream(SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(bus)));
    SamusNormalSuitPalette.LoadPower(samusColours);
    // Samus's suit shows the Power Suit's shade; the import's shade holds the installed colours (#1512).
    RgbColor[] powerSuit = samusColours.ShownSuit(InstalledFixtures.SamusShadeSources with { Suits = suitColors });
    AssertEqual(0x3800, powerSuit[0], "Samus power-suit shade colour zero");
    AssertEqual(0x000d, powerSuit[15], "Samus power-suit shade colour fifteen");

    BindSyntheticSamusRendering(bus, samus);
    // The import composes pose 1's frame-zero picture of the definitions its selector names, laid where the picture's
    // parts read them (#1585).
    SamusBodySources poseOneArtwork = BoundArtwork(samus);
    SamusBodyFramePictures poseOnePictures = poseOneArtwork.Pictures(SamusPoseId.FacingRightNormalPose, 0);
    AssertEqual(0x92d0b0, 0x920000 | SamusBodyNative.RecordPointer(SamusBodySources.Record(poseOnePictures.Top)), "pose 1 frame 0 top tile definition");
    AssertEqual(0x92d1c8, poseOnePictures.Bottom is { } poseOneBottom ? 0x920000 | SamusBodyNative.RecordPointer(SamusBodySources.Record(poseOneBottom)) : 0,
        "pose 1 frame 0 bottom tile definition");
    ReadOnlySpan<byte> poseOneCharacters = SamusBodySheetFixtures.CharactersOf(poseOneArtwork)
        .BodyArtwork(poseOnePictures.Top, poseOnePictures.Bottom).Characters;
    AssertEqual(0x10, poseOneCharacters[0], "Samus top part 1 is the composed body's character 0");
    AssertEqual(0x20, poseOneCharacters[16 * SpriteArtwork.CharacterByteCount], "Samus top part 2 is the composed body's character 16");
    AssertEqual(0x30, poseOneCharacters[8 * SpriteArtwork.CharacterByteCount], "Samus bottom part 1 is the composed body's character 8");
    AssertEqual(0x40, poseOneCharacters[24 * SpriteArtwork.CharacterByteCount], "Samus bottom part 2 is the composed body's character 24");

    SamusPresentation.InitializeAnimation(samus);
    AssertEqual(10, samus.AnimationFrameTimer.RawValue, "pose 1 initial animation delay");

    // Frames 0-3 each last ten calls. The fifth byte is command $F6, which returns a
    // healthy Samus to frame zero rather than ever exposing command index four as art.
    for (int tick = 0; tick < 9; tick++)
        SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
    AssertEqual(0, samus.AnimationFrame, "standing frame remains zero for first nine ticks");
    AssertEqual(1, samus.AnimationFrameTimer.RawValue, "standing timer reaches one before advance");
    SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
    AssertEqual(1, samus.AnimationFrame, "standing frame advances on tenth tick");
    AssertEqual(10, samus.AnimationFrameTimer.RawValue, "next standing frame reloads ten ticks");
    for (int tick = 0; tick < 30; tick++)
        SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
    AssertEqual(0, samus.AnimationFrame, "healthy $F6 command loops standing animation");

    // Below 30 energy, the same command enters frames 5-8. Command $FE,$04 then subtracts
    // four byte positions and loops that faster eight-tick breathing sequence.
    samus.Health = 29;
    SamusPresentation.InitializeAnimation(samus);
    for (int tick = 0; tick < 40; tick++)
        SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
    AssertEqual(5, samus.AnimationFrame, "low-health $F6 enters alternate sequence");
    AssertEqual(8, samus.AnimationFrameTimer.RawValue, "low-health sequence uses eight-tick delay");
    for (int tick = 0; tick < 32; tick++)
        SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
    AssertEqual(5, samus.AnimationFrame, "$FE,$04 loops low-health standing sequence");

    // The item transition's two poses, chosen by equipment (#1473). Only the unused poses $3F/$40 hold one in retail;
    // pose $3F's run is one zero-tick frame, so the death-animation entry point, which shares the interpreter but needs no
    // gameplay movement metadata, reaches the transition on its first tick.
    var itemTransitionSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = SamusPoseId.InstantMorphingRightPose };
    SamusPresentation.InitializeAnimation(itemTransitionSamus);
    SamusPresentation.AnimateDeathFrame(itemTransitionSamus);
    AssertEqual(SamusPoseId.MorphBallGroundRightPose, itemTransitionSamus.PendingTransitionalPose,
        "an item transition without Spring Ball selects its unequipped pose");
    itemTransitionSamus.EquippedItems = SamusEquipmentFlags.SpringBall;
    SamusPresentation.InitializeAnimation(itemTransitionSamus);
    SamusPresentation.AnimateDeathFrame(itemTransitionSamus);
    AssertEqual(SamusPoseId.SpringBallGroundRightPose, itemTransitionSamus.PendingTransitionalPose,
        "an item transition with Spring Ball selects its equipped pose");

    // Restore the ordinary debugger scenario before verifying frame-zero drawing below.
    samus.Health = 99;
    SamusPresentation.InitializeAnimation(samus);

    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, oamFrame, cameraPosition: new(0x0400, 0));
    oam = oamFrame.FinalizeAsOam();

    // Pose $01's graphics Y offset is six, so world (1152,134) becomes origin (128,128).
    AssertEqual(128, samus.Presentation.DrawOrigin.X, "Samus default screen X calculation");
    AssertEqual(128, samus.Presentation.DrawOrigin.Y, "Samus signed graphics-Y offset calculation");
    // The body records the pose and picture frame it drew, not a table index (#1465), as one whole picture (#1513, #1585).
    var poseOne = new SamusBodyFrame(SamusPoseId.FacingRightNormalPose, 0);
    AssertEqual(poseOne, samus.Presentation.DrawnFrame, "Samus pose 1 draws its frame-zero picture");
    AssertEqual(2, Composition(samus, poseOne).Bottom is null ? 1 : 2, "Samus pose 1's frame-zero picture is composed of its top and bottom halves");
    // The published description: the whole body as one pre-coloured frame, the sheet frame of its pose and picture
    // frame, at the spritemap origin.
    SpriteDraw[] body = [.. oamFrame.Draws.Where(draw => draw.Source.Kind == SpriteSourceKind.Samus)];
    AssertEqual(1, body.Length, "Samus pose 1 publishes one full-body frame");
    AssertTrue(ShowsBodyFrame(samus, body[0], poseOne), "the body draw shows its whole-picture sheet frame");
    AssertEqual(new SpriteSource(SpriteSourceKind.Samus), body[0].Source, "body draw source");
    AssertEqual(samus.Presentation.DrawOrigin, body[0].Origin, "body draw sits at the spritemap origin");
    AssertEqual(7, oam.LastFinalizedSpriteCount, "Samus pose 1 emits four top and three bottom OBJs");

    OamEntry firstTop = oam.GetEntry(0);
    AssertEqual(121, firstTop.Position.X, "Samus top OBJ signed X offset");
    AssertEqual(120, firstTop.Position.Y, "Samus top OBJ signed Y offset");
    AssertTrue(firstTop.IsLarge, "Samus top OBJ preserves ROM size bit");
    AssertEqual(4, firstTop.Palette, "Samus top OBJ preserves ROM palette");
    AssertEqual(2, firstTop.Priority, "Samus top OBJ preserves ROM priority");
    AssertEqual(0, firstTop.TileNumber, "Samus top OBJ tile number");

    OamEntry firstBottom = oam.GetEntry(4);
    AssertEqual(113, firstBottom.Position.X, "Samus bottom OBJ signed X offset");
    AssertEqual(144, firstBottom.Position.Y, "Samus bottom OBJ positive Y offset");
    AssertEqual(8, firstBottom.TileNumber, "Samus bottom OBJ tile number");

    // The verified ordinary transition applies pose $09 after movement/animation and before
    // drawing. Its movement type one uses default position math and always draws both halves.
    SeedPoseNineSamusData(bus);
    SamusGroundedPoseTransitions.ApplyStandingRightToRunningRight(samus);
    AssertEqual(SamusPoseId.MovingRightNormalPose, samus.Pose, "standing-right transition applies running-right pose");
    AssertEqual(0, samus.AnimationFrame, "running transition resets animation frame");
    AssertEqual(2, samus.AnimationFrameTimer.RawValue, "running pose first delay byte");
    AssertEqual(21, samus.Kinematics.Radius.Y, "running pose refreshes collision radius");

    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, oamFrame, cameraPosition: new(0x0400, 0));
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(new SamusBodyFrame(SamusPoseId.MovingRightNormalPose, 0), samus.Presentation.DrawnFrame,
        "running pose draws its frame-zero picture");
    AssertEqual(2, Composition(samus, new SamusBodyFrame(SamusPoseId.MovingRightNormalPose, 0)).Bottom is null ? 1 : 2,
        "running pose's frame-zero picture is composed of its top and bottom halves");
    AssertEqual(2, oam.LastFinalizedSpriteCount, "synthetic running pose draws top and bottom pieces");

    SamusGroundedPoseTransitions.ApplyRunningRightToStandingRight(samus);
    AssertEqual(SamusPoseId.FacingRightNormalPose, samus.Pose, "running no-button fallback applies standing-right pose");
    AssertEqual(0, samus.AnimationFrame, "standing fallback resets animation frame");
    AssertEqual(10, samus.AnimationFrameTimer.RawValue, "standing fallback reloads frame-zero delay");

    // `$90:868D` inserts one direct small-OBJ write between pose `$00`'s ordinary top and
    // bottom spritemaps. This record is easy to lose in a high-level “draw both halves”
    // abstraction, so verify its exact OAM order, coordinates, size, and attribute word.
    SeedForwardFacingSamusData(bus);
    samus.EquippedItems = 0;
    samus.HorizontalSpeed.BaseSpeed = samus.HorizontalSpeed.BaseSpeed.WithWhole(3);
    samus.Kinematics.YSpeed = samus.Kinematics.YSpeed.WithWhole(2);
    samus.PoseHistory.PreviousPose = SamusPoseId.SpinJumpRightPose;
    samus.PoseHistory.PreviousMovement = SamusMovementType.SpinJumping;
    samus.PoseHistory.LastDifferentMovement = SamusMovementType.WallJumping;
    // A $18 live radius takes MakeSamusFaceForward's no-lift branch, so the drawing
    // checks below keep Samus at the seeded Y.
    samus.Kinematics.Radius = samus.Kinematics.Radius.WithY(0x18);
    ushort radiusBeforeForwardSetup = samus.Kinematics.Radius.Y;
    SamusPoseMetadata.ApplyForwardFacingPoseSetup(samus);
    AssertEqual(SamusMovementType.SpinJumping, samus.PoseHistory.LastDifferentMovement,
        "forward setup shifts packed direction/movement metadata");
    AssertEqual(samus.Pose, samus.PoseHistory.PreviousPose, "forward setup commits new pose");
    AssertEqual(SamusPoseMetadata.ReadMovementType(samus),
        samus.PoseHistory.PreviousMovement, "forward setup commits new metadata");
    SamusPoseMetadata.ApplyForwardFacingPoseSetup(samus);
    AssertTrue(!samus.PoseHistory.AllowsWallJumpProbe,
        "repeated forward setup cannot retain an obsolete spin walljump gate");
    AssertEqual(SamusPoseId.ForwardFacingPowerSuitPose, samus.Pose, "no suit selects power forward pose");
    // MakeSamusFaceForward ($91:E3F6) only compares the live radius with $18; the next
    // frame's SetSamusRadius installs the pose's own radius.
    AssertEqual(radiusBeforeForwardSetup, samus.Kinematics.Radius.Y, "forward setup leaves the live radius");
    AssertEqual(8, samus.AnimationFrameTimer.RawValue, "power forward setup reads delay eight");
    AssertEqual(0, samus.HorizontalSpeed.BaseSpeed.Whole, "forward setup clears base X speed");
    AssertEqual(0, samus.Kinematics.YSpeed.Whole, "forward setup clears Y speed");
    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, oamFrame, cameraPosition: new(0x0400, 0));
    oam = oamFrame.FinalizeAsOam();
    SamusBodyFrame forward = samus.Presentation.DrawnFrame ?? throw new InvalidOperationException("Power-suit forward drew no body.");
    AssertEqual(new SamusBodyFrame(SamusPoseId.ForwardFacingPowerSuitPose, 0), forward, "power-suit forward draws its frame-zero picture");
    SamusBodyComposition forwardComposition = Composition(samus, forward);
    AssertEqual(new SamusBodyHalfFrame(SamusPoseId.ForwardFacingPowerSuitPose, SamusBodyHalf.Top, 0), forwardComposition.Top,
        "power-suit forward's picture is composed of its top half's frame-zero picture");
    AssertEqual(new SamusBodyHalfFrame(SamusPoseId.ForwardFacingPowerSuitPose, SamusBodyHalf.Bottom, 0), forwardComposition.Bottom,
        "power-suit forward's picture is composed of its bottom half's frame-zero picture");
    AssertTrue(forwardComposition.ChestCover is not null, "power-suit forward's whole picture holds the chest cover");
    SpriteDraw[] forwardBody = [.. oamFrame.Draws.Where(draw => draw.Source.Kind == SpriteSourceKind.Samus)];
    AssertEqual(1, forwardBody.Length, "power-suit forward publishes its halves and chest cover as one full-body frame");
    AssertTrue(ShowsBodyFrame(samus, forwardBody[0], forward), "power-suit forward shows its full-body sheet frame");
    AssertEqual(3, oam.LastFinalizedSpriteCount, "power-suit forward top/chest/bottom OAM order");
    OamEntry chestCover = oam.GetEntry(1);
    AssertEqual(121, chestCover.Position.X, "forward chest-cover X is Samus screen X minus seven");
    AssertEqual(117, chestCover.Position.Y, "forward chest-cover Y is Samus screen Y minus seventeen");
    AssertEqual(0x21, chestCover.TileNumber, "forward chest-cover tile number");
    AssertEqual(4, chestCover.Palette, "forward chest-cover palette");
    AssertEqual(3, chestCover.Priority, "forward chest-cover priority");
    AssertTrue(!chestCover.IsLarge, "forward chest-cover is one small OBJ");

    // `$9B` uses dedicated suited art and therefore must not inherit `$00`'s chest patch.
    samus.EquippedItems = SamusEquipmentFlags.VariaSuit;
    SamusPoseMetadata.ApplyForwardFacingPoseSetup(samus);
    AssertEqual(SamusPoseId.ForwardFacingSuitedPose, samus.Pose, "Varia selects suited forward pose");
    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, oamFrame, cameraPosition: new(0x0400, 0));
    oam = oamFrame.FinalizeAsOam();
    // The suited forward frame selects bottom set $FF: no bottom transfer. Its whole picture is composed of its own
    // selection alone, with no bottom picture carried from an earlier frame (#1585).
    var suited = new SamusBodyFrame(SamusPoseId.ForwardFacingSuitedPose, 0);
    SamusBodyFramePictures suitedPictures = BoundArtwork(samus).Pictures(suited.Pose, suited.PictureFrame);
    AssertTrue(suitedPictures.Bottom is null, "the suited forward frame names no bottom record");
    AssertTrue(BoundArtwork(samus).Spritemaps.PictureKey(Composition(samus, suited), suitedPictures) is { BottomPicture: null },
        "a frame with no bottom transfer is composed with no bottom picture");
    SpriteDraw[] suitedBody = [.. oamFrame.Draws.Where(draw => draw.Source.Kind == SpriteSourceKind.Samus)];
    AssertTrue(suitedBody.Length == 1 && ShowsBodyFrame(samus, suitedBody[0], suited),
        "a frame with no bottom transfer draws its own whole-picture sheet frame");
    AssertEqual(suited, samus.Presentation.DrawnFrame, "suited forward draws its frame-zero picture");
    AssertEqual(2, oam.LastFinalizedSpriteCount, "suited forward emits no power-suit chest patch");

    // `$90:85E2` suppresses body OAM on odd invincibility frames. This is deliberately not folded into the arm-
    // cannon test: `$90:C663` has a similar-looking but stricter condition of its own.
    samus.InvincibilityTimer = WrappingCountdown.Start(1);
    samus.KnockbackTimer = 0;
    SamusBodyFrame? drawnBeforeHidden = samus.Presentation.DrawnFrame;
    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, oamFrame, cameraPosition: new(0x0400, 0), frameCounter: 1);
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(0, oam.LastFinalizedSpriteCount,
        "odd invincibility frame suppresses complete Samus body");
    AssertEqual(drawnBeforeHidden, samus.Presentation.DrawnFrame,
        "a hidden invincibility frame keeps the picture last drawn, which the echoes repeat");

    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, oamFrame, cameraPosition: new(0x0400, 0), frameCounter: 2);
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(2, oam.LastFinalizedSpriteCount,
        "even invincibility frame draws complete Samus body");

    // Knockback and shine are two independent OR terms in the native branch. Prove each
    // on an odd frame so neither can pass accidentally through the even-NMI condition.
    samus.KnockbackTimer = 1;
    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, oamFrame, cameraPosition: new(0x0400, 0), frameCounter: 1);
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(2, oam.LastFinalizedSpriteCount,
        "knockback overrides odd-frame invincibility blink");

    samus.KnockbackTimer = 0;
    AssertTrue(samus.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(4, 0)),
        "test fixture installs nonzero native shine timer");
    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, oamFrame, cameraPosition: new(0x0400, 0), frameCounter: 1);
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(2, oam.LastFinalizedSpriteCount,
        "shine timer overrides odd-frame invincibility blink");

    // Give every synthetic spritemap index used below one visible OBJ. Top and bottom base indices are both zero, so a
    // split body's picture is the top-only picture: the body sheet lays that picture out once (#1513), and its OAM part
    // count no longer tells the halves apart. The drawn picture's composition is the witness: a bottom half means the
    // lower-half dispatcher accepted that pose and frame, none means the top-only branch.
    SeedBodyHalfRoutingSpritemaps(bus);

    // Reset the unrelated flicker inputs before exercising the complete `$90:86EE/$870C`
    // bottom-half matrix using authored pose/movement pairs. Movement type $0C has no
    // authored pose record, so production correctly cannot reach that dispatcher slot.
    samus.InvincibilityTimer = WrappingCountdown.Start(0);
    samus.KnockbackTimer = 0;
    SeedBodyHalfRoutingTileAnimation(bus);
    SamusRenderingVerifySamusRenderingSliceCase[] routingCases =
    [
        new SamusRenderingVerifySamusRenderingSliceCase(SamusPoseId.DeathSequenceRightPose, 0x0a, 1, 1, "$D7 frame one top-only"),
        new SamusRenderingVerifySamusRenderingSliceCase(SamusPoseId.DeathSequenceLeftPose, 0x0a, 2, 1, "$D8 frame two top-only"),
        new SamusRenderingVerifySamusRenderingSliceCase(SamusPoseId.DeathSequenceRightPose, 0x0a, 3, 2, "$D7 frame three split"),
        new SamusRenderingVerifySamusRenderingSliceCase(SamusPoseId.CrouchingTransitionRightPose, 0x0f, 0, 2, "$35 basic crouch transition split"),
        new SamusRenderingVerifySamusRenderingSliceCase(SamusPoseId.MorphingTransitionRightPose, 0x0f, 0, 1, "$37 morph transition top-only"),
        new SamusRenderingVerifySamusRenderingSliceCase(SamusPoseId.UnmorphingTransitionRightPose, 0x0f, 1, 1, "$3D unmorph transition top-only"),
        new SamusRenderingVerifySamusRenderingSliceCase(SamusPoseId.DraygonGrabbedMovingRightPose, 0x1a, 2, 2, "$F0 held body split"),
        new SamusRenderingVerifySamusRenderingSliceCase(SamusPoseId.CrouchingTransitionAimUpRightPose, 0x0f, 0, 2, "$F1 aimed transition always split"),
    ];
    // Every case's pose records are seeded before any is drawn: each draw reads only its own pose's records, so the cases
    // share one body built from the seeded tables rather than each composing its own body's sheets.
    foreach ((SamusPoseId pose, byte movementType, _, _, _) in routingCases)
    {
        WritePoseDefinition(bus, SamusPoseNumbers.Number(pose), [8, movementType, 0xff, 0xff, 0, 0, 16, 0]);
        WriteSamusTileAnimationPointer(bus, pose, 0xe100);
        WriteSamusSpritemapBases(bus, pose, top: 0, bottom: 0);
    }
    foreach ((SamusPoseId pose, byte movementType, ushort frame, int expectedHalves, string name) in routingCases)
    {
        AssertEqual(movementType, SamusMovementTypeNativeWords.Byte(SamusPoseMetadata.ReadMovementType(pose)), "Rendering fixture uses native movement identity");
        samus.Pose = pose;
        samus.AnimationFrame = frame;
        oamFrame.BeginFrame();
        BindSyntheticSamusRendering(bus, samus);
        samus.Presentation.Draw(samus, oamFrame, cameraPosition: samus.Position.Whole);
        oam = oamFrame.FinalizeAsOam();
        AssertTrue(oam.LastFinalizedSpriteCount > 0, $"{name} draws the body");
        SamusBodyFrame drawn = samus.Presentation.DrawnFrame ?? throw new InvalidOperationException($"{name} published no body frame.");
        AssertEqual(expectedHalves, Composition(samus, drawn).Bottom is null ? 1 : 2, name);
    }

    // `$90:864E` contains exactly 28 bottom-half handlers, and the complete retail
    // `$91:B629-$BD0F` pose table never stores a movement byte above `$1B`. Pose `$FD`'s
    // exact adjacent-code observation is `$8B`, so it is malformed metadata rather than
    // an untranslated draw selector.
    samus.Pose = SamusPoseNumbering.FromNumber(0xfd);
    samus.AnimationFrame = 0;
    AssertThrows<InvalidDataException>(
        () => SamusPoseMetadata.ReadMovementType(samus),
        "movement-type API rejects a discriminator outside the complete retail domain");
    oamFrame.BeginFrame();
    // The body is drawn by the pose's own halves rule, and no path of the game shows a pose past the records (#1513).
    AssertThrows<InvalidOperationException>(
        () => samus.Presentation.Draw(samus, oamFrame, cameraPosition: samus.Position.Whole),
        "a pose past the records is rejected before its malformed movement type is read");

    // Positioning must consume those same ROM bytes instead of duplicating a convenient
    // host switch. `$37` proves signed -4/-2 values.
    samus.Pose = SamusPoseId.MorphingTransitionRightPose;
    samus.Position = samus.Position.WithWholeY(0x0086);
    samus.AnimationFrame = 0;
    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, oamFrame, cameraPosition: new(samus.Position.X.Whole, 0));
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(0x0082, samus.Presentation.DrawOrigin.Y,
        "morph transition frame zero reads signed minus-four table byte");
    samus.AnimationFrame = 1;
    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, oamFrame, cameraPosition: new(samus.Position.X.Whole, 0));
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(0x0084, samus.Presentation.DrawOrigin.Y,
        "morph transition frame one reads signed minus-two table byte");

    // Standing's position selector has two special families. Front-view frames zero/one
    // remain generic, but frame two and later use Y-1. Keep frame two here because pose `$00`
    // already carries a deliberately different graphics offset in the fixture above.
    samus.Pose = SamusPoseId.ForwardFacingPowerSuitPose;
    WriteSamusTileAnimationPointer(bus, samus.Pose, 0xe100);
    samus.AnimationFrame = 2;
    samus.Position = samus.Position.WithWholeY(0x0086);
    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, oamFrame, cameraPosition: new(0x0400, 0));
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(0x0085, samus.Presentation.DrawOrigin.Y,
        "front-facing frame two uses fixed one-pixel graphics offset");

    // Landing's `$90:8D28` table is byte-packed and read by a 16-bit unaligned LDA whose high
    // byte, the next frame's byte, the drawn position never shows, so each frame is placed by
    // its own byte (#1500).
    WriteLandingVerticalOffsets(bus, [
        3, 6, 0, 0,
        3, 6, 0, 0,
        3, 3, 6, 0,
        3, 3, 6, 0,
    ]);
    WritePoseDefinition(bus, SamusPoseNumbers.Number(SamusPoseId.NormalLandingRightPose), [8, 0, 0xff, 0xff, 9, 0, 21, 0]);
    WriteSamusTileAnimationPointer(bus, SamusPoseId.NormalLandingRightPose, 0xe100);
    WriteSamusSpritemapBases(bus, SamusPoseId.NormalLandingRightPose, top: 0, bottom: 0);
    samus.Pose = SamusPoseId.NormalLandingRightPose;
    samus.AnimationFrame = 0;
    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, oamFrame, cameraPosition: new(0x0400, 0));
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(0x0083, samus.Presentation.DrawOrigin.Y,
        "normal-jump landing frame zero is placed by its own byte");
    AssertEqual(0x83, oam.GetEntry(0).Position.Y,
        "landing OAM exposes low-byte three-pixel visual nudge");

    samus.AnimationFrame = 1;
    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, oamFrame, cameraPosition: new(0x0400, 0));
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(0x0080, samus.Presentation.DrawOrigin.Y,
        "normal-jump landing frame one is placed by its own byte");

    // Over the Ceres shaft's turning floor, Samus's drawing turns her point with the floor before applying the
    // ordinary pose offset. A quarter turn around (1152, 128) maps (+8,+6) to (-6,+8). The body must use that
    // temporary point while collision-visible Samus coordinates remain unchanged afterward.
    samus.Pose = SamusPoseId.FacingRightNormalPose;
    samus.AnimationFrame = 0;
    samus.Position = samus.Position.WithWhole(new(0x0488, 0x0086));
    var quarterTurn = new TurningFloorTransform(Math.PI / 2, new Position16<RoomPixels>(0x0480, 0x0080));
    oamFrame.BeginFrame();
    BindSyntheticSamusRendering(bus, samus);
    samus.Presentation.Draw(samus, 
        oamFrame,
        cameraPosition: new(0x0400, 0),
        turningFloor: quarterTurn);
    oam = oamFrame.FinalizeAsOam();
    AssertEqual(0x007a, samus.Presentation.DrawOrigin.X,
        "a quarter-turned floor turns Samus's drawn X about its centre");
    AssertEqual(0x0082, samus.Presentation.DrawOrigin.Y,
        "a quarter-turned floor applies the pose graphics offset after the turned Y");
    AssertEqual(0x0488, samus.Position.X.Whole,
        "the turned drawing restores physical Samus X");
    AssertEqual(0x0086, samus.Position.Y.Whole,
        "the turned drawing restores physical Samus Y");

    Console.WriteLine("  Samus: body art, turning-floor position, bottom rules, tile DMA, OAM, and invincibility flicker agree.");
}

/// <summary>Whether <paramref name="draw"/> is the unmirrored whole-picture sheet frame <paramref name="frame"/> names.</summary>
private static bool ShowsBodyFrame(SamusState samus, SpriteDraw draw, SamusBodyFrame frame)
{
    InstalledSpriteSheets sheets = samus.Presentation.Sheets
        ?? throw new InvalidOperationException("Verification requires bound Samus sprite sheets.");
    return draw is ImageSpriteDraw { Mirror: SpriteMirror.None } image &&
        ReferenceEquals(sheets.Get(SpriteSheetId.SamusBody).Frame(frame.Name), image.Image);
}

/// <summary>The body fixture bound to <paramref name="samus"/>, as the import reads it.</summary>
private static SamusBodySources BoundArtwork(SamusState samus) => samus.Presentation.BoundBodyFixture;

/// <summary>The halves and chest cover the import composed <paramref name="frame"/>'s whole picture of (#1585).</summary>
private static SamusBodyComposition Composition(SamusState samus, SamusBodyFrame frame) =>
    SamusBodyCompositions.Of(frame.Pose, frame.PictureFrame, BoundArtwork(samus));

/// <summary>
/// Walks the complete `$90:C519-$C790` arm-cannon cover lifetime. The fixture keeps the
/// native three-level pose/direction/frame pointer topology intact, so a plausible-looking
/// host animation cannot pass by substituting invented timing or a fixed bitmap.
/// </summary>
internal static void VerifySamusArmCannon()
{
    var bus = new TestAddressSpace();
    SeedPoseOneSamusData(bus);

    // Keep the fixture's custom selector, after-body order and signed offsets, expressed through the extracted drawing
    // data rather than a patched ROM record; the extracted document is import data (#1638).
    var document = RequiredJson.Parse(File.ReadAllText(Path.Combine(
        CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.SamusBodyDirectoryName),
        SamusArmCannonArtworkFormat.ArtworkFileName)));
    document.Required("poseDescriptors")[nameof(SamusPoseId.FacingRightNormalPose)] = 0;
    int[] drawing = [2, 2, 7, 0xfd, 9, 0xfb];
    for (int index = 0; index < drawing.Length; index++)
        document.Required("drawingData")[index] = drawing[index];
    // Direction two's cover keeps its installed appearance, the retail $281F: tile $1F, palette four, priority two.
    document.Required("coverTiles")[2] = new System.Text.Json.Nodes.JsonArray(0, 1, 2);
    using var placement = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString()));
    // The cover characters are import data no installation holds (#1636): the cartridge-derived extraction's are bound.
    using var tiles = File.OpenRead(Path.Combine(CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.SamusBodyDirectoryName),
        SamusArmCannonArtworkFormat.TileFileName));
    SamusArmCannonCoverCharacters coverCharacters = InstalledSamusArmCannonArtworkToolingConstants.LoadWithCover(placement, tiles);
    SamusArmCannonSourceArtwork artwork = coverCharacters.Cannon;
    var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        AnimationFrame = 0,
        Position = FixedPosition<RoomPixels>.FromWhole(new(0x0480, 0x0086)),
        SelectedHudItem = HudItem.Beam,
    };

    samus.Presentation.BindBodyFixture(InstalledFixtures.SamusBody);
    samus.BindArmCannonFixture(artwork);

    // The HUD producer requires two identical samples. Merely changing selection sets the
    // native toggle word to one; only the following stable frame is allowed to transition.
    // A transition start is the only path that rewrites the open flag, so an unchanged open
    // flag across one update proves no transition began during it.
    SamusArmCannonState cannon = samus.ArmCannon;
    cannon.Update(samus);
    AssertEqual(0, cannon.Frame,
        "first closed HUD sample leaves cannon invisible");
    cannon.Update(samus);
    AssertEqual(0, cannon.Frame,
        "second closed HUD sample agrees with already-closed state");
    AssertEqual(2, cannon.StableSelectionUpdates,
        "stable HUD selection saturates arm-cannon toggle at two");
    AssertEqual(SamusArmCannonDrawOrder.AfterBody, cannon.DrawOrder,
        "pose record publishes after-body arm-cannon draw order");

    samus.SelectedHudItem = HudItem.Missile;
    bool openBeforeSelection = cannon.IsOpen;
    cannon.Update(samus);
    AssertTrue(cannon.StableSelectionUpdates == 1 && cannon.IsOpen == openBeforeSelection,
        "new missile selection waits one stable HUD frame");
    AssertEqual(0, cannon.Frame,
        "selection-change frame keeps cannon closed");

    bool openBeforeOpening = cannon.IsOpen;
    cannon.Update(samus);
    AssertTrue(!openBeforeOpening && cannon.IsOpen != openBeforeOpening,
        "second missile sample begins opening");
    AssertEqual(1, cannon.Frame,
        "opening starts at zero and advances to frame one in the same call");
    AssertTrue(cannon.IsOpen, "missile selection opens the cover");
    AssertTrue(cannon.IsTransitioning, "opening frame one is still transitioning");

    cannon.Update(samus);
    AssertEqual(2, cannon.Frame, "opening advances to cover frame two");
    AssertTrue(cannon.IsTransitioning, "frame two remains transitional");
    cannon.Update(samus);
    AssertEqual(3, cannon.Frame, "opening clamps at cover frame three");
    AssertTrue(!cannon.IsTransitioning, "fully open cover ends the transition");

    // Rewind is intentionally unnecessary: selecting frame one in the public state is not
    // possible, which protects the model from debugger-only invalid combinations. The
    // fully-open draw still proves the same selector route and frame-indexed third tile.
    var coverFrame = new SpriteFrameBuffer();
    coverFrame.BeginFrame();
    cannon.Draw(coverFrame, samus, cameraPosition: new(0x0400, 0), frameCounter: 0);
    OamBuffer oam = coverFrame.FinalizeAsOam();
    AssertEqual(1, coverFrame.Draws.Length, "open cover publishes one draw");
    AssertEqual(1, oam.LastFinalizedSpriteCount, "cover stands for exactly one OAM record");
    OamEntry cover = oam.GetEntry(0);
    AssertEqual(135, cover.Position.X, "cover X includes signed pose offset and camera");
    AssertEqual(125, cover.Position.Y,
        "cover Y includes signed offset, graphics origin, and camera");
    // Direction two's retail attributes $281F: tile $1F, palette four, priority two, no flips.
    AssertEqual(0x1f, cover.TileNumber, "direction two attributes select cover OAM tile slot");
    AssertEqual(4, cover.Palette, "direction two attributes select cover OAM palette");
    AssertEqual(2, cover.Priority, "direction two attributes select cover OAM priority");
    AssertTrue(!cover.FlipX && !cover.FlipY, "direction two attributes leave cover unflipped");
    AssertTrue(!cover.IsLarge, "arm-cannon cover is a small OBJ");
    // Natively the third cover tile ($9A:9E00) is uploaded to tile $1F; the draw shows the sheet frame composed over it.
    SpriteArtwork coverTile = SheetFrameSources.CharacterDraw((ImageSpriteDraw)coverFrame.Draws[0]).Artwork;
    AssertEqual(new SpriteArtworkId(SpriteArtworkKind.SamusArmCannonCover,
            SamusArmCannonArtworkFormat.TileSourcePointers.IndexOf(0x9e00)), coverTile.Id,
        "pose record selects direction two, whose frame three binds the third cover tile");
    AssertTrue(coverCharacters.TryResolveTile(0x9a9e00, SpriteArtwork.CharacterByteCount, out ReadOnlyMemory<byte> nativeCover) &&
            coverTile.Characters.SequenceEqual(nativeCover.Span),
        "the bound cover character is the installed tile the native $9A:9E00 upload names");

    // Odd invincibility frames publish no cover draw; an off-screen cover is still published, for the display to clip (#1587).
    samus.InvincibilityTimer = WrappingCountdown.Start(1);
    OamBuffer flickerOam = SpriteFrameBuffer.DrawnOam(frame => cannon.Draw(frame, samus, new(0x0400, 0), frameCounter: 1));
    AssertEqual(0, flickerOam.LastFinalizedSpriteCount, "odd invincibility frame suppresses the cover draw");

    samus.InvincibilityTimer = WrappingCountdown.Start(0);
    var offScreenFrame = new SpriteFrameBuffer();
    offScreenFrame.BeginFrame();
    cannon.Draw(offScreenFrame, samus, cameraPosition: new(0x0500, 0), frameCounter: 0);
    offScreenFrame.FinalizeFrame();
    AssertEqual(1, offScreenFrame.Draws.Length, "off-screen cover is still published");
    AssertEqual(CoordinateAxis.AsSigned(coverFrame.Draws[0].Origin.X) - 256, CoordinateAxis.AsSigned(offScreenFrame.Draws[0].Origin.X),
        "off-screen cover lies a screen left of the view, where the camera's extra 256 pixels place it");
    AssertEqual(coverFrame.Draws[0].Origin.Y, offScreenFrame.Draws[0].Origin.Y, "off-screen cover keeps its row");

    // Closing mirrors opening but starts from synthetic frame four. The same stable-sample
    // call decrements immediately to three, followed by two, one, and invisible zero.
    samus.SelectedHudItem = HudItem.Beam;
    bool openBeforeDeselection = cannon.IsOpen;
    cannon.Update(samus);
    AssertTrue(cannon.StableSelectionUpdates == 1 && cannon.IsOpen == openBeforeDeselection,
        "item deselection also waits one stable frame");
    bool openBeforeClosing = cannon.IsOpen;
    cannon.Update(samus);
    AssertTrue(openBeforeClosing && !cannon.IsOpen,
        "second empty-item sample begins closing");
    AssertEqual(3, cannon.Frame, "closing begins visibly at frame three");
    cannon.Update(samus);
    AssertEqual(2, cannon.Frame, "closing decrements to frame two");
    cannon.Update(samus);
    AssertEqual(1, cannon.Frame, "closing decrements to frame one");
    cannon.Update(samus);
    AssertEqual(0, cannon.Frame, "closing reaches invisible frame zero");
    AssertTrue(!cannon.IsTransitioning, "fully closed cover ends the transition");

    Console.WriteLine(
        "  Samus arm cannon: HUD debounce, open/close cadence, cover draw, clipping, flicker, and cover binding agree.");
}

/// <summary>
/// Proves the byte-aliased `$0A72/$0A73` visor timer, all three room-cycle colors, normal-
/// room reset, X-ray exclusion, and the inactive-charge call seam.
/// </summary>
internal static void VerifySamusVisorPalette()
{
    var bus = new TestAddressSpace();
    var samusColours = new SamusColours();
    ushort[] colors = [0x1000, 0x1001, 0x1002, 0x2000, 0x2001, 0x2002];
    WriteVisorColors(bus, colors);

    var visorColors = SamusVisorColorFile.Read(new MemoryStream(SuperMetroid.AssetExtraction.Samus.SamusVisorColorExtractor.Extract(bus)));
    // The visor cycle names shades (#1512); the import resolves them from the diagnostic visor colours.
    var sources = InstalledFixtures.SamusShadeSources with { Visor = visorColors };
    var state = new SamusVisorPaletteState();
    samusColours.ShowVisor(SamusShades.DoorCompletionVisor);
    state.Update(
        samusColours, specialSamusPaletteType: 0,
        roomVisorAnimation: RoomVisorAnimation.Steady);
    AssertEqual(new VerifySamusVisorPaletteCase(1, 3), new VerifySamusVisorPaletteCase(state.Timer, state.NextShade),
        "ordinary room resets visor animation, priming timer one and the cycle's first shade");
    AssertEqual(SamusShades.DoorCompletionVisor, samusColours.VisorShade,
        "ordinary room reset does not overwrite current visor shade");

    // The first call in a room that animates the visor decrements timer one to zero and immediately copies offset six.
    state.Update(
        samusColours, 0, RoomVisorAnimation.Animated);
    AssertTrue(samusColours.VisorShade != SamusShades.DoorCompletionVisor,
        "backdrop room shows the first visor shade immediately");
    AssertEqual(SamusShades.Visor(3), samusColours.VisorShade, "first visor shade is the cycle's first");
    AssertEqual(0x2000, samusColours.ShownVisor(sources), "first backdrop visor color");
    AssertEqual(new VerifySamusVisorPaletteCase(5, 4), new VerifySamusVisorPaletteCase(state.Timer, state.NextShade),
        "first write reloads five and advances to the cycle's second shade");

    // Four calls retain timers 4/3/2/1. The fifth reaches zero, writes, and reloads five.
    for (ushort expectedTimer = 4; expectedTimer >= 1; expectedTimer--)
    {
        PictureShade? shadeShownBeforeCountdown = samusColours.VisorShade;
        int shadeBeforeCountdown = state.NextShade;
        state.Update(
            samusColours, 0, RoomVisorAnimation.Animated);
        AssertTrue(samusColours.VisorShade == shadeShownBeforeCountdown &&
                state.NextShade == shadeBeforeCountdown,
            $"visor countdown timer {expectedTimer} writes no color and keeps its shade");
        AssertEqual(expectedTimer, state.Timer,
            $"visor countdown reaches {expectedTimer}");
        if (expectedTimer == 1)
            break;
    }
    state.Update(
        samusColours, 0, RoomVisorAnimation.Animated);
    AssertEqual(SamusShades.Visor(4), samusColours.VisorShade, "second visor shade is the cycle's second");
    AssertEqual(0x2001, samusColours.ShownVisor(sources), "second backdrop visor color");
    AssertEqual(new VerifySamusVisorPaletteCase(5, 5), new VerifySamusVisorPaletteCase(state.Timer, state.NextShade),
        "second write advances to the cycle's last shade");

    for (int call = 0; call < 5; call++)
        state.Update(samusColours, SamusSpecialPaletteType.None, RoomVisorAnimation.Animated);
    AssertEqual(0x2002, samusColours.ShownVisor(sources), "third backdrop visor color");
    AssertEqual(new VerifySamusVisorPaletteCase(5, 3), new VerifySamusVisorPaletteCase(state.Timer, state.NextShade),
        "third write wraps only to the room cycle's first shade");

    VerifySamusVisorPaletteCase beforeXray = new VerifySamusVisorPaletteCase(state.Timer, state.NextShade);
    samusColours.ShowVisor(SamusShades.Visor(1));
    state.Update(
        samusColours, SamusSpecialPaletteType.Xray, RoomVisorAnimation.Animated);
    AssertTrue(new VerifySamusVisorPaletteCase(state.Timer, state.NextShade) == beforeXray && samusColours.VisorShade == SamusShades.Visor(1),
        "X-ray special handler suppresses ordinary visor cycle");
    AssertEqual(beforeXray, new VerifySamusVisorPaletteCase(state.Timer, state.NextShade),
        "X-ray suppression freezes the countdown and the shade");
    AssertEqual(SamusShades.Visor(1), samusColours.VisorShade,
        "X-ray suppression preserves its independently owned shade");

    // `HandleBeamChargePalettes` reaches the visor only through its no-charge branch.
    // This integration assertion prevents the exact state machine from becoming orphaned.
    var integratedSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
    var projectiles = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
    SamusBeamChargePaletteStepResult charge = projectiles.UpdateBeamChargePalette(
        samusColours, integratedSamus,
        roomVisorAnimation: RoomVisorAnimation.Animated);
    AssertEqual(SamusBeamChargePaletteAction.Inactive, charge.Action,
        "inactive charging retains beam-palette result");
    AssertEqual(new VerifySamusVisorPaletteCase(5, 4), new VerifySamusVisorPaletteCase(integratedSamus.VisorPalette.Timer, integratedSamus.VisorPalette.NextShade),
        "inactive charging falls through to visor handler, which writes and advances");
    AssertEqual(0x2000, samusColours.ShownVisor(sources),
        "integrated visor call reads bank-$9B room-cycle color");

    Console.WriteLine(
        "  Samus visor: packed timer/index, backdrop cycle, normal reset, and X-ray exclusion agree.");
}

/// <summary>
/// Exercises the complete ordinary `$91:D8AA-$D953` hurt-counter lifetime with diagnostic
/// palette records. This intentionally verifies every call rather than three hand-picked
/// frames: an off-by-one at seven, forty, or sixty would otherwise look visually plausible.
/// </summary>
internal static void VerifySamusHurtFlashPalette()
{
    var bus = new TestAddressSpace();
    var samusColours = new SamusColours();

    SeedHurtFlashPalettes(bus);

    var hurtColors = SamusHurtColorFile.Read(new MemoryStream(SuperMetroid.AssetExtraction.Samus.SamusHurtColorExtractor.Extract(bus)));
    var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        EquippedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit), // Both suit bits prove Gravity's native precedence.
    }.WithHurtFlashCount(1);
    // The flash names shades (#1512); the import resolves them from the diagnostic hurt and suit colours.
    var hurtSources = InstalledFixtures.SamusShadeSources with
    {
        Hurt = hurtColors,
        Suits = SamusSuitColorFile.Read(new MemoryStream(SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(bus))),
    };

    int hurtPaletteCalls = 0;
    int normalPaletteCalls = 0;
    int untouchedCalls = 0;
    for (int call = 1; call <= 59; call++)
    {
        int counterBefore = samus.HurtFlash.Count;
        RgbColor[] paletteBefore = samusColours.ShownSuit(hurtSources);
        int soundsBefore = samus.LiquidPhysics.SoundRequests.Count;
        SamusHurtFlashPalette.Update(
            samusColours, samus, controllerInput: 0);
        RgbColor[] suitShown = samusColours.ShownSuit(hurtSources);
        bool paletteChanged = !paletteBefore.SequenceEqual(suitShown);
        AssertEqual(call, counterBefore,
            $"hurt palette call {call} reads pre-increment counter");

        if (call <= 6 && (call & 1) != 0)
        {
            hurtPaletteCalls++;
            AssertTrue(paletteChanged,
                $"odd hurt call {call} selects fixed flash palette");
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(unchecked((ushort)(0x4000 + color)), suitShown[color],
                    $"hurt call {call} copies flash color {color}");
            }
        }
        else if (call <= 6)
        {
            normalPaletteCalls++;
            AssertTrue(paletteChanged,
                $"even hurt call {call} restores equipment palette");
            AssertEqual(0x0300, suitShown[0],
                $"hurt call {call} gives Gravity ($9B:9800) priority over Varia");
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(unchecked((ushort)(0x0300 + color)), suitShown[color],
                    $"hurt call {call} copies Gravity color {color}");
            }
        }
        else
        {
            untouchedCalls++;
            AssertTrue(!paletteChanged,
                $"hurt call {call} preserves the existing palette");
        }

        if (call == 2)
        {
            AssertEqual(soundsBefore + 1, samus.LiquidPhysics.SoundRequests.Count,
                "hurt call two publishes impact SFX");
            AssertEqual(new SoundRequest(SamusSound.SamusDamaged, SoundRequestPriority.Normal),
                samus.LiquidPhysics.SoundRequests[^1],
                "hurt impact uses library one sound $35 maximum six");
        }
    }

    AssertEqual(3, hurtPaletteCalls, "hurt lifetime has three flash writes");
    AssertEqual(3, normalPaletteCalls, "hurt lifetime has three suit restores");
    AssertEqual(53, untouchedCalls, "hurt lifetime has fifty-three preserving calls");
    AssertEqual(0, samus.HurtFlash.Count,
        "hurt counter clears when call fifty-nine increments it to sixty");
    AssertEqual(1, samus.LiquidPhysics.SoundRequests.Count,
        "ordinary hurt lifetime queues impact sound only once");

    // Cinematic call two suppresses impact audio and uses the dedicated intro palette in
    // place of equipment-selected colors. Odd call one remains the common hurt palette.
    var cinematic = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { EquippedItems = SamusEquipmentFlags.GravitySuit }.WithHurtFlashCount(2);
    cinematic.LiquidPhysics.CinematicActive = true;
    SamusHurtFlashPalette.Update(
        samusColours, cinematic, controllerInput: 0);
    RgbColor[] cinematicSuit = samusColours.ShownSuit(hurtSources);
    AssertEqual(0x5000, cinematicSuit[0],
        "cinematic even hurt call selects intro palette");
    AssertEqual(0, cinematic.LiquidPhysics.SoundRequests.Count,
        "cinematic hurt call suppresses impact SFX, leaving sound queue empty");
    for (int color = 0; color < 16; color++)
    {
        AssertEqual(unchecked((ushort)(0x5000 + color)), cinematicSuit[color],
            $"cinematic restore copies intro color {color}");
    }

    // Counter forty calls command `$1C` for spin/wall-jump movement. A Screw Attack pose
    // must select `$33`; using the ROM movement-type byte keeps this a real dispatcher test.
    WritePoseDefinition(bus, SamusPoseNumbers.Number(SamusPoseId.ScrewAttackRightPose),
        [0x08, 0x03, 0, 0, 0, 0, 0x15, 0]);
    var spinning = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
    {
        Pose = SamusPoseId.ScrewAttackRightPose,
    }.WithHurtFlashCount(39);
    SamusHurtFlashPalette.Update(
        samusColours, spinning, controllerInput: 0);
    AssertEqual(1, spinning.LiquidPhysics.SoundRequests.Count,
        "counter forty restores Screw Attack sound");
    AssertEqual(new SoundRequest(SamusSound.ScrewAttack, SoundRequestPriority.High),
        spinning.LiquidPhysics.SoundRequests[^1],
        "Screw Attack recovery uses library one maximum nine");

    // A non-spinning charged shot arms the native one-word latch. The post-draw consumer
    // queues `$41` only while Shoot is still held, then clears the latch in either case.
    WritePoseDefinition(bus, SamusPoseNumbers.Number(SamusPoseId.FacingRightNormalPose),
        [0x08, 0x00, 0, 0, 0, 0, 0x15, 0]);
    var charging = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
    {
        Pose = SamusPoseId.FacingRightNormalPose,
    }.WithBeamChargeCount(0x10).WithHurtFlashCount(39);
    SamusHurtFlashPalette.Update(
        samusColours, charging, ControllerButton.X);
    AssertEqual(0, charging.LiquidPhysics.SoundRequests.Count,
        "counter forty arms charging-beam recovery without an immediate sound");
    AssertTrue(charging.ResumeChargingBeamSoundPending,
        "charging recovery publishes native flag one");
    AssertTrue(SamusHurtFlashPalette.ConsumeResumeChargingBeamSound(
            charging, ControllerButton.X),
        "post-draw handler queues held charging sound");
    AssertTrue(!charging.ResumeChargingBeamSoundPending,
        "post-draw handler clears charging recovery flag");
    AssertEqual(new SoundRequest(SamusSound.ResumeChargingBeam, SoundRequestPriority.High),
        charging.LiquidPhysics.SoundRequests[^1],
        "charging recovery queues library one sound $41 maximum nine");

    // Grapple's pointer comparison accepts wall-grab release because its native handler is
    // still below `$C856`; cancel-pending is exactly the cutoff and must remain silent.
    var grapple = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()).WithHurtFlashCount(39);
    grapple.Grapple.Phase = GrapplePhase.WallGrabRelease;
    SamusHurtFlashPalette.Update(
        samusColours, grapple, controllerInput: 0);
    AssertEqual(1, grapple.LiquidPhysics.SoundRequests.Count,
        "counter forty restores pre-cancel grapple sound");
    AssertEqual(new SoundRequest(SamusSound.Grappling, SoundRequestPriority.High),
        grapple.LiquidPhysics.SoundRequests[^1],
        "grapple recovery uses library one sound six maximum nine");

    var cancelledGrapple = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()).WithHurtFlashCount(39);
    cancelledGrapple.Grapple.Phase = GrapplePhase.CancelPending;
    SamusHurtFlashPalette.Update(
        samusColours, cancelledGrapple, controllerInput: 0);
    AssertTrue(!cancelledGrapple.ResumeChargingBeamSoundPending,
        "grapple cancel cutoff suppresses recovery sound and charging latch");
    AssertEqual(0, cancelledGrapple.LiquidPhysics.SoundRequests.Count,
        "cancelled grapple leaves sound queue empty");

    Console.WriteLine("  Samus hurt flash: full palette lifetime and impact/recovery sounds agree.");
}

/// <summary>
/// Constructs the exact pointer topology and meaningful bytes for retail pose $01 frame 0.
/// The graphics payload uses diagnostic values because correctness here is about routing;
/// the DebugRunner separately executes the same code against the user's actual cartridge.
/// </summary>
internal static void SeedPoseOneSamusData(TestAddressSpace bus)
{
    // Pose definition $91:B631: facing right, standing movement type, +6 graphics Y.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);

    // Pose 1's delay-table pointer and its complete healthy/low-health bytecode stream.
    WriteTestWord(bus, 0x91b012, 0xb298);
    bus.WriteBytes(0x91b298, [0x0a, 0x0a, 0x0a, 0x0a, 0xf6, 0x08, 0x08, 0x08, 0x08, 0xfe, 0x04]);

    // Pose-to-spritemap base indices and the two pointer-table entries they resolve.
    WriteTestWord(bus, 0x929265, 0x019a);
    WriteTestWord(bus, 0x92945f, 0x04aa);
    WriteTestWord(bus, 0x9283c1, 0xa072);
    WriteTestWord(bus, 0x9289e1, 0xadbb);

    // Top spritemap $92:A072: four pieces. The assembler macro ORs size bit $8000
    // into X values such as $43F9, yielding the deliberately odd encoded word $C3F9.
    bus.WriteBytes(0x92a072, [
        0x04, 0x00,
        0xf9, 0xc3, 0xf8, 0x00, 0x28,
        0xf9, 0xc3, 0xf0, 0x02, 0x28,
        0x0a, 0x00, 0xfd, 0x04, 0x28,
        0x02, 0x00, 0xfd, 0x05, 0x28,
    ]);

    // Bottom spritemap $92:ADBB: three large pieces using tile names $08/$0A/$0C.
    bus.WriteBytes(0x92adbb, [
        0x03, 0x00,
        0xf1, 0xc3, 0x10, 0x08, 0x28,
        0xf9, 0xc3, 0x10, 0x0a, 0x28,
        0xf9, 0xc3, 0x00, 0x0c, 0x28,
    ]);

    // Pose 1 -> animation list $DB48; frame 0 -> top set 7/position C and bottom
    // set 0/position 6. Their list pointers plus position*7 produce D0B0 and D1C8.
    WriteTestWord(bus, 0x92d950, 0xdb48);
    bus.WriteBytes(0x92db48, [0x07, 0x0c, 0x00, 0x06]);
    WriteTestWord(bus, 0x92d92c, 0xd05c);
    WriteTestWord(bus, 0x92d938, 0xd19e);
    bus.WriteBytes(0x92d0b0, [0x00, 0xe1, 0x9c, 0xc0, 0x00, 0x80, 0x00]);
    bus.WriteBytes(0x92d1c8, [0x20, 0x88, 0x9d, 0xc0, 0x00, 0xc0, 0x00]);

    // Distinct first bytes prove each of the four fixed NMI destinations independently.
    WriteTestByte(bus, 0x9ce100, 0x10);
    WriteTestByte(bus, 0x9ce1c0, 0x20);
    WriteTestByte(bus, 0x9d8820, 0x30);
    WriteTestByte(bus, 0x9d88e0, 0x40);

    ushort[] powerSuitColors = [
        0x3800, 0x0108, 0x03bd, 0x1405, 0x3be0, 0x21a8, 0x579f, 0x4ad2,
        0x3a4e, 0x00bb, 0x02b5, 0x016b, 0x0252, 0x1104, 0x0074, 0x000d,
    ];
    WriteTestWords(bus, 0x9b9400, powerSuitColors);
}

/// <summary>
/// Seeds the two front-view pose records with compact diagnostic spritemaps. The base
/// indices, pose metadata, and raw chest-cover attributes are retail values; the one-piece
/// body maps keep this verifier focused on routing because DebugRunner covers real artwork.
/// </summary>
static void SeedForwardFacingSamusData(TestAddressSpace bus)
{
    // Both records face neither left nor right, use standing movement type zero, have an
    // eight-pixel graphics-origin offset, and use the front-view radius of 24 pixels.
    WritePoseDefinition(bus, 0, [0x00, 0x00, 0xff, 0xff, 0x08, 0x00, 0x18, 0x00]);
    bus.WriteBytes(0x91bb01, [0x00, 0x00, 0xff, 0xff, 0x08, 0x00, 0x18, 0x00]);

    // `$00/$9B` share `$91:B56F`: eight ticks on frame zero followed by command `$FF`.
    WriteTestWord(bus, 0x91b010, 0xb56f);
    WriteTestWord(bus, 0x91b146, 0xb56f);
    bus.WriteBytes(0x91b56f, [0x08, 0xff]);

    // Retail frame-zero bases: `$00` top/bottom `$0002/$0062`, `$9B` `$00C2/$0122`.
    WriteTestWord(bus, 0x929263, 0x0002);
    WriteTestWord(bus, 0x92945d, 0x0062);
    WriteTestWord(bus, 0x929399, 0x00c2);
    WriteTestWord(bus, 0x929593, 0x0122);

    // Point those four indices at one-piece diagnostic maps. The direct `$3821` chest
    // record is not included here: production Draw must append it independently.
    WriteTestWord(bus, 0x928091, 0xa200);
    WriteTestWord(bus, 0x928151, 0xa210);
    WriteTestWord(bus, 0x928211, 0xa220);
    WriteTestWord(bus, 0x9282d1, 0xa230);
    bus.WriteBytes(0x92a200, [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28]);
    bus.WriteBytes(0x92a210, [0x01, 0x00, 0x00, 0x00, 0x10, 0x08, 0x28]);
    bus.WriteBytes(0x92a220, [0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x28]);
    bus.WriteBytes(0x92a230, [0x01, 0x00, 0x00, 0x00, 0x10, 0x09, 0x28]);

    // Draw's final tile-selection step follows each pose's four-byte frame record. Zeroed
    // set pointers are safe because this verifier does not execute the resulting DMA.
    WriteTestWord(bus, 0x92d94e, 0xe000);
    WriteTestWord(bus, 0x92da84, 0xe004);
    bus.WriteBytes(0x92e000, [0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0x00]);
}

/// <summary>Seeds the retail pose-$09 pointer topology with compact diagnostic spritemaps.</summary>
static void SeedPoseNineSamusData(TestAddressSpace bus)
{
    // Pose $09: facing right, movement type one, new-pose-unless-buttons $01, +6 graphics
    // offset, and radius 21. These are the eight retail bytes at $91:B671.
    bus.WriteBytes(0x91b671, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);

    // Ten alternating 2/3-tick running frames followed by command $FF back to frame zero.
    WriteTestWord(bus, 0x91b022, 0xb20a);
    bus.WriteBytes(0x91b20a, [0x02, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0x03, 0xff]);

    // Retail frame-zero bases are top $00F9 and bottom $00E3. Their pointer slots select
    // compact one-piece diagnostic maps; production DebugRunner reads the real maps.
    WriteTestWord(bus, 0x929275, 0x00f9);
    WriteTestWord(bus, 0x92946f, 0x00e3);
    WriteTestWord(bus, 0x92827f, 0xa100);
    WriteTestWord(bus, 0x928253, 0xa108);
    bus.WriteBytes(0x92a100, [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28]);
    bus.WriteBytes(0x92a108, [0x01, 0x00, 0x00, 0x08, 0x08, 0x28, 0x28]);

    // Reuse pose one's already-seeded diagnostic tile definitions by making pose $09 frame
    // zero choose the same top set/position and bottom set/position record.
    WriteTestWord(bus, 0x92d960, 0xdc48);
    bus.WriteBytes(0x92dc48, [0x07, 0x0c, 0x00, 0x06]);
}

/// <summary>
/// Gives every spritemap index the body-half matrix uses one visible OBJ, and seeds the
/// posture-transition Y offsets that matrix positions against.
/// </summary>
static void SeedBodyHalfRoutingSpritemaps(TestAddressSpace bus)
{
    bus.WriteBytes(0x92e000, [1, 0, 0, 0, 0, 0x00, 0x28]);
    for (int index = 0; index <= 3; index++)
        WriteTestWord(bus, SamusSpritemapSources.PointerTableAddress + index * 2, 0xe000);
    bus.WriteBytes(SamusSourceDefinitions.PostureTransitionVerticalOffsets.Value, [
        0xf8, 0x00, 0xf8, 0x00,
        0xfc, 0xfe, 0xfc, 0xfe,
        0x00, 0x00, 0x00, 0x00,
        0xfc, 0x00, 0xfc, 0x00,
        0x05, 0x04, 0x05, 0x04,
        0x00, 0x00, 0x00, 0x00,
    ]);
}

/// <summary>
/// Every diagnostic pose in the body-half matrix needs a valid upper-LoROM tile-animation
/// record; a zero pointer would address $92:0000, which the cartridge does not map. Tile
/// selection stays constant so the matrix tests body-half routing only.
/// </summary>
static void SeedBodyHalfRoutingTileAnimation(TestAddressSpace bus)
{
    for (int frame = 0; frame < 4; frame++)
        bus.WriteBytes(0x92e100 + frame * 4, [7, 12, 0xff, 0]);
}

/// <summary>Seeds the byte-packed <c>$90:8D28</c> landing vertical-offset table.</summary>
static void WriteLandingVerticalOffsets(TestAddressSpace bus, ReadOnlySpan<byte> offsets) =>
    bus.WriteBytes(SamusSourceDefinitions.LandingVerticalOffsets.Value, offsets);

/// <summary>Seeds the six <c>$9B:A3C0</c> visor room-cycle colors.</summary>
static void WriteVisorColors(TestAddressSpace bus, ushort[] colors) =>
    WriteTestWords(bus, SamusSourceDefinitions.VisorColors.Value, colors);

/// <summary>
/// Seeds all sixteen Power/Varia/Gravity suit, hurt-flash and intro colors distinctly so a
/// partial ten-color copy cannot pass.
/// </summary>
static void SeedHurtFlashPalettes(TestAddressSpace bus)
{
    for (int color = 0; color < 16; color++)
    {
        WriteTestWord(bus, SamusSourceDefinitions.PowerSuitPalette.Value + color * 2, unchecked((ushort)(0x0100 + color)));
        WriteTestWord(bus, SamusSourceDefinitions.VariaSuitPalette.Value + color * 2, unchecked((ushort)(0x0200 + color)));
        WriteTestWord(bus, SamusSourceDefinitions.GravitySuitPalette.Value + color * 2, unchecked((ushort)(0x0300 + color)));
        WriteTestWord(bus, SamusSourceDefinitions.HurtFlashColors.Value + color * 2, unchecked((ushort)(0x4000 + color)));
        WriteTestWord(bus, SamusSourceDefinitions.IntroHurtFlashColors.Value + color * 2, unchecked((ushort)(0x5000 + color)));
    }
}

    /// <summary>Runs the arm cannon sprite routing and animation fixture.</summary>
    [VerificationCommand("--arm-cannon-fixture")]
    internal static int ArmCannonFixtureCommand()
    {
     VerifySamusArmCannon(); return 0; 
    }

    /// <summary>Runs the visor, hurt flash, and visor-color palette fixtures.</summary>
    [VerificationCommand("--samus-palette-fixtures")]
    internal static int SamusPaletteFixturesCommand()
    {
     VerifySamusVisorPalette(); VerifySamusHurtFlashPalette(); VerifySamusVisorColors(); return 0; 
    }

    /// <summary>Runs the standing Samus body and tile-transfer rendering slice.</summary>
    [VerificationCommand("--samus-rendering-slice")]
    internal static int SamusRenderingSliceCommand()
    {
        Suite(nameof(VerifySamusRenderingSlice), () => VerifySamusRenderingSlice());
        return 0;
    }
}
