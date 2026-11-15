namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One authored component of Ridley's death breakup, which a breakup fragment's spawn settings name.</summary>
internal enum RidleyExplosionPart
{
    /// <summary>The tail's base segment, where it meets the body.</summary>
    TailBaseSegment,
    /// <summary>The second tail segment counted from the body.</summary>
    TailSecondSegment,
    /// <summary>The third tail segment counted from the body.</summary>
    TailThirdSegment,
    /// <summary>The fourth tail segment counted from the body.</summary>
    TailFourthSegment,
    /// <summary>The fifth tail segment counted from the body.</summary>
    TailFifthSegment,
    /// <summary>The sixth tail segment counted from the body.</summary>
    TailSixthSegment,
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
