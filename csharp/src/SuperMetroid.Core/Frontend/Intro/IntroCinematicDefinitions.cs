using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The opening cinematic's shared definitions: its text layout, crossfade timing, music, actors' sounds, and the narration caret.</summary>
public static class IntroCinematicDefinitions
{
    /// <summary>The timing of the repeating crossfades between the narration and the flashback scenes.</summary>
    public static class Crossfade
    {
        /// <summary>The updates a crossfade lasts, its last included.</summary>
        public const int Updates = 128;
    }

    /// <summary>
    /// The old Mother Brain fight's blinking background lights (#1733), started as a crossfade into a flashback ends while
    /// Samus draws first, and ended on the update the cinematic starts page two.
    /// </summary>
    public const ColourEffectId FightLights = ColourEffectId.OldMotherBrainFightBackgroundLights;

    /// <summary>The music the opening's scene changes start.</summary>
    public static class Music
    {
        /// <summary>The song the opening's first spoken line plays once its music set has loaded.</summary>
        public const MusicTrack CaptivityLineSong = MusicTrack.LastMetroidInCaptivity;
        /// <summary>The song the opening's second spoken line plays once its music set has loaded.</summary>
        public const MusicTrack PeaceLineSong = MusicTrack.GalaxyAtPeace;
        /// <summary>The song the illustrated pages play once the introduction's music set has loaded.</summary>
        public const MusicTrack IntroductionSong = MusicTrack.Introduction;
        /// <summary>How long the second line's and the pages' songs wait on the music queue: fourteen passes.</summary>
        public static MusicQueueDelay SceneTrackDelay { get; } = new(14);
    }

    /// <summary>Screen-fade cadences of the opening's scene changes.</summary>
    public static class Fade
    {
        /// <summary>Updates each step of the narration's fades takes: one brightness level every two updates.</summary>
        public const int NarrationUpdatesPerStep = 2;
        /// <summary>Updates each step of the final fade takes: one brightness level every second update.</summary>
        public const int FinishUpdatesPerStep = 2;
    }

    /// <summary>The narration's holds, in updates.</summary>
    public static class Timing
    {
        /// <summary>The first line's hold after it fades in, before the captivity line's song.</summary>
        public const int FirstLineHold = 60;
        /// <summary>The captivity line's hold before the galaxy-at-peace line's music.</summary>
        public const int CaptivityLineHold = 200;
        /// <summary>The four-second hold after the galaxy-at-peace line's music.</summary>
        public const int FourSecondHold = 240;
    }

    /// <summary>Samus as the flashback to her fight with Mother Brain sets her up.</summary>
    public static class Flashback
    {
        /// <summary>Where Samus stands, facing left, in Mother Brain's room.</summary>
        public static Position16<RoomPixels> SamusPosition => new(155, 115);
        /// <summary>The Missiles she carries, and her capacity for them.</summary>
        public const int Missiles = 900;
        /// <summary>
        /// The ground her footsteps and landings go by in the flashback, which loads no room (#2416): dry, so her
        /// footsteps raise dust only while she speed-boosts, and her landings raise nothing.
        /// </summary>
        internal static SamusGround Ground { get; } = new(SamusGroundWetness.Dry, DryLandingsRaiseDust: false);
    }

    /// <summary>The actors' sounds.</summary>
    public static class Sounds
    {
        /// <summary>
        /// How crowded the sound queue may be for the opening's sounds to join it: the Baby's cries, the hatching and the
        /// typewriter are requested at the usual priority.
        /// </summary>
        public static readonly SoundRequestPriority SoundPriority = SoundRequestPriority.Normal;
        /// <summary>The Baby Metroid's first cry.</summary>
        public static readonly SoundEffectId BabyCry1 = RoomSound.BabyMetroidCry1;
        /// <summary>The typewriter sound while narration characters appear.</summary>
        public static readonly SoundEffectId Typewriter = RoomSound.IntroTypewriter;
        /// <summary>The sound of the Baby Metroid hatching.</summary>
        public static readonly SoundEffectId EggHatch = EnemySound.ContactKill;
    }

    /// <summary>The narration caret's positions and the text characters' size.</summary>
    public static class NarrationLayout
    {
        /// <summary>The caret's leftmost column, in pixels.</summary>
        public const int CaretLeftX = 8;
        /// <summary>The caret's row on the first narration line.</summary>
        public const int CaretFirstTextY = 24;
        /// <summary>Width and height of one text character in pixels.</summary>
        public const int CharacterPixelSize = 8;
        /// <summary>Pixels from a text character's origin to its baseline.</summary>
        public const int CharacterBaselineOffset = 8;
        /// <summary>The Japanese text's next-page arrow's column, in pixels from the screen's left: the screen's middle (#1655).</summary>
        public const int NextPageArrowX = 128;
        /// <summary>The Japanese text's next-page arrow's row, in pixels from the screen's top: below the page's text (#1655).</summary>
        public const int NextPageArrowY = 216;
    }
}
