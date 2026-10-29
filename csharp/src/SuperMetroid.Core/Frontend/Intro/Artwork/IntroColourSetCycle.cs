namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Whether an opening colour set may be held as the narration's neutral ramp.</summary>
public enum IntroColourSetCycle
{
    /// <summary>The set holds no ramp; its colours are a flat fill or kept as supplied.</summary>
    None,

    /// <summary>The set's visible colours may step down the four-shade neutral ramp from one foreground colour.</summary>
    NeutralRamp,
}
