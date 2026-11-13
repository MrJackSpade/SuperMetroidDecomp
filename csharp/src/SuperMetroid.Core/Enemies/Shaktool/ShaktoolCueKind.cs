namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>What a cue of the Shaktool's animations does: one per kind of step its lists use.</summary>
internal enum ShaktoolCueKind
{
    /// <summary>Lowers the segment one pixel away from its target angle.</summary>
    Lower,
    /// <summary>Lowers the segment one pixel away from the head's facing.</summary>
    LowerAwayFromCenter,
    /// <summary>Does nothing: the empty step between the two long attack pauses.</summary>
    Nothing,
    /// <summary>Raises the segment one pixel toward its target angle.</summary>
    Raise,
    /// <summary>Raises the segment one pixel toward the head's facing.</summary>
    RaiseTowardCenter,
    /// <summary>Restores every segment's authored frame behaviour.</summary>
    ResetFunctions,
}
