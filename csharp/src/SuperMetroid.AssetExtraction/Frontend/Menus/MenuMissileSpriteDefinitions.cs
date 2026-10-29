namespace SuperMetroid.AssetExtraction.Frontend.Menus;

/// <summary>The selection missile's spritemaps and the menu atlas cells its poses draw, which the menu sheets picture.</summary>
internal static class MenuMissileSpriteDefinitions
{
    /// <summary>$82:BAB2: the spritemap ID frame zero draws; the four consecutive menu spritemaps play in reverse ID order.</summary>
    internal const int FirstFrameSpritemap = 0x37;

    /// <summary>$82:C5D1: the first menu missile spritemap identity; playback order comes from $82:BAB2, not atlas ordering.</summary>
    internal const int FirstSpritemap = 0x34;

    /// <summary>$82:CBD7: the second missile pose's left cell, the top-left of the $EE/$EF/$FE/$FF two-by-two atlas block.</summary>
    internal const int SecondPoseLeftCell = 0xee;

    /// <summary>$82:CBE3: the third missile pose's right cell.</summary>
    internal const int ThirdPoseRightCell = 0xcc;

    /// <summary>$82:CBEF: the final missile pose's left cell; native part order is left then right.</summary>
    internal const int FinalPoseLeftCell = 0xc8;
}
