using SuperMetroid.Core.Compatibility;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Drives a bare <see cref="ManagedSpcPlayer"/> by raw APU port writes for driver-level
/// fixtures (#1304): the player's own <see cref="SoundAcceptanceClock"/> decides when each library
/// accepts and acknowledges a request, exactly as the game session's clock does.
/// </summary>
internal sealed class PortDrivenSpcPlayer
{
    /// <summary>The port model that schedules this player's ticks and polls.</summary>
    private readonly SoundAcceptanceClock ports = new();

    /// <summary>The emulated driver under test.</summary>
    internal ManagedSpcPlayer Player { get; } = new();

    /// <summary>Writes one CPU-to-SPC port; the music port also reaches the sequencer's latch.</summary>
    internal void WritePort(ApuPort port, byte value)
    {
        if (port == ApuPort.Music)
            Player.WriteMusicPort(value);
        else
            ports.Request(ApuPorts.LibraryIndexAt(port), value);
    }

    /// <summary>Applies an upload stream to the player and its port half to the port model.</summary>
    internal void Upload(ReadOnlySpan<byte> stream)
    {
        Player.Uploads.Upload(stream);
        ports.PauseForMusicLoad();
    }

    /// <summary>Runs one update: the port model walks the timer, then the player follows its schedule.</summary>
    internal void GenerateFrame(Span<short> pcm)
    {
        Span<SoundClockStep> schedule = stackalloc SoundClockStep[SoundAcceptanceClock.MaximumStepsPerUpdate];
        int ticks = ports.Advance(schedule);
        Player.GenerateFrame(schedule[..ticks], pcm);
    }

    /// <summary>Reads one SPC-to-CPU port: the sequencer's music echo or a sound-effect acknowledgement.</summary>
    internal byte ReadPort(ApuPort port) =>
        port == ApuPort.Music ? checked((byte)Player.Music.musicPortToSnes) : checked((byte)ports.Acknowledgements[ApuPorts.LibraryIndexAt(port)]);
}
