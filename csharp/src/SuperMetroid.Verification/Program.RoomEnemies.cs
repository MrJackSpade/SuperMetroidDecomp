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
/// Reproduces the rapid-Super visual failure from room $A56B: a fresh owner and its
/// invisible linked slot occupy the same 32-pixel cell as a blocking eye-door projectile.
/// Bank $A0 marks both direction words and lets their own next pre-instruction dispose of
/// them; it never creates either of the visible bank-$93 missile-explosion programs here.
/// </summary>
static void VerifyEnemyProjectileCollisionLifecycle()
{
    const ushort populationPointer = 0x9400;
    var bus = new TestAddressSpace();
    WriteWord(bus, 0xa10000 | populationPointer, 0xffff);
    bus.WriteByte((0xa10000 | populationPointer) + 2, 0);

    var enemies = new RoomEnemySystem();
    enemies.Load(
        bus,
        populationPointer,
        0xffff,
        new SnesVram(),
        new SnesCgram(),
        () => 0);

    RoomEnemyProjectileSlot eyeShot = enemies.EnemyProjectiles[17];
    eyeShot.Kind = RoomEnemyProjectileKind.EyeDoorProjectile;
    eyeShot.XPosition = 0x01b5;
    eyeShot.YPosition = 0x0180;
    eyeShot.BlocksSamusProjectiles = true;
    eyeShot.CollisionOption = 0;
    WriteWord(
        bus,
        0x860000 | unchecked((ushort)((ushort)eyeShot.Kind + 12)),
        0xb5fb);

    var shots = new SamusProjectileSystem();
    SamusProjectileSlot owner = shots.Slots[0];
    owner.Type = 0x8200;
    owner.Damage = 0x012c;
    owner.Direction = (ushort)SamusProjectileDirection.Right;
    owner.XPosition = 0x01b7;
    owner.YPosition = 0x0196;
    owner.InstructionPointer = 0x9f3b;
    owner.InstructionTimer = 15;
    owner.SpritemapPointer = 0xae0f;
    owner.PreInstruction = SamusProjectilePreInstruction.SuperMissile;

    SamusProjectileSlot link = shots.Slots[1];
    link.Type = 0x8200;
    link.Damage = 0x012c;
    link.Direction = (ushort)SamusProjectileDirection.Right;
    link.XPosition = 0x01af;
    link.YPosition = 0x0196;
    link.InstructionPointer = 0x9f7b;
    link.InstructionTimer = 1;
    link.SpritemapPointer = 0;
    link.PreInstruction = SamusProjectilePreInstruction.SuperMissileLink;

    int hits = enemies.ResolveEnemyProjectileSamusProjectileHits(
        bus,
        shots,
        new SamusBombProjectileSystem());

    AssertEqual(2, hits,
        "blocking enemy projectile visits Super Missile owner and linked slot");
    AssertEqual(0x8200, owner.Type,
        "enemy-projectile collision preserves fresh Super Missile family");
    AssertEqual(0x9f3b, owner.InstructionPointer,
        "enemy-projectile collision preserves fresh Super Missile animation program");
    AssertTrue(owner.PackedDirection.HasLowByteLifecycleState,
        "enemy-projectile collision marks Super Missile owner lifecycle");
    AssertEqual(0x8200, link.Type,
        "enemy-projectile collision does not turn invisible link into an explosion");
    AssertEqual(0, link.SpritemapPointer,
        "enemy-projectile collision leaves Super Missile link invisible");
    AssertTrue(link.PackedDirection.HasLowByteLifecycleState,
        "enemy-projectile collision marks Super Missile link lifecycle");
    AssertEqual(0, shots.EarthquakeTimer,
        "enemy-projectile collision does not invent ordinary Super impact quake");
    AssertTrue(!eyeShot.BlocksSamusProjectiles,
        "destructible enemy projectile disables subsequent-frame shot collision");

    Console.WriteLine(
        "  Enemy projectile collision: rapid Super owner/link retain native lifecycle state without false explosion art.");
}

/// <summary>
/// Exercises the first ordinary hostile-enemy translation end to end. Every actor pointer,
/// speed word, animation list, and vulnerability byte is expressed in its native ROM layout;
/// the assertions then enter only through the public loader/frame/contact/projectile seams.
/// </summary>
static void VerifyRipperEnemy()
{
    const ushort definitionPointer = 0xd47f;
    const ushort populationPointer = 0x9580;
    const ushort tilesetPointer = 0x9580;
    const ushort vulnerabilityPointer = 0xedea;
    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var cgram = new SnesCgram();

    WriteEnemyDefinition(
        bus,
        definitionPointer,
        tileDataSize: 0,
        palettePointer: 0xe457,
        bank: 0xa2,
        tileDataAddress: 0xa28000,
        bossId: 0,
        namePointer: 0,
        fieldSeed: 0x5200);
    int header = 0xa00000 | definitionPointer;
    WriteWord(bus, header + 4, 200);
    WriteWord(bus, header + 6, 5);
    WriteWord(bus, header + 8, 8);
    WriteWord(bus, header + 10, 4);
    bus.WriteByte(header + 13, 0);
    WriteWord(bus, header + 18, 0xe49f);
    WriteWord(bus, header + 24, 0xe4da);
    WriteWord(bus, header + 48, 0x8023);
    WriteWord(bus, header + 50, 0x802d);
    WriteWord(bus, header + 60, vulnerabilityPointer);

    // Logical speed one occupies the second eight-byte common-speed record. The pair is
    // +1.0000 and -1.0000 in signed 16.16 form, making wall alignment easy to observe.
    WriteWord(bus, 0xa28187 + 8, 0x0001);
    WriteWord(bus, 0xa28187 + 10, 0x0000);
    WriteWord(bus, 0xa28187 + 12, 0xffff);
    WriteWord(bus, 0xa28187 + 14, 0x0000);

    // Literal four-frame Ripper lists. The map words are the cartridge addresses; the test
    // need not render their entry payloads to prove list selection and timing.
    ushort[] rightList = [8, 0xe54b, 7, 0xe557, 8, 0xe54b, 7, 0xe563, 0x80ed, 0xe477];
    ushort[] leftList = [8, 0xe527, 7, 0xe533, 8, 0xe527, 7, 0xe53f, 0x80ed, 0xe48b];
    for (int index = 0; index < rightList.Length; index++)
        WriteWord(bus, 0xa2e477 + index * 2, rightList[index]);
    for (int index = 0; index < leftList.Length; index++)
        WriteWord(bus, 0xa2e48b + index * 2, leftList[index]);

    // Power Beam is ineffective, Ice freezes, and missiles/supers use multiplier two.
    for (int offset = 0; offset < 22; offset++)
        bus.WriteByte(0xb40000 | (vulnerabilityPointer + offset), 0);
    bus.WriteByte(0xb40000 | (vulnerabilityPointer + 2), 0xff);
    bus.WriteByte(0xb40000 | (vulnerabilityPointer + 12), 2);
    bus.WriteByte(0xb40000 | (vulnerabilityPointer + 13), 2);

    WriteWord(bus, 0xb40000 | tilesetPointer, definitionPointer);
    WriteWord(bus, (0xb40000 | tilesetPointer) + 2, 0);
    WriteWord(bus, (0xb40000 | tilesetPointer) + 4, 0xffff);

    int population = 0xa10000 | populationPointer;
    WriteWord(bus, population, definitionPointer);
    WriteWord(bus, population + 2, 80);
    WriteWord(bus, population + 4, 64);
    WriteWord(bus, population + 6, 0);
    WriteWord(bus, population + 8, 0x2800);
    WriteWord(bus, population + 10, 0);
    WriteWord(bus, population + 12, 1);
    WriteWord(bus, population + 14, 1);
    WriteWord(bus, population + 16, 0xffff);
    bus.WriteByte(population + 18, 1);

    const int roomWidth = 16;
    const int roomHeight = 8;
    ushort[] foreground = new ushort[roomWidth * roomHeight];
    // Column six begins at X=96. Ripper's eight-pixel radius therefore aligns its center to
    // X=88 and reverses when its positive leading edge first enters this wall.
    for (int row = 0; row < roomHeight; row++)
        foreground[row * roomWidth + 6] = 0x8000;
    RoomLevelData level = new(
        roomWidth,
        roomHeight,
        foreground,
        new byte[foreground.Length],
        new ushort[foreground.Length],
        new byte[8]);

    var enemies = new RoomEnemySystem();
    enemies.Load(bus, populationPointer, tilesetPointer, vram, cgram, () => 0);
    RoomEnemySlot ripper = enemies.Slots[0];
    AssertEqual(0xe477, ripper.CurrentInstruction, "Ripper init right animation list");
    AssertEqual(1, ripper.VariableD, "Ripper init signed whole X velocity");
    AssertEqual(0, ripper.VariableC, "Ripper init X subvelocity");

    for (int frame = 0; frame < 9; frame++)
        enemies.StepFrame(0, 0, timeIsFrozen: false, level: level);
    AssertEqual(88, ripper.XPosition, "Ripper wall-aligned reversal X");
    AssertEqual(0xffff, ripper.VariableD, "Ripper reversal signed whole X velocity");
    AssertEqual(0xe527, ripper.SpritemapPointer, "Ripper reversal selects left-facing frame");

    // WriteEnemyOAM `$A0:947B` applies enemy shake to the shared ordinary-spritemap
    // origin. One synthetic one-entry map makes both signs and the timer consumption
    // observable without borrowing the production displacement calculation.
    WriteWord(bus, 0xa2e527, 1);
    WriteWord(bus, 0xa2e529, 0);
    bus.WriteByte(0xa2e52b, 0);
    WriteWord(bus, 0xa2e52c, 0);
    var shakenOam = new OamBuffer();
    ripper.FrameCounter = 0;
    ripper.ShakeTimer = 2;
    shakenOam.BeginFrame();
    enemies.DrawLayers(shakenOam, 0, 0, 0, 7);
    shakenOam.FinalizeFrame();
    AssertEqual(89, shakenOam.GetEntry(0).X,
        "enemy shake frame-counter bit one clear adds one X pixel");
    AssertEqual(1, ripper.ShakeTimer, "enemy draw consumes one shake tick");

    ripper.FrameCounter = 2;
    ripper.ShakeTimer = 2;
    shakenOam.BeginFrame();
    enemies.DrawLayers(shakenOam, 0, 0, 0, 7);
    shakenOam.FinalizeFrame();
    AssertEqual(87, shakenOam.GetEntry(0).X,
        "enemy shake frame-counter bit one set subtracts one X pixel");
    AssertEqual(1, ripper.ShakeTimer, "negative enemy draw consumes one shake tick");

    var samus = new SamusState
    {
        Health = 99,
        XPosition = ripper.XPosition,
        YPosition = ripper.YPosition,
    };
    AssertTrue(enemies.ResolveOrdinarySamusContact(samus, controllerInput: 0),
        "Ripper radius contact reaches common touch AI");
    AssertEqual(94, samus.Health, "Ripper header contact damage");
    AssertEqual(0x60, samus.InvincibilityTimer, "Ripper contact invincibility clock");
    AssertEqual(5, samus.KnockbackTimer, "Ripper contact knockback clock");

    var projectiles = new SamusProjectileSystem();
    var sharedProjectiles = new SamusBombProjectileSystem();
    WriteWord(bus, 0x93867b, 0x9100);
    WriteWord(bus, 0x93867f, 0x9200);

    static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort type,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    SamusProjectileSlot shot = projectiles.Slots[0];
    ArmProjectile(shot, ripper, type: 0, damage: 20);
    AssertEqual(1, enemies.ResolveOrdinaryProjectileHits(bus, projectiles, sharedProjectiles),
        "Ripper ineffective power-beam collision");
    AssertEqual(200, ripper.Health, "Ripper power-beam immunity comes from ROM vulnerability");

    ArmProjectile(shot, ripper, type: 2, damage: 20);
    AssertEqual(1, enemies.ResolveOrdinaryProjectileHits(bus, projectiles, sharedProjectiles),
        "Ripper Ice Beam collision");
    AssertEqual(400, ripper.FrozenTimer, "Ripper Ice vulnerability freezes actor");
    ushort frozenX = ripper.XPosition;
    enemies.StepFrame(0, 0, timeIsFrozen: false, level: level);
    AssertEqual(frozenX, ripper.XPosition, "frozen Ripper suppresses movement");
    AssertEqual(399, ripper.FrozenTimer, "frozen Ripper timer advances");

    // Clear the focused freeze fixture and prove the missile multiplier, hurt flash, and
    // common death/deletion accounting on the same loaded actor.
    ripper.FrozenTimer = 0;
    ripper.InvincibilityTimer = 0;
    ripper.AiHandlerBits = 0;
    for (int hit = 0; hit < 2; hit++)
    {
        ArmProjectile(shot, ripper, type: 0x0100, damage: 100);
        AssertEqual(1, enemies.ResolveOrdinaryProjectileHits(bus, projectiles, sharedProjectiles),
            $"Ripper missile hit {hit + 1}");
    }
    AssertEqual(0, ripper.Health, "Ripper missile vulnerability reaches zero health");
    AssertEqual((ushort)0, ripper.EnemyDefinitionPointer,
        "Ripper generic death clears its common enemy record immediately");
    RoomEnemyProjectileSlot ripperDeath = enemies.EnemyProjectiles[17];
    AssertEqual(RoomEnemyProjectileKind.EnemyDeathExplosion, ripperDeath.Kind,
        "Ripper zero health allocates the shared F345 death actor");
    AssertEqual(definitionPointer, ripperDeath.EnemyHeaderPointer,
        "Ripper death actor retains its header for EEAF drop selection");
    AssertEqual(1, enemies.EnemiesKilled, "Ripper death increments room kill count");

    Console.WriteLine(
        "  Ripper: ROM load, animation, 16.16 movement, wall reversal, contact, " +
        "vulnerabilities, freeze, damage, and death agree.");
}

/// <summary>
/// Proves that the stationary Ceres elevator platform is animated by the room's variant-two
/// door actor, not by either arrival projectile that is deleted when the platform lands.
/// </summary>
static void VerifyCeresElevatorPlatformAnimation()
{
    const ushort definitionPointer = 0xe23f;
    const ushort populationPointer = 0x9480;
    const ushort tilesetPointer = 0x9480;
    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var cgram = new SnesCgram();

    WriteEnemyDefinition(
        bus,
        definitionPointer,
        tileDataSize: 0,
        palettePointer: 0xf4ec,
        bank: 0xa6,
        tileDataAddress: 0xa68000,
        bossId: 0,
        namePointer: 0,
        fieldSeed: 0x4300);
    WriteWord(bus, 0xa00000 | (definitionPointer + 18), 0xf6c5);
    WriteWord(bus, 0xa00000 | (definitionPointer + 24), 0xf765);
    bus.WriteByte(0xa00000 | (definitionPointer + 57), 2);

    // Parameter one equals two, selecting main function $F850. This focused population
    // omits ProcessInstructions because only the independently executing main AI owns the
    // persistent platform transfer.
    int population = 0xa10000 | populationPointer;
    WriteWord(bus, population, definitionPointer);
    WriteWord(bus, population + 2, 0x0080);
    WriteWord(bus, population + 4, 0x0080);
    WriteWord(bus, population + 6, 0);
    WriteWord(bus, population + 8, 0x0800);
    WriteWord(bus, population + 10, 0);
    WriteWord(bus, population + 12, 2);
    WriteWord(bus, population + 14, 0);
    WriteWord(bus, population + 16, 0xffff);
    bus.WriteByte(population + 18, 0);
    WriteWord(bus, 0xb40000 | tilesetPointer, 0xffff);
    WriteWord(bus, 0xa6f72f, 0xf850);
    WriteWord(bus, 0xa6f530, 0xfac7);

    // These are the literal two one-entry transfer records at $A6:F904/$F90E. Each writes
    // four low map bytes to Mode-7 word $060E, alternating by NMI bit one.
    WriteWord(bus, 0xa6f900, 0xf904);
    WriteWord(bus, 0xa6f902, 0xf90e);
    bus.WriteByte(0xa6f904, 0x80);
    WriteLong(bus, 0xa6f905, 0xa6f918);
    WriteWord(bus, 0xa6f908, 4);
    WriteWord(bus, 0xa6f90a, 0x060e);
    bus.WriteByte(0xa6f90c, 0);
    bus.WriteByte(0xa6f90d, 0);
    bus.WriteByte(0xa6f90e, 0x80);
    WriteLong(bus, 0xa6f90f, 0xa6f91c);
    WriteWord(bus, 0xa6f912, 4);
    WriteWord(bus, 0xa6f914, 0x060e);
    bus.WriteByte(0xa6f916, 0);
    bus.WriteByte(0xa6f917, 0);
    byte[] light = [0x68, 0x69, 0x69, 0x78];
    byte[] dark = [0x8d, 0x8e, 0x8e, 0x79];
    for (int index = 0; index < 4; index++)
    {
        bus.WriteByte(0xa6f918 + index, light[index]);
        bus.WriteByte(0xa6f91c + index, dark[index]);
    }
    for (int color = 0; color < 6; color++)
        WriteWord(bus, 0xa6f871 + color * 2, unchecked((ushort)(0x4400 + color)));

    var enemies = new RoomEnemySystem();
    enemies.Load(bus, populationPointer, tilesetPointer, vram, cgram, () => 0);
    enemies.StepFrame(0, 0, timeIsFrozen: false);
    for (int index = 0; index < 4; index++)
        AssertEqual(light[index], vram.ReadByte((0x060e + index) * 2),
            "Ceres elevator landed platform light map frame");

    enemies.StepFrame(0, 0, timeIsFrozen: false);
    enemies.StepFrame(0, 0, timeIsFrozen: false);
    for (int index = 0; index < 4; index++)
        AssertEqual(dark[index], vram.ReadByte((0x060e + index) * 2),
            "Ceres elevator landed platform dark map frame");

    Console.WriteLine("  Ceres elevator: landed Mode-7 platform continues its ROM transfer animation.");
}

/// <summary>
/// Verifies the area-boss branch in Ridley's physical door actor. This is separate from the
/// type-$9 room cap: the enemy is solid during the fight and its ROM instruction stream must
/// make it intangible after `$A6:C117` sets Ceres's area-boss bit.
/// </summary>
static void VerifyCeresDoorBossBranch()
{
    const ushort definitionPointer = 0xe23f;
    const ushort populationPointer = 0x94c0;
    const ushort tilesetPointer = 0x94c0;
    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var cgram = new SnesCgram();

    WriteEnemyDefinition(
        bus,
        definitionPointer,
        tileDataSize: 0,
        palettePointer: 0xf4ec,
        bank: 0xa6,
        tileDataAddress: 0xa68000,
        bossId: 0,
        namePointer: 0,
        fieldSeed: 0x4380);
    WriteWord(bus, 0xa00000 | (definitionPointer + 18), 0xf6c5);
    WriteWord(bus, 0xa00000 | (definitionPointer + 24), 0xf765);
    bus.WriteByte(0xa00000 | (definitionPointer + 57), 2);

    int population = 0xa10000 | populationPointer;
    WriteWord(bus, population, definitionPointer);
    WriteWord(bus, population + 2, 0x0008);
    WriteWord(bus, population + 4, 0x007f);
    WriteWord(bus, population + 6, 0);
    // Property $8000 makes the focused actor solid, $2000 runs its instruction list,
    // and $0800 keeps that list processing even when the synthetic actor is offscreen.
    WriteWord(bus, population + 8, 0xA800);
    WriteWord(bus, population + 10, 0);
    WriteWord(bus, population + 12, 3);
    WriteWord(bus, population + 14, 0);
    WriteWord(bus, population + 16, 0xffff);
    bus.WriteByte(population + 18, 0);
    WriteWord(bus, 0xb40000 | tilesetPointer, 0xffff);

    // Start directly at the retail closed-door loop. `$F66A` either returns to `$F55E`
    // while the boss lives or falls through `$F6B0/$80ED` into the normal list, whose first
    // command `$F68B` is the observable collision-side-effect under test.
    WriteWord(bus, 0xa6f731, 0xf770);
    WriteWord(bus, 0xa6f532, 0xf55e);
    WriteWord(bus, 0xa6f55e, 2);
    WriteWord(bus, 0xa6f560, 0x9000);
    WriteWord(bus, 0xa6f562, 0xf66a);
    WriteWord(bus, 0xa6f564, 0xf55e);
    WriteWord(bus, 0xa6f566, 0xf6b0);
    WriteWord(bus, 0xa6f568, 0x80ed);
    WriteWord(bus, 0xa6f56a, 0xf56c);
    WriteWord(bus, 0xa6f56c, 0xf68b);
    WriteWord(bus, 0xa6f56e, 0xf6a6);
    WriteWord(bus, 0xa6f570, 2);
    WriteWord(bus, 0xa6f572, 0x9000);

    bool areaBossDefeated = false;
    int areaBossReads = 0;
    var enemies = new RoomEnemySystem();
    enemies.Load(
        bus,
        populationPointer,
        tilesetPointer,
        vram,
        cgram,
        () => 0,
        isAreaBossDefeated: () =>
        {
            areaBossReads++;
            return areaBossDefeated;
        });
    RoomEnemySlot door = enemies.Slots[0];

    for (int frame = 0; frame < 4; frame++)
        enemies.StepFrame(0, 0, timeIsFrozen: false);
    AssertEqual(0, door.Properties & (ushort)EnemyProperties.IgnoreSamusCollision,
        "living Ceres boss keeps Ridley-room door tangible");

    areaBossDefeated = true;
    // The boss bit can change while the two-frame closed map is already sleeping. Allow
    // that current record to expire, then require the very next `$F66A` visit to leave the
    // loop; the generous bound is diagnostic and does not alter actor state.
    for (int frame = 0;
        frame < 16 && !door.Properties.HasAny(EnemyProperties.IgnoreSamusCollision);
        frame++)
    {
        enemies.StepFrame(0, 0, timeIsFrozen: false);
    }
    AssertTrue(door.Properties.HasAny(EnemyProperties.IgnoreSamusCollision),
        $"defeated Ceres boss advances door bytecode to intangible setup " +
        $"(PC=${door.CurrentInstruction:X4}, timer=${door.InstructionTimer:X4}, " +
        $"properties=${(ushort)door.Properties:X4}, bossReads={areaBossReads})");
    Console.WriteLine("  Ceres door: area-boss branch opens the physical Ridley-room actor.");
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
    WriteWord(bus, 0xa00000 | (definitionPointer + 8), 8);
    WriteWord(bus, 0xa00000 | (definitionPointer + 10), 8);

    // The native post-battle path dynamically spawns Ceres-door variants five and six as
    // the two Mode-7 chamber walls. Keep their real header/dispatch/list boundaries in this
    // fixture so the escape audit cannot pass by changing only Ridley's logical status.
    const ushort ceresDoorDefinitionPointer = 0xe23f;
    WriteEnemyDefinition(
        bus,
        ceresDoorDefinitionPointer,
        tileDataSize: 0,
        palettePointer: 0xf4ec,
        bank: 0xa6,
        tileDataAddress: 0xa69000,
        bossId: 0,
        namePointer: 0,
        fieldSeed: 0x4100);
    WriteWord(bus, 0xa00000 | (ceresDoorDefinitionPointer + 18), 0xf6c5);
    WriteWord(bus, 0xa00000 | (ceresDoorDefinitionPointer + 24), 0xf765);
    bus.WriteByte(0xa00000 | (ceresDoorDefinitionPointer + 57), 2);
    WriteWord(bus, 0xa6f735, 0xf7a5);
    WriteWord(bus, 0xa6f737, 0xf7a5);
    WriteWord(bus, 0xa6f536, 0xf62a);
    WriteWord(bus, 0xa6f538, 0xf634);
    WriteWord(bus, 0xa6f62a, 0xf68b);
    WriteWord(bus, 0xa6f62c, 1);
    WriteWord(bus, 0xa6f62e, 0xface);
    WriteWord(bus, 0xa6f630, 0x80ed);
    WriteWord(bus, 0xa6f632, 0xf62c);
    WriteWord(bus, 0xa6f634, 0xf68b);
    WriteWord(bus, 0xa6f636, 1);
    WriteWord(bus, 0xa6f638, 0xfb2f);
    WriteWord(bus, 0xa6f63a, 0x80ed);
    WriteWord(bus, 0xa6f63c, 0xf636);

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

    // Active extended frame: one body component sits 32 pixels right of Ridley's origin.
    // Its 16x16 hitbox is outside the header's deliberately tiny 8x8 fallback radius,
    // proving that projectile collision walks cartridge hitboxes instead of a host box.
    WriteWord(bus, 0xa69000, 1);
    WriteWord(bus, 0xa69002, 32);
    WriteWord(bus, 0xa69004, 0);
    WriteWord(bus, 0xa69006, 0x9200);
    WriteWord(bus, 0xa69008, 0x9010);
    WriteWord(bus, 0xa69010, 1);
    WriteWord(bus, 0xa69012, unchecked((ushort)-8));
    WriteWord(bus, 0xa69014, unchecked((ushort)-8));
    WriteWord(bus, 0xa69016, 8);
    WriteWord(bus, 0xa69018, 8);
    WriteWord(bus, 0xa6901a, 0xdf59);
    WriteWord(bus, 0xa6901c, 0xdf8a);
    WriteWord(bus, 0xa69200, 0);

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
    for (int palette = 0; palette < 3; palette++)
    {
        for (int color = 0; color < 14; color++)
            WriteWord(bus, 0xa6e46a + palette * 28 + color * 2,
                unchecked((ushort)(0x7000 + palette * 0x0100 + color)));
    }

    // The two later animation lists are the actual dispatcher seams that make reveal-only
    // implementations fail. The compact roar list keeps its real $E4BE opcode before a
    // single timed map, so both the roaring flag and QueueSfx2_Max6($59) cross the actual
    // instruction dispatcher. The liftoff list retains its real $E969 instruction so AI
    // control changes exactly as it does in the cartridge. Movement divisors are the
    // literal $10..$01 D712 table.
    WriteWord(bus, 0xa6e690, 0xe4be);
    WriteWord(bus, 0xa6e692, 1);
    WriteWord(bus, 0xa6e694, 0x9000);
    WriteWord(bus, 0xa6e696, 0x812f);
    WriteWord(bus, 0xa6e548, 1);
    WriteWord(bus, 0xa6e54a, 0x9000);
    WriteWord(bus, 0xa6e54c, 0x812f);
    WriteWord(bus, 0xa6e91d, 0xe969);
    WriteWord(bus, 0xa6e91f, 1);
    WriteWord(bus, 0xa6e921, 0x9000);
    WriteWord(bus, 0xa6e923, 0x812f);
    for (int divisor = 0; divisor < 16; divisor++)
        bus.WriteByte(0xa6d712 + divisor, unchecked((byte)(16 - divisor)));

    // The tail position solver reads the shared signed 8.8 sine table at $A0:B443.
    // Populate the sixteen cardinal/intercardinal samples touched by the initialized
    // $4000,$4010... tail angles; an all-zero synthetic bus would otherwise make correct
    // polar-to-Cartesian code appear to leave every tail piece at the same coordinate.
    (byte Angle, short Value)[] sineSamples =
    [
        (0x00, 0), (0x10, 98), (0x20, 181), (0x30, 237),
        (0x40, 256), (0x50, 237), (0x60, 181), (0x70, 98),
        (0x80, 0), (0x90, -98), (0xa0, -181), (0xb0, -237),
        (0xc0, -256), (0xd0, -237), (0xe0, -181), (0xf0, -98),
    ];
    foreach ((byte angle, short value) in sineSamples)
        WriteWord(bus, 0xa0b443 + angle * 2, unchecked((ushort)value));

    // End-of-battle palette records are deliberately unmistakable. AA11 must not publish
    // status one without A9A0 first copying all three native destination ranges.
    for (int color = 0; color < 15; color++)
        WriteWord(bus, 0xa6a9e3 + color * 2, unchecked((ushort)(0x6100 + color)));
    for (int color = 0; color < 8; color++)
        WriteWord(bus, 0xa6aa01 + color * 2, unchecked((ushort)(0x6200 + color)));

    // End Mode 7 on its first room-main call, then give each of the two Ceres warning
    // transfer lists one unmistakable record. ProcessSpriteTilesTransfers is allowed to
    // fall through between phases, so both records must be queued on the first C04E call.
    WriteWord(bus, 0xa6ae4d, 0xffff);
    WriteWord(bus, 0xa6c4cb, 2);
    WriteWord(bus, 0xa6c4cd, 0x9200);
    bus.WriteByte(0xa6c4cf, 0xb0);
    WriteWord(bus, 0xa6c4d0, 0x7800);
    WriteWord(bus, 0xa6c4d2, 2);
    WriteWord(bus, 0xa6c4d4, 0x9202);
    bus.WriteByte(0xa6c4d6, 0xb0);
    WriteWord(bus, 0xa6c4d7, 0x7801);
    WriteWord(bus, 0xa6c4d9, 0);
    WriteWord(bus, 0xa6c4fe, 2);
    WriteWord(bus, 0xa6c500, 0x9204);
    bus.WriteByte(0xa6c502, 0xb0);
    WriteWord(bus, 0xa6c503, 0x7802);
    WriteWord(bus, 0xa6c505, 2);
    WriteWord(bus, 0xa6c507, 0x9206);
    bus.WriteByte(0xa6c509, 0xb0);
    WriteWord(bus, 0xa6c50a, 0x7803);
    WriteWord(bus, 0xa6c50c, 0);
    WriteWord(bus, 0xb09200, 0x1234);
    WriteWord(bus, 0xb09202, 0x5678);
    WriteWord(bus, 0xb09204, 0x9abc);
    WriteWord(bus, 0xb09206, 0xdef0);
    WriteCeresEnglishEscapeWarning(bus);
    for (int paletteFrame = 0; paletteFrame < 16; paletteFrame++)
    {
        for (int color = 0; color < 3; color++)
            WriteWord(bus, 0xa6c1df + paletteFrame * 6 + color * 2,
                unchecked((ushort)(0x4000 + paletteFrame * 0x10 + color)));
    }

    // One minimal power-beam data/list pair lets this regression reach Ridley's shot AI
    // through the public producer and explosion owner, rather than mutating HitCounter.
    WritePoseDefinition(
        bus,
        SamusPoseIds.FacingRightNormalPose,
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
    RidleyEnemyState state = enemies.CeresRidley
        ?? throw new InvalidOperationException("Ceres Ridley did not allocate its extended state.");
    AssertEqual(0x00ba, ridley.XPosition, "Ceres Ridley init X");
    AssertEqual(0x00a9, ridley.YPosition, "Ceres Ridley init Y overrides population Y");
    AssertEqual(0xe538, ridley.CurrentInstruction, "Ceres Ridley initial instruction list");
    AssertEqual(0x0e00, ridley.PaletteIndex, "Ceres Ridley dedicated OBJ palette");
    AssertEqual(0x3c00, ridley.Properties, "Ceres Ridley native initialized properties");
    AssertEqual(0x804f, ridley.SpritemapPointer, "Ceres Ridley post-init empty extended map");
    AssertEqual((ushort)RidleyAiFunction.WaitForDoorTransition, (ushort)state.Function,
        "Ceres Ridley initial dispatcher function");
    AssertEqual(0x2000, cgram.Colors[0xa0], "Ceres Ridley first added palette color");
    AssertEqual(0x201f, cgram.Colors[0xbf], "Ceres Ridley final added palette color");
    AssertEqual(0, cgram.Colors[0xf1], "Ceres Ridley hidden body palette starts black");
    AssertEqual(0, cgram.Colors[0xff], "Ceres Ridley hidden body palette ends black");

    // Ridley's private `$A6:A2F2` door overlay bypasses WriteEnemyOAM and reproduces the
    // retail byte-index bug in its word table. Earthquake timer two therefore reads byte
    // $FC and displaces the door four pixels left.
    WriteWord(bus, 0xa6a329, 1);
    WriteWord(bus, 0xa6a32b, 0);
    bus.WriteByte(0xa6a32d, 0);
    WriteWord(bus, 0xa6a32e, 0);
    bus.WriteByte(0xa6a321, 0x00);
    bus.WriteByte(0xa6a322, 0x00);
    bus.WriteByte(0xa6a323, 0xfc);
    bus.WriteByte(0xa6a324, 0xff);
    RoomEnemySlot overlayDoor = enemies.Slots[1];
    overlayDoor.EnemyDefinitionPointer = ceresDoorDefinitionPointer;
    overlayDoor.VariableB = 1;
    overlayDoor.XPosition = 100;
    overlayDoor.YPosition = 80;
    enemies.CeresStatus = 1; // Skip the unrelated private Baby list in this draw fixture.
    enemies.EarthquakeTimer = 2;
    var doorOam = new OamBuffer();
    doorOam.BeginFrame();
    enemies.DrawCeresRidleyImmediateBabyAndDoor(doorOam, 0, 0);
    doorOam.FinalizeFrame();
    AssertEqual(96, doorOam.GetEntry(0).X,
        "Ceres private door hook consumes byte-indexed -4 quake offset");
    overlayDoor.EnemyDefinitionPointer = 0;
    enemies.CeresStatus = 0;
    enemies.EarthquakeTimer = 0;

    enemies.StepFrame(cameraX: 0, cameraY: 0, timeIsFrozen: false);
    AssertEqual((ushort)RidleyAiFunction.InitialDelay, (ushort)state.Function,
        "Ceres Ridley door-clear transition");
    AssertEqual(511, state.FunctionTimer, "Ceres Ridley first delay decrement");
    AssertEqual(0x9000, ridley.SpritemapPointer, "Ceres Ridley left-facing initial map");
    AssertEqual(0xe540, ridley.CurrentInstruction, "Ceres Ridley initial list sleeps");

    for (int frame = 0; frame < 512; frame++)
        enemies.StepFrame(cameraX: 0, cameraY: 0, timeIsFrozen: false);
    AssertEqual((ushort)RidleyAiFunction.FadeInEyes, (ushort)state.Function,
        "Ceres Ridley 512-counter underflow starts eye fade");

    for (int frame = 0; frame < 65; frame++)
        enemies.StepFrame(cameraX: 0, cameraY: 0, timeIsFrozen: false);
    AssertEqual((ushort)RidleyAiFunction.FadeInBody, (ushort)state.Function,
        "Ceres Ridley eye table terminator starts body fade");
    AssertEqual(1, state.MovementAnimationEnabled,
        "Ceres Ridley eye fade enables composite animation");
    AssertEqual(0, state.TailFunctionIndex,
        "Ceres Ridley resting tail has not started its liftoff motion");
    AssertTrue(
        state.TailSegments
            .Select(segment => (segment.XPosition, segment.YPosition))
            .Distinct()
            .Count() > 1,
        "Ceres Ridley resting tail is articulated before liftoff");

    for (int frame = 0; frame < 32; frame++)
        enemies.StepFrame(cameraX: 0, cameraY: 0, timeIsFrozen: false);
    AssertEqual((ushort)RidleyAiFunction.WaitBeforeRoar, (ushort)state.Function,
        "Ceres Ridley body fade completes");
    AssertEqual(4, state.FunctionTimer, "Ceres Ridley pre-roar timer");
    AssertEqual(0x5000 + 0x14a / 2, cgram.Colors[0x91],
        "Ceres Ridley final body-fade row reaches OBJ palette one");
    AssertEqual(0, ridley.Properties & (ushort)EnemyProperties.IgnoreSamusCollision,
        "Ceres Ridley becomes tangible after body fade");
    AssertTrue(enemies.MusicRequests.Contains(new EnemyMusicRequest(
            MusicCommand.SelectTrack(5),
            MusicCommandDelay.EightFrames)),
        "Ceres Ridley final body fade queues battle/escape track five");

    // Finish the 5-frame pre-roar and 253-frame pre-liftoff countdowns, then let the real
    // E969 instruction arm A6AF. Movement uses the translated common 8.8 integrator until
    // A6C8 crosses Y=$50 and publishes fight mode one.
    var samus = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        Health = 99,
    };
    int battleEntryFrames = 0;
    bool observedRoarSound = false;
    while (state.Function != RidleyAiFunction.CeresHovering && battleEntryFrames < 1024)
    {
        enemies.StepFrame(0, 0, timeIsFrozen: false, samus);
        observedRoarSound |= enemies.SoundRequests.Contains(
            new EnemySoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x59), MaximumQueued: 6));
        battleEntryFrames++;
    }
    AssertEqual((ushort)RidleyAiFunction.CeresHovering, (ushort)state.Function,
        "Ceres Ridley liftoff reaches hover dispatcher");
    AssertEqual(1, state.FightMode, "Ceres Ridley liftoff enables battle mode");
    AssertTrue(ridley.YPosition < 80,
        "Ceres Ridley crosses the cartridge's Y=$50 battle threshold");
    AssertTrue(observedRoarSound,
        "Ridley instruction $E4BE publishes QueueSfx2_Max6($59)");

    // Ceres lunge retains neutral-tail AI. Once Ridley closes within 128 pixels,
    // `$A6:CC9A-$CCB9` aims a real seven-segment whip at Samus instead of merely moving
    // the body/wing composite. Start from the cartridge's all-active steady state so this
    // assertion isolates the missing request/target side effects reported in gameplay.
    foreach (RidleyTailSegment segment in state.TailSegments)
    {
        segment.Active = true;
        segment.StaggerAngle = 0xffff;
        segment.MovementDirection = 0x8000;
        segment.TargetDistance = 0;
    }
    state.TailSegments[0].Angle = 0x4000;
    state.TailWhipTargetClockwiseAngle = 0xffff;
    state.TailWhipTargetCounterClockwiseAngle = 0xffff;
    state.Function = RidleyAiFunction.CeresLungeSetup;
    samus.XPosition = unchecked((ushort)(ridley.XPosition - 64));
    samus.YPosition = unchecked((ushort)(ridley.YPosition + 68));
    enemies.StepFrame(0, 0, timeIsFrozen: false, samus);
    AssertEqual((ushort)RidleyAiFunction.CeresLungeMain, (ushort)state.Function,
        "Ceres Ridley enters lunge main");
    AssertTrue(state.TailWhipTargetClockwiseAngle != 0xffff,
        "Ceres Ridley lunge installs cartridge tail-whip target");
    AssertEqual(8, state.TailAngleDelta,
        "Ceres Ridley aimed whip uses eight-angle step");
    AssertTrue(state.TailSegments.Skip(1).Any(segment => segment.TargetDistance == 0x0c00),
        "Ceres Ridley lunge extends articulated tail segments");

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

    bool observedNormalRidleyPalette = false;
    bool observedFlashRidleyPalette = false;
    for (int hit = 0; hit < 100; hit++)
    {
        // Zero fixture muzzle offsets place each stationary power beam at the active ROM
        // component's center, 32 pixels right of Ridley's live origin. Running the public
        // producer proves slot allocation/type/radii before the extended-hitbox walk.
        samus.XPosition = unchecked((ushort)(ridley.XPosition + 32));
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
        observedNormalRidleyPalette |= state.CommonDrawPaletteIndex == 0x0e00;
        observedFlashRidleyPalette |= state.CommonDrawPaletteIndex == 0;
        if (hit == 49)
            AssertEqual(0x7000, cgram.Colors[0xf1],
                "Ceres Ridley 50-hit health palette");
        if (hit == 69)
            AssertEqual(0x7200, cgram.Colors[0xf1],
                "Ceres Ridley 70-hit missing-branch palette quirk");

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
    AssertTrue(observedNormalRidleyPalette && observedFlashRidleyPalette,
        "Ceres Ridley beam hits alternate common body damage palette");

    int retreatFrames = 0;
    bool observedFakeRetreat = false;
    while (enemies.CeresStatus != 1 && retreatFrames < 1024)
    {
        enemies.StepFrame(0, 0, timeIsFrozen: false, samus);
        observedFakeRetreat |= state.Function is
            RidleyAiFunction.CeresFakeRetreatMoveToPosition or
            RidleyAiFunction.CeresFakeRetreatRising or
            RidleyAiFunction.CeresWaitBeforeRetrievingBaby or
            RidleyAiFunction.CeresRetrieveBaby;
        retreatFrames++;
    }
    AssertTrue(observedFakeRetreat,
        "Ceres Ridley returns from current attack through fake retreat/Baby retrieval");
    AssertEqual(0, state.FightMode, "Ceres Ridley retreat disables battle mode");
    AssertEqual(1, enemies.CeresStatus, "Ceres Ridley publishes escape handoff status");
    AssertEqual((ushort)RidleyAiFunction.CeresInactive, (ushort)state.Function,
        "Ceres Ridley installs null dispatcher after battle");
    AssertTrue(ridley.Properties.HasAny(EnemyProperties.Invisible),
        "Ceres Ridley ordinary actor yields to getaway presentation");
    RoomEnemySlot[] mode7Walls = enemies.Slots
        .Where(slot => slot.EnemyDefinitionPointer == ceresDoorDefinitionPointer &&
            slot.Parameter1 is 5 or 6)
        .ToArray();
    AssertEqual(2, mode7Walls.Length,
        "Ceres Ridley spawns both native Mode-7 wall actors");
    AssertEqual(0x6100, cgram.Colors[0x51],
        "Ceres Ridley retreat copies BG palette-five colors");
    AssertEqual(0x6200, cgram.Colors[0x21],
        "Ceres Ridley retreat copies BG palette-two colors");
    AssertEqual(0x6200, cgram.Colors[0xf1],
        "Ceres Ridley retreat copies OBJ palette-seven colors");

    var escapeWrites = new VramWriteQueue();
    enemies.StepFrame(0, 0, timeIsFrozen: false, samus, vramWriteQueue: escapeWrites);
    AssertTrue(enemies.SoundRequests.Contains(
            new EnemySoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x4e), MaximumQueued: 6)),
        "first Mode-7 getaway entry publishes QueueSfx2_Max6($4E)");
    AssertTrue(state.Mode7Finished, "Ceres Ridley consumes the Mode-7 terminator");
    AssertEqual((ushort)RidleyAiFunction.CeresActivateSelfDestruct, (ushort)state.Function,
        "Ceres Ridley Mode-7 terminator installs shared self-destruct dispatcher");
    AssertEqual(0, escapeWrites.Entries.Count,
        "Mode-7 terminator does not run the new actor function in the same enemy frame");

    enemies.StepFrame(0, 0, timeIsFrozen: false, samus, vramWriteQueue: escapeWrites);
    AssertEqual(1, escapeWrites.Entries.Count,
        "Ceres self-destruct queues only one first-list record per enemy frame");
    AssertEqual(2, state.FunctionTimer,
        "Ceres self-destruct remains in first transfer phase while records remain");

    enemies.StepFrame(0, 0, timeIsFrozen: false, samus, vramWriteQueue: escapeWrites);
    AssertEqual(3, escapeWrites.Entries.Count,
        "Ceres self-destruct final first-list record falls through to one second-list record");
    AssertEqual(0xb09200, escapeWrites.Entries[0].SourceAddress,
        "Ceres first warning transfer source");
    AssertEqual(0x7802, escapeWrites.Entries[2].EncodedVramDestination,
        "Ceres second warning transfer destination");
    AssertEqual(4, state.FunctionTimer,
        "Ceres self-destruct remains in second transfer phase while records remain");

    enemies.StepFrame(0, 0, timeIsFrozen: false, samus, vramWriteQueue: escapeWrites);
    AssertEqual(5, escapeWrites.Entries.Count,
        "Ceres self-destruct queues the final second-list record and emergency tilemap");
    AssertEqual(0xa6c164, escapeWrites.Entries[4].SourceAddress,
        "Ceres EMERGENCY text uses the cartridge tilemap words");
    AssertEqual(0x50cb, escapeWrites.Entries[4].EncodedVramDestination,
        "Ceres EMERGENCY text targets BG1 row six");
    AssertEqual(6, state.FunctionTimer,
        "Ceres self-destruct reaches the native 128-frame English hold");
    AssertEqual(128, state.CeresEscapeTextDelayTimer,
        "Ceres English warning hold starts at 128");

    for (int frame = 0; frame < 127; frame++)
    {
        enemies.StepFrame(0, 0, timeIsFrozen: false, samus, vramWriteQueue: escapeWrites);
        AssertTrue(!enemies.CeresEscapeStartedThisFrame,
            $"Ceres escape does not publish early on hold frame {frame}");
    }
    enemies.StepFrame(0, 0, timeIsFrozen: false, samus, vramWriteQueue: escapeWrites);
    AssertTrue(!enemies.CeresEscapeStartedThisFrame,
        "Ceres escape does not skip the English warning after its 128-frame title hold");
    AssertEqual(10, state.FunctionTimer,
        "Ceres English warning enters the native typewriter phase");

    int typewriterFrames = 0;
    while (!enemies.CeresEscapeStartedThisFrame && typewriterFrames < 512)
    {
        enemies.StepFrame(0, 0, timeIsFrozen: false, samus, vramWriteQueue: escapeWrites);
        typewriterFrames++;
    }
    AssertTrue(enemies.CeresEscapeStartedThisFrame,
        "Ceres escape starts only after the complete English warning is typed");
    AssertTrue(typewriterFrames > 128,
        "Ceres English warning remains visible long enough to type all three lines");
    AssertEqual(0x3594, vram.ReadWord(0x5105),
        "Ceres warning renders the S in SELF from the cartridge typewriter alphabet");
    AssertEqual(0x3582, vram.ReadWord(0x5145),
        "Ceres warning renders the A in ACTIVATED on its second line");
    AssertEqual(0x3584, vram.ReadWord(0x5185),
        "Ceres warning renders the C in COLONY on its third line");
    AssertEqual(2, enemies.CeresStatus,
        "Ceres escape publishes status two for door destruction");
    AssertEqual((ushort)RidleyAiFunction.CeresSelfDestructPaletteOnly, (ushort)state.Function,
        "Ceres escape retains the native palette-only actor function");

    Console.WriteLine(
        "  Ceres Ridley: reveal, liftoff, real beam impacts, 100-hit battle exit, " +
        "retreat, Mode 7, warning DMA, and timed escape handoff agree.");
}

/// <summary>
/// Installs the exact USA/Japan-English typewriter program from <c>$A6:C450</c>. Keeping
/// all three destination commands in the fixture catches the former phase-six-to-twelve
/// jump, which rendered only EMERGENCY and never touched any of these tilemap rows.
/// </summary>
static void WriteCeresEnglishEscapeWarning(TestAddressSpace bus)
{
    int cursor = 0xa6c450;

    void Word(ushort value)
    {
        WriteWord(bus, cursor, value);
        cursor += 2;
    }

    void Text(string value)
    {
        foreach (char character in value)
            bus.WriteByte(cursor++, checked((byte)character));
    }

    Word(1);
    Word(2);
    Word(13);
    Word(0x5105);
    Text("SELF DESTRUCT SEQUENCE");
    Word(13);
    Word(0x5145);
    Text("ACTIVATED EVACUATE");
    Word(13);
    Word(0x5185);
    Text("COLONY IMMEDIATELY");
    Word(0);
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
