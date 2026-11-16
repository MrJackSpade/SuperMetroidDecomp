namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One cue of Mini-Kraid's animations: what it does, with the values it acts on.</summary>
internal abstract record FakeKraidCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private FakeKraidCue()
    {
    }

    /// <summary>Spits a pair of projectiles the cue's way (#2163).</summary>
    /// <param name="Direction">The way the spit flies.</param>
    internal sealed record Spit(HorizontalDirection Direction) : FakeKraidCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(FakeKraidCueKind Kind) : FakeKraidCue;
}
