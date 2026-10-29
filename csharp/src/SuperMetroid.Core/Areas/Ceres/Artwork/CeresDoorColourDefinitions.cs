using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>The background colours Ceres doors write (#1405).</summary>
internal static class CeresDoorColourDefinitions
{
    /// <summary>The six colours the door's animation cycles: the first tile set's colours nine to fourteen.</summary>
    internal static readonly BackgroundColourRun Animation = new(BackgroundColourSet.TilesFirst, 9, 6);

    /// <summary>The fifteen colours an inactive door shows: the second enemy store's colours one to fifteen.</summary>
    internal static readonly SpriteColourRun Inactive = new(SpriteColourStore.EnemiesSecond, 1, 15);

    /// <summary>The fifteen colours the active door shows, normal or escaping: the fourth enemy store's colours one to fifteen.</summary>
    internal static readonly SpriteColourRun Active = new(SpriteColourStore.EnemiesFourth, 1, 15);
}
