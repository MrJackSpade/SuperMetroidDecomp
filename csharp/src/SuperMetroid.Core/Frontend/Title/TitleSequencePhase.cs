namespace SuperMetroid.Core.Frontend.Title;

/// <summary>The visible title sequence's steps.</summary>
public enum TitleSequencePhase
{
    /// <summary>Runs the progressive 1994 title card over black, including its initial blank hold, before triggering the lower-background pan.</summary>
    YearText,
    /// <summary>Pans the lower Mode 7 scene left by 1.5 pixels per update until X falls below -7 pixels, then starts the Nintendo card.</summary>
    SceneZeroPan,
    /// <summary>Runs the progressive Nintendo title card with the Mode 7 background hidden, then triggers the upper-background pan.</summary>
    NintendoText,
    /// <summary>Pans the upper Mode 7 scene left by 1.5 pixels per update until X falls below -176 pixels, then starts the Presents card.</summary>
    SceneOnePan,
    /// <summary>Runs the progressive Presents title card over black before triggering the downward-background pan.</summary>
    PresentsText,
    /// <summary>Pans the Mode 7 scene down by 1.5 pixels per update until Y reaches 163 pixels, then starts the Metroid 3 card.</summary>
    SceneTwoPan,
    /// <summary>Runs the progressive Metroid 3 title card, including its 120-update final hold, before triggering the final zoom.</summary>
    MetroidThreeText,
    /// <summary>Enables the title gradient and zooms out by increasing the 8.8 Mode 7 scale from 67/256 to unity on alternating updates.</summary>
    SceneThreeZoom,
    /// <summary>Displays the title logo at the completed Mode 7 transform for the 32-update logo stage before showing the copyright.</summary>
    TitleLogoFade,
    /// <summary>Displays the logo and Nintendo copyright for the 32-update copyright stage before starting the idle-title demo countdown.</summary>
    CopyrightFade,
    /// <summary>The skip fade: lowers brightness by two levels per update, then installs the immediate title objects and queues track six at black.</summary>
    SkipFadeOut,
    /// <summary>The immediate-title fade-in after skipping the opening or returning from a demo; raises brightness by two levels per update before starting the idle countdown.</summary>
    TitleScreenFadeIn,
    /// <summary>Waits up to 900 NTSC title updates for a new B, Start, or A press; countdown expiration takes precedence and selects a demo.</summary>
    TitleScreen,
    /// <summary>Combines the file-select and demo transitions, reducing brightness by one level every two updates and requesting the selected destination at black.</summary>
    TitleScreenFadeOut,
}
