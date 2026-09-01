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
    ushort Shoot,
    ushort Jump,
    ushort Dash,
    ushort ItemSelect,
    ushort ItemCancel,
    ushort AimUp,
    ushort AimDown)
{
    /// <summary>The literal bindings installed by <c>NewSaveFile</c> at <c>$81:B2CB</c>.</summary>
    public static ControllerBindings Default => new(
        Shoot: (ushort)SnesButton.X,
        Jump: (ushort)SnesButton.A,
        Dash: (ushort)SnesButton.B,
        ItemSelect: (ushort)SnesButton.Select,
        ItemCancel: (ushort)SnesButton.Y,
        AimUp: (ushort)SnesButton.R,
        AimDown: (ushort)SnesButton.L);

    /// <summary>
    /// Physical buttons admitted by <c>OptionsMenuControllerFunc_0</c>, in the same order
    /// as ROM table <c>$82:F558</c>. Its final Left/Right entries are not examined by that
    /// routine; only these first seven can be assigned in an ordinary retail options menu.
    /// </summary>
    public static ReadOnlySpan<ushort> AssignableButtons =>
    [
        (ushort)SnesButton.X,
        (ushort)SnesButton.A,
        (ushort)SnesButton.B,
        (ushort)SnesButton.Select,
        (ushort)SnesButton.Y,
        (ushort)SnesButton.L,
        (ushort)SnesButton.R,
    ];

    /// <summary>Returns one action word using the controller-menu row order.</summary>
    public ushort this[int action]
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
    public ControllerBindings AssignAndSwap(int action, ushort physicalButton)
    {
        if ((uint)action >= 7)
            throw new ArgumentOutOfRangeException(nameof(action));
        if (AssignableButtons.IndexOf(physicalButton) < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(physicalButton), physicalButton, "Button is not retail-assignable.");
        }

        ushort displaced = this[action];
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
    public ushort Normalize(ushort physicalInput)
    {
        const SnesButton fixedControls =
            SnesButton.Up | SnesButton.Down | SnesButton.Left | SnesButton.Right |
            SnesButton.Start;
        ushort normalized = unchecked((ushort)(physicalInput & (ushort)fixedControls));
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
            Span<ushort> seen = stackalloc ushort[7];
            for (int action = 0; action < 7; action++)
            {
                ushort button = this[action];
                if (AssignableButtons.IndexOf(button) < 0 || seen[..action].Contains(button))
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
                $"buttons: shoot=${Shoot:X4}, jump=${Jump:X4}, dash=${Dash:X4}, " +
                $"select=${ItemSelect:X4}, cancel=${ItemCancel:X4}, " +
                $"aim-up=${AimUp:X4}, aim-down=${AimDown:X4}.");
        }

        return this;
    }

    private ControllerBindings WithAction(int action, ushort value) => action switch
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

    private static ushort Map(
        ushort physicalInput,
        ushort physicalBinding,
        SnesButton canonicalAction,
        ushort normalized) =>
        (physicalInput & physicalBinding) != 0
            ? unchecked((ushort)(normalized | (ushort)canonicalAction))
            : normalized;
}
