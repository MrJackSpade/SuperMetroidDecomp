namespace SuperMetroid.Core.Assets;

/// <summary>
/// Installed Draygon BG2 tilemap presentation. The $A5 instruction selector,
/// collision geometry, and callback records are not editable visual data.
/// </summary>
public sealed class DraygonBg2FrameCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => frames.ContentIdentity;

    /// <summary>Validated Draygon visual writes indexed by the native instruction pointer that selects each frame.</summary>
    private readonly EnemyBg2FrameCatalog frames;

    /// <summary>Wraps the validated BG2 frame definitions for use by Draygon's extended-frame renderer.</summary>
    /// <param name="frames">Compiled named frames and their ordered tilemap writes.</param>
    private DraygonBg2FrameCatalog(EnemyBg2FrameCatalog frames) => this.frames = frames;

    /// <summary>Looks up the ordered BG2 writes selected by a native Draygon frame pointer.</summary>
    /// <param name="pointer">Native instruction pointer identifying the requested visual frame.</param>
    /// <param name="writes">Receives the selected frame's tilemap writes, or an empty value when no frame matches.</param>
    /// <returns><see langword="true"/> when the pointer identifies one of the catalogued Draygon frames.</returns>
    internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
        frames.TryGet(pointer, out writes);

    /// <summary>Compiles version-one BG2 tilemap writes for the thirty-four bank-$A5 body roots: seventeen left-facing and seventeen right-facing poses; the forty-eight ordinary OAM roots use a separate presentation catalog.</summary>
    /// <param name="json">Caller-owned JSON stream, left open, with every pointer-named pose containing ordered horizontal runs of unsigned 16-bit tile words in 32-by-64 tilemap-cell coordinates.</param>
    /// <returns>Installed visual writes selected by native extended-frame identity; collision callbacks and the new-instruction-frame write gate remain engine-owned.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON properties/schema, required pose identities, command counts, destinations, or tile words are invalid.</exception>
    public static DraygonBg2FrameCatalog Load(Stream json) =>
        new(EnemyBg2FrameCatalog.Load(json, DraygonBg2FrameDefinitions.Frames,
            DraygonBg2FrameDefinitions.Version, "Draygon"));
}
