namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Which pause dispatch advances the menu's sprite animations.</summary>
public enum PauseAnimationPhase
{
    /// <summary>The stable pause dispatch, which also advances the colour animation.</summary>
    Stable,

    /// <summary>The pause fade-in, which draws without advancing the colours.</summary>
    FadingIn,

    /// <summary>The unpause fade-out, which draws destination labels before map icons and does not advance the colours.</summary>
    FadingOut,
}
