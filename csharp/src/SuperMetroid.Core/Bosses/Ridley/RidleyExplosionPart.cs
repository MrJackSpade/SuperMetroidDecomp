namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One authored component of Ridley's death breakup; <see cref="RidleyExplosionDefinitions.PartParameters"/> holds the spawn parameter selecting each.</summary>
internal enum RidleyExplosionPart : ushort
{
    /// <summary>First tail segment.</summary>
    Tail0,
    /// <summary>Second tail segment.</summary>
    Tail1,
    /// <summary>Third tail segment.</summary>
    Tail2,
    /// <summary>Fourth tail segment.</summary>
    Tail3,
    /// <summary>Fifth tail segment.</summary>
    Tail4,
    /// <summary>Sixth tail segment.</summary>
    Tail5,
    /// <summary>Seventh segment, whose image is selected from the combined tip angle.</summary>
    TailTip,
    /// <summary>Wing fragment.</summary>
    Wings,
    /// <summary>Leg fragment.</summary>
    Legs,
    /// <summary>Open-head and neck fragment.</summary>
    OpenHeadAndNeck,
    /// <summary>Torso fragment.</summary>
    Torso,
    /// <summary>Claw fragment.</summary>
    Claw,
}
