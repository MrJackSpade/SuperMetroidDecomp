using SuperMetroid.EnsureAnalyzer;

namespace SuperMetroid.EnsureVerification;

internal static partial class Program
{
    /// <summary>#223: SME6350 recognizes little-endian words rebuilt from adjacent byte reads.</summary>
    private static void VerifyWordReadAnalyzer()
    {
        var analyzer = new WordReadAnalyzer();
        void Expect(string source, string? id = null, string? message = null) => VerifySnippet(source, id, message, analyzer);
        const string api =
            "namespace SuperMetroid.Core.Hardware { " +
            "public interface IImportCartridgeSource { byte ReadCartridgeByte(int cpuAddress); } " +
            "public static class SnesCartridgeImportExtensions { public static byte ReadCartridgeByte(this ISnesAddressSpace bus, int a) => 0; } " +
            "public static class SnesAddressMath2 { } } " +
            "namespace Game { using SuperMetroid.Core.Hardware; ";
        const string end = " }";

        // Linear and fixed-bank progressions, through the cartridge source, the address space and WRAM.
        Expect(api + "class C { ushort M(IImportCartridgeSource rom, int a) => (ushort)(rom.ReadCartridgeByte(a) | rom.ReadCartridgeByte(a + 1) << 8); }" + end,
            WordReadAnalyzer.ReconstructedWordId, "Use rom.ReadCartridgeWord(a)");
        Expect(api + "class C { ushort M(ISnesAddressSpace bus, int a) => unchecked((ushort)(bus.ReadCartridgeByte(a) | bus.ReadCartridgeByte(a + 1) << 8)); }" + end,
            WordReadAnalyzer.ReconstructedWordId, "Use CartridgeImportSource.Require(bus).ReadCartridgeWord(a)");
        Expect(api + "class C { ushort M(ISnesMutableMemory ram, int a) => (ushort)(ram.ReadWorkRamByte(a) | ram.ReadWorkRamByte(SnesAddressMath.AddWithinBank(a, 1)) << 8); }" + end,
            WordReadAnalyzer.ReconstructedWordId, "Use ram.ReadWorkRamWordFixedBank(a)");
        Expect(api + "class C { ushort M(IImportCartridgeSource rom, int p) => (ushort)(rom.ReadCartridgeByte(0x840000 | p) | rom.ReadCartridgeByte(0x840000 | unchecked((ushort)(p + 1))) << 8); }" + end,
            WordReadAnalyzer.ReconstructedWordId, "ReadCartridgeWordFixedBank(0x840000 | p)");
        // A bank OR'd with an unwrapped offset + 1 is reported for a deliberate choice.
        Expect(api + "class C { int M(IImportCartridgeSource rom, int p) => rom.ReadCartridgeByte(0x840000 | p) | rom.ReadCartridgeByte(0x840000 | (p + 1)) << 8; }" + end,
            WordReadAnalyzer.ReconstructedWordId, "choose Linear or FixedBank deliberately");
        // A bank OR'd into the same chain is kept.
        Expect(api + "class C { int M(IImportCartridgeSource rom, int p) => 0xad0000 | rom.ReadCartridgeByte(p) | rom.ReadCartridgeByte(p + 1) << 8; }" + end,
            WordReadAnalyzer.ReconstructedWordId, "Use rom.ReadCartridgeWord(p)");

        // Constant offsets, masked bank wraps and 24-bit longs.
        Expect(api + "class C { int M(IImportCartridgeSource rom, int c) => rom.ReadCartridgeByte(c + 4) | rom.ReadCartridgeByte(c + 5) << 8; }" + end,
            WordReadAnalyzer.ReconstructedWordId, "Use rom.ReadCartridgeWord(c + 4)");
        Expect(api + "class C { int M(IImportCartridgeSource rom, int a) => rom.ReadCartridgeByte(a) | rom.ReadCartridgeByte((a & 0xff0000) | ((a + 1) & 0xffff)) << 8; }" + end,
            WordReadAnalyzer.ReconstructedWordId, "Use rom.ReadCartridgeWordFixedBank(a)");
        Expect(api + "static class X { public static ushort ReadCartridgeWord(this IImportCartridgeSource s, int a) => 0; } " +
            "class C { int M(IImportCartridgeSource rom, int a) => rom.ReadCartridgeWord(a) | rom.ReadCartridgeByte(a + 2) << 16; }" + end,
            WordReadAnalyzer.ReconstructedWordId, "Use rom.ReadCartridgeLong(a)");

        // Non-adjacent bytes, different sources, unrelated byte APIs and the canonical implementation are left alone.
        Expect(api + "class C { int M(IImportCartridgeSource rom, int a) => rom.ReadCartridgeByte(a) | rom.ReadCartridgeByte(a + 2) << 8; }" + end);
        Expect(api + "class C { int M(IImportCartridgeSource rom, IImportCartridgeSource other, int a) => rom.ReadCartridgeByte(a) | other.ReadCartridgeByte(a + 1) << 8; }" + end);
        Expect(api + "class C { byte[] b = new byte[4]; int M(int a) => b[a] | b[a + 1] << 8; }" + end);
        Expect(api + "class R { public byte ReadCartridgeByte(int a) => 0; } class C { int M(R r, int a) => r.ReadCartridgeByte(a) | r.ReadCartridgeByte(a + 1) << 8; }" + end);
        Expect("namespace SuperMetroid.Core.Hardware { public interface IImportCartridgeSource { byte ReadCartridgeByte(int cpuAddress); } " +
            "public static class CartridgeWordReads { public static ushort ReadCartridgeWord(IImportCartridgeSource c, int a) => " +
            "(ushort)(c.ReadCartridgeByte(a) | c.ReadCartridgeByte(a + 1) << 8); } }");
    }
}
