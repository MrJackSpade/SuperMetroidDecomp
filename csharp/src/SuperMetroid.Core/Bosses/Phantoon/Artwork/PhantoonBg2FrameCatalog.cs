using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// Installed Phantoon BG2 presentation; only visual tilemap writes are editable.
/// Physical hitboxes and native frame timing remain engine-owned.
/// </summary>
public sealed class PhantoonBg2FrameCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => Writes.ContentIdentity;

    /// <summary>The frames' writes, compiled and indexed by native extended-frame roots.</summary>
    internal EnemyBg2FrameCatalog Writes { get; }


    /// <summary>Wraps validated BG2 frame writes without changing engine-owned timing or hitboxes.</summary>
    private PhantoonBg2FrameCatalog(EnemyBg2FrameCatalog frames) => Writes = frames;

    /// <summary>Finds the installed tilemap writes for a native frame root, if that root is defined.</summary>
    internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
        Writes.TryGet(frame, out writes);

    /// <summary>Compiles version-one BG2 artwork for all twenty-two bank-$A7 body/eye, tentacle, and mouth roots selected by Phantoon's compiled instruction programs.</summary>
    /// <param name="json">Caller-owned JSON stream, left open, with every named frame containing ordered horizontal runs of unsigned 16-bit tile words in 32-by-64 tilemap-cell coordinates.</param>
    /// <returns>Installed visual writes keyed by extended-frame roots; body/eye hitboxes, gaze selection, instruction timing, and new-frame write gating remain engine-owned.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON properties/schema, required frame identities, command counts, destinations, or tile words are invalid.</exception>
    public static PhantoonBg2FrameCatalog Load(Stream json) =>
        new(EnemyBg2FrameCatalog.Load(json, PhantoonBg2FrameDefinitions.Frames,
            PhantoonBg2FrameDefinitions.Version, "Phantoon"));
}
