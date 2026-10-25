namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>What a demo scene does to its room once the room has loaded.</summary>
public enum AttractDemoRoomSetup
{
    /// <summary>The Charge Beam demo closes one screen's scroll with a red boundary.</summary>
    ChargeBeamScroll,
    /// <summary>The room is left as loaded.</summary>
    NoOp,
    /// <summary>The Landing Site shows its scrolling sky's tall layer-2 map.</summary>
    LandingSiteSky,
    /// <summary>Kraid's body waits before its next action.</summary>
    KraidTimer,
    /// <summary>Brinstar's area boss counts as defeated.</summary>
    DefeatedKraid,
}
