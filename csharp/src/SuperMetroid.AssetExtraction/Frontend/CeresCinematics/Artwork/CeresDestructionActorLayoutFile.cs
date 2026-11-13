using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Reads and writes the installed layout of the persistent actors behind Ceres's explosion.</summary>
public static class CeresDestructionActorLayoutFile
{
    /// <summary>Loads the supported layout schema, requiring the three persistent actor IDs in spawn order and signed screen-pixel starting coordinates.</summary>
    /// <param name="json">Readable JSON stream, left open after deserialization.</param>
    /// <returns>Validated persistent-actor placements, without changing asteroid motion/wrap, vortex behavior, explosion spawn events, or later Zebes artwork.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema version, actor count/order/identity, or coordinate range is invalid.</exception>
    public static CeresDestructionActorPlacements Read(Stream json)
    {
        Ensure.NotNull(json);
        CeresDestructionActorLayoutDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresDestructionActorLayoutDocument>(json,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Ceres destruction actor layout is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres destruction actor layout JSON.", error);
        }

        if (document.Version != CeresDestructionActorLayoutFormat.Version ||
            document.Actors is not { Length: CeresDestructionActorDefinitions.InitialActorCount })
            throw new InvalidDataException(
                "Ceres destruction requires exactly three ordered actor placements.");
        for (int index = 0; index < document.Actors.Length; index++)
        {
            CeresDestructionActorPlacement placement = document.Actors[index];
            if (placement is null ||
                placement.Id != CeresDestructionActors.InitialPlacementId(index) ||
                !CeresActorScreenCoordinate.IsSigned(placement.X) ||
                !CeresActorScreenCoordinate.IsSigned(placement.Y))
                throw new InvalidDataException(
                    $"Ceres destruction actor {index} must retain its identity and signed screen-pixel coordinates.");
        }
        return new CeresDestructionActorPlacements(document.Actors);
    }

    /// <summary>Serializes the selected destruction placements as UTF-8 JSON and writes them to the destination.</summary>
    /// <param name="json">Destination stream, written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Supported-version document containing the three ordered persistent-actor placements.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    public static void Write(Stream json, CeresDestructionActorLayoutDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        json.Write(bytes);
    }
}
