namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>What a cue of the Shaktool's animations does: raise or lower a segment, or have every segment resume its own motion.</summary>
internal enum ShaktoolCueKind
{
    /// <summary>Lowers the segment one pixel away from its target angle.</summary>
    Lower,
    /// <summary>Raises the segment one pixel toward its target angle.</summary>
    Raise,
    /// <summary>Every segment resumes its own motion once the head bob ends.</summary>
    ResumeMotion,
}
