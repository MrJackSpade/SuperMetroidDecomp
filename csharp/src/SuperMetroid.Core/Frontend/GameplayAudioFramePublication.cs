using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Once-only publication of the sound producers preceding Samus's echo call.
/// One instance belongs to one frontend Step, never to the serialized game graph.
/// </summary>
/// <param name="audio">Cartridge audio state whose queued requests are published once per frame.</param>
internal sealed class GameplayAudioFramePublication(CartridgeAudioState audio)
{
    /// <summary>Counts room, palette-sound, palette-music, and liquid-sound requests already published for this frame.</summary>
    private int roomSounds, paletteSounds, paletteMusic, liquidSounds;

    /// <summary>Records whether the pending HUD selection sound has already been published for this frame.</summary>
    private bool selectionPublished;

    /// <summary>
    /// Flushes earlier native producers before the queue-dependent animation lookup.
    /// A later completion call publishes only newly appended requests, not duplicates.
    /// </summary>
    public void PublishPrefix(SuperMetroidRuntime runtime)
    {
        if (runtime.IsAttractDemo)
            return;
        var room = runtime.RoomLayer3Fx.SoundRequests;
        while (roomSounds < room.Count)
        {
            var request = room[roomSounds++];
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        }
        var palette = runtime.RoomPaletteFx.SoundRequests;
        while (paletteSounds < palette.Count)
        {
            var request = palette[paletteSounds++];
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        }
        var music = runtime.RoomPaletteFx.MusicRequests;
        while (paletteMusic < music.Count)
        {
            var request = music[paletteMusic++];
            audio.QueueMusicDelayed(request.Command, request.Delay);
        }
        if (runtime.Samus is not { } samus)
            return;
        // The early beta/echo pass can run before this frame refreshes the HUD.
        // Consume the request so the next frontend frame cannot replay the old flag.
        if (!selectionPublished && runtime.Hud.ConsumeSelectionSoundRequest())
        {
            audio.QueueSoundAndGetAccumulator(SoundEffectLibrary1Sounds.HudWeaponSelect, 6,
                soundSuppressed: runtime.Hud.SelectionSoundSuppressedThisFrame);
            selectionPublished = true;
        }
        var liquid = samus.LiquidPhysics.SoundRequests;
        while (liquidSounds < liquid.Count)
        {
            var request = liquid[liquidSounds++];
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        }
    }

    /// <summary>Executes the native Max6 sound call, preserving its accumulator result.</summary>
    public ushort QueueEcho(SuperMetroidRuntime runtime)
    {
        PublishPrefix(runtime);
        return audio.QueueSoundAndGetAccumulator(
            SoundEffectLibrary3Sounds.SpeedBoosterEcho, 6,
            soundSuppressed: runtime.IsAttractDemo || runtime.PowerBombExplosionSuppressesSounds);
    }

    /// <summary>Publishes the admitted Samus beta health check after earlier prefix producers.</summary>
    public void CheckLowHealth(SuperMetroidRuntime runtime)
    {
        PublishPrefix(runtime);
        if (runtime.Samus is not { } samus) throw new InvalidOperationException("Health check requires Samus.");
        samus.HealthWarning.Update(samus.Health, audio,
            soundSuppressed: runtime.IsAttractDemo || runtime.PowerBombExplosionSuppressesSounds);
    }
}
