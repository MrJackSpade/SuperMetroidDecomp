namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A door-opening scroll's direction, camera and destination (#633).</summary>
internal sealed record DoorOpeningScrollSetup
{
    /// <summary>The direction the door scrolls the screen.</summary>
    public required DoorDirection Direction { get; init; }

    /// <summary>Samus's signed 16.16 displacement for one scroll update, along the travel axis.</summary>
    public required FixedVector<RoomPixels> SamusScrollStep { get; init; }

    /// <summary>The scroll updates left before the scroll reaches its destination.</summary>
    public required int RemainingFrames { get; init; }

    /// <summary>The camera's position as the scroll carries it.</summary>
    public required Position16<RoomPixels> CameraPosition { get; init; }

    /// <summary>The second background layer's position as the scroll carries it.</summary>
    public required Position16<BackgroundPixels> Layer2Position { get; init; }

    /// <summary>Samus's 16.16 position as the scroll carries her.</summary>
    public required FixedPosition<RoomPixels> SamusPosition { get; init; }

    /// <summary>The camera's position at the scroll's end.</summary>
    public required Position16<RoomPixels> FinalCameraPosition { get; init; }

    /// <summary>The second background layer's position at the scroll's end.</summary>
    public required Position16<BackgroundPixels> FinalLayer2Position { get; init; }

    /// <summary>Samus's position at the scroll's end.</summary>
    public required FixedPosition<RoomPixels> FinalSamusPosition { get; init; }
}
