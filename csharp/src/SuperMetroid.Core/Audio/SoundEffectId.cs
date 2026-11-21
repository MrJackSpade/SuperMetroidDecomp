namespace SuperMetroid.Core.Audio;

/// <summary>
/// One request a sound library's queue takes: a sound it plays, a member of <see cref="SamusSound"/>,
/// <see cref="EnemySound"/> or <see cref="RoomSound"/>, or a stop that cuts its sound off and plays nothing, a member of
/// <see cref="SamusSoundStop"/>, <see cref="EnemySoundStop"/> or <see cref="RoomSoundStop"/> (#2580), together with the
/// library whose queue takes it.
/// </summary>
/// <remarks>
/// The library is part of the identity because it decides which queue a request waits in and which sounds interrupt one
/// another. Each library's sounds and stops convert implicitly, so a request names the sound or stop itself. The identity
/// holds no number: the import pipeline alone maps a sound to the installed sound effect it requests. The identity is a
/// closed choice of the three libraries' sounds and stops (#2554), so every identity names one library's sound or stop.
/// </remarks>
public abstract record SoundEffectId
{
    /// <summary>Only the three libraries' sounds and stops below exist.</summary>
    private SoundEffectId() { }

    /// <summary>The library whose queue plays the sound.</summary>
    public abstract SoundEffectLibrary Library { get; }

    /// <summary>
    /// The request's installed key (#2520): its list's name, a dot and the sound's or stop's own key, as the clip index keys
    /// each request.
    /// </summary>
    [InstalledKey]
    internal abstract string Key { get; }

    /// <summary>A sound of Samus's library.</summary>
    public static implicit operator SoundEffectId(SamusSound sound) => new OfSamus(sound);

    /// <summary>A sound of the enemies' library.</summary>
    public static implicit operator SoundEffectId(EnemySound sound) => new OfEnemies(sound);

    /// <summary>A sound of the room's library.</summary>
    public static implicit operator SoundEffectId(RoomSound sound) => new OfRoom(sound);

    /// <summary>A stop of Samus's library.</summary>
    public static implicit operator SoundEffectId(SamusSoundStop stop) => new OfSamusStop(stop);

    /// <summary>A stop of the enemies' library.</summary>
    public static implicit operator SoundEffectId(EnemySoundStop stop) => new OfEnemiesStop(stop);

    /// <summary>A stop of the room's library.</summary>
    public static implicit operator SoundEffectId(RoomSoundStop stop) => new OfRoomStop(stop);

    /// <inheritdoc />
    public sealed override string ToString() => Key;

    /// <summary>A sound of Samus's library.</summary>
    /// <param name="Sound">Samus's sound.</param>
    public sealed record OfSamus(SamusSound Sound) : SoundEffectId
    {
        /// <inheritdoc />
        public override SoundEffectLibrary Library => SoundEffectLibrary.Samus;

        /// <inheritdoc />
        [InstalledKey]
        internal override string Key => "SamusSound." + SamusSoundKeys.Installed.Of(Sound);
    }

    /// <summary>A sound of the enemies' library.</summary>
    /// <param name="Sound">The enemies' sound.</param>
    public sealed record OfEnemies(EnemySound Sound) : SoundEffectId
    {
        /// <inheritdoc />
        public override SoundEffectLibrary Library => SoundEffectLibrary.Enemies;

        /// <inheritdoc />
        [InstalledKey]
        internal override string Key => "EnemySound." + EnemySoundKeys.Installed.Of(Sound);
    }

    /// <summary>A sound of the room's library.</summary>
    /// <param name="Sound">The room's sound.</param>
    public sealed record OfRoom(RoomSound Sound) : SoundEffectId
    {
        /// <inheritdoc />
        public override SoundEffectLibrary Library => SoundEffectLibrary.Room;

        /// <inheritdoc />
        [InstalledKey]
        internal override string Key => "RoomSound." + RoomSoundKeys.Installed.Of(Sound);
    }

    /// <summary>A stop of Samus's library.</summary>
    /// <param name="Stop">Samus's stop.</param>
    public sealed record OfSamusStop(SamusSoundStop Stop) : SoundEffectId
    {
        /// <inheritdoc />
        public override SoundEffectLibrary Library => SoundEffectLibrary.Samus;

        /// <inheritdoc />
        [InstalledKey]
        internal override string Key => "SamusSoundStop." + SamusSoundStopKeys.Installed.Of(Stop);
    }

    /// <summary>A stop of the enemies' library.</summary>
    /// <param name="Stop">The enemies' stop.</param>
    public sealed record OfEnemiesStop(EnemySoundStop Stop) : SoundEffectId
    {
        /// <inheritdoc />
        public override SoundEffectLibrary Library => SoundEffectLibrary.Enemies;

        /// <inheritdoc />
        [InstalledKey]
        internal override string Key => "EnemySoundStop." + EnemySoundStopKeys.Installed.Of(Stop);
    }

    /// <summary>A stop of the room's library.</summary>
    /// <param name="Stop">The room's stop.</param>
    public sealed record OfRoomStop(RoomSoundStop Stop) : SoundEffectId
    {
        /// <inheritdoc />
        public override SoundEffectLibrary Library => SoundEffectLibrary.Room;

        /// <inheritdoc />
        [InstalledKey]
        internal override string Key => "RoomSoundStop." + RoomSoundStopKeys.Installed.Of(Stop);
    }
}
