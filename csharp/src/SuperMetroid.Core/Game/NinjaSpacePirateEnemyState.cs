using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Confirmed meanings in ninja-Pirate population parameter one. The cartridge currently
/// tests only bit zero; preserving the remaining bits as raw data avoids assigning names to
/// behavior that the retail routine never actually proves.
/// </summary>
[Flags]
public enum NinjaSpacePirateParameterFlags : ushort
{
    /// <summary>Starts at the left post and initially faces toward the right post.</summary>
    StartsAtLeftPostFacingRight = 0x0001,
}

/// <summary>
/// Literal bank-$B2 function pointers stored in native enemy variable A. These values are
/// intentionally addresses, so debugger state can be compared directly with WRAM and the
/// disassembly instead of being hidden behind a host-only state number.
/// </summary>
public enum NinjaSpacePirateFunction : ushort
{
    /// <summary>$B2:804B, shared return stub used while animation bytecode owns the actor.</summary>
    NoOperation = 0x804b,
    /// <summary>$B2:F6A9, waits for Samus to enter activation range.</summary>
    Initial = 0xf6a9,
    /// <summary>$B2:F6E4, chooses flinch, kick, jump, or claw attacks.</summary>
    Active = 0xf6e4,
    /// <summary>$B2:F817, rises during a leftward post-to-post spin jump.</summary>
    SpinJumpLeftRising = 0xf817,
    /// <summary>$B2:F84C, falls during a leftward post-to-post spin jump.</summary>
    SpinJumpLeftFalling = 0xf84c,
    /// <summary>$B2:F890, rises during a rightward post-to-post spin jump.</summary>
    SpinJumpRightRising = 0xf890,
    /// <summary>$B2:F8C5, falls during a rightward post-to-post spin jump.</summary>
    SpinJumpRightFalling = 0xf8c5,
    /// <summary>$B2:F909, waits at the jump peak for a divekick opportunity.</summary>
    ReadyToDivekick = 0xf909,
    /// <summary>$B2:F985, begins the leftward divekick jump.</summary>
    DivekickLeftJump = 0xf985,
    /// <summary>$B2:F9C1, performs the leftward collision-bearing dive.</summary>
    DivekickLeftDive = 0xf9c1,
    /// <summary>$B2:FA15, walks leftward back to the authored post.</summary>
    DivekickLeftWalkToPost = 0xfa15,
    /// <summary>$B2:FA59, begins the rightward divekick jump.</summary>
    DivekickRightJump = 0xfa59,
    /// <summary>$B2:FA95, performs the rightward collision-bearing dive.</summary>
    DivekickRightDive = 0xfa95,
    /// <summary>$B2:FAE9, walks rightward back to the authored post.</summary>
    DivekickRightWalkToPost = 0xfae9,
}

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
