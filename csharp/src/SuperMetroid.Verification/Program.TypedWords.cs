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

    Console.WriteLine("  Native words: enums, flags, packed fields, and unknown-bit preservation agree.");
}

}
