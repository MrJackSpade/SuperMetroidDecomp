namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The audio calls a session's gameplay frame makes, by scenario: the Speed Booster's echo, whose queue's answer selects the
/// boost's cadence, and the low-health check. Both join the runtime's <see cref="GameplayAudioRequests"/> as they run, after
/// every request the update made before them.
/// </summary>
internal static class GameplayFrameAudio
{
    /// <summary>The audio calls one gameplay frame of <paramref name="runtime"/> makes: the echo and the low-health check.</summary>
    public static RuntimeFrameAudio Gameplay(SuperMetroidRuntime runtime)
    {
        Ensure.NotNull(runtime);
        return RuntimeFrameAudio.Gameplay(() => QueueEcho(runtime), () => CheckLowHealth(runtime));
    }

    /// <summary>The audio calls one attract-playback frame of <paramref name="runtime"/> makes: the echo only.</summary>
    public static RuntimeFrameAudio Attract(SuperMetroidRuntime runtime)
    {
        Ensure.NotNull(runtime);
        return RuntimeFrameAudio.EchoOnly(() => QueueEcho(runtime));
    }

    /// <summary>Requests the Speed Booster echo at normal priority, returning what the room library's queue did with it.</summary>
    /// <exception cref="InvalidOperationException">The runtime's requests reach no session queue to answer the echo.</exception>
    private static SoundQueueAdmission QueueEcho(SuperMetroidRuntime runtime) =>
        runtime.AudioRequests.QueueSound(RoomSound.SpeedBooster, SoundRequestPriority.Normal,
            soundSuppressed: runtime.PowerBombExplosionSuppressesSounds)
        ?? throw new InvalidOperationException("The Speed Booster's echo needs the session's sound queues to answer it.");

    /// <summary>Runs Samus's admitted low-health check.</summary>
    /// <exception cref="InvalidOperationException">The frame has no Samus.</exception>
    private static void CheckLowHealth(SuperMetroidRuntime runtime)
    {
        if (runtime.Player.Samus is not { } samus) throw new InvalidOperationException("Health check requires Samus.");
        samus.HealthWarning.Update(samus.Health, runtime.AudioRequests,
            soundSuppressed: runtime.PowerBombExplosionSuppressesSounds);
    }
}
