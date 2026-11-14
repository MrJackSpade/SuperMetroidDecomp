namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>The game-over menu's fade, music, sound and Baby timing.</summary>
public static class GameOverMenuDefinitions
{
    /// <summary>The full screen brightness; the fade-in enters the interactive menu on reaching it.</summary>
    public const int MaximumBrightness = 15;

    /// <summary>The priority of the selection missile's sound and all three Baby cries: normal.</summary>
    public static readonly SoundRequestPriority MaximumQueuedSounds = SoundRequestPriority.Normal;

    /// <summary>The statue-hall song, requested once the initial music queue drains.</summary>
    public const MusicTrack MenuSong = MusicTrack.StatueHall;

    /// <summary>Menu updates the current Baby frame is held for once either answer is accepted, holding it through the fade-out.</summary>
    public const int AcceptedAnswerHoldDuration = 180;

    /// <summary>The cry queued after the first cry group's first frame.</summary>
    public static readonly SoundEffectId FirstCry = RoomSound.BabyMetroidCry1;

    /// <summary>The cry queued after the second cry group's first frame.</summary>
    public static readonly SoundEffectId SecondCry = RoomSound.BabyMetroidCry2;

    /// <summary>The cry queued after the third cry group's first frame.</summary>
    public static readonly SoundEffectId ThirdCry = RoomSound.BabyMetroidCry3;
}
