namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Fixed animation selectors for Skree and Metaree.</summary>
internal static class SkreeMetareeAnimationDefinitions
{
    /// <summary>$A3:894E: the Metaree program for each named animation phase.</summary>
    internal static readonly Dictionary<SkreeMetareeAnimationPhase, SkreeMetareeProgram> MetareePrograms = new()
    {
        [SkreeMetareeAnimationPhase.Idling] = SkreeMetareeProgram.MetareeIdling,
        [SkreeMetareeAnimationPhase.PreparingAttack] = SkreeMetareeProgram.MetareePreparingAttack,
        [SkreeMetareeAnimationPhase.Diving] = SkreeMetareeProgram.MetareeDiving,
        [SkreeMetareeAnimationPhase.StopAnimating] = SkreeMetareeProgram.MetareeStopAnimating,
    };

    /// <summary>$A3:C69C: the Skree program for each named animation phase.</summary>
    internal static readonly Dictionary<SkreeMetareeAnimationPhase, SkreeMetareeProgram> SkreePrograms = new()
    {
        [SkreeMetareeAnimationPhase.Idling] = SkreeMetareeProgram.SkreeIdling,
        [SkreeMetareeAnimationPhase.PreparingAttack] = SkreeMetareeProgram.SkreePreparingAttack,
        [SkreeMetareeAnimationPhase.Diving] = SkreeMetareeProgram.SkreeDiving,
        [SkreeMetareeAnimationPhase.StopAnimating] = SkreeMetareeProgram.SkreeStopAnimating,
    };
}
