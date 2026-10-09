using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Compares the exported one-block elevatube draw DTO and its stable visual lookup with the native draw-list words.</summary>
    /// <param name="rom">Cartridge address space supplying the native draw-list header, physical word, and run offsets.</param>
    private static void VerifyElevatubePhysicalDraw(SuperMetroidAddressSpace rom)
    {
        AssertEqual(ReadSamusEaterPlmWord(rom, 0x849369), MaridiaElevatubePlmDefinitions.PhysicalWord, "Elevatube native scalar");
        var all = MaridiaElevatubePlmDefinitions.AllDraws.ToArray();
        AssertEqual(1, all.Length, "Elevatube single export");
        AssertTrue(MaridiaElevatubePlmDefinitions.TryGetDrawByVisualId("elevatube-block", out var selected), "Elevatube ordinal ID");
        foreach (var draw in new[] {MaridiaElevatubePlmDefinitions.Draw,all[0],selected})
        {
            AssertEqual((ushort)0x9367, draw.Pointer, "Elevatube original shared pointer");
            AssertEqual(1, draw.Runs.Length, "Elevatube one run");
            var run = draw.Runs.Span[0];
            AssertEqual(ReadSamusEaterPlmWord(rom, 0x849367), run.DirectionAndCount, "Elevatube native run header");
            AssertEqual(1, run.LevelWords.Length, "Elevatube one word");
            AssertEqual(ReadSamusEaterPlmWord(rom, 0x849369), run.LevelWords.Span[0], "Elevatube DTO original word");
            AssertEqual(unchecked((sbyte)rom.ReadByte(0x84936b)), run.NextX, "Elevatube original X");
            AssertEqual(unchecked((sbyte)rom.ReadByte(0x84936c)), run.NextY, "Elevatube original Y");
        }
        foreach (string id in new[] {"", "ELEVATUBE-BLOCK", "elevatube-block "})
        {
            AssertTrue(!MaridiaElevatubePlmDefinitions.TryGetDrawByVisualId(id, out var missing), "Elevatube unknown visual ID");
            AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Elevatube missing DTO");
        }
    }

    /// <summary>Checks the elevatube instruction list's word-sized controls against native bytes and shared PLM-reader ownership.</summary>
    /// <param name="rom">Cartridge address space containing the native instruction-list words.</param>
    private static void VerifyElevatubeProgramControls(SuperMetroidAddressSpace rom) => VerifyElevatubeProgramField(rom, false);

    /// <summary>Checks the elevatube's physical draw-pointer word against native bytes and shared PLM-reader ownership.</summary>
    /// <param name="rom">Cartridge address space containing the native draw-pointer word.</param>
    private static void VerifyElevatubeProgramDraw(SuperMetroidAddressSpace rom) => VerifyElevatubeProgramField(rom, true);

    /// <summary>Exhaustively checks which bank-local instruction addresses are recognized as control words or the draw pointer.</summary>
    /// <param name="rom">Cartridge address space used to compare recognized word values with the native instruction list.</param>
    /// <param name="draw"><see langword="true"/> to compare the physical draw-pointer word; <see langword="false"/> to compare instruction controls.</param>
    private static void VerifyElevatubeProgramField(SuperMetroidAddressSpace rom, bool draw)
    {
        ushort[] controls = [0xb8f0,0xb8f4,0xb8f7];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            bool owned = raw == 0xb8f2 || controls.Contains((ushort)raw);
            AssertEqual(owned, MaridiaElevatubePlmDefinitions.TryReadMechanicsWord((ushort)raw, out ushort value), "Elevatube complete word domain");
            if (!owned) AssertEqual((ushort)0, value, "Elevatube missing word zero");
            else if (draw == (raw == 0xb8f2))
            {
                AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | raw), value, "Elevatube native program field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord((ushort)raw, out ushort shared), "Elevatube shared reader ownership");
                AssertEqual(value, shared, "Elevatube shared reader value");
            }
        }
    }

    /// <summary>Checks the byte-reader domain and native value of the instruction list's sole library-two sound operand.</summary>
    /// <param name="rom">Cartridge address space containing the native sound operand byte.</param>
    private static void VerifyElevatubeProgramSound(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            AssertEqual(raw == 0xb8f6, MaridiaElevatubePlmDefinitions.TryReadMechanicsByte((ushort)raw, out byte value), "Elevatube sole sound byte domain");
            AssertEqual(raw == 0xb8f6 ? rom.ReadByte(0x84b8f6) : (byte)0, value, "Elevatube native sound or missing zero");
        }
    }
}
