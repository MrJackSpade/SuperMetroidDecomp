namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What a refill station does to Samus's resource (#2484): tests whether it is full, and fills it.</summary>
internal static partial class StationRefills
{
    /// <summary>Whether Samus's resource that <paramref name="refill"/> refills is already at its current maximum.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="refill"/> is undefined.</exception>
    internal static bool IsFull(this StationRefill refill, IRoomSamus samus)
    {
        Ensure.NotNull(samus);
        return refill switch
        {
            StationRefill.Energy => samus.Health == samus.MaxHealth,
            StationRefill.Missiles => samus.Missiles == samus.MaxMissiles,
            _ => throw new InvalidOperationException($"Undefined {nameof(StationRefill)} {refill}."),
        };
    }

    /// <summary>Fills Samus's resource that <paramref name="refill"/> refills to its current maximum.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="refill"/> is undefined.</exception>
    internal static void Fill(this StationRefill refill, IRoomSamus samus)
    {
        Ensure.NotNull(samus);
        switch (refill)
        {
            case StationRefill.Energy:
                samus.Health = samus.MaxHealth;
                return;
            case StationRefill.Missiles:
                samus.Missiles = samus.MaxMissiles;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(StationRefill)} {refill}.");
        }
    }
}
