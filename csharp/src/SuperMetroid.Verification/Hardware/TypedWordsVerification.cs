using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hardware;
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
using SuperMetroid.Core.Map;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Progression;

/// <summary>Exercises semantic native-word views while checking that unnamed cartridge bits survive typed operations.</summary>
internal static class TypedWordsVerification
{

/// <summary>
/// Proves that the semantic enums and packed-word views decode verified fields without
/// normalizing, discarding, or assigning meaning to any other native bits.
/// </summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
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

    // Suit palette tables use byte offsets rather than enum ordinals. Verify all three
    // branches plus the native Gravity-over-Varia priority that motivated centralization.
    AssertEqual(0, SamusEquipmentFlags.None.GetSuitPaletteTableOffset(),
        "Power Suit palette table offset");
    AssertEqual(2, SamusEquipmentFlags.VariaSuit.GetSuitPaletteTableOffset(),
        "Varia Suit palette table offset");
    AssertEqual(4, SamusEquipmentFlags.GravitySuit.GetSuitPaletteTableOffset(),
        "Gravity Suit palette table offset");
    AssertEqual(4,
        (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit)
            .GetSuitPaletteTableOffset(),
        "Gravity Suit palette table priority");

    // Facing is an ordinary discriminator, not a flag set. The typed view must recognize
    // verified values four/eight while retaining an unknown byte for debugger inspection.
    var poseBus = new TestAddressSpace();
    AssertTrue(SamusPoseMetadata.IsFacingLeft(SamusPoseId.FacingLeftNormalPose), "typed left-facing pose query");
    AssertEqual(SamusFacingDirection.Right, SamusPoseMetadata.ReadFacingDirection(SamusPoseId.FacingRightNormalPose),
        "typed right-facing pose query");
    AssertEqual(41, (byte)SamusPoseMetadata.ReadFacingDirection((SamusPoseId)0xfe),
        "typed facing query preserves compiled unnamed adjacent-code byte");

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

    AssertEqual(256, SnesPpuLayout.ScreenWidthPixels, "PPU low-resolution screen width");
    AssertEqual(224, SnesPpuLayout.ScreenHeightPixels, "PPU visible screen height");
    AssertEqual(192, SnesPpuLayout.GameplayViewportHeightPixels,
        "gameplay viewport excludes the 32-line HUD");
    AssertEqual(0x10000, SnesPpuLayout.VramByteCount, "PPU VRAM byte capacity");
    AssertEqual(512, SnesPpuLayout.CgramByteCount, "PPU CGRAM byte capacity");
    AssertEqual(544, SnesPpuLayoutConstants.OamUploadByteCount, "PPU OAM upload size");
    AssertEqual(0x0400, SnesPpuLayout.TilemapPageWordCount,
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
    AssertEqual((ushort)0x4000, SnesAngle.QuarterTurn.RawValue,
        "SNES quarter-turn raw word");
    AssertEqual((byte)0x40, SnesAngle.QuarterTurn.TableIndex,
        "SNES quarter-turn sine index");
    AssertEqual(0x80, SnesAngle.QuarterTurn.SineTableByteOffset,
        "SNES quarter-turn word-table offset");
    AssertEqual(SnesAngle.Zero, SnesAngle.NormalizeRaw(0x1_0000),
        "SNES raw angle wraps at one turn");
    AssertEqual(SnesAngle.Zero, SnesAngle.NormalizeTableIndex(0x100),
        "SNES table angle wraps at 256 units");
    AssertEqual((ushort)0x41ab, SnesAngle.FromRaw(0x40ab).AddTableUnits(1).RawValue,
        "SNES whole-unit addition retains fractional angle byte");
    AssertEqual((short)-0x4000, SnesAngle.Zero.SignedDeltaTo(SnesAngle.ThreeQuarterTurn),
        "SNES signed raw delta crosses wrap seam");
    AssertEqual((sbyte)-0x40,
        SnesAngle.Zero.SignedTableDeltaTo(SnesAngle.ThreeQuarterTurn),
        "SNES signed table delta crosses wrap seam");

    var positiveEightEight = new SnesSignedEightEight(0x0180);
    AssertEqual((sbyte)1, positiveEightEight.WholePixels,
        "signed 8.8 positive whole byte");
    AssertEqual((byte)0x80, positiveEightEight.Fraction,
        "signed 8.8 positive fraction byte");
    AssertEqual(0x0001_8000,
        positiveEightEight.ToSixteenSixteenDelta().RawValue,
        "signed 8.8 promotes to signed 16.16");
    var negativeEightEight = new SnesSignedEightEight(0xff80);
    AssertEqual(unchecked((int)0xffff_8000),
        negativeEightEight.ToSixteenSixteenDelta().RawValue,
        "negative signed 8.8 promotion sign-extends");
    SnesFixedPosition halfPixelBack = negativeEightEight
        .ToSixteenSixteenDelta()
        .AddTo(10, 0x4000);
    AssertEqual(new SnesFixedPosition(9, 0xc000), halfPixelBack,
        "signed fixed delta carries and borrows across split position words");
    AssertEqual(new SnesFixedPosition(0, 0),
        SnesSignedSixteenSixteen.FromWholePixels(1).AddTo(ushort.MaxValue, 0),
        "signed fixed position addition wraps native whole word");
    AssertEqual(unchecked((ushort)0xff00),
        SnesSignedEightEight.FromWholePixels(127).AddRawWrapping(0x8000).RawValue,
        "signed 8.8 arithmetic wraps the cartridge word");
    AssertThrows<OverflowException>(
        () => SnesSignedEightEight.FromWholePixels(128),
        "signed 8.8 checked whole conversion rejects positive overflow");
    AssertThrows<OverflowException>(
        () => SnesSignedSixteenSixteen.FromWholePixels(32768),
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
        AssertEqual(raw, (ushort)cell, $"{name} map cell raw round trip");
    }
    AssertTrue(MapTileWords.PauseBlank.IsBlank && MapTileWords.HudBlank.IsBlank,
        "pause and HUD blank sentinels share the empty character");
    AssertEqual((ushort)0xa812, (ushort)new MapTileWord(0xac12).AsExplored(),
        "explored map cell clears only cartridge palette bit $0400");
    AssertEqual((ushort)0xebff, (ushort)new MapTileWord(0xffff).ForHud(explored: true),
        "HUD explored cell preserves character and flips while replacing attributes");
    AssertEqual((ushort)0xec12, (ushort)new MapTileWord(0xe012).ForHud(explored: false),
        "HUD unexplored cell preserves character and flips while replacing attributes");
    AssertEqual((ushort)0x3c12, (ushort)new MapTileWord(0x2812).WithLocationBlink(),
        "HUD location blink applies native palette bits");

    // The low byte belongs to individual enemy families; the typed word keeps it untouched while making every
    // independently composable high-byte property explicit.
    var enemyProperties = new EnemyPropertyWord(EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision |
        EnemyProperties.BlocksPlasmaBeam | EnemyProperties.SolidToSamus, familyData: 90);
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
    AssertEqual(0x945a, enemyProperties.ToNativeWord(), "enemy flag operations preserve the family-specific low byte");
    AssertEqual(enemyProperties, EnemyPropertyWord.FromNativeWord(0x945a), "enemy property word round trips its native word");
    AssertEqual(EnemyProperties.IgnoreSamusCollision | EnemyProperties.BlocksPlasmaBeam | EnemyProperties.SolidToSamus,
        enemyProperties.Flags, "enemy property word exposes only its typed high-byte flags");
    AssertThrows<ArgumentOutOfRangeException>(
        () => enemyProperties.With((EnemyProperties)0x0001),
        "enemy property word rejects family data disguised as a shared flag");

    EnemyExtraPropertyWord enemyExtraProperties = new EnemyExtraPropertyWord(EnemyExtraProperties.UsesExtendedSpritemap)
        .WithUntranslatedBits(EnemyExtraPropertyRawBits.CrocomireInitializerBit0400, "typed-word verification");
    AssertEqual(0x0404, enemyExtraProperties.ToNativeWord(),
        "enemy extra-property escape hatch preserves explicitly owned untranslated data");
    AssertEqual(enemyExtraProperties, EnemyExtraPropertyWord.FromNativeWord(0x0404),
        "enemy extra-property word splits its native word into flags and untranslated bits");
    AssertThrows<ArgumentException>(
        () => default(EnemyExtraPropertyWord).WithUntranslatedBits(
            (ushort)EnemyExtraProperties.UsesExtendedSpritemap, "typed-word verification"),
        "enemy extra-property escape hatch rejects known flags");

    // $E selects the verified grapple collision handler, $C supplies both parent flips,
    // and $02A is the visual block. All three packed fields share this exact raw word.
    var levelWord = new RoomLevelWord(0xec2a);
    AssertEqual(RoomCollisionType.GrappleBlock, levelWord.CollisionType,
        "level collision enum");
    AssertEqual(0x002a, levelWord.VisualBlockIndex, "level visual index field");
    AssertEqual(LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical,
        levelWord.VisualFlipFlags, "level parent flip flags");
    AssertEqual(0xec2a, (ushort)levelWord, "level word raw round trip");

    RoomLevelWord encodedLevelWord = RoomLevelWord.Create(
        visualBlockIndex: 0x002a,
        LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical,
        RoomCollisionType.GrappleBlock);
    AssertEqual(0xec2a, encodedLevelWord.Raw, "level word typed encode");
    AssertEqual(0x5c2a,
        encodedLevelWord.WithCollisionType(RoomCollisionType.HorizontalExtension).Raw,
        "level collision replacement preserves visual flips");
    AssertEqual(0xe82a,
        encodedLevelWord.WithVisualFlipFlags(LevelBlockFlipFlags.Vertical).Raw,
        "level flip replacement preserves collision and index");
    AssertEqual(0xec55, encodedLevelWord.WithVisualBlockIndex(0x0055).Raw,
        "level block replacement preserves collision and flips");
    AssertThrows<ArgumentOutOfRangeException>(
        () => encodedLevelWord.WithVisualFlipFlags((LevelBlockFlipFlags)0x1000),
        "level flip replacement rejects collision bits");

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
        var typedLevelWord = new RoomLevelWord((ushort)(collisionNibble << 12));
        AssertEqual(retailCollisionTypes[collisionNibble], typedLevelWord.CollisionType,
            $"level collision nibble ${collisionNibble:X1}");
        AssertEqual(collisionNibble, typedLevelWord.CollisionTypeValue,
            $"level collision raw nibble ${collisionNibble:X1}");
        AssertTrue(typedLevelWord.HasSolidProbeBit == (collisionNibble >= 8),
            $"attachment probe tests the raw high bit for collision nibble ${collisionNibble:X1}");
    }

    // BTS remains a raw byte until the collision nibble selects its meaning. Exercise
    // each contextual view independently so no future cleanup conflates overlapping bits.
    var reflectedSlopeOrAreaReaction = new RoomBlockBehavior(0xff);
    AssertEqual((sbyte)-1, reflectedSlopeOrAreaReaction.ExtensionOffset,
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

    var typedPose = new SamusState();
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
    AssertEqual(SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical,
        bgEntry.FlipFlags, "BG tile flip flags");
    AssertEqual(0x7555, bgEntry.ToggleFlips(SnesTileFlipFlags.Vertical),
        "BG tile toggles only vertical flip");
    SnesBgTilemapWord encodedBg = SnesBgTilemapWord.Create(
        characterIndex: 0x155,
        paletteIndex: 5,
        priority: true,
        SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical);
    AssertEqual(bgEntry, encodedBg, "BG tile encode/decode round trip");
    AssertEqual(0xd955, encodedBg.WithPaletteIndex(6).WithPriority(false),
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
        SnesTileFlipFlags.Horizontal);
    AssertEqual(objEntry, encodedObj, "OBJ attribute encode/decode round trip");
    AssertEqual(0x63ab, encodedObj.WithPaletteIndex(1),
        "OBJ palette replacement preserves tile, priority, and flips");
    AssertEqual(0x6e00, SnesObjPalettes.Palette(7).Or(
            SnesObjAttributeWord.Create(0, 0, 2, SnesTileFlipFlags.Horizontal)),
        "typed OBJ palette fragment composes with priority and flip fields");
    SnesOamHighTablePair highPair = SnesOamHighTablePair.FromSprite(
        screenX: 0x0100,
        isLarge: true);
    AssertTrue(highPair.XHigh && highPair.IsLarge && highPair.Raw == 3,
        "OAM high-table pair encodes X-high and size independently");
    AssertThrows<ArgumentOutOfRangeException>(
        () => SnesObjAttributeWord.FromPaletteBits(0x1000),
        "OBJ palette fragment rejects priority bits");

    // The $1000 high bit is deliberately outside every named projectile field here. Family
    // replacement changes only bits 8-11 and must therefore retain that unknown state.
    var projectileType = new SamusProjectileTypeWord(0x9219);
    AssertEqual(SamusProjectileFamily.SuperMissile, projectileType.Family,
        "projectile family enum");
    AssertEqual(SamusBeamCombination.PlasmaWave, projectileType.BeamCombination, "projectile low-nibble payload");
    AssertTrue(projectileType.IsChargedBeam && projectileType.IsLive,
        "projectile verified control fields");
    AssertEqual(0x9819,
        projectileType.WithFamily(SamusProjectileFamily.MissileExplosion),
        "projectile family replacement preserves all other bits");

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
        SamusProjectileTypeWord dispatchWord = new(raw);
        AssertEqual(family, dispatchWord.Family,
            $"projectile family dispatch for raw word ${raw:X4}");
        AssertTrue(dispatchWord.IsFamily(family),
            $"projectile family predicate for raw word ${raw:X4}");
    }

    var direction = new SamusProjectileDirectionWord(0xab07);
    AssertEqual(SamusProjectileDirection.Left, direction.Direction,
        "projectile direction enum");
    AssertTrue(direction.IsValidInitialDirection,
        "projectile direction ignores deliberately unnamed high byte");
    AssertTrue(!new SamusProjectileDirectionWord(0x0017).IsValidInitialDirection,
        "projectile low-byte lifecycle state rejects initial movement");

    var beams = new SamusBeamLoadoutWord((SamusBeamFlags)0x9009);
    AssertTrue(beams.HasAny(SamusBeamFlags.Charge | SamusBeamFlags.Plasma),
        "beam loadout exposes verified flags");
    AssertEqual(0x9002, beams.WithCombinationIndex(2),
        "beam combination replacement preserves upper raw bits");

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
