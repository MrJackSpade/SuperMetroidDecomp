namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// One family of installed enemy sheets (#1392, #1513, #1531): the sheets by key, how each of their frames an encounter's
/// artwork sequence changes follows it, and where a rotting corpse lies in each frame that shows one.
/// </summary>
/// <typeparam name="TKey">What selects a sheet.</typeparam>
/// <typeparam name="TSheet">The installed sheet.</typeparam>
/// <param name="Sheets">The sheets by key.</param>
/// <param name="Variations">How each frame of each sheet follows the sequences that change it, by frame name.</param>
/// <param name="CorpsePictures">Where a rotting corpse lies in each frame of each sheet that shows one, by frame name.</param>
/// <param name="BlankPictures">
/// Each sheet's frames' pictures that show nothing, which it does not hold: pictures whose every character the frame reads is
/// blank, as an encounter's artwork sequence or the room's load leaves them.
/// </param>
public sealed record EnemySheetGroup<TKey, TSheet>(
    IReadOnlyDictionary<TKey, TSheet> Sheets,
    IReadOnlyDictionary<TKey, IReadOnlyDictionary<string, FrameArtworkVariation[]>> Variations,
    IReadOnlyDictionary<TKey, IReadOnlyDictionary<string, CorpsePictureLayout>> CorpsePictures,
    IReadOnlyDictionary<TKey, IReadOnlySet<string>> BlankPictures)
    where TKey : notnull;
