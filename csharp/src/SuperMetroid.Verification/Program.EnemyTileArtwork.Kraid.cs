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
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address == KraidBackgroundRomData.UpperTilemap ||
                address == KraidBackgroundRomData.LowerTilemap)
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
