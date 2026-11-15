namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound effect the game plays: a member of <see cref="SamusSound"/>, <see cref="EnemySound"/> or
/// <see cref="RoomSound"/>, together with the library whose queue plays it.
/// </summary>
/// <remarks>
/// The library is part of the identity because it decides which queue a request waits in and which sounds interrupt one
/// another. Each library's sounds convert implicitly, so a request names the sound itself. The identity holds no number:
/// the import pipeline alone maps a sound to the installed sound effect it requests. The default identity holds no sound
/// of any library, and asking it for its library fails (#1897).
/// </remarks>
public readonly record struct SoundEffectId
{
    /// <summary>Samus's sound, or null when the sound is of another library.</summary>
    private readonly SamusSound? samus;
    /// <summary>The enemies' sound, or null when the sound is of another library.</summary>
    private readonly EnemySound? enemies;
    /// <summary>The room's sound, or null when the sound is of another library.</summary>
    private readonly RoomSound? room;

    /// <summary>Records one library's sound; the other libraries' sounds are null.</summary>
    private SoundEffectId(SamusSound? samus, EnemySound? enemies, RoomSound? room)
    {
        this.samus = samus;
        this.enemies = enemies;
        this.room = room;
    }

    /// <summary>The library whose queue plays the sound.</summary>
    /// <exception cref="InvalidOperationException">This is the default identity, which holds no sound.</exception>
    public SoundEffectLibrary Library =>
        samus is not null ? SoundEffectLibrary.Samus
        : enemies is not null ? SoundEffectLibrary.Enemies
        : room is not null ? SoundEffectLibrary.Room
        : throw new InvalidOperationException("The default sound identity names no sound.");

    /// <summary>A sound of Samus's library.</summary>
    public static implicit operator SoundEffectId(SamusSound sound) => new(sound, null, null);

    /// <summary>A sound of the enemies' library.</summary>
    public static implicit operator SoundEffectId(EnemySound sound) => new(null, sound, null);

    /// <summary>A sound of the room's library.</summary>
    public static implicit operator SoundEffectId(RoomSound sound) => new(null, null, sound);

    /// <inheritdoc />
    public override string ToString() =>
        samus is { } samusSound ? $"{nameof(SamusSound)}.{samusSound}"
        : enemies is { } enemySound ? $"{nameof(EnemySound)}.{enemySound}"
        : room is { } roomSound ? $"{nameof(RoomSound)}.{roomSound}"
        : throw new InvalidOperationException("The default sound identity names no sound.");
}
