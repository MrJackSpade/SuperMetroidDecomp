using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A7 function words stored in Dachora variable F. These are native addresses, not
/// host-only state IDs, so debugger watches can be compared directly with the cartridge.
/// </summary>
public enum DachoraAiFunction : ushort
{
    /// <summary><c>$A7:F570</c>: probes the floor and waits for Samus to enter the activation rectangle.</summary>
    WaitingForSamus = 0xf570,

    /// <summary><c>$A7:F5BC</c>: runs the 120-update warning countdown before installing the first run list.</summary>
    BlinkingBeforeRun = 0xf5bc,

    /// <summary><c>$A7:F5ED</c>: accelerates left along the floor until the turn coordinate or a wall.</summary>
    RunningLeft = 0xf5ed,

    /// <summary><c>$A7:F65E</c>: accelerates right until the tutorial launch coordinate or a wall.</summary>
    RunningRight = 0xf65e,

    /// <summary><c>$A7:F78F</c>: cycles the shine palette and counts down to the vertical launch.</summary>
    ChargingShinespark = 0xf78f,

    /// <summary><c>$A7:F806</c>: accelerates upward, publishes echo positions, and transitions on ceiling collision.</summary>
    Shinesparking = 0xf806,

    /// <summary><c>$A7:F935</c>: applies capped gravity until floor collision resumes the running phase.</summary>
    Falling = 0xf935,

    /// <summary><c>$A7:F98C</c>: alternates one physical echo slot's visibility for its eight-update lifetime.</summary>
    Echo = 0xf98c,
}

/// <summary>
/// Typed view of the six native Dachora variables. Their meanings intentionally change
/// between phases: A/B are horizontal speed while running and vertical speed after the
/// shinespark; D is vertical subacceleration on the body but echo lifetime on echo slots.
/// </summary>
public sealed class DachoraEnemyState
{
    /// <summary>Physical Dachora body or echo slot carrying its phase-dependent variables.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects Dachora's six native words from its occupied enemy slot.</summary>
    internal DachoraEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Variable A: whole speed, countdown, or echo-position interval.</summary>
    public ushort SpeedOrTimer
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Variable B: fractional speed.</summary>
    public ushort Subspeed
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Variable C: whole vertical acceleration during the shinespark.</summary>
    public ushort VerticalAcceleration
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Variable D: fractional vertical acceleration or echo visibility lifetime.</summary>
    public ushort VerticalSubaccelerationOrLifetime
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Variable E: packed palette-animation frame and low-byte countdown.</summary>
    public ushort PaletteAnimationTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Variable F: bank-$A7 indirect main-AI address.</summary>
    public DachoraAiFunction Function
    {
        get => (DachoraAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
