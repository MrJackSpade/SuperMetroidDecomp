using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// Typed view of Cacatac's patrol velocities, direction and function, plus its two patrol bounds.
/// The rightward velocity's fraction is the one half the generic slot projection does not cover,
/// so it is held here; every other half stays in the slot's variable words.
/// </summary>
public sealed class CacatacEnemyState
{
    /// <summary>Common enemy slot backing Cacatac's native patrol velocity and function words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>
    /// Holds the rightward velocity's fraction, which has no slot word; its whole half is ignored and
    /// <see cref="RightVelocity"/> reads the whole from variable A.
    /// </summary>
    private SnesSignedSixteenSixteen _rightVelocityFraction;

    /// <summary>Projects a live Cacatac slot into typed patrol and spike-facing state.</summary>
    internal CacatacEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<CacatacProgram> Program { get; set; }

    /// <summary>
    /// Signed 16.16 room-pixel displacement added to X on every rightward patrol update. Its whole half stays
    /// in variable A, which other enemies' aliased enemy-RAM reads can observe; its fraction has no slot word.
    /// </summary>
    public SnesSignedSixteenSixteen RightVelocity
    {
        get => _rightVelocityFraction.WithWholePixels(unchecked((short)_slot.VariableA));
        internal set
        {
            _rightVelocityFraction = value;
            _slot.VariableA = value.WholeWord;
        }
    }

    /// <summary>
    /// Signed 16.16 room-pixel displacement added to X on every leftward patrol update (negative for the
    /// authored speeds). Its fraction and whole halves stay in variables B and C for the same aliased reads.
    /// </summary>
    public SnesSignedSixteenSixteen LeftVelocity
    {
        get => SnesSignedSixteenSixteen.FromParts(unchecked((short)_slot.VariableC), _slot.VariableB);
        internal set
        {
            _slot.VariableB = value.Fraction;
            _slot.VariableC = value.WholeWord;
        }
    }

    /// <summary>Native <c>$0FB0,x</c>; zero is left, one is right, two initially stops.</summary>
    public CacatacDirection Direction
    {
        get => (CacatacDirection)_slot.VariableD;
        internal set => _slot.VariableD = (ushort)value;
    }

    /// <summary>Native indirect function pointer at <c>$0FB2,x</c>.</summary>
    public CacatacEnemyFunction Function
    {
        get => (CacatacEnemyFunction)_slot.VariableE;
        internal set => _slot.VariableE = (ushort)value;
    }

    /// <summary>
    /// Left patrol limit; only X is meaningful. Leftward patrol turns right once the whole X position lies
    /// strictly left of it, measured as the wrapped signed difference, so a limit that wrapped past the room's
    /// left edge still turns the enemy as the cartridge does.
    /// </summary>
    public Position16<RoomPixels> LeftPatrolLimit { get; internal set; }

    /// <summary>
    /// Right patrol limit; only X is meaningful. Rightward patrol turns left once the whole X position reaches
    /// or passes it, measured as the wrapped signed difference.
    /// </summary>
    public Position16<RoomPixels> RightPatrolLimit { get; internal set; }

    /// <summary>
    /// Parameter-one's high-byte presentation selector. The assembly tests only zero versus
    /// nonzero, so this is a Boolean discriminator rather than a guessed flags enum.
    /// </summary>
    public bool UpsideUp { get; internal set; }
}
