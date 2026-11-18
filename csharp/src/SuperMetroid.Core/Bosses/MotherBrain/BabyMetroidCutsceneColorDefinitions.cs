namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The shape of the cutscene Baby Metroid's fade to black that the game plays; its colours are the import's (#1527).</summary>
/// <remarks>
/// The count stays in Core (#2471): the Baby's fade is made of its images (<see cref="MotherBrainFadeDefinitions.BabyFade"/>),
/// as well as the import reading it.
/// </remarks>
public static class BabyMetroidCutsceneColorDefinitions
{
    /// <summary>The fade's six shown steps; the step after the sixth ends the fade rather than showing a seventh.</summary>
    public const int FadeFrameCount = 6;
}
