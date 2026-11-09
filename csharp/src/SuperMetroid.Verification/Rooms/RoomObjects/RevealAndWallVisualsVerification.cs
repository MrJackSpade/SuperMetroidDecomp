using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the installed pictures of the bombed reveals, Crateria's escape passage, the old Tourian escape shaft wall and
/// the escape animals' rescue wall (#1557) against the cartridge's own draws.
/// </summary>
internal static class RevealAndWallVisualsVerification
{
    /// <summary>
    /// Confirms every installed frame shows, cell by cell in run order, the block the cartridge's draw list writes there,
    /// and that a document missing, repeating or reshaping a frame is rejected.
    /// </summary>
    internal static void VerifyRevealAndWallVisuals()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        InstalledRoomObjectRevealAndWallVisuals installed = InstalledRoomObjectRevealAndWallVisuals.Stock();
        var entries = new List<RoomObjectVisualEntry>();
        foreach (RoomObjectShotBlockDrawDefinitions.DrawList frame in RoomObjectRevealAndWallVisualFiles.All)
        {
            RoomBlockAppearance[] native = NativeCells(rom, RoomObjectDrawAddressCodec.Of(frame.Draw));
            string id = RoomObjectRevealAndWallVisualFiles.VisualId(frame.Draw);
            for (int cell = 0; cell < native.Length; cell++)
                AssertEqual(native[cell], installed.GetAppearance(frame.Draw, cell), $"{id} cell {cell} shows the cartridge's block");
            AssertThrows<ArgumentOutOfRangeException>(() => installed.GetAppearance(frame.Draw, native.Length), $"{id} has no cell past its last");
            entries.Add(new RoomObjectVisualEntry(id, native));
        }
        AssertThrows<InvalidDataException>(() => RoomObjectRevealAndWallVisualFiles.Create(entries.Skip(1)), "a missing reveal or wall frame is rejected");
        AssertThrows<InvalidDataException>(() => RoomObjectRevealAndWallVisualFiles.Create(entries.Append(entries[0])), "a repeated reveal or wall frame is rejected");
        RoomObjectVisualEntry[] reshaped = [.. entries];
        reshaped[0] = new(entries[0].Id, [.. entries[0].Blocks, entries[0].Blocks[0]]);
        AssertThrows<InvalidDataException>(() => RoomObjectRevealAndWallVisualFiles.Create(reshaped), "a reshaped reveal or wall frame is rejected");
        Console.WriteLine($"Reveal and wall visuals: {entries.Count} installed frames match the cartridge's draws.");
    }

    /// <summary>The blocks a native PLM draw list shows, run after run: each run's count word, its level words, then its next-run offset bytes, ending at a zero offset.</summary>
    private static RoomBlockAppearance[] NativeCells(SuperMetroidAddressSpace rom, int pointer)
    {
        var cells = new List<RoomBlockAppearance>();
        int cursor = pointer;
        while (true)
        {
            int count = CartridgeImportSource.Require(rom).ReadCartridgeWord(0x840000 | cursor) & 0x7fff;
            cursor += 2;
            for (int cell = 0; cell < count; cell++, cursor += 2)
                cells.Add(RoomLevelWordCodec.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x840000 | cursor)).Appearance);
            int x = rom.ReadCartridgeByte(0x840000 | cursor++), y = rom.ReadCartridgeByte(0x840000 | cursor++);
            if (x == 0 && y == 0)
                return [.. cells];
        }
    }

    /// <summary>Runs the focused reveal and wall picture check against the retail ROM.</summary>
    [VerificationCommand("--reveal-and-wall-visuals")]
    internal static int RevealAndWallVisualsCommand()
    {
        Suite(nameof(VerifyRevealAndWallVisuals), () => VerifyRevealAndWallVisuals());
        return 0;
    }
}
