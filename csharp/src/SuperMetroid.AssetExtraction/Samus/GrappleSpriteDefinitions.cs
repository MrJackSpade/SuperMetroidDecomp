namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Presentation identities for the endpoint and four timed rope records.</summary>
public static class GrappleSpriteDefinitions
{
    /// <summary>Editable JSON filename for Grapple's endpoint and four rope-segment OBJ attribute selections, excluding segment timing and beam mechanics.</summary>
    public const string FileName = "grapple-sprites.json";
    /// <summary>Supported revision of the endpoint-plus-four-segment visual attribute schema.</summary>
    public const int Version = 1;
    /// <summary>The rope's four timed frames, each a duration and one appearance.</summary>
    public const int SegmentFrameCount = 4;
}
