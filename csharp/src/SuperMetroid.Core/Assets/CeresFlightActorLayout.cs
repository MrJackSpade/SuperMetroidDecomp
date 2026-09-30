using System.Text.Json;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable starting screen positions for the five rear-view Ceres approach actors.
/// The front star field, actor instruction programs, and physical motion stay compiled.
/// </summary>
public sealed class CeresFlightActorLayout
{
    private readonly CeresFlightActorPlacement[] placements;

    private CeresFlightActorLayout(CeresFlightActorPlacement[] placements) =>
        this.placements = placements;

    public CeresFlightActorPlacement this[int actorIndex] => placements[actorIndex];

    /// <summary>Identity of ordered selected actor IDs and both initial visual coordinates.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(CeresFlightActorLayout), content =>
    {
        content.Append("actors", placements.Length);
        foreach (CeresFlightActorPlacement placement in placements)
        {
            content.Append("id", System.Text.Encoding.UTF8.GetBytes(placement.Id));
            content.Append("x", placement.X);
            content.Append("y", placement.Y);
        }
    });

    public static CeresFlightActorLayout Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
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
                placement.Id != CeresFlightActorDefinitions.RearViewPlacementSources[index].Id ||
                placement.X is < 0 or > ushort.MaxValue ||
                placement.Y is < 0 or > ushort.MaxValue)
                throw new InvalidDataException(
                    $"Ceres flight actor {index} must retain its identity and 16-bit coordinates.");
        }
        return new CeresFlightActorLayout(document.Actors);
    }

    public static void Write(Stream json, CeresFlightActorLayoutDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record CeresFlightActorPlacement
{
    public required string Id { get; init; }
    public required int X { get; init; }
    public required int Y { get; init; }
}

public sealed record CeresFlightActorLayoutDocument
{
    public required int Version { get; init; }
    public required CeresFlightActorPlacement[] Actors { get; init; }
}

public static class CeresFlightActorLayoutFormat
{
    public const int Version = 1;
    public const string FileName = "ceres-flight-actors.json";
}
