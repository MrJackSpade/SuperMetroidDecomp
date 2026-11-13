namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship liftoff dust clouds' typed programs (#641): three cloud shapes on each side of the ship.</summary>
internal enum GunshipDustSegment
{
    /// <summary>Loads the cloud's single pass.</summary>
    RightInner,
    /// <summary>Repeats the RightInner cloud's poses while its general timer counts, then deletes it.</summary>
    RightInnerLoop,
    /// <summary>Loads the cloud's single pass.</summary>
    RightMiddle,
    /// <summary>Repeats the RightMiddle cloud's poses while its general timer counts, then deletes it.</summary>
    RightMiddleLoop,
    /// <summary>Loads the cloud's single pass.</summary>
    RightOuter,
    /// <summary>Repeats the RightOuter cloud's poses while its general timer counts, then deletes it.</summary>
    RightOuterLoop,
    /// <summary>Loads the cloud's single pass.</summary>
    LeftInner,
    /// <summary>Repeats the LeftInner cloud's poses while its general timer counts, then deletes it.</summary>
    LeftInnerLoop,
    /// <summary>Loads the cloud's single pass.</summary>
    LeftMiddle,
    /// <summary>Repeats the LeftMiddle cloud's poses while its general timer counts, then deletes it.</summary>
    LeftMiddleLoop,
    /// <summary>Loads the cloud's single pass.</summary>
    LeftOuter,
    /// <summary>Repeats the LeftOuter cloud's poses while its general timer counts, then deletes it.</summary>
    LeftOuterLoop,
}
