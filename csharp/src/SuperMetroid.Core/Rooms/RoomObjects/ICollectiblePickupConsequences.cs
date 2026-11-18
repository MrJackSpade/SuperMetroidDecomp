namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What the gameplay frame does for a pickup's consequence (#2383).</summary>
internal interface ICollectiblePickupConsequences
{
    /// <summary>Shows the newly equipped beam combination's colours.</summary>
    void ShowEquippedBeamColours();

    /// <summary>Holds the transformation to <paramref name="suit"/> until the pickup's message closes.</summary>
    void AwaitSuitTransformation(SamusSuitPickupKind suit);
}
