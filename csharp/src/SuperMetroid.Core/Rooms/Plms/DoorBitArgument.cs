namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The opened-door bit a door cap records, and whether opening the cap records it.</summary>
/// <param name="DoorBit">The bit's index in the opened-doors record.</param>
/// <param name="Persistence">Whether opening the cap sets the bit and an already set bit keeps the cap open.</param>
public sealed record DoorBitArgument(int DoorBit, DoorOpeningPersistence Persistence) : PlmRoomArgument
{
    /// <summary>Whether opening the cap sets the bit and an already set bit keeps the cap open.</summary>
    public bool Persists => Persistence == DoorOpeningPersistence.Persistent;
}
