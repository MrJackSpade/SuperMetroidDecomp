using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Installed Draygon BG2 tilemap presentation. The $A5 instruction selector,
/// collision geometry, and callback records are not editable visual data.
/// </summary>
public sealed class DraygonBg2FrameCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => Writes.ContentIdentity;

    /// <summary>The frames' writes, compiled and indexed by native extended-frame roots.</summary>
    internal EnemyBg2FrameCatalog Writes { get; }


    /// <summary>Wraps the validated common BG2 catalog after Draygon's required poses have been admitted.</summary>
    private DraygonBg2FrameCatalog(EnemyBg2FrameCatalog frames) => Writes = frames;

    /// <summary>Compiles BG2 tilemap writes for the thirty-four bank-$A5 body roots: seventeen left-facing and seventeen right-facing poses; the forty-eight ordinary OAM roots use a separate presentation catalog.</summary>
    /// <param name="json">Caller-owned JSON stream, left open, with every pointer-named pose containing ordered horizontal runs of placed tiles in 32-by-64 tilemap-cell coordinates.</param>
    /// <returns>Installed visual writes selected by native extended-frame identity; collision callbacks and the new-instruction-frame write gate remain engine-owned.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON properties/schema, required pose identities, command counts, destinations, or tiles are invalid.</exception>
    public static DraygonBg2FrameCatalog Load(Stream json) =>
        new(EnemyBg2Frames.Load(json, DraygonBg2FrameDefinitions.Frames,
            DraygonBg2FrameDefinitions.Version, "Draygon"));
}
