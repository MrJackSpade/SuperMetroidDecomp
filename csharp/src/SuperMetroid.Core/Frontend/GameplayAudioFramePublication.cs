using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Once-only publication of the sound producers preceding Samus's echo call.
/// One instance belongs to one frontend Step, never to the serialized game graph.
/// </summary>
internal sealed class GameplayAudioFramePublication(CartridgeAudioState audio)
{
    private int roomSounds, paletteSounds, paletteMusic, liquidSounds;
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
            audio.QueueSound(request.SoundEffect, request.MaximumQueued);
        }
        var palette = runtime.RoomPaletteFx.SoundRequests;
        while (paletteSounds < palette.Count)
        {
            var request = palette[paletteSounds++];
            audio.QueueSound(request.SoundEffect, request.MaximumQueued);
        }
        var music = runtime.RoomPaletteFx.MusicRequests;
        while (paletteMusic < music.Count)
        {
            var request = music[paletteMusic++];
            audio.QueueMusicDelayed(request.Command, request.Delay);
        }
        if (runtime.Samus is not { } samus)
            return;
        if (!selectionPublished && runtime.Hud.SelectionSoundRequestedThisFrame)
        {
            audio.QueueSound(SoundEffectLibrary1Sounds.HudWeaponSelect, 6);
            selectionPublished = true;
        }
        var liquid = samus.LiquidPhysics.SoundRequests;
        while (liquidSounds < liquid.Count)
        {
            var request = liquid[liquidSounds++];
            audio.QueueSound(request.SoundEffect, request.MaximumQueued);
        }
    }

    /// <summary>Executes the native Max6 sound call, preserving its accumulator result.</summary>
    public ushort QueueEcho(SuperMetroidRuntime runtime)
    {
        PublishPrefix(runtime);
        return audio.QueueSoundAndGetAccumulator(
            SoundEffectLibrary3Sounds.SpeedBoosterEcho, 6,
            soundSuppressed: runtime.IsAttractDemo ||
                unchecked((short)runtime.PowerBombExplosionStatus) < 0);
    }
}
