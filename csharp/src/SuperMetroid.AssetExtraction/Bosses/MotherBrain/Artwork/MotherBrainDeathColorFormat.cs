namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Installed-resource identity and schema revision for Mother Brain's death colors.</summary>
public static class MotherBrainDeathColorFormat
{
    /// <summary>JSON resource filename used to install the death-fade color catalog.</summary>
    public const string FileName = "mother-brain-death-colors.json";
    /// <summary>
    /// Supported RGB5 document schema revision; it fixes the sixteen-stage body/leg and eight-stage corpse dimensions.
    /// Revision 2 (#1544) holds the fades alone: the exploded door's colours, which no draw showed, are deleted.
    /// </summary>
    public const int Version = 2;

    /// <summary>The colours each body or leg fade image rewrites.</summary>
    public const int BodyColorCount = 14;

    /// <summary>The colours each corpse fade image rewrites.</summary>
    public const int CorpseColorCount = 15;
}
