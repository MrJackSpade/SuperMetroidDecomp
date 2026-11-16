using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Elevator;

/// <summary>Checks the three elevator platform draw lists against native PLM words.</summary>
internal static class LookupElevatorPlatformStockVerification
{
    /// <summary>Compares the three platform frames' pictures with the cartridge, rejects other draws, then checks the import and its rejections (#1557).</summary>
    internal static void VerifyElevatorPlatformStockMapping(CartridgeImportAddressSpace rom,
        InstalledRoomObjectElevatorPlatformVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xaa97,"first-frame"),new LookupEscapeGateStockFrame(0xaaaf,"second-frame"),new LookupEscapeGateStockFrame(0xaac7,"third-frame")];
        foreach (InstalledRoomObjectElevatorPlatformVisuals pictures in new[] { InstalledRoomObjectElevatorPlatformVisuals.Stock(), installed })
        {
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectElevatorPlatformVisualFiles.Family, frames, WrittenCells(RoomObjectElevatorPlatformVisualFiles.Family));
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<ElevatorPlatformDraw>((ushort)raw)), "Elevator platform stock unknown pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectElevatorPlatformVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectElevatorPlatformVisualDirectory);
    }
}
