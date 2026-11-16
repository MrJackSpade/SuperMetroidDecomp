namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>How much each tank Samus collects adds to her capacity, and, for an ammunition tank, to what she carries.</summary>
internal static class CollectibleCapacityDefinitions
{
    /// <summary>The energy an energy tank adds to Samus's maximum health; collecting one also fills her health.</summary>
    internal const int EnergyTankEnergy = 100;

    /// <summary>The energy a reserve tank adds to Samus's reserve capacity.</summary>
    internal const int ReserveTankEnergy = 100;

    /// <summary>The missiles a missile tank adds to Samus's maximum and to the missiles she carries.</summary>
    internal const int MissileTankMissiles = 5;

    /// <summary>The super missiles a super missile tank adds to Samus's maximum and to the super missiles she carries.</summary>
    internal const int SuperMissileTankSuperMissiles = 5;

    /// <summary>The power bombs a power bomb tank adds to Samus's maximum and to the power bombs she carries.</summary>
    internal const int PowerBombTankPowerBombs = 5;
}
