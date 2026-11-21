namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>The installed escape warnings (#2422): each message's lines as the installation holds them, wording and placement.</summary>
public sealed partial class EscapeTypewriterPresentation
{
    /// <summary>Loaded Ceres warning lines and where each starts.</summary>
    private readonly EscapeTypewriterMessage ceres;
    /// <summary>Loaded Zebes time-bomb warning lines and where each starts.</summary>
    private readonly EscapeTypewriterMessage zebes;

    /// <summary>Retains the two messages the import pipeline read and validated, their lines as installed.</summary>
    internal EscapeTypewriterPresentation(Dictionary<EscapeTypewriterMessageId, EscapeTypewriterMessage> messages)
    {
        ceres = messages[EscapeTypewriterMessageId.Ceres];
        zebes = messages[EscapeTypewriterMessageId.Zebes];
    }

    /// <summary>Returns the installed warning text and placement for one escape scenario.</summary>
    /// <param name="id">Ceres or Zebes message identity.</param>
    /// <returns>The retained message with its installed lines, in the order they are typed.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is no message.</exception>
    public EscapeTypewriterMessage Get(EscapeTypewriterMessageId id) => id switch
    {
        EscapeTypewriterMessageId.Ceres => ceres,
        EscapeTypewriterMessageId.Zebes => zebes,
        _ => throw new ArgumentOutOfRangeException(nameof(id), id,
            "No escape warning of this kind is installed."),
    };
}
