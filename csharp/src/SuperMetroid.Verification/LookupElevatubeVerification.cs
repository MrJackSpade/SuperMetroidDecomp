using SuperMetroid.Core.Hardware;
using static SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms.Plms;

/// <summary>Checks the Maridia elevatube PLM's physical draw and short instruction program.</summary>
internal static class LookupElevatubeVerification
{
    /// <summary>Compares the single installed elevatube draw with its native run, tile, and displacement.</summary>
    internal static void VerifyElevatubePhysicalDraw(SuperMetroidAddressSpace rom)
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
            AssertEqual(unchecked((sbyte)ElevatubePhysicalDrawRomByte(rom, 0x84936b)), run.Next.X, "Elevatube original X");
            AssertEqual(unchecked((sbyte)ElevatubePhysicalDrawRomByte(rom, 0x84936c)), run.Next.Y, "Elevatube original Y");
        }
        foreach (string id in new[] {"", "ELEVATUBE-BLOCK", "elevatube-block "})
        {
            AssertTrue(!MaridiaElevatubePlmDefinitions.TryGetDrawByVisualId(id, out var missing), "Elevatube unknown visual ID");
            AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Elevatube missing DTO");
        }
    }

    /// <summary>Reads a native elevatube draw byte for the independent geometry comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Elevatube physical draw")]
    private static byte ElevatubePhysicalDrawRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the elevatube's three compiled control words.</summary>
    internal static void VerifyElevatubeProgramControls(SuperMetroidAddressSpace rom) => VerifyElevatubeProgramField(rom, false);
    /// <summary>Checks the elevatube's one drawing word.</summary>
    internal static void VerifyElevatubeProgramDraw(SuperMetroidAddressSpace rom) => VerifyElevatubeProgramField(rom, true);

    /// <summary>Compares either control or draw words over the complete program lookup domain.</summary>
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

    /// <summary>Checks the sole sound byte and rejection of every other sixteen-bit address.</summary>
    internal static void VerifyElevatubeProgramSound(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            AssertEqual(raw == 0xb8f6, MaridiaElevatubePlmDefinitions.TryReadMechanicsByte((ushort)raw, out byte value), "Elevatube sole sound byte domain");
            AssertEqual(raw == 0xb8f6 ? ElevatubeProgramSoundRomByte(rom, 0x84b8f6) : (byte)0, value, "Elevatube native sound or missing zero");
        }
    }

    /// <summary>Reads the native elevatube sound byte for comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Elevatube program sound")]
    private static byte ElevatubeProgramSoundRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
