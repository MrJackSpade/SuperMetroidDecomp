namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A gameplay cue of Mother Brain's brain programs.</summary>
internal abstract record MotherBrainHeadCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainHeadCue()
    {
    }

    /// <summary>Requests a sound: a roar, an attack cry or the onion rings.</summary>
    /// <param name="Effect">The sound requested.</param>
    internal sealed record PlaySound(SoundEffectId Effect) : MotherBrainHeadCue;

    /// <summary>Lobs a bomb with its afterburn count.</summary>
    /// <param name="Afterburns">The afterburn count the bomb lobs with.</param>
    internal sealed record SpawnBomb(int Afterburns) : MotherBrainHeadCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(MotherBrainHeadCueKind Kind) : MotherBrainHeadCue;

    /// <summary>A cue without operands.</summary>
    internal static MotherBrainHeadCue Of(MotherBrainHeadCueKind kind) => new Command(kind);

    /// <summary>Requests <paramref name="sound"/>.</summary>
    internal static MotherBrainHeadCue Sound(SoundEffectId sound) => new PlaySound(sound);

    /// <summary>Lobs a bomb with <paramref name="afterburns"/> afterburns.</summary>
    internal static MotherBrainHeadCue Bomb(int afterburns) => new SpawnBomb(afterburns);
}
