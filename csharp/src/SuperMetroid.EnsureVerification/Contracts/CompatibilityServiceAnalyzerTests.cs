using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1401 compatibility-layer rule, SME6439.</summary>
internal static class CompatibilityServiceAnalyzerTests
{
    /// <summary>The attribute, declared as Core declares it.</summary>
    private const string Attribute = """
        namespace SuperMetroid.Core.Compatibility
        {
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Interface)]
            sealed class CompatibilityServiceAttribute(string behaviour) : System.Attribute { public string Behaviour { get; } = behaviour; }
        }
        """;

    /// <summary>An unmarked type in the layer, an implementation outside it, a mark outside it, a memory image, and a well-formed service.</summary>
    internal static void VerifyCompatibilityServiceAnalyzer()
    {
        var analyzer = new CompatibilityServiceAnalyzer();
        void Expect(string source, string? message = null) =>
            VerifySnippet(Attribute + source, message is null ? null : CompatibilityServiceAnalyzer.CompatibilityServiceId, message, analyzer,
                assemblyName: CoreLayerAnalyzer.CoreAssembly);

        Expect("namespace SuperMetroid.Core.Compatibility { sealed class SoundAcceptanceClock { } }",
            "'SoundAcceptanceClock' is in the compatibility layer; mark it [CompatibilityService]");
        Expect("namespace SuperMetroid.Core.Audio { [SuperMetroid.Core.Compatibility.CompatibilityService(\"sound acceptance timing\")] sealed class SoundAcceptanceClock { } }",
            "'SoundAcceptanceClock' is marked [CompatibilityService] outside the compatibility layer");
        Expect("""
            namespace SuperMetroid.Core.Compatibility { [CompatibilityService("room load progress")] interface IRoomLoadProgress { } }
            namespace SuperMetroid.Core.Rooms { sealed class InstantLoad : SuperMetroid.Core.Compatibility.IRoomLoadProgress { } }
            """, "'InstantLoad' implements the compatibility contract 'IRoomLoadProgress'");
        Expect("namespace SuperMetroid.Core.Compatibility { [CompatibilityService(\"sound acceptance timing\")] sealed class SoundAcceptanceClock { private readonly byte[] ports = new byte[4]; } }",
            "'ports' holds a memory image (byte[])");
        Expect("namespace SuperMetroid.Core.Compatibility { [CompatibilityService(\"sound acceptance timing\")] sealed class SoundAcceptanceClock { private System.ReadOnlyMemory<ushort> words; } }",
            "'words' holds a memory image");
        // A marked service with typed state, and its nested types, pass; so does a type outside the layer with no mark.
        Expect("""
            namespace SuperMetroid.Core.Compatibility
            {
                [CompatibilityService("when each sound library accepts a request")]
                sealed class SoundAcceptanceClock { private readonly int[] pendingRequests = new int[3]; private sealed class Phase { } }
            }
            namespace SuperMetroid.Core.Audio { sealed class Mixer { private readonly byte[] samples = []; } }
            """);
    }
}
