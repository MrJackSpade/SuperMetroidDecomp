namespace SuperMetroid.Core.Frontend.Title;

/// <summary>The visible title sequence's steps.</summary>
public enum TitleSequencePhase
{
    /// <summary>Runs the progressive 1994 title card over black, including its initial blank hold, before triggering the lower-background pan.</summary>
    YearText,
    /// <summary>Pans the lower turning-floor scene left at the pan speed until it passes the pan's end, then starts the Nintendo card.</summary>
    LowerPan,
    /// <summary>Runs the progressive Nintendo title card with the turning-floor background hidden, then triggers the upper-background pan.</summary>
    NintendoText,
    /// <summary>Pans the upper turning-floor scene left at the pan speed until it passes the pan's end, then starts the Presents card.</summary>
    UpperPan,
    /// <summary>Runs the progressive Presents title card over black before triggering the downward-background pan.</summary>
    PresentsText,
    /// <summary>Pans the turning-floor scene down at the pan speed until it reaches the pan's end, then starts the Metroid 3 card.</summary>
    DownwardPan,
    /// <summary>Runs the progressive Metroid 3 title card, including its final hold, before triggering the final zoom.</summary>
    MetroidThreeText,
    /// <summary>
    /// Enables the title gradient and zooms out by increasing the turning floor's zoom from the final zoom's to its own size on
    /// alternating updates; the next alternating update after reaching its own size completes the zoom. The last step
    /// that accepts the skip.
    /// </summary>
    FinalZoom,
    /// <summary>Displays the title logo at the completed turning-floor transform for the logo stage before showing the copyright; refuses the skip.</summary>
    TitleLogoFade,
    /// <summary>Displays the logo and Nintendo copyright for the copyright stage before starting the idle-title demo countdown; refuses the skip.</summary>
    CopyrightFade,
    /// <summary>
    /// The immediate-title fade-in after the skip's rebuild of the title screen or the return from a demo; raises brightness
    /// by its fast step each update before starting the idle countdown. The skip's fade-out is not a step: it runs alongside the
    /// opening's running step (#1719).
    /// </summary>
    TitleScreenFadeIn,
    /// <summary>Waits out the idle countdown for a new B, Start, or A press; countdown expiration takes precedence and selects a demo.</summary>
    TitleScreen,
    /// <summary>The confirmation's fade to file select, a brightness level at a time; at black it requests file select.</summary>
    TitleScreenFadeOutToFileSelect,
    /// <summary>The idle timeout's fade to the attract demo, a brightness level at a time; at black it requests the demo.</summary>
    TitleScreenFadeOutToDemo,
}
