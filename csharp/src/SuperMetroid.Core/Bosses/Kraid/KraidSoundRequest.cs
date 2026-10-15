namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// A sound request emitted by Kraid's private bank-$A7 logic. The library number matters:
/// the roar uses library two, while spat rocks use library three despite sharing the same
/// room-enemy scheduler.
/// </summary>
/// <param name="SoundEffect">The native sound identifier together with its library, retained for the outer audio queue.</param>
public readonly record struct KraidSoundRequest(
    SoundEffectId SoundEffect);
