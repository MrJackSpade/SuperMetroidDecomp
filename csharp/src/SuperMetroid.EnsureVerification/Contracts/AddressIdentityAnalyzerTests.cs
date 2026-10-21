using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1402 address-identity rules, SME6440 and SME6441.</summary>
internal static class AddressIdentityAnalyzerTests
{
    /// <summary>Each declaration kind carrying a pointer or bank, each way a lookup is keyed by one, and the near misses.</summary>
    internal static void VerifyAddressIdentityAnalyzer()
    {
        var analyzer = new AddressIdentityAnalyzer();
        void Value(string source, string? message = null) =>
            VerifySnippet(source, message is null ? null : AddressIdentityAnalyzer.AddressValueId, message, analyzer,
                assemblyName: CoreLayerAnalyzer.CoreAssembly);
        void Key(string source, string message) =>
            VerifySnippet(source, AddressIdentityAnalyzer.AddressKeyId, message, analyzer, assemblyName: CoreLayerAnalyzer.CoreAssembly);

        // Members, parameters and record components that carry a pointer or a bank.
        Value("class P { ushort spritemapPointer; }", "'spritemapPointer' carries a cartridge pointer; give the thing it locates a semantic identity, decoded at import");
        Value("class P { ushort NativePointer { get; } }", "'NativePointer' carries a cartridge pointer; give the thing it locates a semantic identity, decoded at import");
        Value("class P { int GetDisplayPointer() => 0; }", "'GetDisplayPointer' carries a cartridge pointer; give the thing it locates a semantic identity, decoded at import");
        Value("class P { int[] FramePointers = []; }", "'FramePointers' carries a cartridge pointer; give the thing it locates a semantic identity, decoded at import");
        Value("class P { void Draw(byte bank) { } }", "'bank' carries a cartridge bank; give the thing it locates a semantic identity, decoded at import");
        Value("record SpritemapDefinition(int Bank);", "'Bank' carries a cartridge bank; give the thing it locates a semantic identity, decoded at import");
        // Names that merely contain the words are not addresses: a banker, a pointer device, a bank of lights.
        Value("class P { int Banker; int BankCount; int PointerDevice; int Pointed; }");

        // Lookups keyed by a pointer or bank: built from a key selector, or indexed, probed or filled with one.
        string core = CoreLayerAnalyzer.CoreAssembly;
        VerifySnippetIds("""
            using System.Linq;
            record Row(int SourcePointer);
            class P { object Build(Row[] rows) => rows.ToDictionary(row => row.SourcePointer); }
            """, analyzer, core, AddressIdentityAnalyzer.AddressValueId, AddressIdentityAnalyzer.AddressKeyId);
        Key("""
            using System.Collections.Generic;
            class P { Dictionary<int, string> names = new(); string Name(int id) { var tablePointer = id; return names[tablePointer]; } }
            """, "This lookup is keyed by 'tablePointer', a cartridge pointer; key it by the semantic identity decoded at import");
        VerifySnippetIds("""
            using System.Collections.Generic;
            class P { IReadOnlyDictionary<ushort, string> names = new Dictionary<ushort, string>(); bool Has(S s) => names.ContainsKey(s.bank); }
            class S { public ushort bank; }
            """, analyzer, core, AddressIdentityAnalyzer.AddressKeyId, AddressIdentityAnalyzer.AddressValueId);
        // An integer-keyed lookup probed with an ordinary index is not an address catalog.
        VerifySnippet("""
            using System.Collections.Generic;
            class P { Dictionary<int, string> names = new(); string Name(int index) => names[index]; }
            """, analyzer: analyzer, assemblyName: core);
        // A dictionary keyed by a semantic identity is the destination, whatever the key's name.
        VerifySnippet("""
            using System.Collections.Generic;
            enum FrameId { Idle }
            class P { Dictionary<FrameId, string> names = new(); string Name(FrameId frame) => names[frame]; }
            """, analyzer: analyzer, assemblyName: CoreLayerAnalyzer.CoreAssembly);
        // Outside Core the import pipeline decodes pointers.
        VerifySnippet("class Reader { ushort spritemapPointer; }", analyzer: analyzer, assemblyName: "SuperMetroid.AssetExtraction");
    }
}
