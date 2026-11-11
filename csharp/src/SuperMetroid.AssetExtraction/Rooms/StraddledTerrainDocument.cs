namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed straddled-terrain file (#1612): the terrain a Wave shot's scan past the room's ceiling finds where it
/// lands halfway through a block, decoded once by the import from the level-word codec. Leads and tails are opaque keys
/// the import chooses; the game only looks them up.
/// </summary>
/// <param name="Version">The schema revision, <see cref="StraddledTerrainFormat.Version"/>.</param>
/// <param name="Leads">
/// The lead the end of each first block contributes: by the name of its collision kind, then by the name of its mirroring,
/// then by its block number, 0 through 1023.
/// </param>
/// <param name="Tails">The tail the start of each second block contributes, by its block number, 0 through 1023.</param>
/// <param name="Outcomes">The terrain each lead and tail make, by lead and then by tail.</param>
internal sealed record StraddledTerrainDocument(
    int Version,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<int>>> Leads,
    IReadOnlyList<int> Tails,
    IReadOnlyList<IReadOnlyList<StraddledTerrainDocument.Outcome>> Outcomes)
{
    /// <summary>One straddled terrain block.</summary>
    /// <param name="Block">The block shown, 0 through 1023.</param>
    /// <param name="Mirroring">The name of how the block is mirrored.</param>
    /// <param name="Collision">The name of the kind the block collides as.</param>
    internal sealed record Outcome(int Block, string Mirroring, string Collision);
}
