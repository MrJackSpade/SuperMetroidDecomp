using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Editable BG2 writes for Crocomire's mixed fight-body frames.</summary>
public sealed class CrocomireBg2FrameCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => Writes.ContentIdentity;

    /// <summary>The frames' writes, compiled and indexed by native extended-frame roots.</summary>
    internal EnemyBg2FrameCatalog Writes { get; }


    /// <summary>Retains the validated frame catalog after loading its complete pointer set.</summary>
    private CrocomireBg2FrameCatalog(EnemyBg2FrameCatalog frames) => Writes = frames;

    /// <summary>Compiles BG2 artwork for all forty-two selected mixed fight-body roots in bank $A4; pure-OAM death transitions and skeleton poses are not part of this catalog.</summary>
    /// <param name="json">Caller-owned JSON stream, left open, with every pointer-named frame containing ordered horizontal runs of placed tiles in 32-by-64 tilemap-cell coordinates.</param>
    /// <returns>Installed body-frame writes; the enemy's instruction-frame gate, hitboxes, motion, and separate OAM presentation remain engine-owned.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON properties/schema, required frame identities, command counts, destinations, or tiles are invalid.</exception>
    public static CrocomireBg2FrameCatalog Load(Stream json) =>
        new(EnemyBg2Frames.Load(json, CrocomireBg2FrameDefinitions.Frames,
            CrocomireBg2FrameDefinitions.Version, "Crocomire"));
}
