using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One installed Crocomire skeleton character chunk, shown in the OBJ character table.</summary>
/// <param name="Entry">The chunk's transfer entry.</param>
internal sealed record CrocomireSkeletonChunkCharacters(int Entry) : ObjCharacterSource
{
    /// <inheritdoc/>
    public override ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver) =>
        (InstalledBossObjCharacterArtwork.Of(resolver).BossTiles.CrocomireSkeleton ?? throw new InvalidOperationException(
            "Installed Crocomire artwork is missing its skeleton characters.")).Chunk(Entry);

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"Crocomire skeleton chunk {Entry}";
}
