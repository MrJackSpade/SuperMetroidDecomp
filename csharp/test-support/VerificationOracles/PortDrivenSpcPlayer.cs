using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Drives a bare <see cref="ManagedSpcPlayer"/> by raw music-port writes and library requests for driver-level
/// fixtures (#1304): the player's own <see cref="SoundAcceptanceClock"/> decides when each library
/// accepts and acknowledges a request, exactly as the game session's clock does.
/// </summary>
internal sealed class PortDrivenSpcPlayer
{
    /// <summary>The clock that schedules this player's ticks and request checks.</summary>
    private readonly SoundAcceptanceClock clock = new();

    /// <summary>The emulated driver under test.</summary>
    internal ManagedSpcPlayer Player { get; } = new();

    /// <summary>Writes the driver's music port, reaching the sequencer's latch.</summary>
    internal void WriteMusicPort(byte value) => Player.WriteMusicPort(value);

    /// <summary>Records the request to the library at zero-based <paramref name="library"/>.</summary>
    internal void RequestSound(int library, byte value) => clock.Request(library, value);

    /// <summary>Applies an upload stream to the player and the music load to the clock.</summary>
    internal void Upload(ReadOnlySpan<byte> stream)
    {
        Player.Uploads.Upload(stream);
        clock.PauseForMusicLoad();
    }

    /// <summary>Runs one update: the clock walks its steps, then the player follows its schedule.</summary>
    internal void GenerateFrame(Span<short> pcm)
    {
        Span<SoundClockStep> schedule = stackalloc SoundClockStep[SoundAcceptanceClock.MaximumStepsPerUpdate];
        int ticks = clock.Advance(schedule);
        Player.GenerateFrame(schedule[..ticks], pcm);
    }

    /// <summary>The acknowledgement the library at zero-based <paramref name="library"/> reports.</summary>
    internal byte Acknowledgement(int library) => checked((byte)clock.Acknowledgements[library]);
}
