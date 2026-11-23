namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>One named cinematic actor's initial whole-pixel screen position; animation, colouring, sliding, and deletion are the game's own.</summary>
public sealed partial record CeresRevealActorPlacement
{
    /// <summary>The validated spawn position as one screen-pixel value.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Offset16<ScreenPixels> Place => Offset16<ScreenPixels>.Of(X, Y);

    /// <summary>The actor the placement places, by what it shows (#1768): its installed key.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(ZebesRevealActorRoleJsonConverter))]
    public required ZebesRevealActorRole Role { get; init; }
    /// <summary>The actor's starting screen X, in whole pixels: negative lies left of the screen (#1627).</summary>
    public required int X { get; init; }
    /// <summary>The actor's starting screen Y, in whole pixels: negative lies above the screen and it increases
    /// downward (#1627).</summary>
    public required int Y { get; init; }
}
