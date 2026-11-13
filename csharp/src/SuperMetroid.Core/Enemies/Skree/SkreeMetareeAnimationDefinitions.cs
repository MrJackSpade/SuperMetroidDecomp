namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Fixed animation selectors for Skree and Metaree.</summary>
internal static class SkreeMetareeAnimationDefinitions
{
    /// <summary>The Metaree segment for each named animation phase.</summary>
    internal static readonly Dictionary<SkreeMetareeAnimationPhase, SkreeMetareeSegment> MetareeAnimations = new()
    {
        [SkreeMetareeAnimationPhase.Idling] = SkreeMetareeSegment.MetareeIdling,
        [SkreeMetareeAnimationPhase.PreparingAttack] = SkreeMetareeSegment.MetareePreparingAttack,
        [SkreeMetareeAnimationPhase.Diving] = SkreeMetareeSegment.MetareeDiving,
        [SkreeMetareeAnimationPhase.StopAnimating] = SkreeMetareeSegment.MetareeStopAnimating,
    };

    /// <summary>The Skree segment for each named animation phase.</summary>
    internal static readonly Dictionary<SkreeMetareeAnimationPhase, SkreeMetareeSegment> SkreeAnimations = new()
    {
        [SkreeMetareeAnimationPhase.Idling] = SkreeMetareeSegment.SkreeIdling,
        [SkreeMetareeAnimationPhase.PreparingAttack] = SkreeMetareeSegment.SkreePreparingAttack,
        [SkreeMetareeAnimationPhase.Diving] = SkreeMetareeSegment.SkreeDiving,
        [SkreeMetareeAnimationPhase.StopAnimating] = SkreeMetareeSegment.SkreeStopAnimating,
    };
}
