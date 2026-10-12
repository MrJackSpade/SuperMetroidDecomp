using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Exact bank-$A2 function words dispatched by Norfair Rio main AI.</summary>
public enum NorfairRioEnemyFunction : ushort
{
    /// <summary>Native <c>Function_Geruta_Flames</c> at <c>$A2:C281</c>; follows the preceding parent using its signed animation-authored Y offset, hides for parent freeze/idle, and deletes the follower when the parent dies.</summary>
    FollowParent = 0xc281,
    /// <summary>Native <c>Function_Geruta_Idle</c> at <c>$A2:C2E7</c>; admits an attack when RNG mask <c>$0101</c> is nonzero and Samus is strictly within 192 horizontal room pixels, sampling a launch velocity from that RNG word.</summary>
    WaitForAttackOpportunity = 0xc2e7,
    /// <summary>Native <c>Function_Geruta_StartSwoop</c> at <c>$A2:C33F</c>; waits for the animation handshake, then installs descending artwork, enters the dive, and requests library-two sound <c>$65</c>.</summary>
    WaitForTakeoffAnimation = 0xc33f,
    /// <summary>Native <c>Function_Geruta_Swoop_Descending</c> at <c>$A2:C361</c>; moves down and horizontally with wall reversal, transitioning to return on vertical collision or when vertical velocity becomes negative.</summary>
    Dive = 0xc361,
    /// <summary>Native <c>Function_Geruta_Swoop_Ascending</c> at <c>$A2:C3B1</c>; accelerates upward while moving horizontally and responding to animation signals, ending on vertical collision rather than a saved perch coordinate.</summary>
    ReturnToPerch = 0xc3b1,
    /// <summary>Native <c>Function_Geruta_FinishSwoop</c> at <c>$A2:C406</c>; one-update state that restores only the idle dispatch address, leaving the existing looping animation list and follower visibility intact.</summary>
    FinishLanding = 0xc406,
}

/// <summary>
/// Typed projection of Norfair Rio's common A/B/F variables and three family-extra words.
/// The extra words live in bank-$7E family RAM rather than the ordinary enemy record, so
/// they remain explicit host fields while velocities/function stay backed by the slot.
/// </summary>
public sealed class NorfairRioEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal NorfairRioEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Extra word $00: last instruction list installed by function seven.</summary>
    public ushort InstalledInstructionList { get; internal set; }

    /// <summary>Extra word $01: no-operand animation handshake set by opcode $C1C9.</summary>
    public bool AnimationSignal { get; internal set; }

    /// <summary>
    /// Extra word $02: signed vertical placement of the follower half relative to its
    /// parent. Ten private animation opcodes publish the offset directly from ROM lists.
    /// </summary>
    public ushort FollowerYOffset { get; internal set; }

    /// <summary>Signed 8.8 velocity: the horizontal word is native common variable B and the vertical word variable A; the horizontal word feeds the 16.16 collision helper.</summary>
    public EightEightVector<RoomPixels> Velocity
    {
        get => new(new SnesSignedEightEight(_slot.VariableB), new SnesSignedEightEight(_slot.VariableA));
        internal set
        {
            _slot.VariableB = value.X.RawValue;
            _slot.VariableA = value.Y.RawValue;
        }
    }

    /// <summary>Current indirect bank-$A2 AI address, native <c>Geruta.function</c> in the owning slot's variable F at <c>$0FB2,x</c>; shared by parent and flame-follower dispatch.</summary>
    public NorfairRioEnemyFunction Function
    {
        get => (NorfairRioEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Whether population parameter 1's sign bit selects the flame follower instead of the parent; a follower must immediately follow a parent of this same enemy definition.</summary>
    public bool IsFollower => (_slot.Parameter1 & 0x8000) != 0;
}
