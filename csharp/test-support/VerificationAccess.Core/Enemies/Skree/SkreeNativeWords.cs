using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// Verification's native view of the Skree identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SkreeNativeWords
{
    /// <summary>The routine word naming each <see cref="SkreeAction"/>.</summary>
    internal static readonly NativeWordIdentities<SkreeAction> SkreeActions = new(new Dictionary<SkreeAction, ushort>
    {
        [SkreeAction.Idling] = 0xc6d5,
        [SkreeAction.PreparingAttack] = 0xc6f7,
        [SkreeAction.Diving] = 0xc716,
        [SkreeAction.Burrowing] = 0xc77f,
    });

    /// <summary>
    /// The animation-phase number naming each <see cref="SkreeMetareeAnimationPhase"/>, shared by Skree and Metaree: the
    /// requested and installed phase words, and the position of each phase's animation in the two phase tables.
    /// </summary>
    internal static readonly NativeWordIdentities<SkreeMetareeAnimationPhase> AnimationPhases = new(new Dictionary<SkreeMetareeAnimationPhase, ushort>
    {
        [SkreeMetareeAnimationPhase.Idling] = 0,
        [SkreeMetareeAnimationPhase.PreparingAttack] = 1,
        [SkreeMetareeAnimationPhase.Diving] = 2,
        [SkreeMetareeAnimationPhase.StopAnimating] = 3,
    });
}
