using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1421 host-concern rule, SME6448.</summary>
internal static class HostConcernAnalyzerTests
{
    /// <summary>Each kind of host concern in Core, and the game code that shares a word with one.</summary>
    internal static void VerifyHostConcernAnalyzer()
    {
        var analyzer = new HostConcernAnalyzer();
        string core = CoreLayerAnalyzer.CoreAssembly;
        void Check(string source, string? message = null) =>
            VerifySnippet(source, message is null ? null : HostConcernAnalyzer.HostConcernId, message, analyzer, assemblyName: core);

        // Windows API names: the platform prefix, an API's own name, and a platform-invoke entry point.
        Check("class WindowsGamepadCodes { }", "'WindowsGamepadCodes' names the Windows API ('Windows')");
        Check("static class WinMMErrors { }", "'WinMMErrors' names the Windows API ('WinMM')");
        Check("""
            static class Calls { [System.Runtime.InteropServices.DllImport("winmm.dll")] public static extern int joyGetNumDevs(); }
            """, "'joyGetNumDevs' declares a native platform API entry point");
        // Device and driver identifiers.
        Check("class UsbAdapter { }", "'UsbAdapter' names an input device or driver ('Usb')");
        Check("enum JoystickPositionQueryFlags { None }", "'JoystickPositionQueryFlags' names an input device or driver ('Joystick')");
        // Synchronization primitives used for presentation: by the type's name, by its namespace, and a mailbox.
        Check("""
            sealed class PresentationGate { private readonly System.Threading.SemaphoreSlim sync = new(1, 1); }
            """, "'SemaphoreSlim' synchronizes presentation across threads");
        Check("""
            namespace Game.Rendering { sealed class FrameSlot { private readonly object gate = new(); public void Put() { lock (gate) { } } } }
            """, "'lock' synchronizes presentation across threads");
        Check("sealed class LatestFrameMailbox { }", "'LatestFrameMailbox' names a presentation hand-off between threads ('Mailbox')");
        // Wall-clock reads and deadlines.
        Check("""
            static class Pacing { public static long Sample() => System.Diagnostics.Stopwatch.GetTimestamp(); }
            """, "'Stopwatch' reads the wall clock");
        Check("""
            class Stamp { public System.DateTime At() => System.DateTime.UtcNow; }
            """, "'UtcNow' reads the wall clock");
        Check("readonly record struct FrameDeadline(double Next);", "'FrameDeadline' names wall-clock scheduling ('Deadline')");
        Check("static class Pacing { public const int MaximumCatchUpFrames = 2; }", "'MaximumCatchUpFrames' names wall-clock scheduling ('CatchUp')");
        // Host-configuration files.
        Check("static class GameOptionsIni { }", "'GameOptionsIni' names a host-configuration file format ('Ini')");
        Check("""
            static class Options { public const string FileName = "Game.ini"; }
            """, "'Game.ini' names a host-configuration file");
        // A named renderer.
        Check("enum RendererChoice { Software, Direct3D11 }", "'Direct3D11' names a renderer ('Direct3D11')");

        // Display windows are not the platform.
        Check("class GameplayWindows { public int Windows { get; } }");
        // The input latch hands buttons between threads; that is not presentation.
        Check("""
            namespace Game.Input { sealed class FrameInputLatch { private readonly object gate = new(); public void Put() { lock (gate) { } } } }
            """);
        // An audio queue's failure flag is not presentation either.
        Check("""
            namespace Game.Audio { sealed class QueuedSink { private int failed; public bool Failed => System.Threading.Volatile.Read(ref failed) != 0; } }
            """);
        // Words that only share letters with a device or renderer name.
        Check("class Enemy { public bool Hidden { get; } public int InitialTimer { get; } }");
        Check("enum EnemyFrame { KraidFootOam8D3D }");
        // A game timer is not the wall clock.
        Check("sealed class Timer { } class Bomb { private readonly Timer fuse = new(); }");
        // Outside Core the host owns all of it.
        VerifySnippet("class WindowsGamepadCodes { }", analyzer: analyzer, assemblyName: "SuperMetroid.Desktop");
    }
}
