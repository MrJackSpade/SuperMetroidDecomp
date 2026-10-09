namespace SuperMetroid.Core.Assets;

/// <summary>
/// Installed Phantoon BG2 presentation; only visual tilemap writes are editable.
/// Physical hitboxes and native frame timing remain engine-owned.
/// </summary>
public sealed class PhantoonBg2FrameCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => frames.ContentIdentity;

    /// <summary>Validated Phantoon BG2 tilemap writes indexed by native instruction-frame pointer.</summary>
    private readonly EnemyBg2FrameCatalog frames;

    /// <summary>Wraps the compiled frame catalog used for Phantoon's BG2 presentation lookups.</summary>
    /// <param name="frames">Validated BG2 writes keyed by the native pointer for each frame.</param>
    private PhantoonBg2FrameCatalog(EnemyBg2FrameCatalog frames) => this.frames = frames;

    /// <summary>Looks up the BG2 tilemap writes associated with a native Phantoon instruction pointer.</summary>
    /// <param name="pointer">Native pointer identifying the requested visual frame.</param>
    /// <param name="writes">Receives that frame's ordered BG2 writes when the pointer is catalogued.</param>
    /// <returns><see langword="true"/> when the pointer identifies a Phantoon frame; otherwise <see langword="false"/>.</returns>
    internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
        frames.TryGet(pointer, out writes);

    /// <summary>Compiles version-one BG2 artwork for all twenty-two bank-$A7 body/eye, tentacle, and mouth roots selected by Phantoon's compiled instruction programs.</summary>
    /// <param name="json">Caller-owned JSON stream, left open, with every named frame containing ordered horizontal runs of unsigned 16-bit tile words in 32-by-64 tilemap-cell coordinates.</param>
    /// <returns>Installed visual writes keyed by extended-frame roots; body/eye hitboxes, gaze selection, instruction timing, and new-frame write gating remain engine-owned.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON properties/schema, required frame identities, command counts, destinations, or tile words are invalid.</exception>
    public static PhantoonBg2FrameCatalog Load(Stream json) =>
        new(EnemyBg2FrameCatalog.Load(json, PhantoonBg2FrameDefinitions.Frames,
            PhantoonBg2FrameDefinitions.Version, "Phantoon"));
}
