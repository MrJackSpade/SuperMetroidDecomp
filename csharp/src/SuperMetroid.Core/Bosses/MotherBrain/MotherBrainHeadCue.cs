namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A gameplay cue of Mother Brain's brain programs.</summary>
internal abstract record MotherBrainHeadCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainHeadCue()
    {
    }

    /// <summary>Queues a library-two sound.</summary>
    /// <param name="Sound">The sound queued.</param>
    internal sealed record QueueSound(SoundEffectId Sound) : MotherBrainHeadCue;

    /// <summary>Queues a library-three sound.</summary>
    /// <param name="Sound">The sound queued.</param>
    internal sealed record QueueLibrary3Sound(SoundEffectId Sound) : MotherBrainHeadCue;

    /// <summary>Lobs a bomb with its afterburn count.</summary>
    /// <param name="Afterburns">The afterburn count the bomb lobs with.</param>
    internal sealed record SpawnBomb(int Afterburns) : MotherBrainHeadCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(MotherBrainHeadCueKind Kind) : MotherBrainHeadCue;

    /// <summary>A cue without operands.</summary>
    internal static MotherBrainHeadCue Of(MotherBrainHeadCueKind kind) => new Command(kind);

    /// <summary>Queues library-two <paramref name="sound"/>.</summary>
    internal static MotherBrainHeadCue Sound2(SoundEffectId sound) => new QueueSound(sound);

    /// <summary>Queues library-three <paramref name="sound"/>.</summary>
    internal static MotherBrainHeadCue Sound3(SoundEffectId sound) => new QueueLibrary3Sound(sound);

    /// <summary>Lobs a bomb with <paramref name="afterburns"/> afterburns.</summary>
    internal static MotherBrainHeadCue Bomb(int afterburns) => new SpawnBomb(afterburns);
}
