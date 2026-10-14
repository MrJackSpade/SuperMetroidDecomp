namespace SuperMetroid.Core.Game;

/// <summary>A gameplay cue of Mother Brain's brain programs.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Sound">The sound a sound cue queues.</param>
/// <param name="Afterburns">The afterburn count a bomb cue lobs.</param>
internal readonly record struct MotherBrainHeadCue(MotherBrainHeadCueKind Kind, SoundEffectId? Sound, int Afterburns)
{
    /// <summary>A cue without operands.</summary>
    internal static MotherBrainHeadCue Of(MotherBrainHeadCueKind kind) => new(kind, null, 0);
    /// <summary>Queues library-two <paramref name="sound"/>.</summary>
    internal static MotherBrainHeadCue Sound2(SoundEffectId sound) => new(MotherBrainHeadCueKind.QueueSound, sound, 0);
    /// <summary>Queues library-three <paramref name="sound"/>.</summary>
    internal static MotherBrainHeadCue Sound3(SoundEffectId sound) => new(MotherBrainHeadCueKind.QueueLibrary3Sound, sound, 0);
    /// <summary>Lobs a bomb that afterburns <paramref name="afterburns"/> times.</summary>
    internal static MotherBrainHeadCue Bomb(int afterburns) => new(MotherBrainHeadCueKind.SpawnBomb, null, afterburns);
}
