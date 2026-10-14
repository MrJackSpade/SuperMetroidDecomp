namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// Typed projection of Norfair Rio's common A/B/F variables and three family-extra words.
/// The extra words live in bank-$7E family RAM rather than the ordinary enemy record, so
/// they remain explicit host fields while velocities/function stay backed by the slot.
/// </summary>
public sealed class NorfairRioEnemyState
{
    /// <summary>Native Rio slot holding the common velocities, function pointer, and population parameter.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Binds typed Rio state to its existing enemy slot.</summary>
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
