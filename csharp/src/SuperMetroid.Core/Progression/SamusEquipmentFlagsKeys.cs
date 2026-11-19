namespace SuperMetroid.Core.Progression;

/// <summary>
/// The installed key of each <see cref="SamusEquipmentFlags"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class SamusEquipmentFlagsKeys
{
    /// <summary>Each SamusEquipmentFlags member's installed key.</summary>
    internal static InstalledKeys<SamusEquipmentFlags> Installed { get; } = new(new Dictionary<SamusEquipmentFlags, string>
    {
        [SamusEquipmentFlags.None] = "None",
        [SamusEquipmentFlags.VariaSuit] = "VariaSuit",
        [SamusEquipmentFlags.SpringBall] = "SpringBall",
        [SamusEquipmentFlags.MorphBall] = "MorphBall",
        [SamusEquipmentFlags.ScrewAttack] = "ScrewAttack",
        [SamusEquipmentFlags.GravitySuit] = "GravitySuit",
        [SamusEquipmentFlags.HiJumpBoots] = "HiJumpBoots",
        [SamusEquipmentFlags.SpaceJump] = "SpaceJump",
        [SamusEquipmentFlags.Bombs] = "Bombs",
        [SamusEquipmentFlags.SpeedBooster] = "SpeedBooster",
        [SamusEquipmentFlags.GrappleBeam] = "GrappleBeam",
        [SamusEquipmentFlags.XrayScope] = "XrayScope",
    });
}
