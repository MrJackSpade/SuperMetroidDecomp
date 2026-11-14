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
    /// The animation-phase number naming each Skree animation the AI starts: the requested and installed phase words, and
    /// the position of the animation in the Skree's phase table (#1810: Core holds the animation itself).
    /// </summary>
    internal static readonly NativeWordIdentities<SkreeMetareeSegment> SkreeAnimationPhases = new(new Dictionary<SkreeMetareeSegment, ushort>
    {
        [SkreeMetareeSegment.SkreeIdling] = 0,
        [SkreeMetareeSegment.SkreePreparingAttack] = 1,
        [SkreeMetareeSegment.SkreeDiving] = 2,
    });

    /// <summary>
    /// The animation-phase number naming each Metaree animation the AI starts: the requested and installed phase words,
    /// and the position of the animation in the Metaree's phase table (#1810).
    /// </summary>
    internal static readonly NativeWordIdentities<SkreeMetareeSegment> MetareeAnimationPhases = new(new Dictionary<SkreeMetareeSegment, ushort>
    {
        [SkreeMetareeSegment.MetareeIdling] = 0,
        [SkreeMetareeSegment.MetareePreparingAttack] = 1,
        [SkreeMetareeSegment.MetareeDiving] = 2,
    });
}
