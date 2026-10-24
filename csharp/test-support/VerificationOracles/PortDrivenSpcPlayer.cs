using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Drives a bare <see cref="ManagedSpcPlayer"/> by raw music-port writes and library requests for driver-level
/// fixtures (#1304): the player's own <see cref="SoundAcceptanceClock{TSound}"/> decides when each library
/// accepts and acknowledges a request, exactly as the game session's clock does.
/// </summary>
internal sealed class PortDrivenSpcPlayer
{
    /// <summary>The clock that schedules this player's ticks and request checks.</summary>
    private readonly SoundAcceptanceClock<byte> clock = new();

    /// <summary>The byte the original library-one port held while new music loaded.</summary>
    private const byte MusicLoadByte = 187;

    /// <summary>The emulated driver under test.</summary>
    internal ManagedSpcPlayer Player { get; } = new();

    /// <summary>Writes the driver's music port, reaching the sequencer's latch.</summary>
    internal void WriteMusicPort(byte value) => Player.WriteMusicPort(value);

    /// <summary>Records the raw request number to the library at zero-based <paramref name="library"/>; zero requests nothing.</summary>
    internal void RequestSound(int library, byte value) => clock.Request(library, value == 0 ? null : value);

    /// <summary>Applies an upload stream to the player and the music load to the clock.</summary>
    internal void Upload(ReadOnlySpan<byte> stream)
    {
        Player.Uploads.Upload(stream);
        clock.PauseForMusicLoad();
    }

    /// <summary>Runs one update: the clock walks its steps, then the player follows its schedule.</summary>
    internal void GenerateFrame(Span<short> pcm)
    {
        Span<SoundClockStep<byte>> schedule = stackalloc SoundClockStep<byte>[SoundClockTiming.MaximumStepsPerUpdate];
        int ticks = clock.Advance(schedule);
        Player.GenerateFrame(schedule[..ticks], pcm);
    }

    /// <summary>The acknowledgement the library at zero-based <paramref name="library"/> reports.</summary>
    /// <remarks>Zero stands for nothing acknowledged, and the original port's byte for a music load.</remarks>
    internal byte Acknowledgement(int library) => clock.Acknowledgements[library] switch
    {
        { IsMusicLoading: true } => MusicLoadByte,
        { Sound: byte request } => request,
        _ => 0,
    };
}
