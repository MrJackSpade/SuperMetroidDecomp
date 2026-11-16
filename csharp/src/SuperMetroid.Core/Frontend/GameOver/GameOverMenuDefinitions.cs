namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>The game-over menu's fade, music, sound and Baby timing.</summary>
public static class GameOverMenuDefinitions
{
    /// <summary>
    /// How crowded the sound queue may be for the menu's sounds to join it: the silence it opens with, the selection
    /// missile's move and all three Baby cries are requested at the usual priority.
    /// </summary>
    public static readonly SoundRequestPriority SoundPriority = SoundRequestPriority.Normal;

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
