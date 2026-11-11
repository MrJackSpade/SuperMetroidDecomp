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

    /// <summary>
    /// An unmarked type in the layer, an implementation outside it, a mark outside it, a memory image, a service without
    /// evidence, and well-formed services stating their evidence in their remarks or their summary.
    /// </summary>
    internal static void VerifyCompatibilityServiceAnalyzer()
    {
        var analyzer = new CompatibilityServiceAnalyzer();
        void Expect(string source, string? message = null) =>
            VerifySnippet(Attribute + source, message is null ? null : CompatibilityServiceAnalyzer.CompatibilityServiceId, message, analyzer,
                assemblyName: CoreLayerAnalyzer.CoreAssembly);

        Expect("namespace SuperMetroid.Core.Compatibility { sealed class SoundAcceptanceClock { } }",
            "'SoundAcceptanceClock' is in the compatibility layer; mark it [CompatibilityService]");
        Expect("""
            namespace SuperMetroid.Core.Audio
            {
                /// <summary>When each sound library accepts a request. Evidence: the acknowledgement traces.</summary>
                [SuperMetroid.Core.Compatibility.CompatibilityService("sound acceptance timing")] sealed class SoundAcceptanceClock { }
            }
            """, "'SoundAcceptanceClock' is marked [CompatibilityService] outside the compatibility layer");
        Expect("""
            namespace SuperMetroid.Core.Compatibility
            {
                /// <summary>How far a room's load has come. Evidence: the room load's own waits.</summary>
                [CompatibilityService("room load progress")] interface IRoomLoadProgress { }
            }
            namespace SuperMetroid.Core.Rooms { sealed class InstantLoad : SuperMetroid.Core.Compatibility.IRoomLoadProgress { } }
            """, "'InstantLoad' implements the compatibility contract 'IRoomLoadProgress'");
        Expect("""
            namespace SuperMetroid.Core.Compatibility
            {
                /// <summary>When each sound library accepts a request.</summary>
                /// <remarks>Evidence: the acknowledgement traces.</remarks>
                [CompatibilityService("sound acceptance timing")] sealed class SoundAcceptanceClock { private readonly byte[] ports = new byte[4]; }
            }
            """, "'ports' holds a memory image (byte[])");
        Expect("""
            namespace SuperMetroid.Core.Compatibility
            {
                /// <summary>When each sound library accepts a request.</summary>
                /// <remarks>Evidence: the acknowledgement traces.</remarks>
                [CompatibilityService("sound acceptance timing")] sealed class SoundAcceptanceClock { private System.ReadOnlyMemory<ushort> words; }
            }
            """, "'words' holds a memory image");

        // A service that names its behaviour but not what in the original game shows it: documented without evidence,
        // with the word only outside its summary and remarks, or not documented at all.
        Expect("""
            namespace SuperMetroid.Core.Compatibility
            {
                /// <summary>When each sound library accepts a request.</summary>
                /// <remarks>Advanced once per update.</remarks>
                [CompatibilityService("sound acceptance timing")] sealed class SoundAcceptanceClock { }
            }
            """, "'SoundAcceptanceClock' is a compatibility service whose documentation states no evidence");
        Expect("""
            namespace SuperMetroid.Core.Compatibility
            {
                /// <summary>When each sound library accepts a request.</summary>
                /// <typeparam name="TSound">Evidence: not a statement about the behaviour.</typeparam>
                [CompatibilityService("sound acceptance timing")] sealed class SoundAcceptanceClock<TSound> { }
            }
            """, "'SoundAcceptanceClock' is a compatibility service whose documentation states no evidence");
        Expect("""
            namespace SuperMetroid.Core.Compatibility
            {
                [CompatibilityService("sound acceptance timing")] sealed class SoundAcceptanceClock { }
            }
            """, "'SoundAcceptanceClock' is a compatibility service whose documentation states no evidence");

        // A marked service with typed state and its evidence in its remarks, and its nested types, pass; so does a type
        // outside the layer with no mark.
        Expect("""
            namespace SuperMetroid.Core.Compatibility
            {
                /// <summary>When each sound library accepts a request.</summary>
                /// <remarks>Advanced once per update. Evidence: the sound program's request read and the acknowledgement traces.</remarks>
                [CompatibilityService("when each sound library accepts a request")]
                sealed class SoundAcceptanceClock { private readonly int[] pendingRequests = new int[3]; private sealed class Phase { } }
            }
            namespace SuperMetroid.Core.Audio { sealed class Mixer { private readonly byte[] samples = []; } }
            """);
        // A service stating its evidence in its summary passes.
        Expect("""
            namespace SuperMetroid.Core.Compatibility
            {
                /// <summary>
                /// What one sound library reports back as acknowledged. Evidence: the acknowledgement traces.
                /// </summary>
                [CompatibilityService("what one sound library reports back as acknowledged")]
                readonly record struct SoundAcknowledgement(int Sound);
            }
            """);
    }
}
