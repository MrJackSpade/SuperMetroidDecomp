namespace SuperMetroid.Core.Frontend.Title;

/// <summary>Debugger-facing names for the visible title-sequence functions.</summary>
public enum TitleSequencePhase
{
    /// <summary>Runs the $8B:A03D progressive 1994 title card over black, including its initial blank hold, before triggering the lower-background pan.</summary>
    YearText,
    /// <summary>Ports $8B:9D17: pans the lower Mode 7 scene left by 1.5 pixels per update until X falls below -7 pixels, then starts the Nintendo card.</summary>
    SceneZeroPan,
    /// <summary>Runs the $8B:A055 progressive Nintendo title card with the Mode 7 background hidden, then triggers the upper-background pan.</summary>
    NintendoText,
    /// <summary>Ports $8B:9D90: pans the upper Mode 7 scene left by 1.5 pixels per update until X falls below -176 pixels, then starts the Presents card.</summary>
    SceneOnePan,
    /// <summary>Runs the $8B:A079 progressive Presents title card over black before triggering the downward-background pan.</summary>
    PresentsText,
    /// <summary>Ports $8B:9E12: pans the Mode 7 scene down by 1.5 pixels per update until Y reaches 163 pixels, then starts the Metroid 3 card.</summary>
    SceneTwoPan,
    /// <summary>Runs the $8B:A09D progressive Metroid 3 title card, including its 120-update final hold, before triggering the final zoom.</summary>
    MetroidThreeText,
    /// <summary>Ports $8B:9E8B: enables the title gradient and zooms out by increasing the native 8.8 Mode 7 scale from $0043 to unity $0100 on alternating updates.</summary>
    SceneThreeZoom,
    /// <summary>Displays the title logo at the completed Mode 7 transform for the 32-update $8B:A0C5 logo-list stage before showing the copyright.</summary>
    TitleLogoFade,
    /// <summary>Displays the logo and Nintendo copyright for the 32-update $8B:A0E1 copyright-list stage before starting the idle-title demo countdown.</summary>
    CopyrightFade,
    /// <summary>Ports the $8B:9A83 skip fade: lowers brightness by two levels per update, then installs the immediate title objects and queues track six at black.</summary>
    SkipFadeOut,
    /// <summary>Ports the $8B:9B53 immediate-title fade-in after skipping the opening or returning from a demo; raises brightness by two levels per update before starting the idle countdown.</summary>
    TitleScreenFadeIn,
    /// <summary>Ports $8B:9F29: waits up to 900 NTSC title updates for a new B, Start, or A press; countdown expiration takes precedence and selects a demo.</summary>
    TitleScreen,
    /// <summary>Combines the $8B:9F52 file-select and $8B:9FAE demo transitions, reducing brightness by one level every two updates and requesting the selected destination at black.</summary>
    TitleScreenFadeOut,
}
