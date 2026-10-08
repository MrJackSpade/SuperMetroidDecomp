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

    /// <summary>Resolves the selected starting placement of one persistent destruction-scene actor, using native defaults when the complete layout is unchanged.</summary>
    /// <param name="actorIndex">Zero-based $8B:C27C-$C295 spawn-order index: large asteroids, small asteroids, then the stationary vortex.</param>
    /// <returns>The fixed actor identity and encoded whole-pixel starting screen coordinates.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside 0 through 2.</exception>
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

    /// <summary>Loads the supported layout schema, requiring the three persistent actor IDs in native order and unsigned sixteen-bit starting coordinates.</summary>
    /// <param name="json">Readable JSON stream, left open after deserialization.</param>
    /// <returns>Validated persistent-actor placements, without changing asteroid motion/wrap, vortex behavior, explosion spawn events, or later Zebes artwork.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema version, actor count/order/identity, or coordinate range is invalid.</exception>
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

    /// <summary>Serializes the selected destruction placements as UTF-8 JSON and validates them through <see cref="Load"/> before writing the destination.</summary>
    /// <param name="json">Destination stream, written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Supported-version document containing the three ordered persistent-actor placements.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The serialized document fails layout validation.</exception>
    public static void Write(Stream json, CeresDestructionActorLayoutDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable whole-pixel spawn position for one persistent Ceres destruction actor; its native instruction and motion identity remain fixed.</summary>
public sealed record CeresDestructionActorPlacement
{
    /// <summary>Case-sensitive persistent actor-role key required to match its ordered slot, not an editable behavior selector.</summary>
    public required string Id { get; init; }
    /// <summary>Initial screen X word encoded as an integer in 0..65535; negative off-screen origins use native two's-complement encoding, and the actor's compiled wrap policy remains unchanged.</summary>
    public required int X { get; init; }
    /// <summary>Initial whole-pixel screen Y word encoded as an integer in 0..65535, without a visible-viewport restriction.</summary>
    public required int Y { get; init; }
}

/// <summary>Editable JSON payload for persistent destruction-scene placements, separate from generated explosion bursts and the later Zebes reveal layout.</summary>
public sealed record CeresDestructionActorLayoutDocument
{
    /// <summary>Schema revision required to equal <see cref="CeresDestructionActorLayoutFormat.Version"/> during loading.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly three nonnull placements in native spawn order: <c>large-asteroid</c>, <c>small-asteroid</c>, and <c>vortex</c>.</summary>
    public required CeresDestructionActorPlacement[] Actors { get; init; }
}

/// <summary>Installed resource identity and supported JSON schema version for Ceres's three persistent destruction actors.</summary>
public static class CeresDestructionActorLayoutFormat
{
    /// <summary>Supported destruction-actor placement schema revision one.</summary>
    public const int Version = 1;
    /// <summary>Installed persistent-actor placement resource, <c>ceres-destruction-actors.json</c>.</summary>
    public const string FileName = "ceres-destruction-actors.json";
}
