namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The direction Samus travels through a door: bits 0-1 of the door header's orientation
/// byte (<c>DoorDirection</c>, <c>$0791</c>). Consumers such as <c>$80:AD3E</c> and
/// <c>$82:E358</c> mask these two bits and dispatch on the four values.
/// </summary>
public enum DoorDirection : byte
{
    /// <summary>Direction 0: the destination lies to the right.</summary>
    Right = 0,
    /// <summary>Direction 1: the destination lies to the left.</summary>
    Left = 1,
    /// <summary>Direction 2: the destination lies below.</summary>
    Down = 2,
    /// <summary>Direction 3: the destination lies above.</summary>
    Up = 3,
}
