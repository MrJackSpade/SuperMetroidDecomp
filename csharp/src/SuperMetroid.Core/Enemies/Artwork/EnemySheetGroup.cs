using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// One family of installed enemy sheets (#1392, #1538, #1539): the sheets by key, the encounters whose looks change each of
/// their frames, and where a rotting corpse lies in each frame that shows one.
/// </summary>
/// <typeparam name="TKey">What selects a sheet.</typeparam>
/// <typeparam name="TSheet">The installed sheet.</typeparam>
/// <param name="Sheets">The sheets by key.</param>
/// <param name="ChangingEncounters">
/// The encounters whose looks change each frame of each sheet, in encounter order, by frame name: the sheet holds the
/// frame's finished picture of each look they show while it is drawn.
/// </param>
/// <param name="CorpsePictures">Where a rotting corpse lies in each frame of each sheet that shows one, by frame name.</param>
public sealed record EnemySheetGroup<TKey, TSheet>(
    IReadOnlyDictionary<TKey, TSheet> Sheets,
    IReadOnlyDictionary<TKey, IReadOnlyDictionary<string, EncounterArtwork[]>> ChangingEncounters,
    IReadOnlyDictionary<TKey, IReadOnlyDictionary<string, CorpsePicturePlacement>> CorpsePictures)
    where TKey : notnull;
