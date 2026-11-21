namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Where Kraid's growth restarts the head: the part of the roar it resumes and the pose it enters there. A closed,
/// opening or wider pose shown next resumes the roar's closing part at that pose; anything else resumes it at the
/// wide-open hold's end with a long delay.
/// </summary>
/// <param name="Segment">The roar's part growth resumes.</param>
/// <param name="Frame">The pose of that part the head enters.</param>
internal readonly record struct KraidHeadGrowthResume(KraidHeadSegment Segment, KraidHeadFrame Frame);
