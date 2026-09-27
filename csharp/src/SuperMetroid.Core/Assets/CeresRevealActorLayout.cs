using System.Text.Json;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable initial screen positions for the six Ceres-to-Zebes reveal actors.
/// Their native instruction lists, slide acceleration, deletion, and phase handoffs
/// remain in compiled cartridge mechanics.
/// </summary>
public sealed class CeresRevealActorLayout
{
    private readonly CeresRevealActorPlacement[] placements;

    private CeresRevealActorLayout(CeresRevealActorPlacement[] placements) =>
        this.placements = placements;

    public CeresRevealActorPlacement this[int actorIndex] => placements[actorIndex];

    public static CeresRevealActorLayout Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CeresRevealActorLayoutDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresRevealActorLayoutDocument>(json,
                MapPresentationFormat.JsonOptions)
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
                placement.Id != CeresDestructionActorDefinitions.ZebesPlacementSources[index].Id ||
                placement.X is < 0 or > ushort.MaxValue ||
                placement.Y is < 0 or > ushort.MaxValue)
                throw new InvalidDataException(
                    $"Ceres reveal actor {index} must retain its identity and 16-bit coordinates.");
        }
        return new CeresRevealActorLayout(document.Actors);
    }

    public static void Write(Stream json, CeresRevealActorLayoutDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record CeresRevealActorPlacement
{
    public required string Id { get; init; }
    public required int X { get; init; }
    public required int Y { get; init; }
}

public sealed record CeresRevealActorLayoutDocument
{
    public required int Version { get; init; }
    public required CeresRevealActorPlacement[] Actors { get; init; }
}

public static class CeresRevealActorLayoutFormat
{
    public const int Version = 1;
    public const string FileName = "ceres-zebes-reveal-actors.json";
}
