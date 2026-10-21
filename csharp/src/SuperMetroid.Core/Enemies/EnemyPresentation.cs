using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy presentation state: the bound VRAM and CGRAM, boss and gradual palette effects, the
/// host-installed color and artwork catalogs, and installed-spritemap drawing.
/// </summary>
internal sealed class EnemyPresentation
{
    /// <summary>Room artwork binding used to resolve enemy display compositions.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Binds enemy presentation to its room's installed artwork catalog.</summary>
    internal EnemyPresentation(EnemyRoomBinding room)
    {
        _room = room;
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
        if (_room.Artwork?.Spritemaps?.TryGetDisplay(_room.Frames.Bank(draw.Frame), _room.Frames.Spritemap(draw.Frame),
                out EnemySpritemapParts installed) == true)
        {
            ushort paletteBits = checked((ushort)draw.PaletteBits);
            PublishParts(sprites, objects, source, EnemySpriteFrames.Frame(installed, checked((ushort)draw.BaseTileIndex), paletteBits),
                new Position16<ScreenPixels>(draw.Origin.X, draw.Origin.Y), paletteBits);
            return;
        }
        throw new InvalidDataException($"Enemy frame {draw.Frame} requires an installed display composition.");
    }

    /// <summary>Publishes one enemy-format frame over the OBJ page.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="objects">The gameplay OBJ character page.</param>
    /// <param name="source">The actor shown.</param>
    /// <param name="frame">The whole frame, its parts' palettes and tiles already formed.</param>
    /// <param name="origin">The frame origin on screen.</param>
    /// <param name="paletteBits">The palette selection the frame was formed with.</param>
    internal static void PublishParts(SpriteFrameBuffer sprites, SpriteArtwork objects, SpriteSource source,
        SpriteFrame frame, Position16<ScreenPixels> origin, ushort paletteBits) =>
        sprites.Add(new SpriteDraw(source, frame, objects, origin, default,
            (byte)SnesObjAttributeWord.FromPaletteBits(paletteBits).PaletteIndex, 0));

}
