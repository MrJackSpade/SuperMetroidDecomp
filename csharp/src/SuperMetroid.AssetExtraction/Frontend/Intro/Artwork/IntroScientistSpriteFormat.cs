namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Installed file identity for the six scientist-scene baby sprite frames.</summary>
public static class IntroScientistSpriteFormat
{
    /// <summary>
    /// The supported scientist-scene sprite JSON schema version; 2 holds only the poses the scientist scenes' babies show,
    /// without the caret's block or the next-page arrow (#1654).
    /// </summary>
    public const int Version = 2;

    /// <summary>The embedded editable scientist-scene sprite asset file name.</summary>
    public const string FileName = "intro-scientist-baby-sprites.json";
}
