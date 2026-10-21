namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Four bank-$A3 ordinary-Metroid body compositions shared by the chasing and
/// draining loops. Animation cadence, cry callbacks, contact, and the separate
/// outer-body sprite objects remain engine-owned behavior.
/// </summary>
internal static class MetroidVisualDefinitions
{
    /// <summary>Native ordinary-Metroid body spritemap bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>$A3:F10D / Spritemap_Metroid_Insides_0 starts four records with 8/6/8/8 OAM objects.</summary>
    private const ushort FirstFrame = 0xf10d;
    /// <summary>Builds the four body compositions using the shorter second record's native offset adjustment.</summary>
    internal static NativeEnemySpritemapDefinition[] Frames()
    {
        var result = new NativeEnemySpritemapDefinition[4];
        for (int frame = 0; frame < result.Length; frame++)
            result[frame] = new(Bank, (ushort)(FirstFrame + frame * 42 - (frame >= 2 ? 10 : 0)), $"metroid_body_{frame}");
        return result;
    }
}
