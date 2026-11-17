namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>How much each ammunition tank Samus collects adds to her capacity and to what she carries; an energy or reserve tank adds <see cref="Progression.SamusEnergyDefinitions.EnergyPerTank"/>.</summary>
internal static class CollectibleCapacityDefinitions
{
    /// <summary>The missiles a missile tank adds to Samus's maximum and to the missiles she carries.</summary>
    internal const int MissileTankMissiles = 5;

    /// <summary>The super missiles a super missile tank adds to Samus's maximum and to the super missiles she carries.</summary>
    internal const int SuperMissileTankSuperMissiles = 5;

    /// <summary>The power bombs a power bomb tank adds to Samus's maximum and to the power bombs she carries.</summary>
    internal const int PowerBombTankPowerBombs = 5;
}
