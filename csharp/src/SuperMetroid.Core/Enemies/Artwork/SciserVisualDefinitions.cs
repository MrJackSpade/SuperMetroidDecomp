namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Sciser's twelve distinct bank-$A3 OAM compositions. The four-surface typed
/// programs show these frames; their timing and movement remain enemy behavior.
/// </summary>
internal static class SciserVisualDefinitions
{
    /// <summary>Native Sciser spritemap bank $A3.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>$A3:9703 / Spritemap_Sciser_UpsideUp_0 begins twelve four-object records, each 22 bytes.</summary>
    private const ushort FirstFrame = 0x9703;
    /// <summary>Maps four surface orientations to native frame groups, returning three named poses per orientation.</summary>
    internal static EnemySpritemapDefinition[] Frames()
    {
        var result = new EnemySpritemapDefinition[12];
        for (int surface = 0; surface < 4; surface++)
        {
            (int group, string name) = surface switch
            {
                0 => (1, "right"), 1 => (3, "left"), 2 => (2, "down"), _ => (0, "up"),
            };
            for (int frame = 0; frame < 3; frame++)
                result[surface * 3 + frame] = new(Bank, (ushort)(FirstFrame + (group * 3 + frame) * 22),
                    $"sciser_upside_{name}_{frame}");
        }
        return result;
    }
}
