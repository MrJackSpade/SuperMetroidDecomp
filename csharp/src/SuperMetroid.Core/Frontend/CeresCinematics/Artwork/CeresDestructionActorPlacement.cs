namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Editable whole-pixel spawn position for one persistent Ceres destruction actor; its animation and motion identity remain fixed.</summary>
public sealed partial record CeresDestructionActorPlacement
{
    /// <summary>The validated spawn position as one screen-pixel value.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Offset16<ScreenPixels> Place => Offset16<ScreenPixels>.Of(X, Y);

    /// <summary>The actor the placement places, by what it shows (#1768): its installed key, not an editable behavior selector.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(CeresDestructionActorRoleJsonConverter))]
    public required CeresDestructionActorRole Role { get; init; }
    /// <summary>The actor's starting screen X, in whole pixels: negative lies left of the screen (#1627); whether the
    /// actor drifts round the drift loop is not installed.</summary>
    public required int X { get; init; }
    /// <summary>The actor's starting screen Y, in whole pixels: negative lies above the screen (#1627).</summary>
    public required int Y { get; init; }
}
