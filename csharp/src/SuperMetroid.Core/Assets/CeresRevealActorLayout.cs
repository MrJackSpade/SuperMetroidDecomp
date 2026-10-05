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
    private readonly CeresRevealActorPlacement[]? placements;

    private CeresRevealActorLayout(CeresRevealActorPlacement[] placements)
    {
        bool stock = true;
        for (int index = 0; index < placements.Length; index++)
            stock &= placements[index] == DefaultPlacement(index);
        this.placements = stock ? null : placements;
    }

    public CeresRevealActorPlacement this[int actorIndex]
    {
        get
        {
            if ((uint)actorIndex >= CeresDestructionActorDefinitions.ZebesActorCount)
                throw new IndexOutOfRangeException();
            return placements is null ? DefaultPlacement(actorIndex) : placements[actorIndex];
        }
    }

    /// <summary>Resolves native named-actor initializer placement without a second stock lookup.</summary>
    private static CeresRevealActorPlacement DefaultPlacement(int index)
    {
        var actor = CeresDestructionActorDefinitions.ZebesActor(index);
        return new() { Id = CeresDestructionActorDefinitions.ZebesPlacementSource(index).Id, X = actor.X, Y = actor.Y };
    }
    /// <summary>Identity of ordered selected actor IDs and both initial visual coordinates.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(CeresRevealActorLayout), content =>
    {
        content.Append("actors", CeresDestructionActorDefinitions.ZebesActorCount);
        for (int index = 0; index < CeresDestructionActorDefinitions.ZebesActorCount; index++)
        {
            CeresRevealActorPlacement placement = this[index];
            content.Append("id", System.Text.Encoding.UTF8.GetBytes(placement.Id));
            content.Append("x", placement.X);
            content.Append("y", placement.Y);
        }
    });

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
                placement.Id != CeresDestructionActorDefinitions.ZebesPlacementSource(index).Id ||
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
