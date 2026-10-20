using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Samus.Death.SamusDeathExplosionTimingDefinitionsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Verifies Samus's Crystal Flash, X-ray, and death state machines with focused cartridge fixtures.</summary>
internal static class SamusSpecialStatesVerification
{

/// <summary>The largest component of the stock fixed-color row three at $88:8D85, which bounds the native afterglow.</summary>
[CpuBusBoundary("exact bus test: retail ROM oracle for the Crystal Flash fixed-color row")]
static int ReadRetailCrystalFlashStockFadeSteps()
{
    var nativeColors = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc"));
    return Enumerable.Range(0, 3).Max(component =>
        nativeColors.ReadCartridgeByte(0x888d85 + 3 * 3 + component));
}

/// <summary>Seeds the Crystal Flash pose records, delay streams, palette records and Power Bomb fade color.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for Crystal Flash poses, animation and palettes")]
static void SeedCrystalFlashCartridge(TestAddressSpace bus)
{
    // Only bytes zero, one, and six matter to this fixture: facing, movement type, and Y
    // radius. The production implementation still reads them through the actual ROM table
    // addresses instead of receiving test-only pose metadata.
    WritePoseDefinition(bus, (int)SamusPoseId.FacingRightNormalPose,
        [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.FacingLeftNormalPose,
        [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.CrystalFlashRightPose,
        [0x08, 0x1b, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.CrystalFlashLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]);

    WriteTestWord(bus, 0x91b010 + (int)SamusPoseId.FacingRightNormalPose * 2, 0xb600);
    WriteTestWord(bus, 0x91b010 + (int)SamusPoseId.FacingLeftNormalPose * 2, 0xb601);
    WriteTestWord(bus, 0x91b010 + (int)SamusPoseId.CrystalFlashRightPose * 2, 0xb545);
    WriteTestWord(bus, 0x91b010 + (int)SamusPoseId.CrystalFlashLeftPose * 2, 0xb556);
    bus.WriteBytes(0x91b600, [0x05, 0x05]);
    bus.WriteBytes(0x91b545, [
        0x03, 0x03, 0x01, 0x01, 0xfe, 0x02,
        0x0c, 0x0c, 0x0c, 0x0c, 0xfe, 0x04,
        0x03, 0x03, 0x03, 0xfd, 0x01,
    ]);
    bus.WriteBytes(0x91b556, [
        0x03, 0x03, 0x01, 0x01, 0xfe, 0x02,
        0x0c, 0x0c, 0x0c, 0x0c, 0xfe, 0x04,
        0x03, 0x03, 0x03, 0xfd, 0x02,
    ]);

    // Palette handler seven splits sprite palette six into ten body colors and six bubble
    // colors. Distinct synthetic records expose both destination ranges, their independent
    // timers, and `$90:ACC2`'s full beam-palette restoration at finish.
    for (int frame = 0; frame < CrystalFlashColorFormat.BodyFrameCount; frame++)
        WriteTestWord(bus, SamusPaletteRomData.CrystalFlash.BodyRecords + frame * 4, 0x9500);
    for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
        WriteTestWord(bus, SamusPaletteRomData.CrystalFlash.BubblePointers + frame * 2, 0x9620);
    WriteTestWord(bus, 0x91dc02, 10);
    WriteTestWord(bus, 0x91dc28, 0x9600);
    WriteTestWord(bus, 0x91dc2a, 0x9620);
    for (int selection = 0; selection < BeamTileAtlasDefinitions.SelectionCount; selection++)
        WriteTestWord(bus, SamusProjectileRomData.Beams.PalettePointers + selection * 2, 0x9700);
    for (ushort color = 0; color < 10; color++)
        WriteTestWord(bus, 0x9b9500 + color * 2, unchecked((ushort)(0x0100 + color)));
    for (ushort color = 0; color < 6; color++)
    {
        WriteTestWord(bus, 0x9b9600 + color * 2, unchecked((ushort)(0x0200 + color)));
        WriteTestWord(bus, 0x9b9620 + color * 2, unchecked((ushort)(0x0220 + color)));
    }
    for (ushort color = 0; color < 16; color++)
        WriteTestWord(bus, 0x909700 + color * 2, unchecked((ushort)(0x0300 + color)));

    // The final Crystal Flash explosion frame selects color index three from the shared
    // bank-$88 Power Bomb table. Components 4/3/2 make its exact four-call fade cadence
    // observable without depending on a second copy of the production state machine.
    bus.WriteBytes(0x888d85 + 3 * 3, [0x04, 0x03, 0x02]);
}

/// <summary>Checks Crystal Flash admission, ammo drain, pose timing, body and bubble colors, and release.</summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
internal static void VerifySamusCrystalFlash()
{
    var bus = new TestAddressSpace();

    SeedCrystalFlashCartridge(bus);

    var crystalColors = CrystalFlashColorCatalog.Load(new MemoryStream(
        SuperMetroid.AssetExtraction.Samus.CrystalFlashColorExtractor.Extract(bus)));
    var beamPalettes = BeamPaletteCatalog.Load(new MemoryStream(
        SuperMetroid.AssetExtraction.Samus.BeamPaletteExtractor.Extract(bus)));
    const SnesButton chord = (SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);
    var samus = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(0x0120, 0x0080)),
        Health = 1,
        MaxHealth = 99,
        Missiles = 10,
        SuperMissiles = 10,
        PowerBombs = 10,
        InvincibilityTimer = NativeCountdown.Start(96),
    };
    samus.PoseMetadata.RefreshCollisionRadii();

    samus.PoseHistory.PreviousPose = SamusPoseId.FacingRightNormalPose;
    samus.PoseHistory.PreviousDirectionAndMovement = 8;
    samus.PoseHistory.LastDifferentPose = SamusPoseId.SpinJumpLeftPose;
    samus.PoseHistory.LastDifferentDirectionAndMovement = 0x0304;
    AssertTrue(!samus.CrystalFlash.TryBegin(samus, chord | SnesButton.A),
        "Crystal Flash rejects extra held input");
    AssertEqual(96, samus.InvincibilityTimer.RawValue, "rejected Crystal Flash preserves hit immunity");
    samus.Kinematics.YSpeed = samus.Kinematics.YSpeed.WithFraction(1);
    AssertTrue(!samus.CrystalFlash.TryBegin(samus, chord),
        "Crystal Flash rejects fractional vertical movement");
    samus.Kinematics.YSpeed = samus.Kinematics.YSpeed.WithFraction(0);
    AssertEqual(SamusPoseId.SpinJumpLeftPose, samus.PoseHistory.LastDifferentPose,
        "rejected Crystal Flash must not shift history");
    AssertTrue(samus.CrystalFlash.TryBegin(samus, chord),
        "Crystal Flash accepts exact chord and resources");
    AssertEqual(0, samus.InvincibilityTimer.RawValue, "Crystal Flash activation clears native hit immunity");
    AssertEqual(SamusPoseId.CrystalFlashRightPose, samus.Pose,
        "source direction selects right Crystal Flash pose");
    AssertEqual(SamusPoseId.FacingRightNormalPose, samus.PoseHistory.LastDifferentPose, "Crystal Flash shifts previous pose");
    AssertEqual(8, samus.PoseHistory.LastDifferentDirectionAndMovement, "Crystal Flash shifts previous metadata");
    AssertEqual(SamusPoseId.CrystalFlashRightPose, samus.PoseHistory.PreviousPose, "Crystal Flash commits right pose");
    AssertEqual(0x1b08, samus.PoseHistory.PreviousDirectionAndMovement, "Crystal Flash commits right metadata");
    AssertEqual(CrystalFlashPhase.Raising, samus.CrystalFlash.Phase,
        "Crystal Flash installs raise handler");
    AssertEqual(SamusSpecialPaletteType.CrystalFlash, samus.CrystalFlash.SpecialPaletteKind,
        "Crystal Flash installs palette handler seven");

    var crystalCgram = new SnesCgram();
    AssertTrue(samus.CrystalFlash.UpdatePalette(crystalCgram, samus, beamPalettes, crystalColors),
        "Crystal Flash palette handler owns first visible frame");
    AssertEqual(0x0100, crystalCgram.Colors[0xe0],
        "Crystal Flash body palette begins at sprite palette-six color zero");
    AssertEqual(0x0109, crystalCgram.Colors[0xe9],
        "Crystal Flash body palette copies ten colors");
    AssertEqual(0x0200, crystalCgram.Colors[0xea],
        "Crystal Flash bubble palette begins at color ten");
    AssertEqual(0x0205, crystalCgram.Colors[0xef],
        "Crystal Flash bubble palette copies six colors");
    AssertEqual(5, samus.CrystalFlash.SpecialPaletteTimer.RawValue,
        "Crystal Flash bubble timer reloads five");
    AssertEqual(10, samus.CrystalFlash.CrystalPaletteTimer.RawValue,
        "Crystal Flash body timer comes from interleaved ROM record");
    AssertEqual(4, samus.CrystalFlash.BodyPaletteRecordOffset,
        "Crystal Flash body record advances four bytes");
    AssertEqual(2, samus.HorizontalSpeed.SpecialPaletteFrame,
        "Crystal Flash publishes aliased $0ACE palette frame");
    AssertEqual(4, samus.HorizontalSpeed.SpecialPaletteTimer.RawValue,
        "Crystal Flash publishes aliased $0AD0 record offset");

    // Five more calls expire only the bubble timer and select pointer one. The ten-call
    // body timer remains halfway through its first record.
    for (int paletteCall = 0; paletteCall < 5; paletteCall++)
        samus.CrystalFlash.UpdatePalette(crystalCgram, samus, beamPalettes, crystalColors);
    AssertEqual(0x0220, crystalCgram.Colors[0xea],
        "Crystal Flash bubble palette advances independently");
    AssertEqual(5, samus.CrystalFlash.CrystalPaletteTimer.RawValue,
        "Crystal Flash body palette retains independent countdown");

    ushort initialY = samus.Position.Y.Whole;
    samus.InvincibilityTimer = NativeCountdown.Start(77);
    for (int frame = 0; frame < 9; frame++)
    {
        samus.CrystalFlash.Step(samus, (ushort)frame);
        AssertEqual(CrystalFlashPhase.Raising, samus.CrystalFlash.Phase,
            $"raise frame {frame} retains start handler");
        AssertEqual(77, samus.InvincibilityTimer.RawValue, "ordinary raise handler does not clear immunity");
        samus.Presentation.AnimateNoFx(new SamusAnimationStep { Random = new CartridgeRandom(), ControllerInput = chord });
    }
    AssertEqual((initialY - 18), samus.Position.Y.Whole, "first nine raise calls move 18 pixels");

    samus.CrystalFlash.Step(samus, 9);
    AssertEqual(0, samus.InvincibilityTimer.RawValue, "raise completion clears hit immunity");
    AssertEqual(CrystalFlashPhase.DrainingAmmo, samus.CrystalFlash.Phase,
        "tenth raise call installs ammo handler");
    AssertEqual((initialY - 20), samus.Position.Y.Whole, "complete raise is 20 pixels");
    AssertEqual(samus.Position.Y.Whole, samus.CrystalFlash.RaisedPosition.Y,
        "raised Y capture follows tenth displacement");
    AssertEqual(6, samus.AnimationFrame, "raise transition forces animation frame six");
    AssertTrue(samus.CrystalFlash.BubbleHdmaRequested,
        "raise transition publishes Crystal Flash HDMA spawn seam");
    AssertTrue(samus.CrystalFlash.ConsumeActivationSoundRequest(),
        "Crystal Flash activation sound is consumable exactly once");
    AssertTrue(!samus.CrystalFlash.ConsumeActivationSoundRequest(),
        "consumed Crystal Flash sound cannot replay while ammo drains");

    var crystalWindow = new SamusPowerBombExplosionState
    {
        PresentationColors = PowerBombFixedColorCatalog.Load(new MemoryStream(
            SuperMetroid.AssetExtraction.Samus.PowerBombFixedColorExtractor.Extract(bus))),
    };
    crystalWindow.Arm();
    crystalWindow.BeginCrystalFlash(samus.Position.Whole);
    AssertTrue(!crystalWindow.IsArmed,
        "Crystal Flash spawn clears one-at-a-time Power Bomb flag");
    AssertEqual(PowerBombExplosionPhase.CrystalFlashExplosion, crystalWindow.Phase,
        "Crystal Flash installs bank-$88 stage-one pre-instruction");
    AssertEqual(0x0400, crystalWindow.ExplosionRadius.RawValue,
        "Crystal Flash bubble begins at radius four");

    // Seventeen calls remain below `$20.00`; call eighteen adds the old `$0330` speed,
    // reaches `$20B0`, and installs the afterglow. Its rendered table still used `$1D80`.
    for (int hdmaCall = 1; hdmaCall <= 17; hdmaCall++)
        AssertTrue(!crystalWindow.StepFrame(), $"Crystal Flash expansion call {hdmaCall}");
    AssertEqual(PowerBombExplosionPhase.CrystalFlashExplosion, crystalWindow.Phase,
        "Crystal Flash expansion remains active through call seventeen");
    AssertTrue(!crystalWindow.StepFrame(), "Crystal Flash expansion transition is not cleanup");
    AssertEqual(PowerBombExplosionPhase.CrystalFlashAfterglow, crystalWindow.Phase,
        "Crystal Flash call eighteen installs afterglow");
    AssertEqual(0x20b0, crystalWindow.ExplosionRadius.RawValue,
        "Crystal Flash transition radius preserves 8.8 acceleration sum");
    AssertEqual(0x1d80, crystalWindow.RenderedExplosionRadius.RawValue,
        "Crystal Flash transition renders pre-update radius");
    AssertEqual(4, crystalWindow.FixedColorRed,
        "Crystal Flash transition selects shared fixed-color entry three");

    // Editable RGB reaches black after four calls, but the native lifetime remains
    // tied to the stock color row. Compare that lifetime against the cartridge itself.
    int stockFadeSteps = ReadRetailCrystalFlashStockFadeSteps();
    for (int hdmaCall = 1; hdmaCall <= stockFadeSteps; hdmaCall++)
    {
        AssertTrue(!crystalWindow.StepFrame(), $"Crystal Flash afterglow call {hdmaCall}");
        AssertEqual(Math.Max(0, 4 - hdmaCall), crystalWindow.FixedColorRed,
            "custom Crystal Flash red fades without shortening native lifetime");
        AssertEqual(Math.Max(0, 3 - hdmaCall), crystalWindow.FixedColorGreen,
            "custom Crystal Flash green fade");
        AssertEqual(Math.Max(0, 2 - hdmaCall), crystalWindow.FixedColorBlue,
            "custom Crystal Flash blue fade");
    }
    AssertTrue(crystalWindow.StepFrame(),
        "Crystal Flash cleans up on the call after the native fade count");
    AssertEqual(PowerBombExplosionPhase.Inactive, crystalWindow.Phase,
        "Crystal Flash HDMA cleanup clears phase");
    AssertEqual(0, crystalWindow.Status,
        "Crystal Flash HDMA cleanup clears shared status");
    samus.Presentation.AnimateNoFx(new SamusAnimationStep { Random = new CartridgeRandom(), ControllerInput = chord });
    AssertEqual(2, samus.AnimationFrameTimer.RawValue,
        "same beta frame decrements forced timer three to two");

    // Call only accepted NMI counters divisible by eight. These are the only handler calls
    // that can consume ammo or restore energy; skipped counters are checked separately.
    samus.InvincibilityTimer = NativeCountdown.Start(77);
    samus.KnockbackTimer = NativeCountdown.Start(5);
    var resourcesBeforeSkipped =
        (samus.Health, samus.Missiles, samus.SuperMissiles, samus.PowerBombs);
    samus.CrystalFlash.Step(samus, 15);
    AssertEqual(0, samus.InvincibilityTimer.RawValue, "non-draining main frame still clears hit immunity");
    AssertEqual(0, samus.KnockbackTimer.RawValue, "non-draining main frame still clears knockback timer");
    AssertEqual(resourcesBeforeSkipped,
        (samus.Health, samus.Missiles, samus.SuperMissiles, samus.PowerBombs),
        "non-mod-eight frame leaves Crystal Flash resources untouched");
    for (ushort drain = 1; drain <= 30; drain++)
    {
        samus.InvincibilityTimer = NativeCountdown.Start(77);
        samus.KnockbackTimer = NativeCountdown.Start(5);
        samus.CrystalFlash.Step(samus, unchecked((ushort)(drain * 8)));
        AssertEqual(0, samus.InvincibilityTimer.RawValue, "ammo-drain frame clears hit immunity, including final drain");
        AssertEqual(0, samus.KnockbackTimer.RawValue, "ammo-drain frame clears knockback timer");
    }

    AssertEqual(0, samus.Missiles, "Crystal Flash consumes ten missiles");
    AssertEqual(0, samus.SuperMissiles, "Crystal Flash consumes ten supers");
    AssertEqual(0, samus.PowerBombs, "Crystal Flash consumes ten power bombs");
    AssertEqual(99, samus.Health, "Crystal Flash energy restoration caps at max");
    AssertEqual(CrystalFlashPhase.Finishing, samus.CrystalFlash.Phase,
        "thirtieth drain installs finish handler");
    AssertEqual(12, samus.AnimationFrame, "ammo completion forces finish frame twelve");

    // Finish animation is ROM bytecode, not a host countdown. Movement runs before animation
    // each frame; `$FD,$01` publishes standing-right, which is committed after animation.
    samus.Presentation.AnimateNoFx(new SamusAnimationStep { Random = new CartridgeRandom(), ControllerInput = chord });
    int finishFrames = 0;
    while (samus.PendingTransitionalPose is null && finishFrames++ < 20)
    {
        samus.CrystalFlash.Step(samus, (ushort)(0x0100 + finishFrames));
        samus.Presentation.AnimateNoFx(new SamusAnimationStep { Random = new CartridgeRandom(), ControllerInput = chord });
    }
    AssertEqual<SamusPoseId?>(SamusPoseId.FacingRightNormalPose, samus.PendingTransitionalPose,
        "Crystal Flash ROM finish command publishes standing right");
    AssertTrue(samus.PoseTransitions.ApplyPendingVerifiedAnimationTransition(),
        "Crystal Flash standing transition is applied");
    AssertEqual(CrystalFlashPhase.Finishing, samus.CrystalFlash.Phase,
        "pose transition does not prematurely replace installed handler");
    samus.InvincibilityTimer = NativeCountdown.Start(77);
    samus.CrystalFlash.Step(samus, 0x0200);
    AssertEqual(0, samus.InvincibilityTimer.RawValue, "Crystal Flash normal-input completion clears hit immunity");
    AssertEqual(CrystalFlashPhase.Inactive, samus.CrystalFlash.Phase,
        "following beta pass restores normal movement handler");
    AssertEqual(0xffff, samus.CrystalFlash.SpecialPaletteTimer.RawValue,
        "cleanup requests normal palette restoration");
    AssertTrue(samus.CrystalFlash.UpdatePalette(crystalCgram, samus, beamPalettes, crystalColors),
        "Crystal Flash finish restores beam palette");
    AssertEqual(0x0300, crystalCgram.Colors[0xe0],
        "Crystal Flash finish restores beam palette color zero");
    AssertEqual(0x030f, crystalCgram.Colors[0xef],
        "Crystal Flash finish restores all sixteen beam palette colors");
    AssertEqual(SamusSpecialPaletteType.None, samus.CrystalFlash.SpecialPaletteKind,
        "Crystal Flash palette handler clears after restoration");

    var left = new SamusState
    {
        Pose = SamusPoseId.FacingLeftNormalPose,
        Health = 50,
        Missiles = 10,
        SuperMissiles = 10,
        PowerBombs = 10,
    };
    left.PoseMetadata.RefreshCollisionRadii();
    left.PoseHistory.PreviousPose = left.Pose;
    left.PoseHistory.PreviousDirectionAndMovement = 4;
    AssertTrue(left.CrystalFlash.TryBegin(left, chord),
        "left-facing Crystal Flash begins");
    AssertEqual(SamusPoseId.CrystalFlashLeftPose, left.Pose,
        "source direction selects left Crystal Flash pose");
    AssertEqual(SamusPoseId.FacingLeftNormalPose, left.PoseHistory.LastDifferentPose, "left Crystal Flash shifts previous pose");
    AssertEqual(4, left.PoseHistory.LastDifferentDirectionAndMovement, "left Crystal Flash shifts previous metadata");
    AssertEqual(SamusPoseId.CrystalFlashLeftPose, left.PoseHistory.PreviousPose, "Crystal Flash commits left pose");
    AssertEqual(0x1b04, left.PoseHistory.PreviousDirectionAndMovement, "Crystal Flash commits left metadata");

    Console.WriteLine("  Crystal Flash: prerequisites, handlers, resources, HDMA bubble, palette, and ROM animation agree.");
}

/// <summary>Seeds the X-ray pose records, visor and suit colors, and the two window tangent words.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for X-ray poses, visor palettes and the absolute-tangent table")]
static void SeedXrayCartridge(TestAddressSpace bus)
{
    // Complete literal pose records from `$91:B631/B751/B851/BCD1-BCF9`. X-ray does not
    // invent a new movement type: standing bodies are zero, crouched bodies five, and its
    // intermediate turn bodies use `$0E`, which makes the installed movement handler RTS.
    (SamusPoseId Pose, byte[] Definition)[] poses = [
        (SamusPoseId.FacingRightNormalPose, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseId.FacingLeftNormalPose, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseId.TurningRightToLeftPose, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseId.TurningLeftToRightPose, [0x08, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseId.CrouchingRightPose, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseId.CrouchingLeftPose, [0x04, 0x05, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseId.TurningRightToLeftCrouchingPose, [0x04, 0x0e, 0xff, 0xfb, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseId.TurningLeftToRightCrouchingPose, [0x08, 0x0e, 0xff, 0xfb, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseId.XrayingStandingRightPose, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseId.XrayingStandingLeftPose, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseId.XrayingCrouchingRightPose, [0x08, 0x05, 0xff, 0x02, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseId.XrayingCrouchingLeftPose, [0x04, 0x05, 0xff, 0x07, 0x00, 0x00, 0x10, 0x00]),
    ];
    foreach ((SamusPoseId pose, byte[] definition) in poses)
        WritePoseDefinition(bus, (int)pose, definition);

    // Animation pointers and delays are compiled cartridge mechanics. In particular,
    // pose $25 uses three two-tick frames; fake-bus writes to $91:B010 no longer
    // override its native frame-two/timer-one X-ray completion gate.

    // `$9B:A3C0` contains widening colors 3BE0/5FF0/7FFF and full-beam colors
    // 43FF/2F5A/1AB5. The normal-palette pointer is deliberately synthetic so teardown's
    // complete 16-color restoration cannot pass by leaving the previous visor word behind.
    ushort[] visorColors = [0x3be0, 0x5ff0, 0x7fff, 0x43ff, 0x2f5a, 0x1ab5];
    WriteTestWords(bus, SamusXrayRomData.Palette.VisorWords, visorColors);
    WriteTestWord(bus, SamusXrayRomData.Palette.NormalSuitPointers, 0x9400);
    for (ushort index = 0; index < SamusXrayRomData.Palette.SuitColorCount; index++)
    {
        WriteTestWord(
            bus,
            SamusXrayRomData.Palette.PaletteBank | (0x9400 + index * 2),
            unchecked((ushort)(0x0100 + index)));
    }

    // Full right-facing width ten uses boundary angles `$36/$4A`. Both entries in the
    // literal `$91:C9D4` absolute-tangent table are `$03FE`; seeding only those two words
    // makes the window test fail if production code invents trigonometry or reads a nearby
    // table entry. Ten scanlines from the origin, the 8.8 accumulator lands on X+39.
    WriteTestWord(bus, SamusXrayRomData.Window.AbsoluteTangentTable + 0x36 * 2, 0x03fe);
    WriteTestWord(bus, SamusXrayRomData.Window.AbsoluteTangentTable + 0x4a * 2, 0x03fe);
}

/// <summary>
/// Walks `$91:E16D/$91:EEA6/$91:FCAF`, `$90:E94F`, `$88:86EF-$8AA3`, and
/// `$91:DCB4-$DD30`: admission, all four bodies, dedicated turning, angle frames, setup,
/// widening, aiming, visor palette, teardown, and the native crouched-turn stand-up glitch.
/// </summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
internal static void VerifySamusXray()
{
    var bus = new TestAddressSpace();

    SeedXrayCartridge(bus);

    var cgram = new SnesCgram();
    var xrayColors = SamusVisorColorCatalog.Load(new MemoryStream(
        SuperMetroid.AssetExtraction.Samus.SamusVisorColorExtractor.Extract(bus)));
    var xraySuitColors = SamusSuitColorCatalog.Load(new MemoryStream(
        SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(bus)));
    var standing = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(100, 200)),
    };
    standing.Xray.PresentationColors = xrayColors;
    standing.PoseMetadata.RefreshCollisionRadii();
    standing.Presentation.InitializeAnimation();

    AssertTrue(
        standing.Xray.TryBegin(standing, previousMovementType: SamusMovementType.Standing, conditions: new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }),
        "standing X-ray setup accepted");
    AssertEqual(SamusPoseId.XrayingStandingRightPose, standing.Pose, "right standing X-ray pose");
    AssertEqual(21, standing.Kinematics.Radius.Y, "standing X-ray radius");
    AssertEqual(2, standing.AnimationFrame, "command five starts X-ray frame two");
    AssertEqual(0x3f, standing.AnimationFrameTimer.RawValue, "command five X-ray timer");
    AssertEqual(SnesAngle.QuarterTurn.TableIndex, standing.Xray.Angle.TableIndex,
        "right X-ray initial angle");
    AssertEqual(XraySetupStage.FreezeTimeBackupBg2Registers, standing.Xray.SetupStage, "X-ray starts setup stage one");
    AssertTrue(standing.Xray.TimeIsFrozen, "X-ray freezes time");
    AssertTrue(standing.Xray.ActivationSoundRequested, "X-ray activation sound requested");
    AssertTrue(standing.Xray.ConsumeActivationSoundRequest(),
        "X-ray startup sound is consumable exactly once");
    AssertTrue(!standing.Xray.ConsumeActivationSoundRequest(),
        "consumed X-ray startup sound cannot replay");

    AssertEqual(2, (standing.Xray.StepMovement(standing) ?? throw new InvalidOperationException("Verification requires standing.Xray.StepMovement(bus, standing).")),
        "angle 40 selects forward X-ray frame");
    AssertEqual(15, standing.AnimationFrameTimer.RawValue, "X-ray movement forces timer fifteen");
    standing.Presentation.AnimateNoFx(new SamusAnimationStep { Random = new CartridgeRandom() });
    AssertEqual(14, standing.AnimationFrameTimer.RawValue,
        "generic animation follows X-ray movement timer write");

    // Palette handler eight runs in the palette-FX phase. Timer one expires immediately,
    // writes only visor color four, advances byte offset zero to two, and reloads five.
    AssertTrue(standing.Xray.UpdatePalette(cgram, standing.EquippedItems, xraySuitColors),
        "X-ray widening palette writes first visor color");
    AssertEqual(0x3be0, cgram.Colors[196], "first widening visor color");
    AssertEqual(2, standing.Xray.SpecialPaletteFrame, "widening palette offset advances");
    AssertEqual(SamusXrayRomData.Palette.FrameDelay, standing.Xray.CommonPaletteTimer.RawValue,
        "widening palette timer reload");

    // Eight instruction-list setup functions execute before the main bank-$88 preinstruction.
    // The eighth call clears SetupStage; the next call changes X-ray state zero to one.
    for (int stage = 1; stage <= 8; stage++)
        standing.Xray.StepBeam(standing, SnesButton.B);
    AssertEqual(XraySetupStage.Complete, standing.Xray.SetupStage, "eight X-ray setup stages complete");
    AssertEqual(XrayBeamPhase.NoBeam, standing.Xray.BeamPhase, "setup retains state zero");
    standing.Xray.StepBeam(standing, SnesButton.B);
    AssertEqual(XrayBeamPhase.Widening, standing.Xray.BeamPhase, "state zero starts widening");

    // Fixed-point acceleration reaches 10.F800 after 26 widening calls and would cross
    // eleven on call 27; native clamps the whole width to ten, clears its fraction, and
    // advances to full-beam state two on that exact call.
    for (int frame = 1; frame <= 26; frame++)
        standing.Xray.StepBeam(standing, SnesButton.B);
    AssertEqual(XrayBeamPhase.Widening, standing.Xray.BeamPhase,
        "X-ray remains widening through call 26");
    // After n widening calls the speed is n steps and the width is the triangular number n(n+1)/2 of steps.
    const int wideningCalls = 26;
    AssertEqual(10, standing.Xray.AngularWidth, "call 26 whole width");
    AssertEqual(SnesSignedSixteenSixteen.FromRaw(wideningCalls * (wideningCalls + 1) / 2 *
            checked((int)SamusXrayRomData.Window.WideningStep)), standing.Xray.AngularHalfWidth,
        "call 26 width is 351 widening steps, carrying into whole units exactly");
    standing.Xray.StepBeam(standing, SnesButton.B);
    AssertEqual(XrayBeamPhase.Full, standing.Xray.BeamPhase, "call 27 reaches full beam");
    AssertEqual(10, standing.Xray.AngularWidth, "full beam clamps width ten");
    AssertEqual(SnesSignedSixteenSixteen.FromParts(10, 0), standing.Xray.AngularHalfWidth,
        "full beam clears width fraction");

    // `$88:88B8/$88DC` puts this standing-right fixture's origin at screen (103,184).
    // `$91:C5FF` advances `$03FE` once per scanline and publishes the high byte, making
    // X=142 the inclusive upper/lower boundary ten lines away. `$88:817B` keeps the cone
    // bright and applies add-seven-then-half color math everywhere outside it.
    var xrayFrame = new Rgba32[SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height];
    Array.Fill(xrayFrame, new Rgba32(248, 248, 248, 255));
    SnesGameplayFrameRenderer.ApplyXrayWindowColorMath(
        xrayFrame,
        bus,
        standing.Xray,
        standing,
        layer1: new Position16<RoomPixels>(0, 0));
    AssertEqual(248, xrayFrame[184 * 256 + 255].R,
        "X-ray horizontal center remains inside window");
    AssertEqual(156, xrayFrame[184 * 256].R,
        "X-ray opposite half-plane receives outside half color math");
    AssertEqual(248, xrayFrame[174 * 256 + 142].R,
        "X-ray upper tangent boundary is inclusive after 8.8 truncation");
    AssertEqual(156, xrayFrame[174 * 256 + 141].R,
        "X-ray pixel beyond upper tangent boundary is outside");
    AssertEqual(248, xrayFrame[194 * 256 + 142].R,
        "X-ray lower tangent boundary mirrors upper boundary");
    AssertEqual(248, xrayFrame[0].R,
        "X-ray gameplay window never modifies the IRQ-owned HUD band");

    AssertTrue(standing.Xray.UpdatePalette(cgram, standing.EquippedItems, xraySuitColors),
        "full beam enters visor cycle");
    AssertEqual(XrayVisorPaletteMode.FullBeam, standing.Xray.VisorPaletteMode, "full beam palette mode");
    AssertEqual(0x43ff, cgram.Colors[196], "first full-beam visor color");
    AssertEqual(8, standing.Xray.SpecialPaletteFrame, "full-beam palette offset advances");

    // Full-beam aiming moves one angle unit per call and Up wins over Down. Width ten clamps
    // the right-facing center at angle ten, so 80 calls cannot wrap into left-facing space.
    standing.Xray.StepBeam(
        standing,
        (SnesButton.B | SnesButton.Up | SnesButton.Down));
    AssertEqual(0x3f, standing.Xray.Angle.TableIndex, "X-ray Up wins over Down");
    for (int frame = 0; frame < 80; frame++)
        standing.Xray.StepBeam(standing, (SnesButton.B | SnesButton.Up));
    AssertEqual(10, standing.Xray.Angle.TableIndex, "right X-ray upper clamp includes width");
    AssertEqual(0, (standing.Xray.StepMovement(standing) ?? throw new InvalidOperationException("Verification requires standing.Xray.StepMovement(bus, standing).")),
        "upper-clamped angle selects looking-up art");

    // Start a turn while the dedicated handler owns input. `$0100-angle` mirrors ten to
    // F6, pose `$25` supplies type `$0E`, and five native animation ticks reach the
    // exact frame-two/timer-one completion gate before `$D6` is installed.
    standing.PoseHistory.PreviousPose = standing.Pose;
    standing.PoseHistory.PreviousDirectionAndMovement = 0x0008;
    standing.PoseHistory.LastDifferentPose = SamusPoseId.SpinJumpRightPose;
    standing.PoseHistory.LastDifferentDirectionAndMovement = 0x0308;
    SamusPoseId xrayPoseBeforeTurn = standing.Pose;
    standing.Xray.HandlePoseInput(
        standing,
        SnesButton.Left);
    AssertTrue(standing.Pose != xrayPoseBeforeTurn, "X-ray starts standing turn");
    AssertEqual(xrayPoseBeforeTurn, standing.PoseHistory.LastDifferentPose, "X-ray turn shifts previous pose");
    AssertEqual(8, standing.PoseHistory.LastDifferentDirectionAndMovement, "X-ray turn shifts previous metadata");
    AssertEqual(standing.Pose, standing.PoseHistory.PreviousPose, "X-ray turn publishes turning pose");
    AssertEqual(0x0e04, standing.PoseHistory.PreviousDirectionAndMovement, "X-ray turn publishes turning metadata");
    AssertEqual(SamusPoseId.TurningRightToLeftPose, standing.Pose, "X-ray right-to-left standing turn pose");
    AssertEqual(0xf6, standing.Xray.Angle.TableIndex, "X-ray turn mirrors angle");
    AssertTrue(standing.Xray.StepMovement(standing) is null,
        "X-ray movement is RTS during type-E turn");
    for (int tick = 0; tick < 5; tick++)
        standing.Presentation.AnimateNoFx(new SamusAnimationStep { Random = new CartridgeRandom() });
    AssertEqual(2, standing.AnimationFrame, "X-ray turn reaches frame two");
    AssertEqual(1, standing.AnimationFrameTimer.RawValue, "X-ray turn reaches timer one");
    standing.Xray.HandlePoseInput(standing, 0);
    AssertTrue((int)standing.Pose != 0x25, "X-ray completes standing turn");
    AssertEqual(SamusPoseId.TurningRightToLeftPose, standing.PoseHistory.LastDifferentPose, "X-ray completion shifts turning pose");
    AssertEqual(0x0e04, standing.PoseHistory.LastDifferentDirectionAndMovement, "X-ray completion shifts turning metadata");
    AssertEqual(standing.Pose, standing.PoseHistory.PreviousPose, "X-ray completion publishes stable pose");
    AssertEqual(4, standing.PoseHistory.PreviousDirectionAndMovement, "X-ray completion publishes stable metadata");
    AssertEqual(SamusPoseId.XrayingStandingLeftPose, standing.Pose, "X-ray turn installs left standing body");
    AssertEqual(0, (standing.Xray.StepMovement(standing) ?? throw new InvalidOperationException("Verification requires standing.Xray.StepMovement(bus, standing).")),
        "left near-up angle selects looking-up art");

    // Releasing Dash enters states three/four/five. State five restores ordinary left
    // standing, requests palette restoration and sound ten, and unfreezes every subsystem.
    standing.Xray.StepBeam(standing, 0);
    AssertEqual(XrayBeamPhase.RestoreFirstHalf, standing.Xray.BeamPhase,
        "Dash release starts first BG2 restore");
    standing.Xray.StepBeam(standing, 0);
    standing.Xray.StepBeam(standing, 0);
    XrayBeamStepResult finished = standing.Xray.StepBeam(standing, 0);
    AssertTrue(finished.Completed, "X-ray state five completes");
    AssertTrue(!standing.Xray.IsActive && !standing.Xray.TimeIsFrozen,
        "X-ray teardown restores time and handlers");
    AssertEqual(SamusPoseId.FacingLeftNormalPose, standing.Pose, "left X-ray exits to ordinary standing");
    AssertEqual(XrayVisorPaletteMode.RestoreSuitPalette, standing.Xray.VisorPaletteMode,
        "X-ray teardown requests palette restoration");
    AssertTrue(standing.Xray.DeactivationSoundRequested, "X-ray deactivation sound requested");
    AssertTrue(standing.Xray.ConsumeDeactivationSoundRequest(),
        "X-ray teardown sound is consumable exactly once");
    AssertTrue(!standing.Xray.ConsumeDeactivationSoundRequest(),
        "consumed X-ray teardown sound cannot replay");
    AssertTrue(standing.Xray.UpdatePalette(cgram, standing.EquippedItems, xraySuitColors),
        "X-ray teardown restores normal suit palette");
    AssertEqual(0x0104, cgram.Colors[196], "normal palette replaces visor color");
    AssertEqual(SamusSpecialPaletteType.None, standing.Xray.SpecialPaletteKind, "X-ray palette handler clears");

    // Crouched setup selects `$D9`. Releasing while its `$43` turn is still active makes
    // `$91:E2AD` classify movement type `$0E` as standing, choose left `$02`, expand radius
    // 16 -> 21, and move the center five pixels upward: the retail X-ray stand-up glitch.
    var crouched = new SamusState
    {
        Pose = SamusPoseId.CrouchingRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(100, 200)),
    };
    crouched.PoseMetadata.RefreshCollisionRadii();
    crouched.Presentation.InitializeAnimation();
    AssertTrue(crouched.Xray.TryBegin(crouched, previousMovementType: SamusMovementType.Crouching, conditions: new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }),
        "crouched X-ray setup accepted");
    AssertEqual(SamusPoseId.XrayingCrouchingRightPose, crouched.Pose, "right crouched X-ray pose");
    AssertEqual(16, crouched.Kinematics.Radius.Y, "crouched X-ray radius");
    crouched.Xray.HandlePoseInput(crouched, SnesButton.Left);
    AssertEqual(SamusPoseId.TurningRightToLeftCrouchingPose, crouched.Pose, "crouched X-ray turn pose");
    AssertEqual(16, crouched.Kinematics.Radius.Y, "crouched turn retains radius");
    for (int stage = 1; stage <= 8; stage++)
        crouched.Xray.StepBeam(crouched, 0);
    crouched.Xray.StepBeam(crouched, 0); // State 0 -> state 3 on released Dash.
    crouched.Xray.StepBeam(crouched, 0); // State 3 -> state 4.
    crouched.Xray.StepBeam(crouched, 0); // State 4 -> state 5.
    crouched.Xray.StepBeam(crouched, 0); // State 5 -> teardown.
    AssertEqual(SamusPoseId.FacingLeftNormalPose, crouched.Pose, "crouched-turn release triggers standing-left glitch");
    AssertEqual(21, crouched.Kinematics.Radius.Y, "stand-up glitch expands radius");
    AssertEqual(195, crouched.Position.Y.Whole, "stand-up glitch moves center up five pixels");
    AssertEqual(SamusPoseId.TurningRightToLeftCrouchingPose, crouched.PoseHistory.LastDifferentPose, "X-ray release shifts crouched turn pose");
    AssertEqual(0x0e04, crouched.PoseHistory.LastDifferentDirectionAndMovement, "X-ray release shifts turn metadata");
    AssertEqual(SamusPoseId.FacingLeftNormalPose, crouched.PoseHistory.PreviousPose, "X-ray release publishes standing pose");
    AssertEqual(4, crouched.PoseHistory.PreviousDirectionAndMovement, "X-ray release publishes standing metadata");

    // Native movie `x-mode max upward jump speed store.smv`, frame 522, executes
    // `$91:E16D` while hit-expiry command one wins UpdateSamusPose over X-Ray's special
    // command five. WRAM retains ordinary pose `$0A` and normal handlers, but `$0A78`
    // becomes one and `$198D/$1C23/$1E79/$1EF1` all lose their enable high bit. Preserve
    // that ownership split rather than cancelling the complete X-Ray setup with its pose.
    var interrupted = new SamusState
    {
        Pose = SamusPoseId.FacingLeftNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(0x0174, 0x028b)),
    };
    interrupted.PoseMetadata.RefreshCollisionRadii();
    interrupted.Presentation.InitializeAnimation();
    AssertTrue(interrupted.Xray.TryBegin(interrupted, previousMovementType: SamusMovementType.Standing, conditions: new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }, deferActivation: true),
        "X-Mode fixture admits deferred left-facing X-Ray setup");
    AssertTrue(interrupted.Xray.IsActive && !interrupted.Xray.OwnsSamusControl,
        "deferred setup starts beam ownership before pose-command arbitration");
    AssertEqual(XraySuspendedSubsystems.All, interrupted.Xray.SuspendedSubsystems,
        "deferred setup immediately disables all four native subsystems");
    AssertEqual(SnesAngle.ThreeQuarterTurn.TableIndex, interrupted.Xray.Angle.TableIndex,
        "deferred left-facing setup publishes native angle C0");
    AssertEqual(SamusSpecialPaletteType.None, interrupted.Xray.SpecialPaletteKind,
        "deferred setup does not install command-five palette ownership early");
    AssertTrue(!interrupted.Xray.ActivationSoundRequested,
        "deferred setup does not queue command-five activation sound early");

    interrupted.Pose = SamusPoseId.MovingLeftNormalPose;
    AssertTrue(!interrupted.Xray.CommitPendingActivation(interrupted, superseded: true),
        "higher-priority hit expiry suppresses only X-Ray's Samus command");
    AssertEqual(SamusPoseId.MovingLeftNormalPose, interrupted.Pose,
        "interrupted X-Mode retains native winning ordinary pose");
    AssertTrue(interrupted.Xray.IsActive && interrupted.Xray.TimeIsFrozen,
        "interrupted X-Mode retains beam lifecycle and frozen time");
    AssertTrue(!interrupted.Xray.OwnsSamusControl,
        "interrupted X-Mode retains normal Samus handler ownership");
    AssertEqual(XraySetupStage.FreezeTimeBackupBg2Registers, interrupted.Xray.SetupStage,
        "interrupted X-Mode retains first HDMA setup stage");
    interrupted.Xray.StepBeam(interrupted, SnesButton.B);
    AssertEqual(XraySetupStage.ReadBg1SecondScreen, interrupted.Xray.SetupStage,
        "interrupted X-Mode continues the independent beam setup program");

    // Admission failures are kept independent so no broad host-side `grounded` boolean can
    // accidentally replace the native previous/current type, landing, velocity, and rare
    // five-bomb conjunction checks.
    SamusState Rejected(SamusPoseId pose, ushort ySpeed = 0, ushort ySubspeed = 0)
    {
        var sample = new SamusState { Pose = pose };
        sample.PoseMetadata.RefreshCollisionRadii();
        sample.Presentation.InitializeAnimation();
        sample.Kinematics.YSpeed = new SnesUnsignedSixteenSixteen(ySpeed, ySubspeed);
        return sample;
    }

    // Seed excluded landing `$A4` and falling `$29` definitions/animations only for gates.
    WritePoseDefinition(bus, (int)SamusPoseId.NormalLandingRightPose,
        [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.FallingRightPose,
        [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]);
    WritePoseAnimationDelays(bus, SamusPoseId.NormalLandingRightPose, 0xc700, [0x01, 0xff]);
    WritePoseAnimationDelays(bus, SamusPoseId.FallingRightPose, 0xc710, [0x01, 0xff]);
    var landing = Rejected(SamusPoseId.NormalLandingRightPose);
    AssertTrue(!landing.Xray.TryBegin(landing, previousMovementType: SamusMovementType.Standing, conditions: new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }),
        "X-ray rejects landing pose");
    var movingVertically = Rejected(SamusPoseId.FacingRightNormalPose, ySubspeed: 1);
    AssertTrue(!movingVertically.Xray.TryBegin(movingVertically, previousMovementType: SamusMovementType.Standing, conditions: new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }),
        "X-ray rejects fractional Y velocity");
    var badPrevious = Rejected(SamusPoseId.FacingRightNormalPose);
    AssertTrue(!badPrevious.Xray.TryBegin(badPrevious, previousMovementType: SamusMovementType.Falling, conditions: new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }),
        "X-ray rejects unsupported previous movement type");
    var fiveBombQuirk = Rejected(SamusPoseId.FacingRightNormalPose);
    fiveBombQuirk.XSpeedDivisor = 2;
    AssertTrue(!fiveBombQuirk.Xray.TryBegin(fiveBombQuirk, previousMovementType: SamusMovementType.Standing, conditions: new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay, ProjectileCooldownTimer = NativeCountdown.Start(7), BombCounter = 5 }),
        "X-ray preserves five-bomb cooldown/divisor rejection");

    Console.WriteLine(
        "  X-ray: admission, interrupted X-Mode ownership, poses, turns, ROM-tangent window, half color math, visor palette, teardown, and stand-up glitch agree.");
}

/// <summary>
/// Seeds the death-sequence pose records and delay streams, the ten suit and suitless
/// palettes, the explosion timer/palette records and the whiteout shades.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the Samus death sequence")]
static void SeedSamusDeathSequenceCartridge(TestAddressSpace bus, ushort[] shades)
{
    // Source standing/morph/spin records prove three distinct `$9B:B420` decisions. The
    // death records are literal `$91:BCE1/BCE9`: type `$0A`, ordinary humanoid radii, and
    // right/left direction bytes. Their delay stream is exactly 2,2,2,2,2,2,FE,01.
    WritePoseDefinition(bus, (int)SamusPoseId.FacingRightNormalPose,
        [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.MorphBallGroundLeftPose,
        [0x04, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.SpinJumpRightPose,
        [0x08, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0b, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.DeathSequenceRightPose,
        [0x08, 0x0a, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.DeathSequenceLeftPose,
        [0x04, 0x0a, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.FacingRightNormalPose, 0xc000);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.MorphBallGroundLeftPose, 0xc010);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.SpinJumpRightPose, 0xc020);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.DeathSequenceRightPose, 0xb567);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.DeathSequenceLeftPose, 0xb567);
    WriteAnimationDelayStream(bus, 0xc000, [0x01, 0xff]);
    WriteAnimationDelayStream(bus, 0xc010, [0x01, 0xff]);
    WriteAnimationDelayStream(bus, 0xc020, [0x01, 0xff]);
    WriteAnimationDelayStream(bus, 0xb567, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xfe, 0x01]);

    // Ten suit and ten suitless pointers reproduce `$9B:B7D3/B80F`. Every palette gets a
    // unique first color, making each table index independently visible in assertions.
    for (ushort palette = 0; palette < 10; palette++)
    {
        ushort suitPointer = unchecked((ushort)(0xc000 + palette * 0x20));
        ushort suitlessPointer = unchecked((ushort)(0xd400 + palette * 0x20));
        WriteTestWord(bus, 0x9bb7d3 + palette * 2, suitPointer);
        WriteTestWord(bus, 0x9bb80f + palette * 2, suitlessPointer);
        for (ushort color = 0; color < 16; color++)
        {
            WriteTestWord(bus, 0x9b0000 | (suitPointer + color * 2),
                unchecked((ushort)(0x0100 + palette * 0x20 + color)));
            WriteTestWord(bus, 0x9b0000 | (suitlessPointer + color * 2),
                unchecked((ushort)(0x0400 + palette * 0x20 + color)));
        }
    }

    // Interleaved timer/palette bytes at `$9B:B823`: index zero uses 21/palette zero,
    // indices one through eight use 6/2,3/3,4/4,5/5,5/6,6/7,6/8,80/9.
    bus.WriteBytes(SamusDeathExplosionTimingDefinitions.NativeFirstTimerAddress,
        [0x15, 0x00, 0x06, 0x02, 0x03, 0x03, 0x04, 0x04, 0x05, 0x05,
         0x05, 0x06, 0x06, 0x07, 0x06, 0x08, 0x50, 0x09]);
    WriteTestWords(bus, 0x9bb835, shades);
}

/// <summary>
/// Installs the seeded synthetic death colors through the production artwork boundary. This
/// fixture exercises Power suit; the other suit families use the same supplied colors.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture decoding the seeded Samus death palettes")]
static SamusDeathPaletteArtworkCatalog DecodeSamusDeathFixtureArtwork(TestAddressSpace bus, ushort[] shades)
{
    ushort[] FixturePalette(int pointer) => Enumerable.Range(0, 16)
        .Select(color => SnesLittleEndian.Word(bus.ReadByte(pointer + color * 2), bus.ReadByte(pointer + color * 2 + 1))).ToArray();
    ushort[][] suitColors = Enumerable.Range(0, 10).Select(palette => FixturePalette(0x9bc000 + palette * 0x20)).ToArray();
    ushort[][] suitlessColors = Enumerable.Range(0, 10).Select(palette => FixturePalette(0x9bd400 + palette * 0x20)).ToArray();
    ushort[] paletteIndices = Enumerable.Range(0, SamusDeathExplosionTimingDefinitions.RecordCount)
        .Select(index => (ushort)bus.ReadByte(SamusDeathExplosionTimingDefinitions.NativeFirstTimerAddress + index * 2 + 1)).ToArray();
    return new SamusDeathPaletteArtworkCatalog([ToColors(suitColors), ToColors(suitColors), ToColors(suitColors)], ToColors(suitlessColors), ToColors(shades), paletteIndices);
}

/// <summary>
/// Walks `$9B:B3A7-$B85F` and `$90:8976-$89FF`: movement-type-selected `$D7/$D8`
/// setup, animation-only preflash, five death-tile transfers, alternating suit/suitless
/// palettes, 60-call flash, nine explosion frames, non-Samus palette whiteout, and terminal
/// no-draw state.
/// </summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
internal static void VerifySamusDeathSequence()
{
    var bus = new TestAddressSpace();

    ushort[] shades =
    [
        0x0421, 0x0c63, 0x14a5, 0x1ce7, 0x2529, 0x2d6b, 0x35ad, 0x4210,
        0x4a52, 0x4e73, 0x5294, 0x56b5, 0x5ad6, 0x5ef7, 0x6318, 0x6739,
        0x6b5a, 0x6f7b, 0x739c, 0x77bd, 0x7bde, 0x7fff,
    ];
    SeedSamusDeathSequenceCartridge(bus, shades);
    SamusDeathPaletteArtworkCatalog artwork = DecodeSamusDeathFixtureArtwork(bus, shades);
    var guardedBus = NewSamusDeathExplosionTimingReadGuard(bus);
    var samus = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(0x0480, 0x04c0)),
    };
    samus.PoseMetadata.RefreshCollisionRadii();
    samus.Presentation.InitializeAnimation();
    samus.PoseHistory.PreviousPose = samus.Pose;
    samus.PoseHistory.PreviousDirectionAndMovement = 8;
    samus.PoseHistory.LastDifferentPose = SamusPoseId.SpinJumpLeftPose;
    samus.PoseHistory.LastDifferentDirectionAndMovement = 0x0304;
    SamusMovementType deathSourceMovementType = samus.PoseMetadata.ReadMovementType();
    samus.DeathSequence.Begin(
        samus,
        layer1: new(0x03e0, 0x0400));
    AssertEqual(SamusMovementType.Standing, deathSourceMovementType, "death source standing type");
    AssertEqual(SamusPoseId.DeathSequenceRightPose, samus.Pose, "death selects right pose");
    AssertEqual(SamusPoseId.FacingRightNormalPose, samus.PoseHistory.LastDifferentPose, "death shifts prior pose");
    AssertEqual(8, samus.PoseHistory.LastDifferentDirectionAndMovement, "death shifts prior metadata");
    AssertEqual(SamusPoseId.DeathSequenceRightPose, samus.PoseHistory.PreviousPose, "death commits right pose");
    AssertEqual(SnesLittleEndian.Word(samus.PoseMetadata.ReadPoseXDirection(), (byte)samus.PoseMetadata.ReadMovementType()),
        samus.PoseHistory.PreviousDirectionAndMovement, "death commits right metadata");
    AssertEqual(5, samus.AnimationFrame, "ordinary death starts unmorphed frame five");
    AssertEqual(new Position16<ScreenPixels>(0x00a0, 0x00c0), samus.DeathSequence.ScreenPosition, "death captures screen position");
    AssertTrue(!samus.DeathSequence.SpinJumpSoundRequested, "ordinary death does not request spin SFX");
    AssertEqual(2, samus.AnimationFrameTimer.RawValue, "death pose retains two-tick delay");

    var cgram = new SnesCgram();
    SamusDeathSequenceStepResult step = default;
    for (int call = 1; call <= 16; call++)
        step = samus.DeathSequence.Step(samus, cgram, artwork);
    AssertEqual(SamusDeathSequencePhase.Flashing, samus.DeathSequence.Phase,
        "sixteen preflash calls enter flashing");
    AssertEqual(5, samus.AnimationFrame, "unmorphed death frame loops at five");
    AssertTrue(step.DrawPose, "last preflash call still draws death pose");

    // Calls one through four queue the four high OBJ segments. Call 60 finishes flashing,
    // queues segment four at `$6000`, resets the explosion state, immediately decrements
    // 21 to 20, and draws right-facing spritemap `$81C`.
    for (int call = 1; call <= 60; call++)
        step = samus.DeathSequence.Step(samus, cgram, artwork);
    AssertEqual(SamusDeathSequencePhase.SuitExplosion, samus.DeathSequence.Phase,
        "60 flashing calls enter suit explosion");
    AssertEqual(0, samus.DeathSequence.AnimationIndex,
        "explosion begins at index zero");
    AssertEqual(20, samus.DeathSequence.AnimationTimer.RawValue,
        "same-call first explosion decrement");
    AssertEqual(0x081c, (samus.DeathSequence.ExplosionSpritemapIndex ?? throw new InvalidOperationException("Verification requires samus.DeathSequence.ExplosionSpritemapIndex.")),
        "right explosion base spritemap");
    AssertEqual(0x0100, cgram.Colors[192], "finish restores suit palette zero");
    AssertEqual(0x0400, cgram.Colors[240], "finish restores suitless palette zero");

    // The remaining literal timers total 135 calls. The terminal call increments index
    // eight to nine, forces white shade 21, and deliberately emits no tenth spritemap.
    int explosionCalls = 0;
    while (samus.DeathSequence.Phase != SamusDeathSequencePhase.Complete)
    {
        step = samus.DeathSequence.Step(samus, cgram, artwork);
        explosionCalls++;
        AssertTrue(explosionCalls <= 135, "death explosion terminates on native timer sum");
    }
    AssertEqual(135, explosionCalls, "death explosion remaining call count");
    AssertTrue(step.Completed && !step.DrawExplosion,
        "terminal death call completes without drawing");
    AssertEqual(9, samus.DeathSequence.AnimationIndex,
        "death terminal index nine");
    AssertEqual(0x0015, samus.DeathSequence.AnimationCounter,
        "death terminal whiteout shade index");
    AssertEqual(0x7fff, cgram.Colors[0], "death whiteout reaches full white");
    AssertEqual(0x7fff, cgram.Colors[239], "death whiteout includes palette six end");
    AssertEqual(0x0220, cgram.Colors[192],
        "whiteout preserves final Samus suit palette nine");
    AssertEqual(0x0520, cgram.Colors[240],
        "whiteout preserves final suitless palette nine");
    AssertEqual(0, guardedBus.ForbiddenReadAttempts,
        "production death sequence performs no explosion-timer ROM reads");

    // Morph Ball begins frame one and uses left pose `$D8`; spin jumping still starts frame
    // five but uniquely requests library-one sound `$32` before pose replacement.
    var morphedLeft = new SamusState
    {
        Pose = SamusPoseId.MorphBallGroundLeftPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(64, 80)),
    };
    morphedLeft.PoseMetadata.RefreshCollisionRadii();
    morphedLeft.Presentation.InitializeAnimation();
    morphedLeft.PoseHistory.PreviousPose = morphedLeft.Pose;
    morphedLeft.PoseHistory.PreviousDirectionAndMovement = 0x0404;
    morphedLeft.DeathSequence.Begin(
        morphedLeft, layer1: new(0, 0));
    AssertEqual(SamusPoseId.DeathSequenceLeftPose, morphedLeft.Pose, "left Morph death selects D8");
    AssertEqual(SamusPoseId.MorphBallGroundLeftPose, morphedLeft.PoseHistory.LastDifferentPose, "left death shifts prior pose");
    AssertEqual(0x0404, morphedLeft.PoseHistory.LastDifferentDirectionAndMovement, "left death shifts prior metadata");
    AssertEqual(SamusPoseId.DeathSequenceLeftPose, morphedLeft.PoseHistory.PreviousPose, "death commits left pose");
    AssertEqual(SnesLittleEndian.Word(morphedLeft.PoseMetadata.ReadPoseXDirection(), (byte)morphedLeft.PoseMetadata.ReadMovementType()),
        morphedLeft.PoseHistory.PreviousDirectionAndMovement, "death commits left metadata");
    AssertEqual(1, morphedLeft.AnimationFrame, "Morph death begins unmorph frame one");

    var spinning = new SamusState { Pose = SamusPoseId.SpinJumpRightPose };
    spinning.PoseMetadata.RefreshCollisionRadii();
    spinning.Presentation.InitializeAnimation();
    spinning.DeathSequence.Begin(
        spinning, layer1: new(0, 0));
    AssertTrue(spinning.DeathSequence.SpinJumpSoundRequested, "spin death requests sound $32");
    AssertTrue(spinning.DeathSequence.ConsumeSpinJumpSoundRequest(),
        "spin-death sound is consumable exactly once");
    AssertTrue(!spinning.DeathSequence.ConsumeSpinJumpSoundRequest(),
        "consumed spin-death sound cannot replay during death animation");
    AssertEqual(5, spinning.AnimationFrame, "spin death begins unmorphed frame five");

    Console.WriteLine(
        "  Samus death: D7/D8 selection, unmorph art, five VRAM segments, flash palettes, " +
        "whiteout, and nine explosion frames agree without timer-ROM reads.");
}

    /// <summary>Runs the seeded death pose, explosion, palette, and whiteout verification.</summary>
    [VerificationCommand("--death-sequence-fixture")]
    internal static int DeathSequenceFixtureCommand()
    {
     VerifySamusDeathSequence(); return 0; 
    }

    /// <summary>Runs Crystal Flash and X-ray palette state fixtures.</summary>
    [VerificationCommand("--special-palette-fixtures")]
    internal static int SpecialPaletteFixturesCommand()
    {
     VerifySamusCrystalFlash(); VerifySamusXray(); return 0; 
    }

    /// <summary>Runs the X-ray visor, movement, and window fixture.</summary>
    [VerificationCommand("--samus-xray")]
    internal static int SamusXrayCommand()
    {
        Suite(nameof(VerifySamusXray), () => VerifySamusXray());
        return 0;
    }

    /// <summary>Runs the Crystal Flash state and palette fixture.</summary>
    [VerificationCommand("--crystal-flash")]
    internal static int CrystalFlashCommand()
    {
        Suite(nameof(VerifySamusCrystalFlash), () => VerifySamusCrystalFlash());
        return 0;
    }
}
