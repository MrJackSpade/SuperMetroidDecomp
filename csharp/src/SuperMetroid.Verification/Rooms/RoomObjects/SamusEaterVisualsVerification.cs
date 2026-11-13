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
        InstalledRoomObjectSamusEaterVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9e0d,"floor-idle"), new LookupEscapeGateStockFrame(0x9e45,"floor-chew-1"),
            new LookupEscapeGateStockFrame(0x9e61,"floor-chew-2"), new LookupEscapeGateStockFrame(0x9e7d,"floor-chew-3"), new LookupEscapeGateStockFrame(0x9e99,"ceiling-idle"),
            new LookupEscapeGateStockFrame(0x9ed1,"ceiling-chew-1"), new LookupEscapeGateStockFrame(0x9eed,"ceiling-chew-2"), new LookupEscapeGateStockFrame(0x9f09,"ceiling-chew-3")];
        foreach (InstalledRoomObjectSamusEaterVisuals pictures in new[] { InstalledRoomObjectSamusEaterVisuals.Stock(), installed })
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
        InstalledRoomObjectSamusEaterVisuals pictures = InstalledRoomObjectSamusEaterVisuals.Stock();
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            int frame = Array.IndexOf(pointers, pointer);
            AssertEqual(frame >= 0, SamusEaterObjectDraws.TryDescribe(NativeObjectDraws.At<SamusEaterDraw>(pointer), out var draw),
                "plant draw pointer ownership includes only supported records");
            AssertEqual(frame >= 0, RoomObjectSamusEaterVisualFiles.TryGet(NativeObjectDraws.At<SamusEaterDraw>(pointer), out var dto),
                "plant draw DTO pointer ownership");
            if (frame < 0)
            {
                AssertEqual(default(SamusEaterObjectDraws.Draw), draw, "missing plant descriptor");
                AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), dto, "missing plant DTO");
                continue;
            }
            AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[frame].Draw), "plant export native order");
            AssertEqual(3, dto.Runs.Length, "plant run count");
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
                        AssertEqual(native.Collision, draw.CollisionAt(run, block), "calculated plant collision");
                        AssertEqual(native.Collision, dtoRun.Collisions.Span[block], "direct plant DTO collision");
                        AssertEqual(native.Collision, exportedRun.Collisions.Span[block], "exported plant collision");
                        AssertEqual(native.Appearance, pictures.Image(dto.Draw).At(dtoRun.CellAt(block)), "installed plant picture");
                    }
                    foreach (int invalid in new[] { int.MinValue, -1, count, 256, int.MaxValue })
                        AssertThrows<ArgumentOutOfRangeException>(() => draw.CollisionAt(run, invalid), "plant block bounds");
                }
                else
                {
                    AssertEqual(count, SamusEaterObjectDraws.Draw.Count(run), "plant native horizontal count");
                    AssertRunHeader(count, dtoRun, "plant DTO direction/count");
                    AssertRunHeader(count, exportedRun, "plant export direction/count");
                    ushort offset = ReadSamusEaterObjectWord(rom, cursor + 2 + count * 2);
                    sbyte x = unchecked((sbyte)offset), y = unchecked((sbyte)(LittleEndianBytes.HighByte(offset)));
                    // The cartridge ends each run with the next run's start, and the last with no offset.
                    Offset16<RoomBlocks> continuation = run + 1 < SamusEaterObjectDraws.Draw.RunCount
                        ? draw.Start(run + 1) : Offset16<RoomBlocks>.Zero;
                    AssertEqual(x, continuation.X, "plant signed horizontal offset");
                    AssertEqual(y, continuation.Y, "plant signed vertical offset");
                    AssertEqual(x, RoomObjectRunContinuationCodecConstants.X(dto, run), "plant DTO horizontal offset");
                    AssertEqual(y, RoomObjectRunContinuationCodecConstants.Y(dto, run), "plant DTO vertical offset");
                    AssertEqual(x, RoomObjectRunContinuationCodecConstants.X(exported[frame], run), "plant export horizontal offset");
                    AssertEqual(y, RoomObjectRunContinuationCodecConstants.Y(exported[frame], run), "plant export vertical offset");
                }
                cursor += 4 + count * 2;
            }
            foreach (int invalid in new[] { int.MinValue, -1, 3, 256, int.MaxValue })
            {
                AssertThrows<ArgumentOutOfRangeException>(() => SamusEaterObjectDraws.Draw.Count(invalid), "plant run count bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => draw.Start(invalid), "plant start bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => draw.CollisionAt(invalid, 0), "plant word run bounds");

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
