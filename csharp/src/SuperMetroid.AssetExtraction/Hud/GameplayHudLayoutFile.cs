using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// The codec of the installed gameplay-HUD layout (#1638): the extraction writes it from the HUD's decoded artwork, whose
/// cells are import data, and the game reads it as the layout it places the HUD's images by (#1616).
/// </summary>
public static class GameplayHudLayoutFile
{
    /// <summary>The layout document of <paramref name="artwork"/>, as UTF-8 JSON.</summary>
    /// <param name="artwork">The HUD's decoded, validated artwork, whose layout is written.</param>
    public static byte[] Encode(GameplayHudArtwork artwork)
    {
        GameplayHudPresentationParts parts = Ensure.NotNull(artwork).LayoutParts;
        var anchors = new Dictionary<string, Position16<ScreenPixels>>(StringComparer.Ordinal);
        var showsOverTemplate = new Dictionary<string, bool>(StringComparer.Ordinal);
        for (int icon = 0; icon < GameplayHudDefinitions.IconNames.Length; icon++)
        {
            anchors.Add(GameplayHudDefinitions.IconNames[icon], parts.IconAnchors[icon]);
            showsOverTemplate.Add(GameplayHudDefinitions.IconNames[icon], parts.IconsShowOverTemplate[icon]);
        }
        return JsonSerializer.SerializeToUtf8Bytes(new GameplayHudLayoutDocument
        {
            Version = GameplayHudDocumentDefinitions.LayoutVersion,
            EnergyTankCorners = [.. parts.EnergyTankCorners],
            HealthAnchor = parts.HealthAnchor,
            MissileAmmoAnchor = parts.MissileAmmoAnchor,
            SuperMissileAmmoAnchor = parts.SuperMissileAmmoAnchor,
            PowerBombAmmoAnchor = parts.PowerBombAmmoAnchor,
            AutoReserveCorners = [.. parts.AutoReserveCorners],
            IconAnchors = anchors,
            IconsShowOverTemplate = showsOverTemplate,
            MinimapAnchor = parts.MinimapAnchor,
        }, ArtworkJson.Options);
    }

    /// <summary>Reads and validates an installed gameplay-HUD layout.</summary>
    /// <param name="json">The UTF-8 JSON document stream.</param>
    /// <returns>The layout the game places the HUD's images by.</returns>
    /// <exception cref="InvalidDataException">The document is malformed, incomplete, or places an element outside the HUD's band.</exception>
    public static GameplayHudPresentation Read(Stream json)
    {
        Ensure.NotNull(json);
        byte[] source;
        using (var buffer = new MemoryStream())
        {
            json.CopyTo(buffer);
            source = buffer.ToArray();
        }
        GameplayHudLayoutDocument document;
        try
        {
            document = JsonAssetDocument.Read<GameplayHudLayoutDocument>(source, ArtworkJson.Options) ??
                throw new InvalidDataException("Gameplay HUD layout document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid gameplay HUD layout JSON.", error);
        }
        if (document.Version != GameplayHudDocumentDefinitions.LayoutVersion)
            throw new InvalidDataException($"Gameplay HUD layout requires version {GameplayHudDocumentDefinitions.LayoutVersion}.");
        if (document.EnergyTankCorners is null || document.EnergyTankCorners.Length != GameplayHudDefinitions.EnergyTankCount ||
            document.AutoReserveCorners is null || document.AutoReserveCorners.Length != GameplayHudDefinitions.AutoReserveCellCount)
            throw new InvalidDataException("Gameplay HUD layout requires every energy tank's and AUTO part's corner.");
        if (document.IconAnchors is null || document.IconsShowOverTemplate is null ||
            document.IconAnchors.Count != GameplayHudDefinitions.IconNames.Length ||
            document.IconsShowOverTemplate.Count != GameplayHudDefinitions.IconNames.Length ||
            GameplayHudDefinitions.IconNames.Any(name => !document.IconAnchors.ContainsKey(name) || !document.IconsShowOverTemplate.ContainsKey(name)))
            throw new InvalidDataException("Gameplay HUD layout requires exactly the five named item icons.");
        return new GameplayHudPresentation(new GameplayHudPresentationParts
        {
            EnergyTankCorners = [.. document.EnergyTankCorners.Select((corner, index) => InBand(corner, $"energy tank {index}"))],
            HealthAnchor = InBand(document.HealthAnchor, "health digits"),
            MissileAmmoAnchor = InBand(document.MissileAmmoAnchor, "missile digits"),
            SuperMissileAmmoAnchor = InBand(document.SuperMissileAmmoAnchor, "Super Missile digits"),
            PowerBombAmmoAnchor = InBand(document.PowerBombAmmoAnchor, "Power Bomb digits"),
            AutoReserveCorners = [.. document.AutoReserveCorners.Select((corner, index) => InBand(corner, $"AUTO part {index}"))],
            IconAnchors = [.. GameplayHudDefinitions.IconNames.Select(name => InBand(document.IconAnchors[name], $"{name} icon"))],
            IconsShowOverTemplate = [.. GameplayHudDefinitions.IconNames.Select(name => document.IconsShowOverTemplate[name])],
            MinimapAnchor = InBand(document.MinimapAnchor, "minimap"),
        });
    }

    /// <summary>
    /// Requires <paramref name="corner"/> to lie in the band the HUD's values are drawn over: across the HUD's columns,
    /// below the static top row and above the band's end.
    /// </summary>
    /// <exception cref="InvalidDataException">The corner lies outside the band.</exception>
    private static Position16<ScreenPixels> InBand(Position16<ScreenPixels> corner, string name)
    {
        const int width = GameplayHudDocumentDefinitions.Columns * CharacterCellConversions.CellPixels;
        const int bottom = GameplayHudDefinitions.TopRowHeightPixels + GameplayHudDocumentDefinitions.DrawnRows * CharacterCellConversions.CellPixels;
        if (corner.X < 0 || corner.X >= width || corner.Y < GameplayHudDefinitions.TopRowHeightPixels || corner.Y >= bottom)
            throw new InvalidDataException($"Gameplay HUD {name} lies outside the band the HUD's values are drawn over.");
        return corner;
    }
}
