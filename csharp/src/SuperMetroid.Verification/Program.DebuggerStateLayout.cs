using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Desktop;

/// <summary>Represents debugger state layout verification.</summary>
internal static class DebuggerStateLayoutVerification
{
    /// <summary>
    /// #1270: debugger states restore only this build's exact layout. A stale field, a type this build
    /// lacks, or a retyped value is rejected as an unsupported state version instead of being converted.
    /// </summary>
    internal static void VerifyDebuggerStateRejectsStaleLayouts()
    {
        var state = new SamusGrappleState { PoseChangeAutoFireTimer = 7 };
        byte[] current = SerializeDebuggerGraph(state);
        AssertEqual(7, DeserializeDebuggerGraph<SamusGrappleState>(current).PoseChangeAutoFireTimer,
            "a current-layout graph round-trips");

        // Same-length replacements keep every length prefix valid, so only the identity changes.
        RequireUnsupported(Replace(current, "<PoseChangeAutoFireTimer>k__BackingField", "<PoseChangeAutoFireTamer>k__BackingField"),
            "a renamed field");
        RequireUnsupported(Replace(current, "SuperMetroid.Core.Game.SamusGrappleState,", "SuperMetroid.Core.Game.SamusGrappleStatf,"),
            "a type this build does not have");
        RequireUnsupported(Replace(current, "System.Int32,", "System.Int64,"), "a retyped field value");
        Console.WriteLine("Debugger state layout: current graphs round-trip; renamed fields, missing types and retyped values are rejected.");

        static void RequireUnsupported(byte[] stale, string label)
        {
            try
            {
                _ = DeserializeDebuggerGraph<SamusGrappleState>(stale);
            }
            catch (InvalidDataException exception) when (
                exception.Message.Contains(DebuggerObjectGraphSerializer.UnsupportedVersion, StringComparison.Ordinal))
            {
                return;
            }
            catch (EndOfStreamException)
            {
                throw new InvalidOperationException($"Verification failed: {label} was misread instead of rejected.");
            }
            throw new InvalidOperationException($"Verification failed: {label} was restored instead of rejected.");
        }

        static byte[] Replace(byte[] graph, string from, string to)
        {
            byte[] source = Encoding.UTF8.GetBytes(from), target = Encoding.UTF8.GetBytes(to);
            if (source.Length != target.Length)
                throw new InvalidOperationException("A layout substitution must keep the encoded length.");
            int index = graph.AsSpan().IndexOf(source);
            if (index < 0)
                throw new InvalidOperationException($"The serialized graph does not contain '{from}'.");
            byte[] copy = [.. graph];
            target.CopyTo(copy, index);
            return copy;
        }
    }

    /// <summary>serialize debugger graph.</summary>
    private static byte[] SerializeDebuggerGraph(object root)
    {
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, root);
        return stream.ToArray();
    }

    /// <summary>deserialize debugger graph.</summary>
    private static T DeserializeDebuggerGraph<T>(byte[] graph) where T : class
    {
        using var stream = new MemoryStream(graph, writable: false);
        return DebuggerObjectGraphSerializer.Deserialize<T>(stream);
    }

    /// <summary>debugger state layout command.</summary>
    [VerificationCommand("--debugger-state-layout")]
    internal static int DebuggerStateLayoutCommand()
    {
        Suite(nameof(VerifyDebuggerStateRejectsStaleLayouts), () => VerifyDebuggerStateRejectsStaleLayouts());
        return 0;
    }
}
