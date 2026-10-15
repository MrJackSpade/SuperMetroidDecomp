using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values and helpers of <see cref="YappingMawVisualDefinitions"/> that only verification reads.</summary>
internal static class YappingMawVisualDefinitionsConstants
{
    /// <summary>The pose the upward attack opens on, the idle Yapping Maw's first frame.</summary>
    internal static ushort IdleFrame => YappingMawAnimations.Spritemaps(YappingMawProgram.AttackingUp).First();
}
