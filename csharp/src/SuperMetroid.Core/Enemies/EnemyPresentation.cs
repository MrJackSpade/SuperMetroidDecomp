using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy presentation state: the room's appearance and animation frames, the boss body layer, the shared gradual-change
/// counter, and drawing from the installed pre-coloured sheets under each draw's shading (#1512).
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

    /// <summary>What gameplay shows over the room's load (#1466), as Kraid's rise shows his backdrop.</summary>
    internal RoomAppearance Appearance
    {
        get => field ?? throw new InvalidOperationException("Enemy presentation has no bound room appearance; the room must be bound first.");
        private set;
    }

    /// <summary>
    /// The frame each of the room's animations shows (#1406), which an encounter's animated looks, such as Crocomire's
    /// melting bands, select their pictures by.
    /// </summary>
    internal ArtworkFrameSelections ArtworkFrames { get; private set; } = new();

    /// <summary>
    /// Room load: binds what gameplay shows over the room's backgrounds and the room's animation frames, or a fresh set no
    /// animation has selected in when <paramref name="frames"/> is null.
    /// </summary>
    internal void BindBackgrounds(RoomAppearance appearance, ArtworkFrameSelections? frames = null)
    {
        Appearance = Ensure.NotNull(appearance);
        ArtworkFrames = frames ?? new ArtworkFrameSelections();
    }

    /// <summary>
    /// The body layer the room's boss draws its body on (#1353, #1466), once a boss that shows one attaches it; Kraid
    /// publishes his own.
    /// </summary>
    internal BossBodyCanvas? BossBody { get; private set; }

    /// <summary>A boss's initialization: attaches the cleared body layer it draws its body on.</summary>
    internal void AttachBossBody(BossBodyCanvas body) => BossBody = Ensure.NotNull(body);

    /// <summary>The attached body layer.</summary>
    /// <exception cref="InvalidOperationException">No boss has attached one.</exception>
    internal BossBodyCanvas RequireBossBody() =>
        BossBody ?? throw new InvalidOperationException("No boss has attached its body layer.");

    /// <summary>How far the room's encounters have played their artwork sequences, which their frames' pictures follow (#1513).</summary>
    internal EncounterArtworkProgress ArtworkProgress => _room.Session.ArtworkProgress;

    /// <summary>Advances the encounter playing <paramref name="sequence"/> to step <paramref name="step"/> at this update (#1513).</summary>
    internal void ShowArtwork(EncounterArtworkSequence sequence, int step) => ArtworkProgress.Show(sequence, step);

    /// <summary>
    /// Advances the encounter playing <paramref name="sequence"/>, whose steps show <paramref name="order"/>, to the step
    /// showing <paramref name="look"/> at this update (#1513).
    /// </summary>
    internal void ShowArtwork<TLook>(EncounterArtworkSequence sequence, IReadOnlyList<TLook> order, TLook look)
        where TLook : IEquatable<TLook> => ArtworkProgress.Show(sequence, order, look);

    /// <summary>
    /// The session's shared <c>PaletteChangeNumerator</c>. Enemies and the runtime's door and death
    /// fades advance this one counter, as the game's fades all advance one shared counter.
    /// </summary>
    public GradualColorChangeCounter GradualColorChange => _room.Session.GradualColorChange;

    /// <summary>
    /// Publishes one frame of the enemy <paramref name="draw"/> shows from its installed sheet (#1392): the sheet its room's
    /// graphics set names, pre-coloured in the colours the set loads for the enemy, under the shading the draw names
    /// (#1512), such as a hit flash or a glow's frame. A frame that shows a rotting corpse is drawn through the corpse's
    /// rows as its rot leaves them (#1531).
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The actor shown.</param>
    /// <param name="draw">The frame drawn: the enemy, its sheet frame, origin, shading and the corpse's rows.</param>
    /// <exception cref="InvalidDataException">A rotting corpse's frame is not as its corpse's rows say.</exception>
    internal void PublishSheetFrame(SpriteFrameBuffer sprites, SpriteSource source, EnemySheetDraw draw)
    {
        (EnemyDefinitionId enemy, string frame, Position16<ScreenPixels> origin) = draw;
        PictureShading shading = draw.Shading ?? PictureShading.Standard;
        SpriteSheet sheet = Sheets.For(LoadedSet, enemy);
        string picture = Picture(frame, Sheets.Variations(LoadedSet, enemy, frame));
        // A picture whose every character the frame reads is blank, as the room's load or an encounter's artwork sequence
        // leaves them, shows nothing: the sheet holds none and the frame draws nothing then (#1513).
        if (Sheets.ShowsNothing(LoadedSet, enemy, picture))
            return;
        SpriteSheetFrame image = sheet.Frame(picture);
        sprites.Add(new ImageSpriteDraw(source, image, origin, SpriteMirror.None)
        {
            Shading = shading,
            CorpseRot = CorpseRot(Sheets.CorpseLayout(LoadedSet, enemy, picture), draw.RotRows, image, picture),
        });
    }

    /// <summary>
    /// Publishes one spawned object's frame from the loaded graphics set's sheet of the owner its graphics name (#1392,
    /// #1513), under the shading the object carries (#1512), through a rotting corpse's <paramref name="rows"/> when the
    /// frame shows the corpse (#1531).
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The object shown.</param>
    /// <param name="frame">The sheet frame's name.</param>
    /// <param name="origin">The frame origin on screen.</param>
    /// <param name="graphics">Whose spawned-object sheet the object draws from, and the shading it shows its picture in.</param>
    /// <param name="rows">The rows of the rotting corpse the frame shows, as its rot has left them; none for one that shows none.</param>
    /// <exception cref="InvalidDataException">The set has no sheet for that owner, or a rotting corpse's frame is not as its rows say.</exception>
    internal void PublishSpawnedFrame(SpriteFrameBuffer sprites, SpriteSource source, string frame, Position16<ScreenPixels> origin,
        SpriteGraphics graphics, CorpseRotRows? rows = null)
    {
        SpawnedObjectOwner owner = graphics.Owner;
        SpawnedObjectSheet sheet = Sheets.Spawned(LoadedSet, owner);
        string picture = Picture(frame, Sheets.SpawnedVariations(LoadedSet, owner, frame));
        // A picture whose every character the frame reads is blank shows nothing: the sheet holds none and the object draws
        // nothing then (#1513).
        if (Sheets.SpawnedShowsNothing(LoadedSet, owner, picture))
            return;
        SpriteSheetFrame image = sheet.Sheet.Frame(picture);
        sprites.Add(new ImageSpriteDraw(source, image, origin, SpriteMirror.None)
        {
            Shading = graphics.Shading,
            CorpseRot = CorpseRot(Sheets.SpawnedCorpseLayout(LoadedSet, owner, picture), rows, image, picture),
        });
    }

    /// <summary>
    /// How <paramref name="picture"/> shows the rotting corpse it lies in as <paramref name="layout"/> says, through the
    /// corpse's <paramref name="rows"/> (#1531); none for a picture that shows no rotting corpse, or before the corpse's rows
    /// rot, when the picture shows itself.
    /// </summary>
    /// <exception cref="InvalidDataException">The picture is not as the layout and the rows say.</exception>
    private static CorpseRotPicture? CorpseRot(CorpsePictureLayout? layout, CorpseRotRows? rows, SpriteSheetFrame image, string picture) =>
        layout is not null && rows is not null ? CorpseRotDrawing.Picture(layout, rows, image.Size, picture) : null;

    /// <summary>
    /// The picture <paramref name="frame"/> draws as: for a frame the room's encounters' artwork sequences change, its
    /// picture of the step each of its <paramref name="variations"/> has reached (#1513); else the frame itself.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The frame is drawn at a step of a sequence, or before a sequence's first step, at which it is never drawn, so its
    /// sheet holds no picture of it then.
    /// </exception>
    private string Picture(string frame, IReadOnlyList<FrameArtworkVariation> variations)
    {
        var shown = new List<EncounterArtworkStep>();
        foreach (FrameArtworkVariation variation in variations)
        {
            int? reached = ArtworkProgress.StepOf(variation.Sequence);
            if (reached is null && !variation.DrawnBeforeFirstStep)
                throw new InvalidDataException($"Sheet frame {frame} is drawn before {variation.Sequence} reaches its first step, when it is never drawn; its sheet holds no picture of it then.");
            if ((reached is int step ? variation.PictureAt(step, frame) : variation.PictureBeforeFirstStep) is int picture)
                shown.Add(new EncounterArtworkStep(variation.Sequence, picture));
        }
        return FrameArtworkVariation.PictureName(frame, shown);
    }

    /// <summary>
    /// Publishes a frame an encounter draws itself, outside the enemy list (#1392): from its graphics set's shared
    /// spawned-object sheet.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The actor shown.</param>
    /// <param name="frame">The frame.</param>
    /// <param name="origin">The frame origin on screen.</param>
    /// <param name="shading">The shades and colour effects the frame shows.</param>
    /// <param name="rows">The rows of the rotting corpse the frame shows, as its rot has left them; none for one that shows none.</param>
    internal void PublishEncounterFrame(SpriteFrameBuffer sprites, SpriteSource source, EnemyFrameId frame,
        Position16<ScreenPixels> origin, PictureShading shading, CorpseRotRows? rows = null) =>
        PublishSpawnedFrame(sprites, source, EnemySheetDefinitions.EncounterFrameName(frame), origin, SpriteGraphics.SharedIn(shading), rows);

    /// <summary>The installed enemy sheets.</summary>
    private IEnemySheetSource Sheets => _room.Session.Sheets ?? throw new InvalidOperationException(
        "Enemy drawing requires the installed enemy sheets.");

    /// <summary>The graphics set the room draws under.</summary>
    private RoomEnemyGraphicsSetDefinition LoadedSet => _graphicsSet.LoadedSet;

}
