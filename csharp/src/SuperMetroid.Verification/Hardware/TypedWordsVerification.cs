using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Map;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus;

/// <summary>Exercises semantic native-word views while checking that unnamed cartridge bits survive typed operations.</summary>
internal static class TypedWordsVerification
{

/// <summary>
/// Proves that the semantic enums and packed-word views decode verified fields without
/// normalizing, discarding, or assigning meaning to any other native bits.
/// </summary>
internal static void VerifyTypedNativeWords()
{
    // Equipment helpers must mutate only the requested independent bits: Varia, which neither names,
    // survives both operations unchanged.
    SamusEquipmentFlags equipment = SamusEquipmentFlags.VariaSuit;
    equipment |= SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.Bombs;
    AssertTrue(equipment.HasAll(SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.Bombs),
        "typed equipment flags set independently");
    equipment &= ~SamusEquipmentFlags.GravitySuit;
    AssertEqual(0x1001, equipment.ToEquipmentWord(), "equipment flags keep the item neither operation names");

    // The codec is the one place that knows each item's word bit: the word round-trips, and a word
    // setting a bit that records no item is rejected rather than dropped.
    AssertEqual(equipment, SamusEquipmentWordCodec.FromEquipmentWord(0x1001), "equipment word decodes to its items");
    // The equipment word's bit 4 records no item; Golden Torizo's item code sets it and decoding drops it (#1778).
    AssertEqual(SamusEquipmentFlags.VariaSuit, SamusEquipmentWordCodec.FromEquipmentWord(0x0011), "the unused item bit is dropped");
    AssertEqual((ushort)0xf32f, SamusEquipmentWordCodec.FromEquipmentWord(0xf32f).ToEquipmentWord(),
        "every recorded item bit round-trips through the codec");
    AssertThrows<InvalidDataException>(() => SamusEquipmentWordCodec.FromEquipmentWord(0x0080),
        "an equipment word bit that records no item is rejected");
    AssertEqual(SamusBeamFlags.Charge | SamusBeamFlags.Plasma | SamusBeamFlags.Wave, SamusEquipmentWordCodec.FromBeamWord(0x1009),
        "beam word decodes to its beams");
    AssertEqual((ushort)0x100f, SamusEquipmentWordCodec.FromBeamWord(0x100f).ToEquipmentWord(),
        "every recorded beam bit round-trips through the codec");
    AssertThrows<InvalidDataException>(() => SamusEquipmentWordCodec.FromBeamWord(0x0010),
        "a beam word bit that records no beam is rejected");

    // #1456: a checksum-valid save slot may set bits that record nothing, which the cartridge never reads. The save
    // boundary keeps exactly the items and beams the words record, and re-encoding writes the other bits as zero.
    AssertEqual(SamusEquipmentWordCodec.FromEquipmentWord(0xf33f), SamusEquipmentWordCodec.FromSavedEquipmentWord(0xffff),
        "a saved equipment word decodes to the items it records");
    AssertEqual((ushort)0xf32f, SamusEquipmentWordCodec.FromSavedEquipmentWord(0xffff).ToEquipmentWord(),
        "a saved equipment word's unassigned bits, the unused item bit among them (#1778), re-encode as zero");
    AssertEqual(SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma | SamusBeamFlags.Charge,
        SamusEquipmentWordCodec.FromSavedBeamWord(0xffff), "a saved beam word decodes to the beams it records");
    AssertEqual((ushort)0x100f, SamusEquipmentWordCodec.FromSavedBeamWord(0xffff).ToEquipmentWord(),
        "a saved beam word's unassigned bits re-encode as zero");

    // The snapshot boundary decodes $0A76 and $0DC0 into typed state and rejects undefined words.
    AssertEqual(true, SamusSnapshotWords.DecodeHyperBeam(SamusSnapshotWords.HyperBeamEnabledWord), "hyper beam $8000 imports as enabled");
    AssertEqual(false, SamusSnapshotWords.DecodeHyperBeam(0), "hyper beam zero imports as disabled");
    AssertThrows<InvalidDataException>(() => SamusSnapshotWords.DecodeHyperBeam(1), "undefined hyper beam word is rejected");
    AssertEqual(SamusSnapshotWords.HyperBeamEnabledWord, SamusSnapshotWords.EncodeHyperBeam(true), "enabled hyper beam encodes as $8000");
    AssertEqual(false, SamusSnapshotWords.DecodeResumeChargingBeamSound(0), "resume latch zero is clear");
    AssertEqual(true, SamusSnapshotWords.DecodeResumeChargingBeamSound(1), "resume latch one resumes next pass");
    AssertThrows<InvalidDataException>(() => SamusSnapshotWords.DecodeResumeChargingBeamSound(ushort.MaxValue), "a resume latch word the game never writes is rejected");
    AssertThrows<InvalidDataException>(() => SamusSnapshotWords.DecodeResumeChargingBeamSound(2), "undefined resume latch word is rejected");

    // The snapshot boundary decodes $0B18, $0CD0/$0DC2 and $0A46 into typed state (#1682): every word the game writes
    // round-trips, and every other word is rejected.
    for (ushort word = 0; word <= 4; word++)
        AssertEqual(word, SamusSnapshotWords.EncodeChargedShotGlow(SamusSnapshotWords.DecodeChargedShotGlow(word)), $"charged-shot glow word {word} round-trips");
    AssertEqual(ChargedShotGlow.ChargedShotFlash, SamusSnapshotWords.DecodeChargedShotGlow(4), "a charged shot's glow word four is its flash's start");
    for (ushort word = 1 << 15; word <= (1 << 15) + 20; word++)
        AssertEqual(word, SamusSnapshotWords.EncodeChargedShotGlow(SamusSnapshotWords.DecodeChargedShotGlow(word)), $"Hyper Beam glow word {word:X4} round-trips");
    AssertEqual(ChargedShotGlow.HyperBeamGlow, SamusSnapshotWords.DecodeChargedShotGlow((1 << 15) + 20), "the Hyper Beam shot's glow word is its glow's start");
    AssertEqual(ChargedShotGlow.Of(ChargedShotGlowKind.HyperBeamGlow, 0), SamusSnapshotWords.DecodeChargedShotGlow(1 << 15), "the Hyper Beam glow's last step is its marker alone");
    AssertThrows<InvalidDataException>(() => SamusSnapshotWords.DecodeChargedShotGlow(5), "a glow word past the flash is rejected");
    AssertThrows<InvalidDataException>(() => SamusSnapshotWords.DecodeChargedShotGlow((1 << 15) + 21), "a glow word past the Hyper Beam glow is rejected");
    for (ushort word = 0; word <= BeamChargeDefinitions.SpecialAttackCounter; word++)
        AssertEqual(word, SamusSnapshotWords.EncodeBeamCharge(SamusSnapshotWords.DecodeBeamCharge(word)), $"beam charge word {word} round-trips");
    AssertEqual(BeamChargeLevel.HyperBeam, SamusSnapshotWords.DecodeBeamCharge(1 << 15), "beam charge $8000 is the Hyper Beam's flare");
    AssertEqual((ushort)(1 << 15), SamusSnapshotWords.EncodeBeamCharge(BeamChargeLevel.HyperBeam), "the Hyper Beam's flare encodes as $8000");
    AssertThrows<InvalidDataException>(() => SamusSnapshotWords.DecodeBeamCharge(BeamChargeDefinitions.SpecialAttackCounter + 1), "a charge past the clamp is rejected");
    AssertTrue(SamusSnapshotWords.AlignsToSlopes(3), "the slope-alignment word the game writes enables the alignment");
    AssertTrue(!SamusSnapshotWords.AlignsToSlopes(1), "a slope-alignment word without its enable bit disables it");

    // Samus counts are signed words: the cartridge's Crystal Flash drain takes an ammunition count below zero.
    AssertEqual(SamusQuantity.MaximumValue, SamusCountWords.Decode(0x7fff), "the largest count word decodes");
    AssertEqual(-1, SamusCountWords.Decode(0xffff), "a drained-past-zero word decodes as -1");
    AssertEqual(SamusQuantity.MinimumValue, SamusCountWords.Decode(0x8000), "the smallest count word decodes");
    AssertEqual((ushort)0x7fff, SamusCountWords.Encode(SamusQuantity.MaximumValue), "the largest count encodes");
    AssertEqual((ushort)0xffff, SamusCountWords.Encode(-1), "-1 encodes as the word the cartridge stores");
    AssertThrows<InvalidOperationException>(() => SamusCountWords.Encode(SamusQuantity.MinimumValue - 1), "an undersized count cannot be stored");
    AssertThrows<InvalidOperationException>(() => SamusCountWords.Encode(SamusQuantity.MaximumValue + 1), "an oversized count cannot be stored");
    AssertEqual(-1, SamusQuantity.DecrementWrapping(0), "the unchecked decrement takes zero to -1");
    AssertEqual(SamusQuantity.MaximumValue, SamusQuantity.DecrementWrapping(SamusQuantity.MinimumValue), "the unchecked decrement wraps the smallest count");

    // The shown suit is a three-way choice. Verify all three branches plus the native
    // Gravity-over-Varia priority that motivated centralization.
    AssertEqual(SamusSuit.Power, SamusEquipmentFlags.None.ShownSuit(),
        "Power Suit shown without Varia or Gravity");
    AssertEqual(SamusSuit.Varia, SamusEquipmentFlags.VariaSuit.ShownSuit(),
        "Varia Suit shown with Varia");
    AssertEqual(SamusSuit.Gravity, SamusEquipmentFlags.GravitySuit.ShownSuit(),
        "Gravity Suit shown with Gravity");
    AssertEqual(SamusSuit.Gravity,
        (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit).ShownSuit(),
        "Gravity Suit shown over Varia");

    // Facing is an ordinary discriminator, not a flag set. The typed view recognizes left and right and faces
    // neither way for every other record; the record's own byte stays exact in the pose-record codec.
    var poseBus = new TestAddressSpace();
    AssertTrue(SamusPoseMetadata.IsFacingLeft(SamusPoseId.FacingLeftNormalPose), "typed left-facing pose query");
    AssertEqual(SamusFacingDirection.Right, SamusPoseMetadata.ReadFacingDirection(SamusPoseId.FacingRightNormalPose),
        "typed right-facing pose query");
    AssertEqual(SamusFacingDirection.ForwardOrSpecial, SamusPoseMetadata.ReadFacingDirection(SamusPoseNumbering.FromNumber(0xfe)),
        "typed facing query faces an unnamed adjacent-code record neither way");
    AssertEqual(41, SamusPoseRecordCodecTooling.FacingByte(SamusPoseNumbering.FromNumber(0xfe)),
        "pose-record codec writes the unnamed adjacent-code record's own facing byte");

    // Fixed-bank pointer arithmetic must wrap only the low word. A host addition would
    // incorrectly cross from bank $91 into $92 and read unrelated cartridge data.
    AssertEqual(0x910000, SnesAddressMath.AddWithinBank(0x91ffff, 1),
        "fixed-bank address addition wraps low word");
    AssertEqual(0x91fffe, SnesAddressMath.AddWithinBank(0x910002, -4),
        "fixed-bank address addition wraps negative displacement");

    var typedAddress = new SnesAddress(0x91, 0xb629);
    AssertEqual((byte)0x91, typedAddress.Bank, "typed SNES address bank");
    AssertEqual((ushort)0xb629, typedAddress.Offset, "typed SNES address offset");
    AssertEqual(0x91b629, (int)typedAddress, "typed SNES address explicit bus value");
    AssertEqual(0x910000, (int)new SnesAddress(0x91, 0xffff).AddWithinBank(1),
        "typed fixed-bank addition wraps without bank carry");
    AssertEqual(0x91ffff, (int)new SnesAddress(0x91, 0).AddWithinBank(-1),
        "typed fixed-bank subtraction wraps without bank borrow");
    AssertTrue(SnesAddress.FromUpperLoRom(0x94, 0xe000).IsUpperLoRomWindow,
        "typed upper-LoROM validation accepts cartridge window");
    AssertThrows<ArgumentOutOfRangeException>(
        () => SnesAddress.FromUpperLoRom(0x94, 0x7fff),
        "typed upper-LoROM validation rejects lower bank window");
    AssertThrows<ArgumentOutOfRangeException>(
        () => SnesAddress.FromBusAddress(0x0100_0000),
        "typed SNES address rejects a 25-bit host value");
    AssertEqual(0x958000, (int)new SnesAddress(0x94, 0xffff).NextLoRomByte(),
        "typed linear LoROM step crosses to next upper window");
    AssertEqual(0x940000, (int)new SnesAddress(0x94, 0xffff).AddWithinBank(1),
        "typed fixed-bank step remains distinct from linear LoROM step");

    AssertEqual(256, ScreenGeometry.ScreenWidthPixels, "PPU low-resolution screen width");
    AssertEqual(224, ScreenGeometry.ScreenHeightPixels, "PPU visible screen height");
    AssertEqual(192, ScreenGeometry.GameplayViewportHeightPixels,
        "gameplay viewport excludes the 32-line HUD");
    AssertEqual(0x10000, SnesVram.ByteCount, "PPU VRAM byte capacity");
    AssertEqual(512, SnesCgram.ColorCount * sizeof(ushort), "PPU CGRAM byte capacity");
    AssertEqual(544, SnesPpuLayoutConstants.OamUploadByteCount, "PPU OAM upload size");
    AssertEqual(0x0400, SuperMetroid.AssetExtraction.Video.TilemapScreenGeometry.CellCount,
        "PPU 32-by-32 tilemap page size");
    AssertEqual((ushort)0x5000, GameplayVramLayout.Bg1TilemapWord,
        "engine gameplay BG1 tilemap placement");

    ushort[] translatedObjectInstructionCodes =
    [
        (ushort)RoomObjectInstruction.Sleep,
        (ushort)RoomObjectInstruction.Delete,
        (ushort)RoomObjectInstruction.InstallFrameBehavior,
        (ushort)RoomObjectInstruction.Goto,
        (ushort)RoomObjectInstruction.DecrementTimerAndGoto,
        (ushort)RoomObjectInstruction.SetEightBitTimer,
        (ushort)RoomObjectInstruction.CopyFromRamToVram,
        (ushort)RoomObjectInstruction.GotoIfAreaBossBitSet,
        (ushort)RoomObjectInstruction.GotoIfEventSet,
        (ushort)RoomObjectInstruction.SetEvent,
        (ushort)RoomObjectInstruction.DrawObjectBlock,
        (ushort)RoomObjectInstruction.DrawObjectBlockClone,
        (ushort)RoomObjectInstruction.QueueSoundLibrary2Maximum6,
        (ushort)RoomObjectInstruction.QueueSoundLibrary3Maximum6,
        (ushort)RoomObjectInstruction.QueueSoundLibrary2Maximum3,
        (ushort)RoomObjectInstruction.QueueSoundLibrary2Maximum1,
        (ushort)RoomObjectInstruction.QueueSoundLibrary2Maximum1Direct,

        (ushort)RoomObjectInstruction.SetObjectBtsToOne,
        (ushort)RoomObjectInstruction.GotoIfRoomArgumentLess,
        (ushort)RoomObjectInstruction.SpawnFourMotherBrainGlassShards,
        (ushort)RoomObjectInstruction.SpawnTorizoStatueBreaking,
        (ushort)RoomObjectInstruction.QueueSongOneMusicTrack,
    ];
    AssertEqual(translatedObjectInstructionCodes.Length,
        translatedObjectInstructionCodes.Distinct().Count(),
        "translated PLM instruction catalog has unique native pointers");

    // A cartridge turn is a 16-bit wrapping domain whose high byte indexes the shared
    // 256-entry sine tables. Verify named axes, full-turn normalization, fractional
    // preservation, and the signed modular subtraction used near the wrap seam.
    AssertEqual((ushort)0x4000, RotationAngle.QuarterTurn.RawValue,
        "SNES quarter-turn raw word");
    AssertEqual((byte)0x40, RotationAngle.QuarterTurn.ToTrigonometryAngle().RawValue,
        "SNES quarter-turn sine index");
    AssertEqual(0x80, RotationAngle.QuarterTurn.SineTableByteOffset,
        "SNES quarter-turn word-table offset");
    AssertEqual(RotationAngle.Zero, RotationAngle.NormalizeRaw(0x1_0000),
        "SNES raw angle wraps at one turn");
    AssertEqual(RotationAngle.Zero, RotationAngle.WrappedSteps(0x100),
        "SNES table angle wraps at 256 units");
    AssertEqual((ushort)0x41ab, RotationAngle.FromRaw(0x40ab).TurnedBySteps(1).RawValue,
        "SNES whole-unit addition retains fractional angle byte");
    AssertEqual((short)-0x4000, RotationAngle.Zero.SignedDeltaTo(RotationAngle.ThreeQuarterTurn),
        "SNES signed raw delta crosses wrap seam");
    AssertEqual((sbyte)-0x40,
        RotationAngle.Zero.SignedTableDeltaTo(RotationAngle.ThreeQuarterTurn),
        "SNES signed table delta crosses wrap seam");

    var positiveEightEight = new SignedEightEight(0x0180);
    AssertEqual(1, positiveEightEight.WholePixels,
        "signed 8.8 positive whole byte");
    AssertEqual((byte)0x80, positiveEightEight.Fraction,
        "signed 8.8 positive fraction byte");
    AssertEqual(0x0001_8000,
        positiveEightEight.ToSixteenSixteenDelta().RawValue,
        "signed 8.8 promotes to signed 16.16");
    var negativeEightEight = new SignedEightEight(0xff80);
    AssertEqual(unchecked((int)0xffff_8000),
        negativeEightEight.ToSixteenSixteenDelta().RawValue,
        "negative signed 8.8 promotion sign-extends");
    FixedCoordinate halfPixelBack = negativeEightEight
        .ToSixteenSixteenDelta()
        .AddTo(10, 0x4000);
    AssertEqual(new FixedCoordinate(9, 0xc000), halfPixelBack,
        "signed fixed delta carries and borrows across split position words");
    AssertEqual(new FixedCoordinate(0, 0),
        SignedSixteenSixteen.FromWholePixels(1).AddTo(ushort.MaxValue, 0),
        "signed fixed position addition wraps native whole word");
    AssertEqual(unchecked((ushort)0xff00),
        SignedEightEight.FromWholePixels(127).AddRawWrapping(0x8000).RawValue,
        "signed 8.8 arithmetic wraps the cartridge word");
    AssertThrows<OverflowException>(
        () => SignedEightEight.FromWholePixels(128),
        "signed 8.8 checked whole conversion rejects positive overflow");
    AssertThrows<ArgumentOutOfRangeException>(
        () => SignedSixteenSixteen.FromWholePixels(32768),
        "signed 16.16 whole conversion rejects positive overflow");

    CountdownStep fromOne = WrappingCounter.Decrement(0x0001);
    AssertEqual((ushort)0x0000, fromOne.Value, "native DEC one reaches zero");
    AssertTrue(fromOne.IsZero && fromOne.IsNonNegative && !fromOne.IsNegative,
        "native DEC one exposes Z without N");
    CountdownStep fromZero = WrappingCounter.Decrement(0x0000);
    AssertEqual(ushort.MaxValue, fromZero.Value, "native DEC zero wraps to $FFFF");
    AssertTrue(fromZero.IsNegative && fromZero.Underflowed && !fromZero.IsZero,
        "native DEC zero exposes N and underflow without Z");
    CountdownStep fromMaximum = WrappingCounter.Decrement(ushort.MaxValue);
    AssertEqual((ushort)0xfffe, fromMaximum.Value, "native DEC $FFFF reaches $FFFE");
    AssertTrue(fromMaximum.IsNegative && !fromMaximum.Underflowed,
        "native DEC $FFFF retains N without reporting zero underflow");
    CountdownStep ordinaryCounter = WrappingCounter.Decrement(0x1234);
    AssertEqual((ushort)0x1233, ordinaryCounter.Value, "native DEC ordinary positive value");
    AssertTrue(ordinaryCounter.IsNonNegative && !ordinaryCounter.IsZeroOrNegative,
        "native DEC ordinary positive result exposes neither expiry flag");
    AssertEqual((ushort)0, WrappingCounter.DecrementSaturating(0),
        "saturating native counter pins zero");
    AssertEqual((ushort)0, WrappingCounter.DecrementSaturating(1),
        "saturating native counter decrements one");
    AssertEqual((ushort)0x1233, WrappingCounter.DecrementSaturating(0x1234),
        "saturating native counter decrements ordinary positive value");

    // #1324: the stored countdown keeps each native branch's expiry count and the exact wrapped word.
    CountdownStep clearedStep = WrappingCountdown.Start(0).Decrement();
    AssertTrue(clearedStep.IsNegative, "a cleared countdown goes negative on the first DEC (DEC then BPL)");
    AssertEqual(ushort.MaxValue, clearedStep.Remaining.RawValue, "a cleared countdown is left holding the wrapped $FFFF");
    AssertTrue(clearedStep.Remaining.IsNegative && !clearedStep.IsZero,
        "the wrapped word reads back negative and does not report reaching zero");
    const int startedFrames = 3;
    WrappingCountdown started = WrappingCountdown.Start(startedFrames);
    int negativeCall = 0;
    for (int call = 1; call <= startedFrames + 1; call++)
    {
        CountdownStep step = started.Decrement();
        started = step.Remaining;
        if (step.IsNegative && negativeCall == 0)
            negativeCall = call;
    }
    AssertEqual(startedFrames + 1, negativeCall, "a countdown started at N goes negative on call N + 1");
    WrappingCountdown reachedZero = WrappingCountdown.Start(0);
    int zeroCall = 0;
    for (int call = 1; call <= ushort.MaxValue + 1; call++)
    {
        CountdownStep step = reachedZero.Decrement();
        reachedZero = step.Remaining;
        if (step.IsZero)
        {
            zeroCall = call;
            break;
        }
    }
    AssertEqual(ushort.MaxValue + 1, zeroCall,
        "reached-zero from a cleared countdown wraps to $FFFF and waits 65,535 more DECs (DEC then BEQ)");
    AssertTrue(WrappingCountdown.Start(1).Decrement().IsZeroOrNegative && WrappingCountdown.Start(0).Decrement().IsZeroOrNegative &&
        !WrappingCountdown.Start(2).Decrement().IsZeroOrNegative, "DEC, BEQ, BMI expires on zero or on wrapping");
    AssertEqual(WrappingCountdown.Start(0), WrappingCountdown.Start(0).DecrementToZero(), "the saturating countdown never wraps");
    AssertEqual(WrappingCountdown.Start(4), WrappingCountdown.Start(5).DecrementToZero(), "the saturating countdown decrements");
    AssertEqual((ushort)0x8001, WrappingCountdown.Start(0x8001).RawValue, "a raw countdown word is preserved exactly");
    AssertThrows<ArgumentOutOfRangeException>(() => WrappingCountdown.Start(ushort.MaxValue + 1),
        "a countdown start must fit the native word");
    AssertThrows<ArgumentOutOfRangeException>(() => WrappingCountdown.Start(-1), "a countdown start cannot be negative");

    // A sustained countdown is the count the game's producers leave when they set the sustained frames' flag in a
    // running timer: the rule matches that flag over the whole count for both sustained lengths the game uses.
    foreach (int sustainedFrames in (int[])[16, 32])
        for (int frames = 0; frames <= ushort.MaxValue; frames++)
            if (WrappingCountdown.Start(frames).Sustained(sustainedFrames).RawValue != (ushort)(frames | sustainedFrames))
                AssertEqual((ushort)(frames | sustainedFrames), WrappingCountdown.Start(frames).Sustained(sustainedFrames).RawValue,
                    $"sustaining {frames} frames for {sustainedFrames} sets the sustained frames' flag");
    AssertEqual(WrappingCountdown.Start(37), WrappingCountdown.Start(5).Sustained(32), "a countdown in the first half of its round moves on");
    AssertEqual(WrappingCountdown.Start(40), WrappingCountdown.Start(40).Sustained(32), "a countdown in the later half of its round is kept");
    AssertEqual(WrappingCountdown.Start(102), WrappingCountdown.Start(70).Sustained(32), "the rounds repeat past twice the sustained frames");
    WrappingCountdown renewed = WrappingCountdown.Start(0);
    for (int update = 0; update < 200; update++)
    {
        renewed = renewed.Sustained(32).Decrement().Remaining;
        AssertTrue(renewed.IsAtLeast(31) && !renewed.IsAtLeast(64), $"a countdown renewed every update stays in its round at update {update}");
    }
    AssertThrows<ArgumentOutOfRangeException>(() => WrappingCountdown.Start(0).Sustained(24),
        "sustained frames that do not divide the countdown's rounds are refused");
    AssertThrows<ArgumentOutOfRangeException>(() => WrappingCountdown.Start(0).Sustained(0), "zero sustained frames are refused");

    // Map cells are all ordinary SNES tile words; elevator, item-dot, and station meanings do
    // not occupy a separate packed field. Give those constructed fixture categories distinct
    // character/attribute combinations to prove the typed boundary never normalizes them.
    // The explored/unexplored operations then prove the two renderer-specific mutations touch
    // only their documented attributes.
    MapCellRoundTrip[] mapCellRoundTrips =
    [
        new MapCellRoundTrip("blank", 0x001f),
        new MapCellRoundTrip("explored room", 0x2812),
        new MapCellRoundTrip("unexplored room", 0x2c12),
        new MapCellRoundTrip("elevator", 0x6c0e),
        new MapCellRoundTrip("item", 0xac05),
        new MapCellRoundTrip("station", 0x2c1a),
    ];
    foreach ((string name, ushort raw) in mapCellRoundTrips)
    {
        var cell = new MapTileWord(raw);
        AssertEqual(raw, cell.Raw, $"{name} map cell raw round trip");
    }
    // The import decodes a map word to the cell it pictures, its mirroring folded into the cell's edges (#1513).
    AssertEqual(MapCell.Empty,
        MapCellCharacters.Picture(MapTileWords.HudBlank.CharacterIndex, MapTileWords.HudBlank.Mirroring),
        "the HUD's hidden cell shows the empty map character");
    AssertEqual(new MapCell(MapCellKind.Room, new(MapCellEdge.Wall,
            MapCellEdge.Wall, MapCellEdge.Open, MapCellEdge.Wall)),
        MapCellCharacters.Decode(new MapTileWord(0x4c21)), "a mirrored room character decodes to the room walled on its mirrored edges");
    AssertThrows<InvalidDataException>(() => MapCellCharacters.Decode(new MapTileWord(0x0021)),
        "a mapped cell in a colour row no retail map stores it in fails to decode");
    AssertEqual((ushort)0xa812, new MapTileWord(0xac12).AsExplored().Raw,
        "explored map cell clears only cartridge palette bit $0400");
    AssertEqual((ushort)0xebff, new MapTileWord(0xffff).ForHud(explored: true).Raw,
        "HUD explored cell preserves character and flips while replacing attributes");
    AssertEqual((ushort)0xec12, new MapTileWord(0xe012).ForHud(explored: false).Raw,
        "HUD unexplored cell preserves character and flips while replacing attributes");
    AssertEqual((ushort)0x3c12, new MapTileWord(0x2812).WithLocationBlink().Raw,
        "HUD location blink applies native palette bits");

    // Every independently composable enemy property is an explicit flag of its concern.
    EnemyContact enemyContact = EnemyContact.IgnoresSamus | EnemyContact.BlocksPlasmaBeam | EnemyContact.SolidToSamus;
    EnemyPresence enemyPresence = EnemyPresence.Hidden;
    EnemyProcessing enemyProcessing = EnemyProcessing.None;
    AssertTrue(enemyContact.HasAny(EnemyContact.IgnoresSamus) && enemyContact.HasAny(EnemyContact.BlocksPlasmaBeam) &&
            enemyContact.HasAny(EnemyContact.SolidToSamus) && enemyPresence.HasAny(EnemyPresence.Hidden),
        "enemy visibility, collision, plasma, and platform flags round trip");
    enemyPresence = enemyPresence.With(EnemyPresence.Deleted);
    enemyProcessing = enemyProcessing.With(EnemyProcessing.OffScreen | EnemyProcessing.Animates);
    AssertTrue(enemyPresence.HasAny(EnemyPresence.Deleted) && enemyProcessing.HasAny(EnemyProcessing.OffScreen) &&
            enemyProcessing.HasAny(EnemyProcessing.Animates),
        "enemy deletion and processing flags round trip");
    enemyPresence = enemyPresence.Without(EnemyPresence.Hidden | EnemyPresence.Deleted);
    enemyProcessing = enemyProcessing.Without(EnemyProcessing.OffScreen | EnemyProcessing.Animates);
    AssertEqual(EnemyContact.IgnoresSamus | EnemyContact.BlocksPlasmaBeam | EnemyContact.SolidToSamus,
        enemyContact, "enemy flag operations clear only the named flags");
    AssertEqual(EnemyPresence.None, enemyPresence, "enemy presence operations clear the named flags");
    AssertEqual(EnemyProcessing.None, enemyProcessing, "enemy processing operations clear the named flags");
    var enemyProperties = new SuperMetroid.AssetExtraction.Enemies.EnemyPropertyState(
        enemyContact, enemyProcessing, enemyPresence, CollidesByFrameHitboxes: true, NewInstructionFrame: false);
    AssertEqual(enemyProperties, SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.Decode(
        SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.EncodePropertyWord(enemyProperties),
        SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.EncodeExtraPropertyWord(enemyProperties)),
        "enemy properties round trip their native words");

    // The import's level-word codec: $E selects the grapple collision kind, $C supplies both
    // mirrorings, and $02A is the block. The typed block holds each field by name.
    RoomTerrainBlock grapple = RoomTerrainBlock.Of(42, BlockMirroring.Horizontal | BlockMirroring.Vertical,
        RoomCollisionType.GrappleBlock);
    AssertEqual(grapple, RoomLevelWordCodec.Decode(0xec2a), "level word decodes kind, block and mirroring");
    AssertEqual(0xec2a, (int)RoomLevelWordEncoder.Encode(grapple), "typed block encodes the level word");
    AssertEqual(grapple.Appearance, RoomLevelWordCodec.DecodeAppearance(0xec2a), "appearance ignores the collision kind");
    AssertEqual(0x0c2a, (int)RoomLevelWordEncoder.EncodeAppearance(grapple.Appearance), "appearance encodes without collision bits");
    AssertEqual(RoomTerrainBlock.Of(42, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
        grapple.WithCollision(RoomCollisionType.HorizontalExtension), "collision replacement keeps the appearance");
    AssertEqual(RoomTerrainBlock.Of(42, BlockMirroring.Vertical, RoomCollisionType.GrappleBlock),
        grapple.WithAppearance(grapple.Appearance.WithMirroring(BlockMirroring.Vertical)),
        "mirroring replacement keeps the kind and block");
    AssertThrows<ArgumentOutOfRangeException>(
        () => grapple.Appearance.WithMirroring((BlockMirroring)4),
        "an appearance mirrors only horizontally and vertically");
    AssertThrows<ArgumentOutOfRangeException>(
        () => RoomBlockAppearance.Of(RoomBlockAppearance.LastBlock + 1),
        "an appearance names a block of the tileset's set");

    RoomCollisionType[] retailCollisionTypes =
    [
        RoomCollisionType.Air,
        RoomCollisionType.Slope,
        RoomCollisionType.SpikeAir,
        RoomCollisionType.SpecialAir,
        RoomCollisionType.ShootableAir,
        RoomCollisionType.HorizontalExtension,
        RoomCollisionType.UnrevealedAir,
        RoomCollisionType.BombableAir,
        RoomCollisionType.SolidBlock,
        RoomCollisionType.DoorBlock,
        RoomCollisionType.SpikeBlock,
        RoomCollisionType.SpecialBlock,
        RoomCollisionType.ShootableBlock,
        RoomCollisionType.VerticalExtension,
        RoomCollisionType.GrappleBlock,
        RoomCollisionType.BombableBlock,
    ];
    for (byte collisionNibble = 0; collisionNibble < retailCollisionTypes.Length;
         collisionNibble++)
    {
        RoomTerrainBlock decoded = RoomLevelWordCodec.Decode((ushort)(collisionNibble << 12));
        AssertEqual(retailCollisionTypes[collisionNibble], decoded.Collision,
            $"level collision nibble ${collisionNibble:X1}");
        AssertEqual(collisionNibble << 12, (int)RoomLevelWordEncoder.Encode(decoded),
            $"level collision kind encodes nibble ${collisionNibble:X1}");
        AssertTrue(decoded.StopsAttachment == (collisionNibble >= 8),
            $"attachment stops at the solid kinds, the upper half of the nibbles (${collisionNibble:X1})");
        AssertEqual(collisionNibble >= 8 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air, decoded.Deactivated().Collision,
            $"deactivation keeps solid kinds solid and makes the rest air (${collisionNibble:X1})");
    }

    // The import decodes every reading of the behaviour byte at once; each collision kind reads its own. Exercise each
    // reading independently so no future cleanup conflates overlapping bits.
    var reflectedSlopeOrAreaReaction = RoomBlockBehaviorCodec.Decode(0xff);
    AssertEqual(-1, reflectedSlopeOrAreaReaction.ExtensionOffset,
        "BTS signed extension offset");
    AssertEqual(0x1f, reflectedSlopeOrAreaReaction.Slope.Shape,
        "BTS slope shape field");
    AssertTrue(reflectedSlopeOrAreaReaction.Slope.IsNonSquare &&
        reflectedSlopeOrAreaReaction.Slope.FlipsHorizontally &&
        reflectedSlopeOrAreaReaction.Slope.FlipsVertically,
        "BTS slope contextual fields");
    AssertEqual(BlockMirroring.Both, reflectedSlopeOrAreaReaction.Slope.Mirroring,
        "BTS slope reflections");
    AssertTrue(reflectedSlopeOrAreaReaction.Shootable is null &&
        reflectedSlopeOrAreaReaction.Bombable == BombableReaction.Inert &&
        reflectedSlopeOrAreaReaction.Spike is null &&
        reflectedSlopeOrAreaReaction.Grapple == GrappleReaction.Rejected,
        "BTS area reactions: an inert bomb block and a rejecting grapple block, and no shootable or spike reaction past the tables");
    AssertEqual(0x7f, reflectedSlopeOrAreaReaction.DoorLink,
        "BTS door-link index");
    AssertTrue(RoomBlockBehaviorCodec.Decode(3).Grapple == GrappleReaction.UnrevealedAnchor &&
        ShotBlockReactions.Of(RoomBlockBehaviorCodec.Decode(3).Shootable) == ShotBlockReaction.RespawningBreak,
        "BTS overlapping grapple and breakable-block views stay contextual");
    AssertEqual(ShotBlockReaction.PermanentBreak, ShotBlockReactions.Of(RoomBlockBehaviorCodec.Decode(6).Shootable),
        "BTS permanent breakable-block view");
    AssertEqual(ShootableReaction.PermanentBreak1x2, RoomBlockBehaviorCodec.Decode(6).Shootable,
        "BTS permanent break keeps its block shape");
    AssertTrue(ShotBlockReactions.Of(RoomBlockBehaviorCodec.Decode(8).Shootable) == ShotBlockReaction.RespawningPowerBombBreak &&
        ShotBlockReactions.Of(RoomBlockBehaviorCodec.Decode(9).Shootable) == ShotBlockReaction.PermanentPowerBombBreak &&
        ShotBlockReactions.Of(RoomBlockBehaviorCodec.Decode(10).Shootable) == ShotBlockReaction.RespawningSuperMissileBreak &&
        ShotBlockReactions.Of(RoomBlockBehaviorCodec.Decode(11).Shootable) == ShotBlockReaction.PermanentSuperMissileBreak,
        "BTS weapon-gated shot-block views");
    for (int value = 0; value <= byte.MaxValue; value++)
    {
        // The shot-block dispatcher's own entries are 0 through 63 and 78, and the block that sets off only when solid
        // (the area entries 0 through 7); every other byte is dispatched elsewhere or has no shootable reaction.
        bool dispatched = value is >= 0 and <= 63 or 78 or >= 0x80 and <= 0x87;
        AssertEqual(dispatched, ShotBlockReactions.Of(RoomBlockBehaviorCodec.Decode(value).Shootable) is not null,
            $"BTS ${value:X2} reaches the shot-block dispatcher");
        AssertEqual(value is >= 17 and <= 63 or 78,
            RoomBlockBehaviorCodec.Decode(value).Shootable == ShootableReaction.IgnoresShots,
            $"BTS ${value:X2} is a shot-block entry that ignores shots");
        AssertEqual(value is >= 0x50 and <= 0x7f or >= 0x88,
            RoomBlockBehaviorCodec.Decode(value).Shootable is null,
            $"BTS ${value:X2} is past the shot-block tables, no shootable reaction");
        AssertEqual(value == 0x10, RoomBlockBehaviorCodec.Decode(value).Shootable == ShootableReaction.CollisionProbe,
            $"BTS ${value:X2} is the shot-block collision probe");
    }

    // Every reading of every byte, written as each other reading, is checked by BlockParameterCarryOverVerification.
    AssertEqual(ShootableReaction.NothingWhenSolid, RoomBlockBehaviorCodec.Decode(0x82).Shootable,
        "BTS area-reaction table view");

    RoomBlockBehavior[] blueDoorBehaviors =
    [
        RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Left)),
        RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Right)),
        RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Up)),
        RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Down)),
    ];
    for (int orientationIndex = 0; orientationIndex < blueDoorBehaviors.Length;
         orientationIndex++)
    {
        AssertTrue(blueDoorBehaviors[orientationIndex].TryGetBlueDoorOrientation(
            out ColoredDoorOrientation orientation),
            $"blue-door BTS {orientationIndex} decodes");
        AssertEqual((ColoredDoorOrientation)orientationIndex, orientation,
            $"blue-door BTS {orientationIndex} orientation");
    }
    AssertTrue(!RoomBlockBehavior.Reacting(ShootableReaction.CollectibleTrigger).TryGetBlueDoorOrientation(out _),
        "non-door BTS rejects blue-door view");

    foreach (StationAccessBehavior expectedAccess in Enum.GetValues<StationAccessBehavior>())
    {
        ushort stationValue = RoomObjectBlockBehaviorNativeWords.StationAccesses.Word(expectedAccess);
        var stationBehavior = RoomBlockBehaviorCodec.Decode(stationValue);
        AssertTrue(stationBehavior.TryGetStationAccess(out StationAccessBehavior access),
            $"station BTS ${stationValue:X2} decodes");
        AssertEqual(expectedAccess, access,
            $"station BTS ${stationValue:X2} decodes to its access");
        AssertEqual(stationBehavior, RoomBlockBehavior.Reacting(BlockReactionDefinitions.StationAccesses.ReactionFor(access)),
            $"station BTS ${stationValue:X2} remains lossless");
    }
    AssertTrue(!RoomBlockBehavior.Reacting(SpecialAirReaction.ScrollTrigger).TryGetStationAccess(out _),
        "non-station BTS rejects station view");

    var typedPose = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
    typedPose.PoseId = SamusPoseId.ScrewAttackLeftPose;
    AssertEqual(SamusPoseId.ScrewAttackLeftPose, typedPose.Pose,
        "typed Samus pose writes the native byte");
    typedPose.Pose = SamusPoseNumbering.FromNumber(0xfe);
    AssertEqual(SamusPoseNumbering.FromNumber(0xfe), typedPose.PoseId,
        "undefined Samus pose byte remains observable for diagnostics");

    var bgEntry = new SnesBgTilemapWord(0xf555);
    AssertEqual(0x155, bgEntry.CharacterIndex, "BG tile character field");
    AssertEqual(5, bgEntry.PaletteIndex, "BG tile palette field");
    AssertTrue(bgEntry.HasPriority, "BG tile priority bit");
    AssertEqual(PictureFlip.Horizontal | PictureFlip.Vertical,
        bgEntry.FlipFlags, "BG tile flip flags");
    AssertEqual(0x7555, bgEntry.ToggleFlips(PictureFlip.Vertical).Raw,
        "BG tile toggles only vertical flip");
    SnesBgTilemapWord encodedBg = SnesBgTilemapWord.Create(
        characterIndex: 0x155,
        paletteIndex: 5,
        priority: true,
        PictureFlip.Horizontal | PictureFlip.Vertical);
    AssertEqual(bgEntry, encodedBg, "BG tile encode/decode round trip");
    AssertEqual(0xd955, encodedBg.WithPaletteIndex(6).WithPriority(false).Raw,
        "BG tile field replacement preserves character and flips");

    var objEntry = new SnesObjAttributeWord(0x6dab);
    AssertEqual(0x1ab, objEntry.TileNumber, "OBJ tile-number field");
    AssertEqual(6, objEntry.PaletteIndex, "OBJ palette field");
    AssertEqual(2, objEntry.Priority, "OBJ priority field");
    AssertTrue(objEntry.FlipHorizontally && !objEntry.FlipVertically,
        "OBJ independent flip fields");
    SnesObjAttributeWord encodedObj = SnesObjAttributeWord.Create(
        tileNumber: 0x1ab,
        paletteIndex: 6,
        priority: 2,
        PictureFlip.Horizontal);
    AssertEqual(objEntry, encodedObj, "OBJ attribute encode/decode round trip");
    AssertEqual(0x63ab, encodedObj.WithPaletteIndex(1).Raw,
        "OBJ palette replacement preserves tile, priority, and flips");
    AssertEqual(0x6e00, SnesObjAttributeWord.Create(0, 7, 0).Or(
            SnesObjAttributeWord.Create(0, 0, 2, PictureFlip.Horizontal)).Raw,
        "typed OBJ palette fragment composes with priority and flip fields");
    SnesOamHighTablePair highPair = SnesOamHighTablePair.FromSprite(
        screenX: 0x0100,
        isLarge: true);
    AssertTrue(highPair.XHigh && highPair.IsLarge && highPair.Raw == 3,
        "OAM high-table pair encodes X-high and size independently");
    AssertThrows<ArgumentOutOfRangeException>(
        () => SnesObjAttributeWordConstants.FromPaletteBits(0x1000),
        "OBJ palette fragment rejects priority bits");

    // The import codec decodes a projectile type word once into the projectile it records; the Charge
    // equipment bit ($1000) records nothing the game reads and is not carried.
    SamusProjectileType projectileType = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(0x9219);
    AssertEqual(SamusProjectileFamily.SuperMissile, projectileType.Family,
        "projectile family decodes from the family row");
    AssertEqual(SamusBeamCombination.PlasmaWave, projectileType.BeamCombination, "projectile beams decode from the low nibble");
    AssertTrue(projectileType.IsChargedBeam, "projectile charge decodes from its bit");
    AssertEqual(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(0x9819),
        projectileType.AsFamily(SamusProjectileFamily.MissileExplosion),
        "a projectile becoming another family keeps its beams and charge");
    AssertTrue(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(0).IsNone, "the zero type word is no projectile");

    ProjectileDispatchCase[] projectileDispatchCases =
    [
        new ProjectileDispatchCase(0x801f, SamusProjectileFamily.Beam),
        new ProjectileDispatchCase(0x8110, SamusProjectileFamily.Missile),
        new ProjectileDispatchCase(0x8210, SamusProjectileFamily.SuperMissile),
        new ProjectileDispatchCase(0x8300, SamusProjectileFamily.PowerBomb),
        new ProjectileDispatchCase(0x8501, SamusProjectileFamily.Bomb),
    ];
    foreach ((ushort raw, SamusProjectileFamily family) in projectileDispatchCases)
    {
        SamusProjectileType dispatchType = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(raw);
        AssertEqual(family, dispatchType.Family,
            $"projectile family decoded from raw word ${raw:X4}");
        AssertTrue(dispatchType.IsFamily(family),
            $"projectile family predicate for raw word ${raw:X4}");
    }

    AssertEqual(SamusProjectileDirection.Left, SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeDirection(0xab07),
        "projectile direction decodes from the low nibble");
    AssertTrue(!SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeSpent(0xab07),
        "projectile direction word's high byte records nothing spent");
    AssertTrue(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeSpent(0x0017),
        "projectile low-byte lifecycle bit decodes as spent");
    AssertEqual(SamusBeamCombination.Plasma, SamusBeamCombinations.Of(SamusBeamFlags.Charge | SamusBeamFlags.Plasma),
        "equipped beams select their combination, ignoring Charge");

    AreaId[] retailAreas =
    [
        AreaId.Crateria,
        AreaId.Brinstar,
        AreaId.Norfair,
        AreaId.WreckedShip,
        AreaId.Maridia,
        AreaId.Tourian,
        AreaId.Ceres,
    ];
    for (byte rawArea = 0; rawArea < retailAreas.Length; rawArea++)
    {
        AssertEqual(retailAreas[rawArea], SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Decode(rawArea, "typed-area test"),
            $"retail area byte {rawArea} validates");
        AssertEqual(rawArea, SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(retailAreas[rawArea]),
            $"retail area {retailAreas[rawArea]} indexes cartridge tables");
    }
    AssertThrows<InvalidDataException>(
        () => SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Decode(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count, "typed-area test"),
        "non-retail cartridge area rejected");
    AssertThrows<ArgumentOutOfRangeException>(
        () => SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode((AreaId)SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count),
        "forged non-retail area rejected before table access");

    var crateriaRoom = new RoomIdentity(AreaId.Crateria, 0x1c);
    var brinstarRoom = new RoomIdentity(AreaId.Brinstar, 0x1c);
    AssertEqual(RoomIdentities.CrateriaSpacePirateShaft, crateriaRoom,
        "named room identity combines area and per-area index");
    AssertTrue(crateriaRoom != brinstarRoom,
        "equal room bytes in different areas remain distinct");
    AssertEqual("Crateria room 28", crateriaRoom.ToString(),
        "room identity diagnostic format");
    AssertThrows<ArgumentOutOfRangeException>(
        () => new RoomIdentity((AreaId)SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count, 0),
        "room identity rejects forged area enum");
    // A room's index is any index within its area (#1627).
    AssertEqual(300, new RoomIdentity(AreaId.Crateria, 300).RoomIndex, "room identity keeps an index past 255");
    AssertThrows<ArgumentOutOfRangeException>(() => new RoomIdentity(AreaId.Crateria, -1), "room identity rejects a negative index");
    AssertTrue(RoomIdentities.UsesTourianStyleLandingDust(
            RoomIdentities.BrinstarDirectLandingDust),
        "named Brinstar dust room matches shared landing policy");
    AssertTrue(!RoomIdentities.UsesTourianStyleLandingDust(
            new RoomIdentity(AreaId.Crateria, 0x08)),
        "same room byte in unrelated area does not match landing policy");

    Console.WriteLine("  Native words: enums, flags, angles, packed fields, and unknown-bit preservation agree.");
}

/// <summary>Runs the semantic native-word checks on their own.</summary>
[VerificationCommand("--typed-native-words")]
internal static int TypedNativeWordsCommand()
{
    VerifyTypedNativeWords();
    return 0;
}

}
