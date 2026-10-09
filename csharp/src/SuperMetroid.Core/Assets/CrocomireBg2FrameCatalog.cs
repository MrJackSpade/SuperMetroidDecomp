namespace SuperMetroid.Core.Assets;

/// <summary>Editable BG2 writes for Crocomire's mixed fight-body frames.</summary>
public sealed class CrocomireBg2FrameCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => frames.ContentIdentity;

    private readonly EnemyBg2FrameCatalog frames;

    private CrocomireBg2FrameCatalog(EnemyBg2FrameCatalog frames) => this.frames = frames;

    internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
        frames.TryGet(pointer, out writes);

    /// <summary>Compiles version-one BG2 artwork for all forty-two selected mixed fight-body roots in bank $A4; pure-OAM death transitions and skeleton poses are not part of this catalog.</summary>
    /// <param name="json">Caller-owned JSON stream, left open, with every pointer-named frame containing ordered horizontal runs of unsigned 16-bit tile words in 32-by-64 tilemap-cell coordinates.</param>
    /// <returns>Installed body-frame writes; the enemy's instruction-frame gate, hitboxes, motion, and separate OAM presentation remain engine-owned.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON properties/schema, required frame identities, command counts, destinations, or tile words are invalid.</exception>
    public static CrocomireBg2FrameCatalog Load(Stream json) =>
        new(EnemyBg2FrameCatalog.Load(json, CrocomireBg2FrameDefinitions.Frames,
            CrocomireBg2FrameDefinitions.Version, "Crocomire"));
}
