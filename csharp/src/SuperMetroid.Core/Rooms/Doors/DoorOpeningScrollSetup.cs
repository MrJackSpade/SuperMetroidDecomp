namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A door-opening scroll's direction, camera and destination (#633).</summary>
internal sealed record DoorOpeningScrollSetup
{
    /// <summary>Direction.</summary>
    public required DoorDirection Direction { get; init; }

    /// <summary>Samus's signed 16.16 displacement for one moving IRQ call, along the travel axis.</summary>
    public required FixedVector<RoomPixels> SamusIrqStep { get; init; }

    /// <summary>Remaining frames.</summary>
    public required int RemainingFrames { get; init; }

    /// <summary>The layer-one words <c>$0911/$0915</c> owned by the IRQ.</summary>
    public required Position16<RoomPixels> CameraPosition { get; init; }

    /// <summary>The layer-two words <c>$0917/$0919</c> owned by the IRQ.</summary>
    public required Position16<BackgroundPixels> Layer2Position { get; init; }

    /// <summary>Samus's 16.16 position as the IRQ carries her.</summary>
    public required FixedPosition<RoomPixels> SamusPosition { get; init; }

    /// <summary>Final camera position.</summary>
    public required Position16<RoomPixels> FinalCameraPosition { get; init; }

    /// <summary>Final layer2 position.</summary>
    public required Position16<BackgroundPixels> FinalLayer2Position { get; init; }

    /// <summary>Final samus position.</summary>
    public required FixedPosition<RoomPixels> FinalSamusPosition { get; init; }
}
