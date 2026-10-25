namespace SuperMetroid.Core.Input;

/// <summary>
/// The seven configurable action words stored at WRAM <c>$09B2-$09BE</c> and mirrored in
/// every SRAM slot. Directional input and Start are hardware controls rather than actions,
/// so they deliberately do not appear in this type.
/// </summary>
/// <remarks>
/// Most translated bank-$90/$91 routines already compare their input against the default
/// action bits. <see cref="Normalize"/> is the single equivalent of the cartridge loading
/// those configurable words before each comparison: it converts physical buttons into the
/// canonical action bits without making every movement routine own a second binding table.
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
    /// <summary>The literal bindings installed by <c>NewSaveFile</c> at <c>$81:B2CB</c>.</summary>
    /// <remarks>
    /// Independently reviewed for #1165: NTSC J/U v1.0 ROM and bank_81.asm agree on all
    /// seven immediate/store pairs: Shoot=X/$09B2, Jump=A/$09B4, Dash=B/$09B6,
    /// ItemSelect=Select/$09BA, ItemCancel=Y/$09B8, AimUp=R/$09BE, and
    /// AimDown=L/$09BC. The constructor and action indexer use menu row order
    /// 0..6, which differs from WRAM address order for the final four fields.
    /// These are named action-to-button cases. The existing constructor and bounded
    /// action indexer already express that mapping without a stored lookup table.
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
    /// Number of physical buttons admitted by <c>GameOptions_ControllerSettings_SetBinding</c>.
    /// </summary>
    public const int AssignableButtonCount = 7;

    /// <summary>
    /// Physical button admitted by <c>GameOptions_ControllerSettings_SetBinding</c>, in the same order
    /// as ROM table <c>$82:F575</c>. Its final Left/Right entries are not examined by that
    /// routine; only these first seven can be assigned in an ordinary retail options menu.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165: NTSC J/U v1.0 ROM and Controller_Input_Bitmasks in
    /// bank_82.asm agree on all nine little-endian words: X, A, B, Select, Y, L, R,
    /// Left, Right. The managed seven-word prefix matches exactly; the final two words
    /// remain adjacent ROM data, not assignable actions. Native set-binding starts at
    /// byte offset 12 and scans backward to zero; Save_GameOptionsMenu_ControllerBindings
    /// indexes the same seven choices. The managed menu scans these cases backward and
    /// AssignAndSwap validates membership through the inverse mapping. Each case selects
    /// a named hardware button, so no numerical curve or stored span is needed.
    /// Only indices 0..6 are supported; throw ArgumentOutOfRangeException.
    /// </remarks>
    public static ControllerButton AssignableButton(int index) => index switch
    {
        0 => ControllerButton.X,
        1 => ControllerButton.A,
        2 => ControllerButton.B,
        3 => ControllerButton.Select,
        4 => ControllerButton.Y,
        5 => ControllerButton.L,
        6 => ControllerButton.R,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>
    /// Inverse of <see cref="AssignableButton"/>; null for every other button set,
    /// including combinations of valid buttons, directions, Start, and zero.
    /// </summary>
    public static int? AssignableButtonIndex(ControllerButton button) => button switch
    {
        ControllerButton.X => 0,
        ControllerButton.A => 1,
        ControllerButton.B => 2,
        ControllerButton.Select => 3,
        ControllerButton.Y => 4,
        ControllerButton.L => 5,
        ControllerButton.R => 6,
        ControllerButton.None or
            ControllerButton.Right or
            ControllerButton.Left or
            ControllerButton.Down or
            ControllerButton.Up or
            ControllerButton.Start => null,
        _ => null,
    };

    /// <summary>Returns one action's button using the controller-menu row order.</summary>
    public ControllerButton this[int action]
    {
        get => action switch
        {
            0 => Shoot,
            1 => Jump,
            2 => Dash,
            3 => ItemSelect,
            4 => ItemCancel,
            5 => AimUp,
            6 => AimDown,
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };
    }

    /// <summary>
    /// Reproduces the options screen's permutation swap. Assigning a physical button to one
    /// action moves that action's old button to whichever other action previously owned the
    /// requested button; the seven bindings therefore remain unique.
    /// </summary>
    public ControllerBindings AssignAndSwap(int action, ControllerButton physicalButton)
    {
        if ((uint)action >= 7)
            throw new ArgumentOutOfRangeException(nameof(action));
        if (AssignableButtonIndex(physicalButton) is null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(physicalButton), physicalButton, "Button is not retail-assignable.");
        }

        ControllerButton displaced = this[action];
        ControllerBindings result = WithAction(action, physicalButton);
        for (int other = 0; other < 7; other++)
        {
            if (other != action && this[other] == physicalButton)
                return result.WithAction(other, displaced);
        }

        // A corrupt SRAM permutation can omit the selected physical button. Native scans
        // six peers and then leaves the displaced value nowhere; matching that behavior is
        // safer than inventing a repair policy at runtime.
        return result;
    }

    /// <summary>
    /// Converts one physical SNES sample into the default action layout expected by the
    /// translated gameplay routines while preserving non-configurable directions and Start.
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

    /// <summary>True when all seven words are single, distinct retail-assignable buttons.</summary>
    public bool IsRetailPermutation
    {
        get
        {
            Span<ControllerButton> seen = stackalloc ControllerButton[7];
            for (int action = 0; action < 7; action++)
            {
                ControllerButton button = this[action];
                if (AssignableButtonIndex(button) is null || seen[..action].Contains(button))
                    return false;
                seen[action] = button;
            }
            return true;
        }
    }

    /// <summary>
    /// Returns this binding set only when it is a complete retail permutation.
    /// </summary>
    /// <remarks>
    /// A valid SRAM checksum proves only that the payload was written consistently; it does
    /// not make malformed button words safe. Replacing a corrupt permutation with defaults
    /// used to hide both bad save data and translation bugs. The caller now gets the exact
    /// seven words in the exception and can decide whether to delete or repair the save.
    /// </remarks>
    public ControllerBindings RequireRetailPermutation()
    {
        if (!IsRetailPermutation)
        {
            throw new InvalidDataException(
                "Controller bindings are not a unique permutation of the seven retail " +
                $"buttons: shoot=${Shoot:X}, jump=${Jump:X}, dash=${Dash:X}, " +
                $"select=${ItemSelect:X}, cancel=${ItemCancel:X}, " +
                $"aim-up=${AimUp:X}, aim-down=${AimDown:X}.");
        }

        return this;
    }

    /// <summary>Replaces one menu-row action word while retaining the other six bindings.</summary>
    private ControllerBindings WithAction(int action, ControllerButton value) => action switch
    {
        0 => this with { Shoot = value },
        1 => this with { Jump = value },
        2 => this with { Dash = value },
        3 => this with { ItemSelect = value },
        4 => this with { ItemCancel = value },
        5 => this with { AimUp = value },
        6 => this with { AimDown = value },
        _ => throw new ArgumentOutOfRangeException(nameof(action)),
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
