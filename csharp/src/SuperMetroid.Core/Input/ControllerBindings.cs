using System.Collections.Immutable;

namespace SuperMetroid.Core.Input;

/// <summary>
/// The seven configurable action bindings, kept in every save slot: the button bound to each action, keyed by the action
/// (#2565). Directional input and Start are fixed controls rather than actions, so they deliberately do not appear in this
/// type.
/// </summary>
/// <remarks>
/// Gameplay tests <see cref="GameplayAction"/>s. <see cref="ToActions"/> is the single equivalent of the game loading
/// these configurable bindings before each comparison: it reads the buttons as actions once per accepted update, so no
/// gameplay behaviour owns a binding table or names a button. A binding set binds every action from its construction, so no
/// value of it binds nothing.
/// </remarks>
public sealed partial class ControllerBindings : IEquatable<ControllerBindings>
{
    /// <summary>Every configurable action, in the order the bindings are checked and listed (#2567).</summary>
    private static readonly ControllerAction[] Actions = [.. Ensure.EveryMemberOnce<ControllerAction>(
    [
        ControllerAction.Shoot,
        ControllerAction.Jump,
        ControllerAction.Dash,
        ControllerAction.ItemSelect,
        ControllerAction.ItemCancel,
        ControllerAction.AimUp,
        ControllerAction.AimDown,
    ])];

    /// <summary>The button bound to each action, keyed by the action.</summary>
    private readonly ImmutableDictionary<ControllerAction, ControllerButton> buttons;

    /// <summary>Binds each action to the button <paramref name="buttons"/> holds for it.</summary>
    /// <param name="buttons">The button bound to each of the seven actions, keyed by the action.</param>
    /// <exception cref="ArgumentException"><paramref name="buttons"/> does not bind exactly each of the seven actions.</exception>
    public ControllerBindings(IReadOnlyDictionary<ControllerAction, ControllerButton> buttons)
    {
        Ensure.NotNull(buttons);
        if (buttons.Count != Actions.Length || Actions.Any(action => !buttons.ContainsKey(action)))
            throw new ArgumentException("Controller bindings bind each of the seven actions exactly once.", nameof(buttons));
        this.buttons = buttons.ToImmutableDictionary();
    }

    /// <summary>The literal bindings a new save file installs.</summary>
    /// <remarks>
    /// The new-save bindings are Shoot=X, Jump=A, Dash=B, ItemSelect=Select, ItemCancel=Y, AimUp=R, and AimDown=L, read
    /// through the <see cref="ControllerAction"/> indexer.
    /// </remarks>
    public static ControllerBindings Default => new(new Dictionary<ControllerAction, ControllerButton>
    {
        [ControllerAction.Shoot] = ControllerButton.X,
        [ControllerAction.Jump] = ControllerButton.A,
        [ControllerAction.Dash] = ControllerButton.B,
        [ControllerAction.ItemSelect] = ControllerButton.Select,
        [ControllerAction.ItemCancel] = ControllerButton.Y,
        [ControllerAction.AimUp] = ControllerButton.R,
        [ControllerAction.AimDown] = ControllerButton.L,
    });

    /// <summary>
    /// The physical buttons the options menu admits when it sets a binding, in the order of the options menu's button
    /// list. Only these seven can be assigned in an ordinary options menu.
    /// </summary>
    /// <remarks>
    /// <see cref="AssignAndSwap"/> admits only these buttons. When the player presses several at once to set a binding,
    /// the options menu takes the one latest in this list.
    /// </remarks>
    public static ImmutableArray<ControllerButton> AssignableButtons { get; } =
    [
        ControllerButton.X,
        ControllerButton.A,
        ControllerButton.B,
        ControllerButton.Select,
        ControllerButton.Y,
        ControllerButton.L,
        ControllerButton.R,
    ];

    /// <summary>
    /// Whether <paramref name="button"/> is one of the <see cref="AssignableButtons"/>; false for every other button set,
    /// including combinations of valid buttons, directions, Start, and none.
    /// </summary>
    private static bool IsAssignable(ControllerButton button) => AssignableButtons.Contains(button);

    /// <summary>Returns the button bound to one action.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="action"/> is not a defined action.</exception>
    public ControllerButton this[ControllerAction action] => buttons.TryGetValue(action, out ControllerButton button)
        ? button
        : throw new ArgumentOutOfRangeException(nameof(action), action, "Undefined controller action.");

    /// <summary>
    /// Reproduces the options screen's permutation swap. Assigning a physical button to one
    /// action moves that action's old button to whichever other action previously owned the
    /// requested button; the seven bindings therefore remain unique. Assigning an action the
    /// button it already has changes nothing.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="action"/> is not a defined action, or <paramref name="physicalButton"/> is not assignable.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// No action holds <paramref name="physicalButton"/>: these bindings are not a unique permutation of the assignable
    /// buttons, which every save, the options menu and the swap itself keep them.
    /// </exception>
    public ControllerBindings AssignAndSwap(ControllerAction action, ControllerButton physicalButton)
    {
        // Bindings with one action rebound and the other six kept.
        static ControllerBindings Rebound(ControllerBindings bindings, ControllerAction rebound, ControllerButton value) =>
            new(bindings.buttons.SetItem(rebound, value));
        ControllerButton displaced = this[action];
        if (!IsAssignable(physicalButton))
        {
            throw new ArgumentOutOfRangeException(
                nameof(physicalButton), physicalButton, "Button is not assignable.");
        }

        if (displaced == physicalButton)
            return this;
        ControllerBindings result = Rebound(this, action, physicalButton);
        foreach (ControllerAction other in Actions)
        {
            if (other != action && this[other] == physicalButton)
                return Rebound(result, other, displaced);
        }

        throw new InvalidOperationException(
            $"No action holds {physicalButton}, so the bindings are not a unique permutation of the assignable buttons.");
    }

    /// <summary>
    /// Reads the buttons of one controller sample as the gameplay actions they ask for: the directions and Start are
    /// fixed controls, and each configurable action is held while any button of its binding is held.
    /// </summary>
    public GameplayAction ToActions(ControllerButton held)
    {
        GameplayAction actions =
            FixedControl(held, ControllerButton.Up, GameplayAction.Up) |
            FixedControl(held, ControllerButton.Down, GameplayAction.Down) |
            FixedControl(held, ControllerButton.Left, GameplayAction.Left) |
            FixedControl(held, ControllerButton.Right, GameplayAction.Right) |
            FixedControl(held, ControllerButton.Start, GameplayAction.Start);
        foreach (ControllerAction action in Actions)
        {
            if (held.HasAny(this[action]))
                actions |= GameplayActions.Of(action);
        }
        return actions;
    }

    /// <summary>True when all seven bindings are single, distinct assignable buttons.</summary>
    internal bool IsUniquePermutation
    {
        get
        {
            Span<ControllerButton> seen = stackalloc ControllerButton[Actions.Length];
            for (int index = 0; index < Actions.Length; index++)
            {
                ControllerButton button = this[Actions[index]];
                if (!IsAssignable(button) || IsAmong(seen[..index], button))
                    return false;
                seen[index] = button;
            }
            return true;
        }
    }

    /// <summary>
    /// Whether <paramref name="button"/> is one of <paramref name="buttons"/>. Compared one by one: the span's own
    /// search asks more of its element than an enumeration gives on every .NET this is built for.
    /// </summary>
    private static bool IsAmong(ReadOnlySpan<ControllerButton> buttons, ControllerButton button)
    {
        foreach (ControllerButton one in buttons)
        {
            if (one == button)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns this binding set only when it is a complete permutation.
    /// </summary>
    /// <remarks>
    /// A save that reads back whole can still hold malformed button bindings. A malformed permutation is never replaced
    /// with the defaults: the save's loader gets the exact seven bindings in the exception and decides whether to delete or
    /// repair the save.
    /// </remarks>
    public ControllerBindings RequireUniquePermutation()
    {
        if (!IsUniquePermutation)
        {
            throw new InvalidDataException(
                $"Controller bindings are not a unique permutation of the seven assignable buttons: {this}.");
        }

        return this;
    }


    /// <summary>Whether both bind every action to the same button.</summary>
    public bool Equals(ControllerBindings? other) =>
        other is not null && Actions.All(action => buttons[action] == other.buttons[action]);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ControllerBindings other && Equals(other);

    /// <summary>Hashes the button each action is bound to.</summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (ControllerAction action in Actions)
            hash.Add(buttons[action]);
        return hash.ToHashCode();
    }

    /// <summary>Names each action's button, for diagnostics.</summary>
    public override string ToString() => string.Join(", ", Actions.Select(action => $"{action}={buttons[action]}"));

    /// <summary>The fixed control <paramref name="action"/> when <paramref name="button"/> is held, otherwise none.</summary>
    private static GameplayAction FixedControl(ControllerButton held, ControllerButton button, GameplayAction action) =>
        held.HasAny(button) ? action : GameplayAction.None;
}
