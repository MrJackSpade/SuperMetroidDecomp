namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The room liquid's motion words room PLMs write and read (#1395). The room FX state implements it.</summary>
public interface IRoomFxMotion
{
    /// <summary>The liquid's base surface.</summary>
    SnesFixedPosition BaseY { get; }

    /// <summary>The surface the liquid moves toward.</summary>
    Position16<RoomPixels> TargetSurface { get; }

    /// <summary>Writes the shared liquid motion words bank $84 writes; each null word is left as it is.</summary>
    void ApplyCartridgeMotionWrites(
        ushort? baseYPosition = null,
        ushort? targetYPosition = null,
        ushort? packedYVelocity = null,
        ushort? timer = null);
}
