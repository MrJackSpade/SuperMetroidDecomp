using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Frontend.Map;
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
    // Equipment helpers must mutate only the requested independent bits. The unassigned item
    // slot is set by no ability and deliberately survives both operations unchanged.
    SamusEquipmentFlags equipment = SamusEquipmentFlags.UnassignedItem;
    equipment |= SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.Bombs;
    AssertTrue(equipment.HasAll(SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.Bombs),
        "typed equipment flags set independently");
    equipment &= ~SamusEquipmentFlags.GravitySuit;
    AssertEqual(0x1010, equipment.ToEquipmentWord(), "equipment flags keep the unassigned item slot");

    // The codec is the one place that knows each item's word bit: the word round-trips, and a word
    // setting a bit that records no item is rejected rather than dropped.
    AssertEqual(equipment, SamusEquipmentWordCodec.FromEquipmentWord(0x1010), "equipment word decodes to its items");
    AssertEqual((ushort)0xf32f, SamusEquipmentWordCodec.FromEquipmentWord(0xf32f).ToEquipmentWord(),
        "every recorded item bit round-trips through the codec");
    AssertThrows<InvalidDataException>(() => SamusEquipmentWordCodec.FromEquipmentWord(0x0080),
        "an equipment word bit that records no item is rejected");

    // #1456: a checksum-valid save slot may set bits that record nothing, which the cartridge never reads. The save
    // boundary keeps exactly the items and beams the words record, and re-encoding writes the other bits as zero.
    AssertEqual(SamusEquipmentWordCodec.FromEquipmentWord(0xf33f), SamusEquipmentWordCodec.FromSavedEquipmentWord(0xffff),
        "a saved equipment word decodes to the items it records");
    AssertEqual((ushort)0xf33f, SamusEquipmentWordCodec.FromSavedEquipmentWord(0xffff).ToEquipmentWord(),
        "a saved equipment word's unassigned bits re-encode as zero");
    AssertEqual(SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma | SamusBeamFlags.Charge,
        SamusEquipmentWordCodec.FromSavedBeamWord(0xffff), "a saved beam word decodes to the beams it records");
    AssertEqual((ushort)0x100f, SamusEquipmentWordCodec.FromSavedBeamWord(0xffff).ToEquipmentWord(),
        "a saved beam word's unassigned bits re-encode as zero");

    // The snapshot boundary decodes $0A76 and $0DC0 into typed state and rejects undefined words.
    AssertEqual(true, SamusSnapshotWords.DecodeHyperBeam(SamusSnapshotWords.HyperBeamEnabledWord), "hyper beam $8000 imports as enabled");
    AssertEqual(false, SamusSnapshotWords.DecodeHyperBeam(0), "hyper beam zero imports as disabled");
    AssertThrows<InvalidDataException>(() => SamusSnapshotWords.DecodeHyperBeam(1), "undefined hyper beam word is rejected");
    AssertEqual(SamusSnapshotWords.HyperBeamEnabledWord, SamusSnapshotWords.EncodeHyperBeam(true), "enabled hyper beam encodes as $8000");
    AssertEqual(ResumeChargingBeamSoundLatch.Clear, SamusSnapshotWords.DecodeResumeChargingBeamSound(0), "resume latch zero is clear");
    AssertEqual(ResumeChargingBeamSoundLatch.ResumeNextPass, SamusSnapshotWords.DecodeResumeChargingBeamSound(1), "resume latch one resumes next pass");
    AssertEqual(ResumeChargingBeamSoundLatch.SignedLatch, SamusSnapshotWords.DecodeResumeChargingBeamSound(ushort.MaxValue), "resume latch $FFFF is the signed latch");
    AssertThrows<InvalidDataException>(() => SamusSnapshotWords.DecodeResumeChargingBeamSound(2), "undefined resume latch word is rejected");

    // Samus counts are signed words: the cartridge's Crystal Flash drain takes an ammunition count below zero.
    AssertEqual(SamusQuantity.MaximumValue, SamusQuantity.Decode(0x7fff), "the largest count word decodes");
    AssertEqual(-1, SamusQuantity.Decode(0xffff), "a drained-past-zero word decodes as -1");
    AssertEqual(SamusQuantity.MinimumValue, SamusQuantity.Decode(0x8000), "the smallest count word decodes");
    AssertEqual((ushort)0x7fff, SamusQuantity.Encode(SamusQuantity.MaximumValue), "the largest count encodes");
    AssertEqual((ushort)0xffff, SamusQuantity.Encode(-1), "-1 encodes as the word the cartridge stores");
    AssertThrows<InvalidOperationException>(() => SamusQuantity.Encode(SamusQuantity.MinimumValue - 1), "an undersized count cannot be stored");
    AssertThrows<InvalidOperationException>(() => SamusQuantity.Encode(SamusQuantity.MaximumValue + 1), "an oversized count cannot be stored");
    AssertEqual(-1, SamusQuantity.DecrementWord(0), "the unchecked decrement takes zero to -1");
    AssertEqual(SamusQuantity.MaximumValue, SamusQuantity.DecrementWord(SamusQuantity.MinimumValue), "the unchecked decrement wraps the smallest count");

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
    AssertEqual(SamusFacingDirection.ForwardOrSpecial, SamusPoseMetadata.ReadFacingDirection((SamusPoseId)0xfe),
        "typed facing query faces an unnamed adjacent-code record neither way");
    AssertEqual(41, SamusPoseMetadata.ReadPoseXDirection((SamusPoseId)0xfe),
        "pose-record facing byte preserves the compiled unnamed adjacent-code byte");

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
    AssertEqual(0x0400, ScreenGeometry.TilemapScreenCellCount,
        "PPU 32-by-32 tilemap page size");
    AssertEqual((ushort)0x5000, GameplayVramLayout.Bg1TilemapWord,
        "engine gameplay BG1 tilemap placement");

    ushort[] translatedPlmInstructionCodes =
    [
        (ushort)RoomPlmInstruction.Sleep,
        (ushort)RoomPlmInstruction.Delete,
        (ushort)RoomPlmInstruction.InstallPreInstruction,
        (ushort)RoomPlmInstruction.Goto,
        (ushort)RoomPlmInstruction.DecrementTimerAndGoto,
        (ushort)RoomPlmInstruction.SetEightBitTimer,
        (ushort)RoomPlmInstruction.CopyFromRamToVram,
        (ushort)RoomPlmInstruction.GotoIfAreaBossBitSet,
        (ushort)RoomPlmInstruction.GotoIfEventSet,
        (ushort)RoomPlmInstruction.SetEvent,
        (ushort)RoomPlmInstruction.DrawPlmBlock,
        (ushort)RoomPlmInstruction.DrawPlmBlockClone,
        (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum6,
        (ushort)RoomPlmInstruction.QueueSoundLibrary3Maximum6,
        (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum3,
        (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum1,
        (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct,

        (ushort)RoomPlmInstruction.SetPlmBtsToOne,
        (ushort)RoomPlmInstruction.GotoIfRoomArgumentLess,
        (ushort)RoomPlmInstruction.SpawnFourMotherBrainGlassShards,
        (ushort)RoomPlmInstruction.SpawnTorizoStatueBreaking,
        (ushort)RoomPlmInstruction.QueueSongOneMusicTrack,
    ];
    AssertEqual(translatedPlmInstructionCodes.Length,
        translatedPlmInstructionCodes.Distinct().Count(),
        "translated PLM instruction catalog has unique native pointers");

    // A cartridge turn is a 16-bit wrapping domain whose high byte indexes the shared
    // 256-entry sine tables. Verify named axes, full-turn normalization, fractional
    // preservation, and the signed modular subtraction used near the wrap seam.
    AssertEqual((ushort)0x4000, RotationAngle.QuarterTurn.RawValue,
        "SNES quarter-turn raw word");
    AssertEqual((byte)0x40, RotationAngle.QuarterTurn.TableIndex,
        "SNES quarter-turn sine index");
    AssertEqual(0x80, RotationAngle.QuarterTurn.SineTableByteOffset,
        "SNES quarter-turn word-table offset");
    AssertEqual(RotationAngle.Zero, RotationAngle.NormalizeRaw(0x1_0000),
        "SNES raw angle wraps at one turn");
    AssertEqual(RotationAngle.Zero, RotationAngle.NormalizeTableIndex(0x100),
        "SNES table angle wraps at 256 units");
    AssertEqual((ushort)0x41ab, RotationAngle.FromRaw(0x40ab).AddTableUnits(1).RawValue,
        "SNES whole-unit addition retains fractional angle byte");
    AssertEqual((short)-0x4000, RotationAngle.Zero.SignedDeltaTo(RotationAngle.ThreeQuarterTurn),
        "SNES signed raw delta crosses wrap seam");
    AssertEqual((sbyte)-0x40,
        RotationAngle.Zero.SignedTableDeltaTo(RotationAngle.ThreeQuarterTurn),
        "SNES signed table delta crosses wrap seam");

    var positiveEightEight = new SignedEightEight(0x0180);
    AssertEqual((sbyte)1, positiveEightEight.WholePixels,
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
    AssertThrows<OverflowException>(
        () => SignedSixteenSixteen.FromWholePixels(32768),
        "signed 16.16 checked whole conversion rejects positive overflow");

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
    AssertEqual((ushort)0x8001, WrappingCountdown.FromRaw(0x8001).RawValue, "a raw countdown word is preserved exactly");
    AssertThrows<ArgumentOutOfRangeException>(() => WrappingCountdown.Start(ushort.MaxValue + 1),
        "a countdown start must fit the native word");
    AssertThrows<ArgumentOutOfRangeException>(() => WrappingCountdown.Start(-1), "a countdown start cannot be negative");

    LowByteCountdownStep laneToZero = WrappingCounter.DecrementLowByte(0x1201);
    AssertEqual((ushort)0x1200, laneToZero.Remaining, "8-bit low-byte DEC keeps the high byte");
    AssertTrue(laneToZero.IsZero && !laneToZero.IsNegative, "8-bit low-byte DEC reports byte-width zero");
    LowByteCountdownStep laneWrapped = WrappingCounter.DecrementLowByte(0x3400);
    AssertEqual((ushort)0x34ff, laneWrapped.Remaining, "8-bit low-byte DEC wraps without borrowing from the high byte");
    AssertTrue(laneWrapped.IsNegative && !laneWrapped.IsZero && laneWrapped.IsZeroOrNegative,
        "8-bit low-byte DEC reports byte-width negative");
    LowByteCountdownStep laneHighBitOnly = WrappingCounter.DecrementLowByte(0x8003);
    AssertTrue(laneHighBitOnly.IsNonNegative && laneHighBitOnly.Low == 2,
        "8-bit low-byte DEC ignores the high byte's sign bit");

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
    AssertTrue(MapTileWords.PauseBlank.IsBlank && MapTileWords.HudBlank.IsBlank,
        "pause and HUD blank sentinels share the empty character");
    AssertEqual((ushort)0xa812, new MapTileWord(0xac12).AsExplored().Raw,
        "explored map cell clears only cartridge palette bit $0400");
    AssertEqual((ushort)0xebff, new MapTileWord(0xffff).ForHud(explored: true).Raw,
        "HUD explored cell preserves character and flips while replacing attributes");
    AssertEqual((ushort)0xec12, new MapTileWord(0xe012).ForHud(explored: false).Raw,
        "HUD unexplored cell preserves character and flips while replacing attributes");
    AssertEqual((ushort)0x3c12, new MapTileWord(0x2812).WithLocationBlink().Raw,
        "HUD location blink applies native palette bits");

    // Every independently composable enemy property is an explicit flag.
    EnemyProperties enemyProperties = EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision |
        EnemyProperties.BlocksPlasmaBeam | EnemyProperties.SolidToSamus;
    AssertTrue(enemyProperties.HasAll(EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision |
            EnemyProperties.BlocksPlasmaBeam | EnemyProperties.SolidToSamus),
        "enemy visibility, collision, plasma, and platform flags round trip");
    enemyProperties = enemyProperties.With(EnemyProperties.Deleted | EnemyProperties.ProcessOffScreen |
        EnemyProperties.ProcessInstructions);
    AssertTrue(enemyProperties.HasAll(EnemyProperties.Deleted | EnemyProperties.ProcessOffScreen |
            EnemyProperties.ProcessInstructions),
        "enemy deletion and processing flags round trip");
    enemyProperties = enemyProperties.Without(EnemyProperties.Invisible | EnemyProperties.Deleted |
        EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions);
    AssertEqual(EnemyProperties.IgnoreSamusCollision | EnemyProperties.BlocksPlasmaBeam | EnemyProperties.SolidToSamus,
        enemyProperties, "enemy flag operations clear only the named flags");
    AssertEqual(enemyProperties, SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.Decode(
        SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.Encode(enemyProperties)), "enemy properties round trip their native word");

    // The import's level-word codec: $E selects the grapple collision kind, $C supplies both
    // mirrorings, and $02A is the block. The typed block holds each field by name.
    RoomTerrainBlock grapple = RoomTerrainBlock.Of(42, BlockMirroring.Horizontal | BlockMirroring.Vertical,
        RoomCollisionType.GrappleBlock);
    AssertEqual(grapple, RoomLevelWordCodec.Decode(0xec2a), "level word decodes kind, block and mirroring");
    AssertEqual(0xec2a, (int)RoomLevelWordEncoder.Encode(grapple), "typed block encodes the level word");
    AssertEqual(grapple.Appearance, RoomLevelWordCodec.DecodeAppearance(0xec2a), "appearance ignores the collision kind");
    AssertEqual(0x0c2a, (int)RoomLevelWordEncoder.EncodeAppearance(grapple.Appearance), "appearance encodes without collision bits");
    AssertTrue(RoomLevelWordCodec.IsAppearanceWord(0x0c2a) && !RoomLevelWordCodec.IsAppearanceWord(0xec2a),
        "only collision-free words are appearance words");
    AssertEqual(RoomTerrainBlock.Of(42, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.HorizontalExtension),
        grapple.WithCollision(RoomCollisionType.HorizontalExtension), "collision replacement keeps the appearance");
    AssertEqual(RoomTerrainBlock.Of(42, BlockMirroring.Vertical, RoomCollisionType.GrappleBlock),
        grapple.WithAppearance(grapple.Appearance.WithMirroring(BlockMirroring.Vertical)),
        "mirroring replacement keeps the kind and block");
    AssertEqual(RoomTerrainBlock.Of(85, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.GrappleBlock),
        grapple.WithBlock(85), "block replacement keeps the kind and mirroring");
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
        RoomCollisionType.UnusedAir,
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

    // BTS remains a raw byte until the collision nibble selects its meaning. Exercise
    // each contextual view independently so no future cleanup conflates overlapping bits.
    var reflectedSlopeOrAreaReaction = new RoomBlockBehavior(0xff);
    AssertEqual(-1, reflectedSlopeOrAreaReaction.ExtensionOffset,
        "BTS signed extension offset");
    AssertEqual(0x1f, reflectedSlopeOrAreaReaction.SlopeShape,
        "BTS slope shape field");
    AssertTrue(reflectedSlopeOrAreaReaction.IsNonSquareSlope &&
        reflectedSlopeOrAreaReaction.SlopeFlipsHorizontally &&
        reflectedSlopeOrAreaReaction.SlopeFlipsVertically,
        "BTS slope contextual fields");
    AssertEqual(3, reflectedSlopeOrAreaReaction.SlopeOrientation,
        "BTS slope orientation field");
    AssertTrue(reflectedSlopeOrAreaReaction.UsesAreaReactionTable,
        "BTS area-reaction selector");
    AssertEqual(0x7f, reflectedSlopeOrAreaReaction.AreaReactionIndex,
        "BTS area-reaction index");
    AssertTrue(!reflectedSlopeOrAreaReaction.IsAreaReactionIndex(8) &&
        !reflectedSlopeOrAreaReaction.IsNormalReactionIndex(16),
        "BTS reaction bounds preserve table context");
    AssertTrue(new RoomBlockBehavior(3).IsPersistentGrappleReaction &&
        new RoomBlockBehavior(3).IsRespawningReaction,
        "BTS overlapping grapple and breakable-block views stay contextual");
    AssertTrue(new RoomBlockBehavior(6).IsPermanentReaction,
        "BTS permanent breakable-block view");
    AssertTrue(new RoomBlockBehavior(8).RequiresPowerBombReaction &&
        new RoomBlockBehavior(10).RequiresSuperMissileReaction,
        "BTS weapon-gated shot-block views");
    AssertTrue(new RoomBlockBehavior(0x82).IsAreaReactionIndex(8),
        "BTS area-reaction table view");

    RoomBlockBehavior[] blueDoorBehaviors =
    [
        RoomBlockBehaviorValues.BlueDoorFacingLeft,
        RoomBlockBehaviorValues.BlueDoorFacingRight,
        RoomBlockBehaviorValues.BlueDoorFacingUp,
        RoomBlockBehaviorValues.BlueDoorFacingDown,
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
    AssertTrue(!RoomBlockBehaviorValues.CollectibleTrigger.TryGetBlueDoorOrientation(out _),
        "non-door BTS rejects blue-door view");

    for (byte stationValue = (byte)StationAccessBehavior.MapRight;
         stationValue <= (byte)StationAccessBehavior.SaveFloor;
         stationValue++)
    {
        var stationBehavior = new RoomBlockBehavior(stationValue);
        AssertTrue(stationBehavior.TryGetStationAccess(out StationAccessBehavior access),
            $"station BTS ${stationValue:X2} decodes");
        AssertEqual((StationAccessBehavior)stationValue, access,
            $"station BTS ${stationValue:X2} remains lossless");
    }
    AssertTrue(!RoomBlockBehaviorValues.ScrollTrigger.TryGetStationAccess(out _),
        "non-station BTS rejects station view");

    var typedPose = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence());
    typedPose.PoseId = SamusPoseId.ScrewAttackLeftPose;
    AssertEqual(SamusPoseId.ScrewAttackLeftPose, typedPose.Pose,
        "typed Samus pose writes the native byte");
    typedPose.Pose = (SamusPoseId)0xfe;
    AssertEqual((SamusPoseId)0xfe, typedPose.PoseId,
        "undefined Samus pose byte remains observable for diagnostics");

    var bgEntry = new SnesBgTilemapWord(0xf555);
    AssertEqual(0x155, bgEntry.CharacterIndex, "BG tile character field");
    AssertEqual(5, bgEntry.PaletteIndex, "BG tile palette field");
    AssertTrue(bgEntry.HasPriority, "BG tile priority bit");
    AssertEqual(TileFlip.Horizontal | TileFlip.Vertical,
        bgEntry.FlipFlags, "BG tile flip flags");
    AssertEqual(0x7555, bgEntry.ToggleFlips(TileFlip.Vertical).Raw,
        "BG tile toggles only vertical flip");
    SnesBgTilemapWord encodedBg = SnesBgTilemapWord.Create(
        characterIndex: 0x155,
        paletteIndex: 5,
        priority: true,
        TileFlip.Horizontal | TileFlip.Vertical);
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
        TileFlip.Horizontal);
    AssertEqual(objEntry, encodedObj, "OBJ attribute encode/decode round trip");
    AssertEqual(0x63ab, encodedObj.WithPaletteIndex(1).Raw,
        "OBJ palette replacement preserves tile, priority, and flips");
    AssertEqual(0x6e00, SnesObjAttributeWord.Create(0, 7, 0).Or(
            SnesObjAttributeWord.Create(0, 0, 2, TileFlip.Horizontal)).Raw,
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

    AssertTrue(LayerBlendingConfiguration.VisorBackdrop2A.AnimatesVisor(),
        "layer blending enum selects visor backdrop handler");
    AssertTrue(!((LayerBlendingConfiguration)0x1234).AnimatesVisor(),
        "unnamed layer blending value stays outside typed handler");

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
        AssertEqual(retailAreas[rawArea], AreaIds.FromIndex(rawArea, "typed-area test"),
            $"retail area byte {rawArea} validates");
        AssertEqual(rawArea, AreaIds.ToIndex(retailAreas[rawArea]),
            $"retail area {retailAreas[rawArea]} indexes cartridge tables");
    }
    AssertThrows<InvalidDataException>(
        () => AreaIds.FromIndex(AreaIds.RetailCount, "typed-area test"),
        "non-retail cartridge area rejected");
    AssertThrows<ArgumentOutOfRangeException>(
        () => AreaIds.ToIndex((AreaId)AreaIds.RetailCount),
        "forged non-retail area rejected before table access");

    var crateriaRoom = new RoomIdentity(AreaId.Crateria, 0x1c);
    var brinstarRoom = new RoomIdentity(AreaId.Brinstar, 0x1c);
    AssertEqual(RoomIdentities.CrateriaSpacePirateShaft, crateriaRoom,
        "named room identity combines area and per-area index");
    AssertTrue(crateriaRoom != brinstarRoom,
        "equal room bytes in different areas remain distinct");
    AssertEqual("$00/$1C", crateriaRoom.ToString(),
        "room identity diagnostic format");
    AssertThrows<ArgumentOutOfRangeException>(
        () => new RoomIdentity((AreaId)AreaIds.RetailCount, 0),
        "room identity rejects forged area enum");
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
