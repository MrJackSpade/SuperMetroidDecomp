using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Every artwork swap the Bomb and Golden Torizo instruction lists make (#1406): which page of installed characters
/// each shows, and over which OBJ characters.
/// </summary>
internal static class TorizoArtworkSwapDefinitions
{
    /// <summary>The bytes one four-bit OBJ character holds.</summary>
    private const int CharacterBytes = 32;

    /// <summary>The swaps in instruction-list order: each one's page, the byte of the page it starts at, its bytes and its first OBJ character.</summary>
    internal static readonly TorizoArtworkSwapDefinition[] Entries =
    [
        new(TorizoTilePage.LeftAttack, 0x000, 0x0040, 0x130),
        new(TorizoTilePage.RightAttack, 0x000, 0x0040, 0x140),
        new(TorizoTilePage.LeftAttack, 0x040, 0x0020, 0x1e7),
        new(TorizoTilePage.RightAttack, 0x040, 0x0020, 0x1f7),
        new(TorizoTilePage.LeftAttack, 0x060, 0x0020, 0x1e5),
        new(TorizoTilePage.RightAttack, 0x060, 0x0020, 0x1f5),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0040, 0x1c8),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0040, 0x1ca),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0040, 0x1d8),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0040, 0x1da),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0040, 0x1e8),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0040, 0x1f8),
        new(TorizoTilePage.SharedDeath, 0x000, 0x0020, 0x19f),
        new(TorizoTilePage.StatueCrumble, 0x000, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x040, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x080, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x0c0, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x000, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x040, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x080, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x0c0, 0x0040, 0x1d8),
        new(TorizoTilePage.GoldenAwakening, 0x000, 0x0600, 0x0d0),
        new(TorizoTilePage.StatueCrumble, 0x000, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x040, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x080, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x0c0, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x000, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x040, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x080, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x0c0, 0x0040, 0x1d8),
        new(TorizoTilePage.LeftAttack, 0x080, 0x0040, 0x130),
        new(TorizoTilePage.RightAttack, 0x080, 0x0040, 0x140),
        new(TorizoTilePage.LeftAttack, 0x0c0, 0x0040, 0x130),
        new(TorizoTilePage.RightAttack, 0x0c0, 0x0040, 0x140),
        new(TorizoTilePage.LeftAttack, 0x100, 0x0040, 0x130),
        new(TorizoTilePage.RightAttack, 0x100, 0x0040, 0x140),
        new(TorizoTilePage.LeftAttack, 0x100, 0x0040, 0x130),
        new(TorizoTilePage.RightAttack, 0x100, 0x0040, 0x140),
        new(TorizoTilePage.LeftAttack, 0x0c0, 0x0040, 0x130),
        new(TorizoTilePage.RightAttack, 0x0c0, 0x0040, 0x140),
        new(TorizoTilePage.LeftAttack, 0x080, 0x0040, 0x130),
        new(TorizoTilePage.RightAttack, 0x080, 0x0040, 0x140),
        new(TorizoTilePage.GoldenLeftAttack, 0x000, 0x0040, 0x130),
        new(TorizoTilePage.GoldenRightAttack, 0x000, 0x0040, 0x140),
        new(TorizoTilePage.StatueCrumble, 0x000, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x040, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x080, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x0c0, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x000, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x040, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x080, 0x0040, 0x1d8),
        new(TorizoTilePage.StatueCrumble, 0x0c0, 0x0040, 0x1d8),
    ];

    /// <summary>Each swap's characters over the OBJ characters, in entry order (#1392, #1406).</summary>
    internal static IReadOnlyList<RuntimeCharacterUpload> Uploads { get; } = [.. Entries.Select((definition, index) =>
        new RuntimeCharacterUpload(new TorizoArtworkSwapCharacters(index), definition.FirstCharacter * CharacterBytes, definition.ByteCount))];
}
