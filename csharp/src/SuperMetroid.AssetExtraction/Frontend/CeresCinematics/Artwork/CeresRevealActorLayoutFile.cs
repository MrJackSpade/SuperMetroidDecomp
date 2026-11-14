using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Reads and writes the installed layout of the Zebes reveal's actors.</summary>
public static class CeresRevealActorLayoutFile
{
    /// <summary>Validates that each reveal role is placed once and compiles their editable initial positions from ceres-zebes-reveal-actors.json.</summary>
    /// <param name="json">UTF-8 JSON source read from its current position and left open.</param>
    /// <returns>The seven installed placements of the reveal.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema version, seven-entry order, actor identity, or signed screen-pixel coordinate is invalid.</exception>
    public static CeresRevealActorPlacements Read(Stream json)
    {
        Ensure.NotNull(json);
        CeresRevealActorLayoutDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresRevealActorLayoutDocument>(json,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Ceres reveal actor layout is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres reveal actor layout JSON.", error);
        }

        if (document.Version != CeresRevealActorLayoutFormat.Version || document.Actors is null)
            throw new InvalidDataException("Ceres reveal requires a placement for each of its seven actors.");
        foreach (CeresRevealActorPlacement placement in document.Actors)
        {
            if (placement is not null &&
                (!CeresActorScreenCoordinate.IsSigned(placement.X) || !CeresActorScreenCoordinate.IsSigned(placement.Y)))
                throw new InvalidDataException(
                    $"The Zebes reveal's {placement.Role} actor must start at signed screen-pixel coordinates.");
        }
        // Each role must be placed exactly once; the placements reject a missing, repeated or unknown role.
        return new CeresRevealActorPlacements(document.Actors);
    }

    /// <summary>Serializes the editable layout to UTF-8 JSON and writes the bytes.</summary>
    /// <param name="json">Destination written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Ordered actor placements to serialize; the document is not retained.</param>
    /// <exception cref="ArgumentNullException">The destination stream is null.</exception>
    public static void Write(Stream json, CeresRevealActorLayoutDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        json.Write(bytes);
    }
}
