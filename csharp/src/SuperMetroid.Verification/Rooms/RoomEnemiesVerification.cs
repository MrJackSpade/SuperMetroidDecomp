using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.World.Artwork;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Ripper;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileNativeFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Hardware.SignedSineFixtureVerification;
using static SuperMetroid.Verification.Harness.SyntheticEnemyCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;

/// <summary>
/// Focused room-enemy loading and actor verifications that use cartridge-shaped
/// population, graphics-set, and enemy-definition fixtures.
/// </summary>
internal static class RoomEnemiesVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly System.Text.Json.JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };

/// <summary>
/// Seeds the two loading-fixture enemies' palettes and planar tiles at the bank-$A2/$A3
/// addresses their definitions name, plus seven spawn-source words at $B4:9200. Word 5 is a
/// real retail omission (RecordEnemySpawnData copies words 0-4 and 6), so it carries a
/// distinct sentinel that the managed snapshot must not normalize.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for room-enemy loading artwork and spawn-data words")]
static void SeedRoomEnemyLoadingArtworkAndSpawnData(
    TestAddressSpace bus,
    byte[] primaryPaletteBytes,
    byte[] specialPaletteBytes,
    byte[] primaryTileBytes,
    byte[] specialTileBytes)
{
    bus.WriteBytes(0xa29000, primaryPaletteBytes);
    bus.WriteBytes(0xa39300, specialPaletteBytes);
    bus.WriteBytes(0xa29100, primaryTileBytes);
    bus.WriteBytes(0xa39320, specialTileBytes);
    for (int word = 0; word < 7; word++)
        WriteTestWord(bus, 0xb49200 + word * 2, unchecked((ushort)(0x3100 + word)));
}

/// <summary>
/// Checks room-enemy population, 64-byte headers, graphics staging, palettes, spawn
/// snapshots, and the empty-population branch against a cartridge-shaped fixture.
/// </summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
internal static void VerifyRoomEnemyLoading()
{
    const ushort populationPointer = 0x9100;
    const ushort tilesetPointer = 0x9100;
    const EnemyDefinitionId primaryDefinitionPointer = (EnemyDefinitionId)0xd000;
    const EnemyDefinitionId specialDefinitionPointer = (EnemyDefinitionId)0xd040;

    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var cgram = new SnesCgram();

    // The first header is intentionally filled at every documented offset. Besides making
    // the parsed record useful to future damage/interaction translations, this catches a
    // one-word shift immediately rather than waiting for a distant enemy AI to misbehave.
    WriteEnemyDefinition(bus, primaryDefinitionPointer, new EnemyDefinitionFields { TileDataSize = 0x0040, PalettePointer = 0x9000, Bank = 0xa2, TileDataAddress = 0xa29100, BossId = 0x1234, NamePointer = 0x9200, FieldSeed = 0x1100 });

    // Bit fifteen selects the definition's encoded staging address instead of the ordinary
    // running $0800 offset. It is still part of the tile-index arithmetic in retail code.
    WriteEnemyDefinition(bus, specialDefinitionPointer, new EnemyDefinitionFields { TileDataSize = 0x8020, PalettePointer = 0x9300, Bank = 0xa3, TileDataAddress = 0xa39320, BossId = 0, NamePointer = 0, FieldSeed = 0x2200 });

    byte[] primaryPaletteBytes = Enumerable.Range(0, 32).Select(index => unchecked((byte)(0x20 + index))).ToArray();
    byte[] specialPaletteBytes = Enumerable.Range(0, 32).Select(index => unchecked((byte)(0x60 + index))).ToArray();
    byte[] primaryTileBytes = Enumerable.Range(0, 0x40).Select(index => unchecked((byte)(0x80 + index))).ToArray();
    byte[] specialTileBytes = Enumerable.Range(0, 0x20).Select(index => unchecked((byte)(0xc0 + index))).ToArray();
    SeedRoomEnemyLoadingArtworkAndSpawnData(bus, primaryPaletteBytes, specialPaletteBytes, primaryTileBytes, specialTileBytes);

    SeedRoomEnemyGraphicsSet(bus, tilesetPointer,
        new(primaryDefinitionPointer, PaletteRow: 3),
        new(specialDefinitionPointer, PaletteRow: 4, StagingBlock: 1));

    SeedRoomEnemyPopulation(bus, populationPointer, deathQuota: 1,
        new RoomEnemyPopulationRecord(primaryDefinitionPointer, new(0x0456, 0x0789), 0xabcd, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x1357, 0x2468));

    // Import the explicitly seeded fixture art before exercising the installed-only loader.
    var stockSheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>();
    var stockColors = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>();
    foreach ((EnemyDefinitionId pointer, byte[] planar, byte[] paletteBytes) in new[]
             {
                 new VerifyRoomEnemyLoadingCase(primaryDefinitionPointer, primaryTileBytes, primaryPaletteBytes),
                 new VerifyRoomEnemyLoadingCase(specialDefinitionPointer, specialTileBytes, specialPaletteBytes),
             })
    {
        int byteCount = planar.Length;
        int tiles = byteCount / RoomCharacterAtlasFormat.BytesPerTile;
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, tiles,
            out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        stockSheets.Add(pointer, RoomCharacterAtlas.Load(new MemoryStream(png.ToArray()), byteCount));
        var palette = new PaletteRgb5[EnemyPaletteSheet.ColorCount];
        for (int color = 0; color < palette.Length; color++)
        {
            ushort word = SnesLittleEndian.Word(paletteBytes[color * 2], paletteBytes[color * 2 + 1]);
            palette[color] = new PaletteRgb5
            {
                Red = word & 31,
                Green = word >> 5 & 31,
                Blue = word >> 10 & 31,
            };
        }
        stockColors.Add(pointer, EnemyPaletteSheet.Load(new MemoryStream(
            EnemyPaletteSheet.Write(new EnemyPaletteSheetDocument { Version = 1, Colors = palette }))));
    }
    var enemies = new RoomEnemySystem(new EnemySession(), definitions: bus, services: new StandaloneRoomEnemyServices(() => 0x9999))
    {
        TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = stockSheets, Palettes = stockColors }),
    };
    enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(populationPointer), bus.ReadEnemyGraphicsSet(tilesetPointer), BgArtworkFixtures.For(vram), cgram));

    AssertEqual(1, enemies.SlotTable.EnemyCount, "enemy population count");
    AssertEqual(EnemySlotTable.NativeSlotSize, enemies.SlotTable.FirstFreeEnemyIndex,
        "first free enemy keeps native byte offset");
    AssertEqual(1, enemies.PopulationLoader.DeathQuota, "population terminator death quota");
    AssertEqual(0, enemies.RoomStatus.EnemiesKilled, "room load clears killed-enemy count");
    AssertEqual(0x1234, enemies.RoomStatus.BossId, "nonzero definition publishes room boss ID");
    AssertEqual(2, enemies.GraphicsSet.Entries.Count, "terminated enemy graphics-set count");

    RoomEnemyGraphicsSetEntry ordinaryGraphics = enemies.GraphicsSet.Entries[0];
    RoomEnemyGraphicsSetEntry specialGraphics = enemies.GraphicsSet.Entries[1];
    AssertEqual(0x0800, ordinaryGraphics.StagingOffset, "ordinary enemy staging offset");
    AssertEqual(0x0200, specialGraphics.StagingOffset, "high-bit enemy encoded staging offset");
    AssertEqual(2, specialGraphics.VramTilesIndex,
        "second enemy tile index includes first definition size");
    // Room load stages the tiles; they reach VRAM, and the OBJ characters' record, through the NMI queue.
    var tileQueue = new VramWriteQueue();
    enemies.GraphicsSet.QueueGraphicsUploads(tileQueue);
    var seededTiles = new SeededEnemyTileTransfers(new Dictionary<int, byte[]>
    {
        [0xa29100] = primaryTileBytes,
        [0xa39320] = specialTileBytes,
    });
    tileQueue.DrainTo(vram, seededTiles, enemies.Room.Session.ObjCharacters, BgArtworkFixtures.For(vram));
    AssertEqual(2, enemies.Room.Session.ObjCharacters.Uploads.Count, "both enemy tile uploads are recorded for the OBJ page");
    AssertEqual(0x80, vram.ReadByte(0xe000), "ordinary enemy first tile byte");
    AssertEqual(0xbf, vram.ReadByte(0xe03f), "ordinary enemy final tile byte");
    AssertEqual(0xc0, vram.ReadByte(0xda00), "special enemy first tile byte");
    AssertEqual(0x2120, cgram.Colors[(3 + 8) * 16],
        "ordinary enemy palette destination and source");
    AssertEqual(0x6160, cgram.Colors[(4 + 8) * 16],
        "special enemy palette destination and source");

    RoomEnemySlot slot = enemies.SlotTable.Slots[0];
    AssertEqual(0x0456, slot.Position.X.Whole, "population X position");
    AssertEqual(0x0789, slot.Position.Y.Whole, "population Y position");
    AssertEqual(0x0600, slot.PaletteIndex, "graphics-set OBJ palette index");
    AssertEqual(0, slot.VramTilesIndex, "first graphics-set tile index");
    AssertEqual(0xa2, slot.Definition.Bank, "definition AI bank bound to native slot");
    AssertEqual(0x5a, slot.HurtAiTime, "definition hurt-AI time copied into native slot");
    AssertEqual(EnemyAiStates.None, slot.AiStates, "room load clears AI handler bits");
    AssertEqual(0, slot.FlashTimer.RawValue, "room load clears flash timer");
    AssertEqual(0, slot.InvincibilityTimer.RawValue, "room load clears invincibility timer");
    AssertEqual(0, slot.ShakeTimer.RawValue, "room load clears shake timer");
    AssertEqual(0x804f, slot.SpritemapPointer,
        "extended actor receives native extended-nothing map after initialization");

    RoomEnemyDefinition definition = slot.Definition;
    AssertEqual(0x110e, definition.HurtSoundEffect, "definition hurt SFX offset $0E");
    AssertEqual(0x1114, definition.PartCount, "definition part count offset $14");
    AssertEqual(GrappleAiRoutine.Cancel, definition.GrappleAi, "definition grapple AI offset $1A");
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

    // Repeating the installed-art load must preserve the same VRAM geometry while the
    // cartridge tile sources are forbidden. The second definition exercises the native
    // high-bit staging branch, so a single hard-coded destination cannot pass.
    var installedEnemies = new RoomEnemySystem(new EnemySession(), definitions: new EnemyTileSourceReadGuard(bus), services: new StandaloneRoomEnemyServices(() => 0x9999))
    {
        TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = stockSheets, Palettes = stockColors }),
    };
    var installedVram = new SnesVram();
    var installedCgram = new SnesCgram();
    installedEnemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(populationPointer), bus.ReadEnemyGraphicsSet(tilesetPointer), BgArtworkFixtures.For(installedVram), installedCgram));
    var installedQueue = new VramWriteQueue();
    installedEnemies.GraphicsSet.QueueGraphicsUploads(installedQueue);
    installedQueue.DrainTo(installedVram, seededTiles, installedEnemies.Room.Session.ObjCharacters, BgArtworkFixtures.For(installedVram));
    AssertTrue(vram.ImageEquals(installedVram),
        "installed enemy PNGs preserve both native ordinary and high-bit VRAM placements");
    AssertTrue(new CgramPalettes(cgram).ImageEquals(new CgramPalettes(installedCgram)),
        "enemy tile overrides cannot alter native palettes");
    AssertEqual(slot.Position.X.Whole, installedEnemies.SlotTable.Slots[0].Position.X.Whole,
        "enemy tile overrides do not alter population coordinates");
    AssertEqual(slot.Definition.Health, installedEnemies.SlotTable.Slots[0].Definition.Health,
        "enemy tile overrides do not alter health");
    AssertEqual(slot.Definition.Radius.X, installedEnemies.SlotTable.Slots[0].Definition.Radius.X,
        "enemy tile overrides do not alter hitboxes");
    AssertThrows<InvalidDataException>(() =>
        EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() })
            .LoadTo(primaryDefinitionPointer, 0x40, new VramStore(new SnesVram()).All, 0xe000),
        "missing installed enemy sheet fails at its actual upload");
    AssertThrows<InvalidDataException>(() =>
        (installedEnemies.TileArtwork ?? throw new InvalidOperationException("Verification requires installed enemy artwork.")).LoadTo(primaryDefinitionPointer, 0x20, new VramStore(new SnesVram()).All, 0xe000),
        "wrong-size enemy sheet cannot silently truncate its native transfer");

    // Load the next room's enemies, over the same session, with an empty population and a deliberately invalid
    // tileset. Native $A0:8A6D skips graphics processing, and InitializeEnemies' early return leaves
    // first-free/death-quota words untouched even though current enemy counts are cleared.
    enemies = new RoomEnemySystem(enemies.Room.Session, EnemyRoomResidue.Of(enemies), bus, new StandaloneRoomEnemyServices(() => 0));
    const ushort emptyPopulationPointer = 0x9400;
    SeedRoomEnemyPopulation(bus, emptyPopulationPointer, deathQuota: 0x7f);
    cgram.SetColor(128, Bgr555.FromWord(0x4567));
    vram.LoadBytes(0xe000, new byte[] { 0x5a });
    enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(emptyPopulationPointer), TestAddressSpace.UnusedGraphicsSet, BgArtworkFixtures.For(vram), cgram));

    AssertEqual(0, enemies.SlotTable.EnemyCount, "empty population clears enemy count");
    AssertEqual(0, enemies.GraphicsSet.Entries.Count, "empty population skips graphics set");
    AssertEqual(EnemySlotTable.NativeSlotSize, enemies.SlotTable.FirstFreeEnemyIndex,
        "empty population preserves native first-free quirk");
    AssertEqual(1, enemies.PopulationLoader.DeathQuota, "empty population preserves native death-quota quirk");
    AssertEqual(0, enemies.RoomStatus.BossId, "room load clears boss ID before empty population");
    AssertEqual(0x4567, cgram.Colors[128], "empty population leaves CGRAM untouched");
    AssertEqual(0x5a, vram.ReadByte(0xe000), "empty population leaves enemy VRAM untouched");

    Console.WriteLine(
        "  Enemies: complete headers, populations, spawn snapshots, palettes, tile staging, " +
        "boss state, placeholders, and empty-room behavior agree.");
}

/// <summary>
/// Forwards fixture definitions and mutable memory while rejecting native tile and
/// palette reads, proving the installed artwork path supplies those uploads.
/// </summary>
[CpuBusBoundary("exact bus test: enemy fixture source that forwards fixture tables and forbids the native enemy tile-upload ROM ranges")]
internal sealed class EnemyTileSourceReadGuard(TestAddressSpace source) :
    ISnesAddressSpace, IRoomEnemyDefinitionSource, IImportCartridgeSource,
    ISnesMutableMemory
{
    /// <summary>Forwards a compiled enemy header from the fixture source.</summary>
    public RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId pointer) =>
        source.ReadEnemyDefinition(pointer);

    /// <summary>Uses the compiled falling-tube placement; this fixture constructs none.</summary>
    public RoomEnemyPopulationRecord ReadFallingTubePopulation(SuperMetroid.Core.Enemies.MotherBrainTubePiece piece) =>
        CompiledRoomEnemyDefinitions.Instance.ReadFallingTubePopulation(piece);

    /// <summary>Rejects tile-source reads before forwarding other memory bytes.</summary>
    public byte ReadByte(int address)
    {
        RejectTileSource(address);
        return source.ReadByte(address);
    }

    /// <summary>Rejects cartridge tile-source reads while allowing other fixture ROM data.</summary>
    public byte ReadCartridgeByte(int address)
    {
        RejectTileSource(address);
        return source.ReadCartridgeByte(address);
    }

    /// <summary>Forwards WRAM reads; the tile guard applies only to cartridge art ranges.</summary>
    public byte ReadWorkRamByte(int address) => source.ReadWorkRamByte(address);

    /// <summary>Forwards fixture writes outside the guarded read path.</summary>
    public void WriteByte(int address, byte value) => source.WriteByte(address, value);

    /// <summary>Fails when room loading attempts to upload enemy art from native ROM ranges.</summary>
    private static void RejectTileSource(int address)
    {
        if (address is >= 0xa29100 and < 0xa29140 or >= 0xa39320 and < 0xa39340 or
            >= 0xa29000 and < 0xa29020 or >= 0xa39300 and < 0xa39320)
            throw new InvalidDataException($"Installed enemy tile upload read ROM ${address:X6}.");
    }
}

/// <summary>Checks that enemy memory access uses explicit WRAM mapping instead of generic source readers.</summary>
internal static void VerifyEnemyMappedSourceRouting()
{
    const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
    foreach (string removed in new[] { "ReadEnemySourceByte", "ReadWord", "ReadLong" })
        AssertTrue(!MetadataShape.DeclaresMethod(typeof(RoomEnemySystem), removed, flags),
            $"enemy subsystem has no generic {removed} source reader");
    VerifyEnemyWorkRamWordMapping(SuperMetroidAddressSpace.CreateWithoutCartridge());
    Console.WriteLine("Enemy memory boundary: generic source readers are absent; explicit WRAM wrap, mirror and region rejection pass.");
}

/// <summary>Checks fixed-bank WRAM word wrap, mirrors, and rejection of other address regions.</summary>
[CpuBusBoundary("exact bus test: enemy WRAM word bank wrap, low-window mirror and ROM/SRAM/hardware rejection")]
private static void VerifyEnemyWorkRamWordMapping(SuperMetroidAddressSpace memory)
{
    memory.WriteByte(0x7effff, 0x34);
    memory.WriteByte(0x7e0000, 0x12);
    memory.WriteByte(0x7f0000, 0xee);
    AssertEqual((ushort)0x1234, memory.ReadWorkRamWordFixedBank(0x7effff),
        "enemy WRAM word wraps inside its bank, not into the next bank");
    memory.WriteByte(0x800100, 0x78);
    memory.WriteByte(0x800101, 0x56);
    AssertEqual((ushort)0x5678, memory.ReadWorkRamWordFixedBank(0x800100),
        "enemy low-window WRAM alias reads physical live RAM");
    foreach (int address in new[] { 0xa08000, 0x702000, 0x804000 })
        AssertThrows<ArgumentOutOfRangeException>(() => memory.ReadWorkRamWordFixedBank(address),
            "typed enemy WRAM reader rejects ROM, SRAM and hardware windows");
}

/// <summary>
/// Reproduces the rapid-Super visual failure from room $A56B: a fresh owner and its
/// invisible linked slot occupy the same 32-pixel cell as a blocking eye-door projectile.
/// Bank $A0 marks both direction words and lets their own next pre-instruction dispose of
/// them; it never creates either of the visible bank-$93 missile-explosion programs here.
/// </summary>
internal static void VerifyEnemyProjectileCollisionLifecycle()
{
    const ushort populationPointer = 0x9400;
    var bus = new TestAddressSpace();
    SeedRoomEnemyPopulation(bus, populationPointer, deathQuota: 0);

    var enemies = new RoomEnemySystem(new EnemySession(), definitions: bus, services: new StandaloneRoomEnemyServices(() => 0));
    enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(populationPointer), TestAddressSpace.UnusedGraphicsSet, BgArtworkFixtures.For(new SnesVram()), new SnesCgram()));

    RoomEnemyProjectileSlot eyeShot = enemies.Projectiles.EnemyProjectiles[17];
    eyeShot.Kind = RoomEnemyProjectileKind.EyeDoorProjectile;
    eyeShot.Position = eyeShot.Position.WithWhole(new(0x01b5, 0x0180));
    eyeShot.BlocksSamusProjectiles = true;
    eyeShot.CollisionOption = EnemyProjectileCollisionOption.RunShotList;
    SeedEnemyProjectileDefinitionWord(bus, eyeShot.Kind, 12, 0xb5fb);

    var shots = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None);
    SamusProjectileSlot owner = shots.Slots[0];
    owner.Type = 0x8200;
    owner.Damage = 0x012c;
    owner.Direction = (ushort)SamusProjectileDirection.Right;
    owner.Position = owner.Position.WithWhole(new(0x01b7, 0x0196));
    owner.Cursor = SamusProjectileProgramAddresses.CursorAt(0x9f3b);
    owner.InstructionTimer = WrappingCountdown.Start(15);
    owner.SpritemapPointer = 0xae0f;
    owner.PreInstruction = SamusProjectilePreInstruction.SuperMissile;

    SamusProjectileSlot link = shots.Slots[1];
    link.Type = 0x8200;
    link.Damage = 0x012c;
    link.Direction = (ushort)SamusProjectileDirection.Right;
    link.Position = link.Position.WithWhole(new(0x01af, 0x0196));
    link.Cursor = SamusProjectileProgramAddresses.CursorAt(0x9f7b);
    link.InstructionTimer = WrappingCountdown.Start(1);
    link.SpritemapPointer = 0;
    link.PreInstruction = SamusProjectilePreInstruction.SuperMissileLink;

    int hits = enemies.Projectiles.ResolveEnemyProjectileSamusProjectileHits(
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
    AssertEqual(0, shots.Earthquakes.EarthquakeTimer,
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
internal static void VerifyRipperEnemy(bool verifyDeferredContact = false, bool verifyXrayTimers = false,
    string? nativeXrayTimerTrace = null)
{
    const EnemyDefinitionId definitionPointer = EnemyDefinitionId.Ripper;
    const ushort populationPointer = 0x9580;
    const ushort tilesetPointer = 0x9580;
    const ushort vulnerabilityPointer = 0xedea;
    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var cgram = new SnesCgram();

    WriteEnemyDefinition(bus, definitionPointer, new EnemyDefinitionFields { TileDataSize = 32, PalettePointer = 0xe457, Bank = 0xa2, TileDataAddress = 0xa28000, BossId = 0, NamePointer = 0, FieldSeed = 0x5200 });
    SeedEnemyDefinitionWord(bus, definitionPointer, 4, 200);
    SeedEnemyDefinitionWord(bus, definitionPointer, 6, 5);
    SeedEnemyDefinitionWord(bus, definitionPointer, 8, 8);
    SeedEnemyDefinitionWord(bus, definitionPointer, 10, 4);
    SeedEnemyDefinitionByte(bus, definitionPointer, 13, 0);
    SeedEnemyDefinitionWord(bus, definitionPointer, 14, 0x003e);
    SeedEnemyDefinitionWord(bus, definitionPointer, 18, 0xe49f);
    SeedEnemyDefinitionWord(bus, definitionPointer, 24, 0xe4da);
    SeedEnemyDefinitionWord(bus, definitionPointer, 48, 0x8023);
    SeedEnemyDefinitionWord(bus, definitionPointer, 50, 0x802d);
    SeedEnemyDefinitionWord(bus, definitionPointer, 60, vulnerabilityPointer);

    // Native speed parameter 16 gives +/-1.0000 pixels per frame. Keep that
    // movement fixture without inventing replacement values for the common table.

    // Literal four-frame Ripper lists. The map words are the cartridge addresses; the test
    // need not render their entry payloads to prove list selection and timing.
    ushort[] rightList = [8, 0xe54b, 7, 0xe557, 8, 0xe54b, 7, 0xe563, 0x80ed, 0xe477];
    ushort[] leftList = [8, 0xe527, 7, 0xe533, 8, 0xe527, 7, 0xe53f, 0x80ed, 0xe48b];
    SeedRipperInstructionLists(bus, rightList, leftList);

    // The compiled native Ripper record makes Power Beam and missiles ineffective,
    // freezes on Ice, and gives Super Missiles multiplier two.

    SeedRoomEnemyGraphicsSet(bus, tilesetPointer, new RoomEnemyGraphicsSetHeader(definitionPointer, 0));

    SeedRoomEnemyPopulation(bus, populationPointer, deathQuota: 1,
        new RoomEnemyPopulationRecord(definitionPointer, new(80, 64), 0, EnemyPropertyWord.FromNativeWord(0x2800), default, 16, 1));

    const int roomWidth = 16;
    const int roomHeight = 8;
    ushort[] foreground = new ushort[roomWidth * roomHeight];
    // Column six begins at X=96. Ripper's eight-pixel radius therefore aligns its center to
    // X=88 and reverses when its positive leading edge first enters this wall.
    for (int row = 0; row < roomHeight; row++)
        foreground[row * roomWidth + 6] = 0x8000;
    RoomLevelData level = new RoomLevelData(new(roomWidth, roomHeight), new RoomLevelPlanes(foreground, new byte[foreground.Length], new ushort[foreground.Length]), new byte[8]);

    var paletteDocument = new EnemyPaletteSheetDocument
    {
        Version = 1,
        Colors = Enumerable.Range(0, EnemyPaletteSheet.ColorCount).Select(_ =>
            new PaletteRgb5 { Red = 0, Green = 0, Blue = 0 }).ToArray(),
    };
    using var paletteJson = new MemoryStream(EnemyPaletteSheet.Write(paletteDocument));
    var palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>
    {
        [definitionPointer] = EnemyPaletteSheet.Load(paletteJson),
    };
    // Installed artwork requires complete tiles; one blank tile replaces the old omitted art.
    using var tilePng = new MemoryStream();
    IndexedPng.Write(tilePng, new(8, 8), new byte[64], SnesGraphics.DiagnosticPalette(16));
    tilePng.Position = 0;
    var sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>
    {
        [definitionPointer] = RoomCharacterAtlas.Load(tilePng, 32),
    };
    var enemies = new RoomEnemySystem(new EnemySession(), definitions: bus, services: new StandaloneRoomEnemyServices(() => 0))
    {
        TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = sheets, Palettes = palettes }),
    };
    enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(populationPointer), bus.ReadEnemyGraphicsSet(tilesetPointer), BgArtworkFixtures.For(vram), cgram));
    RoomEnemySlot ripper = enemies.SlotTable.Slots[0];
    if (verifyXrayTimers)
    {
        if (nativeXrayTimerTrace is not null)
        {
            string[] lines = File.ReadAllLines(nativeXrayTimerTrace);
            AssertEqual("intangible,initial,frame,invincibility,flash,frozen,x,instruction", lines[0],
                "original EnemyMain trace schema");
            AssertEqual(121, lines.Length, "all ten native cases and twelve frames");
            int index = 1;
            foreach (bool intangible in new[] { false, true })
            foreach (ushort initial in new ushort[] { 0, 1, 2, 10, ushort.MaxValue })
            {
                ripper.Properties = EnemyPropertyWord.FromNativeWord((ushort)(EnemyProperties.Invisible | EnemyProperties.ProcessOffScreen |
                    (intangible ? EnemyProperties.IgnoreSamusCollision : default(EnemyProperties))));
                ripper.InvincibilityTimer = WrappingCountdown.FromRaw(initial);
                ripper.FlashTimer = WrappingCountdown.Start(16);
                ripper.FrozenTimer = WrappingCountdown.Start(400);
                for (int frame = 1; frame <= 12; frame++)
                {
                    enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = true, Level = level });
                    string actual = $"{(intangible ? 1 : 0)},{initial},{frame},{ripper.InvincibilityTimer.RawValue}," +
                        $"{ripper.FlashTimer.RawValue},{ripper.FrozenTimer.RawValue},{ripper.Position.X.Whole},{ripper.CurrentInstruction}";
                    AssertEqual(lines[index++], actual, "original CPU versus production frozen EnemyMain");
                }
            }
            Console.WriteLine("X-Plasma: 120 original-CPU EnemyMain frame records match production exactly.");
            return;
        }
        ripper.InvincibilityTimer = WrappingCountdown.Start(10);
        ripper.FlashTimer = WrappingCountdown.Start(16);
        ripper.FrozenTimer = WrappingCountdown.Start(400);
        ushort initialX = ripper.Position.X.Whole;
        int initialInstruction = ripper.CurrentInstruction;
        for (int frame = 1; frame <= 12; frame++)
        {
            enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = true, Level = level });
            AssertEqual(Math.Max(0, 10 - frame), ripper.InvincibilityTimer.RawValue,
                $"X-Plasma prerequisite: tangible invincibility expires during X-ray frame {frame}");
            AssertEqual(16, ripper.FlashTimer.RawValue, "X-ray holds flash timer independently");
            AssertEqual(400, ripper.FrozenTimer.RawValue, "X-ray holds Ice freeze timer independently");
            AssertEqual(initialX, ripper.Position.X.Whole, "X-ray holds enemy movement");
            AssertEqual(initialInstruction, ripper.CurrentInstruction, "X-ray holds enemy instruction pointer");
        }
        ripper.Properties = ripper.Properties.With(EnemyProperties.IgnoreSamusCollision);
        ripper.InvincibilityTimer = WrappingCountdown.Start(10);
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = true, Level = level });
        AssertEqual(10, ripper.InvincibilityTimer.RawValue, "intangible actor bypasses native invincibility clock");
        ripper.Properties = ripper.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        ripper.InvincibilityTimer = WrappingCountdown.Start(1);
        ripper.FrozenTimer = WrappingCountdown.Start(0);
        ripper.FlashTimer = WrappingCountdown.Start(0);
        var touchingSamus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Health = 99,
            Position = FixedPosition<RoomPixels>.FromWhole(ripper.Position.Whole),
        };
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = touchingSamus, Level = level, ResolveSamusContactBeforeAi = true });
        AssertEqual(0, ripper.InvincibilityTimer.RawValue, "ordinary frame expires the last invincibility tick");
        AssertEqual(99, touchingSamus.Health, "expiration frame still skips native collision branch");
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = touchingSamus, Level = level, ResolveSamusContactBeforeAi = true });
        AssertEqual(94, touchingSamus.Health, "following frame resumes body contact damage");
        Console.WriteLine("X-Plasma timer prerequisite: invincibility expires while movement/flash/Ice remain frozen.");
        return;
    }
    AssertEqual(AnimationProgramCursor<RipperProgram>.Start(RipperProgram.RipperMovingRight),
        enemies.RipperBehavior.Extensions[ripper.SlotIndex].Program, "Ripper init right animation");
    RipperEnemyState ripperState = ripper.SpeciesState<RipperEnemyState>()
        ?? throw new InvalidOperationException("The initialized Ripper has no state.");
    AssertEqual(1, ripperState.XVelocity.WholeWord, "Ripper init signed whole X velocity");
    AssertEqual(0, ripperState.XVelocity.Fraction, "Ripper init X subvelocity");

    for (int frame = 0; frame < 9; frame++)
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Level = level });
    AssertEqual(88, ripper.Position.X.Whole, "Ripper wall-aligned reversal X");
    AssertEqual(0xffff, ripperState.XVelocity.WholeWord, "Ripper reversal signed whole X velocity");
    AssertEqual(0xe527, ripper.SpritemapPointer, "Ripper reversal selects left-facing frame");

    // WriteEnemyOAM `$A0:947B` applies enemy shake to the shared ordinary-spritemap
    // origin. One synthetic one-entry map makes both signs and the timer consumption
    // observable without borrowing the production displacement calculation.
    var frames = EnemySpritemapDefinitions.Frames.ToArray();
    var spriteDocument = new EnemySpritemapDocument
    {
        Version = (int)EnemySpritemapSchema.Current,
        Frames = frames.ToDictionary(frame => frame.Name, frame =>
            frame.Bank == 0xa2 && frame.Pointer == 0xe527
                ? new[] { new SpriteVisualPart { OffsetX = 0, OffsetY = 0, Size = 8,
                    TileColumn = 0, TileRow = 0, Palette = 0, Priority = 0,
                    FlipX = false, FlipY = false } }
                : Array.Empty<SpriteVisualPart>()),
        DisplayFrames = frames.ToDictionary(frame => frame.Name, frame => frame.Name),
    };
    using var spriteJson = new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
        spriteDocument, CamelCaseJson));
    enemies.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = sheets, Palettes = palettes, Spritemaps = EnemySpritemapDocuments.Load(spriteJson) });
    var shakenOam = new OamBuffer();
    ripper.FrameCounter = 0;
    ripper.ShakeTimer = WrappingCountdown.Start(2);
    shakenOam.BeginFrame();
    enemies.Drawing.DrawLayersAsOam(shakenOam, new(0, 0), 0, 7);
    shakenOam.FinalizeFrame();
    AssertEqual(89, shakenOam.GetEntry(0).Position.X,
        "enemy shake frame-counter bit one clear adds one X pixel");
    AssertEqual(1, ripper.ShakeTimer.RawValue, "enemy draw consumes one shake tick");

    ripper.FrameCounter = 2;
    ripper.ShakeTimer = WrappingCountdown.Start(2);
    shakenOam.BeginFrame();
    enemies.Drawing.DrawLayersAsOam(shakenOam, new(0, 0), 0, 7);
    shakenOam.FinalizeFrame();
    AssertEqual(87, shakenOam.GetEntry(0).Position.X,
        "enemy shake frame-counter bit one set subtracts one X pixel");
    AssertEqual(1, ripper.ShakeTimer.RawValue, "negative enemy draw consumes one shake tick");

    var samus = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Health = 99,
        Position = FixedPosition<RoomPixels>.FromWhole(ripper.Position.Whole),
    };
    AssertTrue(enemies.Collision.ResolveOrdinarySamusContact(samus, controllerInput: 0),
        "Ripper radius contact reaches common touch AI");
    AssertEqual(94, samus.Health, "Ripper header contact damage");
    AssertEqual(0x60, samus.InvincibilityTimer.RawValue, "Ripper contact invincibility clock");
    AssertEqual(5, samus.KnockbackTimer.RawValue, "Ripper contact knockback clock");

    if (verifyDeferredContact)
        Console.WriteLine($"CONTACT health={samus.Health} pose={(int)samus.Pose:X2} timer={samus.KnockbackTimer.RawValue} invincibility={samus.InvincibilityTimer.RawValue} direction={samus.KnockbackDirection} active={samus.KnockbackActive}");
    AssertEqual(SamusPoseId.FacingRightNormalPose, samus.Pose,
            "touch callback publishes request without changing the pre-movement pose");
    AssertTrue(!samus.KnockbackActive && samus.KnockbackDirection == 0,
            "touch callback does not install the later hurt movement handler");
    AssertEqual(KnockbackHorizontalDirection.Right, samus.KnockbackXDirection, "contact publishes source-side word for later interruption");
    if (verifyDeferredContact) return;

    var projectiles = CreateProjectileFixture();
    var sharedProjectiles = CreateBombFixture();
    SeedRipperProjectileDataPointers(bus);

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
        projectile.Position = projectile.Position.WithWhole(target.Position.Whole);
        projectile.Radius = new(4, 4);
        projectile.Cursor = ProjectileFixtureCursors.Active;
        projectile.InstructionTimer = WrappingCountdown.Start(1);
    }

    SamusProjectileSlot shot = projectiles.Slots[0];
    ArmProjectile(shot, ripper, type: 0x8000, damage: 20);
    AssertEqual(1, enemies.Collision.ResolveOrdinaryProjectileHits(projectiles, sharedProjectiles),
        "Ripper ineffective power-beam collision");
    AssertEqual(200, ripper.Health, "Ripper power-beam immunity comes from ROM vulnerability");

    ArmProjectile(shot, ripper, type: 0x8002, damage: 20);
    AssertEqual(1, enemies.Collision.ResolveOrdinaryProjectileHits(projectiles, sharedProjectiles),
        "Ripper Ice Beam collision");
    AssertEqual(400, ripper.FrozenTimer.RawValue, "Ripper Ice vulnerability freezes actor");
    ushort frozenX = ripper.Position.X.Whole;
    enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Level = level });
    AssertEqual(frozenX, ripper.Position.X.Whole, "frozen Ripper suppresses movement");
    AssertEqual(399, ripper.FrozenTimer.RawValue, "frozen Ripper timer advances");

    // Clear the focused freeze fixture and prove the Super Missile multiplier, hurt flash, and
    // common death/deletion accounting on the same loaded actor.
    ripper.FrozenTimer = WrappingCountdown.Start(0);
    ripper.InvincibilityTimer = WrappingCountdown.Start(0);
    ripper.AiStates = EnemyAiStates.None;
    byte expectedHurtCry = unchecked((byte)ripper.Definition.HurtSoundEffect);
    int criesBefore = enemies.Audio.SoundRequests.Count(request =>
        request.SoundEffect.Library == SoundEffectLibrary.Library2 &&
        request.SoundEffect.Value == expectedHurtCry && request.MaximumQueued == 3);
    for (int hit = 0; hit < 2; hit++)
    {
        ArmProjectile(shot, ripper, type: 0x0200, damage: 100);
        AssertEqual(1, enemies.Collision.ResolveOrdinaryProjectileHits(projectiles, sharedProjectiles),
            $"Ripper Super Missile hit {hit + 1}");
        AssertEqual(criesBefore + hit + 1, enemies.Audio.SoundRequests.Count(request =>
            request.SoundEffect.Library == SoundEffectLibrary.Library2 &&
            request.SoundEffect.Value == expectedHurtCry && request.MaximumQueued == 3),
            "common shot queues header low-byte cry for surviving and lethal hits");
    }
    AssertEqual(0, ripper.Health, "Ripper Super Missile vulnerability reaches zero health");
    AssertEqual(EnemyDefinitionId.None, ripper.DefinitionId,
        "Ripper generic death clears its common enemy record immediately");
    RoomEnemyProjectileSlot ripperDeath = enemies.Projectiles.EnemyProjectiles[17];
    AssertEqual(RoomEnemyProjectileKind.EnemyDeathExplosion, ripperDeath.Kind,
        "Ripper zero health allocates the shared F345 death actor");
    AssertEqual(definitionPointer, ripperDeath.DropDefinitionId,
        "Ripper death actor retains its header for EEAF drop selection");
    AssertEqual(1, enemies.RoomStatus.EnemiesKilled, "Ripper death increments room kill count");

    Console.WriteLine(
        "  Ripper: ROM load, animation, 16.16 movement, wall reversal, contact, " +
        "vulnerabilities, freeze, damage, and death agree.");
}

/// <summary>Extracts fixture Ceres-door tiles and colors into an installed Ceres-door visual catalog.</summary>
private static CeresDoorVisualCatalog CreateCeresDoorFixtureVisuals(TestAddressSpace bus)
{
    using var directoryScratch = new TestScratchDirectory("ceres-door-fixture");
    string directory = directoryScratch.Root;
    SuperMetroid.AssetExtraction.Areas.Ceres.CeresDoorVisualFiles.Extract(bus, directory);
    var visuals = SuperMetroid.AssetExtraction.Areas.Ceres.CeresDoorVisualFiles.Load(
        File.ReadAllBytes(Path.Combine(directory, CeresDoorVisualFormat.TilesFileName)),
        File.ReadAllBytes(Path.Combine(directory, CeresDoorVisualFormat.ColorsFileName)));
    return visuals;
}

/// <summary>
/// Proves that the stationary Ceres elevator platform is animated by the room's variant-two
/// door actor, not by either arrival projectile that is deleted when the platform lands.
/// </summary>
[PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
internal static void VerifyCeresElevatorPlatformAnimation()
{
    const EnemyDefinitionId definitionPointer = EnemyDefinitionId.CeresDoor;
    const ushort populationPointer = 0x9480;
    const ushort tilesetPointer = 0x9480;
    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var cgram = new SnesCgram();

    WriteEnemyDefinition(bus, definitionPointer, new EnemyDefinitionFields { TileDataSize = 0, PalettePointer = 0xf4ec, Bank = 0xa6, TileDataAddress = 0xa68000, BossId = 0, NamePointer = 0, FieldSeed = 0x4300 });
    SeedEnemyDefinitionWord(bus, definitionPointer, 18, 0xf6c5);
    SeedEnemyDefinitionWord(bus, definitionPointer, 24, 0xf765);
    SeedEnemyDefinitionByte(bus, definitionPointer, 57, 2);

    // Parameter one equals two, selecting main function $F850. This focused population
    // omits ProcessInstructions because only the independently executing main AI owns the
    // persistent platform transfer.
    SeedRoomEnemyPopulation(bus, populationPointer, deathQuota: 0,
        new RoomEnemyPopulationRecord(definitionPointer, new(0x0080, 0x0080), 0, EnemyPropertyWord.FromNativeWord(0x0800), default, 2, 0));
    SeedRoomEnemyGraphicsSet(bus, tilesetPointer);
    byte[] light = [0x68, 0x69, 0x69, 0x78];
    byte[] dark = [0x8d, 0x8e, 0x8e, 0x79];
    SeedCeresElevatorPlatformTransfers(bus, light, dark);

    // The platform frames are written into the room's Mode 7 map (#1349).
    var map = new Mode7Map(new byte[Mode7Map.Cells], default);
    var enemies = new RoomEnemySystem(new EnemySession(), definitions: bus, services: new StandaloneRoomEnemyServices(() => 0) { Mode7Map = map }) { TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), AreaArtwork = new AreaArtworkCatalog { CeresDoorVisual = CreateCeresDoorFixtureVisuals(bus) } };
    enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(populationPointer), bus.ReadEnemyGraphicsSet(tilesetPointer), BgArtworkFixtures.For(vram), cgram));
    enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false });
    for (int index = 0; index < 4; index++)
        AssertEqual(light[index], map.Publish().Cells[0x060e + index],
            "Ceres elevator landed platform light map frame");

    enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false });
    enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false });
    for (int index = 0; index < 4; index++)
        AssertEqual(dark[index], map.Publish().Cells[0x060e + index],
            "Ceres elevator landed platform dark map frame");

    Console.WriteLine("  Ceres elevator: landed Mode-7 platform continues its ROM transfer animation.");
}

/// <summary>
/// Verifies the area-boss branch in Ridley's physical door actor. This is separate from the
/// type-$9 room cap: the enemy is solid during the fight and its ROM instruction stream must
/// make it intangible after `$A6:C117` sets Ceres's area-boss bit.
/// </summary>
internal static void VerifyCeresDoorBossBranch()
{
    const EnemyDefinitionId definitionPointer = EnemyDefinitionId.CeresDoor;
    const ushort populationPointer = 0x94c0;
    const ushort tilesetPointer = 0x94c0;
    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var cgram = new SnesCgram();

    WriteEnemyDefinition(bus, definitionPointer, new EnemyDefinitionFields { TileDataSize = 0, PalettePointer = 0xf4ec, Bank = 0xa6, TileDataAddress = 0xa68000, BossId = 0, NamePointer = 0, FieldSeed = 0x4380 });
    SeedEnemyDefinitionWord(bus, definitionPointer, 18, 0xf6c5);
    SeedEnemyDefinitionWord(bus, definitionPointer, 24, 0xf765);
    SeedEnemyDefinitionByte(bus, definitionPointer, 57, 2);

    // Property $8000 makes the focused actor solid, $2000 runs its instruction list,
    // and $0800 keeps that list processing even when the synthetic actor is offscreen.
    SeedRoomEnemyPopulation(bus, populationPointer, deathQuota: 0,
        new RoomEnemyPopulationRecord(definitionPointer, new(0x0008, 0x007f), 0, EnemyPropertyWord.FromNativeWord(0xA800), default, 3, 0));
    SeedRoomEnemyGraphicsSet(bus, tilesetPointer);

    // Start directly at the retail Ridley-room closed-door loop. Its mechanics deliberately
    // are absent from this synthetic bus: the compiled catalog must return `$F66A`, which
    // loops at `$F55E` while the boss lives; after defeat it clears the Ridley-drawn flag
    // and enters the ordinary closed-door loop at `$F598`. A nearby Samus then drives the
    // native opening frames to `$F68B`, whose intangible side effect is under test. Frame
    // selectors use compiled mechanics; this fixture does not draw the door sprite.

    bool areaBossDefeated = false;
    int areaBossReads = 0;
    var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0008, 0x007f)) };
    var enemies = new RoomEnemySystem(new EnemySession(), definitions: bus, services: new StandaloneRoomEnemyServices(() => 0) { Bosses = new FixtureAreaBossFlags { IsBossDefeated = () =>
        {
            areaBossReads++;
            return areaBossDefeated;
        } } }) { TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), AreaArtwork = new AreaArtworkCatalog { CeresDoorVisual = CreateCeresDoorFixtureVisuals(bus) } };
    enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(populationPointer), bus.ReadEnemyGraphicsSet(tilesetPointer), BgArtworkFixtures.For(vram), cgram) { Samus = samus });
    RoomEnemySlot door = enemies.SlotTable.Slots[0];
    door.CurrentInstruction = 0xf55e;
    door.InstructionTimer = WrappingCountdown.Start(1);

    for (int frame = 0; frame < 4; frame++)
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
    AssertTrue(!door.Properties.HasAny(EnemyProperties.IgnoreSamusCollision),
        "living Ceres boss keeps Ridley-room door tangible");

    areaBossDefeated = true;
    // The boss bit can change while the two-frame closed map is already sleeping. Allow
    // that current record to expire, then require the very next `$F66A` visit to leave the
    // loop; the generous bound is diagnostic and does not alter actor state.
    for (int frame = 0;
        frame < 40 && !door.Properties.HasAny(EnemyProperties.IgnoreSamusCollision);
        frame++)
    {
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
    }
    AssertTrue(door.Properties.HasAny(EnemyProperties.IgnoreSamusCollision),
        $"defeated Ceres boss advances door bytecode to intangible setup " +
        $"(PC=${door.CurrentInstruction:X4}, timer=${door.InstructionTimer.RawValue:X4}, " +
        $"properties=${door.Properties.ToNativeWord():X4}, bossReads={areaBossReads})");
    Console.WriteLine("  Ceres door: area-boss branch opens the physical Ridley-room actor.");
}

/// <summary>Reads a native seven-byte Ceres escape VRAM transfer list (size, 24-bit source, destination) up to its zero terminator.</summary>
[CpuBusBoundary("exact bus test: retail ROM oracle for the Ceres escape VRAM transfer lists in bank $A6")]
static List<VramWriteEntry> ReadRetailCeresEscapeTransfers(ISnesAddressSpace rom, int pointer)
{
    var records = new List<VramWriteEntry>();
    for (int record = 0; record < 32; record++, pointer += 7)
    {
        ushort size = CartridgeImportSource.Require(rom).ReadCartridgeWord(pointer);
        if (size == 0) return records;
        records.Add(new VramWriteEntry(size,
            CartridgeImportSource.Require(rom).ReadCartridgeLong((pointer + 2)),
            VramDmaDestination.FromNative(CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + 5)))));
    }
    throw new InvalidDataException("Native Ceres transfer fixture lacks a terminator.");
}

/// <summary>
/// Regresses the exact enemy $E13F failure reported when the second normal Ceres door loads
/// room $E0B5. This fixture uses native $A0/$A1/$A6/$B4 layouts and follows the translated
/// dispatcher through the complete initial delay, eye fade, and body fade.
/// </summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
internal static void VerifyCeresRidleyRoomEntry()
{
    const EnemyDefinitionId definitionPointer = EnemyDefinitionId.RidleyCeres;
    const ushort populationPointer = 0x9500;
    const ushort tilesetPointer = 0x9500;

    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var cgram = new SnesCgram();

    SeedCeresRidleyRoomEntryCartridge(bus, definitionPointer, populationPointer, tilesetPointer);
    const EnemyDefinitionId ceresDoorDefinitionPointer = EnemyDefinitionId.CeresDoor;

    (ushort[] fixturePaletteWords, byte[] fixturePlanar) = ReadCeresRidleyFixtureArtwork(bus);
    var fixturePalette = new PaletteRgb5[EnemyPaletteSheet.ColorCount];
    for (int color = 0; color < fixturePalette.Length; color++)
    {
        ushort word = fixturePaletteWords[color];
        fixturePalette[color] = new PaletteRgb5
        {
            Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31,
        };
    }
    byte[] fixturePixels = SnesGraphics.DecodePlanarTiles(fixturePlanar, 4, 1,
        out Extent16<ImagePixels> fixtureSize);
    using var fixtureTilePng = new MemoryStream();
    IndexedPng.Write(fixtureTilePng, fixtureSize, fixturePixels,
        SnesGraphics.DiagnosticPalette(16));
    var fixtureSpriteFrames = EnemySpritemapDefinitions.Frames.ToArray();
    var fixtureSprites = new EnemySpritemapDocument
    {
        Version = (int)EnemySpritemapSchema.Current,
        Frames = fixtureSpriteFrames.ToDictionary(frame => frame.Name, frame =>
            frame.Bank == 0xa6 && frame.Pointer == 0xa329
                ? new[] { new SpriteVisualPart { OffsetX = 0, OffsetY = 0, Size = 8,
                    TileColumn = 0, TileRow = 0, Palette = 0, Priority = 0,
                    FlipX = false, FlipY = false } }
                : Array.Empty<SpriteVisualPart>()),
        DisplayFrames = fixtureSpriteFrames.ToDictionary(frame => frame.Name, frame => frame.Name),
    };
    using var fixtureSpriteJson = new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
        fixtureSprites, CamelCaseJson));
    bool ceresBossDefeated = false;
    ushort generatedRandom = 0x1234;
    ushort currentRandom = 0x1234;
    int generatedRandomCalls = 0;
    // The escape writes its warning text over BG1 (#1346).
    RoomTilemapOverlay ceresOverlay = RoomTilemapOverlay.Empty;
    var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(() => { generatedRandomCalls++; return generatedRandom; }) { ReadRandom = () => currentRandom, Bosses = new FixtureAreaBossFlags { IsBossDefeated = () => ceresBossDefeated, SetBossDefeated = () => ceresBossDefeated = true }, DrawBg1OverlayWords = (firstWord, words) => ceresOverlay = ceresOverlay.With(firstWord, words), Mode7Map = new Mode7Map(new byte[Mode7Map.Cells], default) })
    {
        TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>
            {
                [definitionPointer] = RoomCharacterAtlas.Load(
                    new MemoryStream(fixtureTilePng.ToArray()), 0x20),
            }, Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>
            {
                [definitionPointer] = EnemyPaletteSheet.Load(new MemoryStream(
                    EnemyPaletteSheet.Write(new EnemyPaletteSheetDocument { Version = 1, Colors = fixturePalette }))),
            }, Spritemaps = EnemySpritemapDocuments.Load(fixtureSpriteJson) }), AreaArtwork = new AreaArtworkCatalog { CeresDoorVisual = CreateCeresDoorFixtureVisuals(bus), CeresEscapeTiles = InstalledFixtures.AreaArtwork.CeresEscapeTiles, CeresEscapeOverlayTilemaps = InstalledFixtures.AreaArtwork.CeresEscapeOverlayTilemaps },
        BossArtworkBinding =
        {
            CeresRidleyColors = CeresRidleyColorCatalog.Load(new MemoryStream(
                SuperMetroid.AssetExtraction.Bosses.Ridley.CeresRidleyColorExtractor.Extract(bus))),
            CeresRidleyMode7Colors = CeresRidleyMode7ColorCatalog.Load(new MemoryStream(
                SuperMetroid.AssetExtraction.Bosses.Ridley.CeresRidleyMode7ColorExtractor.Extract(bus))),
        },
    };
    using (var escapeText = new MemoryStream(
        SuperMetroid.AssetExtraction.Areas.Escape.EscapeTypewriterExtractor.Extract(bus), writable: false))
        enemies.BossArtworkBinding.EscapeTypewriterPresentation = EscapeTypewriterPresentation.Load(escapeText);
    ClearCeresEscapeTypewriterSource(bus);
    enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(populationPointer), bus.ReadEnemyGraphicsSet(tilesetPointer), BgArtworkFixtures.For(vram), cgram));

    RoomEnemySlot ridley = enemies.SlotTable.Slots[0];
    RidleyEnemyState state = enemies.RidleyEncounter.CeresRidley
        ?? throw new InvalidOperationException("Ceres Ridley did not allocate its extended state.");
    AssertEqual(0x00ba, ridley.Position.X.Whole, "Ceres Ridley init X");
    AssertEqual(0x00a9, ridley.Position.Y.Whole, "Ceres Ridley init Y overrides population Y");
    AssertEqual(AnimationProgramCursor<RidleyBodyProgram>.Start(RidleyBodyProgram.Initial), state.BodyProgram, "Ceres Ridley starts its initial program");
    AssertEqual(0x0e00, ridley.PaletteIndex, "Ceres Ridley dedicated OBJ palette");
    AssertEqual(0x3c00, ridley.Properties.ToNativeWord(), "Ceres Ridley native initialized properties");
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
    SeedCeresDoorQuakeOverlay(bus);
    RoomEnemySlot overlayDoor = enemies.SlotTable.Slots[1];
    overlayDoor.Bind(ceresDoorDefinitionPointer, overlayDoor.Definition);
    var overlayDoorState = new CeresDoorEnemyState { HandsDrawingToRidley = true };
    overlayDoor.SetSpeciesState(overlayDoorState, overlayDoorState.ClearWithRecord);
    overlayDoor.Position = overlayDoor.Position.WithWhole(new(100, 80));
    enemies.RidleyEncounter.CeresStatus = CeresEscapeStatus.RidleyEscaped; // Skip the unrelated private Baby list in this draw fixture.
    enemies.Room.Session.Mode7QueueResidue = new SuperMetroid.Core.Hardware.Mode7TransferQueueResidue();
    short[] expectedDoorOffsets = [0, 0, -4, -1];
    for (ushort timer = 0; timer < expectedDoorOffsets.Length; timer++)
    {
        enemies.Earthquake.EarthquakeTimer = WrappingCountdown.FromRaw(timer);
        var doorOam = new OamBuffer();
        doorOam.BeginFrame();
        doorOam.AppendDrawn(frame => enemies.CeresRidleyBehavior.BabyScene.DrawCeresRidleyImmediateBabyAndDoor(frame, EnemyFormatDrawAccess.BlankObjectPage, new(0, 0)));
        doorOam.FinalizeFrame();
        AssertEqual(100 + expectedDoorOffsets[timer], doorOam.GetEntry(0).Position.X,
            $"Ceres private door hook byte-indexed quake phase {timer}");
    }
    overlayDoor.Bind(0, overlayDoor.Definition);
    enemies.RidleyEncounter.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;
    enemies.Earthquake.EarthquakeTimer = WrappingCountdown.Start(0);

    enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false });
    AssertEqual((ushort)RidleyAiFunction.InitialDelay, (ushort)state.Function,
        "Ceres Ridley door-clear transition");
    AssertEqual(511, state.FunctionTimer.RawValue, "Ceres Ridley first delay decrement");
    var nativeRidleyRom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc"));
    ushort initialRidleyMap = CartridgeImportSource.Require(nativeRidleyRom).ReadCartridgeWord(0xa6e53e);
    AssertEqual(initialRidleyMap, ridley.SpritemapPointer, "Ceres Ridley left-facing initial map");
    AssertEqual(0xe540, new SuperMetroid.Verification.Bosses.Ridley.RidleyNativeInstructionPointers(
        CartridgeImportSource.Require(nativeRidleyRom)).Body(state), "Ceres Ridley initial list sleeps");

    for (int frame = 0; frame < 512; frame++)
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false });
    AssertEqual((ushort)RidleyAiFunction.FadeInEyes, (ushort)state.Function,
        "Ceres Ridley 512-counter underflow starts eye fade");

    for (int frame = 0; frame < 65; frame++)
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false });
    AssertEqual((ushort)RidleyAiFunction.FadeInBody, (ushort)state.Function,
        "Ceres Ridley eye table terminator starts body fade");
    AssertTrue(state.MovementAnimationEnabled,
        "Ceres Ridley eye fade enables composite animation");
    AssertEqual(RidleyTailFunction.None, state.TailFunctionIndex,
        "Ceres Ridley resting tail has not started its liftoff motion");
    // Native evidence (100% movie, source frame 3645): once the eye fade enables
    // composite animation, all seven inactive segments share the tail-root position
    // and stay there through the body fade until liftoff.
    AssertEqual(1, state.TailSegments
            .Select(segment => new MotherBrainRinkaDoorSpawnExpected(segment.Position.X, segment.Position.Y))
            .Distinct()
            .Count(),
        "Ceres Ridley resting tail segments share the tail root before liftoff");

    for (int frame = 0; frame < 32; frame++)
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false });
    AssertEqual((ushort)RidleyAiFunction.WaitBeforeRoar, (ushort)state.Function,
        "Ceres Ridley body fade completes");
    AssertEqual(4, state.FunctionTimer.RawValue, "Ceres Ridley pre-roar timer");
    AssertEqual(0x5000 + 0x14a / 2, cgram.Colors[0x91],
        "Ceres Ridley final body-fade row reaches OBJ palette one");
    AssertTrue(!ridley.Properties.HasAny(EnemyProperties.IgnoreSamusCollision),
        "Ceres Ridley becomes tangible after body fade");
    AssertTrue(enemies.Audio.MusicRequests.Contains(new DelayedMusicRequest(
            MusicCommand.SelectTrack(5),
            MusicCommandDelay.EightFrames)),
        "Ceres Ridley final body fade queues battle/escape track five");

    // Finish the 5-frame pre-roar and 253-frame pre-liftoff countdowns, then let the real
    // E969 instruction arm A6AF. Movement uses the translated common 8.8 integrator until
    // A6C8 crosses Y=$50 and publishes fight mode one.
    var samus = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Health = 99,
    };
    int battleEntryFrames = 0;
    bool observedRoarSound = false;
    while (state.Function != RidleyAiFunction.CeresHovering && battleEntryFrames < 1024)
    {
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
        observedRoarSound |= enemies.Audio.SoundRequests.Contains(
            new SoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x59), MaximumQueued: 6));
        battleEntryFrames++;
    }
    AssertEqual((ushort)RidleyAiFunction.CeresHovering, (ushort)state.Function,
        "Ceres Ridley liftoff reaches hover dispatcher");
    AssertEqual(RidleyFightMode.Fighting, state.FightMode, "Ceres Ridley liftoff enables battle mode");
    AssertTrue(ridley.Position.Y.Whole < 80,
        "Ceres Ridley crosses the cartridge's Y=$50 battle threshold");
    AssertTrue(observedRoarSound,
        "Ridley instruction $E4BE publishes QueueSfx2_Max6($59)");

    // #1275: $A6:A72D selects the next attack from RandomNumberSeed as it stands; it does
    // not generate a number. Fireballs (low nibble 9) and swoop (low nibble E) tell the two apart.
    currentRandom = 0x4529;
    generatedRandom = 0x5ade;
    int hoverFrames = 0;
    while (state.Function == RidleyAiFunction.CeresHovering && hoverFrames < 256)
    {
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
        hoverFrames++;
    }
    AssertEqual((ushort)RidleyAiFunction.CeresFireballMoveToPosition, (ushort)state.Function,
        "Ceres Ridley hover chooses its attack from the current random number");
    // $A6:A7F9 jitters the fireball hover target from the same seed, again without generating.
    int callsBeforeFireballs = generatedRandomCalls;
    int fireballFrames = 0;
    while (state.Function == RidleyAiFunction.CeresFireballMoveToPosition && fireballFrames < 256)
    {
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
        fireballFrames++;
    }
    AssertEqual((ushort)RidleyAiFunction.CeresFireballShooting, (ushort)state.Function,
        "Ceres Ridley reaches its fireball hover");
    for (int frame = 0; frame < 8; frame++)
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
    AssertEqual(callsBeforeFireballs, generatedRandomCalls,
        "Ceres Ridley fireball hover reads the random number without generating one");
    // #1275: at low energy the fireballing instruction list's $A6:E4D2 stores 8 into the
    // shared $7E:7800 timer when the list reaches it, ending the 224-frame fireball hover early.
    int savedHealth = samus.Health;
    samus.Health = 20;
    int lowEnergyFrames = 0;
    while (state.Function == RidleyAiFunction.CeresFireballShooting && lowEnergyFrames < 224)
    {
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
        lowEnergyFrames++;
    }
    AssertEqual((ushort)RidleyAiFunction.CeresHovering, (ushort)state.Function,
        "Ceres Ridley low-energy fireball branch returns to hovering");
    AssertTrue(lowEnergyFrames < 120,
        $"Ceres Ridley low-energy branch shortens the fireball hover (took {lowEnergyFrames} frames)");
    samus.Health = savedHealth;
    currentRandom = generatedRandom = 0x1234;

    // Ceres lunge retains neutral-tail AI. Once Ridley closes within 128 pixels,
    // `$A6:CC9A-$CCB9` aims a real seven-segment whip at Samus instead of merely moving
    // the body/wing composite. Start from the cartridge's all-active steady state so this
    // assertion isolates the missing request/target side effects reported in gameplay.
    foreach (RidleyTailSegment segment in state.TailSegments)
    {
        segment.Active = true;
        segment.StaggerAngle = 0xffff;
        segment.Clockwise = true;
        segment.TargetDistance = 0;
    }
    state.TailSegments[0].Angle = 0x4000;
    state.ClockwiseWhipTarget = null;
    state.CounterClockwiseWhipTarget = null;
    state.Function = RidleyAiFunction.CeresLungeSetup;
    samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(ridley.Position.X.Whole - 64)), unchecked((ushort)(ridley.Position.Y.Whole + 68))));
    enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
    AssertEqual((ushort)RidleyAiFunction.CeresLungeMain, (ushort)state.Function,
        "Ceres Ridley enters lunge main");
    AssertTrue(state.ClockwiseWhipTarget is not null,
        "Ceres Ridley lunge installs cartridge tail-whip target");
    AssertEqual(8, state.TailAngleDelta,
        "Ceres Ridley aimed whip uses eight-angle step");
    AssertTrue(state.TailSegments.Skip(1).Any(segment => segment.TargetDistance == 0x0c00),
        "Ceres Ridley lunge extends articulated tail segments");

    const int roomWidth = 32;
    const int roomHeight = 16;
    RoomLevelData air = new RoomLevelData(new(roomWidth, roomHeight), new RoomLevelPlanes(new ushort[roomWidth * roomHeight], new byte[roomWidth * roomHeight], new ushort[roomWidth * roomHeight]), new byte[8]);
    var sharedProjectiles = CreateBombFixture();
    var projectiles = CreateProjectileFixture();

    bool observedNormalRidleyPalette = false;
    bool observedFlashRidleyPalette = false;
    int nativeExplosionFrames = NativeProjectileLifetime(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")), 0x93867b) + 1;
    SeedUnchargedNoWavePreInstruction(bus);
    for (int hit = 0; hit < 100; hit++)
    {
        // Hold the independently read native initial frame for each isolated shot.
        // Its body component spans X=0..28, beyond the header radius of eight.
        ridley.SpritemapPointer = initialRidleyMap;
        // Place Samus relative to the active component, then retain native muzzle offset
        // and first-frame motion. The public producer exercises allocation/type/radii
        // before this focused extended-hitbox/shot-AI test resolves the impact.
        samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(ridley.Position.X.Whole + 8)), ridley.Position.Y.Whole));
        sharedProjectiles.StepFrame(new SamusTerrain { Level = air }, samus, 0, 0);
        SamusProjectileSlotObservation slotsBeforeFire = projectiles.ObserveSlots();
        projectiles.StepFrame(new SamusTerrain { Level = air }, samus, new ProjectileButtonInput { ControllerInput = SnesButton.X, ControllerNewInput = SnesButton.X }, new(0, 0), sharedProjectiles);
        AssertEqual(0, slotsBeforeFire.FiredSlot(projectiles),
            $"Ceres Ridley hit {hit + 1} allocates the power-beam slot");
        AssertTrue(projectiles.Slots[0].Position.X.Whole - projectiles.Slots[0].Radius.X > ridley.Position.X.Whole + 8,
            $"Ceres Ridley hit {hit + 1} is outside the header collision radius");
        AssertEqual(1, enemies.CeresRidleyBehavior.Hits.ResolveCeresRidleyProjectileHits(
            projectiles, sharedProjectiles),
            $"Ceres Ridley hit {hit + 1} reaches enemy shot AI");
        observedNormalRidleyPalette |= state.CommonDrawPaletteIndex.PaletteBits == 0x0e00;
        observedFlashRidleyPalette |= state.CommonDrawPaletteIndex.PaletteBits == 0;
        if (hit == 49)
            AssertEqual(0x7000, cgram.Colors[0xf1],
                "Ceres Ridley 50-hit health palette");
        if (hit == 69)
            AssertEqual(0x7200, cgram.Colors[0xf1],
                "Ceres Ridley 70-hit missing-branch palette quirk");

        // The hit publishes an unconsumed first frame. Run its full native lifetime
        // plus initialization, returning the same slot without shortening the program.
        for (int explosionFrame = 0; explosionFrame < nativeExplosionFrames; explosionFrame++)
        {
            sharedProjectiles.StepFrame(new SamusTerrain { Level = air }, samus, 0, 0);
            projectiles.StepFrame(new SamusTerrain { Level = air }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0 }, new(0, 0), sharedProjectiles);
        }

        // This focused hit-counter test used to patch the cartridge cooldown to one.
        // Wait through the real shared projectile owner instead; do not bypass admission
        // or speed up the separately stepped enemy phase/timer fixture.
        for (int cooldownFrame = 0; cooldownFrame < 15 && !sharedProjectiles.CooldownTimer.IsZero; cooldownFrame++)
        {
            sharedProjectiles.StepFrame(new SamusTerrain { Level = air }, samus, 0, 0);
            projectiles.StepFrame(new SamusTerrain { Level = air }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0 }, new(0, 0), sharedProjectiles);
        }
        AssertEqual((ushort)0, sharedProjectiles.CooldownTimer.RawValue, "Native cooldown expires before the next Ceres shot");

        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
    }

    AssertEqual(100, state.HitCounter, "Ceres Ridley uses dedicated 100-shot counter");
    AssertTrue(observedNormalRidleyPalette && observedFlashRidleyPalette,
        "Ceres Ridley beam hits alternate common body damage palette");

    int retreatFrames = 0;
    bool observedFakeRetreat = false;
    Bgr555? retreatBackgroundColor = null;
    while (enemies.RidleyEncounter.CeresStatus != CeresEscapeStatus.RidleyEscaped && retreatFrames < 1024)
    {
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
        // $A6:A9E3 writes BG palette five as the retreat delay expires; the next frame's
        // first getaway step replaces it with the Mode-7 zoom palette from $51.
        if (state.Function == RidleyAiFunction.CeresPublishEscapeHandoff)
            retreatBackgroundColor ??= cgram.Colors[0x51];
        // Room main follows enemy AI, so the handoff frame also takes getaway step zero.
        enemies.CeresRidleyBehavior.Getaway.RunCeresRidleyGetawayRoomMain(samus, 0);
        observedFakeRetreat |= state.Function is
            RidleyAiFunction.CeresFakeRetreatMoveToPosition or
            RidleyAiFunction.CeresFakeRetreatRising or
            RidleyAiFunction.CeresWaitBeforeRetrievingBaby or
            RidleyAiFunction.CeresRetrieveBaby;
        retreatFrames++;
    }
    AssertTrue(observedFakeRetreat,
        "Ceres Ridley returns from current attack through fake retreat/Baby retrieval");
    AssertEqual(RidleyFightMode.NotFighting, state.FightMode, "Ceres Ridley retreat disables battle mode");
    AssertEqual(CeresEscapeStatus.RidleyEscaped, enemies.RidleyEncounter.CeresStatus, "Ceres Ridley publishes escape handoff status");
    AssertEqual((ushort)RidleyAiFunction.CeresInactive, (ushort)state.Function,
        "Ceres Ridley installs null dispatcher after battle");
    AssertTrue(ridley.Properties.HasAny(EnemyProperties.Invisible),
        "Ceres Ridley ordinary actor yields to getaway presentation");
    RoomEnemySlot[] mode7Walls = enemies.SlotTable.Slots
        .Where(slot => slot.DefinitionId == ceresDoorDefinitionPointer &&
            slot.Spawn.Population.Parameter1 is 5 or 6)
        .ToArray();
    AssertEqual(2, mode7Walls.Length,
        "Ceres Ridley spawns both native Mode-7 wall actors");
    AssertEqual((Bgr555?)Bgr555.FromWord(0x6100), retreatBackgroundColor,
        "Ceres Ridley retreat copies BG palette-five colors");
    AssertEqual(0x6200, cgram.Colors[0x21],
        "Ceres Ridley retreat copies BG palette-two colors");
    AssertEqual(0x6200, cgram.Colors[0xf1],
        "Ceres Ridley retreat copies OBJ palette-seven colors");

    var escapeWrites = new VramWriteQueue();
    using (var timerPng = new MemoryStream())
    {
        IndexedPng.Write(timerPng,
            new(EscapeTimerTileAtlasFormat.Width, EscapeTimerTileAtlasFormat.Height),
            new byte[EscapeTimerTileAtlasFormat.Width * EscapeTimerTileAtlasFormat.Height],
            SnesGraphics.DiagnosticPalette(EscapeTimerTileAtlasFormat.ColorCount));
        timerPng.Position = 0;
        enemies.BossArtworkBinding.EscapeTimerArtwork = EscapeTimerTileAtlas.Load(timerPng);
    }
    AssertTrue(enemies.Audio.SoundRequests.Contains(
            new SoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x4e), MaximumQueued: 6)),
        "handoff frame's first Mode-7 getaway entry publishes QueueSfx2_Max6($4E)");
    void GetawayFrame()
    {
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, VramWriteQueue = escapeWrites });
        enemies.CeresRidleyBehavior.Getaway.RunCeresRidleyGetawayRoomMain(samus, 0);
    }
    ushort expectedX = 0xff80, expectedY = 0x20;
    for (int curveIndex = 0; curveIndex < 112; curveIndex++)
    {
        if (curveIndex != 0)
            GetawayFrame();
        var curve = CeresRidleyGetawayDefinitions.FromIndex(curveIndex);
        expectedX = unchecked((ushort)(expectedX - curve.Velocity.X));
        expectedY = unchecked((ushort)(expectedY + curve.Velocity.Y));
        AssertEqual(curve.Zoom, state.Mode7Zoom, "Ceres real getaway zoom publication");
        AssertEqual(unchecked((short)expectedX), state.Mode7Offset.X, "Ceres real getaway horizontal integration");
        AssertEqual(unchecked((short)expectedY), state.Mode7Offset.Y, "Ceres real getaway vertical integration");
        AssertTrue(!state.Mode7Finished, "Ceres getaway retains all 112 authored motion frames");
    }
    GetawayFrame();
    AssertTrue(state.Mode7Finished, "Ceres Ridley consumes the Mode-7 terminator");
    AssertEqual((ushort)RidleyAiFunction.CeresActivateSelfDestruct, (ushort)state.Function,
        "Ceres Ridley Mode-7 terminator installs shared self-destruct dispatcher");
    AssertEqual(0, escapeWrites.Entries.Count,
        "Mode-7 terminator does not run the new actor function in the same enemy frame");

    var spriteTransfers = ReadRetailCeresEscapeTransfers(nativeRidleyRom, 0xa6c4cb);
    var backgroundTransfers = ReadRetailCeresEscapeTransfers(nativeRidleyRom, 0xa6c4fe);
    AssertEqual(7, spriteTransfers.Count, "native Ceres sprite transfer count");
    AssertEqual(8, backgroundTransfers.Count, "native Ceres background transfer count");
    for (int record = 0; record < spriteTransfers.Count; record++)
    {
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, VramWriteQueue = escapeWrites });
        bool finalSprite = record == spriteTransfers.Count - 1;
        AssertEqual(record + 1 + (finalSprite ? 1 : 0), escapeWrites.Entries.Count,
            "Ceres sprite transfers yield one per frame and fall through on the final record");
        AssertEqual(finalSprite ? 4 : 2, state.FunctionTimer.RawValue, "Ceres sprite/background phase handoff");
    }
    for (int record = 1; record < backgroundTransfers.Count; record++)
    {
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, VramWriteQueue = escapeWrites });
        bool finalBackground = record == backgroundTransfers.Count - 1;
        AssertEqual(spriteTransfers.Count + record + 1, escapeWrites.Entries.Count, "Ceres background transfers yield one per frame");
        AssertEqual(finalBackground, ceresOverlay.Words.Any(word => word != RoomTilemapOverlay.Unwritten),
            "Ceres writes EMERGENCY over BG1 on the final background frame");
        AssertEqual(finalBackground ? 6 : 4, state.FunctionTimer.RawValue, "Ceres background/hold phase handoff");
    }
    var nativeTransfers = spriteTransfers.Concat(backgroundTransfers).ToArray();
    for (int index = 0; index < nativeTransfers.Length; index++)
    {
        VramWriteEntry expected = nativeTransfers[index];
        VramWriteEntry actual = escapeWrites.Entries[index];
        AssertEqual(expected.SizeInBytes, actual.SizeInBytes, $"Ceres transfer {index} native size");
        if (index < 2)
        {
            AssertEqual(0xb0c000 + index * 0x200, expected.SourceAddress,
                $"Ceres timer {index} native source mapped to installed asset");
            AssertEqual(index == 0 ? VramAssetId.EscapeTimerFirstTiles : VramAssetId.EscapeTimerSecondTiles,
                actual.AssetId, $"Ceres timer {index} installed source identity");
        }
        else if (CeresEscapeTileCharacters.ForTransfer(expected.SourceAddress, expected.SizeInBytes) is { } characters)
            AssertEqual<ObjCharacterSource?>(characters, actual.OwnedSource, $"Ceres transfer {index} names its installed character page");
        else
            AssertEqual(expected.SourceAddress, actual.SourceAddress, $"Ceres transfer {index} native source");
        AssertEqual(expected.Destination.Encoded, actual.Destination.Encoded,
            $"Ceres transfer {index} native destination");
    }
    AssertEqual(VramAssetId.EscapeTimerFirstTiles, escapeWrites.Entries[0].AssetId,
        "Ceres first timer record selects installed artwork");
    AssertEqual(VramAssetId.EscapeTimerSecondTiles, escapeWrites.Entries[1].AssetId,
        "Ceres second timer record selects installed artwork");
    CeresEscapeOverlayTilemapDefinition emergency = CeresEscapeOverlayTilemapDefinitions.Emergency;
    AssertTrue((InstalledFixtures.AreaArtwork.CeresEscapeOverlayTilemaps ?? throw new InvalidOperationException(
            "Verification requires installed Ceres overlay tilemaps.")).TryResolve(0xa6c164, emergency.WordCount * sizeof(ushort),
            out ReadOnlyMemory<byte> emergencyBytes),
        "Ceres EMERGENCY text resolves the cartridge tilemap words");
    for (int word = 0; word < emergency.WordCount; word++)
        AssertEqual(SnesLittleEndian.Word(emergencyBytes.Span[word * 2], emergencyBytes.Span[word * 2 + 1]),
            ceresOverlay.Words[0x50cb - 0x5000 + word], $"Ceres EMERGENCY word {word} is shown at BG1 row six");
    AssertEqual(emergency.WordCount, ceresOverlay.Words.Count(word => word != RoomTilemapOverlay.Unwritten),
        "Ceres EMERGENCY writes only its own words over BG1");
    AssertEqual(6, state.FunctionTimer.RawValue,
        "Ceres self-destruct reaches the native 128-frame English hold");
    AssertEqual(128, state.CeresEscapeTextDelayTimer.RawValue,
        "Ceres English warning hold starts at 128");

    for (int frame = 0; frame < 127; frame++)
    {
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, VramWriteQueue = escapeWrites });
        AssertTrue(!enemies.LastEnemyFrame.CeresEscapeStarted,
            $"Ceres escape does not publish early on hold frame {frame}");
    }
    enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, VramWriteQueue = escapeWrites });
    AssertTrue(!enemies.LastEnemyFrame.CeresEscapeStarted,
        "Ceres escape does not skip the English warning after its 128-frame title hold");
    AssertEqual(10, state.FunctionTimer.RawValue,
        "Ceres English warning enters the native typewriter phase");

    int typewriterFrames = 0;
    while (!enemies.LastEnemyFrame.CeresEscapeStarted && typewriterFrames < 512)
    {
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, VramWriteQueue = escapeWrites });
        typewriterFrames++;
    }
    AssertTrue(enemies.LastEnemyFrame.CeresEscapeStarted,
        "Ceres escape starts only after the complete English warning is typed");
    AssertTrue(ceresBossDefeated,
        "$A6:C131 sets the area-boss bit within the escape-start enemy pass");
    AssertTrue(typewriterFrames > 128,
        "Ceres English warning remains visible long enough to type all three lines");
    AssertEqual(0x3594, ceresOverlay.Words[0x5105 - 0x5000],
        "Ceres warning writes the S in SELF from the cartridge typewriter alphabet over BG1");
    AssertEqual(0x3582, ceresOverlay.Words[0x5145 - 0x5000],
        "Ceres warning writes the A in ACTIVATED on its second line over BG1");
    AssertEqual(0x3584, ceresOverlay.Words[0x5185 - 0x5000],
        "Ceres warning writes the C in COLONY on its third line over BG1");
    AssertEqual(CeresEscapeStatus.SelfDestructStarted, enemies.RidleyEncounter.CeresStatus,
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
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the Ceres English escape typewriter program")]
static void WriteCeresEnglishEscapeWarning(TestAddressSpace bus)
{
    int cursor = 0xa6c450;

    void Word(ushort value)
    {
        WriteTestWord(bus, cursor, value);
        cursor += 2;
    }

    void Text(string value)
    {
        foreach (char character in value)
            WriteTestByte(bus, cursor++, checked((byte)character));
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

/// <summary>Writes the Zebes escape warning's control words and text into the fixture bus.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the Zebes escape typewriter program")]
static void WriteZebesEscapeWarning(TestAddressSpace bus)
{
    int cursor = EscapeTypewriterDefinitions.ZebesSourceAddress;

    void Word(ushort value)
    {
        WriteTestWord(bus, cursor, value);
        cursor += 2;
    }

    void Text(string value)
    {
        foreach (char character in value)
            WriteTestByte(bus, cursor++, checked((byte)character));
    }

    Word(EscapeTypewriterRomData.Delay);
    Word(EscapeTypewriterDefinitions.CharacterDelayFrames);
    Word(EscapeTypewriterRomData.Destination);
    Word(0x4905);
    Text("TIME BOMB SET!");
    Word(EscapeTypewriterRomData.Destination);
    Word(0x4945);
    Text("ESCAPE IMMEDIATELY!");
    Word(EscapeTypewriterRomData.End);
}

/// <summary>Seeds Ripper's literal four-frame right- and left-facing bank-$A2 instruction lists.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for Ripper's bank-$A2 instruction lists")]
static void SeedRipperInstructionLists(TestAddressSpace bus, ushort[] rightList, ushort[] leftList)
{
    WriteTestWords(bus, 0xa2e477, rightList);
    WriteTestWords(bus, 0xa2e48b, leftList);
}

/// <summary>Points the two bank-$93 projectile data words Ripper's shot fixture consults at $9100/$9200.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the Ripper shot projectile data pointers")]
static void SeedRipperProjectileDataPointers(TestAddressSpace bus)
{
    WriteTestWord(bus, 0x93867b, 0x9100);
    WriteTestWord(bus, 0x93867f, 0x9200);
}

/// <summary>
/// Seeds the Ceres elevator platform's main-function and instruction words plus the literal
/// two one-entry transfer records at $A6:F904/$F90E. Each writes four low map bytes
/// (<paramref name="light"/>, then <paramref name="dark"/>) to Mode-7 word $060E, alternating
/// by NMI bit one; six distinct palette words follow at $A6:F871.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the Ceres elevator platform Mode-7 transfers")]
static void SeedCeresElevatorPlatformTransfers(TestAddressSpace bus, byte[] light, byte[] dark)
{
    WriteTestWord(bus, 0xa6f72f, 0xf850);
    WriteTestWord(bus, 0xa6f530, 0xfac7);
    WriteTestWord(bus, 0xa6f900, 0xf904);
    WriteTestWord(bus, 0xa6f902, 0xf90e);
    WriteTestByte(bus, 0xa6f904, 0x80);
    WriteTestLong(bus, 0xa6f905, 0xa6f918);
    WriteTestWord(bus, 0xa6f908, 4);
    WriteTestWord(bus, 0xa6f90a, 0x060e);
    WriteTestByte(bus, 0xa6f90c, 0);
    WriteTestByte(bus, 0xa6f90d, 0);
    WriteTestByte(bus, 0xa6f90e, 0x80);
    WriteTestLong(bus, 0xa6f90f, 0xa6f91c);
    WriteTestWord(bus, 0xa6f912, 4);
    WriteTestWord(bus, 0xa6f914, 0x060e);
    WriteTestByte(bus, 0xa6f916, 0);
    WriteTestByte(bus, 0xa6f917, 0);
    for (int index = 0; index < 4; index++)
    {
        WriteTestByte(bus, 0xa6f918 + index, light[index]);
        WriteTestByte(bus, 0xa6f91c + index, dark[index]);
    }
    for (int color = 0; color < 6; color++)
        WriteTestWord(bus, 0xa6f871 + color * 2, unchecked((ushort)(0x4400 + color)));
}

/// <summary>Seeds the uncharged beam pre-instruction table's first entry with the no-wave pre-instruction.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the uncharged no-wave beam pre-instruction")]
static void SeedUnchargedNoWavePreInstruction(TestAddressSpace bus) =>
    WriteTestWord(bus, SamusBeamPreInstructionCodes.UnchargedTable, SamusBeamPreInstructionCodes.NoWave);

/// <summary>
/// Zeroes the Ceres escape typewriter's cartridge source (up to $A6:C4CB) after its presentation
/// has been extracted, so the runtime cannot pass by reading the text from cartridge bytes.
/// </summary>
[CpuBusBoundary("exact bus test: clears the synthetic Ceres escape typewriter cartridge source")]
static void ClearCeresEscapeTypewriterSource(TestAddressSpace bus)
{
    for (int address = EscapeTypewriterDefinitions.CeresSourceAddress;
         address < 0xa6c4cb;
         address++)
        WriteTestByte(bus, address, 0);
}

/// <summary>
/// Seeds Ridley's private `$A6:A2F2` door overlay: a one-entry spritemap at $A6:A329 and the
/// earthquake displacement word table at $A6:A321 whose retail byte-index bug reads $FC for timer two.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the Ceres door earthquake overlay")]
static void SeedCeresDoorQuakeOverlay(TestAddressSpace bus)
{
    WriteTestWord(bus, 0xa6a329, 1);
    WriteTestWord(bus, 0xa6a32b, 0);
    WriteTestByte(bus, 0xa6a32d, 0);
    WriteTestWord(bus, 0xa6a32e, 0);
    WriteTestByte(bus, 0xa6a321, 0x00);
    WriteTestByte(bus, 0xa6a322, 0x00);
    WriteTestByte(bus, 0xa6a323, 0xfc);
    WriteTestByte(bus, 0xa6a324, 0xff);
}

/// <summary>Reads back the seeded Ceres Ridley palette words ($A6:E14F) and one planar tile ($A6:9000) as installed fixture artwork.</summary>
[CpuBusBoundary("exact bus test: reads back the synthetic cartridge's seeded Ceres Ridley palette and tile bytes")]
static ReadCeresRidleyFixtureArtworkResult ReadCeresRidleyFixtureArtwork(TestAddressSpace bus)
{
    ushort[] paletteWords = Enumerable.Range(0, EnemyPaletteSheet.ColorCount)
        .Select(color => SnesLittleEndian.Word(bus.ReadByte(0xa6e14f + color * 2), bus.ReadByte(0xa6e150 + color * 2)))
        .ToArray();
    byte[] planar = Enumerable.Range(0, 0x20)
        .Select(offset => bus.ReadByte(0xa69000 + offset)).ToArray();
    return new ReadCeresRidleyFixtureArtworkResult(paletteWords, planar);
}

/// <summary>Writes one complete fixture header while keeping pointer-bearing fields valid.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for one complete bank-$A0 enemy definition header")]
internal static void WriteEnemyDefinition(TestAddressSpace bus, EnemyDefinitionId definitionPointer, EnemyDefinitionFields fields)
{
    int address = 0xa00000 | (ushort)definitionPointer;
    for (int offset = 0; offset < 64; offset += 2)
        WriteTestWord(bus, address + offset, unchecked((ushort)(fields.FieldSeed + offset)));

    WriteTestWord(bus, address, fields.TileDataSize);
    WriteTestWord(bus, address + 2, fields.PalettePointer);
    WriteTestByte(bus, address + 12, fields.Bank);
    WriteTestByte(bus, address + 13, 0x5a);
    WriteTestWord(bus, address + 16, fields.BossId);
    WriteTestWord(bus, address + 18, 0x804c); // Bank-local no-op initialization AI.
    WriteTestWord(bus, address + 24, 0x804c); // Bank-local no-op main AI.
    WriteTestWord(bus, address + 26, (ushort)GrappleAiRoutine.Cancel); // Grapple AI must be a real routine.
    WriteTestLong(bus, address + 54, fields.TileDataAddress);
    WriteTestByte(bus, address + 57, 5);
    WriteTestWord(bus, address + 62, fields.NamePointer);
}

/// <summary>Writes one native little-endian 24-bit long pointer into the sparse SNES-bus fixture.</summary>
[CpuBusAccess]
[CpuBusBoundary("exact bus test: sparse-fixture 24-bit long-pointer seeding primitive")]
static void WriteTestLong(TestAddressSpace bus, int address, int value)
{
    WriteTestByte(bus, address, unchecked((byte)value));
    WriteTestByte(bus, address + 1, SnesLittleEndian.HighByte(unchecked((ushort)value)));
    WriteTestByte(bus, address + 2, SnesLittleEndian.BankByte(value));
}

/// <summary>
/// Synthetic cartridge for the Ceres Ridley room entry: Ridley's and the Ceres door's headers,
/// door dispatch lists, graphics set, palettes, the retail population record, Ridley's literal
/// animation lists and extended hitbox frame, additional palettes, end-of-battle palettes,
/// escape-warning transfers and text, and a minimal power-beam data/list pair.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the Ceres Ridley room entry")]
static void SeedCeresRidleyRoomEntryCartridge(
    TestAddressSpace bus,
    EnemyDefinitionId definitionPointer,
    ushort populationPointer,
    ushort tilesetPointer)
{
    WriteEnemyDefinition(bus, definitionPointer, new EnemyDefinitionFields { TileDataSize = 0x0020, PalettePointer = 0xe14f, Bank = 0xa6, TileDataAddress = 0xa69000, BossId = 1, NamePointer = 0, FieldSeed = 0x4000 });
    SeedEnemyDefinitionWord(bus, definitionPointer, 18, 0xa0f5);
    SeedEnemyDefinitionWord(bus, definitionPointer, 24, 0xa288);
    SeedEnemyDefinitionWord(bus, definitionPointer, 8, 8);
    SeedEnemyDefinitionWord(bus, definitionPointer, 10, 8);

    // The native post-battle path dynamically spawns Ceres-door variants five and six as
    // the two Mode-7 chamber walls. Keep their real header/dispatch/list boundaries in this
    // fixture so the escape audit cannot pass by changing only Ridley's logical status.
    const EnemyDefinitionId ceresDoorDefinitionPointer = EnemyDefinitionId.CeresDoor;
    WriteEnemyDefinition(bus, ceresDoorDefinitionPointer, new EnemyDefinitionFields { TileDataSize = 0, PalettePointer = 0xf4ec, Bank = 0xa6, TileDataAddress = 0xa69000, BossId = 0, NamePointer = 0, FieldSeed = 0x4100 });
    SeedEnemyDefinitionWord(bus, ceresDoorDefinitionPointer, 18, 0xf6c5);
    SeedEnemyDefinitionWord(bus, ceresDoorDefinitionPointer, 24, 0xf765);
    SeedEnemyDefinitionByte(bus, ceresDoorDefinitionPointer, 57, 2);
    WriteTestWord(bus, 0xa6f735, 0xf7a5);
    WriteTestWord(bus, 0xa6f737, 0xf7a5);
    WriteTestWord(bus, 0xa6f536, 0xf62a);
    WriteTestWord(bus, 0xa6f538, 0xf634);
    WriteTestWord(bus, 0xa6f62a, 0xf68b);
    WriteTestWord(bus, 0xa6f62c, 1);
    WriteTestWord(bus, 0xa6f62e, 0xface);
    WriteTestWord(bus, 0xa6f630, 0x80ed);
    WriteTestWord(bus, 0xa6f632, 0xf62c);
    WriteTestWord(bus, 0xa6f634, 0xf68b);
    WriteTestWord(bus, 0xa6f636, 1);
    WriteTestWord(bus, 0xa6f638, 0xfb2f);
    WriteTestWord(bus, 0xa6f63a, 0x80ed);
    WriteTestWord(bus, 0xa6f63c, 0xf636);

    // One graphics-set entry supplies Ridley's OBJ palette/tile association. The byte data
    // need not depict retail art here; the regression concerns loader and AI addresses, and
    // the real-ROM audit covers the actual encoded graphics immediately afterward.
    WriteTestWord(bus, 0xb40000 | tilesetPointer, (ushort)definitionPointer);
    WriteTestWord(bus, (0xb40000 | tilesetPointer) + 2, 0x0001);
    WriteTestWord(bus, (0xb40000 | tilesetPointer) + 4, 0xffff);
    for (int color = 0; color < 16; color++)
        WriteTestWord(bus, 0xa6e14f + color * 2, unchecked((ushort)(0x1000 + color)));

    // The Ceres population record is deliberately the retail one: ($BA,$AB), init zero,
    // properties $2800, no extra bits, and two zero speed/parameter words.
    int population = 0xa10000 | populationPointer;
    WriteTestWord(bus, population, (ushort)definitionPointer);
    WriteTestWord(bus, population + 2, 0x00ba);
    WriteTestWord(bus, population + 4, 0x00ab);
    WriteTestWord(bus, population + 6, 0);
    WriteTestWord(bus, population + 8, 0x2800);
    WriteTestWord(bus, population + 10, 0);
    WriteTestWord(bus, population + 12, 0);
    WriteTestWord(bus, population + 14, 0);
    WriteTestWord(bus, population + 16, 0xffff);
    WriteTestByte(bus, population + 18, 0);

    // $E538 selects the left-facing initial frame and then sleeps. Supplying the literal
    // list catches the custom $E517 branch rather than letting a generic timed frame mask it.
    WriteTestWord(bus, 0xa6e538, 0xe517);
    WriteTestWord(bus, 0xa6e53a, 0xe542);
    WriteTestWord(bus, 0xa6e53c, 12);
    WriteTestWord(bus, 0xa6e53e, 0x9000);
    WriteTestWord(bus, 0xa6e540, 0x812f);

    // Active extended frame: one body component sits 32 pixels right of Ridley's origin.
    // Its 16x16 hitbox is outside the header's deliberately tiny 8x8 fallback radius,
    // proving that projectile collision walks cartridge hitboxes instead of a host box.
    WriteTestWord(bus, 0xa69000, 1);
    WriteTestWord(bus, 0xa69002, 32);
    WriteTestWord(bus, 0xa69004, 0);
    WriteTestWord(bus, 0xa69006, 0x9200);
    WriteTestWord(bus, 0xa69008, 0x9010);
    WriteTestWord(bus, 0xa69010, 1);
    WriteTestWord(bus, 0xa69012, unchecked((ushort)-8));
    WriteTestWord(bus, 0xa69014, unchecked((ushort)-8));
    WriteTestWord(bus, 0xa69016, 8);
    WriteTestWord(bus, 0xa69018, 8);
    WriteTestWord(bus, 0xa6901a, 0xdf59);
    WriteTestWord(bus, 0xa6901c, 0xdf8a);
    WriteTestWord(bus, 0xa69200, 0);

    // Initial additional palettes are copied to OBJ palettes two/three. The later eye and
    // body tables use distinct sentinels so destination/index mistakes remain observable.
    for (int color = 0; color < 32; color++)
        WriteTestWord(bus, 0xa6e16f + color * 2, unchecked((ushort)(0x2000 + color)));
    for (int index = 0; index < 64; index++)
        WriteTestByte(bus, 0xa6e269 + index, index < 16 ? unchecked((byte)(15 - index)) : (byte)0);
    WriteTestByte(bus, 0xa6e2a9, 0xff);
    for (int word = 0; word < (0xe30a - 0xe2aa) / 2; word++)
        WriteTestWord(bus, 0xa6e2aa + word * 2, unchecked((ushort)(0x3000 + word)));
    for (int word = 0; word < 0x160 / 2; word++)
        WriteTestWord(bus, 0xa6e30a + word * 2, unchecked((ushort)(0x5000 + word)));
    for (int palette = 0; palette < 3; palette++)
    {
        for (int color = 0; color < 14; color++)
            WriteTestWord(bus, 0xa6e46a + palette * 28 + color * 2,
                unchecked((ushort)(0x7000 + palette * 0x0100 + color)));
    }

    // The two later animation lists are the actual dispatcher seams that make reveal-only
    // implementations fail. The compact roar list keeps its real $E4BE opcode before a
    // single timed map, so both the roaring flag and QueueSfx2_Max6($59) cross the actual
    // instruction dispatcher. The liftoff list retains its real $E969 instruction so AI
    // control changes exactly as it does in the cartridge. Movement divisors are the
    // literal $10..$01 D712 table.
    WriteTestWord(bus, 0xa6e690, 0xe4be);
    WriteTestWord(bus, 0xa6e692, 1);
    WriteTestWord(bus, 0xa6e694, 0x9000);
    WriteTestWord(bus, 0xa6e696, 0x812f);
    WriteTestWord(bus, 0xa6e548, 1);
    WriteTestWord(bus, 0xa6e54a, 0x9000);
    WriteTestWord(bus, 0xa6e54c, 0x812f);
    WriteTestWord(bus, 0xa6e91d, 0xe969);
    WriteTestWord(bus, 0xa6e91f, 1);
    WriteTestWord(bus, 0xa6e921, 0x9000);
    WriteTestWord(bus, 0xa6e923, 0x812f);
    for (int divisor = 0; divisor < 16; divisor++)
        WriteTestByte(bus, 0xa6d712 + divisor, unchecked((byte)(16 - divisor)));

    // The tail position solver reads the shared signed 8.8 sine table at $A0:B443.
    // Populate the sixteen cardinal/intercardinal samples touched by the initialized
    // $4000,$4010... tail angles; an all-zero synthetic bus would otherwise make correct
    // polar-to-Cartesian code appear to leave every tail piece at the same coordinate.
    SineSample[] sineSamples =
    [
        new SineSample(0x00, 0), new SineSample(0x10, 98), new SineSample(0x20, 181), new SineSample(0x30, 237),
        new SineSample(0x40, 256), new SineSample(0x50, 237), new SineSample(0x60, 181), new SineSample(0x70, 98),
        new SineSample(0x80, 0), new SineSample(0x90, -98), new SineSample(0xa0, -181), new SineSample(0xb0, -237),
        new SineSample(0xc0, -256), new SineSample(0xd0, -237), new SineSample(0xe0, -181), new SineSample(0xf0, -98),
    ];
    SeedSignedSineTable(bus, sineSamples);

    // End-of-battle palette records are deliberately unmistakable. AA11 must not publish
    // status one without A9A0 first copying all three native destination ranges.
    for (int color = 0; color < 15; color++)
        WriteTestWord(bus, 0xa6a9e3 + color * 2, unchecked((ushort)(0x6100 + color)));
    for (int color = 0; color < 8; color++)
        WriteTestWord(bus, 0xa6aa01 + color * 2, unchecked((ushort)(0x6200 + color)));

    // Keep warning transfers distinguishable after the full compiled Mode-7 curve.
    // The later assertions cover the first-list yield and second-list handoff separately.
    WriteTestWord(bus, 0xa6c4cb, EscapeTimerTileAtlasFormat.FirstByteCount);
    WriteTestWord(bus, 0xa6c4cd, 0xc000);
    WriteTestByte(bus, 0xa6c4cf, 0xb0);
    WriteTestWord(bus, 0xa6c4d0, EscapeTimerTileAtlasFormat.FirstDestinationWord);
    WriteTestWord(bus, 0xa6c4d2, EscapeTimerTileAtlasFormat.SecondByteCount);
    WriteTestWord(bus, 0xa6c4d4, 0xc200);
    WriteTestByte(bus, 0xa6c4d6, 0xb0);
    WriteTestWord(bus, 0xa6c4d7, EscapeTimerTileAtlasFormat.SecondDestinationWord);
    WriteTestWord(bus, 0xa6c4d9, 0);
    WriteTestWord(bus, 0xa6c4fe, 2);
    WriteTestWord(bus, 0xa6c500, 0x9204);
    WriteTestByte(bus, 0xa6c502, 0xb0);
    WriteTestWord(bus, 0xa6c503, 0x7802);
    WriteTestWord(bus, 0xa6c505, 2);
    WriteTestWord(bus, 0xa6c507, 0x9206);
    WriteTestByte(bus, 0xa6c509, 0xb0);
    WriteTestWord(bus, 0xa6c50a, 0x7803);
    WriteTestWord(bus, 0xa6c50c, 0);
    WriteTestWord(bus, 0xb09200, 0x1234);
    WriteTestWord(bus, 0xb09202, 0x5678);
    WriteTestWord(bus, 0xb09204, 0x9abc);
    WriteTestWord(bus, 0xb09206, 0xdef0);
    WriteCeresEnglishEscapeWarning(bus);
    WriteZebesEscapeWarning(bus);
    for (int paletteFrame = 0; paletteFrame < 16; paletteFrame++)
    {
        for (int color = 0; color < 3; color++)
            WriteTestWord(bus, 0xa6c1df + paletteFrame * 6 + color * 2,
                unchecked((ushort)(0x4000 + paletteFrame * 0x10 + color)));
    }

    // One minimal power-beam data/list pair lets this regression reach Ridley's shot AI
    // through the public producer and explosion owner, rather than mutating HitCounter.
    WritePoseDefinition(
        bus,
        (int)SamusPoseId.FacingRightNormalPose,
        [0x08, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00]);
    WriteTestWord(bus, 0x9383c1, 0x8431);
    WriteTestWord(bus, 0x938431, 20);
    WriteTestWord(bus, 0x938437, 0x9000); // Direction two is data-pointer word three.
    WriteTestWord(bus, 0x939000, 1);
    WriteTestWord(bus, 0x939002, 0x9000);
    WriteTestByte(bus, 0x939004, 8);
    WriteTestByte(bus, 0x939005, 8);
    WriteTestWord(bus, 0x939006, 0);
    WriteTestWord(bus, 0x939008, 0x8239);
    WriteTestWord(bus, 0x93900a, 0x9000);
    WriteTestWord(bus, 0x93867b, 0x9100);
    WriteTestWord(bus, 0x939100, 1);
    WriteTestWord(bus, 0x939102, 0x9000);
    WriteTestByte(bus, 0x939104, 8);
    WriteTestByte(bus, 0x939105, 8);
    WriteTestWord(bus, 0x939106, 0);
    WriteTestWord(bus, 0x939108, 0x822f);
    WriteTestWord(bus, 0x90c28f, 0x000b);
}

    /// <summary>Runs the room-enemy population, graphics staging, and palette loading fixture.</summary>
    [VerificationCommand("--room-enemy-loading")]
    internal static int RoomEnemyLoadingCommand()
    {
        Suite(nameof(VerifyRoomEnemyLoading), () => VerifyRoomEnemyLoading());
        return 0;
    }

    /// <summary>Runs the explicit enemy WRAM mapping and source-reader boundary checks.</summary>
    [VerificationCommand("--enemy-source-routing")]
    internal static int EnemySourceRoutingCommand()
    {
        Suite(nameof(VerifyEnemyMappedSourceRouting), () => VerifyEnemyMappedSourceRouting());
        return 0;
    }

    /// <summary>Checks Ripper contact during the deferred enemy collision phase.</summary>
    [VerificationCommand("--enemy-contact-phase")]
    internal static int EnemyContactPhaseCommand()
    {
        Suite(nameof(VerifyRipperEnemy), () => VerifyRipperEnemy(verifyDeferredContact: true));
        return 0;
    }

    /// <summary>Runs the Ripper animation, movement, damage, and death fixture.</summary>
    [VerificationCommand("--ripper-enemy")]
    internal static int RipperEnemyCommand()
    {
        Suite(nameof(VerifyRipperEnemy), () => VerifyRipperEnemy());
        return 0;
    }

    /// <summary>Checks that the landed Ceres platform remains animated by its door actor.</summary>
    [VerificationCommand("--ceres-elevator-platform")]
    internal static int CeresElevatorPlatformCommand()
    {
        Suite(nameof(VerifyCeresElevatorPlatformAnimation), () => VerifyCeresElevatorPlatformAnimation());
        return 0;
    }

    /// <summary>Checks the Ceres door's boss-state branch in the room-enemy fixture.</summary>
    [VerificationCommand("--ceres-door-boss")]
    internal static int CeresDoorBossCommand()
    {
        Suite(nameof(VerifyCeresDoorBossBranch), () => VerifyCeresDoorBossBranch());
        return 0;
    }

    /// <summary>Checks Ripper X-Ray and Plasma interaction timers against the native fixture.</summary>
    [VerificationCommand("--x-plasma-timers")]
    internal static int XPlasmaTimersCommand()
    {
        Suite(nameof(VerifyRipperEnemy), () => VerifyRipperEnemy(verifyXrayTimers: true));
        return 0;
    }

    /// <summary>Checks Ceres Ridley's state when its room is entered.</summary>
    [VerificationCommand("--ceres-ridley-room-entry")]
    internal static int CeresRidleyRoomEntryCommand()
    {
        Suite(nameof(VerifyCeresRidleyRoomEntry), () => VerifyCeresRidleyRoomEntry());
        return 0;
    }
}

/// <summary>Serves the loading fixture's seeded enemy tiles at their seeded addresses: the bytes installed artwork
/// would hold for those definitions.</summary>
/// <param name="tiles">Each seeded tile block by its 24-bit source address.</param>
internal sealed class SeededEnemyTileTransfers(IReadOnlyDictionary<int, byte[]> tiles)
    : IVramAssetProvider, IInstalledArtworkTransferSource, IObjCharacterSourceResolver
{
    /// <summary>Rejects typed assets, which the fixture never queues.</summary>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset) => throw new InvalidOperationException($"The loading fixture queues no asset {asset}.");

    /// <summary>Serves a seeded block whose size matches the transfer.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        bool found = tiles.TryGetValue(sourceAddress, out byte[]? block) && block.Length == byteCount;
        data = found ? block : ReadOnlyMemory<byte>.Empty;
        return found;
    }

    /// <summary>Rejects typed assets, which the fixture never binds.</summary>
    public ReadOnlyMemory<byte> ResolveAsset(VramAssetId asset) => Resolve(asset);

    /// <summary>Resolves a recorded seeded transfer.</summary>
    public ReadOnlyMemory<byte> ResolveTransfer(int sourceAddress, int byteCount) =>
        TryResolve(sourceAddress, byteCount, out ReadOnlyMemory<byte> data) ? data
            : throw new InvalidOperationException($"The loading fixture seeded no {byteCount} tile bytes at ${sourceAddress:X6}.");

}
