using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Full stock import, live edit, repair and invalid-override contract for room PNGs.</summary>
    private static void VerifyRoomCharacterInstallation(string sourceRom)
    {
        string root = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "room-character-installation-" + Guid.NewGuid().ToString("N")));
        try
        {
            GameInstallation installed = GameAssetInstaller.Install(sourceRom, root);
            AssertTrue(File.Exists(Path.Combine(installed.RoomCharacterDirectory,
                    RoomCharacterArtworkFiles.ManifestFileName)),
                "room-character stock manifest is installed");
            RoomCharacterAtlasCatalog stock = installed.LoadRoomCharacters();
            SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
            CartridgeRoomHeader landing = CartridgeRoomHeader.Load(bus, 0x91f8);
            CartridgeRoomAssets native = CartridgeRoomAssets.Load(bus, landing);
            CartridgeRoomAssets installedRoom = CartridgeRoomAssets.Load(bus, landing, stock);
            AssertTrue(installedRoom.CreCharacters.AsSpan().SequenceEqual(native.CreCharacters) &&
                installedRoom.RoomCharacters.AsSpan().SequenceEqual(native.RoomCharacters),
                "installed stock room PNGs retain Landing Site character bytes");

            int source = native.Tileset.CharacterAddress;
            string name = RoomCharacterAtlasFormat.SourceFileName(source);
            int tileCount = native.RoomCharacters.Length / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
            int rows = (tileCount + columns - 1) / columns;
            IndexedPngImage image;
            using (var stockFile = File.OpenRead(Path.Combine(installed.RoomCharacterDirectory, name)))
                image = IndexedPng.Read(stockFile, columns * 8, rows * 8);
            image.Pixels[0] ^= 1;
            Directory.CreateDirectory(installed.RoomCharacterOverrideDirectory);
            string overridePath = Path.Combine(installed.RoomCharacterOverrideDirectory, name);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            RoomCharacterAtlasCatalog edited = installed.LoadRoomCharacters();
            CartridgeRoomAssets editedRoom = CartridgeRoomAssets.Load(bus, landing, edited);
            AssertTrue(!editedRoom.RoomCharacters.AsSpan().SequenceEqual(native.RoomCharacters),
                "installed PNG override changes the real room-loader output");
            AssertTrue(stock.Get(source).Transfer.Span.SequenceEqual(native.RoomCharacters),
                "room override does not mutate stock artwork already loaded");

            // A missing stock resource causes atomic re-extraction from the installed
            // cartridge; the edit remains outside the replaceable game directory.
            File.Delete(Path.Combine(installed.RoomCharacterDirectory, name));
            GameInstallation repaired = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Installed room-art repair lost its ROM.");
            AssertTrue(File.Exists(overridePath), "room-character override survives stock repair");
            AssertTrue(File.Exists(Path.Combine(repaired.RoomCharacterDirectory, name)),
                "room-character stock sheet is restored by repair");
            CartridgeRoomAssets repairedEdited = CartridgeRoomAssets.Load(bus, landing,
                repaired.LoadRoomCharacters());
            AssertTrue(repairedEdited.RoomCharacters.AsSpan().SequenceEqual(editedRoom.RoomCharacters),
                "repaired installation retains the selected user room artwork");

            File.WriteAllBytes(overridePath, "invalid indexed PNG"u8.ToArray());
            AssertThrows<InvalidDataException>(() => repaired.LoadRoomCharacters(),
                "invalid room-art override fails loudly instead of reverting to stock");

            Console.WriteLine("  Room character installation: stock import, live edit, " +
                "repair preservation and invalid-override rejection verified.");
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
