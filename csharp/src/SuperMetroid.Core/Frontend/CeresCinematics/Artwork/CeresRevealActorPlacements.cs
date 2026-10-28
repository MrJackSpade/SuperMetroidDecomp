using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Editable initial screen positions for the six Ceres-to-Zebes reveal actors.
/// Their native instruction lists, slide acceleration, deletion, and phase handoffs
/// remain in compiled cartridge mechanics.
/// </summary>
public sealed class CeresRevealActorPlacements
{
    /// <summary>Explicit authored positions, or null when all six actors use their compiled native placements.</summary>
    internal readonly CeresRevealActorPlacement[]? placements;

    /// <summary>Collapses an all-stock layout to native placement lookup while retaining edited actor positions.</summary>
    private CeresRevealActorPlacements(CeresRevealActorPlacement[] placements)
    {
        bool stock = true;
        for (int index = 0; index < placements.Length; index++)
            stock &= placements[index] == DefaultPlacement(index);
        this.placements = stock ? null : placements;
    }

    /// <summary>Returns the selected initial placement for one actor in the native $8B:C810-C831 Zebes-reveal spawn order.</summary>
    /// <param name="actorIndex">Index 0..5: planet, upper-left stars, upper-right stars, lower-left stars, lower-right stars, then Planet Zebes title.</param>
    /// <returns>Named screen-pixel placement used when creating the actor, without changing its instruction list or motion policy.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The actor index is outside 0..5.</exception>
    public CeresRevealActorPlacement this[int actorIndex]
    {
        get
        {
            if ((uint)actorIndex >= CeresDestructionActorDefinitions.ZebesActorCount)
                throw new ArgumentOutOfRangeException(nameof(actorIndex));
            return placements is null ? DefaultPlacement(actorIndex) : placements[actorIndex];
        }
    }

    /// <summary>Resolves native named-actor initializer placement without a second stock lookup.</summary>
    private static CeresRevealActorPlacement DefaultPlacement(int index)
    {
        var actor = CeresDestructionActors.ZebesActor(index);
        return new() { Id = CeresDestructionActors.ZebesPlacementId(index), X = actor.Position.X, Y = actor.Position.Y };
    }
    /// <summary>Identity of ordered selected actor IDs and both initial visual coordinates.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("CeresRevealActorLayout", content =>
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

    /// <summary>Validates the six fixed reveal-actor identities and compiles their editable initial positions from ceres-zebes-reveal-actors.json.</summary>
    /// <param name="json">UTF-8 JSON source read from its current position and left open.</param>
    /// <returns>Selected placements for the reveal, with stock positions resolved from the compiled native initializers.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema version, six-entry order, actor identity, or unsigned 16-bit coordinate is invalid.</exception>
    public static CeresRevealActorPlacements Load(Stream json)
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

    /// <summary>Serializes the editable layout to UTF-8 JSON, validates it through <see cref="Load"/>, then writes the validated bytes.</summary>
    /// <param name="json">Destination written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Ordered actor placements to serialize; the document is not retained.</param>
    /// <exception cref="ArgumentNullException">The destination stream is null.</exception>
    /// <exception cref="InvalidDataException">The serialized document fails schema, identity, order, or coordinate validation.</exception>
    public static void Write(Stream json, CeresRevealActorLayoutDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
