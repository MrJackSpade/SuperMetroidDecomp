namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending sequence's setup wait, music, reward selection and authored hold durations (#1416).</summary>
public static class EndingSequenceDefinitions
{
    /// <summary>The frames the ending's setup waits before its scene setup: nine.</summary>
    public const int SetupFrameWaits = 9;

    /// <summary>The music the escape and ending sequences queue at their boundaries.</summary>
    public static class Music
    {
        /// <summary>The song the escape selects once Zebes's explosion music has loaded.</summary>
        public const MusicTrack EscapeSong = MusicTrack.ZebesExplosion;

        /// <summary>The song the ending selects once the credits' music has loaded.</summary>
        public const MusicTrack CreditsSong = MusicTrack.Credits;

        /// <summary>The delayed-music argument fourteen, which the music queue's delay conversion reads rather than counting frames.</summary>
        public const int DelayArgument = 14;
    }

    /// <summary>Whole-hour thresholds selecting the suitless, helmetless or fully armored Samus reward.</summary>
    public static class Rewards
    {
        /// <summary>Exclusive whole-hour cutoff for the suitless reward: recorded game time below three hours.</summary>
        public const int SuitlessMaximumHoursExclusive = 3;

        /// <summary>Exclusive whole-hour cutoff for the helmetless suited reward: at least three but fewer than ten hours; ten or more keeps the helmet.</summary>
        public const int HelmetlessMaximumHoursExclusive = 10;
    }

    /// <summary>Authored reveal, palette-fade and panel-hold durations, in cinematic updates.</summary>
    public static class Timing
    {
        /// <summary>The reward reveal pauses after its counter runs from 127 to 63; the remaining 64 calls follow later.</summary>
        public const int RewardRevealHalfFrames = 64;

        /// <summary>The copyright panel's hold.</summary>
        public const int CopyrightHoldFrames = 180;

        /// <summary>The waiting palette fade's steps: 32 additions of target-component / 32 in 8.8 precision.</summary>
        public const int WaitingPaletteFadeFrames = 32;

        /// <summary>The waiting backdrop's hold.</summary>
        public const int WaitingBackdropFrames = 180;

        /// <summary>Pixels the item-percentage text moves down the screen on each call of its forty-call move down.</summary>
        public const int TextDropPixelsPerCall = 2;
    }
}
