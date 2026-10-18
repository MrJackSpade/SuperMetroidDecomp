using SuperMetroid.Core.Rooms;

namespace SuperMetroid.RenderVerification.Rooms;

/// <summary>One retail door and the destination room its transition reaches (#633).</summary>
internal readonly record struct RetailDoorCase
{
    /// <summary>Door.</summary>
    public required ushort Door { get; init; }

    /// <summary>Destination.</summary>
    public required RoomHeaderDefinition Destination { get; init; }
}
