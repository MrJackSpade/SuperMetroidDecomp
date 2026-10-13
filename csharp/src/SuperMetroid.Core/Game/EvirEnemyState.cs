using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>Native bank-$A8 function words stored by the Evir composite actors.</summary>
public enum EvirAiFunction : ushort
{
    /// <summary>$A8:8922, <c>Function_Evir_HandleBodyArms</c>: bobs the body vertically or positions its following arms record.</summary>
    HandleBodyOrArms = 0x8922,
    /// <summary>$A8:8A34, <c>Function_EvirProjectile_Idle</c>: keeps the reusable projectile at the body's mouth until a launch is selected.</summary>
    ProjectileIdle = 0x8a34,
    /// <summary>$A8:8A3B, <c>Function_EvirProjectile_Moving</c>: checks for regeneration beyond the viewport, then advances the aimed projectile's fixed-point position.</summary>
    ProjectileMoving = 0x8a3b,
    /// <summary>$A8:8A78, <c>Function_EvirProjectile_Regenerating</c>: anchors the growing projectile to the mouth plus its animation-controlled X offset; a frozen body holds this state.</summary>
    ProjectileRegenerating = 0x8a78,
}

/// <summary>
/// Typed view of the common and extended enemy RAM used by Evir (also called Mini-Draygon
/// in older source annotations). The cartridge builds each creature from three consecutive
/// physical enemy records: body, arms, and a reusable aimed projectile.
/// </summary>
public sealed class EvirEnemyState
{
    /// <summary>Common 64-byte enemy record backing Evir's native B, C, E, and F variables.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Attaches Evir's typed common-word view to one physical body, arms, or projectile record.</summary>
    internal EvirEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Zero for left and one for right; native common variable B.</summary>
    public ushort FacingDirection
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Bank-$A8 indirect function word in native common variable C.</summary>
    public EvirAiFunction Function
    {
        get => (EvirAiFunction)_slot.VariableC;
        internal set => _slot.VariableC = (ushort)value;
    }

    /// <summary>Body bobbing countdown in native common variable E.</summary>
    public NativeCountdown MovementTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableE);
        internal set => _slot.VariableE = value.RawValue;
    }

    /// <summary>Regeneration X offset in native common variable F.</summary>
    public ushort RegenerationXOffset
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    // The remaining words live in the parallel $7E:7800 extended-enemy array. Keeping them
    // on this state object preserves their native ownership without polluting the common
    // 64-byte RoomEnemySlot abstraction with one family's aliases.
    /// <summary>Body bobbing direction: zero selects upward velocity and one downward velocity; toggled when the movement countdown underflows. Native extended offset $00.</summary>
    public ushort MovementDirection { get; internal set; }
    /// <summary>Bank-$A8 base pointer of the last installed animation list, not its advancing instruction cursor; zero forces installation of the requested list. Native extended offset $02.</summary>
    public ushort InstalledInstructionList { get; internal set; }
    /// <summary>Bank-$A8 animation list requested by the actor's current facing or projectile phase; a changed request restarts the instruction timer at one. Native extended offset $04.</summary>
    public ushort RequestedInstructionList { get; internal set; }
    /// <summary>Signed whole-pixel component of the body's downward Y velocity per AI update, paired with <see cref="DownSubvelocity"/> as signed 16.16. Native extended offset $08.</summary>
    public short DownVelocity { get; internal set; }
    /// <summary>Unsigned fractional component of the body's downward Y velocity, in 1/65536 pixel per AI update. Native extended offset $06.</summary>
    public ushort DownSubvelocity { get; internal set; }
    /// <summary>Signed whole-pixel component of the body's upward Y velocity per AI update, paired with <see cref="UpSubvelocity"/> as signed 16.16. Native extended offset $0C.</summary>
    public short UpVelocity { get; internal set; }
    /// <summary>Unsigned fractional component of the body's upward Y velocity, in 1/65536 pixel per AI update; the whole component carries the negative sign. Native extended offset $0A.</summary>
    public ushort UpSubvelocity { get; internal set; }
    /// <summary>Signed 16.16 projectile velocity per AI update, calculated from its launch angle. Native extended offsets $0E/$10 (X whole/fraction) and $12/$14 (Y whole/fraction).</summary>
    public FixedVector<RoomPixels> ProjectileVelocity { get; internal set; }
    /// <summary>Projectile flight flag, one while moving and zero while idle or regenerating; flight selects the normal projectile animation. Native extended offset $16.</summary>
    public ushort MovingFlag { get; internal set; }
    /// <summary>Projectile growth flag, set to one when distant flight ends and cleared by the regeneration-complete animation command at $A8:87CB; also holds the body's facing during growth. Native extended offset $18.</summary>
    public ushort RegenerationFlag { get; internal set; }
}
