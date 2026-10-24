namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// One family of installed enemy sheets (#1392, #1406): the sheets by key, the regions each of their frames drawn over
/// uploaded characters draws as, and the kind of corpse whose rot each frame drawn over a rotting corpse's rows shows.
/// </summary>
/// <typeparam name="TKey">What selects a sheet.</typeparam>
/// <typeparam name="TSheet">The installed sheet.</typeparam>
/// <param name="Sheets">The sheets by key.</param>
/// <param name="Pictures">The regions each frame of each sheet draws as, by frame name.</param>
/// <param name="RotPictures">The corpse each frame of each sheet shows the rot of, by frame name.</param>
public sealed record EnemySheetGroup<TKey, TSheet>(
    IReadOnlyDictionary<TKey, TSheet> Sheets,
    IReadOnlyDictionary<TKey, IReadOnlyDictionary<string, CharacterRegion[][]>> Pictures,
    IReadOnlyDictionary<TKey, IReadOnlyDictionary<string, string>> RotPictures)
    where TKey : notnull;
