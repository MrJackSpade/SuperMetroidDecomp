using System.Text.Json;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable spawn positions for the three persistent actors behind Ceres's explosion.
/// Their instruction lists, wrap behavior, and movement remain compiled mechanics.
/// </summary>
public sealed class CeresDestructionActorLayout
{
    private readonly CeresDestructionActorPlacement[]? placements;

    private CeresDestructionActorLayout(CeresDestructionActorPlacement[] placements)
    {
        bool stock = true;
        for (int index = 0; index < placements.Length; index++)
            stock &= placements[index] == DefaultPlacement(index);
        this.placements = stock ? null : placements;
    }

    public CeresDestructionActorPlacement this[int actorIndex]
    {
        get
        {
            if ((uint)actorIndex >= CeresDestructionActorDefinitions.InitialActorCount)
                throw new IndexOutOfRangeException();
            return placements is null ? DefaultPlacement(actorIndex) : placements[actorIndex];
        }
    }

    /// <summary>Resolves native named-actor initializer placement without a second stock lookup.</summary>
    private static CeresDestructionActorPlacement DefaultPlacement(int index)
    {
        var actor = CeresDestructionActorDefinitions.InitialActor(index);
        return new() { Id = CeresDestructionActorDefinitions.InitialPlacementId(index), X = actor.X, Y = actor.Y };
    }
    /// <summary>Identity of ordered selected actor IDs and both initial visual coordinates.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(CeresDestructionActorLayout), content =>
    {
        content.Append("actors", CeresDestructionActorDefinitions.InitialActorCount);
        for (int index = 0; index < CeresDestructionActorDefinitions.InitialActorCount; index++)
        {
            CeresDestructionActorPlacement placement = this[index];
            content.Append("id", System.Text.Encoding.UTF8.GetBytes(placement.Id));
            content.Append("x", placement.X);
            content.Append("y", placement.Y);
        }
    });

    public static CeresDestructionActorLayout Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CeresDestructionActorLayoutDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresDestructionActorLayoutDocument>(json,
                MapPresentationFormat.JsonOptions)
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
                placement.Id != CeresDestructionActorDefinitions.InitialPlacementId(index) ||
                placement.X is < 0 or > ushort.MaxValue ||
                placement.Y is < 0 or > ushort.MaxValue)
                throw new InvalidDataException(
                    $"Ceres destruction actor {index} must retain its identity and 16-bit coordinates.");
        }
        return new CeresDestructionActorLayout(document.Actors);
    }

    public static void Write(Stream json, CeresDestructionActorLayoutDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record CeresDestructionActorPlacement
{
    public required string Id { get; init; }
    public required int X { get; init; }
    public required int Y { get; init; }
}

public sealed record CeresDestructionActorLayoutDocument
{
    public required int Version { get; init; }
    public required CeresDestructionActorPlacement[] Actors { get; init; }
}

public static class CeresDestructionActorLayoutFormat
{
    public const int Version = 1;
    public const string FileName = "ceres-destruction-actors.json";
}
