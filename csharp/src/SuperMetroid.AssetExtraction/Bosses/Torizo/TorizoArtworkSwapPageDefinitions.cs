
namespace SuperMetroid.AssetExtraction.Bosses.Torizo;

/// <summary>
/// Which page of installed characters each artwork swap the Bomb and Golden Torizo instruction lists make shows (#1406,
/// #1463), in the order of <see cref="SuperMetroid.Core.Bosses.Torizo.TorizoArtworkSwapDefinitions.Sources"/>: the import's
/// knowledge of the bytes each swap's pictures are composed from.
/// </summary>
internal static class TorizoArtworkSwapPageDefinitions
{
    /// <summary>The swaps in instruction-list order: each one's page, the byte of the page it starts at and its bytes.</summary>
    internal static readonly TorizoArtworkSwapDefinition[] Entries =
    [
        new(TorizoTilePage.LeftAttack, 0, 64),
        new(TorizoTilePage.RightAttack, 0, 64),
        new(TorizoTilePage.LeftAttack, 64, 32),
        new(TorizoTilePage.RightAttack, 64, 32),
        new(TorizoTilePage.LeftAttack, 96, 32),
        new(TorizoTilePage.RightAttack, 96, 32),
        new(TorizoTilePage.SharedDeath, 0, 64),
        new(TorizoTilePage.SharedDeath, 0, 64),
        new(TorizoTilePage.SharedDeath, 0, 64),
        new(TorizoTilePage.SharedDeath, 0, 64),
        new(TorizoTilePage.SharedDeath, 0, 64),
        new(TorizoTilePage.SharedDeath, 0, 64),
        new(TorizoTilePage.SharedDeath, 0, 32),
        new(TorizoTilePage.StatueCrumble, 0, 64),
        new(TorizoTilePage.StatueCrumble, 64, 64),
        new(TorizoTilePage.StatueCrumble, 128, 64),
        new(TorizoTilePage.StatueCrumble, 192, 64),
        new(TorizoTilePage.StatueCrumble, 0, 64),
        new(TorizoTilePage.StatueCrumble, 64, 64),
        new(TorizoTilePage.StatueCrumble, 128, 64),
        new(TorizoTilePage.StatueCrumble, 192, 64),
        new(TorizoTilePage.GoldenAwakening, 0, 1536),
        new(TorizoTilePage.StatueCrumble, 0, 64),
        new(TorizoTilePage.StatueCrumble, 64, 64),
        new(TorizoTilePage.StatueCrumble, 128, 64),
        new(TorizoTilePage.StatueCrumble, 192, 64),
        new(TorizoTilePage.StatueCrumble, 0, 64),
        new(TorizoTilePage.StatueCrumble, 64, 64),
        new(TorizoTilePage.StatueCrumble, 128, 64),
        new(TorizoTilePage.StatueCrumble, 192, 64),
        new(TorizoTilePage.LeftAttack, 128, 64),
        new(TorizoTilePage.RightAttack, 128, 64),
        new(TorizoTilePage.LeftAttack, 192, 64),
        new(TorizoTilePage.RightAttack, 192, 64),
        new(TorizoTilePage.LeftAttack, 256, 64),
        new(TorizoTilePage.RightAttack, 256, 64),
        new(TorizoTilePage.LeftAttack, 256, 64),
        new(TorizoTilePage.RightAttack, 256, 64),
        new(TorizoTilePage.LeftAttack, 192, 64),
        new(TorizoTilePage.RightAttack, 192, 64),
        new(TorizoTilePage.LeftAttack, 128, 64),
        new(TorizoTilePage.RightAttack, 128, 64),
        new(TorizoTilePage.GoldenLeftAttack, 0, 64),
        new(TorizoTilePage.GoldenRightAttack, 0, 64),
        new(TorizoTilePage.StatueCrumble, 0, 64),
        new(TorizoTilePage.StatueCrumble, 64, 64),
        new(TorizoTilePage.StatueCrumble, 128, 64),
        new(TorizoTilePage.StatueCrumble, 192, 64),
        new(TorizoTilePage.StatueCrumble, 0, 64),
        new(TorizoTilePage.StatueCrumble, 64, 64),
        new(TorizoTilePage.StatueCrumble, 128, 64),
        new(TorizoTilePage.StatueCrumble, 192, 64),
    ];
}
