namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>The installed Torizo character pages: each page's size and file; where it comes from in the cartridge is extraction's knowledge.</summary>
internal static class TorizoSwapArtworkDefinitions
{
    /// <summary>Shared death/recovery characters at $AA:B0A5.</summary>
    internal static readonly TorizoTileSheet SharedDeath =
        new(TorizoTilePage.SharedDeath, 0x0040, "torizo-shared-death-tiles.png");

    /// <summary>Alternating statue-crumble characters at $AA:B279..B378.</summary>
    internal static readonly TorizoTileSheet StatueCrumble =
        new(TorizoTilePage.StatueCrumble, 0x0100, "torizo-statue-crumble-tiles.png");

    /// <summary>Left-facing attack/death characters at $AA:B479..B5B8.</summary>
    internal static readonly TorizoTileSheet LeftAttack =
        new(TorizoTilePage.LeftAttack, 0x0140, "torizo-left-attack-tiles.png");

    /// <summary>Right-facing attack/death characters at $AA:B679..B7B8.</summary>
    internal static readonly TorizoTileSheet RightAttack =
        new(TorizoTilePage.RightAttack, 0x0140, "torizo-right-attack-tiles.png");

    /// <summary>Golden Torizo's initial character upload at $AF:E200..E7FF.</summary>
    internal static readonly TorizoTileSheet GoldenAwakening =
        new(TorizoTilePage.GoldenAwakening, 0x0600, "golden-torizo-awakening-tiles.png");

    /// <summary>Golden Torizo's alternate left-side attack characters at $AF:C800.</summary>
    internal static readonly TorizoTileSheet GoldenLeftAttack =
        new(TorizoTilePage.GoldenLeftAttack, 0x0040, "golden-torizo-left-attack-tiles.png");

    /// <summary>Golden Torizo's alternate right-side attack characters at $AF:CA00.</summary>
    internal static readonly TorizoTileSheet GoldenRightAttack =
        new(TorizoTilePage.GoldenRightAttack, 0x0040, "golden-torizo-right-attack-tiles.png");

    /// <summary>PLM-uploaded Bomb Torizo Chozo fragments, <c>Tiles_BombTorizosCrumblingChozo</c> at $AD:B200.</summary>
    internal static readonly TorizoTileSheet ChozoDebris =
        new(TorizoTilePage.ChozoDebris, 0x0400, "torizo-chozo-debris-tiles.png");

    /// <summary>Enumerates the eight tile pages in stable installed-manifest and hash order.</summary>
    internal static PageSequence All => default;

    /// <summary>Indexable view of the fixed set of Torizo instruction tile sources.</summary>
    internal readonly struct PageSequence : IReadOnlyList<TorizoTileSheet>
    {
        /// <summary>Gets the number of required tile sheets.</summary>
        public int Count => 8;
        /// <summary>Gets the sequence length for callers using array-style access.</summary>
        public int Length => Count;
        /// <summary>Gets the tile sheet assigned to a stable page-role index.</summary>
        public TorizoTileSheet this[int index] => index switch
        {
            0 => SharedDeath,
            1 => StatueCrumble,
            2 => LeftAttack,
            3 => RightAttack,
            4 => GoldenAwakening,
            5 => GoldenLeftAttack,
            6 => GoldenRightAttack,
            7 => ChozoDebris,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Yields every required tile sheet in page-role order.</summary>
        public IEnumerator<TorizoTileSheet> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same page-role order to non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
