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
    AssertEqual(RoomCollisionType.Grapple, levelWord.CollisionType, "level collision enum");
    AssertEqual(0x002a, levelWord.VisualBlockIndex, "level visual index field");
    AssertEqual(LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical,
        levelWord.VisualFlipFlags, "level parent flip flags");
    AssertEqual(0xec2a, (ushort)levelWord, "level word raw round trip");

    // Untranslated collision value $A remains observable as enum numeric value $A rather
    // than being coerced to a known handler or rejected by the wrapper.
    var unnamedCollision = new RoomLevelWord(0xa123);
    AssertEqual(0x0a, (byte)unnamedCollision.CollisionType,
        "unnamed collision nibble remains lossless");

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
