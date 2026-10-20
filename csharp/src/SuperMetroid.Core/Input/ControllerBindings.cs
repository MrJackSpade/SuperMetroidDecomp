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
    SnesButton Shoot,
    SnesButton Jump,
    SnesButton Dash,
    SnesButton ItemSelect,
    SnesButton ItemCancel,
    SnesButton AimUp,
    SnesButton AimDown)
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
        Shoot: SnesButton.X,
        Jump: SnesButton.A,
        Dash: SnesButton.B,
        ItemSelect: SnesButton.Select,
        ItemCancel: SnesButton.Y,
        AimUp: SnesButton.R,
        AimDown: SnesButton.L);

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
    public static SnesButton AssignableButton(int index) => index switch
    {
        0 => SnesButton.X,
        1 => SnesButton.A,
        2 => SnesButton.B,
        3 => SnesButton.Select,
        4 => SnesButton.Y,
        5 => SnesButton.L,
        6 => SnesButton.R,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>
    /// Inverse of <see cref="AssignableButton"/>; null for every other button set,
    /// including combinations of valid buttons, directions, Start, and zero.
    /// </summary>
    public static int? AssignableButtonIndex(SnesButton button) => button switch
    {
        SnesButton.X => 0,
        SnesButton.A => 1,
        SnesButton.B => 2,
        SnesButton.Select => 3,
        SnesButton.Y => 4,
        SnesButton.L => 5,
        SnesButton.R => 6,
        SnesButton.None or
            SnesButton.Right or
            SnesButton.Left or
            SnesButton.Down or
            SnesButton.Up or
            SnesButton.Start => null,
        _ => null,
    };

    /// <summary>Returns one action's button using the controller-menu row order.</summary>
    public SnesButton this[int action]
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
    public ControllerBindings AssignAndSwap(int action, SnesButton physicalButton)
    {
        if ((uint)action >= 7)
            throw new ArgumentOutOfRangeException(nameof(action));
        if (AssignableButtonIndex(physicalButton) is null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(physicalButton), physicalButton, "Button is not retail-assignable.");
        }

        SnesButton displaced = this[action];
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
    public SnesButton Normalize(SnesButton physicalInput)
    {
        const SnesButton fixedControls =
            SnesButton.Up | SnesButton.Down | SnesButton.Left | SnesButton.Right |
            SnesButton.Start;
        SnesButton normalized = physicalInput & fixedControls;
        normalized = Map(physicalInput, Shoot, SnesButton.X, normalized);
        normalized = Map(physicalInput, Jump, SnesButton.A, normalized);
        normalized = Map(physicalInput, Dash, SnesButton.B, normalized);
        normalized = Map(physicalInput, ItemSelect, SnesButton.Select, normalized);
        normalized = Map(physicalInput, ItemCancel, SnesButton.Y, normalized);
        normalized = Map(physicalInput, AimUp, SnesButton.R, normalized);
        normalized = Map(physicalInput, AimDown, SnesButton.L, normalized);
        return normalized;
    }

    /// <summary>True when all seven words are single, distinct retail-assignable buttons.</summary>
    public bool IsRetailPermutation
    {
        get
        {
            Span<SnesButton> seen = stackalloc SnesButton[7];
            for (int action = 0; action < 7; action++)
            {
                SnesButton button = this[action];
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
    private ControllerBindings WithAction(int action, SnesButton value) => action switch
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
    private static SnesButton Map(
        SnesButton physicalInput,
        SnesButton physicalBinding,
        SnesButton canonicalAction,
        SnesButton normalized) =>
        (physicalInput & physicalBinding) != SnesButton.None
            ? normalized | canonicalAction
            : normalized;
}
