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

    /// <summary>Each SamusEquipmentFlags flag on its own in the order a save and an installed document list a set of them (#2567), written out beside the keys so reordering the members never changes the bytes written; the empty set is no flag.</summary>
    internal static IReadOnlyList<SamusEquipmentFlags> ListedOrder { get; } =
    [
        SamusEquipmentFlags.VariaSuit,
        SamusEquipmentFlags.SpringBall,
        SamusEquipmentFlags.MorphBall,
        SamusEquipmentFlags.ScrewAttack,
        SamusEquipmentFlags.GravitySuit,
        SamusEquipmentFlags.HiJumpBoots,
        SamusEquipmentFlags.SpaceJump,
        SamusEquipmentFlags.Bombs,
        SamusEquipmentFlags.SpeedBooster,
        SamusEquipmentFlags.GrappleBeam,
        SamusEquipmentFlags.XrayScope,
    ];
}
