namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Once-only publication of the sound producers preceding Samus's echo call.
/// One instance belongs to one frontend Step, never to the serialized game graph.
/// </summary>
internal sealed class GameplayAudioFramePublication(AudioQueues audio)
{
    /// <summary>Per-producer cursors prevent replaying requests when prefix publication runs more than once.</summary>
    private int roomSounds, paletteSounds, paletteMusic, liquidSounds;
    /// <summary>Prevents a HUD selection request from being queued twice in the same frontend step.</summary>
    private bool selectionPublished;

    /// <summary>
    /// Flushes earlier native producers before the queue-dependent animation lookup.
    /// A later completion call publishes only newly appended requests, not duplicates.
    /// </summary>
    public void PublishPrefix(SuperMetroidRuntime runtime)
    {
        if (runtime.AttractDemo.IsAttractDemo)
            return;
        var room = runtime.Room.LoadedScope.Layer3Fx.SoundRequests;
        while (roomSounds < room.Count)
        {
            var request = room[roomSounds++];
            audio.QueueSound(request);
        }
        var palette = runtime.Room.LoadedScope.PaletteFx.SoundRequests;
        while (paletteSounds < palette.Count)
        {
            var request = palette[paletteSounds++];
            audio.QueueSound(request);
        }
        var music = runtime.Room.LoadedScope.PaletteFx.MusicRequests;
        while (paletteMusic < music.Count)
        {
            var request = music[paletteMusic++];
            audio.QueueMusicDelayed(request.Request, request.Delay);
        }
        if (runtime.Player.Samus is not { } samus)
            return;
        // The early beta/echo pass can run before this frame refreshes the HUD.
        // Consume the request so the next frontend frame cannot replay the old flag.
        if (!selectionPublished && runtime.Hud.ConsumeSelectionSoundRequest())
        {
            audio.QueueSound(Library1Sound.HudWeaponSelect, SoundQueueLimit.Max6,
                soundSuppressed: runtime.Hud.SelectionSoundSuppressedThisFrame);
            selectionPublished = true;
        }
        var liquid = samus.LiquidPhysics.SoundRequests;
        while (liquidSounds < liquid.Count)
        {
            var request = liquid[liquidSounds++];
            audio.QueueSound(request);
        }
    }

    /// <summary>Executes the native Max6 sound call, returning what the queue did with the echo.</summary>
    public SoundQueueAdmission QueueEcho(SuperMetroidRuntime runtime)
    {
        PublishPrefix(runtime);
        return audio.QueueSound(
            Library3Sound.SpeedBooster, SoundQueueLimit.Max6,
            soundSuppressed: runtime.AttractDemo.IsAttractDemo || runtime.PowerBombExplosionSuppressesSounds);
    }

    /// <summary>Publishes the admitted Samus beta health check after earlier prefix producers.</summary>
    public void CheckLowHealth(SuperMetroidRuntime runtime)
    {
        PublishPrefix(runtime);
        if (runtime.Player.Samus is not { } samus) throw new InvalidOperationException("Health check requires Samus.");
        samus.HealthWarning.Update(samus.Health, audio,
            soundSuppressed: runtime.AttractDemo.IsAttractDemo || runtime.PowerBombExplosionSuppressesSounds);
    }
    /// <summary>The audio calls one gameplay frame of <paramref name="runtime"/> makes through this publication.</summary>
    public RuntimeFrameAudio GameplayFrameAudio(SuperMetroidRuntime runtime) =>
        RuntimeFrameAudio.Gameplay(() => QueueEcho(runtime), () => CheckLowHealth(runtime));

    /// <summary>The audio calls one attract-playback frame of <paramref name="runtime"/> makes: the echo only.</summary>
    public RuntimeFrameAudio AttractFrameAudio(SuperMetroidRuntime runtime) =>
        RuntimeFrameAudio.EchoOnly(() => QueueEcho(runtime));
}
