using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>What one enemy's sheet composes under a graphics set.</summary>
/// <param name="Enemy">The enemy.</param>
/// <param name="Sources">Its sheet's sources.</param>
/// <param name="Pictures">The layers each of its frames drawn over uploaded characters draws as, by frame name.</param>
/// <param name="RotPictures">The kind of corpse whose rot each of its frames drawn over a corpse's rows shows, by frame name (#1406).</param>
internal sealed record EnemySheetSource(EnemyDefinitionId Enemy, SpriteSheetSources Sources,
    IReadOnlyDictionary<string, CharacterRange[][]> Pictures, IReadOnlyDictionary<string, string> RotPictures);
