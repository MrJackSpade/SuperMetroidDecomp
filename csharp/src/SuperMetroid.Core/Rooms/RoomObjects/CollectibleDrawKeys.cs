namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed key of each <see cref="CollectibleDraw"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class CollectibleDrawKeys
{
    /// <summary>Each CollectibleDraw member's installed key.</summary>
    internal static InstalledKeys<CollectibleDraw> Installed { get; } = new(new Dictionary<CollectibleDraw, string>
    {
        [CollectibleDraw.Empty] = "Empty",
        [CollectibleDraw.ChozoOrbDim] = "ChozoOrbDim",
        [CollectibleDraw.ChozoOrbGlowing] = "ChozoOrbGlowing",
        [CollectibleDraw.ChozoOrbBright] = "ChozoOrbBright",
        [CollectibleDraw.ChozoOrbBurst] = "ChozoOrbBurst",
        [CollectibleDraw.EnergyTankBright] = "EnergyTankBright",
        [CollectibleDraw.EnergyTankDim] = "EnergyTankDim",
        [CollectibleDraw.MissileTankDim] = "MissileTankDim",
        [CollectibleDraw.MissileTankBright] = "MissileTankBright",
        [CollectibleDraw.SuperMissileTankDim] = "SuperMissileTankDim",
        [CollectibleDraw.SuperMissileTankBright] = "SuperMissileTankBright",
        [CollectibleDraw.PowerBombTankBright] = "PowerBombTankBright",
        [CollectibleDraw.PowerBombTankDim] = "PowerBombTankDim",
        [CollectibleDraw.FirstBlockPairPrimaryLook] = "FirstBlockPairPrimaryLook",
        [CollectibleDraw.FirstBlockPairAlternateLook] = "FirstBlockPairAlternateLook",
        [CollectibleDraw.SecondBlockPairPrimaryLook] = "SecondBlockPairPrimaryLook",
        [CollectibleDraw.SecondBlockPairAlternateLook] = "SecondBlockPairAlternateLook",
        [CollectibleDraw.ThirdBlockPairPrimaryLook] = "ThirdBlockPairPrimaryLook",
        [CollectibleDraw.ThirdBlockPairAlternateLook] = "ThirdBlockPairAlternateLook",
        [CollectibleDraw.FourthBlockPairPrimaryLook] = "FourthBlockPairPrimaryLook",
        [CollectibleDraw.FourthBlockPairAlternateLook] = "FourthBlockPairAlternateLook",
        [CollectibleDraw.ShotBlockIntact] = "ShotBlockIntact",
        [CollectibleDraw.ShotBlockCracking] = "ShotBlockCracking",
        [CollectibleDraw.ShotBlockDust] = "ShotBlockDust",
    });
}
