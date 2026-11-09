namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a grey door cap waits for on each update (#1554).</summary>
internal enum GreyDoorWait
{
    /// <summary>Nothing: the cap is not yet locked, or is opening.</summary>
    Nothing,
    /// <summary>Its condition: until it holds, a shot makes the dud sound.</summary>
    Condition,
    /// <summary>A shot, which opens it.</summary>
    Shot,
}
