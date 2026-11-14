namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The Skree and Metaree animations of each animation phase.</summary>
internal static class SkreeMetareeAnimationDefinitions
{
    /// <summary>The Metaree segment for each named animation phase.</summary>
    internal static readonly Dictionary<SkreeMetareeAnimationPhase, SkreeMetareeSegment> MetareeAnimations = new()
    {
        [SkreeMetareeAnimationPhase.Idling] = SkreeMetareeSegment.MetareeIdling,
        [SkreeMetareeAnimationPhase.PreparingAttack] = SkreeMetareeSegment.MetareePreparingAttack,
        [SkreeMetareeAnimationPhase.Diving] = SkreeMetareeSegment.MetareeDiving,
    };

    /// <summary>The Skree segment for each named animation phase.</summary>
    internal static readonly Dictionary<SkreeMetareeAnimationPhase, SkreeMetareeSegment> SkreeAnimations = new()
    {
        [SkreeMetareeAnimationPhase.Idling] = SkreeMetareeSegment.SkreeIdling,
        [SkreeMetareeAnimationPhase.PreparingAttack] = SkreeMetareeSegment.SkreePreparingAttack,
        [SkreeMetareeAnimationPhase.Diving] = SkreeMetareeSegment.SkreeDiving,
    };
}
