using System.Collections.Immutable;

namespace SuperMetroid.Core.Input;

/// <summary>
/// The seven configurable action bindings, kept in
/// every save slot. Directional input and Start are fixed controls rather than actions,
/// so they deliberately do not appear in this type.
/// </summary>
/// <remarks>
/// Most movement behaviours already compare their input against the default
/// action bits. <see cref="Normalize"/> is the single equivalent of the game loading
/// those configurable bindings before each comparison: it converts physical buttons into the
/// canonical action bits without making every movement behaviour own a second binding table.
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
    /// Independently reviewed for #1165: the new-save bindings are
    /// Shoot=X, Jump=A, Dash=B,
    /// ItemSelect=Select, ItemCancel=Y, AimUp=R, and
    /// AimDown=L. These are named action-to-button cases; the constructor and the
    /// <see cref="ControllerAction"/> indexer already express that mapping without a stored lookup table.
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
    /// list. Only these seven can be assigned in an ordinary retail options menu.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165: the options menu's button list holds nine
    /// buttons: X, A, B, Select, Y, L, R, Left, Right. The managed seven-button prefix
    /// matches exactly; the final two follow the list, not assignable actions. Setting a
    /// binding starts at the seventh choice and scans backward to the first; saving the
    /// controller settings uses the same seven choices. The managed menu scans this list backward and
    /// AssignAndSwap validates membership through it.
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
    /// The position of <paramref name="button"/> in <see cref="AssignableButtons"/>; null for every other button set,
    /// including combinations of valid buttons, directions, Start, and none.
    /// </summary>
    public static int? AssignableButtonIndex(ControllerButton button)
    {
        int index = AssignableButtons.IndexOf(button);
        return index < 0 ? null : index;
    }

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
    /// requested button; the seven bindings therefore remain unique.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="action"/> is not a defined action, or <paramref name="physicalButton"/> is not assignable.
    /// </exception>
    public ControllerBindings AssignAndSwap(ControllerAction action, ControllerButton physicalButton)
    {
        ControllerButton displaced = this[action];
        if (AssignableButtonIndex(physicalButton) is null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(physicalButton), physicalButton, "Button is not retail-assignable.");
        }

        ControllerBindings result = WithAction(action, physicalButton);
        foreach (ControllerAction other in Actions)
        {
            if (other != action && this[other] == physicalButton)
                return result.WithAction(other, displaced);
        }

        // A corrupt saved permutation can omit the selected physical button. The game scans
        // six peers and then leaves the displaced value nowhere; matching that behavior is
        // safer than inventing a repair policy at runtime.
        return result;
    }

    /// <summary>
    /// Converts one physical controller sample into the default action layout expected by the
    /// gameplay while preserving non-configurable directions and Start.
    /// </summary>
    public ControllerButton Normalize(ControllerButton physicalInput)
    {
        const ControllerButton fixedControls =
            ControllerButton.Up | ControllerButton.Down | ControllerButton.Left | ControllerButton.Right |
            ControllerButton.Start;
        ControllerButton normalized = physicalInput & fixedControls;
        normalized = Map(physicalInput, Shoot, ControllerButton.X, normalized);
        normalized = Map(physicalInput, Jump, ControllerButton.A, normalized);
        normalized = Map(physicalInput, Dash, ControllerButton.B, normalized);
        normalized = Map(physicalInput, ItemSelect, ControllerButton.Select, normalized);
        normalized = Map(physicalInput, ItemCancel, ControllerButton.Y, normalized);
        normalized = Map(physicalInput, AimUp, ControllerButton.R, normalized);
        normalized = Map(physicalInput, AimDown, ControllerButton.L, normalized);
        return normalized;
    }

    /// <summary>True when all seven bindings are single, distinct retail-assignable buttons.</summary>
    public bool IsRetailPermutation
    {
        get
        {
            Span<ControllerButton> seen = stackalloc ControllerButton[Actions.Length];
            for (int index = 0; index < Actions.Length; index++)
            {
                ControllerButton button = this[Actions[index]];
                if (AssignableButtonIndex(button) is null || seen[..index].Contains(button))
                    return false;
                seen[index] = button;
            }
            return true;
        }
    }

    /// <summary>
    /// Returns this binding set only when it is a complete retail permutation.
    /// </summary>
    /// <remarks>
    /// A valid save checksum proves only that the slot was written consistently; it does
    /// not make malformed button bindings safe. Replacing a corrupt permutation with defaults
    /// used to hide both bad save data and translation bugs. The caller now gets the exact
    /// seven bindings in the exception and can decide whether to delete or repair the save.
    /// </remarks>
    public ControllerBindings RequireRetailPermutation()
    {
        if (!IsRetailPermutation)
        {
            throw new InvalidDataException(
                "Controller bindings are not a unique permutation of the seven retail " +
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

    /// <summary>Sets a canonical action bit when its configured physical button is pressed in the sampled input.</summary>
    private static ControllerButton Map(
        ControllerButton physicalInput,
        ControllerButton physicalBinding,
        ControllerButton canonicalAction,
        ControllerButton normalized) =>
        (physicalInput & physicalBinding) != ControllerButton.None
            ? normalized | canonicalAction
            : normalized;
}
