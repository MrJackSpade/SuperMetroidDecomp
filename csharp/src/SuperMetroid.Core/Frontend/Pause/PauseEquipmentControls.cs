using System.Collections.Immutable;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The equipment page's controls (#1772): each named with its column, its installed selector anchor and, for an upgrade,
/// the upgrade it toggles and its label; and each column as the ordered list the cursor moves along.
/// </summary>
internal static class PauseEquipmentControls
{
    /// <summary>The AUTO/MANUAL mode switch: where the cursor starts while Samus has reserve capacity, and returns after a manual refill.</summary>
    internal static readonly PauseReserveControl ReserveMode = new("Reserve.Mode");
    /// <summary>The manual reserve transfer.</summary>
    internal static readonly PauseReserveControl ReserveTransfer = new("Reserve.Transfer");

    /// <summary>CHARGE.</summary>
    internal static readonly PauseUpgradeControl Charge =
        new(PauseEquipmentCategory.Beams, "Beam.Charge", new BeamPauseUpgrade(SamusBeamFlags.Charge), PauseEquipmentLabelId.Charge);
    /// <summary>ICE.</summary>
    internal static readonly PauseUpgradeControl Ice =
        new(PauseEquipmentCategory.Beams, "Beam.Ice", new BeamPauseUpgrade(SamusBeamFlags.Ice), PauseEquipmentLabelId.Ice);
    /// <summary>WAVE.</summary>
    internal static readonly PauseUpgradeControl Wave =
        new(PauseEquipmentCategory.Beams, "Beam.Wave", new BeamPauseUpgrade(SamusBeamFlags.Wave), PauseEquipmentLabelId.Wave);
    /// <summary>SPAZER.</summary>
    internal static readonly PauseUpgradeControl Spazer =
        new(PauseEquipmentCategory.Beams, "Beam.Spazer", new BeamPauseUpgrade(SamusBeamFlags.Spazer), PauseEquipmentLabelId.Spazer);
    /// <summary>PLASMA.</summary>
    internal static readonly PauseUpgradeControl Plasma =
        new(PauseEquipmentCategory.Beams, "Beam.Plasma", new BeamPauseUpgrade(SamusBeamFlags.Plasma), PauseEquipmentLabelId.Plasma);

    /// <summary>VARIA SUIT.</summary>
    internal static readonly PauseUpgradeControl Varia =
        new(PauseEquipmentCategory.Suits, "Equipment.Varia", new ItemPauseUpgrade(SamusEquipmentFlags.VariaSuit), PauseEquipmentLabelId.Varia);
    /// <summary>GRAVITY SUIT.</summary>
    internal static readonly PauseUpgradeControl Gravity =
        new(PauseEquipmentCategory.Suits, "Equipment.Gravity", new ItemPauseUpgrade(SamusEquipmentFlags.GravitySuit), PauseEquipmentLabelId.Gravity);
    /// <summary>MORPHING BALL.</summary>
    internal static readonly PauseUpgradeControl MorphBall =
        new(PauseEquipmentCategory.Suits, "Equipment.MorphBall", new ItemPauseUpgrade(SamusEquipmentFlags.MorphBall), PauseEquipmentLabelId.MorphBall);
    /// <summary>BOMBS.</summary>
    internal static readonly PauseUpgradeControl Bombs =
        new(PauseEquipmentCategory.Suits, "Equipment.Bombs", new ItemPauseUpgrade(SamusEquipmentFlags.Bombs), PauseEquipmentLabelId.Bombs);
    /// <summary>SPRING BALL.</summary>
    internal static readonly PauseUpgradeControl SpringBall =
        new(PauseEquipmentCategory.Suits, "Equipment.SpringBall", new ItemPauseUpgrade(SamusEquipmentFlags.SpringBall), PauseEquipmentLabelId.SpringBall);
    /// <summary>SCREW ATTACK.</summary>
    internal static readonly PauseUpgradeControl ScrewAttack =
        new(PauseEquipmentCategory.Suits, "Equipment.ScrewAttack", new ItemPauseUpgrade(SamusEquipmentFlags.ScrewAttack), PauseEquipmentLabelId.ScrewAttack);

    /// <summary>HI-JUMP BOOTS.</summary>
    internal static readonly PauseUpgradeControl HiJump =
        new(PauseEquipmentCategory.Boots, "Boots.HiJump", new ItemPauseUpgrade(SamusEquipmentFlags.HiJumpBoots), PauseEquipmentLabelId.HiJump);
    /// <summary>SPACE JUMP.</summary>
    internal static readonly PauseUpgradeControl SpaceJump =
        new(PauseEquipmentCategory.Boots, "Boots.SpaceJump", new ItemPauseUpgrade(SamusEquipmentFlags.SpaceJump), PauseEquipmentLabelId.SpaceJump);
    /// <summary>SPEED BOOSTER.</summary>
    internal static readonly PauseUpgradeControl SpeedBooster =
        new(PauseEquipmentCategory.Boots, "Boots.SpeedBooster", new ItemPauseUpgrade(SamusEquipmentFlags.SpeedBooster), PauseEquipmentLabelId.SpeedBooster);

    /// <summary>The reserve column, top to bottom.</summary>
    internal static readonly ImmutableArray<PauseReserveControl> Reserves = [ReserveMode, ReserveTransfer];
    /// <summary>The beam column, top to bottom.</summary>
    internal static readonly ImmutableArray<PauseUpgradeControl> Beams = [Charge, Ice, Wave, Spazer, Plasma];
    /// <summary>The suit and miscellaneous column, top to bottom.</summary>
    internal static readonly ImmutableArray<PauseUpgradeControl> Suits = [Varia, Gravity, MorphBall, Bombs, SpringBall, ScrewAttack];
    /// <summary>The boots column, top to bottom.</summary>
    internal static readonly ImmutableArray<PauseUpgradeControl> Boots = [HiJump, SpaceJump, SpeedBooster];

    /// <summary>The upgrade columns in the order the cursor's search for the first collected upgrade visits them.</summary>
    internal static readonly ImmutableArray<ImmutableArray<PauseUpgradeControl>> UpgradeColumns = [Beams, Suits, Boots];
    /// <summary>Every upgrade control in page order, which is the order of the page's labels.</summary>
    internal static readonly ImmutableArray<PauseUpgradeControl> Upgrades = [.. Beams, .. Suits, .. Boots];
    /// <summary>Every control the cursor can rest on: the reserve column, then the upgrades in page order.</summary>
    internal static readonly ImmutableArray<PauseEquipmentControl> All = [.. Reserves, .. Upgrades];

    /// <summary>Right from the beams without Up held searches the suit column from here, past the two suits.</summary>
    internal static readonly PauseUpgradeControl BeamsRightSuitEntry = MorphBall;
}
