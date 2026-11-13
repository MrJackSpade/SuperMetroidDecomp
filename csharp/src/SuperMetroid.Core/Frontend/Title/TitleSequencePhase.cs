namespace SuperMetroid.Core.Frontend.Title;

/// <summary>The visible title sequence's steps.</summary>
public enum TitleSequencePhase
{
    /// <summary>Runs the progressive 1994 title card over black, including its initial blank hold, before triggering the lower-background pan.</summary>
    YearText,
    /// <summary>Pans the lower turning-floor scene left by 1.5 pixels per update until X falls below -7 pixels, then starts the Nintendo card.</summary>
    SceneZeroPan,
    /// <summary>Runs the progressive Nintendo title card with the turning-floor background hidden, then triggers the upper-background pan.</summary>
    NintendoText,
    /// <summary>Pans the upper turning-floor scene left by 1.5 pixels per update until X falls below -176 pixels, then starts the Presents card.</summary>
    SceneOnePan,
    /// <summary>Runs the progressive Presents title card over black before triggering the downward-background pan.</summary>
    PresentsText,
    /// <summary>Pans the turning-floor scene down by 1.5 pixels per update until Y reaches 163 pixels, then starts the Metroid 3 card.</summary>
    SceneTwoPan,
    /// <summary>Runs the progressive Metroid 3 title card, including its 120-update final hold, before triggering the final zoom.</summary>
    MetroidThreeText,
    /// <summary>
    /// Enables the title gradient and zooms out by increasing the turning floor's zoom from 67/256 to its own size on
    /// alternating updates; the next alternating update after reaching its own size completes the zoom. The last step
    /// that accepts the skip.
    /// </summary>
    SceneThreeZoom,
    /// <summary>Displays the title logo at the completed turning-floor transform for the 32-update logo stage before showing the copyright; refuses the skip.</summary>
    TitleLogoFade,
    /// <summary>Displays the logo and Nintendo copyright for the 32-update copyright stage before starting the idle-title demo countdown; refuses the skip.</summary>
    CopyrightFade,
    /// <summary>
    /// The immediate-title fade-in after the skip's rebuild of the title objects or the return from a demo; raises brightness
    /// by two levels per update before starting the idle countdown. The skip's fade-out is not a step: it runs alongside the
    /// opening's running step (#1719).
    /// </summary>
    TitleScreenFadeIn,
    /// <summary>Waits up to 900 NTSC title updates for a new B, Start, or A press; countdown expiration takes precedence and selects a demo.</summary>
    TitleScreen,
    /// <summary>Combines the file-select and demo transitions, reducing brightness by one level every two updates and requesting the selected destination at black.</summary>
    TitleScreenFadeOut,
}
