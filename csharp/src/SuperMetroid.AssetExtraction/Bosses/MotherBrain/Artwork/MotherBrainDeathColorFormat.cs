namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Installed-resource identity and schema revision for Mother Brain death and escape-door colors.</summary>
public static class MotherBrainDeathColorFormat
{
    /// <summary>JSON resource filename used to install the death-fade and exploded-door color catalog.</summary>
    public const string FileName = "mother-brain-death-colors.json";
    /// <summary>Supported RGB5 document schema revision, one; it fixes the sixteen-stage body/leg and eight-stage corpse dimensions.</summary>
    public const int Version = 1;

    /// <summary>The colours each body or leg fade image rewrites; the exploded door's colours are as many.</summary>
    public const int BodyColorCount = 14;

    /// <summary>The colours each corpse fade image rewrites.</summary>
    public const int CorpseColorCount = 15;
}
