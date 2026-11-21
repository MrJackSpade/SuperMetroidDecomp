using System.Collections.Immutable;

namespace SuperMetroid.AssetExtraction.Bosses.Torizo;

/// <summary>The installed Torizo character pages: each page's size and file; where it comes from in the cartridge is extraction's knowledge.</summary>
internal static class TorizoSwapArtworkDefinitions
{
    /// <summary>Shared death/recovery characters at $AA:B0A5.</summary>
    internal static readonly TorizoTileSheet SharedDeath =
        new(TorizoTilePage.SharedDeath, 64, "torizo-shared-death-tiles.png");

    /// <summary>Alternating statue-crumble characters at $AA:B279..B378.</summary>
    internal static readonly TorizoTileSheet StatueCrumble =
        new(TorizoTilePage.StatueCrumble, 256, "torizo-statue-crumble-tiles.png");

    /// <summary>Left-facing attack/death characters at $AA:B479..B5B8.</summary>
    internal static readonly TorizoTileSheet LeftAttack =
        new(TorizoTilePage.LeftAttack, 320, "torizo-left-attack-tiles.png");

    /// <summary>Right-facing attack/death characters at $AA:B679..B7B8.</summary>
    internal static readonly TorizoTileSheet RightAttack =
        new(TorizoTilePage.RightAttack, 320, "torizo-right-attack-tiles.png");

    /// <summary>Golden Torizo's initial character upload at $AF:E200..E7FF.</summary>
    internal static readonly TorizoTileSheet GoldenAwakening =
        new(TorizoTilePage.GoldenAwakening, 1536, "golden-torizo-awakening-tiles.png");

    /// <summary>Golden Torizo's alternate left-side attack characters at $AF:C800.</summary>
    internal static readonly TorizoTileSheet GoldenLeftAttack =
        new(TorizoTilePage.GoldenLeftAttack, 64, "golden-torizo-left-attack-tiles.png");

    /// <summary>Golden Torizo's alternate right-side attack characters at $AF:CA00.</summary>
    internal static readonly TorizoTileSheet GoldenRightAttack =
        new(TorizoTilePage.GoldenRightAttack, 64, "golden-torizo-right-attack-tiles.png");

    /// <summary>PLM-uploaded Bomb Torizo Chozo fragments, <c>Tiles_BombTorizosCrumblingChozo</c> at $AD:B200.</summary>
    internal static readonly TorizoTileSheet ChozoDebris =
        new(TorizoTilePage.ChozoDebris, 1024, "torizo-chozo-debris-tiles.png");

    /// <summary>The eight tile pages in stable installed-manifest and hash order.</summary>
    internal static readonly ImmutableArray<TorizoTileSheet> All =
    [
        SharedDeath, StatueCrumble, LeftAttack, RightAttack, GoldenAwakening, GoldenLeftAttack, GoldenRightAttack, ChozoDebris,
    ];
}
