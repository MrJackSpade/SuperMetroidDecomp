using System.Collections.Immutable;

namespace SuperMetroid.Core.Input;

/// <summary>
/// The seven configurable action bindings, kept in
/// every save slot. Directional input and Start are fixed controls rather than actions,
/// so they deliberately do not appear in this type.
/// </summary>
/// <remarks>
/// Gameplay tests <see cref="GameplayAction"/>s. <see cref="ToActions"/> is the single equivalent of the game loading
/// these configurable bindings before each comparison: it reads the buttons as actions once per accepted update, so no
/// gameplay behaviour owns a binding table or names a button.
/// </remarks>
public readonly record struct ControllerBindings(
    ControllerButton Shoot,
    ControllerButton Jump,
    ControllerButton Dash,
    ControllerButton ItemSelect,
    ControllerButton ItemCancel,
    ControllerButton AimUp,
    ControllerButton AimDown)
{
    /// <summary>Every configurable action, in declaration order.</summary>
    private static readonly ControllerAction[] Actions = Enum.GetValues<ControllerAction>();

    /// <summary>The literal bindings a new save file installs.</summary>
    /// <remarks>
    /// The new-save bindings are
    /// Shoot=X, Jump=A, Dash=B,
    /// ItemSelect=Select, ItemCancel=Y, AimUp=R, and
    /// AimDown=L. These are named action-to-button cases, read through the constructor and the
    /// <see cref="ControllerAction"/> indexer.
    /// </remarks>
    public static ControllerBindings Default => new(
        Shoot: ControllerButton.X,
        Jump: ControllerButton.A,
        Dash: ControllerButton.B,
        ItemSelect: ControllerButton.Select,
        ItemCancel: ControllerButton.Y,
        AimUp: ControllerButton.R,
        AimDown: ControllerButton.L);

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
    public static bool IsAssignable(ControllerButton button) => AssignableButtons.Contains(button);

    /// <summary>Returns the button bound to one action.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="action"/> is not a defined action.</exception>
    public ControllerButton this[ControllerAction action] => action switch
    {
        ControllerAction.Shoot => Shoot,
        ControllerAction.Jump => Jump,
        ControllerAction.Dash => Dash,
        ControllerAction.ItemSelect => ItemSelect,
        ControllerAction.ItemCancel => ItemCancel,
        ControllerAction.AimUp => AimUp,
        ControllerAction.AimDown => AimDown,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Undefined controller action."),
    };

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
        ControllerButton displaced = this[action];
        if (!IsAssignable(physicalButton))
        {
            throw new ArgumentOutOfRangeException(
                nameof(physicalButton), physicalButton, "Button is not assignable.");
        }

        if (displaced == physicalButton)
            return this;
        ControllerBindings result = WithAction(action, physicalButton);
        foreach (ControllerAction other in Actions)
        {
            if (other != action && this[other] == physicalButton)
                return result.WithAction(other, displaced);
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
    private bool IsUniquePermutation
    {
        get
        {
            Span<ControllerButton> seen = stackalloc ControllerButton[Actions.Length];
            for (int index = 0; index < Actions.Length; index++)
            {
                ControllerButton button = this[Actions[index]];
                if (!IsAssignable(button) || seen[..index].Contains(button))
                    return false;
                seen[index] = button;
            }
            return true;
        }
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
                "Controller bindings are not a unique permutation of the seven assignable " +
                $"buttons: shoot={Shoot}, jump={Jump}, dash={Dash}, " +
                $"select={ItemSelect}, cancel={ItemCancel}, " +
                $"aim-up={AimUp}, aim-down={AimDown}.");
        }

        return this;
    }

    /// <summary>Replaces one action's button while retaining the other six bindings.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="action"/> is not a defined action.</exception>
    private ControllerBindings WithAction(ControllerAction action, ControllerButton value) => action switch
    {
        ControllerAction.Shoot => this with { Shoot = value },
        ControllerAction.Jump => this with { Jump = value },
        ControllerAction.Dash => this with { Dash = value },
        ControllerAction.ItemSelect => this with { ItemSelect = value },
        ControllerAction.ItemCancel => this with { ItemCancel = value },
        ControllerAction.AimUp => this with { AimUp = value },
        ControllerAction.AimDown => this with { AimDown = value },
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Undefined controller action."),
    };

    /// <summary>The fixed control <paramref name="action"/> when <paramref name="button"/> is held, otherwise none.</summary>
    private static GameplayAction FixedControl(ControllerButton held, ControllerButton button, GameplayAction action) =>
        held.HasAny(button) ? action : GameplayAction.None;
}
