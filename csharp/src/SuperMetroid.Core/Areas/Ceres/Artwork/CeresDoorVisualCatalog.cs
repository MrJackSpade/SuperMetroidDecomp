using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Editable Ceres-door tile DMA and RGB5 palettes; actor timing remains engine-owned.</summary>
public sealed class CeresDoorVisualCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-ceres-door-v1", content =>
        {
            content.Append("tiles", tiles.Characters.Span);
            Span<Bgr555> normalColors = stackalloc Bgr555[CeresDoorVisualRomData.SetupColorCount];
            for (int color = 0; color < normalColors.Length; color++) normalColors[color] = normal.ColorAt(color);
            content.AppendColors("normal", normalColors);
            Span<Bgr555> escapeColors = stackalloc Bgr555[CeresDoorVisualRomData.SetupColorCount];
            for (int color = 0; color < escapeColors.Length; color++) escapeColors[color] = EscapeColor(color);
            content.AppendColors("escape", escapeColors);
            content.Append("animation", CeresDoorVisualRomData.AnimationRowCount);
            Span<Bgr555> rowColors = stackalloc Bgr555[CeresDoorVisualRomData.AnimationColorCount];
            for (int row = 0; row < CeresDoorVisualRomData.AnimationRowCount; row++)
            {
                for (int color = 0; color < rowColors.Length; color++) rowColors[color] = AnimationColor(row, color);
                content.AppendColors("row", rowColors);
            }
            content.Append("mode7-frames", CeresDoorVisualRomData.Mode7FrameCount);
            Span<byte> platform = stackalloc byte[CeresDoorVisualRomData.Mode7FrameByteCount];
            for (int frame = 0; frame < CeresDoorVisualRomData.Mode7FrameCount; frame++)
            {
                for (int index = 0; index < platform.Length; index++) platform[index] = PlatformTile(frame, index);
                content.Append("mode7-frame", platform);
            }
        });

    /// <summary>Validated 4bpp door characters used for the VRAM transfer.</summary>
    private readonly RoomCharacterAtlas tiles;
    /// <summary>Normal setup colors, including any supplied edits to stock values.</summary>
    private readonly CeresDoorNormalPaintDefinitions normal;
    /// <summary>Escape setup colors resolved against the normal palette.</summary>
    internal readonly CeresDoorEscapePaintDefinitions escape;
    /// <summary>Eight animation rows resolved against the normal palette.</summary>
    internal readonly CeresDoorAnimationPaintDefinitions animation;
    /// <summary>Mode 7 strip bytes that differ from the canonical platform frames.</summary>
    internal readonly Dictionary<int, byte> platformEdits = [];

    /// <summary>Retains validated visual data and extracts independently edited Mode 7 cells.</summary>
    private CeresDoorVisualCatalog(RoomCharacterAtlas tiles, Bgr555[] normal,
        Bgr555[] escape, Bgr555[][] animation, byte[][] mode7DoorFrames)
    {
        this.tiles = tiles;
        this.normal = new(normal);
        this.escape = new(escape, this.normal);
        this.animation = new(animation, this.normal);
        for (int frame = 0; frame < mode7DoorFrames.Length; frame++)
        for (int index = 0; index < CeresDoorVisualRomData.Mode7FrameByteCount; index++)
            if (mode7DoorFrames[frame][index] != PlatformTile(frame, index))
                platformEdits.Add(frame * CeresDoorVisualRomData.Mode7FrameByteCount + index, mode7DoorFrames[frame][index]);
    }

    /// <summary>Loads the indexed 4bpp door sheet, validating its native transfer length, and retains the door colours and Mode 7 strips the installed visual document supplied.</summary>
    /// <param name="tilePng">Caller-owned <c>ceres-door-tiles.png</c> stream compiling to $0400 bytes.</param>
    /// <param name="normal">Fifteen normal setup colours.</param>
    /// <param name="escape">Fifteen escape setup colours.</param>
    /// <param name="animation">Eight rows of six animation colours.</param>
    /// <param name="mode7DoorFrames">Two four-cell Mode 7 platform strips.</param>
    /// <returns>The compiled visual snapshot; the stream is consumed from its current position and left open.</returns>
    internal static CeresDoorVisualCatalog Load(Stream tilePng, Bgr555[] normal, Bgr555[] escape,
        Bgr555[][] animation, byte[][] mode7DoorFrames)
    {
        Ensure.NotNull(tilePng);
        RoomCharacterAtlas tiles = RoomCharacterAtlas.Load(tilePng,
            CeresDoorVisualRomData.TileByteCount);
        return new CeresDoorVisualCatalog(tiles, normal, escape, animation,
            mode7DoorFrames);
    }

    /// <summary>The door's 32 four-bit characters, $0400 bytes, which variant two shows in place of its native $B0:C400 DMA source.</summary>
    public ReadOnlyMemory<byte> Characters => tiles.Characters;

    /// <summary>Installs fifteen normal nontransparent inks corresponding to $A6:F4EE in current CGRAM, exposing the native completed setup-fade target directly.</summary>
    /// <param name="destination">The door's colours: an inactive door's, or the active door's.</param>
    public void LoadNormalColors(ColourRun destination)
    {
        for (int color = 0; color < CeresDoorVisualRomData.SetupColorCount; color++)
            destination.Write(color, normal.ColorAt(color));
    }

    /// <summary>Installs fifteen escape-state nontransparent inks corresponding to $A6:F50E in current CGRAM; escape-state selection remains actor-owned.</summary>
    /// <param name="destination">The active door's colours.</param>
    public void LoadEscapeColors(ColourRun destination)
    {
        for (int color = 0; color < CeresDoorVisualRomData.SetupColorCount; color++)
            destination.Write(color, EscapeColor(color));
    }

    /// <summary>Resolves one escape-state ink from the selected palette.</summary>
    private Bgr555 EscapeColor(int color) => escape.ColorAt(color);

    /// <summary>Installs the door animation's six colours from animation row 0–7 corresponding to $A6:F871; the actor derives the row from its frame bits 3–5.</summary>
    public void LoadAnimationColors(BackgroundColourSets colours, int row)
    {
        ColourRun animated = CeresDoorColourDefinitions.Animation.In(colours);
        if ((uint)row >= CeresDoorVisualRomData.AnimationRowCount) throw new ArgumentOutOfRangeException(nameof(row));
        for (int color = 0; color < CeresDoorVisualRomData.AnimationColorCount; color++)
            animated.Write(color, AnimationColor(row, color));
    }

    /// <summary>Resolves one ink from the actor-selected door animation row.</summary>
    private Bgr555 AnimationColor(int row, int color) => animation.ColorAt(row, color);

    /// <summary>$A6:F918/F91C: resolve shared canonical platform imagery or an independent supplied cell edit.</summary>
    private byte PlatformTile(int frame, int index)
    {
        byte stock = CeresMode7TransferDefinitions.PlatformTile(frame, index);
        return platformEdits.TryGetValue(frame * CeresDoorVisualRomData.Mode7FrameByteCount + index, out byte edit) ? edit : stock;
    }

    /// <summary>The selected light/dark platform frame 0 or 1: the four Mode 7 map cells the native $A6:F918/$F91C strip writes.</summary>
    public byte[] Mode7DoorFrame(int frame)
    {
        var tiles = new byte[CeresDoorVisualRomData.Mode7FrameByteCount];
        for (int index = 0; index < tiles.Length; index++) tiles[index] = PlatformTile(frame, index);
        return tiles;
    }
}
