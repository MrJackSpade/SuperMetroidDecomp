namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>Editable visual rows; the reveal's cadence and zero terminator stay in code.</summary>
public static class NorfairRidleyColorFormat
{
    /// <summary>Asset filename used to install Lower Norfair Ridley's editable initial and arena-reveal colors.</summary>
    public const string FileName = "norfair-ridley-colors.json";
    /// <summary>Supported JSON schema revision for the 32-color initial palette and fifteen fourteen-color reveal rows.</summary>
    public const int Version = 1;

    /// <summary>Ridley's initial colours: two complete sixteen-colour sprite palettes.</summary>
    public const int InitialColorCount = 32;

    /// <summary>The colours of one reveal step: the arena background palette's colours one to fourteen.</summary>
    public const int RevealColorCount = 14;
}
