
namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// Stock identities for installed enemy compositions. These are visual frame selections,
/// not editable instruction timers, AI callbacks, collision or hitbox definitions.
/// </summary>
internal static class EnemySpritemapDefinitions
{
    /// <summary>Installed document containing editable enemy composition frames and bindings.</summary>
    internal const string FileName = "enemy-compositions.json";
    /// <summary>The schema-ordered stock frame catalog used to validate and bind installed compositions.</summary>
    internal static EnemySpritemapFrameList Frames { get; } = new();
}
