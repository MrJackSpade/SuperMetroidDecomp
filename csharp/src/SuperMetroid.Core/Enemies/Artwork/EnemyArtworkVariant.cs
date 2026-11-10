using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// One installed variant of an enemy, or of the objects one owner spawns (#1392, #1599): an enemy shown in other colours
/// is a distinct enemy with a sheet of its own, pre-coloured in those colours and every shade it shows (#1512), together
/// with the encounters whose looks change its frames (#1538) and where a rotting corpse lies in its pictures (#1539). The
/// variant is known by its name, which the installation's populations name it by.
/// </summary>
/// <typeparam name="TSheet">The variant's installed sheet: an enemy's <see cref="SpriteSheet"/>, or the <see cref="SpawnedObjectSheet"/> of an owner's spawned objects.</typeparam>
public sealed class EnemyArtworkVariant<TSheet>
    where TSheet : class
{
    /// <summary>The encounters whose looks change each of the sheet's frames, in encounter order, by frame name.</summary>
    internal readonly Dictionary<string, EncounterArtwork[]> changingEncounters;

    /// <summary>Where a rotting corpse lies in each of the sheet's pictures that shows one, by picture name.</summary>
    internal readonly Dictionary<string, CorpsePicturePlacement> corpsePictures;

    /// <summary>
    /// The variant named <paramref name="name"/>: <paramref name="sheet"/>, the encounters whose looks change each of its
    /// frames, and where a rotting corpse lies in each of its pictures that shows one.
    /// </summary>
    /// <param name="name">The variant's name.</param>
    /// <param name="sheet">The variant's installed sheet.</param>
    /// <param name="changingEncounters">
    /// The encounters whose looks change each frame, in encounter order, by frame name: the sheet holds the frame's finished
    /// picture of each look they show while it is drawn. A frame no encounter's look changes is absent and has one picture.
    /// </param>
    /// <param name="corpsePictures">Where a rotting corpse lies in each picture that shows one, by picture name.</param>
    public EnemyArtworkVariant(string name, TSheet sheet, IReadOnlyDictionary<string, EncounterArtwork[]> changingEncounters,
        IReadOnlyDictionary<string, CorpsePicturePlacement> corpsePictures)
    {
        Name = Ensure.NotNull(name);
        Sheet = Ensure.NotNull(sheet);
        this.changingEncounters = new Dictionary<string, EncounterArtwork[]>(Ensure.NotNull(changingEncounters), StringComparer.Ordinal);
        this.corpsePictures = new Dictionary<string, CorpsePicturePlacement>(Ensure.NotNull(corpsePictures), StringComparer.Ordinal);
    }

    /// <summary>The variant's name.</summary>
    public string Name { get; }

    /// <summary>The variant's installed sheet.</summary>
    public TSheet Sheet { get; }

    /// <summary>
    /// The encounters whose looks change frame <paramref name="frame"/> of the sheet (#1538), in encounter order: the sheet
    /// holds the frame's finished picture of each look they show while it is drawn. None for a frame no encounter's look
    /// changes, which has one picture.
    /// </summary>
    public IReadOnlyList<EncounterArtwork> ChangingEncounters(string frame) =>
        changingEncounters.TryGetValue(frame, out EncounterArtwork[]? changing) ? changing : [];

    /// <summary>
    /// Where a rotting corpse lies in picture <paramref name="picture"/> of the sheet (#1539), which the picture is drawn
    /// whole through as the corpse's own rot leaves it; none for a picture that shows no rotting corpse.
    /// </summary>
    public CorpsePicturePlacement? CorpsePlacement(string picture) =>
        corpsePictures.TryGetValue(picture, out CorpsePicturePlacement? placement) ? placement : null;
}
