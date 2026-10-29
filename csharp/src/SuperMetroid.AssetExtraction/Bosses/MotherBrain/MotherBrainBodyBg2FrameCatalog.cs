using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>
/// Installed BG2 half of Mother Brain's sixteen mixed body poses. Visual tile
/// references and ordered runs are editable; body position, scroll, instruction
/// timing, AI and collision geometry remain compiled gameplay state.
/// </summary>
public sealed class MotherBrainBodyBg2FrameCatalog
{
    /// <summary>Canonical selected BG2 content identity in native pose-pointer order, retaining each write's destination, command order, and tile-run boundaries independently of JSON formatting.</summary>
    public string ContentIdentity => Writes.ContentIdentity;

    /// <summary>The frames' writes, compiled and indexed by native extended-frame roots.</summary>
    internal EnemyBg2FrameCatalog Writes { get; }

    /// <summary>Wraps the validated common BG2 catalog after all required Mother Brain poses have been admitted.</summary>
    private MotherBrainBodyBg2FrameCatalog(EnemyBg2FrameCatalog frames) => Writes = frames;

    /// <summary>Loads BG2 artwork for all sixteen named bank-$A9 body poses, excluding the OAM-only initial dummy; compiles ordered horizontal tile runs without importing body motion, hitboxes, or instruction timing.</summary>
    /// <param name="json">Caller-owned JSON stream, left open, containing every required pose with nonempty bounded writes in 32-by-64 tilemap-cell coordinates and placed tiles.</param>
    /// <returns>Installed visual writes keyed by the same native extended-frame roots used by the body's compiled instruction programs.</returns>
    /// <exception cref="InvalidDataException">JSON properties/schema, required pose identities, command counts, destinations, or tiles are invalid.</exception>
    public static MotherBrainBodyBg2FrameCatalog Load(Stream json) => new(
        EnemyBg2Frames.Load(json, MotherBrainBodyBg2FrameDefinitions.Frames,
            MotherBrainBodyBg2FrameDefinitions.Version, "Mother Brain body"));
}
