namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One authored component of Ridley's death breakup, which a breakup fragment's spawn settings name.</summary>
internal enum RidleyExplosionPart
{
    /// <summary>The tail's base link, at Ridley's hip, which becomes this fragment.</summary>
    TailBase,
    /// <summary>The tail's first shaft link from the body, which becomes this fragment.</summary>
    TailFirstShaftLink,
    /// <summary>The tail's second shaft link from the body, which becomes this fragment.</summary>
    TailSecondShaftLink,
    /// <summary>The tail's third shaft link from the body, which becomes this fragment.</summary>
    TailThirdShaftLink,
    /// <summary>The tail's fourth shaft link from the body, which becomes this fragment.</summary>
    TailFourthShaftLink,
    /// <summary>The tail's inner joint, its last shaft link before the tip, which becomes this fragment.</summary>
    TailInnerJoint,
    /// <summary>The tail's tip, which becomes this fragment, its picture the direction the tip points.</summary>
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
