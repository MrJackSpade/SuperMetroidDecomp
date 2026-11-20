namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound effect the game plays: a member of <see cref="SamusSound"/>, <see cref="EnemySound"/> or
/// <see cref="RoomSound"/>, together with the library whose queue plays it.
/// </summary>
/// <remarks>
/// The library is part of the identity because it decides which queue a request waits in and which sounds interrupt one
/// another. Each library's sounds convert implicitly, so a request names the sound itself. The identity holds no number:
/// the import pipeline alone maps a sound to the installed sound effect it requests. The identity is a closed choice of
/// the three libraries' sounds (#2554), so every identity names one library's sound.
/// </remarks>
public abstract record SoundEffectId
{
    /// <summary>Only the three libraries' sounds below exist.</summary>
    private SoundEffectId() { }

    /// <summary>The library whose queue plays the sound.</summary>
    public abstract SoundEffectLibrary Library { get; }

    /// <summary>
    /// The sound's installed key (#2520): its library's name, a dot and the sound's own key, as the clip index keys each
    /// sound.
    /// </summary>
    [InstalledKey]
    internal abstract string Key { get; }

    /// <summary>A sound of Samus's library.</summary>
    public static implicit operator SoundEffectId(SamusSound sound) => new OfSamus(sound);

    /// <summary>A sound of the enemies' library.</summary>
    public static implicit operator SoundEffectId(EnemySound sound) => new OfEnemies(sound);

    /// <summary>A sound of the room's library.</summary>
    public static implicit operator SoundEffectId(RoomSound sound) => new OfRoom(sound);

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
}
