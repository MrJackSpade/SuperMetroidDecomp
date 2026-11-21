namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One cue of Fake Kraid's animations: what it does, with the values it acts on.</summary>
internal abstract partial record FakeKraidCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private FakeKraidCue()
    {
    }

    /// <summary>Spits a pair of projectiles the cue's way (#2163).</summary>
    /// <param name="Direction">The way the spit flies.</param>
    internal sealed partial record Spit(HorizontalDirection Direction) : FakeKraidCue;

    /// <summary>Continues at the step or spit animation his wait between spits and his facing select.</summary>
    internal sealed partial record ChooseAction : FakeKraidCue;

    /// <summary>Walks one step (<see cref="FakeKraidProjectileDefinitions.StepPixels"/>) unless the walk's last step turns him round, then faces Samus.</summary>
    internal sealed partial record Move : FakeKraidCue;

    /// <summary>Queues Fake Kraid's cry while he is on screen.</summary>
    internal sealed partial record PlayCrySound : FakeKraidCue;
}
