using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// What one update of the gameplay message box did (#2514): it asked nothing, showed the save choice the player moved,
/// started the gunship's saving sound, or closed, a save confirmation's box handing its answer to what waited on it. An
/// update of the box does at most one of these.
/// </summary>
public abstract partial record MessageBoxStep
{
    /// <summary>Only the steps below exist.</summary>
    private MessageBoxStep()
    {
    }

    /// <summary>
    /// The update asked nothing of the box's owner: the box stays shown, or paused before it opens or after it closes, or
    /// no box is open.
    /// </summary>
    public sealed partial record NothingAsked : MessageBoxStep
    {
        /// <summary>The one update that asks nothing; every such update is the same.</summary>
        internal static NothingAsked Instance { get; } = new();

        /// <summary>Only <see cref="Instance"/> exists.</summary>
        private NothingAsked()
        {
        }
    }

    /// <summary>The update showed the save choice the player moved, on which the menu cursor's sound plays.</summary>
    public sealed partial record ChoiceMoved : MessageBoxStep
    {
        /// <summary>The one update that shows a moved choice; every such update is the same.</summary>
        internal static ChoiceMoved Instance { get; } = new();

        /// <summary>Only <see cref="Instance"/> exists.</summary>
        private ChoiceMoved()
        {
        }
    }

    /// <summary>
    /// The gunship's accepted save closed its prompt's window on this update and starts the saving sound, whose wait the box
    /// holds before its completion notice opens.
    /// </summary>
    public sealed partial record SavingSoundStarted : MessageBoxStep
    {
        /// <summary>The one update that starts the saving sound; every such update is the same.</summary>
        internal static SavingSoundStarted Instance { get; } = new();

        /// <summary>Only <see cref="Instance"/> exists.</summary>
        private SavingSoundStarted()
        {
        }
    }

    /// <summary>The box closed on this update, returning to the gameplay update it paused; a message offering no choice closed.</summary>
    public partial record Closed : MessageBoxStep
    {
        /// <summary>The one close of a message offering no choice; every such close is the same.</summary>
        internal static Closed WithoutAnswer { get; } = new();

        /// <summary>Only <see cref="WithoutAnswer"/> and the answered close exist.</summary>
        private protected Closed()
        {
        }
    }

    /// <summary>
    /// A save confirmation's box closed on this update with the player's <paramref name="Choice"/>; the gunship's accepted
    /// save answers YES once its completion notice closes.
    /// </summary>
    /// <param name="Choice">The answer the player chose.</param>
    public sealed partial record Answered(SaveChoice Choice) : Closed;
}
