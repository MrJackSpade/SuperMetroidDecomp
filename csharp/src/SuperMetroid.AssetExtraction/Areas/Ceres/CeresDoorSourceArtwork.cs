using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The Ceres door's installed artwork as the import pipeline holds it (#1484): the door's characters, which only its
/// pre-coloured sprite sheet is composed from, beside the colours gameplay reads.
/// </summary>
public sealed class CeresDoorSourceArtwork
{
    /// <summary>The door's validated four-bit characters.</summary>
    private readonly RoomCharacterAtlas tiles;

    /// <summary>Retains the door's characters, validating their count, beside its colours.</summary>
    /// <param name="tiles">The installed <c>ceres-door-tiles.png</c> characters, 32 of them.</param>
    /// <param name="visuals">The door's colours.</param>
    /// <exception cref="InvalidDataException">The characters are not the door's 32.</exception>
    public CeresDoorSourceArtwork(RoomCharacterAtlas tiles, CeresDoorVisuals visuals)
    {
        Ensure.NotNull(tiles);
        if (tiles.Characters.Length != CeresDoorVisualFormat.CharacterBytes)
            throw new InvalidDataException($"Ceres-door tiles hold {tiles.Characters.Length} bytes; expected {CeresDoorVisualFormat.CharacterBytes}.");
        this.tiles = tiles;
        Visuals = Ensure.NotNull(visuals);
    }

    /// <summary>The door's colours, which gameplay reads.</summary>
    public CeresDoorVisuals Visuals { get; }

    /// <summary>The door's 32 four-bit characters, which its second variant shows over the sprite characters.</summary>
    public ReadOnlyMemory<byte> Characters => tiles.Characters;

    /// <summary>Canonical selected presentation data: the door's characters and every colour it shows.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-ceres-door-v1", content =>
    {
        content.Append("tiles", tiles.Characters.Span);
        Span<RgbColor> normalColors = stackalloc RgbColor[CeresDoorVisualDefinitions.SetupColorCount];
        for (int color = 0; color < normalColors.Length; color++) normalColors[color] = Visuals.normal.ColorAt(color);
        content.AppendColors("normal", normalColors);
        Span<RgbColor> escapeColors = stackalloc RgbColor[CeresDoorVisualDefinitions.SetupColorCount];
        for (int color = 0; color < escapeColors.Length; color++) escapeColors[color] = Visuals.escape.ColorAt(color);
        content.AppendColors("escape", escapeColors);
        content.Append("animation", CeresDoorVisualDefinitions.AnimationStepCount);
        Span<RgbColor> rowColors = stackalloc RgbColor[CeresDoorVisualDefinitions.AnimationColorCount];
        for (int row = 0; row < CeresDoorVisualDefinitions.AnimationStepCount; row++)
        {
            for (int color = 0; color < rowColors.Length; color++) rowColors[color] = Visuals.animation.ColorAt(row, color);
            content.AppendColors("row", rowColors);
        }
    });
}
