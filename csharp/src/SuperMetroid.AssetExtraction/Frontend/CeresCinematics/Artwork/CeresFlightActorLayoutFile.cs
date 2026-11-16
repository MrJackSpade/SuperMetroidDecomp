using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Reads and writes the installed rear-view Ceres approach actor layout.</summary>
public static class CeresFlightActorLayoutFile
{
    /// <summary>Loads the supported layout schema, requiring one placement for each of the five rear-view roles at signed screen-pixel starting coordinates.</summary>
    /// <param name="json">Readable JSON stream, left open after deserialization.</param>
    /// <returns>Validated starting visual placements without changing instruction programs, physical motion, or the front-view star field.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema version, actor count/order/identity, or coordinate range is invalid.</exception>
    public static CeresFlightActorPlacements Read(Stream json)
    {
        Ensure.NotNull(json);
        CeresFlightActorLayoutDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresFlightActorLayoutDocument>(json,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Ceres flight actor layout is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres flight actor layout JSON.", error);
        }

        if (document.Version != CeresFlightActorLayoutFormat.Version || document.Actors is null)
            throw new InvalidDataException("Ceres flight requires a placement for each of its five rear-view actors.");
        foreach (CeresFlightActorPlacement placement in document.Actors)
        {
            if (placement is not null &&
                (!CeresActorScreenCoordinate.IsSigned(placement.X) || !CeresActorScreenCoordinate.IsSigned(placement.Y)))
                throw new InvalidDataException(
                    $"The Ceres flight's {placement.Role} actor must start at signed screen-pixel coordinates.");
        }
        // Each role must be placed exactly once; the placements reject a missing, repeated or unknown role.
        return new CeresFlightActorPlacements(document.Actors);
    }

    /// <summary>Serializes the editable layout as UTF-8 JSON and writes it to the destination.</summary>
    /// <param name="json">Destination stream, written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Supported-version document containing the five ordered actor placements.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    public static void Write(Stream json, CeresFlightActorLayoutDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        json.Write(bytes);
    }
}
