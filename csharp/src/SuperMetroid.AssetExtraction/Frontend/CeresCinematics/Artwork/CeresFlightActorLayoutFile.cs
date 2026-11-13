using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Reads and writes the installed rear-view Ceres approach actor layout.</summary>
public static class CeresFlightActorLayoutFile
{
    /// <summary>Loads the supported layout schema, requiring all five fixed actor IDs in spawn order and signed screen-pixel starting coordinates.</summary>
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

        if (document.Version != CeresFlightActorLayoutFormat.Version ||
            document.Actors is not { Length: CeresFlightActorDefinitions.RearViewActorCount })
            throw new InvalidDataException(
                "Ceres flight requires exactly five ordered rear-view actor placements.");
        for (int index = 0; index < document.Actors.Length; index++)
        {
            CeresFlightActorPlacement placement = document.Actors[index];
            if (placement is null ||
                placement.Id != CeresFlightActors.RearViewPlacementId(index) ||
                !CeresActorScreenCoordinate.IsSigned(placement.X) ||
                !CeresActorScreenCoordinate.IsSigned(placement.Y))
                throw new InvalidDataException(
                    $"Ceres flight actor {index} must retain its identity and signed screen-pixel coordinates.");
        }
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
