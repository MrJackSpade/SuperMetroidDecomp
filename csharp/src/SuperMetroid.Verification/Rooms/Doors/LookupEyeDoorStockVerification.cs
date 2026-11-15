using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks the installed eye-door frame pictures and the mirrored opening clear against the cartridge (#1557).</summary>
internal static class LookupEyeDoorStockVerification
{
    /// <summary>
    /// Compares the 23 published frames' pictures and the mirrored clear's with the cartridge's draw lists, rejects other
    /// draws, then checks the import and its rejections.
    /// </summary>
    internal static void VerifyEyeDoorStockMapping(CartridgeImportAddressSpace rom, InstalledRoomObjectEyeDoorVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [
            new LookupEscapeGateStockFrame(0x9c03,"left-eye-closed"),new LookupEscapeGateStockFrame(0x9c0b,"left-eye-half-open"),new LookupEscapeGateStockFrame(0x9c13,"left-eye-open"),
            new LookupEscapeGateStockFrame(0x9c1b,"left-eye-flinching"),new LookupEscapeGateStockFrame(0x9c23,"left-eye-open-solid"),
            new LookupEscapeGateStockFrame(0x9c2b,"left-middle-resting"),new LookupEscapeGateStockFrame(0x9c31,"left-middle-swelling"),new LookupEscapeGateStockFrame(0x9c37,"left-middle-swollen"),
            new LookupEscapeGateStockFrame(0x9c3d,"left-bottom-resting"),new LookupEscapeGateStockFrame(0x9c43,"left-bottom-swelling"),new LookupEscapeGateStockFrame(0x9c49,"left-bottom-swollen"),
            new LookupEscapeGateStockFrame(0x9c4f,"left-eye-clear"),
            new LookupEscapeGateStockFrame(0x9c5b,"right-eye-closed"),new LookupEscapeGateStockFrame(0x9c63,"right-eye-half-open"),new LookupEscapeGateStockFrame(0x9c6b,"right-eye-open"),
            new LookupEscapeGateStockFrame(0x9c73,"right-eye-flinching"),new LookupEscapeGateStockFrame(0x9c7b,"right-eye-open-solid"),
            new LookupEscapeGateStockFrame(0x9c83,"right-middle-resting"),new LookupEscapeGateStockFrame(0x9c89,"right-middle-swelling"),new LookupEscapeGateStockFrame(0x9c8f,"right-middle-swollen"),
            new LookupEscapeGateStockFrame(0x9c95,"right-bottom-resting"),new LookupEscapeGateStockFrame(0x9c9b,"right-bottom-swelling"),new LookupEscapeGateStockFrame(0x9ca1,"right-bottom-swollen"),
        ];
        const ushort mirroredClear = 0x9bf7;
        // Each frame is one run; its header word holds how many cells the run writes.
        int written = frames.Sum(frame => ReadSamusEaterObjectWord(rom, 0x840000 | frame.Pointer) & 0x7fff);
        foreach (InstalledRoomObjectEyeDoorVisuals pictures in new[] { InstalledRoomObjectEyeDoorVisuals.Stock(), installed })
        {
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectEyeDoorVisualFiles.Family, frames, written);
            RoomObjectFrameImage clear = pictures.Image(NativeObjectDraws.At<EyeDoorDraws.Draw>(mirroredClear));
            AssertAliasPicturedAsNative(rom, clear, mirroredClear, "Eye door original mirrored native word");
            RoomObjectFrameImage shared = pictures.Image(NativeObjectDraws.At<EyeDoorDraws.Draw>(0x9c4f));
            foreach (KeyValuePair<Offset16<RoomBlocks>, RoomBlockAppearance> cell in shared.Cells)
                AssertEqual(cell.Value.WithMirroring(cell.Value.Mirroring ^ BlockMirroring.Horizontal), clear.At(cell.Key), "Eye door shared clear mirror");
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (raw != mirroredClear && frames.All(frame => frame.Pointer != raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<EyeDoorDraws.Draw>((ushort)raw)), "Eye door stock full pointer rejection");
        AssertImportAndRejections(rom, installed.Images, RoomObjectEyeDoorVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectEyeDoorVisualDirectory);
    }
}
