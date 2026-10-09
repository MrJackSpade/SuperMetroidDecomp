namespace SuperMetroid.Core.Assets;

/// <summary>
/// Installed BG2 half of Mother Brain's sixteen mixed body poses. Visual tile
/// references and ordered runs are editable; body position, scroll, instruction
/// timing, AI and collision geometry remain compiled gameplay state.
/// </summary>
public sealed class MotherBrainBodyBg2FrameCatalog
{
    /// <summary>Canonical selected BG2 content identity in native pose-pointer order, retaining each write's destination, command order, and tile-run boundaries independently of JSON formatting.</summary>
    public string ContentIdentity => frames.ContentIdentity;
    private readonly EnemyBg2FrameCatalog frames;

    private MotherBrainBodyBg2FrameCatalog(EnemyBg2FrameCatalog frames) => this.frames = frames;

    internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
        frames.TryGet(pointer, out writes);

    /// <summary>Loads version-one BG2 artwork for all sixteen named bank-$A9 body poses, excluding the OAM-only initial dummy; compiles ordered horizontal tile runs without importing body motion, hitboxes, or instruction timing.</summary>
    /// <param name="json">Caller-owned JSON stream, left open, containing every required pose with nonempty bounded writes in 32-by-64 tilemap-cell coordinates and unsigned 16-bit tile words.</param>
    /// <returns>Installed visual writes keyed by the same native extended-frame roots used by the body's compiled instruction programs.</returns>
    /// <exception cref="InvalidDataException">JSON properties/schema, required pose identities, command counts, destinations, or tile words are invalid.</exception>
    public static MotherBrainBodyBg2FrameCatalog Load(Stream json) => new(
        EnemyBg2FrameCatalog.Load(json, MotherBrainBodyVisualDefinitions.Bg2Frames,
            MotherBrainBodyVisualDefinitions.Bg2Version, "Mother Brain body"));
}
