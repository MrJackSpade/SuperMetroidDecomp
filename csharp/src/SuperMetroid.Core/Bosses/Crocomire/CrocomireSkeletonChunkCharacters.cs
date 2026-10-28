using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One installed Crocomire skeleton character chunk, shown in the OBJ character table.</summary>
/// <param name="Entry">The chunk's transfer entry.</param>
internal sealed record CrocomireSkeletonChunkCharacters(int Entry) : SpriteArtworkSource
{
    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"Crocomire skeleton chunk {Entry}";
}
