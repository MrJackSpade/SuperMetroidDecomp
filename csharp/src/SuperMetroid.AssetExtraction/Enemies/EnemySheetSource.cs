using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>What one enemy's sheet composes under a graphics set.</summary>
/// <param name="Enemy">The enemy.</param>
/// <param name="Sources">Its sheet's sources.</param>
/// <param name="Variations">How each of its frames an encounter's artwork sequence changes follows it, by frame name (#1513).</param>
/// <param name="RotPictures">The kind of corpse whose rot each of its frames drawn over a corpse's rows shows, by frame name (#1406).</param>
/// <param name="BlankRotPictures">The names of its frames' pictures of the rots at which they show nothing, which it does not hold (#1513).</param>
internal sealed record EnemySheetSource(EnemyDefinitionId Enemy, SpriteSheetSources Sources,
    IReadOnlyDictionary<string, FrameArtworkVariation[]> Variations, IReadOnlyDictionary<string, string> RotPictures,
    IReadOnlySet<string> BlankRotPictures);
