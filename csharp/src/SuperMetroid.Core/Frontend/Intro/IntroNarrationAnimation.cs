namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The animations of the intro narration's caret and Japanese-text next-page arrow (#2197).</summary>
internal enum IntroNarrationAnimation
{
    /// <summary>The caret, shown steadily while the text types.</summary>
    Caret,
    /// <summary>The caret blinking.</summary>
    CaretBlinking,
    /// <summary>The next-page arrow's loop, which the arrow starts on at each page's input wait (#1655).</summary>
    NextPageArrow,
    /// <summary>The next-page arrow hidden while a second subtitle holds the input wait, then its loop.</summary>
    NextPageArrowDelayed,
}
