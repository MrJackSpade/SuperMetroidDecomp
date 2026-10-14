namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-facing projection of one ninja Pirate. Variables A-F remain backed by the
/// ordinary enemy record. The final three properties represent the otherwise-unaliased
/// bank-$7E:7800 scratch words used for current speed, a calculated dive target, and spawn Y.
/// </summary>
public sealed class NinjaSpacePirateEnemyState
{
    /// <summary>Physical enemy slot backing native decision and post-position variables A-F.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects one ninja Pirate slot into typed debugger state.</summary>
    internal NinjaSpacePirateEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Bank-$B2 movement/decision function retained in native variable A.</summary>
    public NinjaSpacePirateFunction Function
    {
        get => (NinjaSpacePirateFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>The derived peak spin-jump speed written to native variable B.</summary>
    public ushort CalculatedSpinJumpSpeed
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>The active left/right instruction-list pointer retained in variable C.</summary>
    public ushort ActiveInstruction
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Gets the horizontal midpoint of the adjusted post pair.</summary>
    public ushort PostsMidpointX
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Gets the adjusted left-post horizontal position in room pixels.</summary>
    public ushort LeftPostX
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Gets the adjusted right-post horizontal position in room pixels.</summary>
    public ushort RightPostX
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Gets the current native subpixel speed accumulator.</summary>
    public ushort Speed { get; internal set; }
    /// <summary>Gets the horizontal target calculated for the current divekick.</summary>
    public ushort DiveTargetX { get; internal set; }
    /// <summary>Gets the vertical spawn position used as the jump baseline.</summary>
    public ushort SpawnY { get; internal set; }
    /// <summary>Gets the host diagnostic count of claw projectiles requested by this actor.</summary>
    public int SpawnedClawCount { get; internal set; }
    /// <summary>Gets the host diagnostic count of landing-dust effects requested by this actor.</summary>
    public int LandingDustCount { get; internal set; }
}
