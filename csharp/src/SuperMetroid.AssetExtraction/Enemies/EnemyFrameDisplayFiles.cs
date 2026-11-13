using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed enemy display bindings (#1638), which the player edits in place: for each named frame, the frame displayed
/// for it. They are what the game reads of the enemy compositions, whose parts are the original's packed sprite placements
/// and stay import data (<see cref="EnemySpritemapDocuments"/>, <see cref="EnemyExtendedFrameDocuments"/>).
/// </summary>
internal static class EnemyFrameDisplayFiles
{
    /// <summary>The installed document's file name.</summary>
    internal const string FileName = "enemy-frame-displays.json";

    /// <summary>The document revision extraction writes and loading accepts.</summary>
    internal const int Revision = 1;

    /// <summary>Strict document encoding: camel-case names, no unknown or duplicate properties, indented output.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>The stock bindings: every frame displayed as itself, as the original displays every frame.</summary>
    internal static byte[] Extract() => JsonSerializer.SerializeToUtf8Bytes(new EnemyFrameDisplayDocument
    {
        Version = Revision,
        DisplayFrames = EnemyFrameBindingRules.Identity(EnemySpritemapDefinitions.Frames),
        ExtendedDisplayFrames = EnemyFrameBindingRules.Identity(EnemyExtendedFrameDefinitions.Frames),
    }, Options);

    /// <summary>
    /// Reads and validates the bindings in <paramref name="json"/> by the rules <see cref="EnemyFrameBindingRules"/> states
    /// by the enemies showing each frame and the families of the extended frames: where the original stored a frame is the
    /// import's knowledge, not a rule of an edited binding (#1710).
    /// </summary>
    /// <exception cref="InvalidDataException">The document, or a binding, is invalid.</exception>
    internal static InstalledEnemyFrameDisplays Load(Stream json)
    {
        Ensure.NotNull(json);
        EnemyFrameDisplayDocument document = JsonAssetDocument.Read<EnemyFrameDisplayDocument>(json, Options, "enemy display bindings");
        if (document.Version != Revision)
            throw new InvalidDataException($"Enemy display bindings require revision {Revision}.");
        return new InstalledEnemyFrameDisplays(
            EnemyFrameBindingRules.Ordinary(document.DisplayFrames),
            EnemyFrameBindingRules.Extended(document.ExtendedDisplayFrames));
    }
}
