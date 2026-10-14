namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed projection of Lower Norfair Rio's C/D/F variables and three family-extra words.
/// This variant shares the physical parent/follower layout with $D2FF but deliberately uses
/// different common velocity words and interprets extra word $02 as a visibility switch.
/// </summary>
public sealed class LowerNorfairRioEnemyState
{
    /// <summary>Physical Rio slot whose common C/D/F words carry velocity and indirect AI dispatch.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects the Lower Norfair Rio state from an existing enemy slot without duplicating WRAM variables.</summary>
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
