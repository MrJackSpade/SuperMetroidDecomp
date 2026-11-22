using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>What the HUD reads of Samus's movement and posture.</summary>
internal static partial class SamusHudObservations
{
    /// <summary>The HUD selection mode of <paramref name="movement"/> (<see cref="SamusMovementTypeDefinitions"/>).</summary>
    internal static SamusHudSelectionMode SelectionMode(SamusMovementType movement) =>
        SamusMovementTypeDefinitions.Movements[movement].HudSelection;
}
