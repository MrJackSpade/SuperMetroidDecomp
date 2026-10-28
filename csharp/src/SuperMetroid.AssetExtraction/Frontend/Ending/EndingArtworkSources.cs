using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>Which cartridge sources the ending's Mode 7 scenes and explosion fragments import from (#1416: moved out of Core).</summary>
internal static class EndingArtworkSources
{
    /// <summary>
    /// A floor scene's sources: escape A is decompressed by $8B:D4B4/D4D6, escape B by $8B:D775/D604 and the explosion
    /// by $8B:D87B/D615, each the high-byte character lane followed by the packed low-byte map source.
    /// </summary>
    public static EndingFloorSources Floor(EndingFloorSceneId id) => id switch
    {
        EndingFloorSceneId.EscapeA => new(EndingSourceDefinitions.Artwork.EscapeACharacters, EndingSourceDefinitions.Artwork.EscapeAPackedMap),
        EndingFloorSceneId.EscapeB => new(EndingSourceDefinitions.Artwork.EscapeBCharacters, EndingSourceDefinitions.Artwork.EscapeBPackedMap),
        EndingFloorSceneId.PlanetExplosion => new(EndingSourceDefinitions.Artwork.ExplosionCharacters, EndingSourceDefinitions.Artwork.ExplosionPackedMap),
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    /// <summary>An explosion fragment's compressed source, decompressed by $8B:D5AF..D5F2.</summary>
    public static int ObjectFragment(EndingObjectFragmentId id) => id switch
    {
        EndingObjectFragmentId.Segment70 => EndingSourceDefinitions.Artwork.WideExplosionTilemap,
        EndingObjectFragmentId.Segment74 => EndingSourceDefinitions.Artwork.ConcentricExplosionTilemap,
        EndingObjectFragmentId.Segment78 => EndingSourceDefinitions.Artwork.EclipseTilemap,
        EndingObjectFragmentId.Segment7C => EndingSourceDefinitions.Artwork.BlankTilemap,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };
}
