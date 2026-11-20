namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A look a Torizo's body animations give its artwork (#1406, #1538, #1761): each one changes a part of the Torizo, its
/// eyes, its gut, its face or its chest, by a stage. A look shows over every look its Torizo showed before it, so a stage's
/// name says what that stage changes, and the Torizo's frames show their finished pictures of the last look it named. Each
/// look is one picture state the player sees (#1942). Both Torizos light their eyes alike as they wake; the gut and
/// face are Bomb Torizo's, the awakening,
/// chest and later glows Golden Torizo's. Its colouring is apart from its artwork's look: a <see cref="TorizoLook"/>. The
/// values are arbitrary identities; each look's installed name is written out in <see cref="TorizoArtworkLooks"/> (#2409).
/// </summary>
internal enum TorizoArtworkLook
{
    /// <summary>The first stage of the eyes lighting up as the statue wakes: the eyes still dark.</summary>
    EyesDark,

    /// <summary>The second stage of the eyes lighting up as the statue wakes: a faint glow.</summary>
    EyesGlowingFaintly,

    /// <summary>The third stage of the eyes lighting up as the statue wakes: a brighter glow.</summary>
    EyesGlowingBrightly,

    /// <summary>The fourth stage of the eyes lighting up as the statue wakes: the eyes glow fully.</summary>
    EyesGlowingFully,

    /// <summary>The eyes dark again as they flicker back on, before the statue stands.</summary>
    EyesDarkAgain,

    /// <summary>The second stage of the eyes flickering back on: a faint glow.</summary>
    EyesGlowingFaintlyAgain,

    /// <summary>The third stage of the eyes flickering back on: a brighter glow.</summary>
    EyesGlowingBrightlyAgain,

    /// <summary>The fourth stage of the eyes flickering back on: the eyes glow fully, as the Torizo stands and fights.</summary>
    EyesGlowingFullyAgain,

    /// <summary>Bomb Torizo's gut blown open, as it fights on at low health; its whole blow-up lands in one update.</summary>
    GutBlownUp,

    /// <summary>Bomb Torizo's face half blown away, the eye still glowing, which shows for one update.</summary>
    FaceHalfBlownAway,

    /// <summary>Bomb Torizo's face blown away, eye and all, as it fights on faceless.</summary>
    FaceBlownUp,

    /// <summary>Golden Torizo's own body, which its first cue shows before any of its frames.</summary>
    Awakening,

    /// <summary>Golden Torizo's chest at its first stage of opening to release an egg, both its halves at once.</summary>
    ChestParting,

    /// <summary>Golden Torizo's chest at its second stage of opening.</summary>
    ChestHalfOpen,

    /// <summary>Golden Torizo's chest open, as it releases its eggs.</summary>
    ChestOpen,

    /// <summary>Golden Torizo's chest held open once its eggs are out.</summary>
    ChestHeldOpen,

    /// <summary>Golden Torizo's chest at its first stage of closing.</summary>
    ChestHalfClosed,

    /// <summary>Golden Torizo's chest at its second stage of closing.</summary>
    ChestNearlyClosed,

    /// <summary>Golden Torizo's chest closed again.</summary>
    ChestClosed,

    /// <summary>Golden Torizo's eyes dark as its eye beam fires.</summary>
    EyeBeamEyesDark,

    /// <summary>The second stage of Golden Torizo's eyes glowing as its eye beam fires.</summary>
    EyeBeamEyesGlowingFaintly,

    /// <summary>The third stage of Golden Torizo's eyes glowing as its eye beam fires.</summary>
    EyeBeamEyesGlowingBrightly,

    /// <summary>Golden Torizo's eyes glowing fully as its eye beam fires.</summary>
    EyeBeamEyesGlowingFully,

    /// <summary>Golden Torizo's eyes dark as it recovers from a stun.</summary>
    StunnedEyesDark,

    /// <summary>The second stage of Golden Torizo's eyes lighting up as it recovers from a stun.</summary>
    StunnedEyesGlowingFaintly,

    /// <summary>The third stage of Golden Torizo's eyes lighting up as it recovers from a stun.</summary>
    StunnedEyesGlowingBrightly,

    /// <summary>Golden Torizo's eyes glowing fully once it has recovered from a stun.</summary>
    StunnedEyesGlowingFully,
}
