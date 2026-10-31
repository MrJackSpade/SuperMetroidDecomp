namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>The game-over menu's fade, music, sound and Baby timing.</summary>
public static class GameOverMenuDefinitions
{
    /// <summary>The full screen brightness; the fade-in enters the interactive menu on reaching it.</summary>
    public const int MaximumBrightness = 15;

    /// <summary>The queue policy of the selection missile's sound and all three Baby cries: at most six queued.</summary>
    public static readonly SoundRequestPriority MaximumQueuedSounds = SoundRequestPriority.Normal;

    /// <summary>The pre-statue-hall track of the title music set, requested once the initial music queue drains.</summary>
    public const int MusicTrack = 4;

    /// <summary>Menu updates the current Baby frame is held for once either answer is accepted, holding it through the fade-out.</summary>
    public const int AcceptedAnswerHoldDuration = 180;

    /// <summary>The colours in each Baby palette phase.</summary>
    public const int BabyPaletteColorCount = 16;

    /// <summary>The cry queued after the first cry group's first frame.</summary>
    public static readonly SoundEffectId FirstCry = Library3Sound.BabyMetroidCry1;

    /// <summary>The cry queued after the second cry group's first frame.</summary>
    public static readonly SoundEffectId SecondCry = Library3Sound.BabyMetroidCry2;

    /// <summary>The cry queued after the third cry group's first frame.</summary>
    public static readonly SoundEffectId ThirdCry = Library3Sound.BabyMetroidCry3;
}
