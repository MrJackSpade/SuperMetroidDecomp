namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Installed resource identity and JSON schema version for the Mother Brain cutscene Baby's initial and death-fade artwork, separate from health and pulse palettes.</summary>
public static class BabyMetroidCutsceneColorFormat
{
    /// <summary>Installed artwork filename loaded with enemy tile resources for the Baby's initial colors and six death-fade images.</summary>
    public const string FileName = "baby-metroid-cutscene-colors.json";
    /// <summary>Supported schema version, requiring a fifteen-color initial image and six fourteen-color fade rows.</summary>
    public const int Version = 1;
}
