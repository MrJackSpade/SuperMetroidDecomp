using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.LevelData;

/// <summary>Checks Samus Eater PLM draw geometry, editable visual blocks and physical collision isolation against retail data.</summary>
internal static class SamusEaterVisualsVerification
{
    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifySamusEaterVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),
            "Samus Eater native oracle revision");
        Suite(nameof(VerifySamusEaterDrawGeometry), () => VerifySamusEaterDrawGeometry(rom));
        Suite(nameof(VerifySamusEaterDrawWords), () => VerifySamusEaterDrawWords(rom));
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectSamusEaterVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomObjectSamusEaterVisualFilesConstants.ValidateStock(
            installation.RoomObjectSamusEaterVisualDirectory);
        VerifySamusEaterStockVisualMapping(rom, installation.LoadRoomObjectSamusEaterVisuals());

        string stockPath = Path.Combine(
            installation.RoomObjectSamusEaterVisualDirectory,
            RoomObjectSamusEaterVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => RoomObjectSamusEaterVisualFilesConstants.ValidateStock(
                installation.RoomObjectSamusEaterVisualDirectory),
            "tampered Samus Eater stock fails loudly where it is read");
        Console.WriteLine("Samus Eater visuals: eight exact native draw lists and stock integrity pass.");
    }

    /// <summary>Compares the eight plant frames' pictures with the cartridge, rejects other draws, then checks the import and its rejections (#1557).</summary>
    private static void VerifySamusEaterStockVisualMapping(CartridgeImportAddressSpace rom,
        InstalledRoomObjectPictures installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9e0d,"floor-idle"), new LookupEscapeGateStockFrame(0x9e45,"floor-teeth-peeking"),
            new LookupEscapeGateStockFrame(0x9e61,"floor-teeth-bared"), new LookupEscapeGateStockFrame(0x9e7d,"floor-fangs-out"), new LookupEscapeGateStockFrame(0x9e99,"ceiling-idle"),
            new LookupEscapeGateStockFrame(0x9ed1,"ceiling-teeth-peeking"), new LookupEscapeGateStockFrame(0x9eed,"ceiling-teeth-bared"), new LookupEscapeGateStockFrame(0x9f09,"ceiling-fangs-out")];
        foreach (InstalledRoomObjectPictures pictures in new[] { RoomObjectPicturesStock.SamusEater, installed })
        {
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectSamusEaterVisualFiles.Family, frames, WrittenCells(RoomObjectSamusEaterVisualFiles.Family));
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<SamusEaterDraw>((ushort)raw)), "plant stock unknown pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectSamusEaterVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectSamusEaterVisualDirectory);
    }

    /// <summary>Checks the run sizes and signed offsets of each compiled Samus Eater draw.</summary>
    private static void VerifySamusEaterDrawGeometry(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterDrawField), () => VerifySamusEaterDrawField(rom, false));

    /// <summary>Checks every compiled physical level word against its cartridge draw list.</summary>
    private static void VerifySamusEaterDrawWords(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterDrawField), () => VerifySamusEaterDrawField(rom, true));

    /// <summary>Scans pointer ownership and compares either draw geometry or level words for all eight native lists.</summary>
    private static void VerifySamusEaterDrawField(CartridgeImportAddressSpace rom, bool words)
    {
        ushort[] pointers = [0x9e0d, 0x9e45, 0x9e61, 0x9e7d, 0x9e99, 0x9ed1, 0x9eed, 0x9f09];
        var exported = RoomObjectSamusEaterVisualFiles.All.ToArray();
        AssertEqual(pointers.Length, exported.Length, "plant draw export coverage");
        InstalledRoomObjectPictures pictures = RoomObjectPicturesStock.SamusEater;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            int frame = Array.IndexOf(pointers, pointer);
            AssertEqual(frame >= 0, RoomObjectDrawIdentity.TryShape(SamusEaterObjectDraws.Family, NativeObjectDraws.At<SamusEaterDraw>(pointer), out var draw),
                "plant draw pointer ownership includes only supported records");
            AssertEqual(frame >= 0, RoomObjectSamusEaterVisualFiles.TryGet(NativeObjectDraws.At<SamusEaterDraw>(pointer), out var dto),
                "plant draw DTO pointer ownership");
            if (frame < 0)
            {
                AssertTrue(draw is null, "missing plant block shape");
                AssertEqual(default(RoomObjectDrawLists.DrawList), dto, "missing plant DTO");
                continue;
            }
            AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[frame].Draw), "plant export native order");
            AssertEqual(3, dto.Runs.Length, "plant run count");
            RoomBlockShape shape = draw ?? throw new InvalidOperationException($"${pointer:X4} has no plant block shape.");
            // The block shape covers exactly the eight cells the cartridge writes (#1862).
            AssertEqual(8, shape.Cells.Count, "plant block shape cell count");
            int cursor = 0x840000 | pointer;
            for (int run = 0; run < 3; run++)
            {
                ushort count = ReadSamusEaterObjectWord(rom, cursor);
                var exportedRun = exported[frame].Runs.Span[run];
                var dtoRun = dto.Runs.Span[run];
                if (words)
                {
                    for (int block = 0; block < count; block++)
                    {
                        RoomTerrainBlock native = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, cursor + 2 + block * 2));
                        AssertEqual(native.Collision, shape.CollisionAt(dtoRun.CellAt(block)), "plant block-shape collision");
                        AssertEqual(native.Collision, dtoRun.Collisions.Span[block], "direct plant DTO collision");
                        AssertEqual(native.Collision, exportedRun.Collisions.Span[block], "exported plant collision");
                        AssertEqual(native.Appearance, pictures.Image(dto.Draw).At(dtoRun.CellAt(block)), "installed plant picture");
                    }
                }
                else
                {
                    AssertRunHeader(count, dtoRun, "plant DTO direction/count");
                    AssertRunHeader(count, exportedRun, "plant export direction/count");
                    ushort offset = ReadSamusEaterObjectWord(rom, cursor + 2 + count * 2);
                    sbyte x = unchecked((sbyte)offset), y = unchecked((sbyte)(LittleEndianBytes.HighByte(offset)));
                    // The cartridge ends each run with the next run's start, and the last with no offset.
                    Offset16<RoomBlocks> continuation = run + 1 < dto.Runs.Length
                        ? dto.Runs.Span[run + 1].Start : Offset16<RoomBlocks>.Zero;
                    AssertEqual(x, continuation.X, "plant signed horizontal offset");
                    AssertEqual(y, continuation.Y, "plant signed vertical offset");
                    AssertEqual(x, RoomObjectRunContinuationCodecConstants.X(dto, run), "plant DTO horizontal offset");
                    AssertEqual(y, RoomObjectRunContinuationCodecConstants.Y(dto, run), "plant DTO vertical offset");
                    AssertEqual(x, RoomObjectRunContinuationCodecConstants.X(exported[frame], run), "plant export horizontal offset");
                    AssertEqual(y, RoomObjectRunContinuationCodecConstants.Y(exported[frame], run), "plant export vertical offset");
                }
                cursor += 4 + count * 2;
            }
        }
    }

    /// <summary>Runs the Samus Eater visual and physical draw checks from the verification command.</summary>
    [VerificationCommand("--samus-eater-visuals")]
    internal static int SamusEaterVisualsCommand()
    {
        Suite(nameof(VerifySamusEaterVisuals), () => VerifySamusEaterVisuals());
        return 0;
    }
}
