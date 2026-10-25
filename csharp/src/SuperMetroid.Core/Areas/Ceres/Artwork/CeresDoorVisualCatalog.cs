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
            Span<Bgr555> normalColors = stackalloc Bgr555[CeresDoorVisualDefinitions.SetupColorCount];
            for (int color = 0; color < normalColors.Length; color++) normalColors[color] = normal.ColorAt(color);
            content.AppendColors("normal", normalColors);
            Span<Bgr555> escapeColors = stackalloc Bgr555[CeresDoorVisualDefinitions.SetupColorCount];
            for (int color = 0; color < escapeColors.Length; color++) escapeColors[color] = EscapeColor(color);
            content.AppendColors("escape", escapeColors);
            content.Append("animation", CeresDoorVisualDefinitions.AnimationRowCount);
            Span<Bgr555> rowColors = stackalloc Bgr555[CeresDoorVisualDefinitions.AnimationColorCount];
            for (int row = 0; row < CeresDoorVisualDefinitions.AnimationRowCount; row++)
            {
                for (int color = 0; color < rowColors.Length; color++) rowColors[color] = AnimationColor(row, color);
                content.AppendColors("row", rowColors);
            }
            content.Append("mode7-frames", CeresDoorVisualDefinitions.PlatformFrameCount);
            Span<byte> platform = stackalloc byte[CeresDoorVisualDefinitions.PlatformCellCount];
            for (int frame = 0; frame < CeresDoorVisualDefinitions.PlatformFrameCount; frame++)
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
    /// <summary>Platform strip cells that differ from the canonical platform frames.</summary>
    internal readonly Dictionary<int, byte> platformEdits = [];

    /// <summary>Retains validated visual data and extracts independently edited platform cells.</summary>
    private CeresDoorVisualCatalog(RoomCharacterAtlas tiles, Bgr555[] normal,
        Bgr555[] escape, Bgr555[][] animation, byte[][] platformFrames)
    {
        this.tiles = tiles;
        this.normal = new(normal);
        this.escape = new(escape, this.normal);
        this.animation = new(animation, this.normal);
        for (int frame = 0; frame < platformFrames.Length; frame++)
        for (int index = 0; index < CeresDoorVisualDefinitions.PlatformCellCount; index++)
            if (platformFrames[frame][index] != PlatformTile(frame, index))
                platformEdits.Add(frame * CeresDoorVisualDefinitions.PlatformCellCount + index, platformFrames[frame][index]);
    }

    /// <summary>Retains the door sheet's characters, validating their count, and the door colours and platform strips the installed visual document supplied.</summary>
    /// <param name="tiles">The installed <c>ceres-door-tiles.png</c> characters, 32 of them.</param>
    /// <param name="normal">Fifteen normal setup colours.</param>
    /// <param name="escape">Fifteen escape setup colours.</param>
    /// <param name="animation">Eight rows of six animation colours.</param>
    /// <param name="platformFrames">The elevator platform's two four-cell floor strips, light then dark.</param>
    /// <returns>The compiled visual snapshot.</returns>
    /// <exception cref="InvalidDataException">The characters are not the door's 32.</exception>
    internal static CeresDoorVisualCatalog Load(RoomCharacterAtlas tiles, Bgr555[] normal, Bgr555[] escape,
        Bgr555[][] animation, byte[][] platformFrames)
    {
        Ensure.NotNull(tiles);
        if (tiles.Characters.Length != CeresDoorVisualDefinitions.CharacterBytes)
            throw new InvalidDataException($"Ceres-door tiles hold {tiles.Characters.Length} bytes; expected {CeresDoorVisualDefinitions.CharacterBytes}.");
        return new CeresDoorVisualCatalog(tiles, normal, escape, animation,
            platformFrames);
    }

    /// <summary>The door's 32 four-bit characters, which its second variant shows over the sprite characters.</summary>
    public ReadOnlyMemory<byte> Characters => tiles.Characters;

    /// <summary>Installs fifteen normal nontransparent inks: the completed setup-fade target.</summary>
    /// <param name="destination">The door's colours: an inactive door's, or the active door's.</param>
    public void LoadNormalColors(ColourRun destination)
    {
        for (int color = 0; color < CeresDoorVisualDefinitions.SetupColorCount; color++)
            destination.Write(color, normal.ColorAt(color));
    }

    /// <summary>Installs fifteen escape-state nontransparent inks; escape-state selection remains actor-owned.</summary>
    /// <param name="destination">The active door's colours.</param>
    public void LoadEscapeColors(ColourRun destination)
    {
        for (int color = 0; color < CeresDoorVisualDefinitions.SetupColorCount; color++)
            destination.Write(color, EscapeColor(color));
    }

    /// <summary>Resolves one escape-state ink from the selected palette.</summary>
    private Bgr555 EscapeColor(int color) => escape.ColorAt(color);

    /// <summary>Installs the door animation's six colours from glow row 0–7; the actor derives the row from its frame bits 3–5.</summary>
    public void LoadAnimationColors(BackgroundColourSets colours, int row)
    {
        ColourRun animated = CeresDoorColourDefinitions.Animation.In(colours);
        if ((uint)row >= CeresDoorVisualDefinitions.AnimationRowCount) throw new ArgumentOutOfRangeException(nameof(row));
        for (int color = 0; color < CeresDoorVisualDefinitions.AnimationColorCount; color++)
            animated.Write(color, AnimationColor(row, color));
    }

    /// <summary>Resolves one ink from the actor-selected door animation row.</summary>
    private Bgr555 AnimationColor(int row, int color) => animation.ColorAt(row, color);

    /// <summary>Resolves one platform cell from the shared canonical strip or an independently supplied cell edit.</summary>
    private byte PlatformTile(int frame, int index)
    {
        byte stock = CeresFloorAnimationDefinitions.PlatformTile(frame, index);
        return platformEdits.TryGetValue(frame * CeresDoorVisualDefinitions.PlatformCellCount + index, out byte edit) ? edit : stock;
    }

    /// <summary>The selected light (0) or dark (1) platform strip: the four floor cells the rotating-elevator door draws.</summary>
    public byte[] PlatformFrame(int frame)
    {
        var tiles = new byte[CeresDoorVisualDefinitions.PlatformCellCount];
        for (int index = 0; index < tiles.Length; index++) tiles[index] = PlatformTile(frame, index);
        return tiles;
    }
}
