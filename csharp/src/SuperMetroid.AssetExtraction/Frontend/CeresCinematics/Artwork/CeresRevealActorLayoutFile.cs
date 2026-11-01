using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Reads and writes the installed layout of the Zebes reveal's actors.</summary>
public static class CeresRevealActorLayoutFile
{
    /// <summary>Validates the six fixed reveal-actor identities and compiles their editable initial positions from ceres-zebes-reveal-actors.json.</summary>
    /// <param name="json">UTF-8 JSON source read from its current position and left open.</param>
    /// <returns>Selected placements for the reveal, with stock positions resolved from the compiled initializers.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema version, six-entry order, actor identity, or unsigned 16-bit coordinate is invalid.</exception>
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

        if (document.Version != CeresRevealActorLayoutFormat.Version ||
            document.Actors is not { Length: CeresDestructionActorDefinitions.ZebesActorCount })
            throw new InvalidDataException("Ceres reveal requires exactly six ordered actor placements.");
        for (int index = 0; index < document.Actors.Length; index++)
        {
            CeresRevealActorPlacement placement = document.Actors[index];
            if (placement is null ||
                placement.Id != CeresDestructionActors.ZebesPlacementId(index) ||
                placement.X is < 0 or > ushort.MaxValue ||
                placement.Y is < 0 or > ushort.MaxValue)
                throw new InvalidDataException(
                    $"Ceres reveal actor {index} must retain its identity and 16-bit coordinates.");
        }
        return new CeresRevealActorPlacements(document.Actors);
    }

    /// <summary>Serializes the editable layout to UTF-8 JSON, validates it through <see cref="Read"/>, then writes the validated bytes.</summary>
    /// <param name="json">Destination written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Ordered actor placements to serialize; the document is not retained.</param>
    /// <exception cref="ArgumentNullException">The destination stream is null.</exception>
    /// <exception cref="InvalidDataException">The serialized document fails schema, identity, order, or coordinate validation.</exception>
    public static void Write(Stream json, CeresRevealActorLayoutDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
