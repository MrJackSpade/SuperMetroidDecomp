using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{

/// <summary>Crystal Flash, X-ray, and death-sequence verification.</summary>
static void VerifySamusCrystalFlash()
{
    var bus = new TestAddressSpace();

    // Only bytes zero, one, and six matter to this fixture: facing, movement type, and Y
    // radius. The production implementation still reads them through the actual ROM table
    // addresses instead of receiving test-only pose metadata.
    WritePoseDefinition(bus, SamusPoseIds.FacingRightNormalPose,
        [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.FacingLeftNormalPose,
        [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.CrystalFlashRightPose,
        [0x08, 0x1b, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.CrystalFlashLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]);

    WriteTestWord(bus, 0x91b010 + SamusPoseIds.FacingRightNormalPose * 2, 0xb600);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.FacingLeftNormalPose * 2, 0xb601);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.CrystalFlashRightPose * 2, 0xb545);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.CrystalFlashLeftPose * 2, 0xb556);
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
    WriteTestWord(bus, 0x91dc00, 0x9500);
    WriteTestWord(bus, 0x91dc02, 10);
    WriteTestWord(bus, 0x91dc28, 0x9600);
    WriteTestWord(bus, 0x91dc2a, 0x9620);
    WriteTestWord(bus, 0x90c3c9, 0x9700);
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

    const ushort chord = (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);
    var samus = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        XPosition = 0x0120,
        YPosition = 0x0080,
        Health = 1,
        MaxHealth = 99,
        Missiles = 10,
        SuperMissiles = 10,
        PowerBombs = 10,
    };
    samus.RefreshCollisionRadii(bus);

    AssertTrue(!samus.CrystalFlash.TryBegin(bus, samus, chord | (ushort)SnesButton.A),
        "Crystal Flash rejects extra held input");
    samus.Kinematics.YSubspeed = 1;
    AssertTrue(!samus.CrystalFlash.TryBegin(bus, samus, chord),
        "Crystal Flash rejects fractional vertical movement");
    samus.Kinematics.YSubspeed = 0;
    AssertTrue(samus.CrystalFlash.TryBegin(bus, samus, chord),
        "Crystal Flash accepts exact chord and resources");
    AssertEqual(SamusPoseIds.CrystalFlashRightPose, samus.Pose,
        "source direction selects right Crystal Flash pose");
    AssertEqual(CrystalFlashPhase.Raising, samus.CrystalFlash.Phase,
        "Crystal Flash installs raise handler");
    AssertEqual(7, samus.CrystalFlash.SpecialPaletteType,
        "Crystal Flash installs palette handler seven");

    var crystalCgram = new SnesCgram();
    AssertTrue(samus.CrystalFlash.UpdatePalette(bus, crystalCgram, samus),
        "Crystal Flash palette handler owns first visible frame");
    AssertEqual(0x0100, crystalCgram.Colors[0xe0],
        "Crystal Flash body palette begins at sprite palette-six color zero");
    AssertEqual(0x0109, crystalCgram.Colors[0xe9],
        "Crystal Flash body palette copies ten colors");
    AssertEqual(0x0200, crystalCgram.Colors[0xea],
        "Crystal Flash bubble palette begins at color ten");
    AssertEqual(0x0205, crystalCgram.Colors[0xef],
        "Crystal Flash bubble palette copies six colors");
    AssertEqual(5, samus.CrystalFlash.SpecialPaletteTimer,
        "Crystal Flash bubble timer reloads five");
    AssertEqual(10, samus.CrystalFlash.CrystalPaletteTimer,
        "Crystal Flash body timer comes from interleaved ROM record");
    AssertEqual(4, samus.CrystalFlash.CommonPaletteTimer,
        "Crystal Flash body record advances four bytes");
    AssertEqual(2, samus.HorizontalSpeed.SpecialPaletteFrame,
        "Crystal Flash publishes aliased $0ACE palette frame");
    AssertEqual(4, samus.HorizontalSpeed.SpecialPaletteTimer,
        "Crystal Flash publishes aliased $0AD0 record offset");

    // Five more calls expire only the bubble timer and select pointer one. The ten-call
    // body timer remains halfway through its first record.
    for (int paletteCall = 0; paletteCall < 5; paletteCall++)
        samus.CrystalFlash.UpdatePalette(bus, crystalCgram, samus);
    AssertEqual(0x0220, crystalCgram.Colors[0xea],
        "Crystal Flash bubble palette advances independently");
    AssertEqual(5, samus.CrystalFlash.CrystalPaletteTimer,
        "Crystal Flash body palette retains independent countdown");

    ushort initialY = samus.YPosition;
    for (int frame = 0; frame < 9; frame++)
    {
        CrystalFlashMovementResult raise = samus.CrystalFlash.Step(bus, samus, (ushort)frame);
        AssertEqual(CrystalFlashPhase.Raising, raise.PhaseAfterStep,
            $"raise frame {frame} retains start handler");
        samus.AnimateNoFx(bus, chord);
    }
    AssertEqual((initialY - 18), samus.YPosition, "first nine raise calls move 18 pixels");

    CrystalFlashMovementResult raiseTransition = samus.CrystalFlash.Step(bus, samus, 9);
    AssertEqual(CrystalFlashPhase.DrainingAmmo, raiseTransition.PhaseAfterStep,
        "tenth raise call installs ammo handler");
    AssertEqual((initialY - 20), samus.YPosition, "complete raise is 20 pixels");
    AssertEqual(samus.YPosition, samus.CrystalFlash.RaisedYPosition,
        "raised Y capture follows tenth displacement");
    AssertEqual(6, samus.AnimationFrame, "raise transition forces animation frame six");
    AssertTrue(samus.CrystalFlash.BubbleHdmaRequested,
        "raise transition publishes Crystal Flash HDMA spawn seam");
    AssertTrue(samus.CrystalFlash.ConsumeActivationSoundRequest(),
        "Crystal Flash activation sound is consumable exactly once");
    AssertTrue(!samus.CrystalFlash.ConsumeActivationSoundRequest(),
        "consumed Crystal Flash sound cannot replay while ammo drains");

    var crystalWindow = new SamusPowerBombExplosionState();
    crystalWindow.Arm();
    crystalWindow.BeginCrystalFlash(samus.XPosition, samus.YPosition);
    AssertTrue(!crystalWindow.IsArmed,
        "Crystal Flash spawn clears one-at-a-time Power Bomb flag");
    AssertEqual(PowerBombExplosionPhase.CrystalFlashExplosion, crystalWindow.Phase,
        "Crystal Flash installs bank-$88 stage-one pre-instruction");
    AssertEqual(0x0400, crystalWindow.ExplosionRadius,
        "Crystal Flash bubble begins at radius four");

    // Seventeen calls remain below `$20.00`; call eighteen adds the old `$0330` speed,
    // reaches `$20B0`, and installs the afterglow. Its rendered table still used `$1D80`.
    for (int hdmaCall = 1; hdmaCall <= 17; hdmaCall++)
        AssertTrue(!crystalWindow.StepFrame(bus), $"Crystal Flash expansion call {hdmaCall}");
    AssertEqual(PowerBombExplosionPhase.CrystalFlashExplosion, crystalWindow.Phase,
        "Crystal Flash expansion remains active through call seventeen");
    AssertTrue(!crystalWindow.StepFrame(bus), "Crystal Flash expansion transition is not cleanup");
    AssertEqual(PowerBombExplosionPhase.CrystalFlashAfterglow, crystalWindow.Phase,
        "Crystal Flash call eighteen installs afterglow");
    AssertEqual(0x20b0, crystalWindow.ExplosionRadius,
        "Crystal Flash transition radius preserves 8.8 acceleration sum");
    AssertEqual(0x1d80, crystalWindow.RenderedExplosionRadius,
        "Crystal Flash transition renders pre-update radius");
    AssertEqual(4, crystalWindow.FixedColorRed,
        "Crystal Flash transition selects shared fixed-color entry three");

    // Components 4/3/2 decrement on afterglow calls 1/5/9/13. Calls 14..16 count down
    // timer 3..0; call 17 observes all-zero color and executes `$88:A317` cleanup.
    for (int hdmaCall = 1; hdmaCall <= 16; hdmaCall++)
        AssertTrue(!crystalWindow.StepFrame(bus), $"Crystal Flash afterglow call {hdmaCall}");
    AssertTrue(crystalWindow.StepFrame(bus),
        "Crystal Flash afterglow call seventeen performs cleanup");
    AssertEqual(PowerBombExplosionPhase.Inactive, crystalWindow.Phase,
        "Crystal Flash HDMA cleanup clears phase");
    AssertEqual(0, crystalWindow.Status,
        "Crystal Flash HDMA cleanup clears shared status");
    samus.AnimateNoFx(bus, chord);
    AssertEqual(2, samus.AnimationFrameTimer,
        "same beta frame decrements forced timer three to two");

    // Call only accepted NMI counters divisible by eight. These are the only handler calls
    // that can consume ammo or restore energy; skipped counters are checked separately.
    CrystalFlashMovementResult skipped = samus.CrystalFlash.Step(bus, samus, 15);
    AssertTrue(!skipped.ConsumedAmmo && !skipped.RestoredEnergy,
        "non-mod-eight frame leaves Crystal Flash resources untouched");
    for (ushort drain = 1; drain <= 30; drain++)
        samus.CrystalFlash.Step(bus, samus, unchecked((ushort)(drain * 8)));

    AssertEqual(0, samus.Missiles, "Crystal Flash consumes ten missiles");
    AssertEqual(0, samus.SuperMissiles, "Crystal Flash consumes ten supers");
    AssertEqual(0, samus.PowerBombs, "Crystal Flash consumes ten power bombs");
    AssertEqual(99, samus.Health, "Crystal Flash energy restoration caps at max");
    AssertEqual(CrystalFlashPhase.Finishing, samus.CrystalFlash.Phase,
        "thirtieth drain installs finish handler");
    AssertEqual(12, samus.AnimationFrame, "ammo completion forces finish frame twelve");

    // Finish animation is ROM bytecode, not a host countdown. Movement runs before animation
    // each frame; `$FD,$01` publishes standing-right, which is committed after animation.
    samus.AnimateNoFx(bus, chord);
    int finishFrames = 0;
    while (samus.PendingTransitionalPose is null && finishFrames++ < 20)
    {
        samus.CrystalFlash.Step(bus, samus, (ushort)(0x0100 + finishFrames));
        samus.AnimateNoFx(bus, chord);
    }
    AssertEqual<byte?>(SamusPoseIds.FacingRightNormalPose, samus.PendingTransitionalPose,
        "Crystal Flash ROM finish command publishes standing right");
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus),
        "Crystal Flash standing transition is applied");
    AssertEqual(CrystalFlashPhase.Finishing, samus.CrystalFlash.Phase,
        "pose transition does not prematurely replace installed handler");
    CrystalFlashMovementResult cleanup = samus.CrystalFlash.Step(bus, samus, 0x0200);
    AssertTrue(cleanup.Completed, "following beta pass restores normal movement handler");
    AssertEqual(0xffff, samus.CrystalFlash.SpecialPaletteTimer,
        "cleanup requests normal palette restoration");
    AssertTrue(samus.CrystalFlash.UpdatePalette(bus, crystalCgram, samus),
        "Crystal Flash finish restores beam palette");
    AssertEqual(0x0300, crystalCgram.Colors[0xe0],
        "Crystal Flash finish restores beam palette color zero");
    AssertEqual(0x030f, crystalCgram.Colors[0xef],
        "Crystal Flash finish restores all sixteen beam palette colors");
    AssertEqual(0, samus.CrystalFlash.SpecialPaletteType,
        "Crystal Flash palette handler clears after restoration");

    var left = new SamusState
    {
        Pose = SamusPoseIds.FacingLeftNormalPose,
        Health = 50,
        Missiles = 10,
        SuperMissiles = 10,
        PowerBombs = 10,
    };
    left.RefreshCollisionRadii(bus);
    AssertTrue(left.CrystalFlash.TryBegin(bus, left, chord),
        "left-facing Crystal Flash begins");
    AssertEqual(SamusPoseIds.CrystalFlashLeftPose, left.Pose,
        "source direction selects left Crystal Flash pose");

    Console.WriteLine("  Crystal Flash: prerequisites, handlers, resources, HDMA bubble, palette, and ROM animation agree.");
}

/// <summary>
/// Walks `$91:E16D/$91:EEA6/$91:FCAF`, `$90:E94F`, `$88:86EF-$8AA3`, and
/// `$91:DCB4-$DD30`: admission, all four bodies, dedicated turning, angle frames, setup,
/// widening, aiming, visor palette, teardown, and the native crouched-turn stand-up glitch.
/// </summary>
static void VerifySamusXray()
{
    var bus = new TestAddressSpace();

    // Complete literal pose records from `$91:B631/B751/B851/BCD1-BCF9`. X-ray does not
    // invent a new movement type: standing bodies are zero, crouched bodies five, and its
    // intermediate turn bodies use `$0E`, which makes the installed movement handler RTS.
    (byte Pose, byte[] Definition)[] poses = [
        (SamusPoseIds.FacingRightNormalPose, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.FacingLeftNormalPose, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.TurningRightToLeftPose, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.TurningLeftToRightPose, [0x08, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.CrouchingRightPose, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.CrouchingLeftPose, [0x04, 0x05, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.TurningRightToLeftCrouchingPose, [0x04, 0x0e, 0xff, 0xfb, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.TurningLeftToRightCrouchingPose, [0x08, 0x0e, 0xff, 0xfb, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.XrayingStandingRightPose, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.XrayingStandingLeftPose, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.XrayingCrouchingRightPose, [0x08, 0x05, 0xff, 0x02, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.XrayingCrouchingLeftPose, [0x04, 0x05, 0xff, 0x07, 0x00, 0x00, 0x10, 0x00]),
    ];
    foreach ((byte pose, byte[] definition) in poses)
        WritePoseDefinition(bus, pose, definition);

    // The four X-ray poses share retail `$0F,$0F,$0F,$0F,$0F,$FF`. Synthetic one-tick
    // turn/ordinary lists make the frame-two/timer-one completion seam deterministic while
    // preserving the byte-indexed command shape expected by the production interpreter.
    ushort nextStream = 0xc600;
    foreach ((byte pose, _) in poses)
    {
        ushort stream = nextStream;
        nextStream = unchecked((ushort)(nextStream + 0x10));
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        bool xrayPose = pose is
            SamusPoseIds.XrayingStandingRightPose or SamusPoseIds.XrayingStandingLeftPose or
            SamusPoseIds.XrayingCrouchingRightPose or SamusPoseIds.XrayingCrouchingLeftPose;
        bus.WriteBytes(
            0x910000 | stream,
            xrayPose ? [0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0xff] : [0x01, 0x01, 0x01, 0xff]);
    }

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

    var cgram = new SnesCgram();
    var standing = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        XPosition = 100,
        YPosition = 200,
    };
    standing.RefreshCollisionRadii(bus);
    standing.InitializeAnimation(bus);

    AssertTrue(
        standing.Xray.TryBegin(bus, standing, previousMovementType: SamusMovementType.Standing),
        "standing X-ray setup accepted");
    AssertEqual(0xd5, standing.Pose, "right standing X-ray pose");
    AssertEqual(21, standing.Kinematics.YRadius, "standing X-ray radius");
    AssertEqual(2, standing.AnimationFrame, "command five starts X-ray frame two");
    AssertEqual(0x3f, standing.AnimationFrameTimer, "command five X-ray timer");
    AssertEqual(SnesAngle.QuarterTurn.TableIndex, standing.Xray.Angle.TableIndex,
        "right X-ray initial angle");
    AssertEqual(1, standing.Xray.SetupStage, "X-ray starts setup stage one");
    AssertTrue(standing.Xray.TimeIsFrozen, "X-ray freezes time");
    AssertTrue(standing.Xray.ActivationSoundRequested, "X-ray activation sound requested");
    AssertTrue(standing.Xray.ConsumeActivationSoundRequest(),
        "X-ray startup sound is consumable exactly once");
    AssertTrue(!standing.Xray.ConsumeActivationSoundRequest(),
        "consumed X-ray startup sound cannot replay");

    AssertEqual(2, standing.Xray.StepMovement(bus, standing)!.Value,
        "angle 40 selects forward X-ray frame");
    AssertEqual(15, standing.AnimationFrameTimer, "X-ray movement forces timer fifteen");
    standing.AnimateNoFx(bus);
    AssertEqual(14, standing.AnimationFrameTimer,
        "generic animation follows X-ray movement timer write");

    // Palette handler eight runs in the palette-FX phase. Timer one expires immediately,
    // writes only visor color four, advances byte offset zero to two, and reloads five.
    AssertTrue(standing.Xray.UpdatePalette(bus, cgram, standing.EquippedItems),
        "X-ray widening palette writes first visor color");
    AssertEqual(0x3be0, cgram.Colors[196], "first widening visor color");
    AssertEqual(2, standing.Xray.SpecialPaletteFrame, "widening palette offset advances");
    AssertEqual(SamusXrayRomData.Palette.FrameDelay, standing.Xray.CommonPaletteTimer,
        "widening palette timer reload");

    // Eight instruction-list setup functions execute before the main bank-$88 preinstruction.
    // The eighth call clears SetupStage; the next call changes X-ray state zero to one.
    for (int stage = 1; stage <= 8; stage++)
        standing.Xray.StepBeam(bus, standing, (ushort)SnesButton.B);
    AssertEqual(0, standing.Xray.SetupStage, "eight X-ray setup stages complete");
    AssertEqual(XrayBeamPhase.NoBeam, standing.Xray.BeamPhase, "setup retains state zero");
    standing.Xray.StepBeam(bus, standing, (ushort)SnesButton.B);
    AssertEqual(XrayBeamPhase.Widening, standing.Xray.BeamPhase, "state zero starts widening");

    // Fixed-point acceleration reaches 10.F800 after 26 widening calls and would cross
    // eleven on call 27; native clamps the whole width to ten, clears its fraction, and
    // advances to full-beam state two on that exact call.
    for (int frame = 1; frame <= 26; frame++)
        standing.Xray.StepBeam(bus, standing, (ushort)SnesButton.B);
    AssertEqual(XrayBeamPhase.Widening, standing.Xray.BeamPhase,
        "X-ray remains widening through call 26");
    AssertEqual(10, standing.Xray.AngularWidth, "call 26 whole width");
    AssertEqual(0xf800, standing.Xray.AngularSubwidth, "call 26 fractional width");
    standing.Xray.StepBeam(bus, standing, (ushort)SnesButton.B);
    AssertEqual(XrayBeamPhase.Full, standing.Xray.BeamPhase, "call 27 reaches full beam");
    AssertEqual(10, standing.Xray.AngularWidth, "full beam clamps width ten");
    AssertEqual(0, standing.Xray.AngularSubwidth, "full beam clears width fraction");

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
        layer1X: 0,
        layer1Y: 0);
    AssertEqual(248, xrayFrame[184 * 256 + 255].R,
        "X-ray horizontal center remains inside window");
    AssertEqual(123, xrayFrame[184 * 256].R,
        "X-ray opposite half-plane receives outside half color math");
    AssertEqual(248, xrayFrame[174 * 256 + 142].R,
        "X-ray upper tangent boundary is inclusive after 8.8 truncation");
    AssertEqual(123, xrayFrame[174 * 256 + 141].R,
        "X-ray pixel beyond upper tangent boundary is outside");
    AssertEqual(248, xrayFrame[194 * 256 + 142].R,
        "X-ray lower tangent boundary mirrors upper boundary");
    AssertEqual(248, xrayFrame[0].R,
        "X-ray gameplay window never modifies the IRQ-owned HUD band");

    AssertTrue(standing.Xray.UpdatePalette(bus, cgram, standing.EquippedItems),
        "full beam enters visor cycle");
    AssertEqual(1, standing.Xray.BeamSizeFlag, "full beam palette flag");
    AssertEqual(0x43ff, cgram.Colors[196], "first full-beam visor color");
    AssertEqual(8, standing.Xray.SpecialPaletteFrame, "full-beam palette offset advances");

    // Full-beam aiming moves one angle unit per call and Up wins over Down. Width ten clamps
    // the right-facing center at angle ten, so 80 calls cannot wrap into left-facing space.
    standing.Xray.StepBeam(
        bus,
        standing,
        (ushort)(SnesButton.B | SnesButton.Up | SnesButton.Down));
    AssertEqual(0x3f, standing.Xray.Angle.TableIndex, "X-ray Up wins over Down");
    for (int frame = 0; frame < 80; frame++)
        standing.Xray.StepBeam(bus, standing, (ushort)(SnesButton.B | SnesButton.Up));
    AssertEqual(10, standing.Xray.Angle.TableIndex, "right X-ray upper clamp includes width");
    AssertEqual(0, standing.Xray.StepMovement(bus, standing)!.Value,
        "upper-clamped angle selects looking-up art");

    // Start a turn while the dedicated handler owns input. `$0100-angle` mirrors ten to
    // F6, pose `$25` supplies type `$0E`, and two one-tick animation advances reach the
    // exact frame-two/timer-one completion gate before `$D6` is installed.
    XrayPoseInputResult startedTurn = standing.Xray.HandlePoseInput(
        bus,
        standing,
        (ushort)SnesButton.Left);
    AssertTrue(startedTurn.StartedTurn, "X-ray starts standing turn");
    AssertEqual(0x25, standing.Pose, "X-ray right-to-left standing turn pose");
    AssertEqual(0xf6, standing.Xray.Angle.TableIndex, "X-ray turn mirrors angle");
    AssertTrue(standing.Xray.StepMovement(bus, standing) is null,
        "X-ray movement is RTS during type-E turn");
    standing.AnimateNoFx(bus);
    standing.AnimateNoFx(bus);
    AssertEqual(2, standing.AnimationFrame, "X-ray turn reaches frame two");
    AssertEqual(1, standing.AnimationFrameTimer, "X-ray turn reaches timer one");
    XrayPoseInputResult completedTurn = standing.Xray.HandlePoseInput(bus, standing, 0);
    AssertTrue(completedTurn.CompletedTurn, "X-ray completes standing turn");
    AssertEqual(0xd6, standing.Pose, "X-ray turn installs left standing body");
    AssertEqual(0, standing.Xray.StepMovement(bus, standing)!.Value,
        "left near-up angle selects looking-up art");

    // Releasing Dash enters states three/four/five. State five restores ordinary left
    // standing, requests palette restoration and sound ten, and unfreezes every subsystem.
    standing.Xray.StepBeam(bus, standing, 0);
    AssertEqual(XrayBeamPhase.RestoreFirstHalf, standing.Xray.BeamPhase,
        "Dash release starts first BG2 restore");
    standing.Xray.StepBeam(bus, standing, 0);
    standing.Xray.StepBeam(bus, standing, 0);
    XrayBeamStepResult finished = standing.Xray.StepBeam(bus, standing, 0);
    AssertTrue(finished.Completed, "X-ray state five completes");
    AssertTrue(!standing.Xray.IsActive && !standing.Xray.TimeIsFrozen,
        "X-ray teardown restores time and handlers");
    AssertEqual(0x02, standing.Pose, "left X-ray exits to ordinary standing");
    AssertEqual(0xffff, standing.Xray.BeamSizeFlag,
        "X-ray teardown requests palette restoration");
    AssertTrue(standing.Xray.DeactivationSoundRequested, "X-ray deactivation sound requested");
    AssertTrue(standing.Xray.ConsumeDeactivationSoundRequest(),
        "X-ray teardown sound is consumable exactly once");
    AssertTrue(!standing.Xray.ConsumeDeactivationSoundRequest(),
        "consumed X-ray teardown sound cannot replay");
    AssertTrue(standing.Xray.UpdatePalette(bus, cgram, standing.EquippedItems),
        "X-ray teardown restores normal suit palette");
    AssertEqual(0x0104, cgram.Colors[196], "normal palette replaces visor color");
    AssertEqual(0, standing.Xray.SpecialPaletteType, "X-ray palette handler clears");

    // Crouched setup selects `$D9`. Releasing while its `$43` turn is still active makes
    // `$91:E2AD` classify movement type `$0E` as standing, choose left `$02`, expand radius
    // 16 -> 21, and move the center five pixels upward: the retail X-ray stand-up glitch.
    var crouched = new SamusState
    {
        Pose = SamusPoseIds.CrouchingRightPose,
        XPosition = 100,
        YPosition = 200,
    };
    crouched.RefreshCollisionRadii(bus);
    crouched.InitializeAnimation(bus);
    AssertTrue(crouched.Xray.TryBegin(
        bus,
        crouched,
        previousMovementType: SamusMovementType.Crouching),
        "crouched X-ray setup accepted");
    AssertEqual(0xd9, crouched.Pose, "right crouched X-ray pose");
    AssertEqual(16, crouched.Kinematics.YRadius, "crouched X-ray radius");
    crouched.Xray.HandlePoseInput(bus, crouched, (ushort)SnesButton.Left);
    AssertEqual(0x43, crouched.Pose, "crouched X-ray turn pose");
    AssertEqual(16, crouched.Kinematics.YRadius, "crouched turn retains radius");
    for (int stage = 1; stage <= 8; stage++)
        crouched.Xray.StepBeam(bus, crouched, 0);
    crouched.Xray.StepBeam(bus, crouched, 0); // State 0 -> state 3 on released Dash.
    crouched.Xray.StepBeam(bus, crouched, 0); // State 3 -> state 4.
    crouched.Xray.StepBeam(bus, crouched, 0); // State 4 -> state 5.
    crouched.Xray.StepBeam(bus, crouched, 0); // State 5 -> teardown.
    AssertEqual(0x02, crouched.Pose, "crouched-turn release triggers standing-left glitch");
    AssertEqual(21, crouched.Kinematics.YRadius, "stand-up glitch expands radius");
    AssertEqual(195, crouched.YPosition, "stand-up glitch moves center up five pixels");

    // Admission failures are kept independent so no broad host-side `grounded` boolean can
    // accidentally replace the native previous/current type, landing, velocity, and rare
    // five-bomb conjunction checks.
    SamusState Rejected(byte pose, ushort ySpeed = 0, ushort ySubspeed = 0)
    {
        var sample = new SamusState { Pose = pose };
        sample.RefreshCollisionRadii(bus);
        sample.InitializeAnimation(bus);
        sample.Kinematics.YSpeed = ySpeed;
        sample.Kinematics.YSubspeed = ySubspeed;
        return sample;
    }

    // Seed excluded landing `$A4` and falling `$29` definitions/animations only for gates.
    WritePoseDefinition(bus, SamusPoseIds.NormalLandingRightPose,
        [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.FallingRightPose,
        [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.NormalLandingRightPose * 2, 0xc700);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.FallingRightPose * 2, 0xc710);
    bus.WriteBytes(0x91c700, [0x01, 0xff]);
    bus.WriteBytes(0x91c710, [0x01, 0xff]);
    var landing = Rejected(SamusPoseIds.NormalLandingRightPose);
    AssertTrue(!landing.Xray.TryBegin(bus, landing, previousMovementType: SamusMovementType.Standing),
        "X-ray rejects landing pose");
    var movingVertically = Rejected(SamusPoseIds.FacingRightNormalPose, ySubspeed: 1);
    AssertTrue(!movingVertically.Xray.TryBegin(
        bus,
        movingVertically,
        previousMovementType: SamusMovementType.Standing),
        "X-ray rejects fractional Y velocity");
    var badPrevious = Rejected(SamusPoseIds.FacingRightNormalPose);
    AssertTrue(!badPrevious.Xray.TryBegin(bus, badPrevious, previousMovementType: SamusMovementType.Falling),
        "X-ray rejects unsupported previous movement type");
    var fiveBombQuirk = Rejected(SamusPoseIds.FacingRightNormalPose);
    fiveBombQuirk.XSpeedDivisor = 2;
    AssertTrue(!fiveBombQuirk.Xray.TryBegin(
        bus,
        fiveBombQuirk,
        previousMovementType: SamusMovementType.Standing,
        projectileCooldownTimer: 7,
        bombCounter: 5),
        "X-ray preserves five-bomb cooldown/divisor rejection");

    Console.WriteLine(
        "  X-ray: admission, poses, turns, ROM-tangent window, half color math, visor palette, teardown, and stand-up glitch agree.");
}

/// <summary>
/// Walks `$9B:B3A7-$B85F` and `$90:8976-$89FF`: movement-type-selected `$D7/$D8`
/// setup, animation-only preflash, five death-tile transfers, alternating suit/suitless
/// palettes, 60-call flash, nine explosion frames, non-Samus palette whiteout, and terminal
/// no-draw state.
/// </summary>
static void VerifySamusDeathSequence()
{
    var bus = new TestAddressSpace();

    // Source standing/morph/spin records prove three distinct `$9B:B420` decisions. The
    // death records are literal `$91:BCE1/BCE9`: type `$0A`, ordinary humanoid radii, and
    // right/left direction bytes. Their delay stream is exactly 2,2,2,2,2,2,FE,01.
    WritePoseDefinition(bus, SamusPoseIds.FacingRightNormalPose,
        [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.MorphBallGroundLeftPose,
        [0x04, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.SpinJumpRightPose,
        [0x08, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0b, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.DeathSequenceRightPose,
        [0x08, 0x0a, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.DeathSequenceLeftPose,
        [0x04, 0x0a, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.FacingRightNormalPose * 2, 0xc000);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.MorphBallGroundLeftPose * 2, 0xc010);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.SpinJumpRightPose * 2, 0xc020);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.DeathSequenceRightPose * 2, 0xb567);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.DeathSequenceLeftPose * 2, 0xb567);
    bus.WriteBytes(0x91c000, [0x01, 0xff]);
    bus.WriteBytes(0x91c010, [0x01, 0xff]);
    bus.WriteBytes(0x91c020, [0x01, 0xff]);
    bus.WriteBytes(0x91b567, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xfe, 0x01]);

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
    bus.WriteBytes(0x9bb823,
        [0x15, 0x00, 0x06, 0x02, 0x03, 0x03, 0x04, 0x04, 0x05, 0x05,
         0x05, 0x06, 0x06, 0x07, 0x06, 0x08, 0x50, 0x09]);
    ushort[] shades =
    [
        0x0421, 0x0c63, 0x14a5, 0x1ce7, 0x2529, 0x2d6b, 0x35ad, 0x4210,
        0x4a52, 0x4e73, 0x5294, 0x56b5, 0x5ad6, 0x5ef7, 0x6318, 0x6739,
        0x6b5a, 0x6f7b, 0x739c, 0x77bd, 0x7bde, 0x7fff,
    ];
    WriteTestWords(bus, 0x9bb835, shades);

    var samus = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        XPosition = 0x0480,
        YPosition = 0x04c0,
    };
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);
    SamusDeathSequenceStartResult start = samus.DeathSequence.Begin(
        bus,
        samus,
        layer1X: 0x03e0,
        layer1Y: 0x0400);
    AssertEqual(SamusMovementType.Standing, start.SourceMovementType, "death source standing type");
    AssertEqual(0xd7, start.DeathPose, "death selects right pose");
    AssertEqual(5, start.InitialFrame, "ordinary death starts unmorphed frame five");
    AssertEqual(0x00a0, start.ScreenX, "death captures screen X");
    AssertEqual(0x00c0, start.ScreenY, "death captures screen Y");
    AssertTrue(!start.SpinJumpSoundRequested, "ordinary death does not request spin SFX");
    AssertEqual(2, samus.AnimationFrameTimer, "death pose retains two-tick delay");

    var cgram = new SnesCgram();
    var writes = new VramWriteQueue();
    SamusDeathSequenceStepResult step = default;
    for (int call = 1; call <= 16; call++)
        step = samus.DeathSequence.Step(bus, samus, cgram, writes);
    AssertEqual(SamusDeathSequencePhase.Flashing, samus.DeathSequence.Phase,
        "sixteen preflash calls enter flashing");
    AssertEqual(5, samus.AnimationFrame, "unmorphed death frame loops at five");
    AssertTrue(step.DrawPose, "last preflash call still draws death pose");

    // Calls one through four queue the four high OBJ segments. Call 60 finishes flashing,
    // queues segment four at `$6000`, resets the explosion state, immediately decrements
    // 21 to 20, and draws right-facing spritemap `$81C`.
    for (int call = 1; call <= 60; call++)
        step = samus.DeathSequence.Step(bus, samus, cgram, writes);
    AssertEqual(SamusDeathSequencePhase.SuitExplosion, samus.DeathSequence.Phase,
        "60 flashing calls enter suit explosion");
    AssertEqual(5, writes.Entries.Count, "death queues exactly five tile segments");
    (int Source, ushort Destination)[] expectedSegments =
    [
        (0x9b8400, 0x6200),
        (0x9b8800, 0x6400),
        (0x9b8c00, 0x6600),
        (0x9b9000, 0x6800),
        (0x9b8000, 0x6000),
    ];
    for (int index = 0; index < expectedSegments.Length; index++)
    {
        AssertEqual(0x0400, writes.Entries[index].SizeInBytes,
            $"death segment {index} size");
        AssertEqual(expectedSegments[index].Source, writes.Entries[index].SourceAddress,
            $"death segment {index} source");
        AssertEqual(expectedSegments[index].Destination,
            writes.Entries[index].EncodedVramDestination,
            $"death segment {index} destination");
    }
    AssertEqual(0, samus.DeathSequence.AnimationIndex,
        "explosion begins at index zero");
    AssertEqual(20, samus.DeathSequence.AnimationTimer,
        "same-call first explosion decrement");
    AssertEqual(0x081c, step.ExplosionSpritemapIndex!.Value,
        "right explosion base spritemap");
    AssertEqual(0x0100, cgram.Colors[192], "finish restores suit palette zero");
    AssertEqual(0x0400, cgram.Colors[240], "finish restores suitless palette zero");

    // The remaining literal timers total 135 calls. The terminal call increments index
    // eight to nine, forces white shade 21, and deliberately emits no tenth spritemap.
    int explosionCalls = 0;
    while (samus.DeathSequence.Phase != SamusDeathSequencePhase.Complete)
    {
        step = samus.DeathSequence.Step(bus, samus, cgram, writes);
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

    // Morph Ball begins frame one and uses left pose `$D8`; spin jumping still starts frame
    // five but uniquely requests library-one sound `$32` before pose replacement.
    var morphedLeft = new SamusState
    {
        Pose = SamusPoseIds.MorphBallGroundLeftPose,
        XPosition = 64,
        YPosition = 80,
    };
    morphedLeft.RefreshCollisionRadii(bus);
    morphedLeft.InitializeAnimation(bus);
    SamusDeathSequenceStartResult morphStart = morphedLeft.DeathSequence.Begin(
        bus, morphedLeft, layer1X: 0, layer1Y: 0);
    AssertEqual(0xd8, morphStart.DeathPose, "left Morph death selects D8");
    AssertEqual(1, morphStart.InitialFrame, "Morph death begins unmorph frame one");

    var spinning = new SamusState { Pose = SamusPoseIds.SpinJumpRightPose };
    spinning.RefreshCollisionRadii(bus);
    spinning.InitializeAnimation(bus);
    SamusDeathSequenceStartResult spinStart = spinning.DeathSequence.Begin(
        bus, spinning, layer1X: 0, layer1Y: 0);
    AssertTrue(spinStart.SpinJumpSoundRequested, "spin death requests sound $32");
    AssertTrue(spinning.DeathSequence.ConsumeSpinJumpSoundRequest(),
        "spin-death sound is consumable exactly once");
    AssertTrue(!spinning.DeathSequence.ConsumeSpinJumpSoundRequest(),
        "consumed spin-death sound cannot replay during death animation");
    AssertEqual(5, spinStart.InitialFrame, "spin death begins unmorphed frame five");

    Console.WriteLine(
        "  Samus death: D7/D8 selection, unmorph art, five VRAM segments, flash palettes, whiteout, and nine explosion frames agree.");
}

/// <summary>
/// Exercises all five `$91:E4AD` drained-controller entries, command `$F7`, the shared
/// old-speed vertical recurrence, collision handoff, and both asymmetric `$FD` releases.
/// </summary>
}
