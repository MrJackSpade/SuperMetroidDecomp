namespace SuperMetroid.Core.Game;

/// <summary>
/// One call to one of the cartridge's three <c>QueueSfx</c> entry points.
/// </summary>
/// <param name="Library">The exclusive cartridge SFX queue/input-port identity.</param>
/// <param name="SoundId">The low-byte sound index supplied by the original 65816 caller.</param>
/// <param name="MaximumQueued">
/// The exact queue-cap variant selected by that caller (for example Max3 or Max6).
/// </param>
public readonly record struct EnemySoundRequest(
    SoundEffectLibrary Library,
    byte SoundId,
    byte MaximumQueued);

/// <summary>One native enemy-owned <c>QueueMusic_Delayed*</c> publication.</summary>
/// <param name="Entry">
/// Raw bank-$80 music entry. The high byte is deliberately retained because values such
/// as <c>$FFxx</c> are data-upload commands rather than ordinary track numbers.
/// </param>
/// <param name="DelayFrames">The caller-selected delay before bank $80 handles the entry.</param>
public readonly record struct EnemyMusicRequest(
    ushort Entry,
    ushort DelayFrames);

public sealed partial class RoomEnemySystem
{
    // Enemy AI can execute several independent actors in one frame. A single nullable
    // "last sound" loses calls when two actors publish during that pass, so new translations
    // use the same append-only-per-frame shape already proven by Samus and PLM audio.
    private readonly List<EnemySoundRequest> _soundRequests = [];
    private readonly List<EnemyMusicRequest> _musicRequests = [];

    /// <summary>
    /// Exact bank-$A0/$A6/$A9 sound-queue calls published by the current enemy frame.
    /// The frontend drains these only after all enemy AI and draw instructions have run.
    /// </summary>
    public IReadOnlyList<EnemySoundRequest> SoundRequests => _soundRequests;

    /// <summary>Exact music-queue calls published by the current enemy frame.</summary>
    public IReadOnlyList<EnemyMusicRequest> MusicRequests => _musicRequests;

    /// <summary>Begins the native EnemyMain publication window.</summary>
    private void BeginEnemySoundRequestFrame()
    {
        _soundRequests.Clear();
        _musicRequests.Clear();

        // The oldest boss translations exposed nullable debugger fields instead of the
        // append-only queue above. Clear those fields at EnemyMain's frame boundary just
        // as their native QueueSfx/QueueMusic calls are momentary publications. Doing the
        // reset here prevents a paused/frozen actor from replaying its previous request.
        if (_kraidState is not null)
            _kraidState.MusicRequest = null;
        if (_draygon is not null)
        {
            _draygon.LastSoundLibrary2 = null;
            _draygon.LastSoundLibrary3 = null;
            _draygon.MusicRequest = null;
        }
        if (_phantoonState is not null)
        {
            _phantoonState.LastMaterializationSound = null;
            _phantoonState.LastCombatSoundEffect = null;
            _phantoonState.MusicRequest = null;
        }
    }

    /// <summary>
    /// Records an already-decoded cartridge queue call without assigning host-side meaning
    /// to its numeric sound ID. The bank-$80 queue remains the sole owner of arbitration.
    /// </summary>
    private void QueueEnemySound(
        SoundEffectLibrary library,
        ushort soundId,
        byte maximumQueued)
    {
        _ = SoundEffectLibraries.ToQueueIndex(library);
        if (soundId > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(soundId), soundId, "SPC sound ID must fit in one byte.");
        if (maximumQueued == 0)
            throw new ArgumentOutOfRangeException(nameof(maximumQueued), maximumQueued, "Queue capacity must be nonzero.");

        _soundRequests.Add(new EnemySoundRequest(
            library,
            unchecked((byte)soundId),
            maximumQueued));
    }

    /// <summary>
    /// Converts the earlier one-value-per-family debugger seams into audible bank-$80
    /// requests after all active actors have run. The library and queue limits below come
    /// from the corresponding retail <c>QueueSfxN_MaxM</c> call, not from sound-ID meaning.
    /// </summary>
    /// <remarks>
    /// New translations should call <see cref="QueueEnemySound"/> at the translated call
    /// site. This adapter exists so the already-large enemy translation can become audible
    /// without rewriting unrelated, verified AI in one risky mechanical pass. A legacy
    /// nullable field can retain only its final same-family call in a frame; that historical
    /// debugger limitation cannot be recovered here. Distinct families still append distinct
    /// requests, and every new/direct producer is lossless.
    /// </remarks>
    private void CollectLegacyEnemyAudioRequests()
    {
        // Ordinary bank-$A2/$A3/$A8/$B2 actors overwhelmingly call QueueSfx2_Max6.
        QueueLegacySound(LastBoyonSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastMamaTurtleSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastCacatacSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastMochtroidSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastBoulderSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastEtecoonSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastDachoraSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastEvirSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastMorphBallEyeSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastYappingMawSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastHopperSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastMetareeSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastAlcoonSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastFuneNamiheSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastKagoBugSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastKzanSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastHibashiSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastNuclearWaffleSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastFakeKraidSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastChozoStatueSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastWorkRobotSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastSpacePirateSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastRioSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastNorfairLavaJumpingEnemySoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastNorfairRioSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastLowerNorfairRioSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastMaridiaLargeSnailSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastDragonSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastKiHunterSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastMagdolliteSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastShutterSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);

        // These actors deliberately use a different library or the tighter Max3 entry.
        QueueLegacySound(LastYardSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 3);
        QueueLegacySound(LastBeetomSoundEffect, library: SoundEffectLibrary.Library3, maximumQueued: 6);
        QueueLegacySound(LastZebetiteSoundEffect, library: SoundEffectLibrary.Library3, maximumQueued: 6);
        QueueLegacySound(LastMetroidSoundEffectLibrary2, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastMetroidSoundEffectLibrary3, library: SoundEffectLibrary.Library3, maximumQueued: 6);

        // Bank-$86 pickup/death opcodes and the common rejected-shot path have explicit,
        // smaller admission limits. Their fields are shared by several projectile actors.
        QueueLegacySound(LastEnemyPickupSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 1);
        QueueLegacySound(LastEnemyDeathSoundEffectLibrary2, library: SoundEffectLibrary.Library2, maximumQueued: 1);
        QueueLegacySound(LastEnemyProjectileDudSoundEffect, library: SoundEffectLibrary.Library1, maximumQueued: 3);

        // Encounter-specific banks. Ceres doors, elevators, Skree, and Bomb Torizo sound
        // opcodes already publish directly and are intentionally absent to avoid duplicates.
        QueueLegacySound(LastBotwoonSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastCrocomireSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastDeadTorizoSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastDeadSidehopperSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 3);
        QueueLegacySound(LastShitroidSoundEffectLibrary2, library: SoundEffectLibrary.Library2, maximumQueued: 6);
        QueueLegacySound(LastSporeSpawnSoundEffectLibrary2, library: SoundEffectLibrary.Library2, maximumQueued: 6);

        if (LastKraidSoundEffect is { } kraidSound)
            QueueEnemySound(kraidSound.Library, kraidSound.SoundEffect, maximumQueued: 6);
        if (_ridleyState?.LastDeathSoundEffect is ushort ridleyDeath)
            QueueEnemySound(library: SoundEffectLibrary.Library2, ridleyDeath, maximumQueued: 3);
        if (_draygon?.LastSoundLibrary2 is ushort draygonLibrary2)
            QueueEnemySound(library: SoundEffectLibrary.Library2, draygonLibrary2, maximumQueued: 6);
        if (_draygon?.LastSoundLibrary3 is ushort draygonLibrary3)
            QueueEnemySound(library: SoundEffectLibrary.Library3, draygonLibrary3, maximumQueued: 6);
        if (_phantoonState?.LastMaterializationSound is ushort phantoonMaterialization)
            QueueEnemySound(library: SoundEffectLibrary.Library2, phantoonMaterialization, maximumQueued: 6);
        if (_phantoonState?.LastCombatSoundEffect is ushort phantoonCombat)
            QueueEnemySound(library: SoundEffectLibrary.Library2, phantoonCombat, maximumQueued: 6);

        if (_motherBrain?.LastSoundEffect is ushort motherBrainLibrary2)
            QueueEnemySound(library: SoundEffectLibrary.Library2, motherBrainLibrary2, maximumQueued: 6);
        if (_motherBrain?.LastSoundEffectLibrary1 is ushort motherBrainLibrary1)
            QueueEnemySound(library: SoundEffectLibrary.Library1, motherBrainLibrary1, maximumQueued: 6);
        if (_motherBrain?.LastSoundEffectLibrary3 is ushort motherBrainLibrary3)
        {
            // Explosion instruction $13 is QueueSfx3_Max3; the other translated library-
            // three Mother Brain instructions use Max6.
            byte maximumQueued = motherBrainLibrary3 == 0x13 ? (byte)3 : (byte)6;
            QueueEnemySound(library: SoundEffectLibrary.Library3, motherBrainLibrary3, maximumQueued);
        }

        QueueLegacyMusic(LastBotwoonMusicRequest?.Track, LastBotwoonMusicRequest?.DelayFrames);
        QueueLegacyMusic(LastBombTorizoMusicRequest?.Track, LastBombTorizoMusicRequest?.DelayFrames);
        QueueLegacyMusic(LastCrocomireMusicRequest?.Track, LastCrocomireMusicRequest?.DelayFrames);
        QueueLegacyMusic(_kraidState?.MusicRequest, delayFrames: 8);
        QueueLegacyMusic(_draygon?.MusicRequest, delayFrames: 8);
        QueueLegacyMusic(_phantoonState?.MusicRequest, delayFrames: 8);
        QueueLegacyMusic(_ridleyState?.MusicRequest, delayFrames: 8);
        QueueLegacyMusic(LastShitroidMusicRequest?.Track, LastShitroidMusicRequest?.DelayFrames);
        if (_motherBrain is not null)
        {
            foreach (MotherBrainMusicRequest request in _motherBrain.MusicRequests)
                _musicRequests.Add(new EnemyMusicRequest(request.RawTrack, request.DelayFrames));
        }
    }

    private void QueueLegacySound(
        ushort? soundId,
        SoundEffectLibrary library,
        byte maximumQueued)
    {
        if (soundId is ushort value)
            QueueEnemySound(library, value, maximumQueued);
    }

    private void QueueLegacyMusic(ushort? entry, ushort? delayFrames)
    {
        if (entry is ushort value && delayFrames is ushort delay)
            _musicRequests.Add(new EnemyMusicRequest(value, delay));
    }
}
