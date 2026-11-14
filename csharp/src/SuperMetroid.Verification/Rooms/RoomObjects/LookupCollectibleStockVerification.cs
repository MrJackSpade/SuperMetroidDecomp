using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks stock collectible visual words and content identity against the native PLM frame table.</summary>
internal static class LookupCollectibleStockVerification
{
    /// <summary>
    /// Compares the 16 installed frames' pictures with the cartridge (#1538, #1557), requires the eight block-pair frames
    /// and every other draw to be unpictured, then checks the import and its rejections.
    /// </summary>
    internal static void VerifyCollectibleStockMapping(CartridgeImportAddressSpace rom,
        InstalledRoomObjectCollectibleVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xa2b5,"empty"),new LookupEscapeGateStockFrame(0xa2c7,"chozo-orb-dim"),new LookupEscapeGateStockFrame(0xa2cd,"chozo-orb-glowing"),new LookupEscapeGateStockFrame(0xa2d3,"chozo-orb-bright"),new LookupEscapeGateStockFrame(0xa2d9,"chozo-orb-burst"),new LookupEscapeGateStockFrame(0xa2df,"energy-tank-bright"),new LookupEscapeGateStockFrame(0xa2e5,"energy-tank-dim"),new LookupEscapeGateStockFrame(0xa2eb,"missile-tank-dim"),new LookupEscapeGateStockFrame(0xa2f1,"missile-tank-bright"),new LookupEscapeGateStockFrame(0xa2f7,"super-missile-tank-dim"),new LookupEscapeGateStockFrame(0xa2fd,"super-missile-tank-bright"),new LookupEscapeGateStockFrame(0xa303,"power-bomb-tank-bright"),new LookupEscapeGateStockFrame(0xa309,"power-bomb-tank-dim"),new LookupEscapeGateStockFrame(0xa30f,"first-block-pair-first-frame"),new LookupEscapeGateStockFrame(0xa315,"first-block-pair-second-frame"),new LookupEscapeGateStockFrame(0xa31b,"second-block-pair-first-frame"),new LookupEscapeGateStockFrame(0xa321,"second-block-pair-second-frame"),new LookupEscapeGateStockFrame(0xa327,"third-block-pair-first-frame"),new LookupEscapeGateStockFrame(0xa32d,"third-block-pair-second-frame"),new LookupEscapeGateStockFrame(0xa333,"fourth-block-pair-first-frame"),new LookupEscapeGateStockFrame(0xa339,"fourth-block-pair-second-frame"),new LookupEscapeGateStockFrame(0xa3dd,"shot-block-intact"),new LookupEscapeGateStockFrame(0xa3e3,"shot-block-cracking"),new LookupEscapeGateStockFrame(0xa3e9,"shot-block-dust")];
        bool Editable(LookupEscapeGateStockFrame frame) => RoomObjectCollectibleVisualFiles.IsEditable(
            RoomObjectDraw.FrameOf<CollectibleDraw>(NativeObjectDraws.At<CollectibleDraw>(frame.Pointer), "collectible"));
        LookupEscapeGateStockFrame[] installedFrames = [.. frames.Where(Editable)];
        AssertEqual(16, installedFrames.Length, "Collectible installed file holds the editable frames");
        foreach (InstalledRoomObjectCollectibleVisuals pictures in new[] { InstalledRoomObjectCollectibleVisuals.Stock(), installed })
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectCollectibleVisualFiles.Family, installedFrames, WrittenCells(RoomObjectCollectibleVisualFiles.Family));
        foreach (LookupEscapeGateStockFrame blockPair in frames.Where(frame => !Editable(frame)))
            AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<CollectibleDraw>(blockPair.Pointer)), "Collectible block-pair frame is not pictured");
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (frames.All(frame => frame.Pointer != pointer))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<CollectibleDraw>((ushort)pointer)), "Collectible stock unknown pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectCollectibleVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectCollectibleVisualDirectory);
    }
}
