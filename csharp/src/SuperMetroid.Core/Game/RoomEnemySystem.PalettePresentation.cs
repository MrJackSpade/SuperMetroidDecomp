using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>Host-owned authored damage colors, rebound after debugger-state restoration.</summary>
    [field: NonSerialized]
    public MotherBrainHealthPalettePresentation? MotherBrainHealthColors { get; set; }

    /// <summary>Host-owned rainbow, drain and revival colors, rebound after debugger-state restoration.</summary>
    [field: NonSerialized]
    public MotherBrainRainbowPalettePresentation? MotherBrainRainbowColors { get; set; }

    /// <summary>Host-owned fake-death room flash and phase-two setup colors.</summary>
    [field: NonSerialized]
    public MotherBrainRoomColorPresentation? MotherBrainRoomColors { get; set; }
}
