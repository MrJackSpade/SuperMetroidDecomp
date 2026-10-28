using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Input;
using SuperMetroid.Verification.Areas.Escape.Artwork;
using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Verification.Enemies.Cacatac;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Verification.Enemies.Dragon;
using SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Samus.SamusAtmosphereArtworkBoundaryVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusVisorColorsVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.ChargeFlareCompositionsVerification;
using static SuperMetroid.Verification.Areas.Escape.Artwork.EscapeTimerPresentationVerification;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using static SuperMetroid.Verification.Samus.Drawing.SamusAtmosphericAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Samus.SamusContentIdentityVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Hud;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Harness;

using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge-oracle checks for the first group of compiled gameplay and presentation catalogs.</summary>
internal static class LookupStream1Verification
{
    /// <summary>Header row of the required Samus OAM pointer report.</summary>
    private static readonly string[] OamPointerCsvHeader =
        ["Index, NativeAddress, Composition"];
    /// <summary>The four escape-timer pixels whose native values still deviate from the calculated shapes.</summary>
    private static readonly int[] RetainedTimerDeviations =
        [1161, 1163, 189, 999];
    /// <summary>Header row of the required Samus body-frame report.</summary>
    private static readonly string[] BodyFrameCsvHeader =
        ["Component, NativeAddress, Value"];
    /// <summary>Bank-$92 body-frame records outside the contiguous runs, each covering four bytes.</summary>
    private static readonly int[] BodyFrameRecordAddresses =
        [0xe050, 0xe0d0, 0xe150, 0xe158, 0xe1d8, 0xe258, 0xe2c0, 0xe37c, 0xe4c8, 0xe4d0, 0xe4f0, 0xe4fc,
        0xe508, 0xe530, 0xe558, 0xe580, 0xe5a8, 0xe5d0, 0xe5f8, 0xe628, 0xe658, 0xe688, 0xe6b8, 0xe728,
        0xe798, 0xe7a4, 0xe7b0, 0xe7bc, 0xe7c8, 0xe7d4, 0xe7e0, 0xe7ec, 0xe7f8, 0xe804, 0xe810, 0xe81c,
        0xe890, 0xe8cc, 0xe938, 0xe974, 0xe9f4, 0xea0c, 0xea24, 0xeba4, 0xea28, 0xe968];
    /// <summary>Bank-$92 body-frame records edited by the final-only check, each covering four bytes.</summary>
    private static readonly int[] FinalBodyFrameEditAddresses =
        [0xdb58, 0xdb54, 0xdba0, 0xdbbc, 0xdbfc, 0xe2ec, 0xe37c, 0xe600, 0xe65c, 0xe6bc, 0xe534, 0xe29c,
        0xe7a8, 0xe050, 0xe158, 0xe060, 0xe0d0, 0xea40, 0xeba8, 0xe984, 0xe8a8, 0xea00, 0xdf40, 0xe458,
        0xe888];

    /// <summary>Checks world-map BG3 primitives and residual pixels against native tiles, including edited full uploads.</summary>
    internal static void VerifyLookupStream1WorldBackground(ISnesAddressSpace rom)
    {
        byte[] Read(int address, int count) => Enumerable.Range(0, count).Select(i => LookupStream1WorldBackgroundRomByte(rom, address + i)).ToArray();
        byte[] front = Read(MapSourceDefinitions.Menu.ForegroundCharacters, WorldMapArtworkFormat.ForegroundBytes);
        byte[] back = Read(MapSourceDefinitions.Menu.BackgroundCharacters, WorldMapArtworkFormat.BackgroundBytes);
        byte[] frontPixels = SnesGraphics.DecodePlanarTiles(front, 4, 16, out Extent16<ImagePixels> size);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(back, 2, 16, out Extent16<ImagePixels> backSize);
        byte[] Png(byte[] selected, int height, int colors)
        {
            using var png = new MemoryStream(); IndexedPng.Write(png, new(size.X, checked((ushort)height)), selected, SnesGraphics.DiagnosticPalette(colors));
            return png.ToArray();
        }
        byte[] frontPng = Png(frontPixels, size.Y, 16);
        WorldMapCharacters Create(byte[] selected) => WorldMapCharacters.Read(new MemoryStream(frontPng), new MemoryStream(Png(selected, backSize.Y, 4)));
        var stock = Create(pixels);
        var stored = stock.background;
        var masks = stock.digitFill;
        AssertEqual(10, masks.Count, "Exactly ten selected digit footprints");
        ulong NativeMask(int tile)
        {
            ulong mask = 0;
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
                if (pixels[(tile / 16 * 8 + y) * size.X + tile % 16 * 8 + x] == 2) mask |= 1UL << (y * 8 + x);
            return mask;
        }
        foreach (var pair in masks) AssertEqual(NativeMask(pair.Key), pair.Value, "Exact native selected footprint bits");
        int calculated = 0;
        for (int index = 0; index < pixels.Length; index++)
        {
            int x = index % size.X, y = index / size.X;
            int tile = y / 8 * 16 + x / 8;
            bool available = tile is >= 0x3c and <= 0x45
                ? WorldMapTileDefinitions.TryOutlinedPixel(tile, new(checked((ushort)(x % 8)), checked((ushort)(y % 8))), NativeMask(tile), out int value)
                : WorldMapTileDefinitions.TryBackgroundPixel(tile, new(checked((ushort)(x % 8)), checked((ushort)(y % 8))), out value);
            if (available) { AssertEqual(pixels[index], value, $"Native primitive tile{y / 8 * 16 + x / 8:X2} pixel{x % 8},{y % 8}"); calculated++; }
            AssertEqual(!available, stored.ContainsKey(index), "Exact font/icon basis, no stock primitive overrides");
        }
        AssertEqual(1725, stored.Count, "Exact residual contour pixels plus five selected font edges");
        void Check(WorldMapArtwork art, byte[] expected)
        {
            var vram = new SnesVram(); art.LoadTo(vram);
            AssertTrue(vram.Bytes.Slice(WorldMapArtworkFormatConstants.ForegroundDestination, front.Length).SequenceEqual(front), "Foreground obligation remains exact and separate");
            AssertTrue(vram.Bytes.Slice(WorldMapArtworkFormatConstants.BackgroundDestination, back.Length).SequenceEqual(
                SnesPlanarTileEncoder.Encode(expected, new(size.X, backSize.Y), 2)), "Actual complete BG3 upload preserves independent pixels");
        }
        Check(stock, pixels);
        for (int tile = 0; tile < 96; tile++)
        {
            byte[] edited = (byte[])pixels.Clone();
            int index = (tile / 16 * 8 + tile / 8 % 8) * size.X + tile % 16 * 8 + tile % 8;
            edited[index] ^= 3;
            Check(Create(edited), edited);
        }
        Check(Create(pixels.Select(p => (byte)(p ^ 3)).ToArray()), pixels.Select(p => (byte)(p ^ 3)).ToArray());
        byte[] invalid = (byte[])pixels.Clone(); invalid[0] = 4;
        AssertThrows<InvalidDataException>(() => WorldMapCharacters.Read(new MemoryStream(frontPng),
            new MemoryStream(Png(invalid, backSize.Y, 16))), "Invalid BG3 pen rejected during import, before upload");
        AssertThrows<ArgumentOutOfRangeException>(() => WorldMapTileDefinitions.TryBackgroundPixel(96, new(0, 0), out _), "Background tile bound");
        Console.WriteLine($"World BG3:6144 native pixels,{calculated} geometry/mask defaults,{stored.Count} exact source pixels/10footprints,96 independent tile edits/all-pixel inversion and actual full uploads pass.");
    }

    /// <summary>Reads a source BG3 byte for the world-map artwork oracle.</summary>
    private static byte LookupStream1WorldBackgroundRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks death atlas source-byte relations, edited explosion characters, hashes, and the explosion's character order.</summary>
    internal static void VerifyLookupStream1DeathPixels(ISnesAddressSpace rom)
    {
        // $9B:8000: the explosion's characters, in the order its spritemaps name them.
        const int explosionCharacters = 0x9b8000;
        byte[] native = Enumerable.Range(0, SamusDeathTileAtlasFormat.TotalByteCount)
            .Select(index => LookupStream1DeathPixelsRomByte(rom, explosionCharacters + SamusDeathTileAtlasFormat.ExplosionByte(index))).ToArray();
        SamusDeathTileAtlas Create(byte[] planar)
        {
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, 8, out Extent16<ImagePixels> size);
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
            png.Position = 0;
            return SamusDeathTileAtlas.Load(png);
        }
        Dictionary<int, byte> Stored(SamusDeathTileAtlas art) => art.sourceBytes;
        void Check(SamusDeathTileAtlas art, byte[] expected)
        {
            ReadOnlySpan<byte> explosion = art.Explosion.Characters;
            for (int index = 0; index < expected.Length; index++)
                AssertEqual(expected[index], explosion[SamusDeathTileAtlasFormat.ExplosionByte(index)], "Exact independent explosion character bytes");
            AssertEqual(SelectedPresentationHash.Create(nameof(SamusDeathTileAtlas),
                content => content.Append("death characters", expected)), art.ContentIdentity, "Exact pre-conversion death hash");
        }
        var stock = Create(native);
        Check(stock, native);
        AssertTrue(stock.Explosion.Characters.SequenceEqual(Enumerable.Range(0, SamusDeathTileAtlasFormat.TotalByteCount)
                .Select(offset => LookupStream1DeathPixelsRomByte(rom, explosionCharacters + offset)).ToArray()),
            "Stock explosion characters equal the native characters in spritemap order");
        AssertEqual(151 * 32, Stored(stock).Count, "Exact151 source tiles, no stock padding/repetition overrides");
        var edits = new HashSet<int>();
        for (int index = 0; index < native.Length; index++)
        {
            int source = SamusDeathTileAtlasFormat.SourceByte(index);
            AssertEqual(native[index], source < 0 ? (byte)0 : native[source], "Direct native padding/patch relation");
            AssertEqual(source == index, Stored(stock).ContainsKey(index), "Exact retained source-byte membership");
            if (source != index) { edits.Add(index); if (source >= 0) edits.Add(source); }
        }
        foreach (int edit in edits)
        {
            byte[] changed = (byte[])native.Clone(); changed[edit] ^= 255;
            Check(Create(changed), changed);
        }
        Console.WriteLine($"Death atlas:5120 native bytes,151 exact source tiles,zero stock relation overrides,{edits.Count} independent source/derived-byte edits,canonical hashes and explosion character order pass.");
    }

    /// <summary>Reads a native Samus death tile byte for exact atlas comparison.</summary>
    private static byte LookupStream1DeathPixelsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks every escape Dachora duration, pose, displacement, and uninterrupted update against bank $B3.</summary>
    internal static void VerifyLookupStream1EscapeDachoraCadence(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1EscapeDachoraCadenceRomByte(rom, address), LookupStream1EscapeDachoraCadenceRomByte(rom, address + 1));
        foreach (int program in new[] { 0, 1, 2 })
        {
            var guard = new CartridgeReadGuard(rom).Forbid(address => address is >= 0xb3e964 and < 0xb3eaa8,
                address => $"Production read a native escape-Dachora list byte ${address:X6}.");
            var enemies = CreateEscapeDachoraProgramSystem();
            var slot = enemies.SlotTable.Slots[0];
            // The typed programs (#641) replacing the native lists at $B3:E964, $B3:E9D0 and $B3:EA34.
            EscapeDachoraEnemyState state = enemies.EscapeDachoraStates[0] ?? throw new InvalidOperationException("The Dachora fixture has no state.");
            state.Program = AnimationProgramCursor<EscapeDachoraProgram>.Start(program == 0 ? EscapeDachoraProgram.RunningAroundLowTide
                : program == 1 ? EscapeDachoraProgram.RunningAroundHighTide : EscapeDachoraProgram.RunningForEscape);
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            int records = program == 2 ? 19 : 60, expectedX = slot.Position.X.Whole, calls = 0;
            for (int record = 0; record < records; record++)
            {
                int address;
                if (program == 2)
                {
                    address = record == 0 ? 0xea34 : 0xea38 + (record - 1) * 6;
                    if (record > 1) expectedX += Word(0xb3eadf);
                }
                else
                {
                    int direction = record / 30;
                    int start = program == 0 ? direction == 0 ? 0xe968 : 0xe99c : direction == 0 ? 0xe9d4 : 0xea04;
                    address = start + (record % 6) * 6;
                    if (record != 0) expectedX += (record <= 30 ? -1 : 1) * Word(0xb3eadf);
                }
                ushort duration = Word(0xb30000 | address), visual = Word(0xb30000 | (address + 2));
                for (int held = 0; held < duration; held++)
                {
                    EnemyInstructionRunner.Process(arguments); calls++;
                    AssertEqual((ushort)(duration - held), slot.InstructionTimer.RawValue, "Every uninterrupted escape Dachora exposure tick");
                    AssertEqual(visual, slot.SpritemapPointer, "Native escape Dachora pose order");
                    AssertEqual(unchecked((ushort)expectedX), slot.Position.X.Whole, "Native callback displacement occurs only at pose completion");
                }
            }
            AssertEqual(program == 0 ? 180 : program == 1 ? 120 : 163, calls, "Exact full native pacing/departure duration");
            EnemyInstructionRunner.Process(arguments);
            expectedX += Word(0xb3eadf);
            AssertEqual(unchecked((ushort)expectedX), slot.Position.X.Whole, "Final movement callback is preserved");
            ushort next = program == 0 ? (ushort)0xe968 : program == 1 ? (ushort)0xe9d4 : (ushort)0xea80;
            AssertEqual(Word(0xb30000 | (next + 2)), slot.SpritemapPointer, "Pacing reversal/maximum speed loop shows the native record's pose");
            AssertEqual(Word(0xb30000 | next), slot.InstructionTimer.RawValue, "Loop reload keeps native cadence");
            AssertEqual(0, guard.ForbiddenReadAttempts, "Escape Dachora mechanics avoid cartridge reads");
        }
        Console.WriteLine("Escape Dachora cadence:139 native records,463 uninterrupted ticks,two complete pacing round trips and full accelerating departure pass.");
    }

    /// <summary>A room enemy system holding one initialized escape Dachora in slot zero.</summary>
    private static RoomEnemySystem CreateEscapeDachoraProgramSystem()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().HasEvent = _ => false;
        RoomEnemySlot dachora = enemies.SlotTable.Slots[0];
        dachora.Bind(EnemyDefinitionId.DachoraEscape, dachora.Definition);
        dachora.Bind(dachora.DefinitionId, default(RoomEnemyDefinition));
        dachora.Position = dachora.Position.WithWhole(new(256, 200));
        enemies.EscapeAnimalsBehavior.InitializeEscapeDachora(dachora);
        return enemies;
    }

    /// <summary>Reads a native escape Dachora timing or movement byte for the cadence oracle.</summary>
    private static byte LookupStream1EscapeDachoraCadenceRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks Powamp body and balloon holds, loops, sleep, and native Y offsets through the live instruction runner.</summary>
    internal static void VerifyLookupStream1PowampCadence(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1PowampCadenceRomByte(rom, address), LookupStream1PowampCadenceRomByte(rom, address + 1));
        foreach (ushort root in new ushort[] { 0xc163, 0xc173, 0xc183, 0xc191 })
        {
            bool isBody = root < 0xc183;
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemySlot> initialize = enemies.PowampBehavior.InitializePowamp;
            var balloon = enemies.SlotTable.Slots[0];
            balloon.Bind(EnemyDefinitionId.Powamp, balloon.Definition);
            balloon.Bind(balloon.DefinitionId, default(RoomEnemyDefinition));
            balloon.SetSpawnParameter1(1);
            initialize(balloon);
            var body = enemies.SlotTable.Slots[1];
            body.Bind(EnemyDefinitionId.Powamp, body.Definition);
            body.Bind(body.DefinitionId, default(RoomEnemyDefinition));
            initialize(body);
            body.Position = body.Position.WithWholeY(100);
            var slot = isBody ? body : balloon;
            PowampEnemyState slotState = slot.RequireSpeciesState<PowampEnemyState>();
            slotState.Program = AnimationProgramCursor<PowampProgram>.Start(root switch
            {
                0xc163 => PowampProgram.BodyFast,
                0xc173 => PowampProgram.BodySlow,
                0xc183 => PowampProgram.BalloonInflate0,
                _ => PowampProgram.BalloonStartSinking,
            });
            var pair = new PowampBalloon(balloon, balloon.RequireSpeciesState<PowampEnemyState>());
            slot.InstructionTimer = WrappingCountdown.Start(1);
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            int calls = 0;
            for (int pose = 0; pose < 3; pose++)
            {
                ushort address = (ushort)(root + pose * 4), duration = Word(0xa80000 | address);
                ushort sprite = Word(0xa80000 | (address + 2));
                for (int held = 0; held < duration; held++)
                {
                    EnemyInstructionRunner.Process(arguments); calls++;
                    AssertEqual((ushort)(duration - held), slot.InstructionTimer.RawValue, "Every uninterrupted Powamp hold tick");
                    AssertEqual(sprite, slot.SpritemapPointer, "Native Powamp held visual");
                    if (!isBody)
                    {
                        PowampBehavior.AlignPowampBalloonY(body, pair);
                        int nativeOffset = unchecked((short)Word(0xa80000 | ((root == 0xc183 ? 0xc277 : 0xc27d) + pose * 2)));
                        AssertEqual((ushort)(100 + nativeOffset), balloon.Position.Y.Whole, "Native pose-driven balloon collision center");
                    }
                }
            }
            AssertEqual(root == 0xc163 ? 15 : root == 0xc173 ? 27 : 167, calls, "Native Powamp sequence duration");
            EnemyInstructionRunner.Process(arguments);
            if (isBody)
            {
                AssertEqual(Word(0xa80000 | (root + 2)), slot.SpritemapPointer, "Actual Powamp loop repeats first pose");
                AssertEqual(Word(0xa80000 | root), slot.InstructionTimer.RawValue, "Powamp loop first hold reload");
            }
            else
            {
                AssertEqual((ushort)0, slot.InstructionTimer.RawValue, "Powamp Sleep follows full terminal hold");
                ushort terminalY = balloon.Position.Y.Whole;
                PowampBehavior.AlignPowampBalloonY(body, pair);
                AssertEqual(terminalY, balloon.Position.Y.Whole, "Sleep preserves terminal balloon collision center");
                EnemyInstructionRunner.Process(arguments);
                AssertEqual(ushort.MaxValue, slot.InstructionTimer.RawValue, "Sleeping Powamp retains native timer underflow");
            }
        }
        Console.WriteLine("Powamp cadence:12 native holds,376 uninterrupted exposure ticks,both Goto/Sleep boundaries and native balloon centers pass.");
    }

    /// <summary>Reads bank-$A8 Powamp timing, artwork, and balloon-offset bytes for comparison.</summary>
    private static byte LookupStream1PowampCadenceRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks Metroid chase/idle pulse holds, loop callbacks, sound, and RNG ownership.</summary>
    internal static void VerifyLookupStream1MetroidPulse(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1MetroidPulseRomByte(rom, address), LookupStream1MetroidPulseRomByte(rom, address + 1));
        foreach (bool chasing in new[] { true, false })
        {
            ushort root = chasing ? (ushort)0xe9cf : (ushort)0xea25;
            int records = chasing ? 20 : 5, randomCalls = 0, calls = 0;
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().NextRandom = () => { randomCalls++; return 5; };
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Metroid, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.InstructionTimer = WrappingCountdown.Start(1);
            enemies.SlotTable.Slots[0].SetSpeciesState(new MetroidEnemyState(new RoomSpriteObjectSlot(0), new RoomSpriteObjectSlot(1))
            {
                Program = AnimationProgramCursor<MetroidProgram>.Start(chasing ? MetroidProgram.ChasingSamus : MetroidProgram.DrainingSamus),
            });
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            for (int record = 0; record < records; record++)
            {
                ushort address = (ushort)(root + record * 4);
                ushort duration = Word(0xa30000 | address), visual = Word(0xa30000 | (address + 2));
                EnemyInstructionRunner.Process(arguments); calls++;
                AssertEqual(duration, slot.InstructionTimer.RawValue, "Actual Metroid duration reload");
                AssertEqual(visual, slot.SpritemapPointer, "Actual native inside-pose identity");
                for (int held = 1; held < duration; held++)
                {
                    EnemyInstructionRunner.Process(arguments); calls++;
                    AssertEqual((ushort)(duration - held), slot.InstructionTimer.RawValue, "Every actual Metroid exposure tick");
                    AssertEqual(visual, slot.SpritemapPointer, "Held Metroid inside pose stays selected");
                }
                AssertEqual(0, randomCalls, "No Metroid RNG call before complete pulse loop");
                AssertTrue(enemies.MetroidBehavior.LastMetroidSoundEffectLibrary2 is null, "No Metroid callback before loop boundary");
            }
            AssertEqual(chasing ? 256 : 64, calls, "Exact native pulse-loop length");
            EnemyInstructionRunner.Process(arguments);
            AssertEqual(chasing ? 1 : 0, randomCalls, "Exact Metroid callback RNG ownership");
            ushort expectedSound = chasing ? Word(0xa3ead6 + 5 * 2) : Word(0xa3eaa8);
            AssertEqual(SoundEffectCommands.DecodeOptional(SoundEffectLibrary.Library2, expectedSound),
                enemies.MetroidBehavior.LastMetroidSoundEffectLibrary2, "Native loop callback sound decodes to the sound queued");
            AssertEqual(Word(0xa30000 | (root + 2)), slot.SpritemapPointer, "Callback loops to first timed record");
            AssertEqual(Word(0xa30000 | root), slot.InstructionTimer.RawValue, "Loop reload preserves first native hold");
        }
        Console.WriteLine("Metroid pulse:25 native records,320 actual exposure ticks,two callback boundaries and exact sound/RNG ownership pass.");
    }

    /// <summary>Reads native Metroid pulse words and callback sound choices.</summary>
    private static byte LookupStream1MetroidPulseRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks all atmospheric animation durations and rejected type/frame boundaries.</summary>
    internal static void VerifyLookupStream1AtmosphericCadence(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1AtmosphericCadenceRomByte(rom, address), LookupStream1AtmosphericCadenceRomByte(rom, address + 1));
        for (byte type = 1; type <= 7; type++)
        {
            int pointer = 0x900000 | Word(0x908b93 + type * 2);
            byte count = (byte)Word(0x908bef + type * 2);
            AssertEqual(count, SamusAtmosphericAnimationDefinitions.FrameCount((SamusAtmosphericEffectType)type), "Named atmospheric frame domain matches native source");
            for (byte frame = 0; frame < count; frame++)
                AssertEqual(Word(pointer + frame * 2), SamusAtmosphericAnimationDefinitions.FrameTimer((SamusAtmosphericEffectType)type, frame), "Calculated atmospheric cadence preserves every native duration");
            AssertThrows<InvalidDataException>(() => SamusAtmosphericAnimationDefinitions.FrameTimer((SamusAtmosphericEffectType)type, count), "Atmospheric first out-of-range frame still rejects");
            AssertThrows<InvalidDataException>(() => SamusAtmosphericAnimationDefinitions.FrameTimer((SamusAtmosphericEffectType)type, byte.MaxValue), "Atmospheric byte-max frame still rejects");
        }
        foreach (byte invalid in new byte[] { 0, 8, byte.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => SamusAtmosphericAnimationDefinitions.FrameCount((SamusAtmosphericEffectType)invalid), "Inactive/out-of-domain atmospheric type rejects");
            AssertThrows<InvalidDataException>(() => SamusAtmosphericAnimationDefinitions.FrameTimer((SamusAtmosphericEffectType)invalid, 0), "Invalid atmospheric type fails before frame selection");
        }
        Suite(nameof(VerifyProductionAtmosphericCadence), () => VerifyProductionAtmosphericCadence());
    }

    /// <summary>Reads bank-$90 atmospheric timer tables for the cadence oracle.</summary>
    private static byte LookupStream1AtmosphericCadenceRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks every 128-frame escape timer correction and the two expiration-boundary frames.</summary>
    internal static void VerifyLookupStream1TimerCadence(ISnesAddressSpace rom)
    {
        var failures = new List<string>();
        foreach (ushort frame in new ushort[] { 124, 125 })
        {
            var timer = new EscapeTimer();
            timer.SelectRunningStatus(EscapeTimerState.RunningInPlace);
            timer.SetTime(PackedDecimal.FromDecimal(0), PackedDecimal.FromDecimal(1), PackedDecimal.FromDecimal(50));
            byte correction = LookupStream1TimerCadenceRomByte(rom, 0x809eec + frame);
            byte expected = correction == 2 ? (byte)0x48 : (byte)0x49;
            bool expired = timer.Process(frame);
            if (timer.Centiseconds.Raw != expected)
                failures.Add("NMI " + frame + ": native decrement" + correction + " expects$" + expected.ToString("X2") + ", actual$" + timer.Centiseconds.Raw.ToString("X2"));
            AssertTrue(!expired && timer.Seconds.Raw == 1 && timer.Minutes.Raw == 0, "Exact cadence fixture preserves non-expiring whole time");
        }
        AssertTrue(failures.Count == 0, string.Join("; ", failures));
        int total = 0;
        for (int index = 0; index < 128; index++)
        {
            byte expected = LookupStream1TimerCadenceRomByte(rom, 0x809eec + index);
            total += expected;
            AssertEqual(expected, EscapeTimerCadenceDefinitions.Centiseconds((ushort)index), "Every native rational cadence sample");
            AssertEqual(expected, EscapeTimerCadenceDefinitions.Centiseconds((ushort)(index + 128)), "Native128-frame wrap");
            AssertEqual(expected, EscapeTimerCadenceDefinitions.Centiseconds((ushort)(index + 0xff80)), "Native ignores high NMI bits");
            var timer = new EscapeTimer();
            timer.SelectRunningStatus(EscapeTimerState.RunningInPlace);
            timer.SetTime(PackedDecimal.FromDecimal(0), PackedDecimal.FromDecimal(1), PackedDecimal.FromDecimal(50));
            AssertTrue(!timer.Process((ushort)index), "Calculated cadence stays non-expiring");
            AssertEqual(expected == 2 ? (byte)0x48 : (byte)0x49, timer.Centiseconds.Raw, "Actual Process consumes each native correction");
        }
        AssertEqual(213, total, "Native full-period centisecond budget");
        foreach (ushort frame in new ushort[] { 124, 125 })
        {
            var timer = new EscapeTimer();
            timer.SelectRunningStatus(EscapeTimerState.RunningInPlace);
            timer.SetTime(PackedDecimal.FromDecimal(0), PackedDecimal.FromDecimal(0), PackedDecimal.FromDecimal(2));
            bool expired = timer.Process(frame);
            AssertEqual(frame == 124, expired, "Corrected native expiration phase");
            AssertEqual(frame == 124 ? (byte)0 : (byte)1, timer.Centiseconds.Raw, "Exact corrected expiration centisecond");
        }
        Console.WriteLine("Timer cadence:128 native corrections,384 phase observations,128 actual countdown updates and exact124/125 expiration pass.");
    }

    /// <summary>Reads the native centisecond correction table in bank $80.</summary>
    private static byte LookupStream1TimerCadenceRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks escape timer glyph geometry, independently edited pixels, and encoded VRAM output.</summary>
    internal static void VerifyLookupStream1TimerGlyphs(ISnesAddressSpace rom)
    {
        byte[] png = EscapeTimerTileAtlasExtractor.Extract(rom);
        var image = IndexedPng.Read(new MemoryStream(png), new(200, 8));
        byte[] native = Enumerable.Range(0, 800).Select(index => LookupStream1TimerGlyphsRomByte(rom, 0xb0c000 + index)).ToArray();
        var stock = EscapeTimerCharacters.Read(new MemoryStream(png));
        HashSet<int> Fill(EscapeTimerCharacters value) => value.fillPixels;
        Dictionary<int, byte> Overrides(EscapeTimerCharacters value) => value.pixelOverrides;
        Dictionary<int, bool> FillOverrides(EscapeTimerCharacters value) => value.fillOverrides;
        bool DefaultFill(int pixel, out bool fill)
        {
            int tile = pixel % 200 / 8, x = pixel % 8, y = pixel / 200 + (tile >= 10 ? 8 : 0);
            if (tile >= 22)
            {
                int labelX = (tile - 22) * 8 + x, labelY = pixel / 200;
                fill = labelX is >= 0 and <= 5 && (labelY == 1 || labelY is >= 2 and <= 5 && labelX is 2 or 3) ||
                    labelX is 7 or 8 && labelY is >= 1 and <= 5 ||
                    labelX is >= 18 and <= 22 && (labelY is 1 or 5 ||
                        labelX is 18 or 19 && labelY is >= 2 and <= 4 || labelY == 3 && labelX <= 21);
                return labelX is <= 5 or 7 or 8 or >= 18 and <= 22;
            }
            fill = x is 3 or 4 && y is >= 1 and <= 13 || x == 2 && y is 2 or 3 || x is >= 2 and <= 5 && y is 12 or 13;
            if (tile is 0 or 10)
            {
                fill = x is >= 1 and <= 6 && y is >= 1 and <= 13 &&
                    !((x == 1 || x == 6) && (y == 1 || y == 13)) &&
                    !(x is >= 3 and <= 4 && y is >= 3 and <= 11);
            }
            if (tile == 8)
            {
                fill = x is >= 1 and <= 6 && y is >= 1 and <= 7 &&
                    !((x == 1 || x == 6) && (y == 1 || y == 7)) &&
                    !(x is >= 3 and <= 4 && y is >= 3 and <= 5);
            }
            if (tile is 2 or 12)
            {
                fill = y == 1 && x is >= 2 and <= 5 ||
                    y == 2 && x is >= 1 and <= 6 ||
                    y is 3 or 4 && x is 1 or 2 or 5 or 6 ||
                    y == 5 && x is 5 or 6 ||
                    y == 6 && x is >= 4 and <= 6 ||
                    y == 7 && x is >= 3 and <= 5 ||
                    y == 8 && x is 3 or 4 ||
                    y == 9 && x is >= 2 and <= 4 ||
                    y == 10 && x is 2 or 3 ||
                    y == 11 && x is >= 1 and <= 3 ||
                    y is 12 or 13 && x is >= 1 and <= 6;
            }
            if (tile is 7 or 17)
            {
                fill = y is 1 or 2 && x is >= 1 and <= 6 || y is >= 10 and <= 13 && x is 2 or 3;
                return y is < 3 or >= 10;
            }
            return tile is 0 or 1 or 2 or 8 or 10 or 11 or 12;
        }
        byte Ink(int pixel) => (byte)(pixel % 200 / 8 < 22 ? 1 : 2);
        int Source(int pixel)
        {
            int tile = pixel % 200 / 8, x = pixel % 8, y = pixel / 200;
            if (tile is 16 or 18) tile = 10;
            if (tile == 10 && y <= 6) { tile = 0; y = 6 - y; }
            if ((tile == 0 || tile == 8) && x >= 4) x = 7 - x;
            if (tile == 21) { tile = 20; if (x >= 4) x -= 3; }
            return y * 200 + tile * 8 + x;
        }
        byte Outline(byte[] pixels, int index)
        {
            int tile = index % 200 / 8, x = index % 8, y = index / 200;
            int gx = tile >= 22 ? (tile - 22) * 8 + x : x, gy = tile is >= 10 and < 20 ? y + 8 : y;
            for (int ny = gy - 1; ny <= gy + 1; ny++)
            for (int nx = gx - 1; nx <= gx + 1; nx++)
            {
                if (nx < 0 || nx >= (tile >= 22 ? 24 : 8) || ny < 0 || ny >= (tile < 20 ? 16 : 8)) continue;
                int selectedTile = tile < 20 ? tile % 10 + ny / 8 * 10 : tile >= 22 ? 22 + nx / 8 : tile;
                int neighbor = ny % 8 * 200 + selectedTile * 8 + nx % 8;
                if (pixels[neighbor] == Ink(neighbor)) return 14;
            }
            return 0;
        }
        byte[] Transfer(EscapeTimerTileAtlas value) => value.Resolve(InstalledArtworkId.EscapeTimerFirstTiles).Span
            .ConcatToArray(value.Resolve(InstalledArtworkId.EscapeTimerSecondTiles).Span);
        AssertTrue(Transfer(stock).SequenceEqual(native), "Both reconstructed timer pages match all800 native planar bytes");
        AssertEqual(275, Fill(stock).Count, "Exact retained timer fill footprint after calculated digit and label shapes");
        AssertEqual(0, FillOverrides(stock).Count, "Zero stock calculated-footprint overrides");
        AssertTrue(Overrides(stock).Keys.ToHashSet().SetEquals(RetainedTimerDeviations), "Exact four still-retained native deviations");
        for (int index = 0; index < 1600; index++)
        {
            bool fill = image.Pixels[index] == Ink(index);
            bool calculated = DefaultFill(index, out bool expectedFill);
            AssertEqual(calculated, EscapeTimerGlyphDefinitions.TryDefaultFill(index, out bool productionFill), "Exact digit/T/I/E footprint domain");
            if (calculated) { AssertEqual(fill, expectedFill, "Direct native three-part digit/T/I/E footprint"); AssertEqual(fill, productionFill, "Direct calculated digit/T/I/E footprint"); }
            AssertEqual(Source(index) == index && !calculated && fill, Fill(stock).Contains(index), "Exact retained stock footprint membership");
            AssertEqual(Source(index), EscapeTimerGlyphDefinitions.SourcePixel(index), "Exact native shared lower-half pixel domain");
            if (Source(index) != index) AssertEqual(image.Pixels[index], image.Pixels[Source(index)], "Direct native repeated lower-half pixel");
            else if (!fill && !Overrides(stock).ContainsKey(index))
                AssertEqual(image.Pixels[index], EscapeTimerGlyphDefinitions.Outline(index, pixel => image.Pixels[pixel] == Ink(pixel)), "Direct native calculated outline/transparency");
            byte[] editedPixels = (byte[])image.Pixels.Clone(); editedPixels[index] ^= 15;
            using var stream = new MemoryStream();
            IndexedPng.Write(stream, new(200, 8), editedPixels, image.Colours); stream.Position = 0;
            var edited = EscapeTimerCharacters.Read(stream);
            for (int other = 0; other < 1600; other++)
            {
                bool selectedFill = editedPixels[other] == Ink(other);
                bool calculatedFill = DefaultFill(other, out bool defaultFill);
                AssertEqual(Source(other) == other && !calculatedFill && selectedFill, Fill(edited).Contains(other), "Every independently supplied retained footprint edit preserved");
                AssertEqual(Source(other) == other && calculatedFill && selectedFill != defaultFill, FillOverrides(edited).ContainsKey(other), "Exact independently supplied calculated-footprint override membership");
                bool expectedOverride = Source(other) != other ? editedPixels[other] != editedPixels[Source(other)] : !selectedFill && editedPixels[other] != Outline(editedPixels, other);
                AssertEqual(expectedOverride, Overrides(edited).ContainsKey(other), "Exact independent outline/shared-pixel deviation membership");
            }
            byte[] expected = (byte[])native.Clone();
            int tile = index % 200 / 8, x = index % 8, y = index / 200;
            for (int plane = 0; plane < 4; plane++) expected[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] ^= (byte)(1 << (7 - x));
            AssertTrue(Transfer(edited).SequenceEqual(expected), "Every PNG pixel edit changes exactly its four native planar bits");
        }
        var queue = new VramWriteQueue(); stock.QueueTo(queue);
        AssertEqual(2, queue.Entries.Count, "Original two-page queue count");
        AssertEqual(512, queue.Entries[0].SizeInBytes, "First page size");
        AssertEqual((ushort)0x7e00, queue.Entries[0].Destination.Encoded, "First page destination");
        AssertEqual(InstalledArtworkId.EscapeTimerFirstTiles, queue.Entries[0].AssetId, "First typed page identity");
        AssertEqual(288, queue.Entries[1].SizeInBytes, "Second page size");
        AssertEqual((ushort)0x7f00, queue.Entries[1].Destination.Encoded, "Second page destination");
        AssertEqual(InstalledArtworkId.EscapeTimerSecondTiles, queue.Entries[1].AssetId, "Second typed page identity");
        var uploads = new SpriteArtworkPlacements();
        AssertTrue(stock.TryRecordNativeTransfer(uploads, 0xb0c000, 512, 0x7e00) && stock.TryRecordNativeTransfer(uploads, 0xb0c200, 288, 0x7f00), "Both synchronous native uploads accepted");
        AssertTrue(uploads.Uploads.Select(upload => upload.FirstByte).SequenceEqual([0x3c00, 0x3e00]) &&
            ((byte[])[.. stock.Resolve(InstalledArtworkId.EscapeTimerFirstTiles).Span, .. stock.Resolve(InstalledArtworkId.EscapeTimerSecondTiles).Span]).SequenceEqual(native),
            "The two synchronous pages show the native bytes where they land in the OBJ characters");
        AssertTrue(stock.TryResolve(0xb0c000, 512, out var first) && first.Span.SequenceEqual(native.AsSpan(0, 512)), "First restored native page resolution");
        AssertTrue(stock.TryResolve(0xb0c200, 288, out var second) && second.Span.SequenceEqual(native.AsSpan(512)), "Second restored native page resolution");
        AssertTrue(!stock.TryResolve(0xb0c000, 511, out _), "Mismatched native page rejected");
        AssertTrue(!stock.TryRecordNativeTransfer(new SpriteArtworkPlacements(), 0xb0c200, 288, 0x7e00), "Mismatched synchronous destination rejected");
        AssertTrue(Transfer(stock).SequenceEqual(native), "Stock timer remains immutable");
        Console.WriteLine("Timer font:836 direct outline pixels,312 shared/reflected pixels,275 retained fill positions/four deviations,173 calculated digit/T/I/E basis fill sites,1600 independent PNG edits,800 native planar bytes and exact two-page queue/VRAM uploads pass.");
    }

    /// <summary>Reads native escape timer glyph bytes for pixel and transfer comparison.</summary>
    private static byte LookupStream1TimerGlyphsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks X-Ray pose/frame selection and independent edited Samus body components against native data.</summary>
    internal static void VerifyLookupStream1XrayBodyFrames(ISnesAddressSpace rom, bool basisOnly = false, bool finalOnly = false)
    {
        int count = SamusBodyArtworkCatalog.FrameCount * 4;
        byte[] native = Enumerable.Range(0, count).Select(index => LookupStream1XrayBodyFramesRomByte(rom, 0x92db48 + index)).ToArray();
        ushort[] Words(int address, int length) => Enumerable.Range(0, length).Select(index =>
            LittleEndianBytes.Word(LookupStream1XrayBodyFramesRomByte(rom, address + index * 2), LookupStream1XrayBodyFramesRomByte(rom, address + index * 2 + 1))).ToArray();
        ushort[] top = Words(0x92d91e, 13), bottom = Words(0x92d938, 11), poses = Words(0x92d94e, 253);
        ushort[] starts = top.Concat(bottom).Order().ToArray();
        SamusBodyTileDefinition[][] Groups(ushort[] pointers) => pointers.Select(pointer =>
        {
            int next = Array.IndexOf(starts, pointer) + 1;
            int end = next == starts.Length ? 0xd7d3 : starts[next];
            return Enumerable.Range(0, (end - pointer) / 7).Select(_ => new SamusBodyTileDefinition(32, 0, new byte[32])).ToArray();
        }).ToArray();
        var upper = Groups(top); var lower = Groups(bottom);
        var template = CreateSamusIdentityFixture();
        SamusBodyFrameSelection[] Decode(byte[] data) => Enumerable.Range(0, count / 4).Select(index =>
            new SamusBodyFrameSelection(data[index * 4], data[index * 4 + 1], data[index * 4 + 2], data[index * 4 + 3])).ToArray();
        SamusBodyArtworkCatalog Create(byte[] data) => new(new SamusBodyArtworkParts { TopSetStarts = SamusBodyNative.Records(top), BottomSetStarts = SamusBodyNative.Records(bottom), PoseFirstFrames = SamusBodyNative.Frames(poses), GraphicsYOffsets = template.GraphicsYOffsets.ToArray(), Frames = Decode(data), Top = upper, Bottom = lower, Spritemaps = template.Spritemaps, Atmosphere = template.Atmosphere, DeathPalettes = template.DeathPalettes, DeathTilesContentIdentity = template.DeathTilesContentIdentity, ArmCannon = template.ArmCannon, LandingYOffsets = template.LandingYOffsets.ToArray(), PostureYOffsets = template.PostureYOffsets.ToArray(), DrainedYOffsets = template.DrainedYOffsets.ToArray() });
        int SourceStep(int index)
        {
            int address = 0xdb48 + index;
            if (address is >= 0xe888 and < 0xe890) return index - 8;
            if (address is >= 0xe298 and < 0xe2b8) return index - 36;
            if (address is >= 0xe2b8 and < 0xe374)
            {
                int offset = address - 0xe2b8, phase = offset / 4, component = offset % 4, basis = phase;
                if (component < 2 && phase is >= 13 and <= 20) basis = 13;
                if (phase >= 23)
                {
                    int relative = phase - 23;
                    basis = component >= 2 ? 23 + relative % 8 : relative / 3 % 2 == 1 ? 13 : 23 + relative / 3 * 3;
                }
                if (basis != phase) return index - (phase - basis) * 4;
            }
            if (address is >= 0xe050 and < 0xe260)
            {
                int start = address < 0xe158 ? 0xe050 : 0xe158, offset = address - start, phase = offset / 4;
                if (offset % 4 >= 2 && phase < 64)
                {
                    int within = phase % 16, basis = within switch { 4 => 3, 6 => 5, 8 or 9 => 7, 11 => 10, 13 => 12, _ => within };
                    return start - 0xdb48 + (phase - within + basis) * 4 + offset % 4;
                }
            }
            if (address >= 0xdf28 && address < 0xe018 && address % 4 < 2)
            {
                int offset = (address - 0xdf28) % 40, phase = offset / 4 % 5;
                int basis = phase is 0 or 1 ? 0 : phase == 4 ? 2 : phase;
                return address - offset - 0xdb48 + basis * 4 + offset % 4;
            }
            if (address is >= 0xdce8 and < 0xdd18 or >= 0xe450 and < 0xe4b0)
            {
                int range = address < 0xdd18 ? 0xdce8 : 0xe450, offset = (address - range) % 24;
                if (offset % 4 < 2) return address - offset - 0xdb48 + (offset / 4 % 3 == 0 ? 0 : 1) * 4 + offset % 4;
                if (range == 0xe450) return 0xdd00 - 0xdb48 + offset;
            }
            if (address is >= 0xe890 and < 0xe908)
            {
                int offset = (address - 0xe890) % 60, phase = offset / 4, component = offset % 4;
                int basis = phase switch { 5 or 10 or 11 => 4, 6 => 2, 9 => 7, 12 => 1, _ => phase };
                if (component >= 2 && (phase == 3 || phase is >= 6 and <= 9)) basis = 2;
                if (phase == 13) basis = component < 2 ? 1 : 0;
                if (basis != phase) return address - offset - 0xdb48 + basis * 4 + component;
            }
            if (address is >= 0xe938 and < 0xe9f4)
            {
                bool left = address >= 0xe974;
                int start = left ? 0xe974 : 0xe938, offset = address - start, phase = offset / 4, component = offset % 4;
                int basis = left ? phase switch
                {
                    >= 3 and <= 6 => 2, 11 => 9, 13 or 17 or 18 or 24 or 25 or 27 or 28 or 30 or 31 => 12,
                    >= 19 and <= 23 => 14 + Math.Min(phase - 19, 23 - phase), 26 or 29 => 8, _ => phase
                } : phase switch { >= 4 and <= 7 => 3, 11 => 9, 13 => 12, _ => phase };
                if (component >= 2 && phase is >= 9 and <= 11) basis = 8;
                if (basis != phase) return start - 0xdb48 + basis * 4 + component;
            }
            if (address >= 0xe9f4 && address < 0xea24 && (address - 0xe9f4) % 24 < 16)
            {
                int offset = (address - 0xe9f4) % 24, phase = offset / 4;
                return address - offset - 0xdb48 + (offset % 4 >= 2 ? 0 : phase == 3 ? 1 : phase) * 4 + offset % 4;
            }
            if (address is >= 0xe050 and < 0xe260)
            {
                int start = address < 0xe158 ? 0xe050 : 0xe158, offset = address - start, phase = offset / 4;
                if (offset % 4 < 2 && phase >= 32)
                    return start - 0xdb48 + (phase < 64 ? phase % 32 : 16) * 4 + offset % 4;
            }
            if (address is >= 0xe798 and < 0xe828)
            {
                int pair = (address - 0xe798) / 24, offset = (address - 0xe798) % 24;
                if (offset >= 12) return 0xe798 + pair * 24 + (2 - (offset - 12) / 4) * 4 + offset % 4 - 0xdb48;
                if (pair is 1 or 2 && offset % 4 >= 2) return 0xe798 - 0xdb48 + offset;
                if (pair >= 3 && offset % 4 < 2) return 0xe798 + (pair - 3) * 24 - 0xdb48 + offset;
                if (pair >= 4 && offset % 4 >= 2) return 0xe7e0 - 0xdb48 + offset;
            }
            if (address >= 0xea24)
            {
                bool suited = address >= 0xeba4;
                int start = suited ? 0xeba4 : 0xea24, offset = address - start, phase = offset / 4, component = offset % 4;
                if (suited && phase >= 1 && component < 2) return 0xea24 - 0xdb48 + offset;
                if (phase >= 2 && component >= 2) return start - 0xdb48 + 8 + component;
                if (suited && phase == 1) return 0xea24 - 0xdb48 + offset;
                if (!suited && phase >= 3 && component < 2)
                {
                    int basis = phase % 2 == 1 ? 3 : phase < 80 ? 2 + (phase - 2) % 6 : phase >= 90 ? phase - 6 : phase;
                    return 0xea24 - 0xdb48 + basis * 4 + component;
                }
            }
            if (address is >= 0xe37c and < 0xe430) return index - 188;
            if (address is >= 0xe530 and < 0xe5f8)
            {
                int list = (address - 0xe508) / 40, offset = (address - 0xe508) % 40;
                int phase = offset / 4;
                return 0xe508 - 0xdb48 + ((list % 2 == 1 && phase < 8) ? 7 - phase : phase) * 4 + offset % 4;
            }
            if (address is >= 0xe4c8 and < 0xe4d8)
            {
                int offset = (address - 0xe4c8) % 8;
                return 0xe4b8 - 0xdb48 + (address - 0xe4c8) / 8 * 8 + (1 - offset / 4) * 4 + offset % 4;
            }
            if (address is >= 0xe4f0 and < 0xe508)
            {
                int offset = (address - 0xe4f0) % 12;
                return 0xe4d8 - 0xdb48 + (address - 0xe4f0) / 12 * 12 + (2 - offset / 4) * 4 + offset % 4;
            }
            if (address is >= 0xe5f8 and < 0xe798)
            {
                int list = address < 0xe6b8 ? (address - 0xe5f8) / 48 : 4 + (address - 0xe6b8) / 112;
                int offset = address < 0xe6b8 ? (address - 0xe5f8) % 48 : (address - 0xe6b8) % 112;
                int phase = offset / 4, wall = list % 2 == 0 ? 0xe2b8 : 0xe374;
                int last = list < 4 ? 11 : 27;
                if (phase == last)
                {
                    if (list >= 2) return 0xe5f8 + list % 2 * 48 + 44 - 0xdb48 + offset % 4;
                }
                else
                {
                    int wallPhase = phase == 0 ? 1 : list < 2 ? phase + 2 : list < 4 ? phase + 12 : phase < 25 ? phase + 22 : phase - 4;
                    return wall - 0xdb48 + wallPhase * 4 + offset % 4;
                }
            }
            if (address is >= 0xdb48 and < 0xdb90)
            {
                int offset = (address - 0xdb48) % 36;
                int phase = offset / 4;
                int first = address - offset;
                int basis = phase switch { 3 or 6 or 8 => 1, 5 => 0, 7 when offset % 4 >= 2 => 2, _ => phase };
                return first - 0xdb48 + basis * 4 + offset % 4;
            }
            if (address >= 0xdba0 && address < 0xdbb8 && address % 4 < 2)
            {
                int side = (address - 0xdba0) / 12, phase = (address - 0xdba0) % 12 / 4;
                return 0xdb90 + side * 8 - 0xdb48 + (1 - phase % 2) * 4 + address % 4;
            }
            if (address >= 0xdbb8 && address < 0xdbf8 && address % 4 >= 2)
                return 0xdb90 + (address - 0xdbb8) / 8 % 2 * 8 - 0xdb48 + (address - 0xdbb8) % 8;
            if (address >= 0xdbf8 && address < 0xdc48 && address % 4 >= 2)
                return 0xdbf8 + (address - 0xdbf8) / 20 * 20 - 0xdb48 + address % 4;
            if (address >= 0xdc20 && address < 0xdc48 && (address - 0xdc20) % 4 < 2) return index - 40;
            if (address >= 0xdc70 && address < 0xdce8 && (address - 0xdc48) % 4 >= 2) return 0xdc48 - 0xdb48 + (address - 0xdc48) % 40;
            if (address >= 0xdf28 && address < 0xe018 && (address - 0xdf28) % 4 >= 2) return 0xdc48 - 0xdb48 + (address - 0xdf28) % 40;
            if (address >= 0xde18 && address < 0xde60 && (address - 0xde18) % 4 < 2) return address - 0xde18;
            if (address is >= 0xdd48 and < 0xdd58) return 0xdd28 - 0xdb48 + address - 0xdd48;
            if (address is >= 0xdeb0 and < 0xdec0) return 0xdd18 - 0xdb48 + address - 0xdeb0;
            return index;
        }
        int Source(int index)
        {
            int source = SourceStep(index);
            while (source != index) { index = source; source = SourceStep(index); }
            return source;
        }
        bool Direct(int index)
        {
            int address = 0xdb48 + index;
            int record = address & ~3;
            if (record is 0xdb58 or 0xdb7c or 0xde28 or 0xde4c or 0xe2c0 or 0xe2e4 or 0xe2e8 or 0xe30c or 0xe310 or
                0xe8a0 or 0xe8dc or 0xe968 or 0xe9a4 or 0xea04 or 0xea1c or 0xe528 or 0xea28 or 0xeba8) return true;            return address % 4 < 2 && (address >= 0xe050 && address < 0xe0d0 || address >= 0xe158 && address < 0xe1d8);
        }
        Dictionary<int, byte> Stored(SamusBodyArtworkCatalog value) =>
            value.frames;
        var stock = Create(native);
        AssertEqual(Enumerable.Range(0, count).Count(index => Source(index) == index && !Direct(index)), Stored(stock).Count, "Exact native shared-component basis");
        for (int index = 0; index < count; index++)
        {
            AssertEqual(Source(index), SamusBodyFrameDefinitions.SourceComponent(index), "Exact native crouching/standing component relation domain");
            AssertEqual(native[index], native[Source(index)], "Direct native source relationship");
            AssertEqual(Direct(index), SamusBodyFrameDefinitions.TryComponent(index, out int calculated), "Exact direct angular-allocation domain");
            if (Direct(index)) AssertEqual(native[index], calculated, "Direct native angular allocation set/position");
            AssertEqual(Source(index) == index && !Direct(index), Stored(stock).ContainsKey(index), "Exact required source-key membership without stock fallbacks");
        }
        AssertTrue(stock.Frames.SequenceEqual(Decode(native)), "All supplied frame components preserved");
        AssertEqual(CanonicalBodyHash(template, top, bottom, poses, Decode(native), upper, lower), stock.ContentIdentity, "Original canonical frame hash");
        Directory.CreateDirectory("csharp/test-temp");
        File.WriteAllLines("csharp/test-temp/samus-body-frame-required.csv", BodyFrameCsvHeader
            .Concat(Stored(stock).OrderBy(pair => pair.Key).Select(pair => $"{pair.Key:X4},{0x92db48 + pair.Key:X6},{pair.Value:X2}")));
        if (basisOnly)
        {
            Console.WriteLine($"Body frame basis:{count} native components,{count - Stored(stock).Count} calculated/aliased components,{Stored(stock).Count} independent source components,zero stock alias fallbacks/canonical hash pass.");
            return;
        }
        IEnumerable<int> editAddresses = Enumerable.Range(0xdbf8, 240).Concat(Enumerable.Range(0xdf28, 240))
            .Concat(Enumerable.Range(0xdb48, 176)).Concat(Enumerable.Range(0xde18, 72))
            .Concat(Enumerable.Range(0xdd18, 32)).Concat(Enumerable.Range(0xdd48, 16)).Concat(Enumerable.Range(0xdeb0, 16))
            .Concat(BodyFrameRecordAddresses.SelectMany(address => Enumerable.Range(address, 4)));
        if (finalOnly)
            editAddresses = FinalBodyFrameEditAddresses.SelectMany(address => Enumerable.Range(address, 4));
        int edits = 0;
        foreach (int address in editAddresses)
        {
            edits++;
            byte[] supplied = (byte[])native.Clone(); supplied[address - 0xdb48] ^= 1;
            var edited = Create(supplied);
            AssertTrue(edited.Frames.SequenceEqual(Decode(supplied)), "Independent source/derived/lower component edits preserved");
            for (int index = 0; index < count; index++)
                AssertEqual(Source(index) != index ? supplied[index] != supplied[Source(index)] : !Direct(index) || supplied[index] != native[index], Stored(edited).ContainsKey(index), "Exact required basis and independent exception membership");
            foreach (SamusPoseId pose in new[]
            {
                SamusPoseId.XrayingStandingRightPose, SamusPoseId.XrayingStandingLeftPose,
                SamusPoseId.XrayingCrouchingRightPose, SamusPoseId.XrayingCrouchingLeftPose
            })
            for (ushort frame = 0; frame < 5; frame++)
                AssertEqual(Decode(supplied)[(poses[(int)pose] - 0xdb48) / 4 + frame], edited.Frame(pose, frame), "Actual four X-ray pose selections resolve independent supplied components");
            foreach (SamusPoseId pose in new[]
            {
                SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose,
                SamusPoseId.MovingRightGunExtendedPose, SamusPoseId.MovingLeftGunExtendedPose,
                SamusPoseId.RunningAimUpRightPose, SamusPoseId.RunningAimUpLeftPose,
                SamusPoseId.RunningAimDiagonalUpRightPose, SamusPoseId.RunningAimDiagonalUpLeftPose,
                SamusPoseId.RunningAimDiagonalDownRightPose, SamusPoseId.RunningAimDiagonalDownLeftPose
            })
            for (ushort frame = 0; frame < 10; frame++)
                AssertEqual(Decode(supplied)[(poses[(int)pose] - 0xdb48) / 4 + frame], edited.Frame(pose, frame), "Actual ten moving poses resolve independent supplied lower gait and upper components");
            foreach (SamusPoseId pose in new[]
            {
                SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
                SamusPoseId.CrouchingRightPose, SamusPoseId.CrouchingLeftPose
            })
            for (ushort frame = 0; frame < 9; frame++)
                AssertEqual(Decode(supplied)[(poses[(int)pose] - 0xdb48) / 4 + frame], edited.Frame(pose, frame), "Actual standing/crouching normal upper selection");
            foreach (SamusPoseId pose in new[]
            {
                SamusPoseId.NormalJumpGunExtendedRightPose, SamusPoseId.NormalJumpGunExtendedLeftPose,
                SamusPoseId.NormalJumpForwardRightPose, SamusPoseId.NormalJumpForwardLeftPose,
                SamusPoseId.NormalJumpAimDownRightPose, SamusPoseId.NormalJumpAimDownLeftPose,
                SamusPoseId.FallingAimDownRightPose, SamusPoseId.FallingAimDownLeftPose
            })
            for (ushort frame = 0; frame < 2; frame++)
                AssertEqual(Decode(supplied)[(poses[(int)pose] - 0xdb48) / 4 + frame], edited.Frame(pose, frame), "Actual stationary/forward jump and downward-aim jump/fall selection");
            AssertEqual(CanonicalBodyHash(template, top, bottom, poses, Decode(supplied), upper, lower), edited.ContentIdentity, "Original canonical edited frame hash");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyFrameDefinitions.SourceComponent(-1), "Negative component rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyFrameDefinitions.SourceComponent(count), "Component end rejected");
        AssertThrows<InvalidDataException>(() => stock.Frame((SamusPoseId)0xfd, 0), "Original pose bound preserved");
        Console.WriteLine($"Body frames:{count - Stored(stock).Count} calculated/aliased components, exact remaining basis,{edits} independent component edits, actual pose selection and original canonical hashes pass.");
    }

    /// <summary>Reads native X-Ray body selection words for the pose-frame oracle.</summary>
    private static byte LookupStream1XrayBodyFramesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks each body pose's upper/lower frame pointers and canonical edit identity.</summary>
    internal static void VerifyLookupStream1BodyPosePointers(ISnesAddressSpace rom)
    {
        ushort[] native = Enumerable.Range(0, 253).Select(pose => LittleEndianBytes.Word(LookupStream1BodyPosePointersRomByte(rom, 0x92d94e + pose * 2), LookupStream1BodyPosePointersRomByte(rom, 0x92d94f + pose * 2))).ToArray();
        SamusBodyArtworkCatalog template = CreateSamusIdentityFixture();
        ushort[] top = template.TopSetPointers.ToArray(), bottom = template.BottomSetPointers.ToArray();
        var topGroups = Enumerable.Range(0, 13).Select(set => template.TopSet(set).ToArray()).ToArray();
        var bottomGroups = Enumerable.Range(0, 11).Select(set => template.BottomSet(set).ToArray()).ToArray();
        var frames = Enumerable.Range(0, SamusBodyArtworkCatalog.FrameCount)
            .Select(index => new SamusBodyFrameSelection((byte)(index % 13), 0, (byte)(index % 11), 0)).ToArray();
        SamusBodyArtworkCatalog Create(ushort[] selected) => new(new SamusBodyArtworkParts { TopSetStarts = SamusBodyNative.Records(top), BottomSetStarts = SamusBodyNative.Records(bottom), PoseFirstFrames = SamusBodyNative.Frames(selected), GraphicsYOffsets = template.GraphicsYOffsets.ToArray(), Frames = frames, Top = topGroups, Bottom = bottomGroups, Spritemaps = template.Spritemaps, Atmosphere = template.Atmosphere, DeathPalettes = template.DeathPalettes, DeathTilesContentIdentity = template.DeathTilesContentIdentity, ArmCannon = template.ArmCannon, LandingYOffsets = template.LandingYOffsets.ToArray(), PostureYOffsets = template.PostureYOffsets.ToArray(), DrainedYOffsets = template.DrainedYOffsets.ToArray() });
        SamusBodyArtworkCatalog stock = Create(native);
        int Count(SamusBodyArtworkCatalog value) => value.poseFirstFrames.Count;
        AssertEqual(0, Count(stock), "No stock pose-pointer override survives");
        AssertEqual(CanonicalBodyHash(template, top, bottom, native, frames, topGroups, bottomGroups), stock.ContentIdentity, "Exact old canonical stock pose hash");
        AssertTrue(stock.Frames.SequenceEqual(frames), "Independent frame payload remains intact");
        for (int pose = 0; pose < 253; pose++)
        {
            AssertEqual(native[pose], SamusBodyNative.FramePointer(SamusBodyPoseDefinitions.DefaultFirstFrame((SamusPoseId)pose)), "Named semantic pose dispatch matches native list identity");
            int index = (native[pose] - 0xdb48) / 4;
            AssertEqual(frames[index], stock.Frame((SamusPoseId)pose, 0), "Runtime selects supplied frame payload through native default");
            ushort[] selected = (ushort[])native.Clone();
            selected[pose] = native[pose] + 4 < 0xed24 ? (ushort)(native[pose] + 4) : (ushort)0xdb48;
            SamusBodyArtworkCatalog changed = Create(selected);
            AssertEqual(1, Count(changed), "Exactly one independent valid pose-pointer edit");
            AssertTrue(changed.PosePointers.SequenceEqual(selected), "All supplied pose-pointer identities preserved");
            AssertTrue(changed.Frames.SequenceEqual(frames), "Pointer edit never changes independent frame records");
            AssertEqual(frames[(selected[pose] - 0xdb48) / 4], changed.Frame((SamusPoseId)pose, 0), "Runtime honors edited frame-list identity");
            AssertEqual(CanonicalBodyHash(template, top, bottom, selected, frames, topGroups, bottomGroups), changed.ContentIdentity, "Exact old canonical edited pose hash");
            AssertTrue(changed.ContentIdentity != stock.ContentIdentity, "Independent pose edit changes hash");
        }
        foreach (SamusPoseId pose in new[] { (SamusPoseId)0xfd, (SamusPoseId)0xfe, (SamusPoseId)0xff })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyPoseDefinitions.DefaultFirstFrame(pose), "Default pose dispatch excludes adjacent data");
            AssertThrows<InvalidDataException>(() => stock.Frame(pose, 0), "Installed pose domain stays bounded");
        }
        ushort[] invalid = (ushort[])native.Clone(); invalid[0] = SamusBodyNative.FramePointer(SamusBodyArtworkCatalog.FrameCount);
        AssertThrows<InvalidDataException>(() => Create(invalid), "Edited pose frame list retains bounds validation");
        AssertTrue(stock.PosePointers.SequenceEqual(native), "Stock pose identities immutable");
        Console.WriteLine("Body pose pointers:253 native semantic cases,156 list identities, zero stock overrides,253 independent edits, supplied frame resolution and exact canonical hashes pass.");
    }

    /// <summary>Reads native Samus body pose pointer bytes from the cartridge table.</summary>
    private static byte LookupStream1BodyPosePointersRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Recreates the content hash from sorted body allocation, pose, frame, and tile data.</summary>
    private static string CanonicalBodyHash(SamusBodyArtworkCatalog template, ushort[] upper, ushort[] lower, ushort[] posePointers, SamusBodyFrameSelection[] frames, SamusBodyTileDefinition[][] upperGroups, SamusBodyTileDefinition[][] lowerGroups) =>
        SelectedPresentationHash.Create("samus-body-v2", content =>
        {
            foreach (int start in SamusBodyNative.Records(upper)) content.Append("top set start", start);
            foreach (int start in SamusBodyNative.Records(lower)) content.Append("bottom set start", start);
            foreach (int firstFrame in SamusBodyNative.Frames(posePointers)) content.Append("pose first frame", firstFrame);
            content.Append("graphics y offsets", template.GraphicsYOffsets.SelectToArray(value => unchecked((byte)value)));
            content.AppendWords("landing y offsets", template.LandingYOffsets);
            content.Append("posture y offsets", template.PostureYOffsets.SelectToArray(value => unchecked((byte)value)));
            content.Append("drained y offsets", template.DrainedYOffsets.SelectToArray(value => unchecked((byte)value)));
            foreach (SamusBodyFrameSelection frame in frames)
            {
                content.Append("top set", frame.TopSet); content.Append("top position", frame.TopPosition);
                content.Append("bottom set", frame.BottomSet); content.Append("bottom position", frame.BottomPosition);
            }
            var definitions = upper.SelectMany((pointer, set) => upperGroups[set].Select((definition, position) => new LookupStream1Definition(Address:SamusBodyNative.Record(pointer) + position, Definition:definition)))
                .Concat(lower.SelectMany((pointer, set) => lowerGroups[set].Select((definition, position) => new LookupStream1Definition(Address:SamusBodyNative.Record(pointer) + position, Definition:definition))));
            foreach (var item in definitions.OrderBy(item => item.Address))
            {
                content.Append("definition record", item.Address);
                content.Append("first transfer size", item.Definition.FirstSize); content.Append("second transfer size", item.Definition.SecondSize);
                content.Append("characters", item.Definition.Planar.Span);
            }
            content.Append("spritemaps", Convert.FromHexString(template.Spritemaps.ContentIdentity));
            content.Append("atmosphere", Convert.FromHexString(template.Atmosphere.ContentIdentity));
            content.Append("death palettes", Convert.FromHexString(template.DeathPalettes.ContentIdentity));
            content.Append("death tiles", Convert.FromHexString(template.DeathTilesContentIdentity));
            content.Append("arm cannon", Convert.FromHexString(template.ArmCannon.ContentIdentity));
        });

    /// <summary>Checks body tile-set allocation roots and independently edited pointer identities.</summary>
    internal static void VerifyLookupStream1BodySetPointers(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1BodySetPointersRomByte(rom, address), LookupStream1BodySetPointersRomByte(rom, address + 1));
        ushort[] top = Enumerable.Range(0, 13).Select(set => Word(0x92d91e + set * 2)).ToArray();
        ushort[] bottom = Enumerable.Range(0, 11).Select(set => Word(0x92d938 + set * 2)).ToArray();
        var physical = top.Select((pointer, set) => new Physical(Upper:true, Set:set, Pointer:pointer))
            .Concat(bottom.Select((pointer, set) => new Physical(Upper:false, Set:set, Pointer:pointer))).OrderBy(value => value.Pointer).ToArray();
        SamusBodyArtworkCatalog template = CreateSamusIdentityFixture();
        SamusBodyTileDefinition sample = template.TopSet(0)[0];
        var topGroups = new SamusBodyTileDefinition[13][]; var bottomGroups = new SamusBodyTileDefinition[11][];
        for (int order = 0; order < physical.Length; order++)
        {
            var item = physical[order]; int end = order + 1 == physical.Length ? 0xd7d3 : physical[order + 1].Pointer;
            (item.Upper ? topGroups : bottomGroups)[item.Set] = Enumerable.Repeat(sample, (end - item.Pointer) / 7).ToArray();
        }
        SamusBodyArtworkCatalog Create(ushort[] upper, ushort[] lower, SamusBodyTileDefinition[][] upperGroups, SamusBodyTileDefinition[][] lowerGroups) => new(new SamusBodyArtworkParts { TopSetStarts = SamusBodyNative.Records(upper), BottomSetStarts = SamusBodyNative.Records(lower), PoseFirstFrames = SamusBodyNative.Frames(template.PosePointers.ToArray()), GraphicsYOffsets = template.GraphicsYOffsets.ToArray(), Frames = template.Frames.ToArray(), Top = upperGroups, Bottom = lowerGroups, Spritemaps = template.Spritemaps, Atmosphere = template.Atmosphere, DeathPalettes = template.DeathPalettes, DeathTilesContentIdentity = template.DeathTilesContentIdentity, ArmCannon = template.ArmCannon, LandingYOffsets = template.LandingYOffsets.ToArray(), PostureYOffsets = template.PostureYOffsets.ToArray(), DrainedYOffsets = template.DrainedYOffsets.ToArray() });
        // Independent oracle for the pre-conversion canonical serialization order (520c810c4).
        string Canonical(ushort[] upper, ushort[] lower, SamusBodyTileDefinition[][] upperGroups, SamusBodyTileDefinition[][] lowerGroups) =>
            CanonicalBodyHash(template, upper, lower, template.PosePointers.ToArray(), template.Frames.ToArray(), upperGroups, lowerGroups);
        SamusBodyArtworkCatalog stock = Create(top, bottom, topGroups, bottomGroups);
        AssertEqual(0, stock.topSetStarts.Count, "Zero stock top pointer overrides");
        AssertEqual(0, stock.bottomSetStarts.Count, "Zero stock bottom pointer overrides");
        for (int set = 0; set < 13; set++) AssertEqual(top[set], SamusBodyNative.RecordPointer(SamusBodyDefinitionLayout.DefaultTopStart(set)), "Named top allocation matches original ROM");
        for (int set = 0; set < 11; set++) AssertEqual(bottom[set], SamusBodyNative.RecordPointer(SamusBodyDefinitionLayout.DefaultBottomStart(set)), "Named bottom allocation matches original ROM");
        AssertEqual(Canonical(top, bottom, topGroups, bottomGroups), stock.ContentIdentity, "Exact pre-conversion canonical stock hash");
        for (int edit = 0; edit < physical.Length; edit++)
        {
            var item = physical[edit]; ushort[] upper = (ushort[])top.Clone(), lower = (ushort[])bottom.Clone();
            var upperGroups = topGroups.Select(group => group.ToArray()).ToArray(); var lowerGroups = bottomGroups.Select(group => group.ToArray()).ToArray();
            (item.Upper ? upper : lower)[item.Set] += 7;
            (item.Upper ? upperGroups : lowerGroups)[item.Set] = (item.Upper ? upperGroups : lowerGroups)[item.Set][1..];
            if (edit > 0)
            {
                var previous = physical[edit - 1];
                var groups = previous.Upper ? upperGroups : lowerGroups;
                groups[previous.Set] = groups[previous.Set].Append(sample).ToArray();
            }
            SamusBodyArtworkCatalog changed = Create(upper, lower, upperGroups, lowerGroups);
            AssertEqual(1, changed.topSetStarts.Count + changed.bottomSetStarts.Count, "Exactly one valid independent pointer override");
            AssertTrue(changed.TopSetPointers.SequenceEqual(upper) && changed.BottomSetPointers.SequenceEqual(lower), "Independent selected allocation identities preserved");
            AssertEqual(0x920000 | (item.Pointer + 7), changed.DefinitionAddress(item.Upper ? SamusBodyHalf.Top : SamusBodyHalf.Bottom, (byte)item.Set, 0), "Runtime definition selection honors arbitrary valid edited pointer");
            AssertEqual(Canonical(upper, lower, upperGroups, lowerGroups), changed.ContentIdentity, "Exact pre-conversion canonical edited hash");
            AssertTrue(changed.ContentIdentity != stock.ContentIdentity, "Each allocation edit changes identity");
        }
        AssertTrue(stock.TopSetPointers.SequenceEqual(top) && stock.BottomSetPointers.SequenceEqual(bottom), "Stock pointer snapshots remain immutable");
        AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyDefinitionLayout.DefaultTopStart(13), "Top allocation selector bound");
        AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyDefinitionLayout.DefaultBottomStart(11), "Bottom allocation selector bound");
        AssertThrows<InvalidDataException>(() => stock.GetDefinition(true, 13, 0), "Installed top selector retains rejection");
        AssertThrows<InvalidDataException>(() => stock.GetDefinition(false, 11, 0), "Installed bottom selector retains rejection");
        Console.WriteLine("Body allocation pointers:24 native semantic identities, zero stock overrides,24 valid independent allocation edits, runtime addresses and exact canonical hashes pass.");
    }

    /// <summary>Reads native upper and lower body tile-set pointer bytes.</summary>
    private static byte LookupStream1BodySetPointersRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks visual body Y origins across all poses without changing projectile origins.</summary>
    internal static void VerifyLookupStream1BodyGraphicsOrigins(ISnesAddressSpace rom)
    {
        sbyte[] native = Enumerable.Range(0, 253).Select(pose => unchecked((sbyte)LookupStream1BodyGraphicsOriginsRomByte(rom, 0x91b62d + pose * 8))).ToArray();
        SamusBodyArtworkCatalog template = CreateSamusIdentityFixture();
        SamusBodyArtworkCatalog Create(sbyte[] selected) => new(new SamusBodyArtworkParts { TopSetStarts = template.TopSetStarts.ToArray(), BottomSetStarts = template.BottomSetStarts.ToArray(), PoseFirstFrames = template.PoseFirstFrames.ToArray(), GraphicsYOffsets = selected, Frames = template.Frames.ToArray(), Top = Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => template.TopSet(index).ToArray()).ToArray(), Bottom = Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => template.BottomSet(index).ToArray()).ToArray(), Spritemaps = template.Spritemaps, Atmosphere = template.Atmosphere, DeathPalettes = template.DeathPalettes, DeathTilesContentIdentity = template.DeathTilesContentIdentity, ArmCannon = template.ArmCannon, LandingYOffsets = template.LandingYOffsets.ToArray(), PostureYOffsets = template.PostureYOffsets.ToArray(), DrainedYOffsets = template.DrainedYOffsets.ToArray() });
        SamusBodyArtworkCatalog stock = Create(native);
        int Count(SamusBodyArtworkCatalog value) => value.graphicsYOffsets.Count;
        AssertEqual(0, Count(stock), "No stock graphics-origin override survives");
        for (int pose = 0; pose < 253; pose++)
        {
            AssertEqual(native[pose], SamusBodyPlacementDefinitions.DefaultGraphicsYOffset((SamusPoseId)pose), "Direct calculated visual origin matches native real-pose byte");
            AssertEqual(native[pose], stock.GraphicsYOffset((SamusPoseId)pose), "Installed default origin");
        }
        AssertTrue(stock.GraphicsYOffsets.SequenceEqual(native), "Visual origin snapshot ordering");
        string identity = stock.ContentIdentity;
        for (int edit = 0; edit < 253; edit++)
        {
            sbyte[] selected = (sbyte[])native.Clone(); selected[edit] ^= 1;
            SamusBodyArtworkCatalog changed = Create(selected);
            AssertEqual(1, Count(changed), "Exactly one independent graphics-origin override");
            AssertTrue(changed.GraphicsYOffsets.SequenceEqual(selected), "Every supplied visual origin remains independent");
            AssertTrue(changed.ContentIdentity != identity, "Every visual-origin edit changes content identity");
            for (int pose = 0; pose < 253; pose++)
            {
                AssertEqual(selected[pose], changed.GraphicsYOffset((SamusPoseId)pose), "Edited runtime origin");
                AssertEqual(unchecked((byte)native[pose]), SamusPoseProjectileOriginDefinitions.ReadYOffset((SamusPoseId)pose), "Visual edits never change physical projectile origins");
            }
        }
        AssertEqual(identity, stock.ContentIdentity, "Stock visual origin hash immutable");
        foreach (SamusPoseId pose in new[] { (SamusPoseId)0xfd, (SamusPoseId)0xfe, (SamusPoseId)0xff })
        {
            AssertThrows<InvalidDataException>(() => stock.GraphicsYOffset(pose), "Installed graphics excludes adjacent instruction poses");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyPlacementDefinitions.DefaultGraphicsYOffset(pose), "Default graphics excludes adjacent instruction poses");
        }
        Console.WriteLine("Body graphics origins:253 direct native/default values, zero stock overrides,253 independent visual edits isolated from physics and hash/bounds checks pass.");
    }

    /// <summary>Reads native Samus visual-origin offsets for all supported poses.</summary>
    private static byte LookupStream1BodyGraphicsOriginsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks drained-pose Y offsets and independent spritemap geometry edits.</summary>
    internal static void VerifyLookupStream1DrainedGeometry(ISnesAddressSpace rom)
    {
        string directory = Path.GetFullPath("csharp/test-temp/drained-geometry-native");
        SamusBodyArtworkFiles.Extract(rom, directory, SupportedCartridge.Sha256);
        SamusBodyArtworkCatalog stock = SamusBodyArtworkFiles.Load(directory, null);
        sbyte[] native = Enumerable.Range(0, 32).Select(index => unchecked((sbyte)LookupStream1DrainedGeometryRomByte(rom, 0x908def + index))).ToArray();
        int Stored(SamusBodyArtworkCatalog art) => art.drainedYOffsets.Count;
        SamusBodyArtworkCatalog Create(sbyte[] offsets, SamusSpritemapArtworkCatalog? maps = null,
            SamusBodyTileDefinition[][]? upper = null, SamusBodyTileDefinition[][]? lower = null, sbyte[]? graphics = null) => new(new SamusBodyArtworkParts { TopSetStarts = stock.TopSetStarts.ToArray(), BottomSetStarts = stock.BottomSetStarts.ToArray(), PoseFirstFrames = stock.PoseFirstFrames.ToArray(), GraphicsYOffsets = graphics ?? stock.GraphicsYOffsets.ToArray(), Frames = stock.Frames.ToArray(), Top = upper ?? Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => stock.TopSet(index).ToArray()).ToArray(), Bottom = lower ?? Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => stock.BottomSet(index).ToArray()).ToArray(), Spritemaps = maps ?? stock.Spritemaps, Atmosphere = stock.Atmosphere, DeathPalettes = stock.DeathPalettes, DeathTilesContentIdentity = stock.DeathTilesContentIdentity, ArmCannon = stock.ArmCannon, LandingYOffsets = stock.LandingYOffsets.ToArray(), PostureYOffsets = stock.PostureYOffsets.ToArray(), DrainedYOffsets = offsets });
        AssertEqual(0, Stored(stock), "Native drained placement uses no stored offset fallback: " + string.Join(", ", stock.drainedYOffsets.Select(pair => pair.Key + " native=" + pair.Value + " derived=" + (SamusBodyPlacementDefinitions.TryDefaultDrainedByte(stock, pair.Key, out int calculated) ? calculated.ToString() : "unavailable"))));
        for (int index = 0; index < 32; index++)
        {
            AssertTrue(SamusBodyPlacementDefinitions.TryDefaultDrainedByte(stock, index, out int direct), "Every native drained phase has a derived default");
            AssertEqual(native[index], direct, "Direct native support-aligned offset");
            sbyte[] changed = (sbyte[])native.Clone(); changed[index] ^= 1;
            var edited = Create(changed);
            AssertTrue(edited.DrainedYOffsets.SequenceEqual(changed), "Every independent drained edit remains exact");
            AssertTrue(edited.ContentIdentity != stock.ContentIdentity, "Drained edit changes selected identity");
        }
        int draws = 0;
        for (ushort frame = 0; frame < 32; frame++)
        {
            AssertTrue(stock.TryDrainedYOffset(frame, out int offset), "Entire native drained byte window admitted");
            AssertEqual(native[frame], offset, "Installed native byte including command slots");
            if (frame is 12 or 13 or 17 or 18 or 24 or 25 or 27 or 28 or 30 or 31) continue;
            var samus = new SamusState { Pose = SamusPoseId.DrainedCrouchingLeftPose, AnimationFrame = frame, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
            samus.Presentation.BodyFrames.BindBodyFixture(stock);
            samus.Presentation.Draw(samus, new SpriteFrameBuffer(), new(0, 0));
            AssertEqual(unchecked((ushort)(128 + native[frame])), samus.Presentation.SpritemapPosition.Y,
                "Actual drained drawing uses exact signed native correction");
            AssertEqual((ushort)128, samus.Position.Y.Whole, "Drawing preserves physical center");
            draws++;
        }
        AssertEqual(22, draws, "Every nonempty source phase drawn");
        AssertTrue(!stock.TryDrainedYOffset(-1, out _) && !stock.TryDrainedYOffset(32, out _), "Exact drained bounds");
        var emptyMaps = new SamusSpritemapArtworkCatalog(stock.Spritemaps.TopBases.ToArray(), stock.Spritemaps.BottomBases.ToArray(),
            stock.Spritemaps.Entries.ToArray(), stock.Spritemaps.Definitions.Select(map => new SamusSpritemapDefinition([])).ToArray());
        var empty = Create(native, emptyMaps);
        AssertTrue(empty.DrainedYOffsets.SequenceEqual(native), "Supplied empty art preserves all independent offsets");
        var zeroPointers = new SamusSpritemapArtworkCatalog(stock.Spritemaps.TopBases.ToArray(), stock.Spritemaps.BottomBases.ToArray(),
            new SamusSpritemapId?[SamusSpritemapArtworkCatalog.EntryCount], []);
        AssertTrue(Create(native, zeroPointers).DrainedYOffsets.SequenceEqual(native), "Mutable-zero-pointer art preserves offset schema");
        SamusBodyTileDefinition[][] Blank(bool top) => Enumerable.Range(0, top ? SamusBodyArtworkCatalog.TopSetCount : SamusBodyArtworkCatalog.BottomSetCount)
            .Select(set => (top ? stock.TopSet(set) : stock.BottomSet(set)).Select(definition =>
                new SamusBodyTileDefinition(definition.FirstSize, definition.SecondSize, new byte[definition.Planar.Length])).ToArray()).ToArray();
        var blankPixels = Create(native, upper: Blank(true), lower: Blank(false));
        AssertTrue(blankPixels.DrainedYOffsets.SequenceEqual(native), "Independently blank PNG pixels preserve original offsets");
        SamusSpritemapId? changedPointer = stock.Spritemaps.Entries[stock.Spritemaps.TopBase(SamusPoseId.DrainedCrouchingLeftPose)];
        var shiftedMaps = new SamusSpritemapArtworkCatalog(stock.Spritemaps.TopBases.ToArray(), stock.Spritemaps.BottomBases.ToArray(),
            stock.Spritemaps.Entries.ToArray(), stock.Spritemaps.Definitions.Select((map, ordinal) => new SamusSpritemapDefinition(
                ordinal == changedPointer?.Ordinal ? map.Parts.Select(part => part with { Offset = part.Offset.WithY(unchecked((sbyte)(part.Offset.Y + 1))) }).ToArray() : map.Parts)).ToArray());
        var shifted = Create(native, shiftedMaps);
        AssertTrue(shifted.DrainedYOffsets.SequenceEqual(native), "Changed source support does not couple independent offset edits");
        AssertTrue(Stored(shifted) > 0, "Edited source geometry stores explicit independent differences");
        sbyte[] editedGraphics = stock.GraphicsYOffsets.ToArray();
        editedGraphics[0xe9]++; editedGraphics[0x02]++;
        AssertTrue(Create(native, graphics: editedGraphics).DrainedYOffsets.SequenceEqual(native),
            "Independently changed crouching/standing origins preserve all supplied drained offsets");
        AssertThrows<ArgumentOutOfRangeException>(() => SamusBodyPlacementDefinitions.TryDefaultDrainedByte(stock, 32, out _), "Default drained bound");
        Console.WriteLine("Drained geometry:32 native defaults, zero stock fallbacks,32 independent edits,22 actual draws and independent empty/pointer/pixel/shifted-art checks pass.");
    }

    /// <summary>Reads native drained Samus pose offsets for geometry comparison.</summary>
    private static byte LookupStream1DrainedGeometryRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks morph-transition posture offsets independently of tile and spritemap edits.</summary>
    internal static void VerifyLookupStream1PostureGeometry(ISnesAddressSpace rom)
    {
        string directory = Path.GetFullPath("csharp/test-temp/posture-geometry-native");
        SamusBodyArtworkFiles.Extract(rom, directory, SupportedCartridge.Sha256);
        SamusBodyArtworkCatalog stock = SamusBodyArtworkFiles.Load(directory, null);
        sbyte[] native = Enumerable.Range(0, 24).Select(index => unchecked((sbyte)LookupStream1PostureGeometryRomByte(rom, 0x908d80 + index))).ToArray();
        int Stored(SamusBodyArtworkCatalog art) => art.postureYOffsets.Count;
        SamusBodyArtworkCatalog Create(sbyte[] offsets, SamusSpritemapArtworkCatalog? maps = null,
            SamusBodyTileDefinition[][]? upper = null, SamusBodyTileDefinition[][]? lower = null) => new(new SamusBodyArtworkParts { TopSetStarts = stock.TopSetStarts.ToArray(), BottomSetStarts = stock.BottomSetStarts.ToArray(), PoseFirstFrames = stock.PoseFirstFrames.ToArray(), GraphicsYOffsets = stock.GraphicsYOffsets.ToArray(), Frames = stock.Frames.ToArray(), Top = upper ?? Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => stock.TopSet(index).ToArray()).ToArray(), Bottom = lower ?? Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => stock.BottomSet(index).ToArray()).ToArray(), Spritemaps = maps ?? stock.Spritemaps, Atmosphere = stock.Atmosphere, DeathPalettes = stock.DeathPalettes, DeathTilesContentIdentity = stock.DeathTilesContentIdentity, ArmCannon = stock.ArmCannon, LandingYOffsets = stock.LandingYOffsets.ToArray(), PostureYOffsets = offsets, DrainedYOffsets = stock.DrainedYOffsets.ToArray() });
        AssertEqual(0, Stored(stock), "Native posture uses no stored offset fallback: " + string.Join(", ", stock.postureYOffsets.Select(pair => pair.Key + " native=" + pair.Value + " derived=" + (SamusBodyPlacementDefinitions.TryDefaultPostureByte(stock, pair.Key, out int calculated) ? calculated.ToString() : "unavailable"))));
        for (int index = 0; index < 24; index++)
        {
            AssertTrue(SamusBodyPlacementDefinitions.TryDefaultPostureByte(stock, index, out int direct), "Every native posture has a derived default");
            AssertEqual(native[index], direct, "Direct native support-aligned offset");
            sbyte[] changed = (sbyte[])native.Clone(); changed[index] ^= 1;
            var edited = Create(changed);
            AssertTrue(edited.PostureYOffsets.SequenceEqual(changed), "Every independent posture edit remains exact");
            AssertTrue(edited.ContentIdentity != stock.ContentIdentity, "Posture edit changes selected identity");
        }
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.CrouchingTransitionRightPose, SamusPoseId.CrouchingTransitionLeftPose,
            SamusPoseId.MorphingTransitionRightPose, SamusPoseId.MorphingTransitionLeftPose,
            SamusPoseId.StandingTransitionRightPose, SamusPoseId.StandingTransitionLeftPose,
            SamusPoseId.UnmorphingTransitionRightPose, SamusPoseId.UnmorphingTransitionLeftPose
        })
        {
            int count = pose is SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionLeftPose or SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionLeftPose ? 1 : 2;
            for (ushort frame = 0; frame < count; frame++)
            {
                var samus = new SamusState { Pose = pose, AnimationFrame = frame, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
                samus.Presentation.BodyFrames.BindBodyFixture(stock);
                samus.Presentation.Draw(samus, new SpriteFrameBuffer(), new(0, 0));
                AssertEqual(unchecked((ushort)(128 + native[((int)pose - 0x35) * 2 + frame])), samus.Presentation.SpritemapPosition.Y,
                    "Actual transition rendering preserves native support correction");
                AssertEqual((ushort)128, samus.Position.Y.Whole, "Posture art never mutates physical center");
            }
        }
        var emptyMaps = new SamusSpritemapArtworkCatalog(stock.Spritemaps.TopBases.ToArray(), stock.Spritemaps.BottomBases.ToArray(),
            stock.Spritemaps.Entries.ToArray(), stock.Spritemaps.Definitions.Select(map => new SamusSpritemapDefinition([])).ToArray());
        var empty = Create(native, emptyMaps);
        AssertTrue(empty.PostureYOffsets.SequenceEqual(native), "Supplied empty art preserves all independent offsets");
        var zeroPointers = new SamusSpritemapArtworkCatalog(stock.Spritemaps.TopBases.ToArray(), stock.Spritemaps.BottomBases.ToArray(),
            new SamusSpritemapId?[SamusSpritemapArtworkCatalog.EntryCount], []);
        AssertTrue(Create(native, zeroPointers).PostureYOffsets.SequenceEqual(native), "Mutable-zero-pointer art preserves offset schema");
        SamusBodyTileDefinition[][] Blank(bool top) => Enumerable.Range(0, top ? SamusBodyArtworkCatalog.TopSetCount : SamusBodyArtworkCatalog.BottomSetCount)
            .Select(set => (top ? stock.TopSet(set) : stock.BottomSet(set)).Select(definition =>
                new SamusBodyTileDefinition(definition.FirstSize, definition.SecondSize, new byte[definition.Planar.Length])).ToArray()).ToArray();
        var blankPixels = Create(native, upper: Blank(true), lower: Blank(false));
        AssertTrue(blankPixels.PostureYOffsets.SequenceEqual(native), "Independently blank PNG pixels preserve original offsets");
        SamusSpritemapId? changedPointer = stock.Spritemaps.Entries[stock.Spritemaps.TopBase(SamusPoseId.MorphingTransitionRightPose)];
        var shiftedMaps = new SamusSpritemapArtworkCatalog(stock.Spritemaps.TopBases.ToArray(), stock.Spritemaps.BottomBases.ToArray(),
            stock.Spritemaps.Entries.ToArray(), stock.Spritemaps.Definitions.Select((map, ordinal) => new SamusSpritemapDefinition(
                ordinal == changedPointer?.Ordinal ? map.Parts.Select(part => part with { Offset = part.Offset.WithY(unchecked((sbyte)(part.Offset.Y + 1))) }).ToArray() : map.Parts)).ToArray());
        var shifted = Create(native, shiftedMaps);
        AssertTrue(shifted.PostureYOffsets.SequenceEqual(native), "Changed source support does not couple independent offset edits");
        AssertTrue(Stored(shifted) > 0, "Edited source geometry stores explicit independent differences");
        Console.WriteLine("Posture geometry:24 direct native defaults, zero stock fallbacks,24 offset edits,12 actual draw cases and empty/pointer/pixel/shifted-art independence pass.");
    }

    /// <summary>Reads native Samus posture Y offsets for the geometry oracle.</summary>
    private static byte LookupStream1PostureGeometryRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks landing and facing offsets, edited placement, and strict selector bounds.</summary>
    internal static void VerifyLookupStream1BodyFacingOffsets(ISnesAddressSpace rom)
    {
        ushort[] landing = Enumerable.Range(0, 17).Select(index => (ushort)LookupStream1BodyFacingOffsetsRomByte(rom, 0x908d28 + index)).ToArray();
        sbyte[] posture = Enumerable.Range(0, 24).Select(index => unchecked((sbyte)LookupStream1BodyFacingOffsetsRomByte(rom, 0x908d80 + index))).ToArray();
        SamusBodyArtworkCatalog template = CreateSamusIdentityFixture();
        SamusBodyArtworkCatalog Create(ushort[] selectedLanding, sbyte[] selectedPosture) => new(new SamusBodyArtworkParts { TopSetStarts = template.TopSetStarts.ToArray(), BottomSetStarts = template.BottomSetStarts.ToArray(), PoseFirstFrames = template.PoseFirstFrames.ToArray(), GraphicsYOffsets = template.GraphicsYOffsets.ToArray(), Frames = template.Frames.ToArray(), Top = Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => template.TopSet(index).ToArray()).ToArray(), Bottom = Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => template.BottomSet(index).ToArray()).ToArray(), Spritemaps = template.Spritemaps, Atmosphere = template.Atmosphere, DeathPalettes = template.DeathPalettes, DeathTilesContentIdentity = template.DeathTilesContentIdentity, ArmCannon = template.ArmCannon, LandingYOffsets = selectedLanding, PostureYOffsets = selectedPosture, DrainedYOffsets = template.DrainedYOffsets.ToArray() });
        SamusBodyArtworkCatalog stock = Create(landing, posture);
        var landingBasis = stock.landingYOffsets;

        AssertEqual(0, landingBasis.Count, "Landing stock stores no coordinate or instruction-byte overrides");
        // This synthetic fixture supplies independent artwork; direct native geometry is confirmed separately.
        for (int index = 0; index < landing.Length; index++)
        {
            int source = index is >= 4 and < 8 or >= 12 and < 16 ? index - 4 : index;
            AssertEqual(source, SamusBodyPlacementDefinitions.LandingSourceIndex(index), "Native normal/spin facing row alias");
            AssertEqual(landing[index], landing[source], "Direct native landing facing equality");
            AssertEqual(landing[index], (ushort)SamusBodyPlacementDefinitions.DefaultLandingByte(index), "Direct native semantic landing default including adjacent PLB");
        }
        for (int index = 0; index < posture.Length; index++)
        {
            int source = (index & 2) != 0 ? index - 2 : index;
            AssertEqual(source, SamusBodyPlacementDefinitions.PostureSourceIndex(index), "Native transition facing row alias");
            AssertEqual(posture[index], posture[source], "Direct native posture facing equality");

        }
        void Check(SamusBodyArtworkCatalog catalog, ushort[] expectedLanding, sbyte[] expectedPosture)
        {
            AssertTrue(catalog.LandingYOffsets.SequenceEqual(expectedLanding), "Landing snapshot preserves supplied bytes");
            AssertTrue(catalog.PostureYOffsets.SequenceEqual(expectedPosture), "Posture snapshot preserves supplied bytes");
            for (int index = 0; index < 16; index++)
            {
                AssertTrue(catalog.TryLandingYOffset(index, out int actual), "Landing word admission");
                AssertEqual(LittleEndianBytes.Word(unchecked((byte)expectedLanding[index]), unchecked((byte)expectedLanding[index + 1])), actual, "Native unaligned landing word including adjacent byte");
            }
            for (int index = 0; index < 24; index++)
            {
                AssertTrue(catalog.TryPostureYOffset(index, out int actual), "Posture admission");
                AssertEqual(expectedPosture[index], actual, "Signed posture offset");
            }
        }
        Check(stock, landing, posture);
        for (int index = 0; index < 16; index++)
        {
            var samus = new SamusState
            {
                Pose = (SamusPoseId)(0xa4 + index / 4), AnimationFrame = (ushort)(index % 4),
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            };
            samus.Presentation.BodyFrames.BindBodyFixture(stock);
            samus.Presentation.Draw(samus, new SpriteFrameBuffer(), layer1: new(0, 0));
            ushort nativeWord = LittleEndianBytes.Word(unchecked((byte)landing[index]), unchecked((byte)landing[index + 1]));
            AssertEqual(unchecked((ushort)(128 - nativeWord)), samus.Presentation.SpritemapPosition.Y, "Actual rendering preserves every bounded native landing word");
            AssertEqual((ushort)128, samus.Position.Y.Whole, "Visual landing correction does not change physical position");
        }
        string identity = stock.ContentIdentity;
        for (int edit = 0; edit < 41; edit++)
        {
            ushort[] changedLanding = (ushort[])landing.Clone(); sbyte[] changedPosture = (sbyte[])posture.Clone();
            if (edit < 17) changedLanding[edit] ^= 1; else changedPosture[edit - 17] ^= 1;
            SamusBodyArtworkCatalog changed = Create(changedLanding, changedPosture);
            Check(changed, changedLanding, changedPosture);
            AssertTrue(changed.ContentIdentity != identity, "Every independent placement edit changes identity");
            Check(stock, landing, posture);
            AssertEqual(identity, stock.ContentIdentity, "Placement edits leave stock identity immutable");
        }
        foreach (int index in new[] { -1, 16, 17, int.MaxValue })
            AssertTrue(!stock.TryLandingYOffset(index, out _), "Landing reader original word bounds");
        foreach (int index in new[] { -1, 24, int.MaxValue })
            AssertTrue(!stock.TryPostureYOffset(index, out _), "Posture reader original bounds");
        AssertEqual((ushort)0xab, landing[16], "Adjacent native PLB byte is the exact retained instruction observation");
        Console.WriteLine("Body facing offsets:17 direct landing defaults with zero overrides,24 posture bytes,41 independent edits,16 unaligned windows and content identities pass.");
    }

    /// <summary>Reads native landing and posture offset words for Samus placement.</summary>
    private static byte LookupStream1BodyFacingOffsetsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks upper/lower OAM base selectors, independent edits, hashes, and invalid poses.</summary>
    internal static void VerifyLookupStream1BodyOamBases(ISnesAddressSpace rom)
    {
        ushort[] ReadWords(int address, int count) => Enumerable.Range(0, count)
            .Select(index => LittleEndianBytes.Word(LookupStream1BodyOamBasesRomByte(rom, address + index * 2), LookupStream1BodyOamBasesRomByte(rom, address + index * 2 + 1))).ToArray();
        ushort[] top = ReadWords(0x929263, 253), bottom = ReadWords(0x92945d, 253), pointers = ReadWords(0x92808d, 2096);
        SamusSpritemapId?[] entries = SamusSpritemapNative.Entries(pointers);
        // Marker parts confirm selected supplied payload identity, not native artwork: every bit of the pointer lands in
        // the marker's character, palette row, priority or mirroring, so distinct pointers give distinct markers.
        static SpritePart Marker(ushort pointer) =>
            SpritePart.Create(Offset16<ScreenPixels>.Of(unchecked((sbyte)pointer), unchecked((sbyte)(pointer >> 8))), (pointer & 1) != 0, pointer & 511,
                (pointer >> 9) & 7, (pointer >> 12) & 3,
                ((pointer & 0x4000) != 0 ? TileFlip.Horizontal : TileFlip.None) | ((pointer & 0x8000) != 0 ? TileFlip.Vertical : TileFlip.None));
        SamusSpritemapDefinition[] definitions = [.. SamusSpritemapNative.Compositions(pointers)
            .Select(pointer => new SamusSpritemapDefinition([Marker(pointer)]))];
        SamusSpritemapArtworkCatalog Create(ushort[] upper, ushort[] lower) => new(upper, lower, entries, definitions);
        var stock = Create(top, bottom);
        string Canonical(ushort[] upper, ushort[] lower) => SelectedPresentationHash.Create("samus-spritemaps-v3", content =>
        {
            content.AppendWords("top bases", upper);
            content.AppendWords("bottom bases", lower);
            foreach (SamusSpritemapId? entry in entries) content.Append("entry", entry?.Ordinal ?? -1);
            foreach (SamusSpritemapDefinition definition in definitions)
            {
                content.Append("part count", definition.Parts.Length);
                foreach (SpritePart part in definition.Parts) AppendCanonicalSpritePart(content, part);
            }
        });
        void CheckSelection(SamusSpritemapArtworkCatalog catalog, ushort selected)
        {
            bool found = catalog.TryGet(selected, out SamusSpritemapDefinition? definition);
            AssertEqual(pointers[selected] != 0, found, "Native zero pointer draws nothing");
            if (found)
                AssertEqual(Marker(pointers[selected]),
                    (definition ?? throw new InvalidOperationException("Verification requires definition.")).Parts.Single(),
                    "Actual selector resolves independent marker payload");
        }
        AssertEqual(0, stock.topBases.Count + stock.bottomBases.Count, "Zero stock OAM-base overrides");
        AssertEqual(Canonical(top, bottom), stock.ContentIdentity, "Exact original canonical stock OAM hash");
        for (int half = 0; half < 2; half++)
        for (int pose = 0; pose < 253; pose++)
        {
            ushort expected = half == 0 ? top[pose] : bottom[pose];
            AssertEqual(expected, half == 0 ? SamusSpritemapPoseDefinitions.TopBase((SamusPoseId)pose) : SamusSpritemapPoseDefinitions.BottomBase((SamusPoseId)pose),
                "Direct named OAM-base case matches native ROM");
            AssertEqual(expected, half == 0 ? stock.TopBase((SamusPoseId)pose) : stock.BottomBase((SamusPoseId)pose), "Runtime native base selection");
            CheckSelection(stock, expected);
            ushort[] upper = (ushort[])top.Clone(), lower = (ushort[])bottom.Clone();
            ushort edited = (ushort)((expected + 1) % 2096);
            (half == 0 ? upper : lower)[pose] = edited;
            var changed = Create(upper, lower);
            AssertEqual(1, changed.topBases.Count + changed.bottomBases.Count, "Exactly one independent OAM-base override");
            AssertTrue(changed.TopBases.SequenceEqual(upper) && changed.BottomBases.SequenceEqual(lower), "All supplied independent bases preserved");
            AssertTrue(changed.Entries.SequenceEqual(entries), "OAM composition entries unchanged");
            ushort selected = half == 0 ? changed.TopBase((SamusPoseId)pose) : changed.BottomBase((SamusPoseId)pose);
            AssertEqual(edited, selected, "Runtime honors independently edited base");
            CheckSelection(changed, selected);
            AssertEqual(Canonical(upper, lower), changed.ContentIdentity, "Exact original canonical edited OAM hash");
            AssertTrue(changed.ContentIdentity != stock.ContentIdentity, "Every independent base edit changes identity");
        }
        foreach (SamusPoseId pose in new[] { (SamusPoseId)0xfd, (SamusPoseId)0xfe, (SamusPoseId)0xff })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusSpritemapPoseDefinitions.TopBase(pose), "Top default excludes adjacent data");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusSpritemapPoseDefinitions.BottomBase(pose), "Bottom default excludes adjacent data");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.TopBase(pose), "Original installed top domain exception");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.BottomBase(pose), "Original installed bottom domain exception");
        }
        ushort[] invalid = (ushort[])top.Clone(); invalid[0] = 2096;
        AssertThrows<InvalidDataException>(() => Create(invalid, bottom), "Edited base retains original range validation");
        AssertTrue(stock.TopBases.SequenceEqual(top) && stock.BottomBases.SequenceEqual(bottom), "Stock bases immutable after edits");
        Console.WriteLine("Samus OAM bases:506 native named cases, zero stock overrides,506 independent edits, actual supplied payload selection, exact canonical hashes and original bounds pass.");
    }

    /// <summary>Reads native Samus top and bottom OAM base words.</summary>
    private static byte LookupStream1BodyOamBasesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks calculated intro and hurt palettes, independent color overrides, and stock immutability.</summary>
    internal static void VerifyLookupStream1HurtBlend(ISnesAddressSpace rom)
    {
        ushort[] native = Enumerable.Range(0, 32).Select(index =>
            LittleEndianBytes.Word(LookupStream1HurtBlendRomByte(rom, 0x9ba380 + index * 2), LookupStream1HurtBlendRomByte(rom, 0x9ba381 + index * 2))).ToArray();
        PaletteRgb5 Color(ushort word) => new() { Red = word & 31, Green = (word >> 5) & 31, Blue = (word >> 10) & 31 };
        SamusHurtColorCatalog Create(ushort[] supplied) => SamusHurtColorFile.Read(new MemoryStream(SamusHurtColorFile.Write(
            new SamusHurtColorDocument { Version = 1, Hurt = supplied.Take(16).Select(Color).ToArray(), Intro = supplied.Skip(16).Select(Color).ToArray() })));
        Dictionary<int, byte> Levels(SamusHurtColorCatalog catalog) => catalog.introLevels;
        var stock = Create(native);
        AssertEqual(0, stock.introOverrides.Count + stock.hurtOverrides.Count, "Zero stock channel/blend fallbacks");
        AssertEqual(0, Levels(stock).Count, "Zero stock shade samples survive");
        AssertTrue(stock.hurtZero is null &&
            stock.introZero is null, "No stock transparent-word fallback");
        AssertEqual(native[0], SamusHurtColorDefinitions.HurtTransparentWord, "Native transparent black");
        AssertEqual(native[16], SamusHurtColorDefinitions.IntroTransparentWord, "Native reviewed transparent identity");
        for (int index = 1; index < 16; index++)
        {
            AssertEqual((byte)(native[index + 16] & 31), SamusHurtColorDefinitions.DefaultIntroLevel(index), "Direct calculated shade level equals native");
            AssertEqual(native[index + 16], SamusHurtColorDefinitions.IntroFromLevel((byte)(native[index + 16] & 31)), "Direct native intro channel relation");
            AssertEqual(native[index], SamusHurtColorDefinitions.HurtFromIntro(RgbColor.FromWord(checked(native[index + 16]))), "Direct native hurt white blend");
        }
        for (int selected = 0; selected < 32; selected++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort[] supplied = (ushort[])native.Clone(); supplied[selected] ^= (ushort)(31 << (channel * 5));
            var edited = Create(supplied);
            for (int index = 0; index < 32; index++)
                AssertEqual(supplied[index], edited.Resolve(index < 16 ? SamusHurtColorVariant.Hurt : SamusHurtColorVariant.Intro, index % 16), "Every supplied RGB channel remains independent across palettes");
            for (int index = 1; index < 16; index++)
            {
                bool stored = (supplied[index + 16] & 31) != (native[index + 16] & 31);
                AssertEqual(stored, Levels(edited).ContainsKey(index), "Exact independent shade difference membership");
                if (stored) AssertEqual((byte)(supplied[index + 16] & 31), Levels(edited)[index], "Exact supplied shade override");
            }
            for (int index = 1; index < 16; index++)
            {
                int red = supplied[index + 16] & 31;
                ushort expectedIntro = (ushort)(red | red << 5 | Math.Max(4, red - 2) << 10);
                int Blend(int shift) => (2 * ((supplied[index + 16] >> shift) & 31) + 155) / 7;
                ushort expectedHurt = (ushort)(Blend(0) | Blend(5) << 5 | Blend(10) << 10);
                AssertEqual(supplied[index + 16] != expectedIntro, edited.introOverrides.ContainsKey(new ColourCell(0, index)), "Exact intro edit exception membership");
                AssertEqual(supplied[index] != expectedHurt, edited.hurtOverrides.ContainsKey(new ColourCell(0, index)), "Exact independent hurt exception membership");
            }
        }
        for (int index = 0; index < 32; index++)
            AssertEqual(native[index], stock.Resolve(index < 16 ? SamusHurtColorVariant.Hurt : SamusHurtColorVariant.Intro, index % 16), "Stock color remains immutable");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve((SamusHurtColorVariant)2, 0), "Variant rejection preserved");
        foreach (int index in new[] { -1, 16 })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(SamusHurtColorVariant.Intro, index), "Color bounds preserved");
        Console.WriteLine("Hurt/intro:32 native colors,30 direct relations,zero stock shade samples,zero stock overrides,96 independent RGB edits and exact exception membership pass.");
    }

    /// <summary>Reads native intro and hurt RGB5 palette words.</summary>
    private static byte LookupStream1HurtBlendRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);






    /// <summary>Checks arm-cannon pose pointers and independent edited placement against native selections.</summary>
    internal static void VerifyLookupStream1CannonPoses(ISnesAddressSpace rom)
    {
        using var directory = new TestScratchDirectory("map-catalog");
        SamusArmCannonArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        byte[] json = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.JsonFileName));
        byte[] png = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.TileFileName));
        var document = RequiredJson.Deserialize<SamusArmCannonArtworkDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var tiles = RoomCharacterAtlasPng.Read(new MemoryStream(png), SamusArmCannonArtworkFormat.TileSourcePointers.Count * 32);
        SamusArmCannonArtworkCatalog Load(SamusArmCannonArtworkDocument value) => SamusArmCannonArtworkCatalog.FromPlacement(
            SamusArmCannonArtworkCatalog.LoadPlacement(new MemoryStream(SamusArmCannonArtworkCatalog.Write(value))), tiles);
        var stock = Load(document);
        Dictionary<int, int> Stored(SamusArmCannonArtworkCatalog value) => value.poseDescriptors;
        ushort Native(int pose) => LittleEndianBytes.Word(LookupStream1CannonPosesRomByte(rom, 0x90C7DF + pose * 2), LookupStream1CannonPosesRomByte(rom, 0x90C7E0 + pose * 2));
        string Identity(SamusArmCannonArtworkDocument value) => SelectedPresentationHash.Create("samus-arm-cannon-v3", hash =>
        {
            foreach (int descriptor in value.PoseDescriptors) hash.Append("pose descriptor", descriptor);
            hash.Append("drawing data", value.DrawingData.Select(item => (byte)item).ToArray());
            foreach (SpriteVisualPart appearance in value.CoverAppearances)
                AppendCanonicalSpritePart(hash, CannonCoverAppearance(appearance.TileRow, appearance.TileColumn,
                    appearance.Palette ?? 0, appearance.Priority, appearance.FlipX, appearance.FlipY));
            foreach (int[] direction in value.CoverTiles)
                foreach (int tile in direction) hash.Append("cover tile", tile);
            hash.Append("characters", tiles.Characters.Span);
        });
        AssertEqual(253, SamusBodyArtworkCatalog.PoseCount, "cannon original 506-byte pointer extent");
        AssertEqual(0, Stored(stock).Count, "cannon pose stock stores no copied pointer overrides");
        AssertEqual(Identity(document), stock.ContentIdentity, "cannon canonical native identity unchanged");
        for (int pose = 0; pose < 253; pose++)
        {
            AssertEqual(Native(pose), checked((ushort)(SamusSourceDefinitions.ArmCannonDrawingData + SamusArmCannonArtworkFormat.StockPoseDescriptor(pose))), "cannon direct native pose dispatch");
            AssertEqual(Native(pose), stock.PoseDrawingData(pose), "cannon installed native pose dispatch");
            int[] descriptors = document.PoseDescriptors.ToArray();
            // Preserve an arbitrary admitted byte offset, including descriptor interiors.
            int replacement = pose;
            if (replacement == descriptors[pose]) replacement++;
            descriptors[pose] = replacement;
            var editedDocument = document with { PoseDescriptors = descriptors };
            var edited = Load(editedDocument);
            AssertEqual(1, Stored(edited).Count, "cannon exactly one independent descriptor override");
            AssertEqual(replacement, Stored(edited)[pose], "cannon stores arbitrary admitted descriptor offset");
            for (int other = 0; other < 253; other++)
                AssertEqual(other == pose ? (ushort)(SamusSourceDefinitions.ArmCannonDrawingData + replacement) : Native(other), edited.PoseDrawingData(other), "cannon isolated descriptor edit");
            AssertEqual(Identity(editedDocument), edited.ContentIdentity, "cannon edited pointer hash preserves canonical representation");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 253, 254, 255, 256, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.PoseDrawingData(invalid), "cannon installed pose domain preserved including FD-FF");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusArmCannonArtworkFormat.StockPoseDescriptor(invalid), "cannon calculated pose domain rejects aliases");
        }
        Console.WriteLine("Cannon poses:253 direct native defaults, zero stock overrides,253 independent arbitrary pointer edits/hashes and exact FD-FF rejection pass.");
    }

    /// <summary>Reads the native pose-to-cannon pointer table.</summary>
    private static byte LookupStream1CannonPosesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks invalid pose aliases against running-delay pointers and live mirrored WRAM mutations.</summary>
    internal static void VerifyLookupStream1AnimationAliases(CartridgeImportAddressSpace bus)
    {
        for (int pose = 0xFD; pose <= 0xFF; pose++)
        {
            int source = SamusAnimationDelayDefinitions.DelayStreamsAddress + (pose - 0xFD) * 2;
            ushort expectedPointer = bus.ReadCartridgeWord(source);
            ushort pointer = SamusAnimationDelayDefinitions.PointerForPose((SamusPoseId)pose);
            AssertEqual(expectedPointer, pointer, $"animation alias ${pose:X2} reads its running-delay source");
            var residue = new SuperMetroid.Core.Compatibility.RecordedFloorUpdateResidue();
            for (int change = 0; change < 2; change++)
            {
                byte expected = (byte)(pose - 0xFD + change * 16);
                residue.Write(pointer - SamusAnimationDelayDefinitions.FloorUpdateResidueOffset, expected);
                AssertEqual(expected, SamusAnimationDelayDefinitions.ReadAnimationByte(residue, pointer, 0),
                    $"animation alias ${pose:X2} observes queue residue mutation {change}");
            }
        }
        Console.WriteLine("Animation aliases: three original running-delay words and six live WRAM mutations match.");
    }

    /// <summary>Checks HUD posture admission and grapple/non-grapple observations over all pose bytes.</summary>
    internal static void VerifyLookupStream1HudPosture(ISnesAddressSpace rom)
    {
        for (int pose = 0; pose < 256; pose++)
        {
            if (pose < 0xDB)
                AssertEqual(LookupStream1HudPostureRomByte(rom, 0x90DDAA + pose - 0x35), SamusHudDefinitions.PostureObservation((SamusPoseId)pose, InstalledFixtures.SamusOverreads),
                    $"HUD exact bounded posture observation {pose:X2}");
            else
                AssertThrows<ArgumentOutOfRangeException>(() => SamusHudDefinitions.PostureObservation((SamusPoseId)pose, InstalledFixtures.SamusOverreads),
                    "HUD rejects prefiltered posture observation");
            foreach (bool grapple in new[] { false, true })
            {
                bool expected = pose >= 0xF1 || (pose < 0xDB && (LookupStream1HudPostureRomByte(rom, 0x90DDAA + pose - 0x35) == 0 || grapple));
                AssertEqual(expected, SamusHudInput.PostureTransitionAdmitsWeapons((SamusPoseId)pose, grapple, InstalledFixtures.SamusOverreads),
                    $"HUD actual admission {pose:X2}, Grapple={grapple}");
            }
        }
        Console.WriteLine("HUD posture: twelve real flags plus207 bounded instruction bytes, all512 admission branches and rejection domain match native.");
    }

    /// <summary>Reads native HUD posture observation bytes.</summary>
    private static byte LookupStream1HudPostureRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the escape timer's anchors, its sheet draws against the native sprites, and independent anchor and spacing edits.</summary>
    internal static void VerifyLookupStream1TimerLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1TimerLayoutRomByte(rom, address), LookupStream1TimerLayoutRomByte(rom, address + 1));
        byte[] json = EscapeTimerPresentationExtractor.Extract();
        var stock = EscapeTimerPresentation.Load(new MemoryStream(json));
        var document = RequiredJson.Deserialize<EscapeTimerPresentationDocument>(json, ArtworkJson.Options);
        AssertEqual(0, stock.anchors.Count, "Timer stock stores no calculated anchor fallbacks");
        int[] anchorImmediates = [0x809f73, 0x809f7c, 0x809f85, 0x809f8e];
        foreach (EscapeTimerAnchor anchor in EscapeTimerPresentationDefinitions.Anchors)
            AssertEqual(new Offset16<SpritePixels>(unchecked((short)Word(anchorImmediates[(int)anchor])), 0), EscapeTimerPresentationDefinitions.DefaultAnchor(anchor), "Timer centered anchor matches native renderer immediate");
        SpriteSheet sheet = EscapeTimerDrawChecks.Sheet(rom);
        SpriteArtwork characters = EscapeTimerCharacters.Read(new MemoryStream(
            SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerTileAtlasExtractor.Extract(rom))).Artwork;
        GameplayBasePalette palettes = InstalledFixtures.GameplayBasePalettes;
        var timer = new EscapeTimer(); timer.Clear();
        for (int digit = 0; digit < 10; digit++)
        {
            timer.SetTime(PackedDecimal.FromDecimal(digit * 11), PackedDecimal.Zero, PackedDecimal.FromDecimal(digit * 11));
            var native = new OamBuffer();
            DrawImportedEscapeTimer(rom, timer, native);
            EscapeTimerDrawChecks.AssertSamePixels(native, EscapeTimerDrawChecks.Draw(timer, stock, sheet), characters, palettes,
                $"Decimal glyph {digit} paints the native sprites' pixels");
        }
        foreach (string anchor in document.Anchors.Keys)
        {
            var edited = Read(); edited.Anchors[anchor] = edited.Anchors[anchor] with { X = (short)(edited.Anchors[anchor].X + 2), Y = 3 };
            edited = edited with { DigitSpacing = 10 };
            var compiled = Load(edited);
            AssertEqual(1, compiled.anchors.Count, "One timer anchor edit stores exactly one independent anchor");
            timer.SetTime(PackedDecimal.FromDecimal(12), PackedDecimal.FromDecimal(34), PackedDecimal.FromDecimal(56));
            EscapeTimerDrawChecks.AssertSamePixels(DrawDocument(edited), EscapeTimerDrawChecks.Draw(timer, compiled, sheet), characters, palettes,
                "Independent anchor and digit spacing edits move the native sprites");
        }
        Console.WriteLine("Escape timer layout: 4 native anchors, zero stock dictionaries, 10 decimal glyphs pixel-exact from the sheet and independent anchor/spacing edits pass.");

        EscapeTimerPresentationDocument Read() => RequiredJson.Deserialize<EscapeTimerPresentationDocument>(json, ArtworkJson.Options);
        static EscapeTimerPresentation Load(EscapeTimerPresentationDocument value)
        {
            using var stream = new MemoryStream(); EscapeTimerPresentation.Write(stream, value); stream.Position = 0;
            return EscapeTimerPresentation.Load(stream);
        }
        OamBuffer DrawDocument(EscapeTimerPresentationDocument selected)
        {
            var result = new OamBuffer();
            Draw(0x80a060, selected.Anchors["Label"]);
            Pair(timer.Minutes.Raw, "Minutes"); Pair(timer.Seconds.Raw, "Seconds"); Pair(timer.Centiseconds.Raw, "Centiseconds");
            return result;
            void Pair(byte bcd, string anchor)
            {
                Draw(0x800000 | Word(0x809fd4 + (bcd >> 4) * 2), selected.Anchors[anchor]);
                Draw(0x800000 | Word(0x809fd4 + (bcd & 15) * 2), selected.Anchors[anchor] with { X = (short)(selected.Anchors[anchor].X + selected.DigitSpacing) });
            }
            void Draw(int spritemap, Offset16<SpritePixels> anchor) =>
                SuperMetroid.Verification.Frontend.Map.MapSpritesVerification.DrawImportedSpritemap(rom, result, spritemap,
                    timer.PixelPosition.ToSpritePosition() + anchor, SnesObjAttributeWord.Create(tileNumber: 0, paletteIndex: 5, priority: 0).PaletteBits);
        }
    }

    /// <summary>Reads native escape timer OAM layout bytes.</summary>
    private static byte LookupStream1TimerLayoutRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks flare origin words, independent placement edits, and emitted OAM.</summary>
    internal static void VerifyLookupStream1FlarePlacement(ISnesAddressSpace rom)
    {
        short Word(int address) => unchecked((short)(LittleEndianBytes.Word(LookupStream1FlarePlacementRomByte(rom, address), LookupStream1FlarePlacementRomByte(rom, address + 1))));
        byte[] json = ChargeFlarePlacementExtractor.Extract(rom);
        var stock = ChargeFlarePlacementCatalog.Load(new MemoryStream(json));
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        AssertEqual(0, stock.offsets.Count, "Beam flare stock has zero stored fallback origins");
        for (int mode = 0; mode < 2; mode++)
        for (int direction = 0; direction < 16; direction++)
        {
            var expected = new ChargeFlareOffset { X = Word((mode == 0 ? 0x90c1a8 : 0x90c1dc) + 2 * direction),
                Y = Word((mode == 0 ? 0x90c1c2 : 0x90c1f0) + 2 * direction) };
            AssertEqual(expected.Offset, ChargeFlarePlacementDefinitions.BeamOffset(mode != 0, direction), "Named flare default matches direct native coordinates and aliases");
            AssertEqual(expected.Offset, stock.Resolve(mode != 0, direction), "Installed flare default matches native");
            for (int axis = 0; axis < 2; axis++)
            {
                var document = RequiredJson.Deserialize<ChargeFlarePlacementDocument>(json, options);
                string key = ChargeFlarePlacementDefinitions.Key(mode != 0, direction);
                var before = document.Offsets[key];
                document.Offsets[key] = axis == 0 ? before with { X = (short)(before.X + 7) } : before with { Y = (short)(before.Y + 7) };
                var edited = ChargeFlarePlacementCatalog.Load(new MemoryStream(ChargeFlarePlacementCatalog.Write(document)));
                AssertEqual(1, edited.offsets.Count, "One independent origin edit stores exactly one override");
                for (int selectedMode = 0; selectedMode < 2; selectedMode++)
                for (int selectedDirection = 0; selectedDirection < 16; selectedDirection++)
                    AssertEqual(document.Offsets[ChargeFlarePlacementDefinitions.Key(selectedMode != 0, selectedDirection)].Offset,
                        edited.Resolve(selectedMode != 0, selectedDirection), "Editing a source origin does not edit an independently supplied adjacent alias");
            }
        }
        var grappleDocument = RequiredJson.Deserialize<ChargeFlarePlacementDocument>(GrappleFlarePlacementExtractor.Extract(rom), options);
        var grapple = ChargeFlarePlacementCatalog.LoadGrapple(new MemoryStream(ChargeFlarePlacementCatalog.Write(grappleDocument)));
        AssertEqual(0, grapple.offsets.Count, "Grapple stock has zero stored fallback origins");
        for (int mode = 0; mode < 2; mode++)
        for (int direction = 0; direction < 16; direction++)
        {
            var expected = new ChargeFlareOffset { X = Word((mode == 0 ? 0x9bc14a : 0x9bc19a) + direction * 2),
                Y = Word((mode == 0 ? 0x9bc15e : 0x9bc1ae) + direction * 2) };
            AssertEqual(expected.Offset, ChargeFlarePlacementDefinitions.GrappleOffset(mode != 0, direction), "Grapple direct defaults preserve all native words including packed adjacent bytes");
            AssertEqual(expected.Offset, grapple.Resolve(mode != 0, direction), "Installed Grapple defaults match native");
            for (int axis = 0; axis < 2; axis++)
            {
                var document = RequiredJson.Deserialize<ChargeFlarePlacementDocument>(GrappleFlarePlacementExtractor.Extract(rom), options);
                string key = ChargeFlarePlacementDefinitions.Key(mode != 0, direction);
                var before = document.Offsets[key];
                document.Offsets[key] = axis == 0 ? before with { X = (short)(before.X + 7) } : before with { Y = (short)(before.Y + 7) };
                var edited = ChargeFlarePlacementCatalog.LoadGrapple(new MemoryStream(ChargeFlarePlacementCatalog.Write(document)));
                AssertEqual(1, edited.offsets.Count, "Grapple independent edit stores exactly one override");
                for (int selectedMode = 0; selectedMode < 2; selectedMode++)
                for (int selectedDirection = 0; selectedDirection < 16; selectedDirection++)
                    AssertEqual(document.Offsets[ChargeFlarePlacementDefinitions.Key(selectedMode != 0, selectedDirection)].Offset, edited.Resolve(selectedMode != 0, selectedDirection),
                        "Grapple edits preserve every other independent origin and alias");
            }
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(false, invalid), "Flare placement catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => grapple.Resolve(true, invalid), "Grapple catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => ChargeFlarePlacementDefinitions.GrappleOffset(false, invalid), "Grapple calculated bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => ChargeFlarePlacementDefinitions.BeamOffset(true, invalid), "Flare calculated bounds");
        }
        var compositions = ChargeFlareSpriteCatalog.Load(new MemoryStream(ChargeFlareSpriteExtractor.Extract(rom)));
        var body = CreateSamusIdentityFixture();
        var system = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None);
        var draw = new Action<SpriteFrameBuffer, SamusState, Position16<RoomPixels>, int, TurningFloorTransform?, ChargeFlareArtwork>(system.Animation.DrawFlareComponent);
        ProjectileDrawArtwork flareObjects = InstalledFixtures.ProjectileArtwork();
        var shiftedDocument = RequiredJson.Deserialize<ChargeFlarePlacementDocument>(json, options);
        foreach (string key in shiftedDocument.Offsets.Keys.ToArray())
            shiftedDocument.Offsets[key] = shiftedDocument.Offsets[key] with { X = (short)(shiftedDocument.Offsets[key].X + 7) };
        var shifted = ChargeFlarePlacementCatalog.Load(new MemoryStream(ChargeFlarePlacementCatalog.Write(shiftedDocument)));
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose
        })
        {
            var samus = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
            samus.Presentation.BodyFrames.BindBodyFixture(body);
            int direction = LookupStream1FlarePlacementRomByte(rom, 0x91b629 + (int)pose * 8 + 3);
            bool running = pose is SamusPoseId.MovingRightNormalPose or SamusPoseId.MovingLeftNormalPose;
            short x = Word((running ? 0x90c1dc : 0x90c1a8) + direction * 2);
            short y = Word((running ? 0x90c1f0 : 0x90c1c2) + direction * 2);
            var expected = new OamBuffer();
            DrawImportedFlareSpritemap((SuperMetroidAddressSpace)rom, expected, 0, new((ushort)(100 + x), unchecked((ushort)(100 + y - unchecked((byte)body.GraphicsYOffset(pose))))));
            OamBuffer actual = SpriteFrameBuffer.DrawnOam(frame => draw(frame, samus, Position16<RoomPixels>.Zero, 0, null,
                new ChargeFlareArtwork { Placement = stock, Objects = flareObjects }));
            OamBuffer edited = SpriteFrameBuffer.DrawnOam(frame => draw(frame, samus, Position16<RoomPixels>.Zero, 0, null,
                new ChargeFlareArtwork { Placement = shifted, Objects = flareObjects }));
            AssertTrue(expected.ImageEquals(actual), "Actual flare OAM matches independently positioned native sprite");
            AssertEqual(expected.NextByteOffset / 4, actual.LastFinalizedSpriteCount, "Native flare OAM admission");
            AssertTrue(actual.LastFinalizedSpriteCount > 0, "Focused flare placement fixture emits its native sprite");
            for (int index = 0; index < actual.LastFinalizedSpriteCount; index++)
            {
                AssertEqual((actual.GetEntry(index).Position.X + 7) & 511, edited.GetEntry(index).Position.X, "Actual flare uses independently edited placement");
                AssertEqual(actual.GetEntry(index).Position.Y, edited.GetEntry(index).Position.Y, "X edit preserves actual flare Y");
            }
        }
        Console.WriteLine("Beam/Grapple flare origins:128 direct native words, zero stock overrides,128 independent edits, bounded aliases and four actual OAM fixtures pass.");
    }

    /// <summary>Reads native beam and grapple flare placement bytes.</summary>
    private static byte LookupStream1FlarePlacementRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Creates a fixture bus that rejects cartridge reads during flare OAM drawing.</summary>
    private static CartridgeReadGuard NewLookupFlareForbiddenBus() =>
        CartridgeReadGuard.Unbacked().RejectWrites(address => $"Unexpected flare write {address:X6}");
    /// <summary>Runs the first lookup suite and checks native pose facing, movement, and selected catalog words.</summary>
    internal static void VerifyLookupStream1(ISnesAddressSpace rom)
    {
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            AssertEqual(LookupStream1RomByte(rom, 0x91b629 + pose * 8), SamusPoseRecordCodec.FacingByte((SamusPoseId)pose), "Native named-pose facing case");
            AssertEqual(LookupStream1RomByte(rom, 0x91b62a + pose * 8), SamusPoseRecordCodecTooling.MovementByte((SamusPoseId)pose), "Native named-pose raw movement case");
            AssertEqual(LookupStream1RomByte(rom, 0x91b62b + pose * 8), (byte)SamusPoseDispatchDefinitions.ReadNoInputPose((SamusPoseId)pose), "Native named-pose no-input case");
        }
        Suite(nameof(VerifyLookupStream1SamusPolicyDomains), () => VerifyLookupStream1SamusPolicyDomains(rom));
        Suite(nameof(VerifyLookupStream1MetroidLayout), () => VerifyLookupStream1MetroidLayout(rom));
        Suite(nameof(VerifyLookupStream1PuyoAndQuota), () => VerifyLookupStream1PuyoAndQuota(rom));
        Suite(nameof(VerifyLookupStream1VisualCatalogs), () => VerifyLookupStream1VisualCatalogs(rom));
        Suite(nameof(VerifyLookupStream1DeathDefinitions), () => VerifyLookupStream1DeathDefinitions(rom));
        Suite(nameof(VerifyLookupStream1Sciser), () => VerifyLookupStream1Sciser(rom));
        Suite(nameof(VerifyLookupStream1PowampMotion), () => VerifyLookupStream1PowampMotion(rom));
        Suite(nameof(VerifyLookupStream1CommonFrames), () => VerifyLookupStream1CommonFrames(rom));
        Suite(nameof(VerifyLookupStream1EnemyMovement), () => VerifyLookupStream1EnemyMovement(rom));
        Suite(nameof(VerifyLookupStream1CadencePrograms), () => VerifyLookupStream1CadencePrograms(rom));
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1RomByte(rom, address), LookupStream1RomByte(rom, address + 1));
        foreach (SamusLiquidMedium medium in Enum.GetValues<SamusLiquidMedium>())
        {
            for (int variant = 0; variant < 4; variant++)
            {
                var actual = SamusVerticalMotionDefinitions.Launch(medium, (variant & 1) != 0, ((variant & 2) != 0) ? SamusLaunchKind.WallJump : SamusLaunchKind.Jump);
                int address = 0x909eb9 + 12 * variant + 2 * (int)medium;
                AssertEqual(Word(address), actual.Whole, "stream1 native jump whole speed");
                AssertEqual(Word(address + 6), actual.Fraction, "stream1 native jump fractional speed");
            }
            var hurt = SamusVerticalMotionDefinitions.Knockback(medium);
            AssertEqual(Word(0x909ee9 + 2 * (int)medium), hurt.Whole, "stream1 native knockback whole speed");
            AssertEqual(Word(0x909eef + 2 * (int)medium), hurt.Fraction, "stream1 native knockback fractional speed");
            var bomb = SamusVerticalMotionDefinitions.BombJump(medium);
            AssertEqual(Word(0x909ef5 + 2 * (int)medium), bomb.Whole, "stream1 native bomb whole speed");
            AssertEqual(Word(0x909efb + 2 * (int)medium), bomb.Fraction, "stream1 native bomb fractional speed");
            var gravity = SamusVerticalMotionDefinitions.Gravity(medium);
            AssertEqual(Word(0x909ea7 + 2 * (int)medium), gravity.Whole, "stream1 native whole gravity");
            AssertEqual(Word(0x909ea1 + 2 * (int)medium), gravity.Fraction, "stream1 native fractional gravity");
        }
        foreach (SamusLiquidMedium invalid in new[] { (SamusLiquidMedium)3, (SamusLiquidMedium)ushort.MaxValue })
        {
            for (int variant = 0; variant < 4; variant++)
                AssertThrows<InvalidOperationException>(() => SamusVerticalMotionDefinitions.Launch(invalid, (variant & 1) != 0, ((variant & 2) != 0) ? SamusLaunchKind.WallJump : SamusLaunchKind.Jump), "stream1 invalid launch medium");
            AssertThrows<InvalidOperationException>(() => SamusVerticalMotionDefinitions.Knockback(invalid), "stream1 invalid hurt medium");
            AssertThrows<InvalidOperationException>(() => SamusVerticalMotionDefinitions.BombJump(invalid), "stream1 invalid bomb medium");
            AssertThrows<InvalidOperationException>(() => SamusVerticalMotionDefinitions.Gravity(invalid), "stream1 invalid gravity medium");
        }
        for (int address = 0x90c254; address <= 0x90c28e; address++)
            AssertEqual(LookupStream1RomByte(rom, address), SamusProjectileCooldownDefinitions.ReadByte(address - SamusSourceDefinitionsConstants.FiringTables.Value), "stream1 all native cooldown bytes including padding");
        AssertEqual(LookupStream1RomByte(rom, 0x90c291), SamusProjectileCooldownDefinitions.ReadByte(0x90c291 - SamusSourceDefinitionsConstants.FiringTables.Value), "stream1 bounded spacetime beam cooldown");
        foreach (int invalid in new[] { int.MinValue, 0x90c253, 0x90c28f, 0x90c290, 0x90c292, int.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusProjectileCooldownDefinitions.ReadByte(invalid - SamusSourceDefinitionsConstants.FiringTables.Value), "stream1 invalid cooldown address");
    }

    /// <summary>Reads source bytes for the broad pose and gameplay lookup comparison.</summary>
    private static byte LookupStream1RomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks Powamp wiggle offsets and balloon/body motion tables.</summary>
    private static void VerifyLookupStream1PowampMotion(ISnesAddressSpace rom)
    {
        short Word(int address) => unchecked((short)(LittleEndianBytes.Word(LookupStream1PowampMotionRomByte(rom, address), LookupStream1PowampMotionRomByte(rom, address + 1))));
        for (int phase = 0; phase < 12; phase++)
            AssertEqual(Word(0xa8c1a1 + 2 * phase), PowampMotionDefinitions.WiggleOffset(phase), "Powamp native wiggle triangle");
        for (int pose = 0; pose < 3; pose++)
        {
            AssertEqual(Word(0xa8c277 + 2 * pose), PowampMotionDefinitions.BalloonOffset(pose, false), "Powamp native rising balloon offset");
            AssertEqual(Word(0xa8c27d + 2 * pose), PowampMotionDefinitions.BalloonOffset(pose, true), "Powamp native sinking balloon offset");
        }
        for (int direction = 0; direction < 8; direction++)
        {
            AssertEqual(Word(0x86d21a + 2 * direction), PowampMotionDefinitions.SpikeXAcceleration(direction), "Powamp native spike X acceleration");
            AssertEqual(Word(0x86d22a + 2 * direction), PowampMotionDefinitions.SpikeYAcceleration(direction), "Powamp native spike Y acceleration");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
            AssertThrows<InvalidDataException>(() => PowampMotionDefinitions.WiggleOffset(invalid), "Powamp wiggle domain");
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
            for (int sinking = 0; sinking < 2; sinking++)
                AssertThrows<ArgumentOutOfRangeException>(() => PowampMotionDefinitions.BalloonOffset(invalid, sinking != 0), "Powamp balloon pose domain");
        foreach (int invalid in new[] { int.MinValue, -1, 8, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => PowampMotionDefinitions.SpikeXAcceleration(invalid), "Powamp X direction domain");
            AssertThrows<InvalidDataException>(() => PowampMotionDefinitions.SpikeYAcceleration(invalid), "Powamp Y direction domain");
        }
    }

    /// <summary>Reads Powamp native wiggle and balloon offset words.</summary>
    private static byte LookupStream1PowampMotionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks compiled Bull movement intervals and other enemy motion values.</summary>
    private static void VerifyLookupStream1EnemyMovement(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1EnemyMovementRomByte(rom, address), LookupStream1EnemyMovementRomByte(rom, address + 1));
        for (ushort index = 0; index < 13; index++)
        {
            var intervals = BullMovementDefinitions.Intervals(index);
            AssertEqual(Word(0xa8d895 + 4 * index), intervals.Acceleration, "stream1 Bull acceleration interval");
            AssertEqual(Word(0xa8d897 + 4 * index), intervals.Deceleration, "stream1 Bull deceleration interval");
        }
        for (int direction = 0; direction < 10; direction++)
            AssertEqual(Word(0xa8d871 + 2 * direction), BullMovementDefinitions.ShotAngle(direction), "stream1 Bull compass angle");
        foreach (int invalid in new[] { -1, 10, int.MaxValue })
            AssertThrows<InvalidDataException>(() => BullMovementDefinitions.ShotAngle(invalid), "stream1 Bull invalid direction");
        AssertThrows<InvalidDataException>(() => BullMovementDefinitions.Intervals(13), "stream1 Bull invalid interval");
        for (byte index = 0; index < 6; index++)
            AssertEqual(Word(0xa29f36 + 2 * index), CacatacMovementDefinitions.TravelDistance(index), "stream1 Cacatac patrol blocks");
        foreach (byte invalid in new byte[] { 6, byte.MaxValue })
            AssertThrows<InvalidDataException>(() => CacatacMovementDefinitions.TravelDistance(invalid), "stream1 Cacatac invalid patrol selector");
        for (ushort direction = 0; direction < 20; direction += 2)
            AssertEqual(Word(0x86d96a + direction), CacatacProjectileNativeListAddresses.Entry(CacatacProjectileDefinitions.Program((CacatacSpikeDirection)direction)), "stream1 Cacatac named direction program");
        foreach (ushort invalid in new ushort[] { 1, 19, 20, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => CacatacProjectileDefinitions.Program((CacatacSpikeDirection)invalid), "stream1 Cacatac invalid direction selector");
    }

    /// <summary>Reads native Bull and related enemy motion words.</summary>
    private static byte LookupStream1EnemyMovementRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks charge-flare instruction bytes and related compiled cadence programs.</summary>
    private static void VerifyLookupStream1CadencePrograms(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1CadenceProgramsRomByte(rom, address), LookupStream1CadenceProgramsRomByte(rom, address + 1));
        for (int address = SamusSourceDefinitions.ChargeFlarePrograms.Value; address < 0x90c4b5; address++)
        {
            AssertEqual(LookupStream1CadenceProgramsRomByte(rom, address),
                ChargeFlareAnimationDefinitions.ReadByte(address - SamusSourceDefinitions.ChargeFlarePrograms.Value), "stream1 original flare cadence byte");
            if (address < 0x90c4b4)
                AssertEqual(Word(address), ChargeFlareAnimationDefinitions.ReadWord(address - SamusSourceDefinitions.ChargeFlarePrograms.Value),
                    "stream1 original flare unaligned word");
        }
        foreach (int invalid in new[] { int.MinValue, 0x90c486, 0x90c4b5, int.MaxValue })
            AssertThrows<InvalidDataException>(() => ChargeFlareAnimationDefinitions.ReadByte(invalid - SamusSourceDefinitions.ChargeFlarePrograms.Value),
                "stream1 flare byte domain");
    }

    /// <summary>Reads native charge-flare and animation cadence instructions.</summary>
    private static byte LookupStream1CadenceProgramsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks bank-$90 projectile motion words and rejects odd or adjacent addresses.</summary>
    internal static void VerifyLookupStream1ProjectileMotion(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1ProjectileMotionRomByte(rom, address), LookupStream1ProjectileMotionRomByte(rom, address + 1));
        for (int address = 0x90c2d1; address < 0x90c37b; address += 2)
            AssertEqual(Word(address), SamusProjectileMotionDefinitions.ReadWord(address - SamusSourceDefinitionsConstants.FiringTables.Value), "stream1 all native projectile motion words");
        foreach (int invalid in new[] { int.MinValue, 0x90c2d0, 0x90c2d2, 0x90c37b, int.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusProjectileMotionDefinitions.ReadWord(invalid - SamusSourceDefinitionsConstants.FiringTables.Value), "stream1 exact projectile motion address domain");
        var initialize = new Action<ProjectileInheritanceRecords, SamusProjectileSlot>(BeamFiring.InitializePowerBeamVelocity);
        var stationary = new ProjectileInheritanceRecords();
        for (ushort combination = 0; combination < 16; combination++)
        for (ushort direction = 0; direction < 10; direction++)
        {
            var slot = new SamusProjectileSlot(0) { Type = combination, Direction = direction };
            initialize(stationary, slot);
            int speed = unchecked((short)Word(0x90c2d1 + combination * 4 + (direction is 1 or 3 or 6 or 8 ? 2 : 0)));
            AssertEqual(unchecked((short)(direction is 1 or 2 or 3 ? speed : direction is 6 or 7 or 8 ? -speed : 0)), slot.Velocity.X.SignedRawValue, "stream1 actual initializer X preserves adjacent missile-data overread");
            AssertEqual(unchecked((short)(direction is 0 or 1 or 8 or 9 ? -speed : direction is 3 or 4 or 5 or 6 ? speed : 0)), slot.Velocity.Y.SignedRawValue, "stream1 actual initializer Y preserves adjacent missile-data overread");
        }
    }

    /// <summary>Reads bank-$90 projectile speed and motion words.</summary>
    private static byte LookupStream1ProjectileMotionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks the full bank-$93 projectile selection table against compiled words.</summary>
    internal static void VerifyLookupStream1Selection(ISnesAddressSpace rom)
    {
        for (int address = 0x9383c1; address < 0x9386db; address += 2)
        {
            ushort expected = LittleEndianBytes.Word(LookupStream1SelectionRomByte(rom, address), LookupStream1SelectionRomByte(rom, address + 1));
            AssertEqual(expected, SamusProjectileProgramsVerification.SelectionWord(address),
                $"stream1 original projectile selector/header {address:X6}");
            AssertThrows<InvalidDataException>(() => SamusProjectileSelectionDefinitions.ReadWord(address + 1),
                "stream1 unaligned selector/header rejection");
        }
        foreach (int invalid in new[] { int.MinValue, 0x9283c1, 0x9383bf, 0x9386db, 0x9483c1, int.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusProjectileSelectionDefinitions.ReadWord(invalid),
                "stream1 selector exact address-domain rejection");
    }

    /// <summary>Reads bank-$93 projectile selector words.</summary>
    private static byte LookupStream1SelectionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks bomb-spread launch roles and their native slot-specific values.</summary>
    internal static void VerifyLookupStream1LaunchRoles(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1LaunchRolesRomByte(rom, address), LookupStream1LaunchRolesRomByte(rom, address + 1));
        for (int index = 0; index < 5; index++)
        {
            var launch = SamusBombSpreadLaunchDefinitions.ForSlot(index);
            AssertEqual(Word(0x90d8cf + index * 2), launch.FuseTimer, "spread native fuse");
            AssertEqual(Word(0x90d8d9 + index * 2), launch.XVelocity, "spread native direction/magnitude X");
            AssertEqual(Word(0x90d8e3 + index * 2), launch.YSpeed.Whole, "spread native whole Y");
            AssertEqual(Word(0x90d8ed + index * 2), launch.YSpeed.Fraction, "spread native fraction Y");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusBombSpreadLaunchDefinitions.ForSlot(invalid), "spread native slot rejection");
        for (int index = 0; index < 22; index++)
        {
            var frame = HibashiDefinitions.ActivityFrame(index);
            AssertEqual(Word(0xa68dbb + index * 2), frame.YOffset, "Hibashi native collision rise");
            AssertEqual(Word(0xa68de7 + index * 2), frame.YRadius, "Hibashi native collision radius");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 22, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => HibashiDefinitions.ActivityFrame(invalid), "Hibashi native frame rejection");
        for (ushort selector = 0; selector < 6; selector++)
        {
            AssertEqual(Word(0xa2e5ef + selector * 2), NativeEnemyProgramOracle.EntryOf(DragonNativeListAddresses.Entries, DragonAnimationDefinitions.Program((DragonAnimationSelector)selector)), "Dragon native phase/facing program");
            var role = EscapeEtecoonDefinitions.Initialization(selector);
            int offset = selector & ~1;
            AssertEqual(Word(0xb3e718 + offset), role.Position.X, "Etecoon native role X");
            AssertEqual(Word(0xb3e71e + offset), role.Position.Y, "Etecoon native role Y");
            AssertEqual(Word(0xb3e724 + offset), AnimalsNativeWords.EscapeEtecoonPreInstructions.Word(role.PreInstruction), "Etecoon native role action");
            AssertEqual(Word(0xb3e72a + offset), EscapeEtecoonNativeListAddresses.Entries.Single(entry => entry.Program == role.Program).Entry, "Etecoon native role program");
            AssertEqual(Word(0xb3e730 + offset), role.Velocity.X.RawValue, "Etecoon native role speed");
        }
        foreach (ushort invalid in new ushort[] { 6, 7, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => DragonAnimationDefinitions.Program((DragonAnimationSelector)invalid), "Dragon invalid selector rejection");
            AssertThrows<ArgumentOutOfRangeException>(() => EscapeEtecoonDefinitions.Initialization(invalid), "Etecoon invalid role rejection");
        }
    }

    /// <summary>Reads native bomb-spread launch values by slot.</summary>
    private static byte LookupStream1LaunchRolesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks empty extended enemy frames across their supported native banks.</summary>
    private static void VerifyLookupStream1CommonFrames(ISnesAddressSpace rom)
    {
        byte[] originalBanks = [0xa0, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3];
        AssertTrue(originalBanks.SequenceEqual(CommonEnemyEmptyExtendedFrameDefinitionsTooling.SupportedBanks), "common empty-frame original bank order");
        for (int bank = 0; bank <= byte.MaxValue; bank++)
        {
            bool expected = originalBanks.Contains((byte)bank);
            AssertEqual(expected, CommonEnemyEmptyExtendedFrameToolingDefinitionsConstants.HasFrame((byte)bank, 0x804f), "common empty extended-frame exact bank domain");
            AssertEqual(expected, CommonEnemyEmptyExtendedFrameToolingDefinitions.HasEmptySpritemap((byte)bank, 0x804d), "common empty OAM exact bank domain");
            AssertTrue(!CommonEnemyEmptyExtendedFrameToolingDefinitionsConstants.HasFrame((byte)bank, 0x8050), "common empty frame excludes neighboring pointer");
            if (expected)
            {
                AssertEqual((byte)0, LookupStream1CommonFramesRomByte(rom, ((int)new SnesAddress(checked((byte)bank), checked(0x804d)))), "native common OAM has zero components");
                AssertEqual((byte)1, LookupStream1CommonFramesRomByte(rom, ((int)new SnesAddress(checked((byte)bank), checked(0x804f)))), "native common extended frame has one component");
            }
        }
        AssertEqual((ushort)NativeEnemyProjectileInstruction.Delete, LittleEndianBytes.Word(LookupStream1CommonFramesRomByte(rom, 0x8684fc), LookupStream1CommonFramesRomByte(rom, 0x8684fd)), "shared projectile delete native instruction");
    }

    /// <summary>Reads empty extended enemy frame headers across supported banks.</summary>
    private static byte LookupStream1CommonFramesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks Sciser directional visual frames against native pointer and spritemap data.</summary>
    private static void VerifyLookupStream1Sciser(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1SciserRomByte(rom, address), LookupStream1SciserRomByte(rom, address + 1));
        var frames = SuperMetroid.Core.Enemies.Artwork.SciserVisualDefinitions.Frames();
        AssertEqual(12, frames.Length, "Sciser visual catalog size");
        string[] names = ["right", "left", "down", "up"];
        for (int surface = 0; surface < 4; surface++)
        for (int frame = 0; frame < 3; frame++)
        {
            var actual = frames[surface * 3 + frame];
            ushort pointer = Word(0xa39681 + surface * 24 + frame * 4);
            AssertEqual(pointer, actual.Pointer, "native Sciser visual identity");
            AssertEqual((byte)0xa3, actual.Bank, "Sciser bank");
            AssertEqual($"sciser_upside_{names[surface]}_{frame}", actual.Name, "Sciser stable editable name");
            AssertEqual((ushort)4, Word(0xa30000 | pointer), "native Sciser four-object record size");
        }
    }

    /// <summary>Reads native Sciser directional frames and spritemap pointers.</summary>
    private static byte LookupStream1SciserRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks death-sequence initial frames and movement-type data against native tables.</summary>
    private static void VerifyLookupStream1DeathDefinitions(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1DeathDefinitionsRomByte(rom, address), LookupStream1DeathDefinitionsRomByte(rom, address + 1));
        SamusMovementType[] movements = Enum.GetValues<SamusMovementType>();
        AssertEqual(28, movements.Length, "death retail movement domain");
        foreach (SamusMovementType movement in movements)
            AssertEqual(LookupStream1DeathDefinitionsRomByte(rom, 0x9bb420 + (byte)movement), SamusDeathSequenceDefinitions.InitialFrame(movement), "native movement death phase");
        // The cartridge uploads the explosion as five pages; the atlas keeps them in that upload order.
        for (int page = 0; page < SamusDeathTileAtlasFormat.SegmentCount; page++)
        {
            int explosionPage = SamusDeathTileAtlasFormat.ExplosionPageOfAtlasPage(page);
            AssertEqual(0x9b8000 + explosionPage * SamusDeathTileAtlasFormat.PageByteCount, 0x9b0000 | Word(0x9bb7bf + page * 2),
                "native death page source is the atlas page's explosion page");
            AssertEqual(0x6000 + explosionPage * SamusDeathTileAtlasFormat.PageByteCount / 2, (int)Word(0x9bb7c9 + page * 2),
                "native death page destination is the atlas page's explosion page");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = SamusDeathTileAtlasFormat.ExplosionPageOfAtlasPage(invalid), "death page exact domain");
    }

    /// <summary>Reads native Samus death-sequence selection data.</summary>
    private static byte LookupStream1DeathDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks charge-flare sprite selectors and extracted visual records against native pointers.</summary>
    private static void VerifyLookupStream1VisualCatalogs(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1VisualCatalogsRomByte(rom, address), LookupStream1VisualCatalogsRomByte(rom, address + 1));
        var selectors = ChargeFlareSpriteDefinitions.Selectors;
        var nativePointers = new List<ushort>();
        byte[] json = ChargeFlareSpriteExtractor.Extract(rom);
        var stock = ChargeFlareSpriteCatalog.Load(new MemoryStream(json));
        for (ushort selector = 0; selector < 54; selector++)
        {
            ushort pointer = Word(0x93a1a1 + selector * 2);
            AssertEqual(pointer, ProjectileSpriteNative.Address(selectors[selector]), "native charge flare phased selector");
            if (!nativePointers.Contains(pointer)) nativePointers.Add(pointer);
            var expected = new OamBuffer();
            var actual = new OamBuffer();
            DrawImportedFlareSpritemap((SuperMetroidAddressSpace)rom, expected, selector, new(100, 100));
            stock.Draw(selector, actual, new(100, 100));
            AssertTrue(expected.ImageEquals(actual),
                "calculated flare selector draws exact native composition");
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, "calculated flare native OAM cursor");
        }
        AssertEqual(28, nativePointers.Count, "native unique flare identity count");
        AssertTrue(nativePointers.SequenceEqual(ChargeFlareSpriteDefinitions.NativePointers), "native flare identity order preserved");
        for (int index = 0; index < nativePointers.Count; index++)
            AssertEqual((ushort)(index < 3 ? 1 : index == 3 ? 4 : 3), Word(0x930000 | nativePointers[index]), "native flare object record sizes");
        foreach (int invalid in new[] { int.MinValue, -1, 54, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = selectors[invalid], "charge flare selector bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 28, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = ChargeFlareSpriteDefinitions.NativePointers[invalid], "flare pointer bounds");
        for (int digit = 0; digit < 10; digit++)
        {
            ushort pointer = EscapeTimerPresentationDefinitions.DigitSpritemapPointer(digit);
            AssertEqual(Word(0x809fd4 + digit * 2), pointer, "native escape timer digit pointer");
            AssertEqual((ushort)2, Word(0x800000 | pointer), "native two-object digit record");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 10, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EscapeTimerPresentationDefinitions.DigitSpritemapPointer(invalid), "escape digit bounds");
        var dragon = DragonVisualDefinitions.Frames();
        ushort[] operands = [0xe59d, 0xe5a3, 0xe5a7, 0xe5af, 0xe5b5, 0xe5b9, 0xe5c1, 0xe5c5, 0xe5c9, 0xe5d9, 0xe5dd, 0xe5e1];
        string[] names = ["body_idle_left", "wing_left_0", "wing_left_1", "body_idle_right", "wing_right_0", "wing_right_1",
            "body_attack_left_0", "body_attack_left_1", "body_attack_left_2", "body_attack_right_0", "body_attack_right_1", "body_attack_right_2"];
        AssertEqual(12, dragon.Length, "Dragon visual identity count");
        for (int index = 0; index < dragon.Length; index++)
        {
            AssertEqual(Word(0xa20000 | operands[index]), dragon[index].Pointer, "native Dragon visual pointer");
            AssertEqual((byte)0xa2, dragon[index].Bank, "Dragon visual bank");
            AssertEqual("dragon_" + names[index], dragon[index].Name, "Dragon stable editable identity");
            AssertEqual((ushort)(index is 1 or 2 or 4 or 5 ? 1 : 8), Word(0xa20000 | dragon[index].Pointer), "native Dragon record size");
        }
    }

    /// <summary>Reads native charge-flare visual selector and sprite data.</summary>
    private static byte LookupStream1VisualCatalogsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks Puyo program data and the native projectile quota selector table.</summary>
    private static void VerifyLookupStream1PuyoAndQuota(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1PuyoAndQuotaRomByte(rom, address), LookupStream1PuyoAndQuotaRomByte(rom, address + 1));
        for (ushort argument = 0; argument <= 24; argument += 2)
        {
            ushort pointer = Word(0x84db28 + argument);
            AssertEqual(pointer, SuperMetroid.Core.Rooms.Plms.PlmPreInstructionNativeRoutines.MetroidsCleared(argument), "native Metroid quota dispatcher identity");
            var eventNumber = SuperMetroid.Core.Rooms.Plms.MetroidsClearedEvents.ResolveEvent(argument);
            if (argument < 18)
            {
                AssertTrue(eventNumber is null, "nine no-op quota identities have no event");
                AssertEqual((byte)0x60, LookupStream1PuyoAndQuotaRomByte(rom, 0x840000 | pointer), "native distinct no-op RTS");
            }
            else AssertEqual(Word(0x840000 | (pointer + 9)), (ushort)(eventNumber ?? throw new InvalidOperationException("Verification requires eventNumber.")), "native quota observer event operand");
        }
        foreach (ushort invalid in new ushort[] { 1, 17, 25, 26, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => SuperMetroid.Core.Rooms.Plms.PlmPreInstructionNativeRoutines.MetroidsCleared(invalid), "quota pointer argument domain");
            AssertThrows<InvalidDataException>(() => SuperMetroid.Core.Rooms.Plms.MetroidsClearedEvents.ResolveEvent(invalid), "quota event argument domain");
        }
    }

    /// <summary>Reads Puyo instruction and projectile quota source words.</summary>
    private static byte LookupStream1PuyoAndQuotaRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks Metroid body frames and instruction presentation layout.</summary>
    private static void VerifyLookupStream1MetroidLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1MetroidLayoutRomByte(rom, address), LookupStream1MetroidLayoutRomByte(rom, address + 1));
        var frames = MetroidVisualDefinitions.Frames();
        AssertEqual(4, frames.Length, "Metroid body identities");
        for (int frame = 0; frame < 4; frame++)
        {
            AssertEqual(Word(0xa3e9d1 + frame * 4), frames[frame].Pointer, "native Metroid body identity");
            AssertEqual((byte)0xa3, frames[frame].Bank, "Metroid body bank");
            AssertEqual($"metroid_body_{frame}", frames[frame].Name, "Metroid stable editable name");
            AssertEqual((ushort)(frame == 1 ? 6 : 8), Word(0xa30000 | frames[frame].Pointer), "native Metroid OAM record size");
        }
    }

    /// <summary>Reads native Metroid frame and instruction presentation data.</summary>
    private static byte LookupStream1MetroidLayoutRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks Samus pose policy tables across the complete byte-valued pose domain.</summary>
    private static void VerifyLookupStream1SamusPolicyDomains(ISnesAddressSpace rom)
    {
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            // The six crash poses, and the drained-crouch poses a drained controller installs mid-crash, which read on past the table.
            if (pose is >= 0xc9 and <= 0xce or (int)SamusPoseId.DrainedCrouchingRightPose or (int)SamusPoseId.DrainedCrouchingLeftPose)
            {
                var angles = ShinesparkEchoDefinitions.DepartureAngles((SamusPoseId)pose);
                AssertEqual(LookupStream1SamusPolicyDomainsRomByte(rom, 0x90d4c6 + (pose - 0xc9) * 2), angles.First.TableIndex, "native crash first departure angle");
                AssertEqual(LookupStream1SamusPolicyDomainsRomByte(rom, 0x90d4c7 + (pose - 0xc9) * 2), angles.Second.TableIndex, "native crash opposite departure angle");
            }
            else AssertThrows<InvalidOperationException>(() => ShinesparkEchoDefinitions.DepartureAngles((SamusPoseId)pose), "crash departure exact pose domain");
        }
        foreach (byte invalid in new byte[] { 28, byte.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHudDefinitions.MovementHandler((SamusMovementType)invalid), "HUD movement exact domain");
            AssertThrows<InvalidDataException>(() => SamusAtmosphericEffectDefinitions.WaterSplashFor((SamusMovementType)invalid), "splash exact movement domain");
        }
        foreach (ushort invalid in new ushort[] { 10, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusAtmosphericEffectDefinitions.IsRunningFootContact(invalid), "foot-contact exact frame domain");
        foreach (byte invalid in new byte[] { 16, byte.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusAtmosphericEffectDefinitions.ForCrateriaRoom(new SuperMetroid.Core.Rooms.RoomIdentity(SuperMetroid.Core.Progression.AreaId.Crateria, invalid)), "atmospheric room exact domain");
        foreach (var (header, room) in new[] { new VerifyLookupStream1SamusPolicyDomainsCase(0x91f8, 0), new VerifyLookupStream1SamusPolicyDomainsCase(0x93fe, 5), new VerifyLookupStream1SamusPolicyDomainsCase(0x948c, 7), new VerifyLookupStream1SamusPolicyDomainsCase(0x94fd, 9), new VerifyLookupStream1SamusPolicyDomainsCase(0x9552, 10), new VerifyLookupStream1SamusPolicyDomainsCase(0x957d, 11), new VerifyLookupStream1SamusPolicyDomainsCase(0x95a8, 12), new VerifyLookupStream1SamusPolicyDomainsCase(0x95ff, 14) })
            AssertEqual((byte)room, LookupStream1SamusPolicyDomainsRomByte(rom, 0x8f0000 | header), "native atmospheric room header identity");
    }

    /// <summary>Reads native pose policy bytes, including the special drained poses.</summary>
    private static byte LookupStream1SamusPolicyDomainsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks arm-cannon tile source words and independent source selection across directions.</summary>
    internal static void VerifyLookupStream1ArmCannonTileSources(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1ArmCannonTileSourcesRomByte(rom, address), LookupStream1ArmCannonTileSourcesRomByte(rom, address + 1));
        var nativeSources = new SortedSet<ushort>();
        for (int direction = 0; direction < 10; direction++)
        {
            int list = 0x900000 | Word(0x90c7a5 + direction * 2);
            for (int frame = 1; frame < 4; frame++) nativeSources.Add(Word(list + frame * 2));
        }
        ushort[] expected = nativeSources.ToArray();
        var sources = SamusArmCannonArtworkFormat.TileSourcePointers;
        AssertEqual(expected.Length, sources.Count, "Calculated cannon source count matches distinct native list operands");
        AssertTrue(expected.SequenceEqual(sources), "Calculated cannon source enumeration retains original ascending tile order");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], sources[index], "Calculated cannon source indexing retains native identity");
        for (int word = 0; word <= ushort.MaxValue; word++)
        {
            int nativeIndex = Array.IndexOf(expected, (ushort)word);
            AssertEqual(nativeIndex, sources.IndexOf((ushort)word), "Calculated cannon reverse index preserves complete word domain");
            AssertEqual(nativeIndex >= 0, sources.Contains((ushort)word), "Calculated cannon membership rejects interior addresses");
        }
        foreach (int invalid in new[] { int.MinValue, -1, expected.Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = sources[invalid], "Calculated cannon source index bounds preserve array contract");
        using var directory = new TestScratchDirectory("map-catalog");
        SamusArmCannonArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        byte[] json = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.JsonFileName));
        byte[] png = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.TileFileName));
        var stock = SamusArmCannonArtworkCatalogTooling.Load(new MemoryStream(json), new MemoryStream(png));
        AssertEqual(0, stock.attributes.Count + stock.openingTiles.Count, "Stock cannon selectors require no stored fallback overrides");
        for (int direction = 0; direction < 10; direction++)
        {
            var aim = (SamusProjectileDirection)direction;
            // The cover appearance has no packed word: its character, palette row, priority and mirroring are compared
            // with the native attribute word decoded by the import pipeline's codec.
            AssertEqual(new SnesObjAttributeWord(Word(0x90c791 + direction * 2)).ToSinglePart(), SamusArmCannonArtworkFormat.StockCoverAppearance(aim),
                "Direct calculated cannon cover appearances match the original native attribute words");
            int list = 0x900000 | Word(0x90c7a5 + direction * 2);
            for (int frame = 0; frame < 4; frame++)
                AssertEqual(Word(list + frame * 2), SamusArmCannonArtworkFormat.StockTileSource(aim, frame),
                    "Direct calculated cannon source defaults match original native operands");
        }
        byte[] nativePlanar = expected.SelectMany(pointer => Enumerable.Range(0, 32).Select(offset => LookupStream1ArmCannonTileSourcesRomByte(rom, (0x9a0000 | pointer) + offset))).ToArray();
        AssertEqual(ReferenceIdentity(json, nativePlanar), stock.ContentIdentity, "Calculated cannon selectors preserve canonical native content identity");
        Suite(nameof(VerifySelectors), () => VerifySelectors(stock, -1, 0));
        for (int direction = 0; direction < 10; direction++)
        {
            var document = RequiredJson.Parse(json);
            ushort replacement = expected[(sources.IndexOf(stock.TileSource(direction, 2)) + 1) % expected.Length];
            document.Required("coverTiles").Required(direction)[1] = sources.IndexOf(replacement);
            document.Required("coverAppearances").Required(direction)["flipX"] = !stock.CoverAppearance(direction).FlipsHorizontally;
            byte[] changedJson = System.Text.Encoding.UTF8.GetBytes(document.ToJsonString());
            var changed = SamusArmCannonArtworkCatalogTooling.Load(new MemoryStream(changedJson), new MemoryStream(png));
            VerifySelectors(changed, direction, replacement);
            AssertEqual(ReferenceIdentity(changedJson, nativePlanar), changed.ContentIdentity, "Independent cannon selector edits preserve canonical hash framing/order");
            VerifySelectors(stock, -1, 0);
        }
        foreach (int invalid in new[] { int.MinValue, -1, 10, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.CoverAppearance(invalid), "Cannon cover appearance bounds remain exact");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.TileSource(invalid, 0), "Cannon tile direction bounds remain exact");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.TileSource(0, invalid), "Cannon cover frame bounds remain exact");
        Verify(stock, false);
        var image = IndexedPng.Read(new MemoryStream(png), new(checked((ushort)(sources.Count * 8)), 8));
        byte[] pixels = (byte[])image.Pixels.Clone();
        pixels[5 * 8] ^= 1;
        using var editedPng = new MemoryStream();
        IndexedPng.Write(editedPng, new(image.Size.X, image.Size.Y), pixels, image.Colours);
        editedPng.Position = 0;
        var edited = SamusArmCannonArtworkCatalogTooling.Load(new MemoryStream(json), editedPng);
        Verify(edited, true);
        Verify(stock, false);
        AssertTrue(!stock.TryResolveTile(0x9b0000 | expected[0], 32, out _), "Cannon transfer retains native bank boundary");
        AssertTrue(!stock.TryResolveTile(0x9a0000 | expected[0], 31, out _), "Cannon transfer retains native byte-count boundary");
        AssertTrue(!stock.TryResolveTile((0x9a0000 | expected[0]) + 1, 32, out _), "Cannon transfer rejects interior source address");

        void VerifySelectors(SamusArmCannonArtworkCatalog catalog, int editedDirection, ushort replacement)
        {
            for (int direction = 0; direction < 10; direction++)
            {
                SpritePart expectedAppearance = new SnesObjAttributeWord(Word(0x90c791 + direction * 2)).ToSinglePart();
                if (direction == editedDirection) expectedAppearance = expectedAppearance with { Flip = expectedAppearance.Flip ^ TileFlip.Horizontal };
                AssertEqual(expectedAppearance, catalog.CoverAppearance(direction), "Calculated cover reflections preserve native appearances and independent edits");
                int list = 0x900000 | Word(0x90c7a5 + direction * 2);
                for (int frame = 0; frame < 4; frame++)
                {
                    ushort expectedSource = direction == editedDirection && frame == 2 ? replacement : Word(list + frame * 2);
                    AssertEqual(expectedSource, catalog.TileSource(direction, frame), "Calculated cannon orientation/frame preserves native selections and independent edits");
                }
            }
        }

        static string ReferenceIdentity(byte[] jsonBytes, byte[] planar)
        {
            using var document = System.Text.Json.JsonDocument.Parse(jsonBytes);
            var root = document.RootElement;
            return SelectedPresentationHash.Create("samus-arm-cannon-v3", content =>
            {
                foreach (var descriptor in root.GetProperty("poseDescriptors").EnumerateArray()) content.Append("pose descriptor", descriptor.GetInt32());
                content.Append("drawing data", root.GetProperty("drawingData").EnumerateArray().Select(value => (byte)value.GetInt32()).ToArray());
                foreach (var appearance in root.GetProperty("coverAppearances").EnumerateArray())
                {
                    int palette = appearance.TryGetProperty("palette", out var paletteValue) && paletteValue.ValueKind != System.Text.Json.JsonValueKind.Null
                        ? paletteValue.GetInt32() : 0;
                    AppendCanonicalSpritePart(content, CannonCoverAppearance(
                        appearance.GetProperty("tileRow").GetInt32(), appearance.GetProperty("tileColumn").GetInt32(),
                        palette, appearance.GetProperty("priority").GetInt32(),
                        appearance.GetProperty("flipX").GetBoolean(), appearance.GetProperty("flipY").GetBoolean()));
                }
                foreach (var direction in root.GetProperty("coverTiles").EnumerateArray())
                    foreach (var tile in direction.EnumerateArray()) content.Append("cover tile", tile.GetInt32());
                content.Append("characters", planar);
            });
        }
        void Verify(SamusArmCannonArtworkCatalog catalog, bool hasEdit)
        {
            for (int index = 0; index < expected.Length; index++)
            {
                int source = 0x9a0000 | expected[index];
                AssertTrue(catalog.TryResolveTile(source, 32, out var tile), "Actual cannon transfer resolves every calculated identity");
                for (int offset = 0; offset < 32; offset++)
                {
                    byte native = LookupStream1ArmCannonTileSourcesRomByte(rom, source + offset);
                    if (hasEdit && index == 5 && offset == 0) native ^= 0x80;
                    AssertEqual(native, tile.Span[offset], "Calculated identity retains original tile bytes and isolated edited pixel");
                }
            }
        }
    }

    /// <summary>Reads native arm-cannon tile source pointers by direction.</summary>
    private static byte LookupStream1ArmCannonTileSourcesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>
    /// Appends one sprite piece in the canonical field order every Samus artwork content identity frames its pieces
    /// with: offset, size, character, both mirrorings, palette row and priority.
    /// </summary>
    private static void AppendCanonicalSpritePart(SelectedPresentationHash content, SpritePart part)
    {
        content.Append("x", part.Offset.X);
        content.Append("y", part.Offset.Y);
        content.Append("large", part.Large ? 1 : 0);
        content.Append("character", part.Character);
        content.Append("flip-horizontal", part.FlipsHorizontally ? 1 : 0);
        content.Append("flip-vertical", part.FlipsVertically ? 1 : 0);
        content.Append("palette", part.Palette);
        content.Append("priority", part.Priority);
    }

    /// <summary>A cover appearance's single small piece at the origin, from its document fields.</summary>
    private static SpritePart CannonCoverAppearance(int tileRow, int tileColumn, int palette, int priority, bool flipHorizontally, bool flipVertically) =>
        SpritePart.Single(tileRow * SpriteArtwork.CharactersPerRow + tileColumn, palette, priority,
            (flipHorizontally ? TileFlip.Horizontal : TileFlip.None) | (flipVertically ? TileFlip.Vertical : TileFlip.None));
    /// <summary>Checks arm-cannon drawing commands and extracted placement through native OAM results.</summary>
    internal static void VerifyLookupStream1CannonDrawingControls(ISnesAddressSpace rom)
    {
        using var directory = new TestScratchDirectory("map-catalog");
        SamusArmCannonArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        byte[] json = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.JsonFileName));
        byte[] png = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.TileFileName));
        var document = RequiredJson.Deserialize<SamusArmCannonArtworkDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var tiles = RoomCharacterAtlasPng.Read(new MemoryStream(png), SamusArmCannonArtworkFormat.TileSourcePointers.Count * 32);
        SamusArmCannonArtworkCatalog Load(SamusArmCannonArtworkDocument value) => SamusArmCannonArtworkCatalog.FromPlacement(
            SamusArmCannonArtworkCatalog.LoadPlacement(new MemoryStream(SamusArmCannonArtworkCatalog.Write(value))), tiles);
        var stock = Load(document);
        Dictionary<int, byte> Stored(SamusArmCannonArtworkCatalog value) => value.drawingData;
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1CannonDrawingControlsRomByte(rom, address), LookupStream1CannonDrawingControlsRomByte(rom, address + 1));
        var headers = new HashSet<int>();
        var descriptors = new SortedSet<int>();
        for (int pose = 0; pose < 253; pose++)
        {
            int pointer = Word(0x90C7DF + pose * 2);
            descriptors.Add(pointer);
            headers.Add(pointer); headers.Add(pointer + 1);
            if ((LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | pointer) & 0x80) != 0)
            { headers.Add(pointer + 2); headers.Add(pointer + 3); }
        }
        AssertEqual(130, headers.Count, "cannon original distinct direction/mode control-byte count");
        for (int address = 0xCC21; address < 0xCC39; address++) headers.Add(address);
        var aliases = new Dictionary<int, int>();
        int[] boundaries = descriptors.Append(0xCC21).ToArray();
        for (int descriptor = 0; descriptor < boundaries.Length - 1; descriptor++)
        {
            int pointer = boundaries[descriptor], next = boundaries[descriptor + 1];
            if ((LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | pointer) & 0x80) != 0 || next - pointer <= 4) continue;
            bool constantPair = true;
            for (int address = pointer + 4; address < next; address++)
                constantPair &= LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | address) == LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | (pointer + 2 + ((address - pointer) & 1)));
            if (constantPair)
                for (int address = pointer + 4; address < next; address++) aliases.Add(address, pointer + 2 + ((address - pointer) & 1));
            else
            {
                bool fixedX = true;
                for (int address = pointer + 4; address < next; address += 2)
                    fixedX &= LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | address) == LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | (pointer + 2));
                if (fixedX)
                {
                    for (int address = pointer + 4; address < next; address += 2) aliases.Add(address, pointer + 2);
                    int frames = (next - pointer - 2) / 2;
                    if (frames is 6 or 10)
                    {
                        bool repeatedHalf = true;
                        for (int address = pointer + 3 + frames; address < next; address += 2)
                            repeatedHalf &= LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | address) == LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | (address - frames));
                        if (repeatedHalf)
                            for (int address = pointer + 3 + frames; address < next; address += 2)
                                aliases.Add(address, address - frames);
                    }
                }
            }
        }
        foreach ((int earlierPose, int laterPose) in new VerifyLookupStream1SamusPolicyDomainsCase[]
        {
            new VerifyLookupStream1SamusPolicyDomainsCase(0x01,0x02), new VerifyLookupStream1SamusPolicyDomainsCase(0x07,0x08), new VerifyLookupStream1SamusPolicyDomainsCase(0x0F,0x10), new VerifyLookupStream1SamusPolicyDomainsCase(0x11,0x12),
            new VerifyLookupStream1SamusPolicyDomainsCase(0x13,0x14), new VerifyLookupStream1SamusPolicyDomainsCase(0x17,0x18), new VerifyLookupStream1SamusPolicyDomainsCase(0x51,0x52), new VerifyLookupStream1SamusPolicyDomainsCase(0x69,0x6A),
            new VerifyLookupStream1SamusPolicyDomainsCase(0x6B,0x6C), new VerifyLookupStream1SamusPolicyDomainsCase(0x67,0x68), new VerifyLookupStream1SamusPolicyDomainsCase(0x2D,0x2E), new VerifyLookupStream1SamusPolicyDomainsCase(0x6D,0x6E),
            new VerifyLookupStream1SamusPolicyDomainsCase(0x6F,0x70), new VerifyLookupStream1SamusPolicyDomainsCase(0x27,0x28), new VerifyLookupStream1SamusPolicyDomainsCase(0x73,0x74), new VerifyLookupStream1SamusPolicyDomainsCase(0x49,0x4A),
            new VerifyLookupStream1SamusPolicyDomainsCase(0x75,0x76), new VerifyLookupStream1SamusPolicyDomainsCase(0x77,0x78), new VerifyLookupStream1SamusPolicyDomainsCase(0x55,0x56),
        })
        {
            int earlier = Word(0x90C7DF + earlierPose * 2), later = Word(0x90C7DF + laterPose * 2);
            int end = boundaries[Array.IndexOf(boundaries, later) + 1] - later;
            for (int offset = 3; offset < end; offset += 2)
            {
                AssertEqual(LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | (earlier + offset)), LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | (later + offset)),
                    "native horizontally reflected poses share this vertical profile");
                if (!aliases.ContainsKey(later + offset)) aliases.Add(later + offset, earlier + offset);
            }
        }
        foreach ((int earlierPose, int laterPose) in new VerifyLookupStream1SamusPolicyDomainsCase[] { new VerifyLookupStream1SamusPolicyDomainsCase(0x03,0x04), new VerifyLookupStream1SamusPolicyDomainsCase(0x15,0x16), new VerifyLookupStream1SamusPolicyDomainsCase(0x85,0x86) })
        {
            int earlierY = Word(0x90C7DF + earlierPose * 2) + 7, laterY = Word(0x90C7DF + laterPose * 2) + 7;
            AssertEqual(LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | earlierY), LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | laterY), "native fully upward secondary pose shares Y");
            aliases.Add(laterY, earlierY);
        }
        var reflections = new Dictionary<int, int>();
        // Native pose identities independently select the paired descriptors; no
        // production reflection selector participates in this expected mapping.
        foreach ((int earlierPose, int laterPose) in new VerifyLookupStream1SamusPolicyDomainsCase[]
        {
            new VerifyLookupStream1SamusPolicyDomainsCase(0x01,0x02), new VerifyLookupStream1SamusPolicyDomainsCase(0x03,0x04), new VerifyLookupStream1SamusPolicyDomainsCase(0x05,0x06), new VerifyLookupStream1SamusPolicyDomainsCase(0x07,0x08),
            new VerifyLookupStream1SamusPolicyDomainsCase(0x0B,0x0C), new VerifyLookupStream1SamusPolicyDomainsCase(0x0F,0x10), new VerifyLookupStream1SamusPolicyDomainsCase(0x11,0x12), new VerifyLookupStream1SamusPolicyDomainsCase(0x13,0x14),
            new VerifyLookupStream1SamusPolicyDomainsCase(0x15,0x16), new VerifyLookupStream1SamusPolicyDomainsCase(0x51,0x52), new VerifyLookupStream1SamusPolicyDomainsCase(0x69,0x6A), new VerifyLookupStream1SamusPolicyDomainsCase(0x6B,0x6C),
            new VerifyLookupStream1SamusPolicyDomainsCase(0x67,0x68), new VerifyLookupStream1SamusPolicyDomainsCase(0x2B,0x2C), new VerifyLookupStream1SamusPolicyDomainsCase(0x6D,0x6E), new VerifyLookupStream1SamusPolicyDomainsCase(0x6F,0x70),
            new VerifyLookupStream1SamusPolicyDomainsCase(0x27,0x28), new VerifyLookupStream1SamusPolicyDomainsCase(0x71,0x72), new VerifyLookupStream1SamusPolicyDomainsCase(0x73,0x74), new VerifyLookupStream1SamusPolicyDomainsCase(0x85,0x86),
            new VerifyLookupStream1SamusPolicyDomainsCase(0x49,0x4A), new VerifyLookupStream1SamusPolicyDomainsCase(0x75,0x76), new VerifyLookupStream1SamusPolicyDomainsCase(0x77,0x78), new VerifyLookupStream1SamusPolicyDomainsCase(0x55,0x56),
        })
        {
            int earlier = Word(0x90C7DF + earlierPose * 2), later = Word(0x90C7DF + laterPose * 2);
            bool variableDirection = (LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | later) & 0x80) != 0;
            int first = variableDirection ? 4 : 2;
            int end = variableDirection ? boundaries[Array.IndexOf(boundaries, later) + 1] - later : first + 2;
            for (int offset = first; offset < end; offset += 2)
            {
                int source = earlier + offset, target = later + offset;
                byte expected = unchecked((byte)(-unchecked((sbyte)LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | source)) - 8));
                AssertEqual(expected, LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | target), "native paired cover spans reflect around Samus origin");
                reflections.Add(target, source);
            }
        }
        AssertEqual(29, reflections.Count, "cannon native reflected origin count");
        AssertEqual(290, aliases.Count, "cannon original repeated coordinate-byte domain");
        const int start = 0xC9D9, count = 608, coordinateCount = 135;
        AssertEqual(coordinateCount, Stored(stock).Count, "cannon stores exactly required coordinate basis");
        for (int index = 0; index < count; index++)
        {
            ushort address = (ushort)(start + index);
            byte expected = LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | address);
            bool calculated = SamusArmCannonArtworkFormat.TryStockDrawingByte(address - start, out int value);
            AssertEqual(headers.Contains(address), calculated, "cannon only original controls/cost aliases are calculated");
            if (calculated) AssertEqual(expected, value, "cannon direct original header/cost calculation");
            bool alias = SamusArmCannonArtworkFormat.TryStockCoordinateSource(address - start, out int source);
            AssertEqual(aliases.ContainsKey(address), alias, "cannon direct native constant-origin domain");
            if (alias)
            {
                AssertEqual(aliases[address] - start, source, "cannon direct original first-pair source");
                AssertTrue(source < address - start, "cannon coordinate aliases strictly decrease");
            }
            bool reflection = SamusArmCannonArtworkFormat.TryStockReflectedXSource(address - start, out int reflectedSource);
            AssertEqual(reflections.ContainsKey(address), reflection, "cannon native reflected-origin domain");
            if (reflection)
            {
                AssertEqual(reflections[address] - start, reflectedSource, "cannon original opposite-facing source");
                AssertTrue(reflectedSource < address - start && !calculated && !alias, "cannon reflection is an acyclic independent source relation");
                AssertEqual(expected, SamusArmCannonArtworkFormat.ReflectCoverX(LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | (start + reflectedSource))), "cannon reflected native default");
            }
            AssertEqual(!calculated && !alias && !reflection, Stored(stock).ContainsKey(index), "cannon exact coordinate storage membership");
            AssertEqual(expected, stock.ReadDrawingByteAt(address), "cannon complete installed byte window");
            int[] drawing = document.DrawingData.ToArray(); drawing[index] ^= 0xFF;
            var edited = Load(document with { DrawingData = drawing });
            int expectedStored = 0;
            for (int selected = 0; selected < count; selected++)
            {
                int selectedAddress = start + selected;
                bool mandatory = !headers.Contains(selectedAddress) && !aliases.ContainsKey(selectedAddress) && !reflections.ContainsKey(selectedAddress);
                int selectedDefault = aliases.TryGetValue(selectedAddress, out int nativeSource)
                    ? drawing[nativeSource - start]
                    : reflections.TryGetValue(selectedAddress, out int reflectedNativeSource)
                        ? unchecked((byte)(-unchecked((sbyte)drawing[reflectedNativeSource - start]) - 8))
                        : document.DrawingData[selected];
                bool shouldStore = mandatory || drawing[selected] != selectedDefault;
                if (shouldStore) expectedStored++;
                AssertEqual(shouldStore, Stored(edited).ContainsKey(selected), "cannon exact supplied-basis exception membership");
            }
            AssertEqual(expectedStored, Stored(edited).Count, "cannon stores only basis and independently supplied differences");
            for (int other = 0; other < count; other++)
                AssertEqual((byte)(other == index ? drawing[index] : document.DrawingData[other]),
                    edited.ReadDrawingByteAt((ushort)(start + other)), "cannon independent control/coordinate/cost edit");
            AssertTrue(stock.ContentIdentity != edited.ContentIdentity, "cannon every supplied byte changes selected identity");
        }
        foreach (ushort address in new ushort[] { 0, start - 1, start + count, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => stock.ReadDrawingByteAt(address), "cannon drawing bounds remain exact");
        Console.WriteLine("Cannon drawing:130 native controls,24 cost aliases,290 shared coordinates,29 reflected origins,135 exact basis bytes and608 independent edits pass.");
    }

    /// <summary>Reads native arm-cannon drawing control and placement words.</summary>
    private static byte LookupStream1CannonDrawingControlsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks atmospheric sprite attributes and independently editable shared frame words.</summary>
    internal static void VerifyLookupStream1AtmosphericAttributes(ISnesAddressSpace rom)
    {
        // The appearances are domain pieces; the native words are decoded by the import pipeline's attribute codec and
        // compared by character, palette row, priority and mirroring.
        ushort[] NativeWords(int address) => Enumerable.Range(0, 4).Select(frame =>
            LittleEndianBytes.Word(LookupStream1AtmosphericAttributesRomByte(rom, address + frame * 2), LookupStream1AtmosphericAttributesRomByte(rom, address + frame * 2 + 1))).ToArray();
        ushort[] oneWords = NativeWords(0x908C0F), sharedWords = NativeWords(0x908C17);
        SpritePart[] one = [.. oneWords.Select(word => new SnesObjAttributeWord(word).ToSinglePart())];
        SpritePart[] shared = [.. sharedWords.Select(word => new SnesObjAttributeWord(word).ToSinglePart())];
        var stock = new SamusAtmosphericArtworkCatalog(one, shared);
        string Identity(SpritePart[] first, SpritePart[] second) => SelectedPresentationHash.Create(nameof(SamusAtmosphericArtworkCatalog) + ".v2", hash =>
        {
            hash.Append("type one", first.Length);
            foreach (SpritePart part in first) AppendCanonicalSpritePart(hash, part);
            hash.Append("shared type four", second.Length);
            foreach (SpritePart part in second) AppendCanonicalSpritePart(hash, part);
        });
        AssertEqual(Identity(one, shared), stock.ContentIdentity, "atmospheric canonical selected identity unchanged");
        AssertEqual(0, stock.typeOne.Count, "atmospheric footsteps contain no stock overrides");
        AssertEqual(0, stock.sharedTypeFour.Count, "atmospheric lava/dust contains no stock overrides");
        for (int kind = 0; kind < 2; kind++)
        for (int frame = 0; frame < 4; frame++)
        {
            ushort expectedWord = (kind == 0 ? oneWords : sharedWords)[frame];
            SpritePart expected = (kind == 0 ? one : shared)[frame];
            AssertEqual(expected, SamusAtmosphericArtworkDefinitions.Attributes(kind == 0, frame), "atmospheric direct native default appearance");
            var packed = new SnesObjAttributeWord(expectedWord);
            AssertEqual(SamusAtmosphericArtworkDefinitions.Palette, packed.PaletteIndex, "atmospheric required palette basis");
            AssertEqual(SamusAtmosphericArtworkDefinitions.Priority, packed.Priority, "atmospheric required priority basis");
            AssertEqual((kind == 0 ? SamusAtmosphericArtworkDefinitions.FootstepFirstTile : SamusAtmosphericArtworkDefinitions.LavaDustFirstTile) + frame,
                packed.TileNumber, "atmospheric nine-bit tile progression");
            SpritePart[] editOne = one.ToArray(), editShared = shared.ToArray();
            // Every bit of the native word inverted: character, palette row, priority and both mirrorings all change.
            SpritePart changed = new SnesObjAttributeWord((ushort)(expectedWord ^ 0xFFFF)).ToSinglePart();
            (kind == 0 ? editOne : editShared)[frame] = changed;
            var edited = new SamusAtmosphericArtworkCatalog(editOne, editShared);
            AssertEqual(1, (kind == 0 ? edited.typeOne : edited.sharedTypeFour).Count, "atmospheric one whole-appearance supplied exception");
            AssertEqual(0, (kind == 0 ? edited.sharedTypeFour : edited.typeOne).Count, "atmospheric other list unchanged");
            AssertEqual(Identity(editOne, editShared), edited.ContentIdentity, "atmospheric edited identity uses exact supplied appearances");
            editOne[frame] = SpritePart.Single(0, 0, 0); editShared[frame] = SpritePart.Single(0, 0, 0);
            foreach (byte type in new byte[] { 1, 4, 6, 7 })
            for (byte other = 0; other < 4; other++)
            {
                AssertTrue(edited.TryResolve(type, other, out SpritePart value), "atmospheric admitted type/frame");
                AssertEqual((type == 1) == (kind == 0) && other == frame ? changed : (type == 1 ? one : shared)[other], value,
                    "atmospheric supplied whole appearance and caller-input isolation");
            }
        }
        AssertTrue(!stock.TryResolve(2, 0, out _), "atmospheric null pointer remains outside installed domain");
        AssertTrue(!stock.TryResolve(1, 4, out _), "atmospheric next frame rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = stock.TypeOne[-1], "atmospheric negative view index");
        Console.WriteLine("Atmospheric attributes: eight direct native defaults, exact OBJ field basis, zero stock overrides, eight full-word edits and canonical identities pass.");
    }

    /// <summary>Reads native atmospheric sprite attribute words.</summary>
    private static byte LookupStream1AtmosphericAttributesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks extracted escape typewriter glyph and placement data against native text drawing.</summary>
    internal static void VerifyLookupStream1EscapeText(ISnesAddressSpace rom)
    {
        byte[] json = EscapeTypewriterExtractor.Extract(rom);
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var document = RequiredJson.Deserialize<EscapeTypewriterDocument>(json, options);
        EscapeTypewriterPresentation Load(EscapeTypewriterDocument value)
        {
            using var output = new MemoryStream();
            EscapeTypewriterPresentation.Write(output, value);
            output.Position = 0;
            return EscapeTypewriterPresentation.Load(output);
        }
        var stock = Load(document);
        int Stored(EscapeTypewriterProgram program) => ((EscapeTypewriterPresentation.LineSequence)program.Lines).overrides.Count;
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1EscapeTextRomByte(rom, address), LookupStream1EscapeTextRomByte(rom, address + 1));
        int totalFrames = 0;
        foreach (EscapeTypewriterProgramId id in new[] { EscapeTypewriterProgramId.Ceres, EscapeTypewriterProgramId.Zebes })
        {
            var program = stock.Get(id);
            var original = document.Programs[id.ToString()].Lines;
            AssertEqual(original.Length, EscapeTypewriterDefinitions.LineCount(id), "escape text required line grouping");
            AssertEqual(0, Stored(program), "escape text stock contains no sampled line overrides");
            for (int index = 0; index < original.Length; index++)
            {
                var expected = new EscapeTypewriterLine(original[index].Cell, original[index].Text);
                AssertEqual(expected, EscapeTypewriterDefinitions.Line(id, index), "escape text direct original placement and wording");
                AssertEqual(expected, program.Lines[index], "escape text installed line");
                var editedLines = original.ToArray();
                editedLines[index] = original[index] with { Text = "TEST!", Cell = original[index].Cell + 7 };
                var programs = new Dictionary<string, EscapeTypewriterProgramDocument>(document.Programs)
                { [id.ToString()] = new() { Lines = editedLines } };
                var edited = Load(document with { Programs = programs });
                AssertEqual(1, Stored(edited.Get(id)), "escape text single supplied line exception");
                for (int other = 0; other < original.Length; other++)
                    AssertEqual(other == index ? new EscapeTypewriterLine(editedLines[index].Cell, "TEST!")
                        : program.Lines[other], edited.Get(id).Lines[other], "escape text independent line edit");
            }
            // Independently interpret original control words and characters; the production
            // side receives only the installed calculated view and no cartridge reader.
            ushort tileBase = id == EscapeTypewriterProgramId.Ceres ? (ushort)0x3582 : (ushort)0x2610;
            // The program's words are VRAM tilemap words; the port types at cells of the overlay over that tilemap.
            int tilemapWord = id == EscapeTypewriterProgramId.Ceres ? GameplayVramLayout.Bg1TilemapWord : GameplayVramLayout.Bg2TilemapWord;
            var actual = new EscapeTypewriterState(program, BackgroundTileWordBytes.Tile(tileBase));
            var actualVram = new SnesVram(); var expectedVram = new SnesVram();
            int cursor = EscapeTypewriterDefinitions.SourceAddress(id), glyphs = 0;
            ushort destination = 0, delay = 0, timer = 0;
            bool done = false;
            for (int frame = 0; !done && frame < 1000; frame++)
            {
                bool click = false;
                if (timer != 0) timer--;
                else
                {
                    timer = delay;
                    while (true)
                    {
                        ushort command = Word(cursor);
                        if (command == 0) { done = true; break; }
                        if (command == 1) { delay = Word(cursor + 2); cursor += 4; continue; }
                        if (command == 13) { destination = Word(cursor + 2); cursor += 4; continue; }
                        byte character = LookupStream1EscapeTextRomByte(rom, cursor++);
                        if (character != ' ')
                        {
                            int glyph = character == '!' ? '[' : character;
                            expectedVram.ExecuteWordTransfer([(ushort)(tileBase + glyph - 'A')], checked((ushort)(destination - tilemapWord)), 1);
                            click = ++glyphs % 2 == 0;
                        }
                        destination++;
                        break;
                    }
                }
                AssertEqual(done, actual.StepInto(actualVram), "escape text native completion");
                AssertEqual(destination - tilemapWord, actual.Cell, "escape text native cell");
                AssertEqual(delay, actual.Delay, "escape text native delay");
                AssertEqual(timer, actual.DelayTimer.RawValue, "escape text native countdown");
                AssertEqual(glyphs, actual.GlyphsWritten, "escape text native glyph count");
                AssertEqual(click, actual.ClickRequested, "escape text native click order");
                AssertTrue(expectedVram.ImageEquals(actualVram), "escape text native per-character VRAM");
                totalFrames++;
            }
            AssertTrue(done, "escape text native program terminates");
            var extraLines = original.Append(new EscapeTypewriterLineDocument { Cell = 0x400, Text = "EXTRA!" }).ToArray();
            var extraPrograms = new Dictionary<string, EscapeTypewriterProgramDocument>(document.Programs)
            { [id.ToString()] = new() { Lines = extraLines } };
            var extra = Load(document with { Programs = extraPrograms }).Get(id);
            AssertEqual(original.Length + 1, extra.Lines.Count, "escape text accepts independently added line");
            AssertEqual(new EscapeTypewriterLine(0x400, "EXTRA!"), extra.Lines[^1], "escape text preserves added content");
            AssertEqual(1, Stored(extra), "escape text stores only added line");
            extraPrograms[id.ToString()] = new() { Lines = original.Take(1).ToArray() };
            AssertEqual(1, Load(document with { Programs = extraPrograms }).Get(id).Lines.Count, "escape text preserves shortened document");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = program.Lines[-1], "escape text negative line");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = program.Lines[original.Length], "escape text next line");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Get(EscapeTypewriterProgramId.None), "escape text rejects absent program");
        Console.WriteLine($"Escape text: five direct native lines, zero stock overrides, independent edits/line counts and {totalFrames} actual native-oracle frames pass.");
    }

    /// <summary>Reads escape typewriter glyph and presentation bytes.</summary>
    private static byte LookupStream1EscapeTextRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks Samus visor color extraction, channel relationships, and independent edits.</summary>
    internal static void VerifyLookupStream1VisorColors(ISnesAddressSpace rom)
    {
        var document = RequiredJson.Deserialize<SamusVisorColorDocument>(
            SamusVisorColorExtractor.Extract(rom), new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        SamusVisorColorCatalog Load(SamusVisorColorDocument value) =>
            SamusVisorColorFile.Read(new MemoryStream(SamusVisorColorFile.Write(value)));
        var stock = Load(document);
        Dictionary<ColourCell, RgbColor> Stored(SamusVisorColorCatalog value) => value.colors;
        ushort Native(int index)
        {
            int address = SamusSourceDefinitions.VisorColors.Value + index * 2;
            return LittleEndianBytes.Word(LookupStream1VisorColorsRomByte(rom, address), LookupStream1VisorColorsRomByte(rom, address + 1));
        }
        AssertEqual(0, Stored(stock).Count, "visor stock stores no sampled fallback colors");
        AssertEqual(Native(0), SamusVisorColorDefinitions.WideningStart, "visor required widening basis");
        AssertEqual(Native(3), SamusVisorColorDefinitions.FullBeamStart, "visor required full-beam basis");
        for (int channel = 0; channel < 3; channel++)
            AssertEqual(SamusVisorColorDefinitions.DarkeningStep,
                ((Native(3) >> (channel * 5)) & 31) - ((Native(4) >> (channel * 5)) & 31), "visor required darkening basis");
        for (int edited = 0; edited < SamusVisorColorFormat.ColorCount; edited++)
        {
            AssertEqual(Native(edited), SamusVisorColorDefinitions.Color(edited), $"visor direct native default {edited}");
            var changedColors = document.Colors.ToArray();
            ushort changedWord = (ushort)(Native(edited) ^ 0x7FFF);
            changedColors[edited] = new PaletteRgb5 { Red = changedWord & 31, Green = (changedWord >> 5) & 31, Blue = changedWord >> 10 };
            var changed = Load(document with { Colors = changedColors });
            AssertEqual(1, Stored(changed).Count, "visor independent edit stores exactly one exception");
            AssertEqual(changedWord, Stored(changed)[new ColourCell(0, edited)], "visor exception preserves supplied RGB5");
            for (int index = 0; index < SamusVisorColorFormat.ColorCount; index++)
                AssertEqual(index == edited ? changedWord : Native(index), changed.Resolve(index), "visor supplied edit is independent");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(-1), "visor rejects negative index");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(6), "visor rejects next index");
        AssertTrue(!stock.TryResolveByteOffset(-2, out _), "visor rejects negative offset");
        Console.WriteLine("Visor: six direct native defaults, exact two-color/step basis, zero stock overrides and six independent edits pass.");
    }

    /// <summary>Reads native Samus visor color words.</summary>
    private static byte LookupStream1VisorColorsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks world-map foreground pixels and independently edited tile uploads.</summary>
    internal static void VerifyLookupStream1WorldForeground(ISnesAddressSpace rom)
    {
        byte[] Read(int address, int count) => Enumerable.Range(0, count).Select(i => LookupStream1WorldForegroundRomByte(rom, address + i)).ToArray();
        byte[] front = Read(MapSourceDefinitions.Menu.ForegroundCharacters, WorldMapArtworkFormat.ForegroundBytes);
        byte[] back = Read(MapSourceDefinitions.Menu.BackgroundCharacters, WorldMapArtworkFormat.BackgroundBytes);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(front, 4, 16, out Extent16<ImagePixels> size);
        byte[] backPixels = SnesGraphics.DecodePlanarTiles(back, 2, 16, out Extent16<ImagePixels> backSize);
        byte[] Png(byte[] selected, int selectedHeight, int colors)
        {
            using var png = new MemoryStream(); IndexedPng.Write(png, new(size.X, checked((ushort)selectedHeight)), selected, SnesGraphics.DiagnosticPalette(colors));
            return png.ToArray();
        }
        byte[] backPng = Png(backPixels, backSize.Y, 4);
        WorldMapCharacters Create(byte[] selected) => WorldMapCharacters.Read(new MemoryStream(Png(selected, size.Y, 16)), new MemoryStream(backPng));
        var stock = Create(pixels);
        var stored = stock.foreground;
        ulong NativeMask(int tile)
        {
            ulong mask = 0;
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
                if (pixels[(tile / 16 * 8 + y) * size.X + tile % 16 * 8 + x] == 14) mask |= 1UL << (y * 8 + x);
            return mask;
        }
        var masks = stock.fontFill;
        AssertEqual(300, masks.Count, "Exact source font masks after blank and named glyph aliases");
        foreach (var pair in masks) AssertEqual(NativeMask(pair.Key), pair.Value, "Exact selected native font footprint");
        int calculated = 0, fontDifferences = 0;
        var edits = new HashSet<int>();
        for (int index = 0; index < pixels.Length; index++)
        {
            int source = WorldMapTileDefinitions.ForegroundSourcePixel(index);
            if (source >= 0)
                AssertEqual(source, WorldMapTileDefinitions.ForegroundSourcePixel(source), "Foreground source dependencies are fixed and acyclic");
            AssertEqual(pixels[index], source < 0 ? (byte)0 : pixels[source], $"Direct foreground relation at pixel{index}");
            bool font = WorldMapTileDefinitions.TryForegroundFontPixel(index, NativeMask, out int fontValue);
            bool required = source == index && (!font || pixels[index] != fontValue);
            AssertEqual(required, stored.ContainsKey(index), "Exact selected source/contour basis; zero stock relation exceptions");
            if (source == index && font && pixels[index] != fontValue) fontDifferences++;
            if (source != index) { calculated++; if (source >= 0) edits.Add(source); }
        }
        AssertEqual(14347, stored.Count, "Exact drawing, contour and primitive source inputs");
        AssertEqual(10845, calculated, "Exact geometry and shared-source relations");

        AssertEqual(360, fontDifferences, "Exact selected glyph bevel/contour decisions");
        void Check(WorldMapArtwork art, byte[] expected)
        {
            var vram = new SnesVram(); art.LoadTo(vram);
            AssertTrue(vram.Bytes.Slice(WorldMapArtworkFormatConstants.ForegroundDestination, front.Length).SequenceEqual(
                SnesPlanarTileEncoder.Encode(expected, size, 4)), "Complete independent foreground upload");
            AssertTrue(vram.Bytes.Slice(WorldMapArtworkFormatConstants.BackgroundDestination, back.Length).SequenceEqual(back), "Completed BG3 remains exact");
        }
        Check(stock, pixels);
        for (int tile = 0; tile < WorldMapArtworkFormat.ForegroundTileCount; tile++)
            edits.Add(tile / 16 * 8 * size.X + tile % 16 * 8);
        foreach (int index in edits)
        {
            byte[] edited = (byte[])pixels.Clone();
            edited[index] ^= 15;
            Check(Create(edited), edited);
        }
        byte[] inverted = pixels.Select(p => (byte)(p ^ 15)).ToArray();
        Check(Create(inverted), inverted);
        AssertThrows<ArgumentOutOfRangeException>(() => WorldMapTileDefinitions.ForegroundSourcePixel(-1), "Foreground lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WorldMapTileDefinitions.ForegroundSourcePixel(pixels.Length), "Foreground upper bound");
        Console.WriteLine($"World foreground:{pixels.Length} native pixels,{calculated} exact source relations,{stored.Count} source pixels/{masks.Count} masks/{fontDifferences} font differences,{edits.Count} independent tile/source edits/full inversion/full uploads pass.");
    }

    /// <summary>Reads world-map BG3 foreground tile bytes.</summary>
    private static byte LookupStream1WorldForegroundRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks projectile collision-radius words across the timed instruction layouts.</summary>
    internal static void VerifyLookupStream1ProjectileRadiusLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1ProjectileRadiusLayoutRomByte(rom, address), LookupStream1ProjectileRadiusLayoutRomByte(rom, address + 1));
        var expected = new List<ushort>();
        for (int address = 0x9386db; address < 0x93a1a1;)
        {
            ushort word = Word(address);
            if (address == 0x93a117)
            {
                AssertEqual((ushort)0, word, "Native intervening empty spritemap record");
                address += 2;
            }
            else if ((word & 0x8000) == 0)
            {
                expected.Add(unchecked((ushort)address));
                address += 8;
            }
            else if (word == (ushort)SamusProjectileInstruction.GoTo) address += 4;
            else
            {
                AssertEqual((ushort)SamusProjectileInstruction.Delete, word, "Only native terminal delete remains");
                address += 2;
            }
        }
        AssertEqual(805, expected.Count, "Independent native timed-record count");
        AssertTrue(SamusProjectileRecordDefinitions.TimedRecordAddresses.SequenceEqual(expected), "Exact calculated pointer domain/order");
        AssertEqual(expected.Count, SamusProjectileRecordDefinitions.TimedRecordAddresses.Count, "Calculated pointer view count");
        for (int index = 0; index < expected.Count; index++)
        {
            ushort pointer = expected[index];
            AssertEqual(pointer, SamusProjectileRecordDefinitions.TimedRecordAddresses[index], "Calculated indexed pointer");
            Extent16<RoomPixels> radius = SamusProjectileProgramsVerification.RecordAt(pointer).Radius;
            AssertEqual(Word(0x930000 | (pointer + 4)), LittleEndianBytes.Word((byte)radius.X, (byte)radius.Y), "Typed record radii are the native bytes");
        }
        var document = new ProjectileFrameBindingDocument
        {
            Version = ProjectileFrameBindingFormat.Version,
            Frames = expected.ToDictionary(pointer => ProjectileFrameBindingFormat.FrameNameAt(pointer),
                pointer => ProjectileSpriteDefinitions.Name(Word(0x930000 | (pointer + 2)))),
        };
        var guard = NewLookupFlareForbiddenBus();
        int interpreted = 0, bombInterpreted = 0;
        foreach (bool edited in new[] { false, true })
        {
            if (edited)
                foreach (ushort pointer in expected) document.Frames[ProjectileFrameBindingFormat.FrameNameAt(pointer)] = ProjectileSpriteDefinitions.Name(0xa117);
            var bindings = ProjectileFrameBindingCatalog.Load(new MemoryStream(ProjectileFrameBindingCatalog.Write(document)));
            var shots = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None) { Animation = { FrameBindings = bindings } };
            var bombs = new SamusBombProjectileSystem { FrameBindings = bindings };
            foreach (ushort pointer in expected)
            {
                ushort duration = Word(0x930000 | pointer);
                ushort sprite = edited ? (ushort)0xa117 : Word(0x930000 | (pointer + 2));
                var slot = new SamusProjectileSlot(0) { InstructionTimer = WrappingCountdown.Start(1), Cursor = SamusProjectileProgramAddresses.CursorAt(pointer) };
                AssertTrue(!shots.Animation.RunProjectileInstructionHandler(slot), "Actual native timed frame survives");
                AssertEqual(duration, slot.InstructionTimer.RawValue, "Actual timer unchanged");
                AssertEqual(sprite, slot.SpritemapPointer, "Independent installed artwork selection");
                AssertEqual(LookupStream1ProjectileRadiusLayoutRomByte(rom, 0x930000 | (pointer + 4)), slot.Radius.X, "Actual X radius independent of art edit");
                AssertEqual(LookupStream1ProjectileRadiusLayoutRomByte(rom, 0x930000 | (pointer + 5)), slot.Radius.Y, "Actual Y radius independent of art edit");
                AssertEqual(Word(0x930000 | (pointer + 6)), slot.AnimationFrame, "Actual trail phase unchanged");
                AssertEqual(unchecked((ushort)(pointer + 8)), slot.InstructionPointer, "Actual record progression unchanged");
                interpreted++;
                if (duration == 0) continue; // Existing bomb contract excludes zero-duration timed records.
                var bomb = new SamusBombProjectileSlot(0) { InstructionTimer = WrappingCountdown.Start(1), Cursor = SamusProjectileProgramAddresses.CursorAt(pointer), Type = 0x0500 };
                AssertTrue(!bombs.RunProjectileInstructionHandler(bomb), "Actual bomb timed frame survives");
                AssertEqual(duration, bomb.InstructionTimer.RawValue, "Actual bomb timer unchanged");
                AssertEqual(sprite, bomb.SpritemapPointer, "Independent bomb artwork selection");
                AssertEqual(LookupStream1ProjectileRadiusLayoutRomByte(rom, 0x930000 | (pointer + 4)), bomb.Radius.X, "Actual bomb X radius");
                AssertEqual(LookupStream1ProjectileRadiusLayoutRomByte(rom, 0x930000 | (pointer + 5)), bomb.Radius.Y, "Actual bomb Y radius");
                AssertEqual(unchecked((ushort)(pointer + 8)), bomb.InstructionPointer, "Actual bomb record progression");
                bombInterpreted++;
            }
        }        for (int step = 0; step < 4; step++)
            AssertEqual(-4 - unchecked((sbyte)LookupStream1ProjectileRadiusLayoutRomByte(rom, 0x93ae70 + 7 * step)),
                ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(step), "Shared lobe source derives from centered native OAM");
        AssertEqual((Word(0x93d64c) & 0x1ff) + 4, ProjectileWaveEnvelopeDefinitions.SpazerInitialAxialSpread,
            "Shared initial Spazer lane source");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = SamusProjectileRecordDefinitions.TimedRecordAddresses[-1], "Lower indexed view rejection");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = SamusProjectileRecordDefinitions.TimedRecordAddresses[805], "Upper indexed view rejection");
        AssertEqual(Extent16<RoomPixels>.Zero, SamusProjectilePrograms.InitialRadius(null), "A shot with no list (the safe Murder Beam) has zero radii");
        Console.WriteLine($"Projectile radii:805 native calculated records/1610 bytes,{interpreted} actual projectile/{bombInterpreted} bomb records with zero reads and independent art edits; exact domain/order/bounds pass.");
    }

    /// <summary>Reads native projectile radius words from bank-$93 programs.</summary>
    private static byte LookupStream1ProjectileRadiusLayoutRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Traverses projectile programs, comparing timed records and mechanics ownership with bank $93.</summary>
    internal static void VerifyLookupStream1ProjectilePrograms(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1ProjectileProgramsRomByte(rom, address), LookupStream1ProjectileProgramsRomByte(rom, address + 1));
        var programs = new List<LookupStream1Program>();
        var records = new List<ushort>();
        var mechanics = new HashSet<int>();
        int programStart = 0x9386db, ticks = 1;
        for (int address = programStart; address < 0x93a1a1;)
        {
            if (address == 0x93a117) { address += 2; programStart = address; continue; }
            ushort word = Word(address);
            mechanics.Add(address);
            if ((word & 0x8000) == 0)
            {
                AssertTrue(word > 0, "Native projectile durations are positive");
                mechanics.Add(address + 6);
                records.Add(unchecked((ushort)address));
                ticks += word;
                address += 8;
            }
            else
            {
                if (word == 0x8239) { mechanics.Add(address + 2); address += 4; }
                else { AssertEqual((ushort)0x822f, word, "Native terminal deletion opcode"); address += 2; }
                programs.Add(new LookupStream1Program(unchecked((ushort)programStart), ticks));
                programStart = address; ticks = 1;
            }
        }
        AssertEqual(805, records.Count, "Original timed-record scope");
        AssertEqual(1816, mechanics.Count, "Original mechanics-word scope");
        AssertEqual(105, programs.Count, "Independent native complete program domains");
        var document = new ProjectileFrameBindingDocument
        {
            Version = ProjectileFrameBindingFormat.Version,
            Frames = records.ToDictionary(pointer => ProjectileFrameBindingFormat.FrameNameAt(pointer),
                pointer => ProjectileSpriteDefinitions.Name(Word(0x930000 | (pointer + 2)))),
        };
        var bindings = ProjectileFrameBindingCatalog.Load(new MemoryStream(ProjectileFrameBindingCatalog.Write(document)));
        var guard = NewLookupFlareForbiddenBus();
        int actualTicks = 0, loops = 0, deletions = 0;
        foreach (var program in programs)
        {
            var shots = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None) { Animation = { FrameBindings = bindings } };
            var bombs = new SamusBombProjectileSystem { FrameBindings = bindings };
            var slot = new SamusProjectileSlot(0) { InstructionTimer = WrappingCountdown.Start(1), Cursor = SamusProjectileProgramAddresses.CursorAt(program.Start), Damage = 30 };
            var bomb = new SamusBombProjectileSlot(0) { InstructionTimer = WrappingCountdown.Start(1), Cursor = SamusProjectileProgramAddresses.CursorAt(program.Start), Type = 0x0500 };
            ushort timer = 1, pointer = program.Start, sprite = 0, x = 0, y = 0, trail = 0;
            var visited = new HashSet<ushort>();
            bool finished = false;
            for (int tick = 0; tick < program.Ticks; tick++)
            {
                bool deleted = false, looped = false;
                timer--;
                if (timer == 0)
                {
                    ushort word = Word(0x930000 | pointer);
                    if (word == 0x8239)
                    {
                        pointer = Word(0x930000 | (pointer + 2));
                        word = Word(0x930000 | pointer);
                    }
                    if (word == 0x822f) deleted = true;
                    else
                    {
                        AssertTrue(word is > 0 and < 0x8000, "Native command reaches a timed frame");
                        looped = !visited.Add(pointer);
                        timer = word; sprite = Word(0x930000 | (pointer + 2));
                        x = LookupStream1ProjectileProgramsRomByte(rom, 0x930000 | (pointer + 4)); y = LookupStream1ProjectileProgramsRomByte(rom, 0x930000 | (pointer + 5));
                        trail = Word(0x930000 | (pointer + 6)); pointer += 8;
                    }
                }
                AssertEqual(deleted, shots.Animation.RunProjectileInstructionHandler(slot), "Exact native projectile deletion tick");
                AssertEqual(deleted, bombs.RunProjectileInstructionHandler(bomb), "Exact native bomb deletion tick");
                actualTicks++;
                if (deleted)
                {
                    AssertEqual(0, slot.Damage, "Projectile deletion clears occupancy");
                    AssertEqual(0, bomb.Type, "Bomb deletion clears occupancy");
                    deletions++; finished = true; break;
                }
                AssertEqual(new VerifyLookupStream1ProjectileProgramsCase(timer, pointer, sprite, x, y, trail),
                    new VerifyLookupStream1ProjectileProgramsCase(slot.InstructionTimer.RawValue, slot.InstructionPointer, slot.SpritemapPointer, slot.Radius.X, slot.Radius.Y, slot.AnimationFrame),
                    "Every scheduled projectile exposure/physical envelope/trail phase matches native");
                AssertEqual(new LookupStream1VerifyLookupStream1ProjectileProgramsCase(timer, pointer, sprite, x, y),
                    new LookupStream1VerifyLookupStream1ProjectileProgramsCase(bomb.InstructionTimer.RawValue, bomb.InstructionPointer, bomb.SpritemapPointer, bomb.Radius.X, bomb.Radius.Y),
                    "Every scheduled bomb exposure/physical envelope matches native");
                slot.AnimationFrame = 0xbeef;
                AssertEqual(trail, ProjectileTrails.GetTrailAnimationFrame(slot, InstalledFixtures.SamusOverreads), "Actual trail consumer reads prior record, not next phase or cached frame");
                slot.AnimationFrame = trail;
                if (looped) { loops++; finished = true; break; }
            }
            AssertTrue(finished, "Complete native startup and first loop or terminal deletion confirmed");
        }
        Console.WriteLine($"Projectile programs:1816 native words,805 records,105 complete programs,{actualTicks} exact ticks per owner,{loops} loop returns/{deletions} terminal deletions, zero runtime reads; original domain preserved.");
    }

    /// <summary>Reads native projectile program words for control and hold comparison.</summary>
    private static byte LookupStream1ProjectileProgramsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks cannon placement relative to Samus body poses and edited artwork geometry.</summary>
    internal static void VerifyLookupStream1CannonPlacement(ISnesAddressSpace rom)
    {
        using var directory = new TestScratchDirectory("map-catalog");
        SamusBodyArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        SamusBodyArtworkCatalog body = SamusBodyArtworkFiles.Load(directory.Root, null);
        byte[] json = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.JsonFileName));
        byte[] png = File.ReadAllBytes(Path.Combine(directory.Root, SamusArmCannonArtworkFormat.TileFileName));
        var document = RequiredJson.Deserialize<SamusArmCannonArtworkDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var tiles = RoomCharacterAtlasPng.Read(new MemoryStream(png), SamusArmCannonArtworkFormat.TileSourcePointers.Count * 32);
        SamusArmCannonArtworkCatalog Load(SamusArmCannonArtworkDocument value) => SamusArmCannonArtworkCatalog.FromPlacement(
            SamusArmCannonArtworkCatalog.LoadPlacement(new MemoryStream(SamusArmCannonArtworkCatalog.Write(value))), tiles);
        Dictionary<int, byte> Stored(SamusArmCannonArtworkCatalog value) => value.drawingData;
        var stock = body.ArmCannon;
        AssertEqual(0, Stored(stock).Count, "Installed cannon has zero stock coordinate fallbacks: " +
            string.Join(", ", Stored(stock).Select(pair => $"{pair.Key + 0xc9d9:X4}={pair.Value:X2}")));
        AssertEqual(Load(document).ContentIdentity, stock.ContentIdentity, "Body binding preserves canonical cannon identity");
        int basis = 0;
        for (int index = 0; index < document.DrawingData.Length; index++)
        {
            ushort address = (ushort)(0xc9d9 + index);
            byte expected = LookupStream1CannonPlacementRomByte(rom, 0x900000 | address);
            if (!SamusArmCannonArtworkFormat.TryStockDrawingByte(index, out _) &&
                !SamusArmCannonArtworkFormat.TryStockCoordinateSource(index, out _) &&
                !SamusArmCannonArtworkFormat.TryStockReflectedXSource(index, out _))
            {
                AssertTrue(SamusArmCannonPlacementDefinitions.TryCoordinate(body, index, out int direct), "Every independent coordinate has a direct body/default source");
                AssertEqual(expected, direct, $"Direct native cannon attachment {address:X4}");
                basis++;
            }
            AssertEqual(expected, stock.ReadDrawingByteAt(address), "Every installed native cannon byte");
            int[] drawing = document.DrawingData.ToArray(); drawing[index] ^= 0xff;
            var edited = Load(document with { DrawingData = drawing }).WithBodyGeometry(body);
            for (int other = 0; other < drawing.Length; other++)
                AssertEqual((byte)drawing[other], edited.ReadDrawingByteAt((ushort)(0xc9d9 + other)), "Independent cover-byte edit preserves all supplied outputs");
            AssertEqual(Load(document with { DrawingData = drawing }).ContentIdentity, edited.ContentIdentity, "Bound edited cover retains exact canonical hash");
        }
        AssertEqual(135, basis, "All formerly independent coordinate basis bytes now have reviewed defaults");
        SamusBodyArtworkCatalog WithMaps(SamusSpritemapArtworkCatalog maps) => new(new SamusBodyArtworkParts { TopSetStarts = body.TopSetStarts.ToArray(), BottomSetStarts = body.BottomSetStarts.ToArray(), PoseFirstFrames = body.PoseFirstFrames.ToArray(), GraphicsYOffsets = body.GraphicsYOffsets.ToArray(), Frames = body.Frames.ToArray(), Top = Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => body.TopSet(index).ToArray()).ToArray(), Bottom = Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => body.BottomSet(index).ToArray()).ToArray(), Spritemaps = maps, Atmosphere = body.Atmosphere, DeathPalettes = body.DeathPalettes, DeathTilesContentIdentity = body.DeathTilesContentIdentity, ArmCannon = stock, LandingYOffsets = body.LandingYOffsets.ToArray(), PostureYOffsets = body.PostureYOffsets.ToArray(), DrainedYOffsets = body.DrainedYOffsets.ToArray() });
        var empty = new SamusSpritemapArtworkCatalog(body.Spritemaps.TopBases.ToArray(), body.Spritemaps.BottomBases.ToArray(),
            body.Spritemaps.Entries.ToArray(), body.Spritemaps.Definitions.Select(map => new SamusSpritemapDefinition([])).ToArray());
        var zero = new SamusSpritemapArtworkCatalog(body.Spritemaps.TopBases.ToArray(), body.Spritemaps.BottomBases.ToArray(),
            new SamusSpritemapId?[SamusSpritemapArtworkCatalog.EntryCount], []);
        var shifted = new SamusSpritemapArtworkCatalog(body.Spritemaps.TopBases.ToArray(), body.Spritemaps.BottomBases.ToArray(),
            body.Spritemaps.Entries.ToArray(), body.Spritemaps.Definitions.Select(map => new SamusSpritemapDefinition(
                map.Parts.Select(part => part with { Offset = Offset16<ScreenPixels>.Of(SpritePart.WrapX(part.Offset.X + 1), unchecked((sbyte)(part.Offset.Y + 1))) }).ToArray())).ToArray());
        foreach (var maps in new[] { empty, zero, shifted })
        {
            var editedBody = WithMaps(maps);
            for (int index = 0; index < document.DrawingData.Length; index++)
                AssertEqual((byte)document.DrawingData[index], editedBody.ArmCannon.ReadDrawingByteAt((ushort)(0xc9d9 + index)), "Independent body geometry cannot rewrite supplied cover placement");
            AssertEqual(stock.ContentIdentity, editedBody.ArmCannon.ContentIdentity, "Body-only edits preserve complete cover identity");
            AssertTrue(Stored(editedBody.ArmCannon).Count > 0, "Different source art retains explicit independent cover offsets");
        }
        int draws = 0;
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.StandingAimUpRightPose,
            SamusPoseId.StandingAimDiagonalUpRightPose, SamusPoseId.StandingAimDiagonalDownRightPose,
            SamusPoseId.MovingRightGunExtendedPose, SamusPoseId.MovingLeftGunExtendedPose,
            SamusPoseId.RunningAimDiagonalUpRightPose, SamusPoseId.RunningAimDiagonalUpLeftPose,
            SamusPoseId.RunningAimDiagonalDownRightPose, SamusPoseId.RunningAimDiagonalDownLeftPose,
            SamusPoseId.NormalJumpAimUpRightPose, SamusPoseId.NormalJumpAimUpLeftPose,
            SamusPoseId.NormalJumpAimDownRightPose, SamusPoseId.NormalJumpAimDownLeftPose,
            SamusPoseId.FallingAimUpRightPose, SamusPoseId.FallingAimUpLeftPose,
            SamusPoseId.FallingAimDownRightPose, SamusPoseId.FallingAimDownLeftPose,
            SamusPoseId.MoonwalkFacingLeftPose, SamusPoseId.MoonwalkFacingRightPose,
            SamusPoseId.NeutralJumpTransitionRightPose, SamusPoseId.CrouchingAimDiagonalUpRightPose,
            SamusPoseId.CrouchingAimDiagonalUpLeftPose, SamusPoseId.MoonwalkAimUpLeftPose,
            SamusPoseId.MoonwalkAimUpRightPose, SamusPoseId.NormalLandingRightPose,
            SamusPoseId.SpinLandingRightPose
        })
        {
            int count = pose == SamusPoseId.NormalLandingRightPose ? 2 : pose == SamusPoseId.SpinLandingRightPose ? 3 : pose is SamusPoseId.StandingAimUpRightPose or SamusPoseId.NormalJumpAimUpRightPose or SamusPoseId.NormalJumpAimUpLeftPose or SamusPoseId.NormalJumpAimDownRightPose or SamusPoseId.NormalJumpAimDownLeftPose or SamusPoseId.FallingAimUpRightPose or SamusPoseId.FallingAimUpLeftPose or SamusPoseId.FallingAimDownRightPose or SamusPoseId.FallingAimDownLeftPose ? 2 : 1;
            for (ushort frame = 0; frame < count; frame++)
            {
                var samus = new SamusState { Pose = pose, AnimationFrame = frame, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), SelectedHudItem = HudItem.Missile };
                samus.Presentation.BodyFrames.BindBodyFixture(body);
                var cannon = new SamusArmCannonState { Artwork = stock };
                cannon.Update(samus); cannon.Update(samus);
                int pointer = LittleEndianBytes.Word(LookupStream1CannonPlacementRomByte(rom, 0x90c7df + (int)pose * 2), LookupStream1CannonPlacementRomByte(rom, 0x90c7e0 + (int)pose * 2));
                int offset = (LookupStream1CannonPlacementRomByte(rom, 0x900000 | pointer) & 128) != 0 ? 4 : 2;
                int x = unchecked((sbyte)LookupStream1CannonPlacementRomByte(rom, 0x900000 | (pointer + offset + frame * 2)));
                int y = unchecked((sbyte)LookupStream1CannonPlacementRomByte(rom, 0x900000 | (pointer + offset + frame * 2 + 1)));
                int graphics = unchecked((sbyte)LookupStream1CannonPlacementRomByte(rom, 0x91b629 + (int)pose * 8 + 4));
                OamBuffer cannonOam = SpriteFrameBuffer.DrawnOam(frame => cannon.Draw(frame, samus, Position16<RoomPixels>.Zero, 0));
                AssertEqual(1, cannonOam.LastFinalizedSpriteCount, "Actual cover draw publication");
                OamEntry cannonEntry = cannonOam.GetEntry(0);
                AssertEqual(128 + x, cannonEntry.Position.X, "Actual native cover X");
                AssertEqual(128 + y - graphics, (int)cannonEntry.Position.Y, "Actual native cover Y including pose origin");
                draws++;
            }
        }
        Console.WriteLine($"Cannon placement:135 direct body/tail defaults,608 native bytes/independent edits,zero stock fallbacks,canonical hashes,independent empty/zero/shifted body art and{draws} actual OAM draws pass.");
    }

    /// <summary>Reads Samus arm-cannon placement source words.</summary>
    private static byte LookupStream1CannonPlacementRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks Samus spritemap pointer roots and independently editable OAM definitions.</summary>
    internal static void VerifyLookupStream1OamPointers(ISnesAddressSpace rom)
    {
        using var directory = new TestScratchDirectory("map-catalog");
        SamusBodyArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        var body = SamusBodyArtworkFiles.Load(directory.Root, null);
        var stock = body.Spritemaps;
        ushort[] nativePointers = Enumerable.Range(0, 2096).Select(index =>
            LittleEndianBytes.Word(LookupStream1OamPointersRomByte(rom, 0x92808d + index * 2), LookupStream1OamPointersRomByte(rom, 0x92808e + index * 2))).ToArray();
        SamusSpritemapId?[] native = SamusSpritemapNative.Entries(nativePointers);
        int aliases = 0, directPointers = 0;
        int compositionCount = stock.Definitions.Count;
        AssertEqual(SamusSpritemapNative.Compositions(nativePointers).Length, compositionCount, "One installed composition per referenced native record");
        for (int index = 0; index < native.Length; index++)
        {
            int source = SamusSpritemapFrameDefinitions.SourceIndex(index);
            AssertEqual(nativePointers[index], nativePointers[source], $"Source-identified native OAM sharing {index:X4}->{source:X4}");
            if (source != index) aliases++;
            else if (SamusSpritemapFrameDefinitions.TryComposition(index, compositionCount, out SamusSpritemapId? calculated))
            {
                AssertEqual(native[index], calculated, $"Direct native composition identity {index:X4}");
                directPointers++;
            }
        }
        AssertTrue(stock.Entries.SequenceEqual(native), "All native pointer observations preserved");
        var stored = stock.entries;
        AssertEqual(native.Length - aliases - directPointers, stored.Count, "Exact independent pointer basis, no stock alias/allocation fallbacks");
        ushort[] top = stock.TopBases.ToArray(), bottom = stock.BottomBases.ToArray();
        SamusSpritemapDefinition[] definitions = stock.Definitions.ToArray();
        string Canonical(SamusSpritemapId?[] entries) => SelectedPresentationHash.Create("samus-spritemaps-v3", content =>
        {
            content.AppendWords("top bases", top); content.AppendWords("bottom bases", bottom);
            foreach (SamusSpritemapId? entry in entries) content.Append("entry", entry?.Ordinal ?? -1);
            foreach (var map in definitions)
            {
                content.Append("part count", map.Parts.Length);
                foreach (SpritePart part in map.Parts) AppendCanonicalSpritePart(content, part);
            }
        });
        AssertEqual(Canonical(native), stock.ContentIdentity, "Original native OAM canonical hash");
        SamusSpritemapArtworkCatalog Create(SamusSpritemapId?[] entries) => new(top, bottom, entries, definitions);
        var emptyCompositions = new SamusSpritemapArtworkCatalog(top, bottom, native,
            definitions.Select(map => new SamusSpritemapDefinition([])).ToArray());
        AssertTrue(emptyCompositions.Entries.SequenceEqual(native), "Independent OAM part-count edits preserve every supplied composition identity");
        for (int index = 0; index < native.Length; index++)
        {
            var supplied = native.ToArray(); supplied[index] = supplied[index] is null ? native[0] : null;
            var edited = Create(supplied);
            AssertTrue(edited.Entries.SequenceEqual(supplied), "Independent basis/alias edit preserves every supplied composition");
            bool exists = edited.TryGet((ushort)index, out var map);
            AssertEqual(supplied[index] is not null, exists, "Actual edited selector retains drawing versus non-drawing distinction");
            if (exists)
                AssertTrue(ReferenceEquals(map, edited.definitions[(supplied[index] ?? throw new InvalidOperationException("Verification requires a composition.")).Ordinal]),
                    "Actual edited composition identity");
            if (index == native.Length - 1) AssertEqual(Canonical(supplied), edited.ContentIdentity, "Original independently edited OAM canonical hash");
        }
        Directory.CreateDirectory("csharp/test-temp");
        File.WriteAllLines("csharp/test-temp/samus-oam-pointer-required.csv", OamPointerCsvHeader
            .Concat(stored.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key:X4},{0x92808d + pair.Key * 2:X6},{pair.Value?.Ordinal ?? -1}")));
        AssertThrows<InvalidDataException>(() => stock.TryGet(2096, out _), "Original upper index rejection");
        AssertThrows<InvalidDataException>(() => stock.TryGet(ushort.MaxValue, out _), "Original full ushort rejection");
        VerifySamusZeroOamSelectorDrawsNothing(stock);
        Console.WriteLine($"Samus OAM pointers:2096 native selectors, {aliases} exact aliases/{directPointers} direct calculations/{stored.Count} retained composition inputs,2096 independent edits,empty OAM,actual identity/WRAM selection and domain checks pass.");
    }

    /// <summary>Reads the native Samus spritemap OAM pointer table.</summary>
    private static byte LookupStream1OamPointersRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>
    /// Checks that a zero OAM pointer-table entry draws nothing: it would dereference $92:0000 direct-page scratch,
    /// which the port exempts (owner decision on #1318).
    /// </summary>
    private static void VerifySamusZeroOamSelectorDrawsNothing(SamusSpritemapArtworkCatalog stock)
    {
        OamBuffer oam = SamusSpritemapDrawsAccess.SamusSpritemapOam(3, new Position16<ScreenPixels>(127, 131),
            SpriteFrameTestArtwork.Spritemaps(stock), new SpriteSource(SpriteSourceKind.Samus));
        OamBuffer empty = SpriteFrameBuffer.DrawnOam(_ => { });
        AssertTrue(oam.LowTable.SequenceEqual(empty.LowTable) && oam.HighTable.SequenceEqual(empty.HighTable),
            "a zero selector draws no OAM entry through the actual renderer");
    }

    /// <summary>Checks body tile pixel inputs, derived bytes, and independent edited planar data.</summary>
    internal static void VerifyLookupStream1BodyPixels(ISnesAddressSpace rom)
    {
        using var directory = new TestScratchDirectory("map-catalog");
        SamusBodyArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        var body = SamusBodyArtworkFiles.Load(directory.Root, null);
        var top = Enumerable.Range(0, 13).Select(set => body.TopSet(set).ToArray()).ToArray();
        var bottom = Enumerable.Range(0, 11).Select(set => body.BottomSet(set).ToArray()).ToArray();
        int bytes = 0, blankBytes = 0, sharedBytes = 0, retained = 0, circleBits = 0, diagnosticBytes = 0;
        var basisRows = new List<string> { "Half,Set,Position,NativeAddress,Byte,Mask" };
        for (int half = 0; half < 2; half++)
        {
            bool upper = half == 0;
            var groups = upper ? top : bottom;
            for (int set = 0; set < groups.Length; set++)
            for (int position = 0; position < groups[set].Length; position++)
            {
                var glyph = groups[set][position];
                int glyphSource = LookupStream1BodySource(rom, upper, set, position);
                var stored = glyph.pixelInputs;
                byte[] image = glyph.Planar.ToArray();
                var contour = glyph.contourEdits;
                AssertEqual(0, (contour ?? throw new InvalidOperationException("Verification requires contour.")).Count, "Zero stock morph-circle contour overrides");
                for (int index = 0; index < image.Length; index++)
                {
                    byte native = LookupStream1BodyPixelsRomByte(rom, glyphSource + index);
                    AssertEqual(native, image[index], "Every native body pixel byte");
                    byte mask = SamusBodyPixelDefinitions.ContourMask(upper, set, position, index);
                    AssertEqual((byte)0, (byte)(native & ~mask), "Exact native circular silhouette");
                    circleBits += System.Numerics.BitOperations.PopCount((byte)~mask);
                    int tile = index / 32;
                    bool blank = SamusBodyPixelDefinitions.IsBlank(upper, set, position, tile);
                    bool relation = SamusBodyPixelDefinitions.TrySourceByte(upper, set, position, index, out int sourcePosition, out int sourceIndex);
                    bool shared = !blank && relation;
                    bool diagnostic = SamusBodyPixelDefinitions.TryDiagnosticByte(upper, set, position, index, out int glyphByte);
                    if (diagnostic) { AssertEqual(native, glyphByte, "Direct native Ep drawing"); diagnosticBytes++; }
                    else if (blank) { AssertEqual((byte)0, native, "Direct native transparent allocation"); blankBytes++; }
                    else if (shared) { AssertEqual(LookupStream1BodyPixelsRomByte(rom, LookupStream1BodySource(rom, upper, set, sourcePosition) + sourceIndex), native, "Direct native shared anatomical patch"); sharedBytes++; }
                    else
                    {
                        retained++;
                        basisRows.Add($"{half},{set:X},{position:X},{glyphSource + index:X6},{native:X2},{mask:X2}");
                    }
                    AssertEqual(!diagnostic && !blank && !shared, (stored ?? throw new InvalidOperationException("Verification requires stored.")).ContainsKey(index), "Exact pixel basis; no stock defaults survive as overrides");
                    bytes++;
                }
            }
        }
        AssertEqual(130464, bytes, "Complete native body planar domain");
        AssertEqual(81 * 32, blankBytes, "Exact transparent tile domain");
        AssertEqual(357 * 32, sharedBytes, "Exact grapple/drained anatomy domain");
        AssertEqual(4 * 32, diagnosticBytes, "Exact bounded diagnostic domain");
        AssertEqual(1536, circleBits, "Eight morph silhouettes exclude48 pixels each in four planes");
        Directory.CreateDirectory("csharp/test-temp");
        File.WriteAllLines("csharp/test-temp/samus-body-pixel-basis.csv", basisRows);
        SamusBodyArtworkCatalog Create(SamusBodyTileDefinition[][] upper, SamusBodyTileDefinition[][]? lower = null) => new(new SamusBodyArtworkParts { TopSetStarts = body.TopSetStarts.ToArray(), BottomSetStarts = body.BottomSetStarts.ToArray(), PoseFirstFrames = body.PoseFirstFrames.ToArray(), GraphicsYOffsets = body.GraphicsYOffsets.ToArray(), Frames = body.Frames.ToArray(), Top = upper, Bottom = lower ?? bottom, Spritemaps = body.Spritemaps, Atmosphere = body.Atmosphere, DeathPalettes = body.DeathPalettes, DeathTilesContentIdentity = body.DeathTilesContentIdentity, ArmCannon = body.ArmCannon, LandingYOffsets = body.LandingYOffsets.ToArray(), PostureYOffsets = body.PostureYOffsets.ToArray(), DrainedYOffsets = body.DrainedYOffsets.ToArray() });
        AssertEqual(CanonicalBodyHash(body, body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PosePointers.ToArray(), body.Frames.ToArray(), top, bottom), body.ContentIdentity, "Original native body hash");
        foreach (var change in new VerifyLookupStream1BodyPixelsCase[] { new VerifyLookupStream1BodyPixelsCase(0, 2, 128), new VerifyLookupStream1BodyPixelsCase(4, 2, 0), new VerifyLookupStream1BodyPixelsCase(4, 3, 0), new VerifyLookupStream1BodyPixelsCase(6, 13, 32), new VerifyLookupStream1BodyPixelsCase(6, 14, 32), new VerifyLookupStream1BodyPixelsCase(3, 4, 64), new VerifyLookupStream1BodyPixelsCase(10, 0, 0), new VerifyLookupStream1BodyPixelsCase(10, 4, 128), new VerifyLookupStream1BodyPixelsCase(1, 4, 0) })
        {
            var supplied = top.Select(group => group.ToArray()).ToArray();
            var original = supplied[change.Set][change.Position];
            byte[] edited = original.Planar.ToArray(); edited[change.Byte] ^= 255;
            supplied[change.Set][change.Position] = new(original.FirstSize, original.SecondSize, edited);
            var actual = Create(supplied);
            for (int set = 0; set < supplied.Length; set++)
            for (int position = 0; position < supplied[set].Length; position++)
                AssertTrue(actual.TopSet(set)[position].Planar.Span.SequenceEqual(supplied[set][position].Planar.Span), "Source, derived and padding edits preserve every independently supplied glyph");
            AssertEqual(CanonicalBodyHash(body, body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PosePointers.ToArray(), body.Frames.ToArray(), supplied, bottom), actual.ContentIdentity, "Exact independent body pixel hash");
        }
        var cableEdits = bottom.Select(group => group.ToArray()).ToArray();
        foreach (int position in new[] { 0, 1, 4, 5, 8, 9, 11, 12, 16 })
        {
            var glyph = cableEdits[3][position];
            byte[] pixels = glyph.Planar.ToArray(); pixels[0] ^= (byte)(1 << (position % 8));
            cableEdits[3][position] = new(glyph.FirstSize, glyph.SecondSize, pixels);
        }
        var cableBody = Create(top, cableEdits);
        for (int set = 0; set < bottom.Length; set++)
        for (int position = 0; position < bottom[set].Length; position++)
            AssertTrue(cableEdits[set][position].Planar.Span.SequenceEqual(cableBody.BottomSet(set)[position].Planar.Span), "Independent repeated/swapped cable source and target edits remain exact");
        AssertEqual(CanonicalBodyHash(body, body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PosePointers.ToArray(), body.Frames.ToArray(), top, cableEdits), cableBody.ContentIdentity, "Exact cable edit hash");
        Console.WriteLine($"Body pixels: {bytes} native bytes, {blankBytes} blank and {sharedBytes} shared bytes, exact {retained} selected inputs, nine individual upper edits plus nine simultaneous distinct cable edits, {diagnosticBytes} drawn diagnostic bytes, {circleBits} circular transparency bits and canonical hashes pass.");
    }

    /// <summary>Where the cartridge stores body record <paramref name="position"/> of set <paramref name="set"/>'s planar bytes, read from its native descriptor.</summary>
    private static int LookupStream1BodySource(ISnesAddressSpace rom, bool upper, int set, int position)
    {
        ushort Word(int at) => (ushort)(rom.ReadCartridgeByte(at) | rom.ReadCartridgeByte(at + 1) << 8);
        int address = 0x920000 | (Word((upper ? 0x92d91e : 0x92d938) + set * 2) + position * 7);
        return (int)new SnesAddress(rom.ReadCartridgeByte(address + 2), Word(address));
    }

    /// <summary>Reads planar Samus body tile bytes before derived pixel expansion.</summary>
    private static byte LookupStream1BodyPixelsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks body tile DMA definitions and actual native-sized VRAM transfer output.</summary>
    internal static void VerifyLookupStream1BodyTransfers(ISnesAddressSpace rom)
    {
        using var directory = new TestScratchDirectory("map-catalog");
        SamusBodyArtworkFiles.Extract(rom, directory.Root, SupportedCartridge.Sha256);
        var body = SamusBodyArtworkFiles.Load(directory.Root, null);
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1BodyTransfersRomByte(rom, address), LookupStream1BodyTransfersRomByte(rom, address + 1));
        var top = Enumerable.Range(0, 13).Select(set => body.TopSet(set).ToArray()).ToArray();
        var bottom = Enumerable.Range(0, 11).Select(set => body.BottomSet(set).ToArray()).ToArray();
        var changedTop = top.Select(group => group.ToArray()).ToArray();
        var changedBottom = bottom.Select(group => group.ToArray()).ToArray();
        int records = 0;
        for (int half = 0; half < 2; half++)
        {
            bool upper = half == 0;
            var groups = upper ? top : bottom;
            for (int set = 0; set < groups.Length; set++)
            for (int position = 0; position < groups[set].Length; position++)
            {
                var value = groups[set][position];
                int address = 0x920000 | (Word((upper ? 0x92d91e : 0x92d938) + set * 2) + position * 7);
                int source = (int)new SnesAddress(LookupStream1BodyTransfersRomByte(rom, address + 2), Word(address));
                ushort first = Word(address + 3), second = Word(address + 5);
                AssertTrue(SamusBodyTransferDefinitions.TryFirstSize(body, upper, set, position, value.Planar.Length, out int calculated), "Every native record has a direct row calculation");
                AssertEqual(first, calculated, $"Direct native row geometry {upper}/{set:X}/{position:X}");
                AssertEqual(first, value.FirstSize, "Installed native first row");
                AssertEqual(second, value.SecondSize, "Installed native remaining row");
                AssertTrue(value.firstSizeOverride is null, "Zero stock metadata overrides");
                byte[] pixels = value.Planar.ToArray();
                for (int index = 0; index < pixels.Length; index++) AssertEqual(LookupStream1BodyTransfersRomByte(rom, source + index), pixels[index], "Separate native pixel payload preserved");
                // Preserve schema-valid non-tile-aligned individual row splits too.
                ushort editedFirst = (ushort)(second == 0 ? first - 1 : first + 1);
                var supplied = new SamusBodyTileDefinition(editedFirst,
                    (ushort)(pixels.Length - editedFirst), pixels);
                (upper ? changedTop : changedBottom)[set][position] = supplied;
                var edited = supplied.WithTransferGeometry(body, upper, set, position);
                AssertEqual(supplied.FirstSize, edited.FirstSize, "Independent row split edit");
                AssertEqual(supplied.SecondSize, edited.SecondSize, "Edited complementary row");
                AssertTrue(edited.Planar.Span.SequenceEqual(pixels), "Metadata edit preserves every pixel");
                records++;
            }
        }
        AssertEqual(435, records, "Complete original metadata domain");
        SamusBodyArtworkCatalog Create(SamusBodyTileDefinition[][] upper, SamusBodyTileDefinition[][] lower, SamusSpritemapArtworkCatalog maps) => new(new SamusBodyArtworkParts { TopSetStarts = body.TopSetStarts.ToArray(), BottomSetStarts = body.BottomSetStarts.ToArray(), PoseFirstFrames = body.PoseFirstFrames.ToArray(), GraphicsYOffsets = body.GraphicsYOffsets.ToArray(), Frames = body.Frames.ToArray(), Top = upper, Bottom = lower, Spritemaps = maps, Atmosphere = body.Atmosphere, DeathPalettes = body.DeathPalettes, DeathTilesContentIdentity = body.DeathTilesContentIdentity, ArmCannon = body.ArmCannon, LandingYOffsets = body.LandingYOffsets.ToArray(), PostureYOffsets = body.PostureYOffsets.ToArray(), DrainedYOffsets = body.DrainedYOffsets.ToArray() });
        var changed = Create(changedTop, changedBottom, body.Spritemaps);
        AssertEqual(CanonicalBodyHash(body, body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PosePointers.ToArray(), body.Frames.ToArray(), top, bottom),
            body.ContentIdentity, "Exact canonical stock metadata hash");
        AssertEqual(CanonicalBodyHash(body, body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PosePointers.ToArray(), body.Frames.ToArray(), changedTop, changedBottom),
            changed.ContentIdentity, "Exact canonical hash preserves every supplied metadata edit");
        var emptyMaps = new SamusSpritemapArtworkCatalog(body.Spritemaps.TopBases.ToArray(), body.Spritemaps.BottomBases.ToArray(),
            body.Spritemaps.Entries.ToArray(), body.Spritemaps.Definitions.Select(map => new SamusSpritemapDefinition([])).ToArray());
        var empty = Create(top, bottom, emptyMaps);
        for (int half = 0; half < 2; half++)
        for (int set = 0; set < (half == 0 ? 13 : 11); set++)
        {
            var expected = half == 0 ? top[set] : bottom[set];
            var actual = half == 0 ? empty.TopSet(set) : empty.BottomSet(set);
            for (int index = 0; index < expected.Length; index++)
            {
                AssertTrue(expected[index].Planar.Span.SequenceEqual(actual[index].Planar.Span), "Empty independent OAM preserves every pixel");
                AssertEqual(expected[index].FirstSize, actual[index].FirstSize, "Empty independent OAM preserves first row");
                AssertEqual(expected[index].SecondSize, actual[index].SecondSize, "Empty independent OAM preserves second row");
            }
        }
        var shorterTop = top.Select(group => group.ToArray()).ToArray();
        var original = top[0][0];
        AssertTrue(original.SecondSize >= 32, "Chosen payload edit has a complete removable second-row tile");
        shorterTop[0][0] = new(original.FirstSize, (ushort)(original.SecondSize - 32), original.Planar.Span[..^32].ToArray());
        var shorter = Create(shorterTop, bottom, body.Spritemaps);
        for (int position = 0; position < top[0].Length; position++)
        {
            AssertTrue(shorterTop[0][position].Planar.Span.SequenceEqual(shorter.TopSet(0)[position].Planar.Span), "Independent payload-length edit cannot rewrite supplied neighboring pixels");
            AssertEqual(shorterTop[0][position].FirstSize, shorter.TopSet(0)[position].FirstSize, "Independent payload length preserves supplied row split");
            AssertEqual(shorterTop[0][position].SecondSize, shorter.TopSet(0)[position].SecondSize, "Exact edited payload remainder");
        }
        // These exact native selectors include the known physical cross-group windows.
        var guard = NewFrontendCartridgeReadGuard(rom);
        int transfers = 0;
        foreach (var selected in new VerifyLookupStream1BodyTransfersCase[] { new VerifyLookupStream1BodyTransfersCase(SamusPoseId.FacingRightNormalPose, 0), new VerifyLookupStream1BodyTransfersCase(SamusPoseId.UnusedPose65, 6), new VerifyLookupStream1BodyTransfersCase(SamusPoseId.UnusedPose65, 7), new VerifyLookupStream1BodyTransfersCase(SamusPoseId.UnusedPose66, 8), new VerifyLookupStream1BodyTransfersCase(SamusPoseId.DeathSequenceLeftPose, 0) })
        {
            var state = new SamusBodyFrameState(); state.BindArtwork(body);
            state.Select(selected.Pose, selected.Phase);
            int frameAddress = 0x920000 | (Word(0x92d94e + (int)selected.Pose * 2) + selected.Phase * 4);
            int topAddress = 0x920000 | (Word(0x92d91e + LookupStream1BodyTransfersRomByte(rom, frameAddress) * 2) + LookupStream1BodyTransfersRomByte(rom, frameAddress + 1) * 7);
            AssertEqual(topAddress, state.TopDefinitionAddress, "Native physical upper selection including cross-group position");
            bool namesBottom = LookupStream1BodyTransfersRomByte(rom, frameAddress + 2) != 255;
            if (namesBottom)
                AssertEqual(0x920000 | (Word(0x92d938 + LookupStream1BodyTransfersRomByte(rom, frameAddress + 2) * 2) + LookupStream1BodyTransfersRomByte(rom, frameAddress + 3) * 7),
                    state.BottomDefinitionAddress, "Native physical lower selection including cross-group position");
            else
                AssertEqual(null, state.BottomDefinitionAddress, "A first selection naming no lower half shows none");
            // The shown body characters equal the native split copies: each definition's first part fills a half row
            // (top from character 0, bottom from character 8) and its second part the half row a full row below.
            var expected = new byte[SamusBodyArtworkCatalog.BodyCharacterCount * SpriteArtwork.CharacterByteCount];
            void Copy(int address, int firstCharacter)
            {
                int source = (int)new SnesAddress(LookupStream1BodyTransfersRomByte(rom, address + 2), Word(address));
                int first = Word(address + 3), second = Word(address + 5);
                for (int index = 0; index < first; index++)
                    expected[firstCharacter * SpriteArtwork.CharacterByteCount + index] = LookupStream1BodyTransfersRomByte(rom, source + index);
                for (int index = 0; index < second; index++)
                    expected[(firstCharacter + SpriteArtwork.CharactersPerRow) * SpriteArtwork.CharacterByteCount + index] =
                        LookupStream1BodyTransfersRomByte(rom, source + first + index);
            }
            Copy(state.TopDefinitionAddress, 0);
            if (state.BottomDefinitionAddress is int bottomAddress) Copy(bottomAddress, SpriteArtwork.CharactersPerRow / 2);
            AssertTrue(body.BodyArtwork(state.Shown.Top, state.Shown.Bottom).Characters.SequenceEqual(expected),
                "Shown body pictures match the native split copies and physical cross-group aliases");
            transfers++;
        }
        Console.WriteLine($"Body transfers:{records} direct native records,zero stock metadata overrides,independent metadata/empty-OAM edits,canonical hash and{transfers} actual split DMA/cross-group selections pass.");
    }

    /// <summary>Reads native Samus body DMA source and transfer words.</summary>
    private static byte LookupStream1BodyTransfersRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs the focused Samus death-transfer definitions oracle with the retail cartridge.</summary>
    [VerificationCommand("--lookup-stream-1-death-definitions")]
    internal static int LookupStream1DeathDefinitionsCommand()
    {
        Suite(nameof(VerifyLookupStream1DeathDefinitions), () => VerifyLookupStream1DeathDefinitions(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the focused escape Dachora full-cadence oracle with the retail cartridge.</summary>
    [VerificationCommand("--lookup-stream-1-escape-dachora-cadence")]
    internal static int LookupStream1EscapeDachoraCadenceCommand()
    {
        Suite(nameof(VerifyLookupStream1EscapeDachoraCadence), () => VerifyLookupStream1EscapeDachoraCadence(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the focused Powamp body and balloon cadence oracle.</summary>
    [VerificationCommand("--lookup-stream-1-powamp-cadence")]
    internal static int LookupStream1PowampCadenceCommand()
    {
        Suite(nameof(VerifyLookupStream1PowampCadence), () => VerifyLookupStream1PowampCadence(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs arm-cannon pose pointer extraction and edit checks.</summary>
    [VerificationCommand("--lookup-stream-1-cannon-poses")]
    internal static int LookupStream1CannonPosesCommand()
    {
        Suite(nameof(VerifyLookupStream1CannonPoses), () => VerifyLookupStream1CannonPoses(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the HUD posture selector check over the full byte pose domain.</summary>
    [VerificationCommand("--lookup-stream-1-hud-posture")]
    internal static int LookupStream1HudPostureCommand()
    {
        Suite(nameof(VerifyLookupStream1HudPosture), () => VerifyLookupStream1HudPosture(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the charge flare cadence program lookups against the retail cartridge.</summary>
    [VerificationCommand("--lookup-stream-1-cadence-programs")]
    internal static int LookupStream1CadenceProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyLookupStream1CadencePrograms), () => VerifyLookupStream1CadencePrograms(rom));
        return 0;
    }

    /// <summary>Runs visor color extraction and the independent visor color behavior suite.</summary>
    [VerificationCommand("--lookup-stream-1-visor-colors")]
    internal static int LookupStream1VisorColorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyLookupStream1VisorColors), () => VerifyLookupStream1VisorColors(rom));
        Suite(nameof(VerifySamusVisorColors), () => VerifySamusVisorColors());
        return 0;
    }

    /// <summary>Runs the escape typewriter presentation oracle against the retail cartridge.</summary>
    [VerificationCommand("--lookup-stream-1-escape-text")]
    internal static int LookupStream1EscapeTextCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyLookupStream1EscapeText), () => VerifyLookupStream1EscapeText(rom));
        return 0;
    }

    /// <summary>Runs atmospheric sprite attribute comparison and its artwork boundary suite.</summary>
    [VerificationCommand("--lookup-stream-1-atmospheric-attributes")]
    internal static int LookupStream1AtmosphericAttributesCommand()
    {
        string sourceRom = Path.GetFullPath("Super Metroid.smc");
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        Suite(nameof(VerifyLookupStream1AtmosphericAttributes), () => VerifyLookupStream1AtmosphericAttributes(rom));
        Suite(nameof(VerifySamusAtmosphereArtworkBoundary), () => VerifySamusAtmosphereArtworkBoundary(sourceRom));
        return 0;
    }

    /// <summary>Runs cannon drawing controls and pose-pointer comparison together.</summary>
    [VerificationCommand("--lookup-stream-1-cannon-drawing")]
    internal static int LookupStream1CannonDrawingCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyLookupStream1CannonDrawingControls), () => VerifyLookupStream1CannonDrawingControls(rom));
        Suite(nameof(VerifyLookupStream1CannonPoses), () => VerifyLookupStream1CannonPoses(rom));
        return 0;
    }
}
