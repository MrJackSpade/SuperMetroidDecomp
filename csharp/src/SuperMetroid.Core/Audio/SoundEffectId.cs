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
/// holds no number:
/// the import pipeline alone maps a sound to the installed sound effect it requests. Which library's sound it is is one
/// closed value (#2560), so no identity names sounds of two libraries. The default identity holds no sound of any library,
/// and asking it for its library fails (#1897).
/// </remarks>
public readonly record struct SoundEffectId
{
    /// <summary>The library's sound; null only in the default identity, which holds none.</summary>
    private readonly LibrarySound? sound;

    /// <summary>Records one library's sound.</summary>
    private SoundEffectId(LibrarySound sound) => this.sound = sound;

    /// <summary>The library whose queue plays the sound.</summary>
    /// <exception cref="InvalidOperationException">This is the default identity, which holds no sound.</exception>
    public SoundEffectLibrary Library => Sound.Library;

    /// <summary>A sound of Samus's library.</summary>
    public static implicit operator SoundEffectId(SamusSound sound) => new(new SamusLibrarySound(sound));

    /// <summary>A sound of the enemies' library.</summary>
    public static implicit operator SoundEffectId(EnemySound sound) => new(new EnemyLibrarySound(sound));

    /// <summary>A sound of the room's library.</summary>
    public static implicit operator SoundEffectId(RoomSound sound) => new(new RoomLibrarySound(sound));

    /// <summary>A stop of Samus's library.</summary>
    public static implicit operator SoundEffectId(SamusSoundStop stop) => new(new SamusLibraryStop(stop));

    /// <summary>A stop of the enemies' library.</summary>
    public static implicit operator SoundEffectId(EnemySoundStop stop) => new(new EnemyLibraryStop(stop));

    /// <summary>A stop of the room's library.</summary>
    public static implicit operator SoundEffectId(RoomSoundStop stop) => new(new RoomLibraryStop(stop));

    /// <summary>
    /// The request's installed key (#2520): its list's name, a dot and the sound's or stop's own key, as the clip index keys
    /// each request.
    /// </summary>
    /// <exception cref="InvalidOperationException">This is the default identity, which holds no sound.</exception>
    [InstalledKey]
    internal string Key => Sound.Key;

    /// <summary>The library's sound.</summary>
    /// <exception cref="InvalidOperationException">This is the default identity, which holds no sound.</exception>
    private LibrarySound Sound => sound ?? throw new InvalidOperationException("The default sound identity names no sound.");

    /// <inheritdoc />
    public override string ToString() => Key;

    /// <summary>A sound or stop of one library (#2560): a closed choice with one case per library's sounds and per its stops.</summary>
    private abstract record LibrarySound
    {
        /// <summary>The library whose queue plays the sound.</summary>
        internal abstract SoundEffectLibrary Library { get; }

        /// <summary>The sound's installed key.</summary>
        internal abstract string Key { get; }
    }

    /// <summary>A sound of Samus's library.</summary>
    /// <param name="Sound">The sound.</param>
    private sealed record SamusLibrarySound(SamusSound Sound) : LibrarySound
    {
        /// <inheritdoc/>
        internal override SoundEffectLibrary Library => SoundEffectLibrary.Samus;

        /// <inheritdoc/>
        internal override string Key => "SamusSound." + SamusSoundKeys.Installed.Of(Sound);
    }

    /// <summary>A sound of the enemies' library.</summary>
    /// <param name="Sound">The sound.</param>
    private sealed record EnemyLibrarySound(EnemySound Sound) : LibrarySound
    {
        /// <inheritdoc/>
        internal override SoundEffectLibrary Library => SoundEffectLibrary.Enemies;

        /// <inheritdoc/>
        internal override string Key => "EnemySound." + EnemySoundKeys.Installed.Of(Sound);
    }

    /// <summary>A sound of the room's library.</summary>
    /// <param name="Sound">The sound.</param>
    private sealed record RoomLibrarySound(RoomSound Sound) : LibrarySound
    {
        /// <inheritdoc/>
        internal override SoundEffectLibrary Library => SoundEffectLibrary.Room;

        /// <inheritdoc/>
        internal override string Key => "RoomSound." + RoomSoundKeys.Installed.Of(Sound);
    }

    /// <summary>A stop of Samus's library.</summary>
    /// <param name="Stop">The stop.</param>
    private sealed record SamusLibraryStop(SamusSoundStop Stop) : LibrarySound
    {
        /// <inheritdoc/>
        internal override SoundEffectLibrary Library => SoundEffectLibrary.Samus;

        /// <inheritdoc/>
        internal override string Key => "SamusSoundStop." + SamusSoundStopKeys.Installed.Of(Stop);
    }

    /// <summary>A stop of the enemies' library.</summary>
    /// <param name="Stop">The stop.</param>
    private sealed record EnemyLibraryStop(EnemySoundStop Stop) : LibrarySound
    {
        /// <inheritdoc/>
        internal override SoundEffectLibrary Library => SoundEffectLibrary.Enemies;

        /// <inheritdoc/>
        internal override string Key => "EnemySoundStop." + EnemySoundStopKeys.Installed.Of(Stop);
    }

    /// <summary>A stop of the room's library.</summary>
    /// <param name="Stop">The stop.</param>
    private sealed record RoomLibraryStop(RoomSoundStop Stop) : LibrarySound
    {
        /// <inheritdoc/>
        internal override SoundEffectLibrary Library => SoundEffectLibrary.Room;

        /// <inheritdoc/>
        internal override string Key => "RoomSoundStop." + RoomSoundStopKeys.Installed.Of(Stop);
    }
}
