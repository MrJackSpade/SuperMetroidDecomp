namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// The installed key of each <see cref="PauseEquipmentLabelId"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class PauseEquipmentLabelIdKeys
{
    /// <summary>Each PauseEquipmentLabelId member's installed key.</summary>
    internal static InstalledKeys<PauseEquipmentLabelId> Installed { get; } = new(new Dictionary<PauseEquipmentLabelId, string>
    {
        [PauseEquipmentLabelId.Charge] = "Charge",
        [PauseEquipmentLabelId.Ice] = "Ice",
        [PauseEquipmentLabelId.Wave] = "Wave",
        [PauseEquipmentLabelId.Spazer] = "Spazer",
        [PauseEquipmentLabelId.Plasma] = "Plasma",
        [PauseEquipmentLabelId.Varia] = "Varia",
        [PauseEquipmentLabelId.Gravity] = "Gravity",
        [PauseEquipmentLabelId.MorphBall] = "MorphBall",
        [PauseEquipmentLabelId.Bombs] = "Bombs",
        [PauseEquipmentLabelId.SpringBall] = "SpringBall",
        [PauseEquipmentLabelId.ScrewAttack] = "ScrewAttack",
        [PauseEquipmentLabelId.HiJump] = "HiJump",
        [PauseEquipmentLabelId.SpaceJump] = "SpaceJump",
        [PauseEquipmentLabelId.SpeedBooster] = "SpeedBooster",
        [PauseEquipmentLabelId.Hyper] = "Hyper",
    });
}
