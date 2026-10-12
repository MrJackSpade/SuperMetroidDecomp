using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Exact bank-$A2 function words dispatched by Lower Norfair Rio main AI.</summary>
public enum LowerNorfairRioEnemyFunction : ushort
{
    /// <summary>Native <c>Function_Holtz_Flames</c> at <c>$A2:C72E</c>; follows the immediately preceding parent twelve pixels below, mirrors its freeze state and flame visibility, and deletes the follower when the parent dies.</summary>
    FollowParent = 0xc72e,
    /// <summary>Native <c>Function_Holtz_Idle</c> at <c>$A2:C771</c>; admits an attack when the RNG mask <c>$0101</c> is nonzero and Samus is strictly within 112 horizontal room pixels.</summary>
    WaitForAttackOpportunity = 0xc771,
    /// <summary>Native <c>Function_Holtz_PrepareToSwoop</c> at <c>$A2:C7BB</c>; waits for the animation instruction's completion signal before clearing it and starting downward motion.</summary>
    WaitForTakeoffAnimation = 0xc7bb,
    /// <summary>Native <c>Function_Holtz_Swoop_Descending</c> at <c>$A2:C7D6</c>; moves horizontally and downward, reverses X velocity on walls, and transitions to return on vertical collision or a negative vertical velocity.</summary>
    Dive = 0xc7d6,
    /// <summary>Native <c>Function_Holtz_Swoop_Ascending</c> at <c>$A2:C82D</c>; accelerates upward while retaining horizontal movement, switches ascent artwork on the animation signal, and enters landing wait on vertical collision rather than targeting a saved Y.</summary>
    ReturnToPerch = 0xc82d,
    /// <summary>Native <c>Function_Holtz_SwoopCooldown</c> at <c>$A2:C888</c>; consumes the landing animation's signal, installs cooldown artwork, and resumes attack-opportunity waiting.</summary>
    WaitForLandingAnimation = 0xc888,
}

/// <summary>
/// Typed projection of Lower Norfair Rio's C/D/F variables and three family-extra words.
/// This variant shares the physical parent/follower layout with $D2FF but deliberately uses
/// different common velocity words and interprets extra word $02 as a visibility switch.
/// </summary>
public sealed class LowerNorfairRioEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal LowerNorfairRioEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Last bank-$A2 instruction-list identity installed for this actor, native <c>Holtz.instList</c> at <c>$7E:7800,x</c>; distinct from the advancing cursor and prevents timer resets when the same list is requested again.</summary>
    internal LowerNorfairRioProgram InstalledInstructionList { get; set; }

    /// <summary>Extra word $01, set by private animation instruction $C6D2.</summary>
    public bool AnimationSignal { get; internal set; }

    /// <summary>
    /// Extra word $02, cleared/set by $C6DD/$C6E8. The following physical slot reads this
    /// word through the cartridge's spawn-data alias and shows its lower sprite only at one.
    /// </summary>
    public bool FollowerVisible { get; internal set; }

    /// <summary>Signed 8.8 velocity: X stored in common variable D, Y in common variable C.</summary>
    public EightEightVector<RoomPixels> Velocity
    {
        get => new(new SnesSignedEightEight(_slot.VariableD), new SnesSignedEightEight(_slot.VariableC));
        internal set
        {
            _slot.VariableC = value.Y.RawValue;
            _slot.VariableD = value.X.RawValue;
        }
    }

    /// <summary>Current indirect bank-$A2 AI address backed by the owning slot's variable F at native <c>$0FB2,x</c>; parent and flame follower dispatch through this same word.</summary>
    public LowerNorfairRioEnemyFunction Function
    {
        get => (LowerNorfairRioEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Whether population parameter 1's sign bit selects the lower flame follower instead of the parent; a follower must immediately follow a parent of the same enemy definition.</summary>
    public bool IsFollower => (_slot.Parameter1 & 0x8000) != 0;
}
