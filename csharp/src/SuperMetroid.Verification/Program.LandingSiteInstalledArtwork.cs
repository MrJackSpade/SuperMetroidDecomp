using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// The dedicated Landing Site loader must select installed artwork just like the
    /// general room loader, while continuing to read the cartridge's collision-bearing
    /// level stream and native setup commands.
    /// </summary>
    private static void VerifyLandingSiteInstalledArtwork(
        GameInstallation installation,
        ISnesAddressSpace bus,
        LandingSiteEntryState entry,
        RoomCharacterAtlasCatalog stockCharacters,
        RoomCharacterAtlasCatalog editedCharacters,
        RoomMetatileCatalog stockBlocks,
        RoomSkyTilemapCatalog stockSky)
    {
        var guard = new LandingVisualSourceReadGuard(bus);
        RoomLevelData nativeLevel = LandingSiteStreamingData.LoadLevel(bus);
        RoomLevelData installedLevel = LandingSiteStreamingData.LoadLevel(guard, stockBlocks);
        AssertTrue(nativeLevel.ForegroundEntries.Span.SequenceEqual(installedLevel.ForegroundEntries.Span) &&
            nativeLevel.BehaviorBytes.Span.SequenceEqual(installedLevel.BehaviorBytes.Span) &&
            nativeLevel.BackgroundEntries.Span.SequenceEqual(installedLevel.BackgroundEntries.Span) &&
            nativeLevel.BlockDefinitions.Span.SequenceEqual(installedLevel.BlockDefinitions.Span),
            "Landing Site installed blocks preserve every native level and visual word");

        RoomLevelData editedLevel = LandingSiteStreamingData.LoadLevel(
            guard, installation.LoadRoomMetatiles());
        AssertTrue(!nativeLevel.BlockDefinitions.Span.SequenceEqual(
                editedLevel.BlockDefinitions.Span) &&
            nativeLevel.ForegroundEntries.Span.SequenceEqual(editedLevel.ForegroundEntries.Span) &&
            nativeLevel.BehaviorBytes.Span.SequenceEqual(editedLevel.BehaviorBytes.Span),
            "Landing Site block edit changes visuals without changing level placement or BTS");

        var nativeVram = new SnesVram();
        LandingSiteStreamingData.LoadCharacterGraphics(bus, nativeVram, entry);
        var installedVram = new SnesVram();
        LandingSiteStreamingData.LoadCharacterGraphics(guard, installedVram, entry,
            stockSky, stockCharacters);
        AssertTrue(nativeVram.Bytes.SequenceEqual(installedVram.Bytes),
            "Landing Site stock characters and sky match native VRAM without visual ROM reads");

        var editedVram = new SnesVram();
        LandingSiteStreamingData.LoadCharacterGraphics(guard, editedVram, entry,
            stockSky, editedCharacters);
        AssertTrue(nativeVram.ReadByte(RoomAssetRomData.GraphicsLayout.AreaCharactersVramByteOffset) !=
            editedVram.ReadByte(RoomAssetRomData.GraphicsLayout.AreaCharactersVramByteOffset),
            "Landing Site area-character edit reaches its native VRAM destination");
        Console.WriteLine("  Landing Site: dedicated block and character loaders use installed art " +
            "without rereading visual ROM sources; stock VRAM and level data match.");
    }

    private sealed class LandingVisualSourceReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (InRange(address, RoomAssetRomData.LandingSite.CreBlockDefinitions) ||
                InRange(address, RoomAssetRomData.LandingSite.AreaBlockDefinitions) ||
                InRange(address, RoomAssetRomData.LandingSite.CreCharacters) ||
                InRange(address, RoomAssetRomData.LandingSite.AreaCharacters) ||
                address >= RoomSkyTilemapFormat.FirstSourceAddress &&
                    address < RoomSkyTilemapFormat.FirstSourceAddress +
                        RoomSkyTilemapFormat.TotalByteCount)
                throw new InvalidOperationException(
                    $"Landing Site reread installed visual source ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static bool InRange(int address,
            RoomAssetRomData.BoundedCompressedAsset stream) =>
            address >= stream.Address && address < stream.Address + stream.StoredByteCount;
    }
}
