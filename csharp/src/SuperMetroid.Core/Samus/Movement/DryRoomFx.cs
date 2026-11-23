namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The room effects with no liquid, as Samus's liquid physics configures them (#2553).</summary>
internal enum DryRoomFx
{
    /// <summary>No effect.</summary>
    None,

    /// <summary>Spores.</summary>
    Spores,

    /// <summary>Rain.</summary>
    Rain,

    /// <summary>Fog.</summary>
    Fog,

    /// <summary>The scrolling sky.</summary>
    ScrollingSky,

    /// <summary>The Fireflea room's darkness.</summary>
    Fireflea,

    /// <summary>Ceres Ridley's room.</summary>
    CeresRidley,

    /// <summary>The Ceres elevator room.</summary>
    CeresElevator,

    /// <summary>The Ceres haze.</summary>
    CeresHaze,
}
