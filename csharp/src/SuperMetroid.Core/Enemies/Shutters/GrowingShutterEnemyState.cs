namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// Typed view of growing-shutter enemy variables and its two extended movement words.
/// Origin words B-E are the four independently positioned 16-pixel growth sections.
/// </summary>
public sealed class GrowingShutterEnemyState
{
    /// <summary>Backs native variable A-F access for one growing-shutter slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates the typed view over a room-owned shutter slot.</summary>
    internal GrowingShutterEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Enemy variable A's bank-$A2 dispatcher pointer; initialization selects a wait function and activation replaces it with a growth function.</summary>
    public GrowingShutterFunction Function
    {
        get => (GrowingShutterFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Variable B: the first section's whole-pixel Y origin, initialized to the spawn position.</summary>
    public ushort GrowthLevel0OriginY { get => _slot.VariableB; internal set => _slot.VariableB = value; }
    /// <summary>Variable C: the second section's Y origin, eight pixels above or below the spawn position according to growth direction.</summary>
    public ushort GrowthLevel1OriginY { get => _slot.VariableC; internal set => _slot.VariableC = value; }
    /// <summary>Variable D: the third section's Y origin, sixteen pixels above or below the spawn position, wrapping as a native word.</summary>
    public ushort GrowthLevel2OriginY { get => _slot.VariableD; internal set => _slot.VariableD = value; }
    /// <summary>Variable E: the fourth section's Y origin, twenty-four pixels above or below the spawn position, wrapping as a native word.</summary>
    public ushort GrowthLevel3OriginY { get => _slot.VariableE; internal set => _slot.VariableE = value; }

    /// <summary>Variable F; zero through four, where four is the fully grown terminal state.</summary>
    public ushort GrowthLevel { get => _slot.VariableF; internal set => _slot.VariableF = value; }

    /// <summary>Signed whole word of the positive 16.16 growth speed.</summary>
    public short GrowthVelocity { get; internal set; }

    /// <summary>Fraction word of the positive 16.16 growth speed.</summary>
    public ushort GrowthSubvelocity { get; internal set; }

    /// <summary>Extended word $7E:8800,x used to derive Samus's whole-pixel carry.</summary>
    public ushort PreviousYPosition { get; internal set; }

    /// <summary>Selects the current growth section's stored Y origin; level four has no next section.</summary>
    internal ushort OriginForLevel() => GrowthLevel switch
    {
        0 => GrowthLevel0OriginY,
        1 => GrowthLevel1OriginY,
        2 => GrowthLevel2OriginY,
        3 => GrowthLevel3OriginY,
        _ => throw new InvalidOperationException(
            $"Growing shutter level {GrowthLevel} has no section origin."),
    };
}
