using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>Host-owned authored damage colors, rebound after debugger-state restoration.</summary>
    [field: NonSerialized]
    public MotherBrainHealthPalettePresentation? MotherBrainHealthColors { get; set; }
}
