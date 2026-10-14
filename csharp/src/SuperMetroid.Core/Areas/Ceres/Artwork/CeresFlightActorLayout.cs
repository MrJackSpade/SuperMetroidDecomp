using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// Editable starting screen positions for the five rear-view Ceres approach actors.
/// The front star field, actor instruction programs, and physical motion stay compiled.
/// </summary>
public sealed class CeresFlightActorLayout
{
    /// <summary>Edited actor positions, or null when all five match their native placements.</summary>
    internal readonly CeresFlightActorPlacement[]? placements;

    /// <summary>Retains supplied placements only when at least one differs from the native layout.</summary>
    private CeresFlightActorLayout(CeresFlightActorPlacement[] placements)
    {
        bool stock = true;
        for (int index = 0; index < placements.Length; index++)
            stock &= placements[index] == DefaultPlacement(index);
        this.placements = stock ? null : placements;
    }

    /// <summary>Resolves one selected rear-view actor's starting screen placement, using its native default when the complete layout is unchanged.</summary>
    /// <param name="actorIndex">Zero-based native spawn-order index: large asteroids, station under attack, small asteroids, vortex, then rear stars.</param>
    /// <returns>The fixed actor identity and encoded whole-pixel X/Y coordinates for that slot.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside 0 through 4.</exception>
    public CeresFlightActorPlacement this[int actorIndex]
    {
        get
        {
            if ((uint)actorIndex >= CeresFlightActorDefinitions.RearViewActorCount)
                throw new IndexOutOfRangeException();
            return placements is null ? DefaultPlacement(actorIndex) : placements[actorIndex];
        }
    }

    /// <summary>Resolves native named-actor initializer placement without a second stock lookup.</summary>
    private static CeresFlightActorPlacement DefaultPlacement(int index)
    {
        var actor = CeresFlightActorDefinitions.RearViewActor(index);
        return new() { Id = CeresFlightActorDefinitions.RearViewPlacementSource(index).Id, X = actor.Position.X, Y = actor.Position.Y };
    }
    /// <summary>Identity of ordered selected actor IDs and both initial visual coordinates.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(CeresFlightActorLayout), content =>
    {
        content.Append("actors", CeresFlightActorDefinitions.RearViewActorCount);
        for (int index = 0; index < CeresFlightActorDefinitions.RearViewActorCount; index++)
        {
            CeresFlightActorPlacement placement = this[index];
            content.Append("id", System.Text.Encoding.UTF8.GetBytes(placement.Id));
            content.Append("x", placement.X);
            content.Append("y", placement.Y);
        }
    });

    /// <summary>Loads the supported layout schema, requiring all five fixed actor IDs in native order and unsigned sixteen-bit starting coordinates.</summary>
    /// <param name="json">Readable JSON stream, left open after deserialization.</param>
    /// <returns>Validated starting visual placements without changing instruction programs, physical motion, or the front-view star field.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema version, actor count/order/identity, or coordinate range is invalid.</exception>
    public static CeresFlightActorLayout Load(Stream json)
    {
        Ensure.NotNull(json);
        CeresFlightActorLayoutDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresFlightActorLayoutDocument>(json,
                MapPresentationFormat.JsonOptions)
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
                placement.Id != CeresFlightActorDefinitions.RearViewPlacementSource(index).Id ||
                placement.X is < 0 or > ushort.MaxValue ||
                placement.Y is < 0 or > ushort.MaxValue)
                throw new InvalidDataException(
                    $"Ceres flight actor {index} must retain its identity and 16-bit coordinates.");
        }
        return new CeresFlightActorLayout(document.Actors);
    }

    /// <summary>Serializes the editable layout as UTF-8 JSON and validates it through <see cref="Load"/> before writing any bytes to the destination.</summary>
    /// <param name="json">Destination stream, written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Supported-version document containing the five ordered actor placements.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The serialized document fails layout validation.</exception>
    public static void Write(Stream json, CeresFlightActorLayoutDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
