using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Editable Ceres-door characters and RGB5 palettes; actor timing remains engine-owned, and the elevator platform's strips are pictures of the room's floor (#1467).</summary>
public sealed class CeresDoorVisuals
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-ceres-door-v1", content =>
        {
            content.Append("tiles", tiles.Characters.Span);
            Span<RgbColor> normalColors = stackalloc RgbColor[CeresDoorVisualDefinitions.SetupColorCount];
            for (int color = 0; color < normalColors.Length; color++) normalColors[color] = normal.ColorAt(color);
            content.AppendColors("normal", normalColors);
            Span<RgbColor> escapeColors = stackalloc RgbColor[CeresDoorVisualDefinitions.SetupColorCount];
            for (int color = 0; color < escapeColors.Length; color++) escapeColors[color] = EscapeColor(color);
            content.AppendColors("escape", escapeColors);
            content.Append("animation", CeresDoorVisualDefinitions.AnimationRowCount);
            Span<RgbColor> rowColors = stackalloc RgbColor[CeresDoorVisualDefinitions.AnimationColorCount];
            for (int row = 0; row < CeresDoorVisualDefinitions.AnimationRowCount; row++)
            {
                for (int color = 0; color < rowColors.Length; color++) rowColors[color] = AnimationColor(row, color);
                content.AppendColors("row", rowColors);
            }
        });

    /// <summary>Validated 4bpp door characters used for the VRAM transfer.</summary>
    private readonly RoomCharacterAtlas tiles;
    /// <summary>Normal setup colors, including any supplied edits to stock values.</summary>
    private readonly CeresDoorNormalPaint normal;
    /// <summary>Escape setup colors resolved against the normal palette.</summary>
    internal readonly CeresDoorEscapePaint escape;
    /// <summary>Eight animation rows resolved against the normal palette.</summary>
    internal readonly CeresDoorAnimationPaint animation;
    /// <summary>Retains validated visual data.</summary>
    private CeresDoorVisuals(RoomCharacterAtlas tiles, RgbColor[] normal,
        RgbColor[] escape, RgbColor[][] animation)
    {
        this.tiles = tiles;
        this.normal = new(normal);
        this.escape = new(escape, this.normal);
        this.animation = new(animation, this.normal);
    }

    /// <summary>Retains the door sheet's characters, validating their count, and the door colours the installed visual document supplied.</summary>
    /// <param name="tiles">The installed <c>ceres-door-tiles.png</c> characters, 32 of them.</param>
    /// <param name="normal">Fifteen normal setup colours.</param>
    /// <param name="escape">Fifteen escape setup colours.</param>
    /// <param name="animation">Eight rows of six animation colours.</param>
    /// <returns>The compiled visual snapshot.</returns>
    /// <exception cref="InvalidDataException">The characters are not the door's 32.</exception>
    internal static CeresDoorVisuals Load(RoomCharacterAtlas tiles, RgbColor[] normal, RgbColor[] escape,
        RgbColor[][] animation)
    {
        Ensure.NotNull(tiles);
        if (tiles.Characters.Length != CeresDoorVisualDefinitions.CharacterBytes)
            throw new InvalidDataException($"Ceres-door tiles hold {tiles.Characters.Length} bytes; expected {CeresDoorVisualDefinitions.CharacterBytes}.");
        return new CeresDoorVisuals(tiles, normal, escape, animation);
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
    private RgbColor EscapeColor(int color) => escape.ColorAt(color);

    /// <summary>Installs the door animation's six colours from glow row 0–7; the actor derives the row from its frame bits 3–5.</summary>
    public void LoadAnimationColors(BackgroundColourSets colours, int row)
    {
        ColourRun animated = CeresDoorColourDefinitions.Animation.In(colours);
        if ((uint)row >= CeresDoorVisualDefinitions.AnimationRowCount) throw new ArgumentOutOfRangeException(nameof(row));
        for (int color = 0; color < CeresDoorVisualDefinitions.AnimationColorCount; color++)
            animated.Write(color, AnimationColor(row, color));
    }

    /// <summary>Resolves one ink from the actor-selected door animation row.</summary>
    private RgbColor AnimationColor(int row, int color) => animation.ColorAt(row, color);
}
