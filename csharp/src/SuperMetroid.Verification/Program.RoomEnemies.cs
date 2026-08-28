using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

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

    AssertEqual(1, enemies.EnemyCount, "enemy population count");
    AssertEqual(RoomEnemySystem.NativeSlotSize, enemies.FirstFreeEnemyIndex,
        "first free enemy keeps native byte offset");
    AssertEqual(1, enemies.DeathQuota, "population terminator death quota");
    AssertEqual(0, enemies.EnemiesKilled, "room load clears killed-enemy count");
    AssertEqual(0x1234, enemies.BossId, "nonzero definition publishes room boss ID");
    AssertEqual(2, enemies.GraphicsSet.Count, "terminated enemy graphics-set count");

    RoomEnemyGraphicsSetEntry ordinaryGraphics = enemies.GraphicsSet[0];
    RoomEnemyGraphicsSetEntry specialGraphics = enemies.GraphicsSet[1];
    AssertEqual(0x0800, ordinaryGraphics.StagingOffset, "ordinary enemy staging offset");
    AssertEqual(0x0200, specialGraphics.StagingOffset, "high-bit enemy encoded staging offset");
    AssertEqual(2, specialGraphics.VramTilesIndex,
        "second enemy tile index includes first definition size");
    AssertEqual(0x80, vram.ReadByte(0xe000), "ordinary enemy first tile byte");
    AssertEqual(0xbf, vram.ReadByte(0xe03f), "ordinary enemy final tile byte");
    AssertEqual(0xc0, vram.ReadByte(0xda00), "special enemy first tile byte");
    AssertEqual(0x2120, cgram.Colors[(3 + 8) * 16],
        "ordinary enemy palette destination and source");
    AssertEqual(0x6160, cgram.Colors[(4 + 8) * 16],
        "special enemy palette destination and source");

    RoomEnemySlot slot = enemies.Slots[0];
    AssertEqual(0x0456, slot.XPosition, "population X position");
    AssertEqual(0x0789, slot.YPosition, "population Y position");
    AssertEqual(0x0600, slot.PaletteIndex, "graphics-set OBJ palette index");
    AssertEqual(0, slot.VramTilesIndex, "first graphics-set tile index");
    AssertEqual(0xa2, slot.AiBank, "definition AI bank copied into native slot");
    AssertEqual(0x5a, slot.HurtAiTime, "definition hurt-AI time copied into native slot");
    AssertEqual(0, slot.AiHandlerBits, "room load clears AI handler bits");
    AssertEqual(0, slot.FlashTimer, "room load clears flash timer");
    AssertEqual(0, slot.InvincibilityTimer, "room load clears invincibility timer");
    AssertEqual(0, slot.ShakeTimer, "room load clears shake timer");
    AssertEqual(0x804f, slot.SpritemapPointer,
        "extended actor receives native extended-nothing map after initialization");
    AssertEqual(0x3100, slot.Spawn.NameWords.Word0, "spawn name first word");
    AssertEqual(0x3104, slot.Spawn.NameWords.Word4, "spawn name fifth copied word");
    AssertEqual(0x3106, slot.Spawn.NameWords.Word6, "spawn name skips source word five");
    AssertEqual(0x0600, slot.Spawn.PaletteIndex,
        "spawn snapshot retains pre-initialization palette index");

    RoomEnemyDefinition definition = slot.Definition;
    AssertEqual(0x110e, definition.HurtSoundEffect, "definition hurt SFX offset $0E");
    AssertEqual(0x1114, definition.PartCount, "definition part count offset $14");
    AssertEqual(0x111a, definition.GrappleAiPointer, "definition grapple AI offset $1A");
    AssertEqual(0x1128, definition.PowerBombReactionPointer,
        "definition power-bomb reaction offset $28");
    AssertEqual(0x1134, definition.InitialSpritemapPointer,
        "definition initial spritemap offset $34");
    AssertEqual(0xa29100, definition.TileDataAddress, "definition 24-bit tile-data pointer");
    AssertEqual(0x113a, definition.ItemDropChancesPointer,
        "definition item-drop pointer offset $3A");
    AssertEqual(0x113c, definition.VulnerabilityPointer,
        "definition vulnerability pointer offset $3C");
    AssertEqual(0x9200, definition.NamePointer, "definition name pointer offset $3E");

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

    AssertEqual(0, enemies.EnemyCount, "empty population clears enemy count");
    AssertEqual(0, enemies.GraphicsSet.Count, "empty population skips graphics set");
    AssertEqual(RoomEnemySystem.NativeSlotSize, enemies.FirstFreeEnemyIndex,
        "empty population preserves native first-free quirk");
    AssertEqual(1, enemies.DeathQuota, "empty population preserves native death-quota quirk");
    AssertEqual(0, enemies.BossId, "room load clears boss ID before empty population");
    AssertEqual(0x4567, cgram.Colors[128], "empty population leaves CGRAM untouched");
    AssertEqual(0x5a, vram.ReadByte(0xe000), "empty population leaves enemy VRAM untouched");

    Console.WriteLine(
        "  Enemies: complete headers, populations, spawn snapshots, palettes, tile staging, " +
        "boss state, placeholders, and empty-room behavior agree.");
}

/// <summary>
/// Regresses the exact enemy $E13F failure reported when the second normal Ceres door loads
/// room $E0B5. This fixture uses native $A0/$A1/$A6/$B4 layouts and follows the translated
/// dispatcher through the complete initial delay, eye fade, and body fade.
/// </summary>
static void VerifyCeresRidleyRoomEntry()
{
    const ushort definitionPointer = 0xe13f;
    const ushort populationPointer = 0x9500;
    const ushort tilesetPointer = 0x9500;

    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var cgram = new SnesCgram();

    WriteEnemyDefinition(
        bus,
        definitionPointer,
        tileDataSize: 0x0020,
        palettePointer: 0xe14f,
        bank: 0xa6,
        tileDataAddress: 0xa69000,
        bossId: 1,
        namePointer: 0,
        fieldSeed: 0x4000);
    WriteWord(bus, 0xa00000 | (definitionPointer + 18), 0xa0f5);
    WriteWord(bus, 0xa00000 | (definitionPointer + 24), 0xa288);

    // One graphics-set entry supplies Ridley's OBJ palette/tile association. The byte data
    // need not depict retail art here; the regression concerns loader and AI addresses, and
    // the real-ROM audit covers the actual encoded graphics immediately afterward.
    WriteWord(bus, 0xb40000 | tilesetPointer, definitionPointer);
    WriteWord(bus, (0xb40000 | tilesetPointer) + 2, 0x0001);
    WriteWord(bus, (0xb40000 | tilesetPointer) + 4, 0xffff);
    for (int color = 0; color < 16; color++)
        WriteWord(bus, 0xa6e14f + color * 2, unchecked((ushort)(0x1000 + color)));

    // The Ceres population record is deliberately the retail one: ($BA,$AB), init zero,
    // properties $2800, no extra bits, and two zero speed/parameter words.
    int population = 0xa10000 | populationPointer;
    WriteWord(bus, population, definitionPointer);
    WriteWord(bus, population + 2, 0x00ba);
    WriteWord(bus, population + 4, 0x00ab);
    WriteWord(bus, population + 6, 0);
    WriteWord(bus, population + 8, 0x2800);
    WriteWord(bus, population + 10, 0);
    WriteWord(bus, population + 12, 0);
    WriteWord(bus, population + 14, 0);
    WriteWord(bus, population + 16, 0xffff);
    bus.WriteByte(population + 18, 0);

    // $E538 selects the left-facing initial frame and then sleeps. Supplying the literal
    // list catches the custom $E517 branch rather than letting a generic timed frame mask it.
    WriteWord(bus, 0xa6e538, 0xe517);
    WriteWord(bus, 0xa6e53a, 0xe542);
    WriteWord(bus, 0xa6e53c, 12);
    WriteWord(bus, 0xa6e53e, 0x9000);
    WriteWord(bus, 0xa6e540, 0x812f);

    // Initial additional palettes are copied to OBJ palettes two/three. The later eye and
    // body tables use distinct sentinels so destination/index mistakes remain observable.
    for (int color = 0; color < 32; color++)
        WriteWord(bus, 0xa6e16f + color * 2, unchecked((ushort)(0x2000 + color)));
    for (int index = 0; index < 64; index++)
        bus.WriteByte(0xa6e269 + index, index < 16 ? unchecked((byte)(15 - index)) : (byte)0);
    bus.WriteByte(0xa6e2a9, 0xff);
    for (int word = 0; word < (0xe30a - 0xe2aa) / 2; word++)
        WriteWord(bus, 0xa6e2aa + word * 2, unchecked((ushort)(0x3000 + word)));
    for (int word = 0; word < 0x160 / 2; word++)
        WriteWord(bus, 0xa6e30a + word * 2, unchecked((ushort)(0x5000 + word)));

    // The two later animation lists are the actual dispatcher seams that make reveal-only
    // implementations fail. A compact timed map stands in for the long roar presentation;
    // the liftoff list retains its real $E969 instruction so AI control changes exactly as
    // it does in the cartridge. Movement divisors are the literal $10..$01 D712 table.
    WriteWord(bus, 0xa6e690, 1);
    WriteWord(bus, 0xa6e692, 0x9000);
    WriteWord(bus, 0xa6e694, 0x812f);
    WriteWord(bus, 0xa6e91d, 0xe969);
    WriteWord(bus, 0xa6e91f, 1);
    WriteWord(bus, 0xa6e921, 0x9000);
    WriteWord(bus, 0xa6e923, 0x812f);
    for (int divisor = 0; divisor < 16; divisor++)
        bus.WriteByte(0xa6d712 + divisor, unchecked((byte)(16 - divisor)));

    // End-of-battle palette records are deliberately unmistakable. AA11 must not publish
    // status one without A9A0 first copying all three native destination ranges.
    for (int color = 0; color < 15; color++)
        WriteWord(bus, 0xa6a9e3 + color * 2, unchecked((ushort)(0x6100 + color)));
    for (int color = 0; color < 8; color++)
        WriteWord(bus, 0xa6aa01 + color * 2, unchecked((ushort)(0x6200 + color)));

    // One minimal power-beam data/list pair lets this regression reach Ridley's shot AI
    // through the public producer and explosion owner, rather than mutating HitCounter.
    WritePoseDefinition(
        bus,
        SamusState.FacingRightNormalPose,
        [0x08, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00]);
    WriteWord(bus, 0x9383c1, 0x8431);
    WriteWord(bus, 0x938431, 20);
    WriteWord(bus, 0x938437, 0x9000); // Direction two is data-pointer word three.
    WriteWord(bus, 0x939000, 1);
    WriteWord(bus, 0x939002, 0x9000);
    bus.WriteByte(0x939004, 8);
    bus.WriteByte(0x939005, 8);
    WriteWord(bus, 0x939006, 0);
    WriteWord(bus, 0x939008, 0x8239);
    WriteWord(bus, 0x93900a, 0x9000);
    WriteWord(bus, 0x93867b, 0x9100);
    WriteWord(bus, 0x939100, 1);
    WriteWord(bus, 0x939102, 0x9000);
    bus.WriteByte(0x939104, 8);
    bus.WriteByte(0x939105, 8);
    WriteWord(bus, 0x939106, 0);
    WriteWord(bus, 0x939108, 0x822f);
    bus.WriteByte(0x90c254, 1);
    WriteWord(bus, 0x90c28f, 0x000b);

    var enemies = new RoomEnemySystem();
    enemies.Load(bus, populationPointer, tilesetPointer, vram, cgram, () => 0x1234);

    RoomEnemySlot ridley = enemies.Slots[0];
    CeresRidleyState state = enemies.CeresRidley
        ?? throw new InvalidOperationException("Ceres Ridley did not allocate its extended state.");
    AssertEqual(0x00ba, ridley.XPosition, "Ceres Ridley init X");
    AssertEqual(0x00a9, ridley.YPosition, "Ceres Ridley init Y overrides population Y");
    AssertEqual(0xe538, ridley.CurrentInstruction, "Ceres Ridley initial instruction list");
    AssertEqual(0x0e00, ridley.PaletteIndex, "Ceres Ridley dedicated OBJ palette");
    AssertEqual(0x3c00, ridley.Properties, "Ceres Ridley native initialized properties");
    AssertEqual(0x804f, ridley.SpritemapPointer, "Ceres Ridley post-init empty extended map");
    AssertEqual((ushort)CeresRidleyAiFunction.WaitForDoorTransition, (ushort)state.Function,
        "Ceres Ridley initial dispatcher function");
    AssertEqual(0x2000, cgram.Colors[0xa0], "Ceres Ridley first added palette color");
    AssertEqual(0x201f, cgram.Colors[0xbf], "Ceres Ridley final added palette color");
    AssertEqual(0, cgram.Colors[0xf1], "Ceres Ridley hidden body palette starts black");
    AssertEqual(0, cgram.Colors[0xff], "Ceres Ridley hidden body palette ends black");

    enemies.StepFrame(cameraX: 0, cameraY: 0, timeIsFrozen: false);
    AssertEqual((ushort)CeresRidleyAiFunction.InitialDelay, (ushort)state.Function,
        "Ceres Ridley door-clear transition");
    AssertEqual(511, state.FunctionTimer, "Ceres Ridley first delay decrement");
    AssertEqual(0x9000, ridley.SpritemapPointer, "Ceres Ridley left-facing initial map");
    AssertEqual(0xe540, ridley.CurrentInstruction, "Ceres Ridley initial list sleeps");

    for (int frame = 0; frame < 512; frame++)
        enemies.StepFrame(cameraX: 0, cameraY: 0, timeIsFrozen: false);
    AssertEqual((ushort)CeresRidleyAiFunction.FadeInEyes, (ushort)state.Function,
        "Ceres Ridley 512-counter underflow starts eye fade");

    for (int frame = 0; frame < 65; frame++)
        enemies.StepFrame(cameraX: 0, cameraY: 0, timeIsFrozen: false);
    AssertEqual((ushort)CeresRidleyAiFunction.FadeInBody, (ushort)state.Function,
        "Ceres Ridley eye table terminator starts body fade");
    AssertEqual(1, state.MovementAnimationEnabled,
        "Ceres Ridley eye fade enables composite animation");

    for (int frame = 0; frame < 32; frame++)
        enemies.StepFrame(cameraX: 0, cameraY: 0, timeIsFrozen: false);
    AssertEqual((ushort)CeresRidleyAiFunction.WaitBeforeRoar, (ushort)state.Function,
        "Ceres Ridley body fade completes");
    AssertEqual(4, state.FunctionTimer, "Ceres Ridley pre-roar timer");
    AssertEqual(0x5000 + 0x14a / 2, cgram.Colors[0x91],
        "Ceres Ridley final body-fade row reaches OBJ palette one");
    AssertEqual(0, ridley.Properties & (ushort)EnemyProperties.IgnoreSamusCollision,
        "Ceres Ridley becomes tangible after body fade");

    // Finish the 5-frame pre-roar and 253-frame pre-liftoff countdowns, then let the real
    // E969 instruction arm A6AF. Movement uses the translated common 8.8 integrator until
    // A6C8 crosses Y=$50 and publishes fight mode one.
    var samus = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        Health = 99,
    };
    int battleEntryFrames = 0;
    while (state.Function != CeresRidleyAiFunction.Hovering && battleEntryFrames < 1024)
    {
        enemies.StepFrame(0, 0, timeIsFrozen: false, samus);
        battleEntryFrames++;
    }
    AssertEqual((ushort)CeresRidleyAiFunction.Hovering, (ushort)state.Function,
        "Ceres Ridley liftoff reaches hover dispatcher");
    AssertEqual(1, state.FightMode, "Ceres Ridley liftoff enables battle mode");
    AssertTrue(ridley.YPosition < 80,
        "Ceres Ridley crosses the cartridge's Y=$50 battle threshold");

    const int roomWidth = 32;
    const int roomHeight = 16;
    RoomLevelData air = new(
        roomWidth,
        roomHeight,
        new ushort[roomWidth * roomHeight],
        new byte[roomWidth * roomHeight],
        new ushort[roomWidth * roomHeight],
        new byte[8]);
    var sharedProjectiles = new SamusBombProjectileSystem();
    var projectiles = new SamusProjectileSystem();

    for (int hit = 0; hit < 100; hit++)
    {
        // Zero fixture muzzle offsets place each stationary power beam at Ridley's live
        // origin. Running the public producer proves slot allocation/type/radii first.
        samus.XPosition = ridley.XPosition;
        samus.YPosition = ridley.YPosition;
        sharedProjectiles.StepFrame(bus, air, samus, 0, 0);
        SamusProjectileFrameResult fired = projectiles.StepFrame(
            bus,
            air,
            samus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            sharedProjectiles);
        AssertEqual((int?)0, fired.FiredSlot,
            $"Ceres Ridley hit {hit + 1} allocates the power-beam slot");
        AssertEqual(1, enemies.ResolveCeresRidleyProjectileHits(
            bus, projectiles, sharedProjectiles),
            $"Ceres Ridley hit {hit + 1} reaches enemy shot AI");

        // Two bank-$93 calls consume the one-frame explosion record and its delete opcode,
        // returning the same slot to the next shot without debugger-only state mutation.
        for (int explosionFrame = 0; explosionFrame < 2; explosionFrame++)
        {
            sharedProjectiles.StepFrame(bus, air, samus, 0, 0);
            projectiles.StepFrame(bus, air, samus, 0, 0, 0, 0, sharedProjectiles);
        }

        enemies.StepFrame(0, 0, timeIsFrozen: false, samus);
    }

    AssertEqual(100, state.HitCounter, "Ceres Ridley uses dedicated 100-shot counter");

    int retreatFrames = 0;
    bool observedFakeRetreat = false;
    while (enemies.CeresStatus != 1 && retreatFrames < 1024)
    {
        enemies.StepFrame(0, 0, timeIsFrozen: false, samus);
        observedFakeRetreat |= state.Function is
            CeresRidleyAiFunction.FakeRetreatMoveToPosition or
            CeresRidleyAiFunction.FakeRetreatRising or
            CeresRidleyAiFunction.WaitBeforeRetrievingBaby or
            CeresRidleyAiFunction.RetrieveBaby;
        retreatFrames++;
    }
    AssertTrue(observedFakeRetreat,
        "Ceres Ridley returns from current attack through fake retreat/Baby retrieval");
    AssertEqual(0, state.FightMode, "Ceres Ridley retreat disables battle mode");
    AssertEqual(1, enemies.CeresStatus, "Ceres Ridley publishes escape handoff status");
    AssertEqual((ushort)CeresRidleyAiFunction.Inactive, (ushort)state.Function,
        "Ceres Ridley installs null dispatcher after battle");
    AssertTrue(ridley.Properties.HasAny(EnemyProperties.Invisible),
        "Ceres Ridley ordinary actor yields to getaway presentation");
    AssertEqual(0x6100, cgram.Colors[0x51],
        "Ceres Ridley retreat copies BG palette-five colors");
    AssertEqual(0x6200, cgram.Colors[0x21],
        "Ceres Ridley retreat copies BG palette-two colors");
    AssertEqual(0x6200, cgram.Colors[0xf1],
        "Ceres Ridley retreat copies OBJ palette-seven colors");

    Console.WriteLine(
        "  Ceres Ridley: reveal, liftoff, real beam impacts, 100-hit battle exit, " +
        "retreat palettes, and escape handoff agree.");
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
