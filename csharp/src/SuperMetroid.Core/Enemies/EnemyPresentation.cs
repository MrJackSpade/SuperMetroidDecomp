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
    /// (#1512), such as a hit flash or a glow's frame.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The actor shown.</param>
    /// <param name="draw">The frame drawn: the enemy, its sheet frame, origin, shading and rot.</param>
    /// <exception cref="InvalidDataException">A frame showing a corpse's rot is drawn without that rot.</exception>
    internal void PublishSheetFrame(SpriteFrameBuffer sprites, SpriteSource source, EnemySheetDraw draw)
    {
        (EnemyDefinitionId enemy, string frame, Position16<ScreenPixels> origin, CorpseRotView? rot) = draw;
        PictureShading shading = draw.Shading ?? PictureShading.Standard;
        SpriteSheet sheet = Sheets.For(LoadedSet, enemy);
        string picture = Picture(frame, Sheets.Variations(LoadedSet, enemy, frame), Sheets.RotCorpse(LoadedSet, enemy, frame), rot);
        sprites.Add(new ImageSpriteDraw(source, sheet.Frame(picture), origin, SpriteMirror.None) { Shading = shading });
    }

    /// <summary>
    /// Publishes one spawned object's frame from the loaded graphics set's sheet of the owner its graphics name (#1392,
    /// #1513), under the shading the object carries (#1512).
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The object shown.</param>
    /// <param name="frame">The sheet frame's name.</param>
    /// <param name="origin">The frame origin on screen.</param>
    /// <param name="graphics">Whose spawned-object sheet the object draws from, and the shading it shows its picture in.</param>
    /// <param name="rot">The rot the drawing encounter selected, which a frame drawn over a rotting corpse's rows shows; none for one that shows none.</param>
    /// <exception cref="InvalidDataException">
    /// The set has no sheet for that owner, or a frame showing a corpse's rot is drawn without that rot.
    /// </exception>
    internal void PublishSpawnedFrame(SpriteFrameBuffer sprites, SpriteSource source, string frame, Position16<ScreenPixels> origin,
        SpriteGraphics graphics, CorpseRotView? rot = null)
    {
        SpawnedObjectOwner owner = graphics.Owner;
        SpawnedObjectSheet sheet = Sheets.Spawned(LoadedSet, owner);
        string picture = Picture(frame, Sheets.SpawnedVariations(LoadedSet, owner, frame),
            Sheets.SpawnedRotCorpse(LoadedSet, owner, frame), rot);
        sprites.Add(new ImageSpriteDraw(source, sheet.Sheet.Frame(picture), origin, SpriteMirror.None) { Shading = graphics.Shading });
    }

    /// <summary>
    /// The picture <paramref name="frame"/> draws as: for a frame drawn over a rotting corpse's rows, its picture of the
    /// <paramref name="rot"/> its draw selected (#1406); for a frame the room's encounters' artwork sequences change, its
    /// picture of the step each of its <paramref name="variations"/> has reached (#1513); else the frame itself.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The frame shows a corpse's rot and the draw selects none, or another corpse's; or it is drawn at a step of a sequence,
    /// or before a sequence's first step, at which it is never drawn, so its sheet holds no picture of it then.
    /// </exception>
    private string Picture(string frame, IReadOnlyList<FrameArtworkVariation> variations, string? rotCorpse, CorpseRotView? rot)
    {
        if (rotCorpse is not null)
        {
            if (rot is null)
                throw new InvalidDataException($"Sheet frame {frame} shows the rot of {rotCorpse}, but its draw selects no rot.");
            if (rot.Corpse.Name != rotCorpse)
                throw new InvalidDataException($"Sheet frame {frame} shows the rot of {rotCorpse}, but its draw selects the rot of {rot.Corpse.Name}.");
            return rot.PictureName(frame);
        }
        var shown = new List<EncounterArtworkStep>();
        foreach (FrameArtworkVariation variation in variations)
        {
            int? reached = ArtworkProgress.StepOf(variation.Sequence);
            if (reached is null && !variation.DrawnBeforeFirstStep)
                throw new InvalidDataException($"Sheet frame {frame} is drawn before {variation.Sequence} reaches its first step, when it is never drawn; its sheet holds no picture of it then.");
            if (reached is int step && variation.PictureAt(step, frame) is int picture)
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
    /// <param name="rot">The rot the encounter selected, which a frame drawn over its rotting rows shows; none for one that shows none.</param>
    internal void PublishEncounterFrame(SpriteFrameBuffer sprites, SpriteSource source, EnemyFrameId frame,
        Position16<ScreenPixels> origin, PictureShading shading, CorpseRotView? rot = null) =>
        PublishSpawnedFrame(sprites, source, EnemySheetDefinitions.EncounterFrameName(frame), origin, SpriteGraphics.SharedIn(shading), rot);

    /// <summary>The installed enemy sheets.</summary>
    private IEnemySheetSource Sheets => _room.Session.Sheets ?? throw new InvalidOperationException(
        "Enemy drawing requires the installed enemy sheets.");

    /// <summary>The graphics set the room draws under.</summary>
    private RoomEnemyGraphicsSetDefinition LoadedSet => _graphicsSet.LoadedSet;

}
