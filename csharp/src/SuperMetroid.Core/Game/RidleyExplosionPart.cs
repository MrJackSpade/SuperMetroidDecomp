namespace SuperMetroid.Core.Game;

/// <summary>Parameters that select one authored component of Ridley's death breakup.</summary>
internal enum RidleyExplosionPart : ushort
{
    /// <summary>First tail segment.</summary>
    Tail0 = 0x0000,
    /// <summary>Second tail segment.</summary>
    Tail1 = 0x0002,
    /// <summary>Third tail segment.</summary>
    Tail2 = 0x0004,
    /// <summary>Fourth tail segment.</summary>
    Tail3 = 0x0006,
    /// <summary>Fifth tail segment.</summary>
    Tail4 = 0x0008,
    /// <summary>Sixth tail segment.</summary>
    Tail5 = 0x000a,
    /// <summary>Seventh segment, whose image is selected from the combined tip angle.</summary>
    TailTip = 0x000c,
    /// <summary>Wing fragment.</summary>
    Wings = 0x000e,
    /// <summary>Leg fragment.</summary>
    Legs = 0x0010,
    /// <summary>Open-head and neck fragment.</summary>
    OpenHeadAndNeck = 0x0012,
    /// <summary>Torso fragment.</summary>
    Torso = 0x0014,
    /// <summary>Claw fragment.</summary>
    Claw = 0x0016,
}
