using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Every artwork swap the Bomb and Golden Torizo instruction lists make (#1406): which page of installed characters
/// each shows, and over which OBJ characters.
/// </summary>
internal static class TorizoArtworkSwapDefinitions
{
    /// <summary>The swaps in instruction-list order: each one's page, the byte of the page it starts at and its bytes.</summary>
    internal static readonly TorizoArtworkSwapDefinition[] Entries =
    [
        new(TorizoTilePage.LeftAttack, 0x000, 0x0040),
        new(TorizoTilePage.RightAttack, 0x000, 0x0040),
        new(TorizoTilePage.LeftAttack, 0x040, 0x0020),
        new(TorizoTilePage.RightAttack, 0x040, 0x0020),
        new(TorizoTilePage.LeftAttack, 0x060, 0x0020),
        new(TorizoTilePage.RightAttack, 0x060, 0x0020),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0040),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0040),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0040),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0040),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0040),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0040),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0020),
        new(TorizoTilePage.StatueCrumble, 0x000, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x040, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x080, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x0c0, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x000, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x040, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x080, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x0c0, 0x0040),
        new(TorizoTilePage.GoldenAwakening, 0x000, 0x0600),
        new(TorizoTilePage.StatueCrumble, 0x000, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x040, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x080, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x0c0, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x000, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x040, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x080, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x0c0, 0x0040),
        new(TorizoTilePage.LeftAttack, 0x080, 0x0040),
        new(TorizoTilePage.RightAttack, 0x080, 0x0040),
        new(TorizoTilePage.LeftAttack, 0x0c0, 0x0040),
        new(TorizoTilePage.RightAttack, 0x0c0, 0x0040),
        new(TorizoTilePage.LeftAttack, 0x100, 0x0040),
        new(TorizoTilePage.RightAttack, 0x100, 0x0040),
        new(TorizoTilePage.LeftAttack, 0x100, 0x0040),
        new(TorizoTilePage.RightAttack, 0x100, 0x0040),
        new(TorizoTilePage.LeftAttack, 0x0c0, 0x0040),
        new(TorizoTilePage.RightAttack, 0x0c0, 0x0040),
        new(TorizoTilePage.LeftAttack, 0x080, 0x0040),
        new(TorizoTilePage.RightAttack, 0x080, 0x0040),
        new(TorizoTilePage.GoldenLeftAttack, 0x000, 0x0040),
        new(TorizoTilePage.GoldenRightAttack, 0x000, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x000, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x040, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x080, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x0c0, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x000, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x040, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x080, 0x0040),
        new(TorizoTilePage.StatueCrumble, 0x0c0, 0x0040),
    ];

    /// <summary>Each swap's characters, in entry order (#1392, #1406).</summary>
    internal static IReadOnlyList<SpriteArtworkSource> Sources { get; } = [.. Entries.Select((_, index) => new TorizoArtworkSwapCharacters(index))];
}
