namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Where Kraid's growth restarts the head, chosen by what the head program would run next when growth begins: a
/// closed, opening or wider pose each resume the roar's closing part at the matching point; anything else resumes it
/// at the wide-open hold's end with a long delay.
/// </summary>
internal enum KraidHeadGrowthResume
{
    /// <summary>The next step shows the closed mouth.</summary>
    NextFrameClosed = 1,
    /// <summary>The next step shows the first opening stage.</summary>
    NextFrameOpening,
    /// <summary>The next step shows the wider opening stage.</summary>
    NextFrameWider,
    /// <summary>The next step is the open pose, a cry or a program end.</summary>
    NextStepOther,
}
