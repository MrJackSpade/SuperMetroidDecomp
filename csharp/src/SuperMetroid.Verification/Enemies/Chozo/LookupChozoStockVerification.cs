using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>Checks editable Chozo statue visuals against native draw runs and catalog constraints.</summary>
internal static class LookupChozoStockVerification
{
    /// <summary>Compares the three statue frames' pictures with the cartridge, rejects other draws, then checks the import and its rejections.</summary>
    internal static void VerifyChozoStatueStockMapping(SuperMetroidAddressSpace rom,
        RoomObjectChozoStatueVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xa2b5,"lower-norfair-cleared-hand"),new LookupEscapeGateStockFrame(0x9cc5,"wrecked-ship-clear-slope-access"),new LookupEscapeGateStockFrame(0x9d0f,"wrecked-ship-block-slope-access")];
        foreach (RoomObjectChozoStatueVisualCatalog pictures in new[] { RoomObjectChozoStatueVisualCatalog.Stock(), installed })
        {
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectChozoStatueVisualFiles.Family, frames, WrittenCells(RoomObjectChozoStatueVisualFiles.Family));
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<ChozoStatueDraw>((ushort)raw)), "Chozo stock unknown pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectChozoStatueVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectChozoStatueVisualDirectory);
    }
}
