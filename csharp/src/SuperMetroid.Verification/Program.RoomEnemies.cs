using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{

/// <summary>
/// Exercises the finite room-enemy loading pipeline separately from actor-specific AI.
/// The fixture mirrors the literal $A1 population, $B4 set, and 64-byte $A0 header formats,
/// so a convenient host-side reinterpretation cannot accidentally replace cartridge layout.
/// </summary>
static void VerifyRoomEnemyLoading()
{
    const ushort populationPointer = 0x9100;
    const ushort tilesetPointer = 0x9100;
    const ushort primaryDefinitionPointer = 0xd000;
    const ushort specialDefinitionPointer = 0xd040;

    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var cgram = new SnesCgram();

    // The first header is intentionally filled at every documented offset. Besides making
    // the parsed record useful to future damage/interaction translations, this catches a
    // one-word shift immediately rather than waiting for a distant enemy AI to misbehave.
    WriteEnemyDefinition(
        bus,
        primaryDefinitionPointer,
        tileDataSize: 0x0040,
        palettePointer: 0x9000,
        bank: 0xa2,
        tileDataAddress: 0xa29100,
        bossId: 0x1234,
        namePointer: 0x9200,
        fieldSeed: 0x1100);

    // Bit fifteen selects the definition's encoded staging address instead of the ordinary
    // running $0800 offset. It is still part of the tile-index arithmetic in retail code.
    WriteEnemyDefinition(
        bus,
        specialDefinitionPointer,
        tileDataSize: 0x8020,
        palettePointer: 0x9300,
        bank: 0xa3,
        tileDataAddress: 0xa39320,
        bossId: 0,
        namePointer: 0,
        fieldSeed: 0x2200);

    for (int colorByte = 0; colorByte < 32; colorByte++)
    {
        bus.WriteByte(0xa29000 + colorByte, unchecked((byte)(0x20 + colorByte)));
        bus.WriteByte(0xa39300 + colorByte, unchecked((byte)(0x60 + colorByte)));
    }
    for (int tileByte = 0; tileByte < 0x40; tileByte++)
        bus.WriteByte(0xa29100 + tileByte, unchecked((byte)(0x80 + tileByte)));
    for (int tileByte = 0; tileByte < 0x20; tileByte++)
        bus.WriteByte(0xa39320 + tileByte, unchecked((byte)(0xc0 + tileByte)));

    // RecordEnemySpawnData copies source words 0-4 and 6. Word 5 is a real retail omission,
    // so give it a distinct sentinel and prove the managed snapshot does not normalize it.
    for (int word = 0; word < 7; word++)
        WriteWord(bus, 0xb49200 + word * 2, unchecked((ushort)(0x3100 + word)));

    WriteWord(bus, 0xb40000 | tilesetPointer, primaryDefinitionPointer);
    WriteWord(bus, (0xb40000 | tilesetPointer) + 2, 0x0003);
    WriteWord(bus, (0xb40000 | tilesetPointer) + 4, specialDefinitionPointer);
    WriteWord(bus, (0xb40000 | tilesetPointer) + 6, 0x1004);
    WriteWord(bus, (0xb40000 | tilesetPointer) + 8, 0xffff);

    int populationAddress = 0xa10000 | populationPointer;
    WriteWord(bus, populationAddress, primaryDefinitionPointer);
    WriteWord(bus, populationAddress + 2, 0x0456);
    WriteWord(bus, populationAddress + 4, 0x0789);
    WriteWord(bus, populationAddress + 6, 0xabcd);
    WriteWord(bus, populationAddress + 8, 0x2000);
    WriteWord(bus, populationAddress + 10, 0x0004);
    WriteWord(bus, populationAddress + 12, 0x1357);
    WriteWord(bus, populationAddress + 14, 0x2468);
    WriteWord(bus, populationAddress + 16, 0xffff);
    bus.WriteByte(populationAddress + 18, 1);

    var enemies = new RoomEnemySystem();
    enemies.Load(bus, populationPointer, tilesetPointer, vram, cgram, () => 0x9999);

    AssertEqual((ushort)1, enemies.EnemyCount, "enemy population count");
    AssertEqual((ushort)RoomEnemySystem.NativeSlotSize, enemies.FirstFreeEnemyIndex,
        "first free enemy keeps native byte offset");
    AssertEqual((byte)1, enemies.DeathQuota, "population terminator death quota");
    AssertEqual((ushort)0, enemies.EnemiesKilled, "room load clears killed-enemy count");
    AssertEqual((ushort)0x1234, enemies.BossId, "nonzero definition publishes room boss ID");
    AssertEqual(2, enemies.GraphicsSet.Count, "terminated enemy graphics-set count");

    RoomEnemyGraphicsSetEntry ordinaryGraphics = enemies.GraphicsSet[0];
    RoomEnemyGraphicsSetEntry specialGraphics = enemies.GraphicsSet[1];
    AssertEqual(0x0800, ordinaryGraphics.StagingOffset, "ordinary enemy staging offset");
    AssertEqual(0x0200, specialGraphics.StagingOffset, "high-bit enemy encoded staging offset");
    AssertEqual((ushort)2, specialGraphics.VramTilesIndex,
        "second enemy tile index includes first definition size");
    AssertEqual((byte)0x80, vram.ReadByte(0xe000), "ordinary enemy first tile byte");
    AssertEqual((byte)0xbf, vram.ReadByte(0xe03f), "ordinary enemy final tile byte");
    AssertEqual((byte)0xc0, vram.ReadByte(0xda00), "special enemy first tile byte");
    AssertEqual((ushort)0x2120, cgram.Colors[(3 + 8) * 16],
        "ordinary enemy palette destination and source");
    AssertEqual((ushort)0x6160, cgram.Colors[(4 + 8) * 16],
        "special enemy palette destination and source");

    RoomEnemySlot slot = enemies.Slots[0];
    AssertEqual((ushort)0x0456, slot.XPosition, "population X position");
    AssertEqual((ushort)0x0789, slot.YPosition, "population Y position");
    AssertEqual((ushort)0x0600, slot.PaletteIndex, "graphics-set OBJ palette index");
    AssertEqual((ushort)0, slot.VramTilesIndex, "first graphics-set tile index");
    AssertEqual((byte)0xa2, slot.AiBank, "definition AI bank copied into native slot");
    AssertEqual((byte)0x5a, slot.HurtAiTime, "definition hurt-AI time copied into native slot");
    AssertEqual((ushort)0, slot.AiHandlerBits, "room load clears AI handler bits");
    AssertEqual((ushort)0, slot.FlashTimer, "room load clears flash timer");
    AssertEqual((ushort)0, slot.InvincibilityTimer, "room load clears invincibility timer");
    AssertEqual((ushort)0, slot.ShakeTimer, "room load clears shake timer");
    AssertEqual((ushort)0x804f, slot.SpritemapPointer,
        "extended actor receives native extended-nothing map after initialization");
    AssertEqual((ushort)0x3100, slot.Spawn.NameWords.Word0, "spawn name first word");
    AssertEqual((ushort)0x3104, slot.Spawn.NameWords.Word4, "spawn name fifth copied word");
    AssertEqual((ushort)0x3106, slot.Spawn.NameWords.Word6, "spawn name skips source word five");
    AssertEqual((ushort)0x0600, slot.Spawn.PaletteIndex,
        "spawn snapshot retains pre-initialization palette index");

    RoomEnemyDefinition definition = slot.Definition;
    AssertEqual((ushort)0x110e, definition.HurtSoundEffect, "definition hurt SFX offset $0E");
    AssertEqual((ushort)0x1114, definition.PartCount, "definition part count offset $14");
    AssertEqual((ushort)0x111a, definition.GrappleAiPointer, "definition grapple AI offset $1A");
    AssertEqual((ushort)0x1128, definition.PowerBombReactionPointer,
        "definition power-bomb reaction offset $28");
    AssertEqual((ushort)0x1134, definition.InitialSpritemapPointer,
        "definition initial spritemap offset $34");
    AssertEqual(0xa29100, definition.TileDataAddress, "definition 24-bit tile-data pointer");
    AssertEqual((ushort)0x113a, definition.ItemDropChancesPointer,
        "definition item-drop pointer offset $3A");
    AssertEqual((ushort)0x113c, definition.VulnerabilityPointer,
        "definition vulnerability pointer offset $3C");
    AssertEqual((ushort)0x9200, definition.NamePointer, "definition name pointer offset $3E");

    // Reload the same object with an empty population and a deliberately invalid tileset.
    // Native $A0:8A6D skips graphics processing, and InitializeEnemies' early return leaves
    // first-free/death-quota words untouched even though current enemy counts are cleared.
    const ushort emptyPopulationPointer = 0x9400;
    int emptyPopulationAddress = 0xa10000 | emptyPopulationPointer;
    WriteWord(bus, emptyPopulationAddress, 0xffff);
    bus.WriteByte(emptyPopulationAddress + 2, 0x7f);
    cgram.SetColor(128, 0x4567);
    vram.LoadBytes(0xe000, new byte[] { 0x5a });
    enemies.Load(bus, emptyPopulationPointer, 0xffff, vram, cgram, () => 0);

    AssertEqual((ushort)0, enemies.EnemyCount, "empty population clears enemy count");
    AssertEqual(0, enemies.GraphicsSet.Count, "empty population skips graphics set");
    AssertEqual((ushort)RoomEnemySystem.NativeSlotSize, enemies.FirstFreeEnemyIndex,
        "empty population preserves native first-free quirk");
    AssertEqual((byte)1, enemies.DeathQuota, "empty population preserves native death-quota quirk");
    AssertEqual((ushort)0, enemies.BossId, "room load clears boss ID before empty population");
    AssertEqual((ushort)0x4567, cgram.Colors[128], "empty population leaves CGRAM untouched");
    AssertEqual((byte)0x5a, vram.ReadByte(0xe000), "empty population leaves enemy VRAM untouched");

    Console.WriteLine(
        "  Enemies: complete headers, populations, spawn snapshots, palettes, tile staging, " +
        "boss state, placeholders, and empty-room behavior agree.");
}

/// <summary>Writes one complete fixture header while keeping pointer-bearing fields valid.</summary>
static void WriteEnemyDefinition(
    TestAddressSpace bus,
    ushort definitionPointer,
    ushort tileDataSize,
    ushort palettePointer,
    byte bank,
    int tileDataAddress,
    ushort bossId,
    ushort namePointer,
    ushort fieldSeed)
{
    int address = 0xa00000 | definitionPointer;
    for (int offset = 0; offset < 64; offset += 2)
        WriteWord(bus, address + offset, unchecked((ushort)(fieldSeed + offset)));

    WriteWord(bus, address, tileDataSize);
    WriteWord(bus, address + 2, palettePointer);
    bus.WriteByte(address + 12, bank);
    bus.WriteByte(address + 13, 0x5a);
    WriteWord(bus, address + 16, bossId);
    WriteWord(bus, address + 18, 0x804c); // Bank-local no-op initialization AI.
    WriteWord(bus, address + 24, 0x804c); // Bank-local no-op main AI.
    WriteLong(bus, address + 54, tileDataAddress);
    bus.WriteByte(address + 57, 5);
    WriteWord(bus, address + 62, namePointer);
}

static void WriteWord(TestAddressSpace bus, int address, ushort value)
{
    bus.WriteByte(address, unchecked((byte)value));
    bus.WriteByte(address + 1, unchecked((byte)(value >> 8)));
}

static void WriteLong(TestAddressSpace bus, int address, int value)
{
    bus.WriteByte(address, unchecked((byte)value));
    bus.WriteByte(address + 1, unchecked((byte)(value >> 8)));
    bus.WriteByte(address + 2, unchecked((byte)(value >> 16)));
}

}
