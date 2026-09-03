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

/// <summary>
/// Proves that the semantic enums and packed-word views decode verified fields without
/// normalizing, discarding, or assigning meaning to any other native bits.
/// </summary>
static void VerifyTypedNativeWords()
{
    // Equipment helpers must mutate only the requested independent bits. Bit $0080 is not
    // named by the enum and deliberately survives both operations unchanged.
    ushort equipment = 0x0080;
    equipment = equipment.With(SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.Bombs);
    AssertTrue(equipment.HasAll(SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.Bombs),
        "typed equipment flags set independently");
    equipment = equipment.Without(SamusEquipmentFlags.GravitySuit);
    AssertEqual(0x1080, equipment, "equipment helper preserves unnamed bits");

    // Suit palette tables use byte offsets rather than enum ordinals. Verify all three
    // branches plus the native Gravity-over-Varia priority that motivated centralization.
    AssertEqual(0, ((ushort)0).GetSuitPaletteTableOffset(),
        "Power Suit palette table offset");
    AssertEqual(2, SamusEquipmentFlags.VariaSuit.ToNativeWord().GetSuitPaletteTableOffset(),
        "Varia Suit palette table offset");
    AssertEqual(4, SamusEquipmentFlags.GravitySuit.ToNativeWord().GetSuitPaletteTableOffset(),
        "Gravity Suit palette table offset");
    AssertEqual(4,
        (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit)
            .ToNativeWord()
            .GetSuitPaletteTableOffset(),
        "Gravity Suit palette table priority");

    // Facing is an ordinary discriminator, not a flag set. The typed view must recognize
    // verified values four/eight while retaining an unknown byte for debugger inspection.
    var poseBus = new TestAddressSpace();
    WritePoseDefinition(poseBus, 1, [4, 0, 0, 0, 0, 0, 0, 0]);
    WritePoseDefinition(poseBus, 2, [8, 0, 0, 0, 0, 0, 0, 0]);
    WritePoseDefinition(poseBus, 3, [0x7f, 0, 0, 0, 0, 0, 0, 0]);
    AssertTrue(SamusState.IsFacingLeft(poseBus, 1), "typed left-facing pose query");
    AssertEqual(SamusFacingDirection.Right, SamusState.ReadFacingDirection(poseBus, 2),
        "typed right-facing pose query");
    AssertEqual(0x7f, (byte)SamusState.ReadFacingDirection(poseBus, 3),
        "typed facing query preserves unnamed direction byte");

    // Fixed-bank pointer arithmetic must wrap only the low word. A host addition would
    // incorrectly cross from bank $91 into $92 and read unrelated cartridge data.
    AssertEqual(0x910000, SnesAddressMath.AddWithinBank(0x91ffff, 1),
        "fixed-bank address addition wraps low word");
    AssertEqual(0x91fffe, SnesAddressMath.AddWithinBank(0x910002, -4),
        "fixed-bank address addition wraps negative displacement");

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

    NativeWordCounterStep fromOne = NativeWordCounter.Decrement(0x0001);
    AssertEqual((ushort)0x0000, fromOne.Value, "native DEC one reaches zero");
    AssertTrue(fromOne.IsZero && fromOne.IsNonNegative && !fromOne.IsNegative,
        "native DEC one exposes Z without N");
    NativeWordCounterStep fromZero = NativeWordCounter.Decrement(0x0000);
    AssertEqual(ushort.MaxValue, fromZero.Value, "native DEC zero wraps to $FFFF");
    AssertTrue(fromZero.IsNegative && fromZero.Underflowed && !fromZero.IsZero,
        "native DEC zero exposes N and underflow without Z");
    NativeWordCounterStep fromMaximum = NativeWordCounter.Decrement(ushort.MaxValue);
    AssertEqual((ushort)0xfffe, fromMaximum.Value, "native DEC $FFFF reaches $FFFE");
    AssertTrue(fromMaximum.IsNegative && !fromMaximum.Underflowed,
        "native DEC $FFFF retains N without reporting zero underflow");
    NativeWordCounterStep ordinaryCounter = NativeWordCounter.Decrement(0x1234);
    AssertEqual((ushort)0x1233, ordinaryCounter.Value, "native DEC ordinary positive value");
    AssertTrue(ordinaryCounter.IsNonNegative && !ordinaryCounter.IsZeroOrNegative,
        "native DEC ordinary positive result exposes neither expiry flag");
    AssertEqual((ushort)0, NativeWordCounter.DecrementSaturating(0),
        "saturating native counter pins zero");
    AssertEqual((ushort)0, NativeWordCounter.DecrementSaturating(1),
        "saturating native counter decrements one");
    AssertEqual((ushort)0x1233, NativeWordCounter.DecrementSaturating(0x1234),
        "saturating native counter decrements ordinary positive value");

    // Map cells are all ordinary SNES tile words; elevator, item-dot, and station meanings do
    // not occupy a separate packed field. Give those constructed fixture categories distinct
    // character/attribute combinations to prove the typed boundary never normalizes them.
    // The explored/unexplored operations then prove the two renderer-specific mutations touch
    // only their documented attributes.
    (string Name, ushort Raw)[] mapCellRoundTrips =
    [
        ("blank", 0x001f),
        ("explored room", 0x2812),
        ("unexplored room", 0x2c12),
        ("elevator", 0x6c0e),
        ("item", 0xac05),
        ("station", 0x2c1a),
    ];
    foreach ((string name, ushort raw) in mapCellRoundTrips)
    {
        MapTileWord cell = raw;
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

    ushort enemyProperties = 0x1000;
    enemyProperties = enemyProperties.With(
        EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
    enemyProperties = enemyProperties.Without(EnemyProperties.Invisible);
    AssertEqual(0x1400, enemyProperties, "enemy flags preserve unnamed property bits");

    // $E selects the verified grapple collision handler, $C supplies both parent flips,
    // and $02A is the visual block. All three packed fields share this exact raw word.
    var levelWord = new RoomLevelWord(0xec2a);
    AssertEqual(RoomCollisionType.GrappleBlock, levelWord.CollisionType,
        "level collision enum");
    AssertEqual(0x002a, levelWord.VisualBlockIndex, "level visual index field");
    AssertEqual(LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical,
        levelWord.VisualFlipFlags, "level parent flip flags");
    AssertEqual(0xec2a, (ushort)levelWord, "level word raw round trip");

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
    }

    // BTS remains a raw byte until the collision nibble selects its meaning. Exercise
    // each contextual view independently so no future cleanup conflates overlapping bits.
    var reflectedSlopeOrAreaReaction = new RoomBlockBehavior(0xff);
    AssertEqual((sbyte)-1, reflectedSlopeOrAreaReaction.ExtensionOffset,
        "BTS signed extension offset");
    AssertEqual((byte)0x1f, reflectedSlopeOrAreaReaction.SlopeShape,
        "BTS slope shape field");
    AssertTrue(reflectedSlopeOrAreaReaction.IsNonSquareSlope &&
        reflectedSlopeOrAreaReaction.SlopeFlipsHorizontally &&
        reflectedSlopeOrAreaReaction.SlopeFlipsVertically,
        "BTS slope contextual fields");
    AssertEqual((byte)3, reflectedSlopeOrAreaReaction.SlopeOrientation,
        "BTS slope orientation field");
    AssertTrue(reflectedSlopeOrAreaReaction.UsesAreaReactionTable,
        "BTS area-reaction selector");
    AssertEqual((byte)0x7f, reflectedSlopeOrAreaReaction.AreaReactionIndex,
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
    AssertEqual(SamusPoseIds.ScrewAttackLeftPose, typedPose.Pose,
        "typed Samus pose writes the native byte");
    typedPose.Pose = 0xfe;
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

    var objEntry = new SnesObjAttributeWord(0x6dab);
    AssertEqual(0x1ab, objEntry.TileNumber, "OBJ tile-number field");
    AssertEqual(6, objEntry.PaletteIndex, "OBJ palette field");
    AssertEqual(2, objEntry.Priority, "OBJ priority field");
    AssertTrue(objEntry.FlipHorizontally && !objEntry.FlipVertically,
        "OBJ independent flip fields");

    // The $1000 high bit is deliberately outside every named projectile field here. Family
    // replacement changes only bits 8-11 and must therefore retain that unknown state.
    var projectileType = new SamusProjectileTypeWord(0x9219);
    AssertEqual(SamusProjectileFamily.SuperMissile, projectileType.Family,
        "projectile family enum");
    AssertEqual(9, projectileType.BeamCombinationIndex, "projectile low-nibble payload");
    AssertTrue(projectileType.IsChargedBeam && projectileType.IsLive,
        "projectile verified control fields");
    AssertEqual(0x9819,
        projectileType.WithFamily(SamusProjectileFamily.MissileExplosion),
        "projectile family replacement preserves all other bits");

    (ushort Raw, SamusProjectileFamily Family)[] projectileDispatchCases =
    [
        (0x801f, SamusProjectileFamily.Beam),
        (0x8110, SamusProjectileFamily.Missile),
        (0x8210, SamusProjectileFamily.SuperMissile),
        (0x8300, SamusProjectileFamily.PowerBomb),
        (0x8501, SamusProjectileFamily.Bomb),
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

    var beams = new SamusBeamLoadoutWord(0x9009);
    AssertTrue(beams.HasAny(SamusBeamFlags.Charge | SamusBeamFlags.Plasma),
        "beam loadout exposes verified flags");
    AssertEqual(0x9002, beams.WithCombinationIndex(2),
        "beam combination replacement preserves upper raw bits");

    AssertTrue(((ushort)LayerBlendingConfiguration.VisorBackdrop2A).AnimatesVisor(),
        "layer blending enum selects visor backdrop handler");
    AssertTrue(!((ushort)0x1234).AnimatesVisor(),
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
        AssertEqual(retailAreas[rawArea], AreaIds.FromCartridge(rawArea, "typed-area test"),
            $"retail area byte {rawArea} validates");
        AssertEqual((int)rawArea, AreaIds.ToIndex(retailAreas[rawArea]),
            $"retail area {retailAreas[rawArea]} indexes cartridge tables");
    }
    AssertThrows<InvalidDataException>(
        () => AreaIds.FromCartridge(AreaIds.RetailCount, "typed-area test"),
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

}
