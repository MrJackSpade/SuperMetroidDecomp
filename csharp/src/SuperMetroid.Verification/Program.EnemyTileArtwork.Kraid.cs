using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledKraidBackground(
        SuperMetroidAddressSpace rom, string directory, EnemyTileArtworkCatalog stock)
    {
        KraidBackgroundArtwork art = stock.KraidBackground
            ?? throw new InvalidDataException("Installed enemy artwork lacks Kraid maps.");
        AssertTrue(art.Upper.Transfer.Span.SequenceEqual(RomDataReader.Decompress(
                rom, KraidBackgroundRomData.UpperTilemap,
                KraidBackgroundRomData.DecompressedTilemapBytes)),
            "installed Kraid upper BG2 tile references preserve source words");
        AssertTrue(art.Lower.Transfer.Span.SequenceEqual(RomDataReader.Decompress(
                rom, KraidBackgroundRomData.LowerTilemap,
                KraidBackgroundRomData.DecompressedTilemapBytes)),
            "installed Kraid lower BG2 tile references preserve source words");

        KraidEnemyState native = BuildKraidWorkingMap(rom, null);
        var guard = new KraidCompressedSourceGuard(rom);
        KraidEnemyState installed = BuildKraidWorkingMap(guard, stock);
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Kraid BG2 construction does not decompress either ROM source");
        AssertTrue(installed.BackgroundTilemapWords.SequenceEqual(native.BackgroundTilemapWords),
            "installed Kraid BG2 construction preserves all native working words");
        AssertEqual(native.BackgroundTilemapsPrepared, installed.BackgroundTilemapsPrepared,
            "Kraid BG2 initialization state remains unchanged");
        AssertEqual(native.OwnsBg2Tilemap, installed.OwnsBg2Tilemap,
            "Kraid BG2 ownership remains unchanged");
        SnesVram nativeVram = TransferKraidVisiblePages(native);
        SnesVram installedVram = TransferKraidVisiblePages(installed);
        AssertTrue(installedVram.Bytes.SequenceEqual(nativeVram.Bytes),
            "installed Kraid BG2 sources preserve both live VRAM pages");

        string upperPath = Path.Combine(directory, KraidBackgroundArtworkFormat.UpperFileName);
        byte[] original = File.ReadAllBytes(upperPath);
        string overrides = Path.Combine(directory, "kraid-override");
        Directory.CreateDirectory(overrides);
        RoomBackgroundTilemapDocument document =
            JsonSerializer.Deserialize<RoomBackgroundTilemapDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        RoomBackgroundTilemapPage[] pages = (RoomBackgroundTilemapPage[])document.Pages.Clone();
        RoomBackgroundTilemapCell[] cells = (RoomBackgroundTilemapCell[])pages[0].Cells.Clone();
        cells[0] = cells[0] with { TileColumn = cells[0].TileColumn ^ 1 };
        pages[0] = pages[0] with { Cells = cells };
        string overridePath = Path.Combine(overrides, KraidBackgroundArtworkFormat.UpperFileName);
        using (var output = File.Create(overridePath))
            RoomBackgroundTilemapAtlas.Write(output, document with { Pages = pages },
                KraidBackgroundRomData.DecompressedTilemapBytes);
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrides);
        KraidEnemyState changed = BuildKraidWorkingMap(new KraidCompressedSourceGuard(rom), edited);
        AssertEqual((ushort)(native.BackgroundTilemapWords[0] ^ 1),
            changed.BackgroundTilemapWords[0],
            "edited Kraid BG2 reference changes the visible working-map cell");
        AssertTrue(changed.BackgroundTilemapWords.AsSpan(1)
                .SequenceEqual(native.BackgroundTilemapWords.AsSpan(1)),
            "edited Kraid BG2 cell leaves the remaining live map unchanged");
        SnesVram editedVram = TransferKraidVisiblePages(changed);
        AssertEqual((byte)(nativeVram.ReadByte(KraidBackgroundRomData.LiveBg2TilemapWord * 2) ^ 1),
            editedVram.ReadByte(KraidBackgroundRomData.LiveBg2TilemapWord * 2),
            "edited Kraid BG2 reference changes the transferred VRAM tile word");
        AssertTrue(BuildKraidWorkingMap(new KraidCompressedSourceGuard(rom),
                EnemyTileArtworkFiles.Load(directory, overrides))
            .BackgroundTilemapWords.SequenceEqual(changed.BackgroundTilemapWords),
            "Kraid BG2 override survives catalog reload");

        File.WriteAllBytes(upperPath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, null),
            "tampered stock Kraid BG2 map fails its manifest hash");
        File.WriteAllBytes(upperPath, original);
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, overrides),
            "malformed Kraid BG2 override fails at load");
        VerifyInstalledKraidHeadFrames(rom, directory, stock);
    }

    private static void VerifyInstalledKraidHeadFrames(
        SuperMetroidAddressSpace rom, string directory, EnemyTileArtworkCatalog stock)
    {
        ushort[] pointers = KraidHeadInstructionDefinitions.All.ToArray()
            .Where(frame => frame.Kind == KraidHeadInstructionKind.Frame)
            .Select(frame => frame.Tilemap).Distinct().Order().ToArray();
        AssertEqual(4, pointers.Length, "four distinct Kraid head art frames");
        foreach (ushort pointer in pointers)
        {
            byte[] native = RomDataReader.ReadFixedBank(rom,
                KraidBackgroundRomData.NativeBank | pointer,
                KraidBackgroundRomData.HeadTilemapWords * sizeof(ushort));
            ReadOnlySpan<ushort> installed = stock.KraidBackground!.HeadWords(pointer);
            for (int word = 0; word < installed.Length; word++)
                AssertEqual((ushort)(native[word * 2] | native[word * 2 + 1] << 8),
                    installed[word], $"Kraid head ${pointer:X4} word {word}");

            (KraidEnemyState baseline, SnesVram nativeVram) = RunKraidHeadFrame(rom, null,
                pointer);
            var guard = new KraidCompressedSourceGuard(rom);
            (KraidEnemyState selected, SnesVram installedVram) = RunKraidHeadFrame(
                guard, stock, pointer);
            AssertEqual(0, guard.ForbiddenReadAttempts,
                $"Kraid head ${pointer:X4} production transfer avoids ROM artwork");
            AssertTrue(selected.BackgroundTilemapWords.SequenceEqual(
                    baseline.BackgroundTilemapWords),
                $"Kraid head ${pointer:X4} preserves the live working map");
            AssertTrue(installedVram.Bytes.SequenceEqual(nativeVram.Bytes),
                $"Kraid head ${pointer:X4} preserves the actual VRAM transfer");
            AssertEqual(baseline.HeadTilemapUploadCount, selected.HeadTilemapUploadCount,
                $"Kraid head ${pointer:X4} preserves upload count");
        }

        ushort editedPointer = pointers[0];
        string fileName = KraidBackgroundArtworkFormat.HeadFileName(editedPointer);
        string stockPath = Path.Combine(directory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(directory, "kraid-head-override");
        Directory.CreateDirectory(overrides);
        KraidHeadTilemapDocument document =
            JsonSerializer.Deserialize<KraidHeadTilemapDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        RoomBackgroundTilemapCell[] cells = (RoomBackgroundTilemapCell[])document.Cells.Clone();
        cells[0] = cells[0] with { TileColumn = cells[0].TileColumn ^ 1 };
        string overridePath = Path.Combine(overrides, fileName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document with { Cells = cells }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            }));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrides);
        var editedGuard = new KraidCompressedSourceGuard(rom);
        (_, SnesVram changedVram) = RunKraidHeadFrame(editedGuard, edited, editedPointer);
        AssertEqual(0, editedGuard.ForbiddenReadAttempts,
            "edited Kraid head frame avoids cartridge art reads");
        (_, SnesVram stockVram) = RunKraidHeadFrame(rom, null, editedPointer);
        int address = KraidBackgroundRomData.LiveBg2TilemapWord * 2;
        AssertEqual((byte)(stockVram.ReadByte(address) ^ 1), changedVram.ReadByte(address),
            "edited Kraid head tile changes the first live VRAM reference");
        AssertTrue(stockVram.Bytes[(address + 1)..]
                .SequenceEqual(changedVram.Bytes[(address + 1)..]),
            "Kraid head edit leaves all later VRAM bytes unchanged");
        (_, SnesVram reloadedVram) = RunKraidHeadFrame(
            new KraidCompressedSourceGuard(rom),
            EnemyTileArtworkFiles.Load(directory, overrides), editedPointer);
        AssertTrue(reloadedVram.Bytes.SequenceEqual(changedVram.Bytes),
            "Kraid head tilemap edit survives a catalog reload");

        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, null),
            "tampered Kraid head stock map fails manifest hash");
        File.WriteAllBytes(stockPath, original);
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, overrides),
            "malformed Kraid head override fails at load");
        VerifyInstalledKraidRoomBackground(rom, directory, stock);
    }

    private static void VerifyInstalledKraidRoomBackground(
        SuperMetroidAddressSpace rom, string directory, EnemyTileArtworkCatalog stock)
    {
        byte[] native = RomDataReader.ReadFixedBank(rom,
            KraidBackgroundRomData.RoomBackgroundTileAddress,
            KraidBackgroundRomData.RoomBackgroundTileBytes);
        AssertTrue(stock.KraidBackground!.RoomBackgroundTiles.Transfer.Span.SequenceEqual(native),
            "installed Kraid room-background PNG preserves native planar characters");
        SnesVram baseline = UploadKraidRoomBackground(rom, null);
        var guard = new KraidCompressedSourceGuard(rom);
        SnesVram installed = UploadKraidRoomBackground(guard, stock);
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Kraid room-background upload avoids ROM characters");
        AssertTrue(installed.Bytes.SequenceEqual(baseline.Bytes),
            "installed Kraid room-background upload preserves full native VRAM");

        string fileName = KraidBackgroundArtworkFormat.RoomBackgroundFileName;
        string stockPath = Path.Combine(directory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(directory, "kraid-room-background-override");
        Directory.CreateDirectory(overrides);
        using var input = new MemoryStream(original, writable: false);
        IndexedPngImage image = IndexedPng.Read(input, 16 * 8, 8);
        image.Pixels[0] ^= 1;
        string overridePath = Path.Combine(overrides, fileName);
        using (var output = File.Create(overridePath))
            IndexedPng.Write(output, image.Width, image.Height,
                image.Pixels, image.Palette);
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrides);
        var editedGuard = new KraidCompressedSourceGuard(rom);
        SnesVram changed = UploadKraidRoomBackground(editedGuard, edited);
        AssertEqual(0, editedGuard.ForbiddenReadAttempts,
            "edited Kraid room-background PNG avoids ROM reads");
        int destination = KraidBackgroundRomData.RoomBackgroundTileVramWord * sizeof(ushort);
        AssertEqual((byte)(baseline.ReadByte(destination) ^ 0x80),
            changed.ReadByte(destination),
            "edited Kraid background pixel changes native VRAM destination");
        AssertTrue(baseline.Bytes[(destination + 1)..]
                .SequenceEqual(changed.Bytes[(destination + 1)..]),
            "Kraid backdrop PNG edit leaves later VRAM bytes unchanged");
        SnesVram reloaded = UploadKraidRoomBackground(new KraidCompressedSourceGuard(rom),
            EnemyTileArtworkFiles.Load(directory, overrides));
        AssertTrue(reloaded.Bytes.SequenceEqual(changed.Bytes),
            "Kraid room-background PNG override survives catalog reload");

        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, null),
            "tampered Kraid room-background stock PNG fails manifest hash");
        File.WriteAllBytes(stockPath, original);
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, overrides),
            "malformed Kraid room-background PNG override fails at load");
    }

    private static SnesVram UploadKraidRoomBackground(
        ISnesAddressSpace bus, EnemyTileArtworkCatalog? artwork)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        var upload = typeof(RoomEnemySystem).GetMethod("UploadKraidRoomBackgroundTiles", flags)!
            .CreateDelegate<Action>(enemies);
        upload();
        return vram;
    }

    private static (KraidEnemyState State, SnesVram Vram) RunKraidHeadFrame(
        ISnesAddressSpace bus, EnemyTileArtworkCatalog? artwork, ushort pointer)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        var transfer = typeof(RoomEnemySystem).GetMethod("TransferKraidHeadTilemap", flags)!
            .CreateDelegate<Action<KraidEnemyState, ushort>>(enemies);
        var state = new KraidEnemyState();
        transfer(state, pointer);
        return (state, vram);
    }

    private static KraidEnemyState BuildKraidWorkingMap(
        ISnesAddressSpace bus, EnemyTileArtworkCatalog? artwork)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        var initialize = typeof(RoomEnemySystem)
            .GetMethod("InitializeKraidBackground", flags)!
            .CreateDelegate<Action<KraidEnemyState>>(enemies);
        var state = new KraidEnemyState();
        initialize(state);
        return state;
    }

    private static SnesVram TransferKraidVisiblePages(KraidEnemyState state)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        foreach (string method in new[] { "TransferKraidTopTilemap", "TransferKraidBottomTilemap" })
        {
            var transfer = typeof(RoomEnemySystem).GetMethod(method, flags)!
                .CreateDelegate<Action<KraidEnemyState>>(enemies);
            transfer(state);
        }
        AssertEqual(1, state.TopTilemapUploadCount, "Kraid upper page transfer count");
        AssertEqual(1, state.BottomTilemapUploadCount, "Kraid lower page transfer count");
        return vram;
    }

    private sealed class KraidCompressedSourceGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        private static readonly ushort[] HeadPointers = KraidHeadInstructionDefinitions.All.ToArray()
            .Where(frame => frame.Kind == KraidHeadInstructionKind.Frame)
            .Select(frame => frame.Tilemap).Distinct().ToArray();

        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address == KraidBackgroundRomData.UpperTilemap ||
                address == KraidBackgroundRomData.LowerTilemap ||
                address is >= KraidBackgroundRomData.RoomBackgroundTileAddress and <
                    KraidBackgroundRomData.RoomBackgroundTileAddress +
                    KraidBackgroundRomData.RoomBackgroundTileBytes ||
                HeadPointers.Any(pointer =>
                        address >= (KraidBackgroundRomData.NativeBank | pointer) &&
                        address < (KraidBackgroundRomData.NativeBank | pointer) +
                            KraidBackgroundRomData.HeadTilemapWords * sizeof(ushort)))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Kraid BG2 tried to decompress ROM source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
