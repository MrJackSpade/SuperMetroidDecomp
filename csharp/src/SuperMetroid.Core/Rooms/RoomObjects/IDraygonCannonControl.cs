namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Draygon cannon control (#633).</summary>
public interface IDraygonCannonControl
{
    /// <summary>Stops Draygon firing from <paramref name="cannon"/>.</summary>
    void DisableDraygonCannon(DraygonWallCannon cannon);
}
