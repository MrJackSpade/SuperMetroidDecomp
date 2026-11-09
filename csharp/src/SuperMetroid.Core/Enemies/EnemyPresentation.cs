using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Assets;
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
    /// The body the room's boss shows its parts' frames on (#1353, #1466, #1515), once a boss that shows one attaches it;
    /// Kraid publishes his own.
    /// </summary>
    internal BossBodyParts? BossBody { get; private set; }

    /// <summary>A boss's initialization: attaches the body, no part showing anything yet, that its parts show their frames on.</summary>
    internal void AttachBossBody(BossBodyParts body) => BossBody = Ensure.NotNull(body);

    /// <summary>The attached body.</summary>
    /// <exception cref="InvalidOperationException">No boss has attached one.</exception>
    internal BossBodyParts RequireBossBody() =>
        BossBody ?? throw new InvalidOperationException("No boss has attached its body.");

    /// <summary>The look each of the room's encounters shows, which the frames its looks change show their pictures of (#1538).</summary>
    internal EncounterLooks Looks => _room.Session.Looks;

    /// <summary>Shows <paramref name="look"/> on its encounter's frames from this update on (#1538).</summary>
    internal void ShowLook(EncounterLook look) => Looks.Show(look);

    /// <summary>
    /// The session's shared fade step count, a compatibility service (#1544): enemies' fades and the runtime's door, death
    /// and time-up fades all count their steps on it, so a fade left part-way shortens the next.
    /// </summary>
    public SharedFadeStepCount FadeStepCount => _room.Session.FadeStepCount;

    /// <summary>
    /// Publishes one frame of the enemy <paramref name="draw"/> shows from its installed sheet (#1392): the sheet its room's
    /// graphics set names, pre-coloured in the colours the set loads for the enemy, under the shading the draw names
    /// (#1512), such as a hit flash or a glow's frame. A frame that shows a rotting corpse is drawn whole through the
    /// corpse's own rows as its rot leaves them (#1539).
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The actor shown.</param>
    /// <param name="draw">The frame drawn: the enemy, its sheet frame, origin, shading and the corpse's rows.</param>
    internal void PublishSheetFrame(SpriteFrameBuffer sprites, SpriteSource source, EnemySheetDraw draw)
    {
        (EnemyDefinitionId enemy, string frame, Position16<ScreenPixels> origin) = draw;
        PictureShading shading = draw.Shading ?? PictureShading.Standard;
        SpriteSheet sheet = Sheets.For(LoadedSet, enemy);
        string picture = Picture(frame, Sheets.ChangingEncounters(LoadedSet, enemy, frame));
        // A finished picture that shows nothing draws nothing (#1538).
        if (sheet.ShowsNothing(picture))
            return;
        SpriteSheetFrame image = sheet.Frame(picture);
        sprites.Add(new ImageSpriteDraw(source, image, origin, SpriteMirror.None)
        {
            Shading = shading,
            CorpseRot = CorpseRot(Sheets.CorpsePlacement(LoadedSet, enemy, picture), draw.RotRows, image),
        });
    }

    /// <summary>
    /// Publishes one spawned object's frame from the loaded graphics set's pictures of the owner its graphics name (#1392,
    /// #1513), the set's own sheet or the common sheet holding the picture, under the shading the object carries (#1512),
    /// through a rotting corpse's own <paramref name="rows"/> when the frame shows the corpse (#1539).
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The object shown.</param>
    /// <param name="frame">The sheet frame's name.</param>
    /// <param name="origin">The frame origin on screen.</param>
    /// <param name="graphics">Whose spawned-object sheet the object draws from, and the shading it shows its picture in.</param>
    /// <param name="rows">The rows of the rotting corpse the frame shows, as its rot has left them; none for one that shows none.</param>
    /// <exception cref="InvalidDataException">
    /// The set has no pictures for that owner, or none of its sheets holds the picture.
    /// </exception>
    internal void PublishSpawnedFrame(SpriteFrameBuffer sprites, SpriteSource source, string frame, Position16<ScreenPixels> origin,
        SpriteGraphics graphics, CorpseRotRows? rows = null)
    {
        SpawnedObjectOwner owner = graphics.Owner;
        SpawnedObjectSheet sheet = Sheets.Spawned(LoadedSet, owner);
        string picture = Picture(frame, Sheets.SpawnedChangingEncounters(LoadedSet, owner, frame));
        // A finished picture that shows nothing draws nothing (#1538).
        if (sheet.ShowsNothing(picture))
            return;
        SpriteSheetFrame image = sheet.Frame(picture);
        sprites.Add(new ImageSpriteDraw(source, image, origin, SpriteMirror.None)
        {
            Shading = graphics.Shading,
            CorpseRot = CorpseRot(Sheets.SpawnedCorpsePlacement(LoadedSet, owner, picture), rows, image),
        });
    }

    /// <summary>
    /// How <paramref name="image"/> shows the rotting corpse that lies in it as <paramref name="placement"/> says, whole,
    /// through the corpse's own <paramref name="rows"/> (#1539); none for a picture that shows no rotting corpse, or a draw
    /// that names no corpse's rows, when the picture shows itself.
    /// </summary>
    private static CorpseRotPicture? CorpseRot(CorpsePicturePlacement? placement, CorpseRotRows? rows, SpriteSheetFrame image) =>
        placement is not null && rows is not null ? CorpseRotDrawing.Picture(placement, rows, image.Size) : null;

    /// <summary>
    /// The finished picture <paramref name="frame"/> draws as (#1538): for a frame the room's encounters' looks change, its
    /// picture of the look each of the <paramref name="encounters"/> shows now; else the frame's one picture. A sheet holds
    /// a frame's picture of only the looks it is drawn in, so a draw in another fails when the sheet is asked for it.
    /// </summary>
    private string Picture(string frame, IReadOnlyList<EncounterArtwork> encounters) =>
        EncounterLook.PictureName(frame, encounters.Select(Looks.Current));

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

    /// <summary>
    /// The installed picture of <paramref name="enemy"/>'s <paramref name="frame"/> in the sheet the room's graphics set
    /// names (#1545), for a scene that shows it as scenery of its own rather than as the enemy's draw.
    /// </summary>
    /// <param name="enemy">The enemy whose sheet holds the picture.</param>
    /// <param name="frame">The frame pictured.</param>
    internal SpriteSheetFrame SheetPicture(EnemyDefinitionId enemy, EnemyFrameId frame) =>
        Sheets.For(LoadedSet, enemy).Frame(EnemySheetDefinitions.FrameName(frame));

    /// <summary>The installed enemy sheets.</summary>
    private IEnemySheetSource Sheets => _room.Session.Sheets ?? throw new InvalidOperationException(
        "Enemy drawing requires the installed enemy sheets.");

    /// <summary>The graphics set the room draws under.</summary>
    private RoomEnemyGraphicsSetDefinition LoadedSet => _graphicsSet.LoadedSet;

}
