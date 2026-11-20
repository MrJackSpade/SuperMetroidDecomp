using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// One row of the controller page (#2206): a row that assigns a button to an action, then Exit and Reset;
/// <see cref="GameOptionsPageRows.Controller"/> places them from the top.
/// </summary>
public abstract record GameOptionsControllerRow
{
    /// <summary>Closes the hierarchy to the rows declared here.</summary>
    private GameOptionsControllerRow()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this game options controller row, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(GameOptionsControllerRowReactions<TContext> reactions, TContext context);


    /// <summary>Assigns the physical button pressed on it to <see cref="Action"/>, swapping the button it displaces.</summary>
    /// <param name="Action">The action the row assigns.</param>
    public sealed record AssignsAction(ControllerAction Action) : GameOptionsControllerRow
    {
        /// <inheritdoc/>
        internal override void React<TContext>(GameOptionsControllerRowReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).AssignsAction(this, context);
    }

    /// <summary>Returns to the primary page.</summary>
    public sealed record Exit : GameOptionsControllerRow
    {
        /// <inheritdoc/>
        internal override void React<TContext>(GameOptionsControllerRowReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Exit(this, context);

        /// <summary>The one Exit row.</summary>
        public static Exit Instance { get; } = new();
    }

    /// <summary>Restores the default button permutation.</summary>
    public sealed record Reset : GameOptionsControllerRow
    {
        /// <inheritdoc/>
        internal override void React<TContext>(GameOptionsControllerRowReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Reset(this, context);

        /// <summary>The one Reset row.</summary>
        public static Reset Instance { get; } = new();
    }
}

/// <summary>What the owner of a <see cref="GameOptionsControllerRow"/> does on each of its cases (#2553).</summary>
/// <param name="AssignsAction">Reacts to a <see cref="GameOptionsControllerRow.AssignsAction"/>.</param>
/// <param name="Exit">Reacts to a <see cref="GameOptionsControllerRow.Exit"/>.</param>
/// <param name="Reset">Reacts to a <see cref="GameOptionsControllerRow.Reset"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
public sealed record GameOptionsControllerRowReactions<TContext>(Action<GameOptionsControllerRow.AssignsAction, TContext> AssignsAction, Action<GameOptionsControllerRow.Exit, TContext> Exit, Action<GameOptionsControllerRow.Reset, TContext> Reset);
