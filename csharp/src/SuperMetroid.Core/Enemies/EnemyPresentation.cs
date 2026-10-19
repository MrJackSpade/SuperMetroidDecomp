using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy presentation state: the bound VRAM and CGRAM, boss and gradual palette effects, the
/// host-installed color and artwork catalogs, and installed-spritemap drawing.
/// </summary>
internal sealed class EnemyPresentation
{
    /// <summary>Current host-owned escape-warning text, rebound after debugger restoration.</summary>
    internal EscapeTypewriterPresentation? EscapeTypewriterPresentation
    {
        get => _room.Session.EscapeTypewriterPresentation;
        set => _room.Session.EscapeTypewriterPresentation = value;
    }

    /// <summary>Room artwork binding used to resolve enemy display compositions.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Binds enemy presentation to its room's installed artwork catalog.</summary>
    internal EnemyPresentation(EnemyRoomBinding room)
    {
        _room = room;
    }

    /// <summary>Host-owned authored damage colors, rebound after debugger-state restoration.</summary>
    public MotherBrainHealthPalettePresentation? MotherBrainHealthColors
    {
        get => _room.Session.MotherBrainHealthColors;
        set => _room.Session.MotherBrainHealthColors = value;
    }

    /// <summary>Host-owned rainbow, drain and revival colors, rebound after debugger-state restoration.</summary>
    public MotherBrainRainbowPalettePresentation? MotherBrainRainbowColors
    {
        get => _room.Session.MotherBrainRainbowColors;
        set => _room.Session.MotherBrainRainbowColors = value;
    }

    /// <summary>Host-owned fake-death room flash and phase-two setup colors.</summary>
    public MotherBrainRoomColorPresentation? MotherBrainRoomColors
    {
        get => _room.Session.MotherBrainRoomColors;
        set => _room.Session.MotherBrainRoomColors = value;
    }
    /// <summary>The bound CGRAM's enemy and boss ranges.</summary>
    internal EnemyPalettes BossPalettes => new(Cgram);

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
    /// <summary>Room load: binds the VRAM the room's enemy artwork transfers write.</summary>
    internal void BindVram(SnesVram vram) => Vram = Ensure.NotNull(vram);

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

    /// <summary>Current host-owned timer artwork; rebound after debugger-state restoration.</summary>
    public EscapeTimerTileAtlas? EscapeTimerArtwork
    {
        get => _room.Session.EscapeTimerArtwork;
        set => _room.Session.EscapeTimerArtwork = value;
    }

    /// <summary>Host-bound Ceres Ridley fade and retreat colors; rebound after state restore.</summary>
    public CeresRidleyColorCatalog? CeresRidleyColors
    {
        get => _room.Session.CeresRidleyColors;
        set => _room.Session.CeresRidleyColors = value;
    }

    /// <summary>Host-bound Ceres Mode-7 zoom colors; rebound after state restore.</summary>
    public CeresRidleyMode7ColorCatalog? CeresRidleyMode7Colors
    {
        get => _room.Session.CeresRidleyMode7Colors;
        set => _room.Session.CeresRidleyMode7Colors = value;
    }

    /// <summary>Installed standard BG3 characters shared with Kraid's death restoration.</summary>
    public HudTileAtlas? HudTileArtwork
    {
        get => _room.Session.HudTileArtwork;
        set => _room.Session.HudTileArtwork = value;
    }

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
        if (_room.Artwork?.Spritemaps?.TryGetDisplay(EnemyFrames.Bank(draw.Frame), EnemyFrames.Spritemap(draw.Frame),
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

    /// <summary>Draws an installed enemy composition or accepts the compiled common empty frame.</summary>
    /// <remarks>A nonempty frame without installed display art is an asset error rather than a cartridge fallback.</remarks>
    internal void DrawEnemySpritemap(OamBuffer oam, EnemySpritemapDraw draw)
    {
        // The common empty frame is compiled definition data, not missing artwork.
        if (draw.Frame == EnemyFrameId.Empty)
            return;
        if (_room.Artwork?.Spritemaps?.TryGetDisplay(EnemyFrames.Bank(draw.Frame), EnemyFrames.Spritemap(draw.Frame),
                out EnemySpritemapParts installed) == true)
        {
            oam.AddEnemySpritemap(installed, new Position16<OamPixels>(draw.Origin.X, draw.Origin.Y),
                checked((ushort)draw.PaletteBits), checked((ushort)draw.BaseTileIndex), draw.ClipVerticalWrap,
                draw.OriginYIsOnScreen);
            return;
        }
        throw new InvalidDataException($"Enemy frame {draw.Frame} requires an installed display composition.");
    }
}
