namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The sixteen ordered rectangle lists Kraid's arm components collide with.</summary>
internal enum KraidArmHitboxList
{
    /// <summary>The first of the seven shapes the outer articulated components advance through.</summary>
    ArticulatedFirst,
    /// <summary>The second articulated shape.</summary>
    ArticulatedSecond,
    /// <summary>The third articulated shape.</summary>
    ArticulatedThird,
    /// <summary>The fourth articulated shape.</summary>
    ArticulatedFourth,
    /// <summary>The fifth articulated shape.</summary>
    ArticulatedFifth,
    /// <summary>The sixth articulated shape.</summary>
    ArticulatedSixth,
    /// <summary>The seventh and last articulated shape.</summary>
    ArticulatedSeventh,
    /// <summary>The connecting components' second shape.</summary>
    ConnectorSecond,
    /// <summary>The connecting components' third shape.</summary>
    ConnectorThird,
    /// <summary>The connecting components' final shape.</summary>
    ConnectorFinal,
    /// <summary>The connecting components' initial shape.</summary>
    ConnectorInitial,
    /// <summary>The body component's final shape.</summary>
    BodyFinal,
    /// <summary>The body component's middle shape.</summary>
    BodyMiddle,
    /// <summary>The body component's initial shape.</summary>
    BodyInitial,
    /// <summary>The first dying/preparing-to-lunge list.</summary>
    FinalPoseFirst,
    /// <summary>The second dying/preparing-to-lunge list.</summary>
    FinalPoseSecond,
}
