using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Exact bank-$A2 function words dispatched by Rio main AI at $A2:BBE3.</summary>
public enum RioEnemyFunction : ushort
{
    /// <summary>$A2:BBED Function_Rio_WaitForSamusToGetNear: starts a dive when Samus is within a strict 160-world-pixel horizontal distance, selecting launch direction toward her.</summary>
    WaitingForSamus = 0xbbed,
    /// <summary>$A2:BC32 Function_Rio_SwoopCooldown: waits for the $A2:BBC3 animation handshake, then installs the post-swoop idle list and resumes proximity waiting.</summary>
    WaitingForLandingAnimation = 0xbc32,
    /// <summary>$A2:BC48 Function_Rio_Swoop_Descending: moves with signed 8.8 velocities, subtracting $0018 from Y velocity per AI update until homing begins or collision starts the bounce.</summary>
    Diving = 0xbc48,
    /// <summary>$A2:BCB7 Function_Rio_Swoop_Ascending: follows the upward bounce with signed 8.8 acceleration; horizontal collision reverses X, and vertical collision starts the cooldown animation.</summary>
    BouncingBackToPerch = 0xbcb7,
    /// <summary>$A2:BCFF Function_Rio_Homing: uses angle-table samples as 8.8 velocities while above Samus, then restores saved horizontal speed and begins ascent at Y velocity $FFFF.</summary>
    HoveringTowardSamus = 0xbcff,
}

/// <summary>
/// Named projection of Rio's six native AI variables. The properties deliberately remain
/// backed by the physical enemy slot so a debugger watch agrees with WRAM $0FA8-$0FB2.
/// </summary>
public sealed class RioEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new rio enemy state instance.</summary>
    internal RioEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Horizontal velocity retained while the hovering phase temporarily stops X.</summary>
    public ushort SavedHorizontalVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Gets the bank-$A2 function pointer corresponding to native $0FAA,x Rio.function, backed by the physical slot and dispatched after the main AI's RNG advance.</summary>
    public RioEnemyFunction Function
    {
        get => (RioEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>Signed 8.8 velocity: the horizontal word is native common variable D and the vertical word variable C; the horizontal word feeds the 16.16 collision helper.</summary>
    public EightEightVector<RoomPixels> Velocity
    {
        get => new(new SnesSignedEightEight(_slot.VariableD), new SnesSignedEightEight(_slot.VariableC));
        internal set
        {
            _slot.VariableD = value.X.RawValue;
            _slot.VariableC = value.Y.RawValue;
        }
    }

    /// <summary>Handshake set by instruction $A2:BBC3 and consumed by main AI.</summary>
    public bool AnimationFinished
    {
        get => _slot.VariableE != 0;
        internal set => _slot.VariableE = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Last instruction-list address installed by Rio_6; zero at room load.</summary>
    public ushort InstalledInstructionList
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }
}
