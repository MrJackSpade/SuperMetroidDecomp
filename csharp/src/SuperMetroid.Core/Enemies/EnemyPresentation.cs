using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy presentation state: the bound VRAM and CGRAM, boss and gradual palette effects, the
/// host-installed color and artwork catalogs, and drawing from the installed sheets.
/// </summary>
[RoomLifetime]
internal sealed class EnemyPresentation
{
    /// <summary>Room artwork binding used to resolve enemy display compositions.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>The room's loaded graphics set, which names each enemy's installed sheet.</summary>
    private readonly EnemyGraphicsSet _graphicsSet;

    /// <summary>Binds enemy presentation to its room's installed artwork catalog and loaded graphics set.</summary>
    internal EnemyPresentation(EnemyRoomBinding room, EnemyGraphicsSet graphicsSet)
    {
        _room = room;
        _graphicsSet = graphicsSet;
    }

    /// <summary>Gameplay's background colour sets (#1405).</summary>
    internal BackgroundColourSets BackgroundSets => Colours.BackgroundSets;

    /// <summary>Sprite colour set <paramref name="set"/> (#1405).</summary>
    internal ColourRun ColourSet(int set) => Colours.SpriteSets.Set(set);

    /// <summary>The sprite colour set <paramref name="slot"/>'s palette row selects (#1405).</summary>
    internal ColourRun ColourSetOf(RoomEnemySlot slot) => ColourSet(slot.PaletteRow);

    /// <summary>The room's bound CGRAM; enemy palette writes require the room's video memory to be bound.</summary>
    internal GameplayColours Colours
    {
        get => field ?? throw new InvalidOperationException("Enemy presentation has no bound CGRAM; the room's video memory must be bound first.");
        private set;
    }
    /// <summary>The room's background artwork bindings, through which enemy background characters show (#1352).</summary>
    internal BgArtworkBindings Backgrounds
    {
        get => field ?? throw new InvalidOperationException("Enemy presentation has no bound background artwork; the room's video memory must be bound first.");
        private set;
    }

    /// <summary>
    /// The frame each of the room's animations shows (#1406): a frame drawn over OBJ characters an encounter's catalogued
    /// animation paints shows the picture of the frame selected here.
    /// </summary>
    internal ArtworkFrameSelections ArtworkFrames { get; private set; } = new();

    /// <summary>
    /// Room load: binds the room's background artwork and the room's animation frames, or a fresh set no animation has
    /// selected in when <paramref name="frames"/> is null.
    /// </summary>
    internal void BindBackgrounds(BgArtworkBindings backgrounds, ArtworkFrameSelections? frames = null)
    {
        Backgrounds = Ensure.NotNull(backgrounds);
        ArtworkFrames = frames ?? new ArtworkFrameSelections();
    }

    /// <summary>
    /// The enemy BG2 image the room's boss draws its body into (#1353), once a boss that owns it attaches it; bosses
    /// not yet converted (#1354-#1357) draw into VRAM instead.
    /// </summary>
    internal EnemyBg2Image? Bg2Image { get; private set; }

    /// <summary>A boss's initialization: attaches the cleared enemy BG2 image it draws its body into.</summary>
    internal void AttachBg2Image(EnemyBg2Image image) => Bg2Image = Ensure.NotNull(image);

    /// <summary>The attached enemy BG2 image.</summary>
    internal EnemyBg2Image RequireBg2Image() =>
        Bg2Image ?? throw new InvalidOperationException("No boss has attached the enemy BG2 image.");

    /// <summary>Room load: binds the CGRAM the room's enemy palettes write.</summary>
    internal void BindColours(GameplayColours colours) => Colours = Ensure.NotNull(colours);
    /// <summary>
    /// The session's gameplay OBJ character table bindings, which enemy-format sprites draw from. Uploads that bypass
    /// the NMI queue (Draygon's Evir sheet, Crocomire's skeleton chunks) record themselves here.
    /// </summary>
    internal SpriteArtworkPlacements SpriteCharacters => _room.Session.SpriteCharacters;

    /// <summary>Shows an encounter's <paramref name="source"/> over the sprite characters (#1406).</summary>
    internal void ShowCharacters(SpriteArtworkSource source) => _room.Session.ShowCharacters(source);

    /// <summary>The rot step each region Mother Brain's rotting corpse uploads shows (#1392).</summary>
    internal CorpseRotSteps RotSteps { get; } = new();

    /// <summary>Records that Mother Brain's corpse shows its row <paramref name="row"/> as rot step <paramref name="step"/> left it (#1406).</summary>
    internal void ShowRotRow(int row, int step) =>
        RotSteps.Show(Sheets.Coverage(LoadedSet).RotRow(row), step, SpriteCharacters.Recorded);

    /// <summary>
    /// The session's shared <c>PaletteChangeNumerator</c>. Enemies and the runtime's door and death
    /// fades advance this one counter, as the cartridge's fades all advance one WRAM word.
    /// </summary>
    public GradualColorChangeCounter GradualColorChange => _room.Session.GradualColorChange;

    /// <summary>
    /// Publishes one frame of the enemy <paramref name="draw"/> shows from its installed sheet (#1392): the sheet its room's graphics set
    /// names. While the draw's palette row is not the one the set loaded the enemy with, as on a hit or freeze flash or
    /// after its AI moves it to another row, the frame shows it ORed into each part's own row.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The actor shown.</param>
    /// <param name="draw">The frame drawn: the enemy, its sheet frame, origin, palette selection and rot.</param>
    /// <exception cref="InvalidDataException">A frame showing a corpse's rot is drawn without that rot.</exception>
    internal void PublishSheetFrame(SpriteFrameBuffer sprites, SpriteSource source, EnemySheetDraw draw)
    {
        (EnemyDefinitionId enemy, string frame, Position16<ScreenPixels> origin, int drawn, CorpseRotView? rot) = draw;
        int loaded = _graphicsSet.DrawIndexes(enemy).PaletteRow;
        SpriteSheet sheet = Sheets.For(LoadedSet, enemy);
        foreach (string picture in Pictures(frame, Sheets.Layers(LoadedSet, enemy, frame), Sheets.RotCorpse(LoadedSet, enemy, frame), rot))
            sprites.Add(new ImageSpriteDraw(source, sheet.Frame(picture), origin, SpriteMirror.None, drawn == loaded ? null : drawn));
    }

    /// <summary>
    /// Publishes one spawned object's frame from the loaded graphics set's sheet for the tile base it draws at (#1392).
    /// While the draw's palette row is not the one the sheet was coloured with, the frame shows it ORed into each
    /// part's own row, as the cartridge ORs it into each part.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The object shown.</param>
    /// <param name="frame">The sheet frame's name.</param>
    /// <param name="origin">The frame origin on screen.</param>
    /// <param name="tileBase">The tile base the object's parts draw at.</param>
    /// <param name="drawn">The palette row the draw selects.</param>
    /// <param name="rot">The rot the drawing encounter selected, which a frame drawn over a rotting corpse's rows shows; none for one that shows none.</param>
    /// <exception cref="InvalidDataException">
    /// The set has no sheet for that tile base, or a frame showing a corpse's rot is drawn without that rot.
    /// </exception>
    internal void PublishSpawnedFrame(SpriteFrameBuffer sprites, SpriteSource source, string frame, Position16<ScreenPixels> origin,
        int tileBase, int drawn, CorpseRotView? rot = null)
    {
        SpawnedObjectSheet sheet = Sheets.Spawned(LoadedSet, tileBase);
        foreach (string picture in Pictures(frame, Sheets.SpawnedLayers(LoadedSet, tileBase, frame),
                     Sheets.SpawnedRotCorpse(LoadedSet, tileBase, frame), rot))
            sprites.Add(new ImageSpriteDraw(source, sheet.Sheet.Frame(picture), origin, SpriteMirror.None, drawn == sheet.Palette ? null : drawn));
    }

    /// <summary>
    /// The pictures <paramref name="frame"/> draws as, front first: the frame whole; for a frame drawn over a rotting
    /// corpse's rows, its picture of the <paramref name="rot"/> its draw selected (#1406); or, for a frame drawn over
    /// characters the room's encounter uploads, the picture of each of its <paramref name="layers"/> showing what each region
    /// it reads holds now, the upload made last over it or the graphics set's own characters.
    /// </summary>
    /// <exception cref="InvalidDataException">The frame shows a corpse's rot and the draw selects none, or another corpse's.</exception>
    private IEnumerable<string> Pictures(string frame, IReadOnlyList<CharacterRegion[]> layers, string? rotCorpse, CorpseRotView? rot)
    {
        if (rotCorpse is not null)
        {
            if (rot is null)
                throw new InvalidDataException($"Sheet frame {frame} shows the rot of {rotCorpse}, but its draw selects no rot.");
            if (rot.Corpse.Name != rotCorpse)
                throw new InvalidDataException($"Sheet frame {frame} shows the rot of {rotCorpse}, but its draw selects the rot of {rot.Corpse.Name}.");
            return [rot.PictureName(frame)];
        }
        if (layers.Count == 0)
            return [frame];
        EncounterCharacterCatalog catalog = Sheets.Encounters;
        IReadOnlyList<SpriteArtworkPlacement> uploads = catalog.Of(LoadedSet);
        SetCharacterCoverage coverage = Sheets.Coverage(LoadedSet);
        return layers.Select((regions, layer) => catalog.PictureName(frame, layer,
            [.. regions.Select(region => RotSteps.TryShowing(region, out int step, out long stamp)
                    && !SpriteCharacters.ShownAfter(stamp).Any(shown => coverage.Covers(shown, region))
                ? new CharacterRegionContent(region, null, step) : catalog.Showing(SpriteCharacters, uploads, region, ArtworkFrames, coverage))]));
    }

    /// <summary>
    /// Publishes a frame an encounter draws itself, outside the enemy list, at character index zero (#1392): from its
    /// graphics set's character-zero spawned-object sheet.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The actor shown.</param>
    /// <param name="frame">The frame.</param>
    /// <param name="origin">The frame origin on screen.</param>
    /// <param name="palette">The palette row the draw selects.</param>
    /// <param name="rot">The rot the encounter selected, which a frame drawn over its rotting rows shows; none for one that shows none.</param>
    internal void PublishEncounterFrame(SpriteFrameBuffer sprites, SpriteSource source, EnemyFrameId frame,
        Position16<ScreenPixels> origin, int palette, CorpseRotView? rot = null) =>
        PublishSpawnedFrame(sprites, source, EnemySheetDefinitions.EncounterFrameName(frame), origin, 0, palette, rot);

    /// <summary>The installed enemy sheets.</summary>
    private IEnemySheetSource Sheets => _room.Session.Sheets ?? throw new InvalidOperationException(
        "Enemy drawing requires the installed enemy sheets.");

    /// <summary>The graphics set the room draws under.</summary>
    private RoomEnemyGraphicsSetDefinition LoadedSet => _graphicsSet.LoadedSet;

}
