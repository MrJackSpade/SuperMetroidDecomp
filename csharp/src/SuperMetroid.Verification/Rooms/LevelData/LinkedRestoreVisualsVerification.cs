using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>Checks bomb and crumble block restore artwork against native draw lists and production PLM behavior.</summary>
internal static class LinkedRestoreVisualsVerification
{
    /// <summary>The bomb-block or contact-crumble restoration draw the cartridge stores at <paramref name="pointer"/>.</summary>
    private static RoomObjectDraw LinkedAt(int pointer) => NativeObjectDraws.IsDrawAt<BombBlockRestoreDraw>(pointer)
        ? NativeObjectDraws.At<BombBlockRestoreDraw>(pointer) : NativeObjectDraws.At<ContactCrumbleRestoreDraw>(pointer);

    /// <summary>Compares the six restoration frames' pictures with the cartridge, rejects other draws, then checks the import and its rejections (#1557).</summary>
    private static void VerifyLinkedRestoreStockMapping(CartridgeImportAddressSpace rom)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xa4c7,"bomb-horizontal"), new LookupEscapeGateStockFrame(0xa4cf,"bomb-vertical"),
            new LookupEscapeGateStockFrame(0xa4d7,"bomb-square"), new LookupEscapeGateStockFrame(0xa4a1,"crumble-horizontal"), new LookupEscapeGateStockFrame(0xa4a9,"crumble-vertical"), new LookupEscapeGateStockFrame(0xa4b1,"crumble-square")];
        InstalledRoomObjectLinkedRestoreVisuals stock = InstalledRoomObjectLinkedRestoreVisuals.Stock();
        AssertPicturedAsNative(rom, stock.Images, RoomObjectLinkedRestoreVisualFiles.Family, frames, WrittenCells(RoomObjectLinkedRestoreVisualFiles.Family));
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (frames.All(frame => frame.Pointer != pointer))
                AssertThrows<InvalidDataException>(() => stock.Image(LinkedAt(pointer)), "linked restore rejects unknown frame");
        AssertImportAndRejections(rom, stock.Images, RoomObjectLinkedRestoreVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectLinkedRestoreVisualDirectory);
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifyLinkedRestoreVisualInstallation(
        CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyLinkedRestoreStockMapping), () => VerifyLinkedRestoreStockMapping(rom));
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectLinkedRestoreVisualDirectory);
        GameInstallation installation = copy.Installation;
        // The installer's extraction checked all six compiled row shapes, full physical
        // words, and signed offsets against the pinned retail cartridge.
        RoomObjectLinkedRestoreVisualFilesConstants.ValidateStock(
            installation.RoomObjectLinkedRestoreVisualDirectory);
        AssertEqual(RoomBlockAppearance.Of(0x058),
            installation.LoadRoomObjectLinkedRestoreVisuals().Image(
                RoomObjectDraw.Of(BombBlockRestoreDraw.Horizontal)).At(Offset16<RoomBlocks>.Zero),
            "stock bomb restore selects cartridge block 058");
        AssertEqual(RoomBlockAppearance.Of(0x0bc),
            installation.LoadRoomObjectLinkedRestoreVisuals().Image(
                RoomObjectDraw.Of(ContactCrumbleRestoreDraw.Horizontal)).At(Offset16<RoomBlocks>.Zero),
            "stock crumble restore selects cartridge block 0BC");

        string stockPath = Path.Combine(
            installation.RoomObjectLinkedRestoreVisualDirectory,
            RoomObjectLinkedRestoreVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => RoomObjectLinkedRestoreVisualFilesConstants.ValidateStock(
                installation.RoomObjectLinkedRestoreVisualDirectory),
            "tampered restore stock fails loudly where it is read");
    }
}
