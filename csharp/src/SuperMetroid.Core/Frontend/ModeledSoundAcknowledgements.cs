namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The port's normal-play policy: each update reads the session's own SPC port model.
/// </summary>
public sealed class ModeledSoundAcknowledgements : ISoundAcknowledgementSource
{
    /// <summary>Shared stateless policy for normal play.</summary>
    public static ModeledSoundAcknowledgements Instance { get; } = new();

    /// <summary>Prevents instances other than <see cref="Instance"/>.</summary>
    private ModeledSoundAcknowledgements() { }

    /// <summary>Returns the modeled acknowledgements unchanged.</summary>
    /// <param name="modeled">The bytes the session's SPC port model published after the previous update.</param>
    /// <returns><paramref name="modeled"/>.</returns>
    public CartridgeAudioAcknowledgements ForNextFrame(CartridgeAudioAcknowledgements modeled) => modeled;
}
