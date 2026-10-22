using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy presentation state: the bound VRAM and CGRAM, boss and gradual palette effects, the
/// host-installed color and artwork catalogs, and installed-spritemap drawing.
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

    /// <summary>The bound CGRAM's enemy and boss ranges.</summary>
    internal EnemyPalettes Palettes => new(Cgram);

    /// <summary>The bound CGRAM's hardware banks and rows, for actors whose palette row is selected at run time.</summary>
    internal CgramPalettes CgramRows => new(Cgram);

    /// <summary>The room's bound VRAM; enemy artwork transfers require the room's video memory to be bound.</summary>
    internal SnesVram Vram
    {
        get => field ?? throw new InvalidOperationException("Enemy presentation has no bound VRAM; the room's video memory must be bound first.");
        private set;
    }

    /// <summary>The room's bound CGRAM; enemy palette writes require the room's video memory to be bound.</summary>
    internal SnesCgram Cgram
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

    /// <summary>Room load: binds the room's background artwork and the VRAM it composes over, which enemy artwork transfers write.</summary>
    internal void BindVram(BgArtworkBindings backgrounds)
    {
        Backgrounds = Ensure.NotNull(backgrounds);
        Vram = backgrounds.Vram;
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
    internal void BindCgram(SnesCgram cgram) => Cgram = Ensure.NotNull(cgram);
    /// <summary>
    /// The session's gameplay OBJ character table bindings, which enemy-format sprites draw from. Uploads that bypass
    /// the NMI queue (Draygon's Evir sheet, Crocomire's skeleton chunks) record themselves here.
    /// </summary>
    internal ObjCharacterUploads ObjCharacters => _room.Session.ObjCharacters;

    /// <summary>The rot step each region the room's rotting corpses upload shows (#1392).</summary>
    internal CorpseRotUploads RotUploads { get; } = new();

    /// <summary>
    /// The session's shared <c>PaletteChangeNumerator</c>. Enemies and the runtime's door and death
    /// fades advance this one counter, as the cartridge's fades all advance one WRAM word.
    /// </summary>
    public GradualColorChangeCounter GradualColorChange => _room.Session.GradualColorChange;

    /// <summary>Publishes an installed enemy composition over the OBJ page, or accepts the compiled common empty frame.</summary>
    /// <remarks>A nonempty frame without installed display art is an asset error rather than a cartridge fallback.</remarks>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="objects">The gameplay OBJ character page.</param>
    /// <param name="source">The enemy shown.</param>
    /// <param name="draw">The composition, its origin on screen, palette bits and packed tile base.</param>
    internal void PublishEnemySpritemap(SpriteFrameBuffer sprites, SpriteArtwork objects, SpriteSource source, EnemySpritemapDraw draw)
    {
        // The common empty frame is compiled definition data, not missing artwork.
        if (draw.Frame == EnemyFrameId.Empty)
            return;
        if (_room.Artwork?.Spritemaps?.TryGetDisplay(draw.Frame, out EnemySpritemapParts installed) == true)
        {
            ushort paletteBits = checked((ushort)draw.PaletteBits);
            PublishParts(sprites, objects, source, EnemySpriteFrames.Frame(installed, checked((ushort)draw.BaseTileIndex), paletteBits),
                new Position16<ScreenPixels>(draw.Origin.X, draw.Origin.Y), paletteBits);
            return;
        }
        throw new InvalidDataException($"Enemy frame {draw.Frame} requires an installed display composition.");
    }

    /// <summary>
    /// Publishes one frame of <paramref name="enemy"/>'s installed sheet (#1392): the sheet its room's graphics set
    /// names. While the draw's palette bits are not those the set loaded the enemy with, as on a hit or freeze flash or
    /// after its AI moves it to another row, the frame shows them ORed into each part's own row.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The actor shown.</param>
    /// <param name="enemy">The enemy whose sheet is drawn.</param>
    /// <param name="frame">The sheet frame's name.</param>
    /// <param name="origin">The frame origin on screen.</param>
    /// <param name="paletteBits">The draw's palette selection.</param>
    /// <param name="tileBase">The draw's packed tile base, which must be the one its sheet was composed at.</param>
    /// <exception cref="InvalidDataException">The enemy draws at another tile base than its sheet's.</exception>
    internal void PublishSheetFrame(SpriteFrameBuffer sprites, SpriteSource source, EnemyDefinitionId enemy, string frame,
        Position16<ScreenPixels> origin, int paletteBits, int tileBase)
    {
        EnemyGraphicsIndexes composed = _graphicsSet.DrawIndexes(enemy);
        if (tileBase != composed.TileIndex)
            throw new InvalidDataException($"{enemy} draws at tile base ${tileBase:X3}; its sheet was composed at ${composed.TileIndex:X3}.");
        int loaded = SnesObjAttributeWord.FromPaletteBits(checked((ushort)composed.PaletteIndex)).PaletteIndex;
        int drawn = SnesObjAttributeWord.FromPaletteBits(checked((ushort)paletteBits)).PaletteIndex;
        SpriteSheet sheet = Sheets.For(LoadedSet, enemy);
        foreach (string picture in Pictures(frame, Sheets.Layers(LoadedSet, enemy, frame)))
            sprites.Add(new ImageSpriteDraw(source, sheet.Frame(picture), origin, SpriteMirror.None, drawn == loaded ? null : drawn));
    }

    /// <summary>
    /// Whether the loaded graphics set's spawned objects (enemy projectiles, room sprite objects and the Ceres elevator's
    /// platforms) draw from pre-coloured sheets (#1392); a set loading an enemy drawn from characters it uploads keeps
    /// them on characters.
    /// </summary>
    internal bool SpawnsFromSheets => Sheets.SpawnsFromSheets(LoadedSet);

    /// <summary>
    /// Publishes one spawned object's frame from the loaded graphics set's sheet for the tile base it draws at (#1392).
    /// While the draw's palette bits are not those the sheet was coloured with, the frame shows them ORed into each
    /// part's own row, as the cartridge ORs them into each part.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The object shown.</param>
    /// <param name="frame">The sheet frame's name.</param>
    /// <param name="origin">The frame origin on screen.</param>
    /// <param name="tileBase">The packed tile base the object's parts draw at.</param>
    /// <param name="paletteBits">The draw's palette selection.</param>
    /// <exception cref="InvalidDataException">The set has no sheet for that tile base.</exception>
    internal void PublishSpawnedFrame(SpriteFrameBuffer sprites, SpriteSource source, string frame, Position16<ScreenPixels> origin,
        int tileBase, ushort paletteBits)
    {
        SpawnedObjectSheet sheet = Sheets.Spawned(LoadedSet, tileBase);
        int drawn = SnesObjAttributeWord.FromPaletteBits(paletteBits).PaletteIndex;
        foreach (string picture in Pictures(frame, Sheets.SpawnedLayers(LoadedSet, tileBase, frame)))
            sprites.Add(new ImageSpriteDraw(source, sheet.Sheet.Frame(picture), origin, SpriteMirror.None, drawn == sheet.Palette ? null : drawn));
    }

    /// <summary>
    /// The pictures <paramref name="frame"/> draws as, front first: the frame whole, or, for a frame drawn over characters
    /// the room's encounter uploads, the picture of each of its <paramref name="layers"/> showing what each region it reads
    /// holds now, the upload made last over it or the graphics set's own characters.
    /// </summary>
    private IEnumerable<string> Pictures(string frame, IReadOnlyList<CharacterRegion[]> layers)
    {
        if (layers.Count == 0)
            return [frame];
        RuntimeCharacterUploadCatalog catalog = Sheets.Uploads;
        IReadOnlyList<RuntimeCharacterUpload> uploads = catalog.Of(LoadedSet);
        return layers.Select((regions, layer) => catalog.PictureName(frame, layer,
            [.. regions.Select(region => RotUploads.TryShowing(region, out int step)
                ? new CharacterRegionContent(region, null, step) : catalog.Showing(ObjCharacters, uploads, region))]));
    }

    /// <summary>
    /// Publishes a frame an encounter draws itself, outside the enemy list, at character index zero (#1392): from its
    /// graphics set's character-zero spawned-object sheet while its spawned objects draw from sheets, else over the OBJ
    /// characters.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="objects">The gameplay OBJ character page.</param>
    /// <param name="source">The actor shown.</param>
    /// <param name="frame">The frame.</param>
    /// <param name="origin">The frame origin on screen.</param>
    /// <param name="paletteBits">The draw's palette selection.</param>
    internal void PublishEncounterFrame(SpriteFrameBuffer sprites, SpriteArtwork objects, SpriteSource source, EnemyFrameId frame,
        Position16<ScreenPixels> origin, int paletteBits)
    {
        if (SpawnsFromSheets)
            PublishSpawnedFrame(sprites, source, EnemySheetDefinitions.EncounterFrameName(frame), origin, 0, checked((ushort)paletteBits));
        else
            PublishEnemySpritemap(sprites, objects, source, new EnemySpritemapDraw { Frame = frame, Origin = origin.ToOam(), PaletteBits = paletteBits, BaseTileIndex = 0 });
    }

    /// <summary>The installed enemy sheets.</summary>
    private IEnemySheetSource Sheets => _room.Session.Sheets ?? throw new InvalidOperationException(
        "Enemy drawing requires the installed enemy sheets.");

    /// <summary>The graphics set the room draws under.</summary>
    private RoomEnemyGraphicsSetDefinition LoadedSet => _graphicsSet.LoadedSet;

    /// <summary>Publishes one enemy-format frame over the OBJ page.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="objects">The gameplay OBJ character page.</param>
    /// <param name="source">The actor shown.</param>
    /// <param name="frame">The whole frame, its parts' palettes and tiles already formed.</param>
    /// <param name="origin">The frame origin on screen.</param>
    /// <param name="paletteBits">The palette selection the frame was formed with.</param>
    internal static void PublishParts(SpriteFrameBuffer sprites, SpriteArtwork objects, SpriteSource source,
        SpriteFrame frame, Position16<ScreenPixels> origin, ushort paletteBits) =>
        sprites.Add(new CharacterSpriteDraw(source, frame, objects, origin, default,
            (byte)SnesObjAttributeWord.FromPaletteBits(paletteBits).PaletteIndex, 0));

}
