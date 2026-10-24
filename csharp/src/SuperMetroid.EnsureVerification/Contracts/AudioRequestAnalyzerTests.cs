using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1410 audio-request rule, SME6446.</summary>
internal static class AudioRequestAnalyzerTests
{
    /// <summary>Audio requests carrying ports, addresses and raw values, and the requests and members the rule leaves alone.</summary>
    internal static void VerifyAudioRequestAnalyzer()
    {
        var analyzer = new AudioRequestAnalyzer();
        string core = CoreLayerAnalyzer.CoreAssembly;
        void Check(string source, string? message = null) =>
            VerifySnippet(source, message is null ? null : AudioRequestAnalyzer.AudioRequestId, message, analyzer, assemblyName: core);
        const string advice = "; an audio request names what it asks for (a music track, a sound, a music data set) by its domain identity";

        // A port write: the port is hardware.
        Check("""
            namespace SuperMetroid.Core.Audio;
            enum ApuPort { Music }
            sealed record WriteCommand(ApuPort Target);
            """, "'Target' carries a port" + advice);
        // The byte a port write carries is a raw value.
        Check("""
            namespace SuperMetroid.Core.Audio;
            sealed record SoundCommand(int Value);
            """, "'Value' carries an integral value" + advice);
        // An upload identified by the bank it installs.
        Check("""
            namespace SuperMetroid.Core.Audio;
            enum AudioBank { TitleSequence }
            sealed record LoadRequest(AudioBank Set);
            """, "'Set' carries a bank" + advice);
        // An upload's source address, even nullable.
        Check("""
            namespace SuperMetroid.Core.Audio;
            sealed record UploadCommand { public long? Source { get; init; } }
            """, "'Source' carries an integral value" + advice);
        // A kind nested in a request hierarchy is a request; the abstract word is reported once, not at each override.
        Check("""
            namespace SuperMetroid.Core.Audio;
            abstract record MusicCommand
            {
                public abstract ushort Word { get; }
                public sealed record Stop : MusicCommand { public override ushort Word => 0; }
            }
            """, "'Word' carries an integral value" + advice);
        // A kind derived from a request outside its declaration is a request.
        Check("""
            namespace SuperMetroid.Core.Audio;
            abstract record MusicRequest;
            sealed record Selection : MusicRequest { public byte[] Tracks { get; init; } = []; }
            """, "'Tracks' carries an integral value" + advice);
        // A request named for sound outside the audio module is an audio request.
        Check("""
            namespace SuperMetroid.Core.Enemies;
            readonly record struct EnemySoundRequest(byte Sound);
            """, "'Sound' carries an integral value" + advice);
        // A port enum held as a field.
        Check("""
            namespace SuperMetroid.Core.Audio;
            enum SoundPorts { Library1 }
            sealed class QueueRequest { private readonly SoundPorts library = SoundPorts.Library1; }
            """, "'library' carries a port" + advice);

        // Domain requests: a track, a data set, a sound and a delay by identity.
        Check("""
            namespace SuperMetroid.Core.Audio;
            enum MusicTrack { MaridiaTheme }
            enum MusicDataSet { Maridia }
            enum SoundEffect { XrayStart }
            readonly record struct MusicDelay(int Frames);
            abstract record MusicRequest
            {
                public sealed record PlayTrack(MusicTrack Track) : MusicRequest;
                public sealed record StopMusic : MusicRequest;
                public sealed record ChangeDataSet(MusicDataSet DataSet) : MusicRequest;
            }
            sealed record DelayedMusicRequest(MusicRequest Music, MusicDelay Delay);
            readonly record struct SoundRequest(SoundEffect Sound, bool Suppressed);
            """);
        // A request's static constant is not something the request carries.
        Check("""
            namespace SuperMetroid.Core.Audio;
            sealed record SoundRequest { public const int Capacity = 16; public static int Count => 3; }
            """);
        // A request outside audio is another rule's concern.
        Check("""
            namespace SuperMetroid.Core.Doors;
            sealed record DoorRequest(int Index);
            """);
        // An audio type that is not a request (a rendered clip's catalog entry) is not reported.
        Check("""
            namespace SuperMetroid.Core.Audio;
            sealed record MusicClipEntry(int Track, long Frames);
            """);
        // Outside Core the import pipeline decodes ports, addresses and request bytes.
        VerifySnippet("""
            namespace SuperMetroid.AssetExtraction.Audio;
            sealed record UploadCommand(int SourceAddress);
            """, analyzer: analyzer, assemblyName: "SuperMetroid.AssetExtraction");
    }
}
