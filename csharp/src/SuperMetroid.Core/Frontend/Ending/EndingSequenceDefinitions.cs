namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending sequence's setup wait, music, reward selection and authored hold durations (#1416).</summary>
public static class EndingSequenceDefinitions
{
    /// <summary>The frames the ending's setup waits before its scene setup.</summary>
    public const int SetupFrameWaits = 9;

    /// <summary>The music the escape and ending sequences queue at their boundaries.</summary>
    public static class Music
    {
        /// <summary>The song the escape selects once Zebes's explosion music has loaded.</summary>
        public const MusicTrack EscapeSong = MusicTrack.ZebesExplosion;

        /// <summary>The song the ending selects once the credits' music has loaded.</summary>
        public const MusicTrack CreditsSong = MusicTrack.Credits;

        /// <summary>How long the credits' song waits on the music queue, in passes.</summary>
        public static MusicQueueDelay CreditsSongDelay { get; } = new(14);
    }

    /// <summary>Whole-hour thresholds selecting the suitless, helmetless or fully armored Samus reward.</summary>
    public static class Rewards
    {
        /// <summary>Exclusive whole-hour cutoff for the suitless reward: recorded game time below it.</summary>
        public const int SuitlessMaximumHoursExclusive = 3;

        /// <summary>Exclusive whole-hour cutoff for the helmetless suited reward: at least <see cref="SuitlessMaximumHoursExclusive"/> but below this; this or more keeps the helmet.</summary>
        public const int HelmetlessMaximumHoursExclusive = 10;
    }

    /// <summary>Authored reveal, colour-fade and panel-hold durations, in cinematic updates.</summary>
    public static class Timing
    {
        /// <summary>Each half of the reward reveal's updates: it pauses after the first half, and the second follows later.</summary>
        public const int RewardRevealHalfFrames = 64;

        /// <summary>The copyright panel's hold.</summary>
        public const int CopyrightHoldFrames = 180;

        /// <summary>The blank screen's hold as the post-credits scenes start.</summary>
        public const int PostCreditsBlankFrames = 60;

        /// <summary>The complete result panel's hold before the waiting Samus.</summary>
        public const int ResultPanelHoldFrames = 180;

        /// <summary>The updates the item-percentage text moves down the screen for.</summary>
        public const int ItemPercentageDropUpdates = 40;

        /// <summary>The explosion's whiteout holds this long, waiting for the planet escape's music, before the flyaway.</summary>
        public const int PlanetEscapeMusicWaitFrames = 120;

        /// <summary>The waiting backdrop fade's steps: it brightens to its colours in equal steps.</summary>
        public const int WaitingBackdropFadeSteps = 32;

        /// <summary>The waiting backdrop's hold.</summary>
        public const int WaitingBackdropFrames = 180;

        /// <summary>Pixels the item-percentage text moves down the screen on each update of its move down (<see cref="ItemPercentageDropUpdates"/>).</summary>
        public const int TextDropPixelsPerUpdate = 2;
    }
}
