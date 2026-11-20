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

    /// <summary>The button each action is bound to.</summary>
    private static IdentityTable<ControllerAction, Func<ControllerBindings, ControllerButton>> Buttons => field ??= new(new Dictionary<ControllerAction, Func<ControllerBindings, ControllerButton>>
    {
        [ControllerAction.Shoot] = static bindings => bindings.Shoot,
        [ControllerAction.Jump] = static bindings => bindings.Jump,
        [ControllerAction.Dash] = static bindings => bindings.Dash,
        [ControllerAction.ItemSelect] = static bindings => bindings.ItemSelect,
        [ControllerAction.ItemCancel] = static bindings => bindings.ItemCancel,
        [ControllerAction.AimUp] = static bindings => bindings.AimUp,
        [ControllerAction.AimDown] = static bindings => bindings.AimDown,
    });

    /// <summary>Returns the button bound to one action.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="action"/> is not a defined action.</exception>
    public ControllerButton this[ControllerAction action] => Buttons[action](this);

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

    /// <summary>The bindings with each action bound to another button.</summary>
    private static IdentityTable<ControllerAction, Func<ControllerBindings, ControllerButton, ControllerBindings>> Rebindings => field ??= new(new Dictionary<ControllerAction, Func<ControllerBindings, ControllerButton, ControllerBindings>>
    {
        [ControllerAction.Shoot] = static (bindings, value) => bindings with { Shoot = value },
        [ControllerAction.Jump] = static (bindings, value) => bindings with { Jump = value },
        [ControllerAction.Dash] = static (bindings, value) => bindings with { Dash = value },
        [ControllerAction.ItemSelect] = static (bindings, value) => bindings with { ItemSelect = value },
        [ControllerAction.ItemCancel] = static (bindings, value) => bindings with { ItemCancel = value },
        [ControllerAction.AimUp] = static (bindings, value) => bindings with { AimUp = value },
        [ControllerAction.AimDown] = static (bindings, value) => bindings with { AimDown = value },
    });

    /// <summary>Replaces one action's button while retaining the other six bindings.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="action"/> is not a defined action.</exception>
    private ControllerBindings WithAction(ControllerAction action, ControllerButton value) => Rebindings[action](this, value);

    /// <summary>The fixed control <paramref name="action"/> when <paramref name="button"/> is held, otherwise none.</summary>
    private static GameplayAction FixedControl(ControllerButton held, ControllerButton button, GameplayAction action) =>
        held.HasAny(button) ? action : GameplayAction.None;
}
