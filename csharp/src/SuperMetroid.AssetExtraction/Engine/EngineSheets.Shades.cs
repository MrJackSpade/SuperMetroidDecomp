namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Pictures of what an enemy spawns, in one of their sheet's shades.</summary>
internal sealed partial class EngineSheets
{
    /// <summary>
    /// Cuts named pictures of what an enemy spawns as one of their sheets' shades shows them, under
    /// <paramref name="folder"/>. Given the same pictures as <see cref="CutSpawned"/>, they are of the same size and
    /// lie in the same place as the ones it cuts.
    /// </summary>
    /// <exception cref="InvalidDataException">A picture is on no sheet, or its sheet has no such shade.</exception>
    internal EngineSheetPictures CutSpawnedShade(EngineAssetTree tree, string folder, string spawner, IEnumerable<string> frames, string shade)
    {
        var placed = new List<(string Name, string Sheet)>();
        foreach (string frame in frames.Distinct(StringComparer.Ordinal))
            placed.Add((frame, SheetHolding(spawner, frame)));
        return CutFrames(tree, EnemyFamily, folder, placed, shade);
    }
}
