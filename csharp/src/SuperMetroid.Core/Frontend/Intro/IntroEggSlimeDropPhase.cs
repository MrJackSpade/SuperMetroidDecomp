namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Where one of the Baby Metroid's slime drops stands: flying by its launch, or splashing where it landed.</summary>
internal enum IntroEggSlimeDropPhase
{
    /// <summary>Flying by its launch until it reaches the puddle height.</summary>
    Falling,

    /// <summary>Playing its puddle animation in place, where its fall stopped, until the animation deletes it.</summary>
    Splashing,
}
