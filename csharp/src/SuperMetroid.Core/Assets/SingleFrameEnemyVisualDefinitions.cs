namespace SuperMetroid.Core.Assets;

/// <summary>Ordinary artwork selected by the single-frame Kzan and Polyp programs.</summary>
internal static class SingleFrameEnemyVisualDefinitions
{
    /// <summary>Spritemap_Kzan at $A6:8CE5, selected by InstList_Kzan's $A6:8B2B operand.</summary>
    internal static readonly EnemySpritemapDefinition Kzan = new(0xa6, 0x8ce5, "kzan_00");

    /// <summary>Spritemap_Polyp at $A2:B5FB, selected by InstList_Polyp's $A2:B51C operand.</summary>
    internal static readonly EnemySpritemapDefinition Polyp = new(0xa2, 0xb5fb, "polyp_00");
}
