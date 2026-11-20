namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Which layers an extended frame's components draw on.</summary>
internal enum EnemyExtendedFrameLayers
{
    /// <summary>Every component is ordinary OAM.</summary>
    Sprites,

    /// <summary>The frame's sprites draw beside a body on BG2, whose components the import extracts separately.</summary>
    SpritesBesideBg2,

    /// <summary>Every component is a BG2 stream; the frame holds no OAM.</summary>
    Bg2Only,
}
