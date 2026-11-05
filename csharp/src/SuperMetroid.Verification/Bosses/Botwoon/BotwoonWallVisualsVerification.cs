using SuperMetroid.AssetExtraction;
using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonWallPlmsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks editable Botwoon wall-clear tiles remain separate from PLM collision and timing.</summary>
internal static class BotwoonWallVisualsVerification
{
    /// <summary>Compares the nine stock draw words with cartridge data and checks cloned custom artwork.</summary>
    internal static void VerifyBotwoonWallStockMapping(SuperMetroidAddressSpace rom)
    {
        var native = new RoomBlockAppearance[9];
        for (int index = 0; index < native.Length; index++)
            native[index] = RoomLevelWordCodec.DecodeAppearance(ReadRomWord(rom, 0x849311 + 2 * index));
        var stock = InstalledRoomPlmBotwoonWallVisuals.Stock();
        var loaded = RoomPlmBotwoonWallVisualFiles.Create([new("clear-wall", native)]);
        string expectedIdentity = SuperMetroid.AssetExtraction.SelectedPresentationHash.Create(
            nameof(InstalledRoomPlmBotwoonWallVisuals), content => content.AppendAppearances("blocks", native));
        AssertEqual(expectedIdentity, stock.ContentIdentity, "Botwoon calculated stock preserves prior content identity");
        AssertEqual(expectedIdentity, loaded.ContentIdentity, "Botwoon loaded stock identity");
        RoomBlockAppearance[] customWords = [.. new[] {0x10,0x20,0x30,0x58,0x50,0x60,0x70,0x80,0x90}.Select(block => RoomBlockAppearance.Of(block))];
        var custom = RoomPlmBotwoonWallVisualFiles.Create([new("clear-wall", customWords)]);
        string customIdentity = custom.ContentIdentity;
        for (int index = 0; index < native.Length; index++)
        {
            AssertEqual(native[index], stock.GetAppearance(NativePlmDraws.At<BotwoonWallDraw>(0x930f), 0, index), "Botwoon stock appearance matches native");
            AssertEqual(native[index], loaded.GetAppearance(NativePlmDraws.At<BotwoonWallDraw>(0x930f), 0, index), "Botwoon imported stock appearance matches native");
            AssertEqual(customWords[index], custom.GetAppearance(NativePlmDraws.At<BotwoonWallDraw>(0x930f), 0, index), "Botwoon custom appearance preserved");
        }
        Array.Fill(customWords, RoomBlockAppearance.Of(0));
        AssertEqual(customIdentity, custom.ContentIdentity, "Botwoon custom payload remains cloned");
        AssertTrue(customIdentity != expectedIdentity, "Botwoon custom content retains distinct identity");
        foreach (var catalog in new[] {stock, loaded, custom})
        {
            foreach (int index in new[] {int.MinValue,-1,9,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<BotwoonWallDraw>(0x930f), 0, index), "Botwoon visual block domain");
            foreach (int run in new[] {int.MinValue,-1,1,int.MaxValue})
                AssertThrows<InvalidDataException>(() => catalog.GetAppearance(NativePlmDraws.At<BotwoonWallDraw>(0x930f), run, 0), "Botwoon visual run domain");
            foreach (ushort pointer in new ushort[] {0,0x930e,0x9310,0xffff})
                AssertThrows<InvalidDataException>(() => catalog.GetAppearance(NativePlmDraws.At<BotwoonWallDraw>(pointer), 0, 0), "Botwoon visual pointer domain");
        }
        Suite(nameof(VerifyBotwoonWallVisualSeparation), () => VerifyBotwoonWallVisualSeparation(custom));
    }

    /// <summary>Requires only the native clear-draw pointer to resolve to Botwoon's stable visual ID.</summary>
    internal static void VerifyBotwoonWallVisualIdMapping()
    {
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (pointer == 0x930f)
                AssertEqual("clear-wall", RoomPlmBotwoonWallVisualFiles.VisualId(NativePlmDraws.At<BotwoonWallDraw>((ushort)pointer)), "Botwoon stable export identity");
            else
                AssertThrows<InvalidDataException>(() => RoomPlmBotwoonWallVisualFiles.VisualId(NativePlmDraws.At<BotwoonWallDraw>((ushort)pointer)), "Botwoon visual ID accepts only the clear draw");
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifyBotwoonWallVisuals()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomPlmBotwoonWallVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomPlmBotwoonWallVisualFilesConstants.ValidateStock(
                installation.RoomPlmBotwoonWallVisualDirectory);
            InstalledRoomPlmBotwoonWallVisuals stock =
                installation.LoadRoomPlmBotwoonWallVisuals();
            for (int block = 0; block < 9; block++)
                AssertEqual(RoomBlockAppearance.Of(0x0ff),
                    stock.GetAppearance(BotwoonWallPlmDrawDefinitions.Clear,
                        0, block),
                    $"stock Botwoon wall clear block {block} matches ROM");
            AssertThrows<InvalidDataException>(
                () => RoomPlmBotwoonWallVisualFiles.Create([]),
                "Botwoon wall visual catalog rejects missing clear frame");

            string stockPath = Path.Combine(
                installation.RoomPlmBotwoonWallVisualDirectory,
                RoomPlmBotwoonWallVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmBotwoonWallVisualFilesConstants.ValidateStock(
                    installation.RoomPlmBotwoonWallVisualDirectory),
                "tampered Botwoon wall stock fails loudly where it is read");
        }
        Console.WriteLine(
            "Botwoon wall visuals: native nine-tile clear, live art edit, physical/timing isolation and strict failures pass.");
    }

    /// <summary>Checks an edited clear tile reaches immediate and later BG1 draws without changing collision or deletion.</summary>
    internal static void VerifyBotwoonWallVisualSeparation(
        InstalledRoomPlmBotwoonWallVisuals edited)
    {
        const int width = 32;
        const int height = 16;
        var words = new RoomTerrainBlock[width * height];
        for (int y = 4; y <= 12; y++)
            words[y * width + 15] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.Air);
        RoomLevelData level = CreateRoom(new(width, height), words,
            new RoomBlockBehavior[words.Length]);
        var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmBotwoonWallVisuals = edited });
        AssertTrue(plms.BossRoomMutations.TrySpawnBotwoonWall(level,
                PlmHeaderId.ClearBotwoonWall),
            "edited Botwoon wall-clear PLM allocates");
        var guard = NewBotwoonWallSourceGuard(new TestAddressSpace());
        plms.Step(level);
        int blockIndex = 7 * width + 15;
        AssertEqual(RoomTerrainBlock.Of(0x0ff, BlockMirroring.None, RoomCollisionType.Air),
            level.GetCollisionBlockByIndex(blockIndex).Terrain,
            "edited Botwoon wall art retains the physical clear word");
        // A PLM redraw identifies its block by the BG1 ring destination it targets.
        AssertEqual(RoomBlockAppearance.Of(0x058), level.DisplayedAppearance(blockIndex),
            "Botwoon wall edit reaches immediate redraw");
        AssertEqual(RoomBlockAppearance.Of(0x058),
            level.DisplayedAppearance(blockIndex),
            "Botwoon wall edit survives background streaming");
        plms.Step(level);
        AssertEqual(0, plms.SlotTable.ActiveCount,
            "edited Botwoon wall preserves native deletion timing");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "edited Botwoon wall draw reads no source bytes");
    }

    /// <summary>Runs the installed Botwoon wall visual check from the command dispatcher.</summary>
    [VerificationCommand("--botwoon-wall-visuals")]
    internal static int BotwoonWallVisualsCommand()
    {
        Suite(nameof(VerifyBotwoonWallVisuals), () => VerifyBotwoonWallVisuals());
        return 0;
    }

}
