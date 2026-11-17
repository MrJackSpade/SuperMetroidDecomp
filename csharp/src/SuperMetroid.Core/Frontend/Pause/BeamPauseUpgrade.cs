using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>A beam a beam-column control stands for.</summary>
/// <param name="Beam">The beam.</param>
public sealed record BeamPauseUpgrade(SamusBeamFlags Beam) : PauseUpgrade
{
    /// <inheritdoc/>
    public override bool IsIn(SamusBeamFlags beams, SamusEquipmentFlags items) => (beams & Beam) != SamusBeamFlags.None;

    /// <inheritdoc/>
    public override SamusBeamFlags AddedTo(SamusBeamFlags beams) => beams | Beam;

    /// <inheritdoc/>
    public override SamusEquipmentFlags AddedTo(SamusEquipmentFlags items) => items;

    /// <inheritdoc/>
    internal override void ToggleEquipped(SamusState samus) => Ensure.NotNull(samus).EquippedBeams ^= Beam;
}
