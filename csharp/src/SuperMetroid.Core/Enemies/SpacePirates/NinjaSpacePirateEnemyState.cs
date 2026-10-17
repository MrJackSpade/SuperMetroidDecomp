using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Debugger-facing projection of one ninja Pirate. Variables A-F remain backed by the ordinary enemy record, whose
/// words other enemies can read as enemy RAM, so each typed view keeps its word exact. The speed and spawn position
/// are the ninja's own state, which nothing else reads.
/// </summary>
public sealed class NinjaSpacePirateEnemyState
{
    /// <summary>Physical enemy slot backing native decision and post-position variables A-F.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects one ninja Pirate slot into typed debugger state.</summary>
    internal NinjaSpacePirateEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<NinjaSpacePirateProgram> Program { get; set; }

    /// <summary>Bank-$B2 movement/decision function retained in native variable A.</summary>
    public NinjaSpacePirateFunction Function
    {
        get => (NinjaSpacePirateFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// The peak spin-jump speed derived at initialization (native variable B), in 1/256 pixel per update: 32 for each
    /// 1/8-pixel acceleration step whose running distance first covers the requested half-span, from 32 to 1,440.
    /// </summary>
    public int CalculatedSpinJumpSpeed
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = checked((ushort)value);
    }

    /// <summary>Native variable C: the active stance animation, facing Samus, that becoming active continues at.</summary>
    internal NinjaSpacePirateProgram ActiveProgram { get; set; }

    /// <summary>
    /// The room position midway between the adjusted posts (native variable D). Only the X axis is stored and
    /// meaningful; Samus and the Pirate are compared with it by the wrapped spatial difference on X.
    /// </summary>
    public Position16<RoomPixels> PostsMidpoint
    {
        get => new(_slot.VariableD, 0);
        internal set => _slot.VariableD = value.X;
    }

    /// <summary>
    /// The adjusted left post (native variable E). Only the X axis is stored and meaningful: a Pirate whose X equals
    /// it stands at the left post, and a return walk ends on passing it.
    /// </summary>
    public Position16<RoomPixels> LeftPost
    {
        get => new(_slot.VariableE, 0);
        internal set => _slot.VariableE = value.X;
    }

    /// <summary>
    /// The adjusted right post (native variable F). Only the X axis is stored and meaningful: a return walk ends on
    /// reaching or passing it.
    /// </summary>
    public Position16<RoomPixels> RightPost
    {
        get => new(_slot.VariableF, 0);
        internal set => _slot.VariableF = value.X;
    }

    /// <summary>
    /// The current signed 8.8 speed in 1/256 pixel per update: the spin jump's horizontal speed, which rises by 32 per
    /// update until the midpoint and falls back to zero, or the divekick's vertical speed, which starts at 1,536 and
    /// falls by 64 per update. Its sign is load-bearing: the divekick rise ends on the update the speed turns negative.
    /// </summary>
    public SnesSignedEightEight Speed { get; internal set; }

    /// <summary>
    /// The room position recorded at initialization, once the Pirate stands at its starting post; a divekick landing
    /// restores its Y.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>Gets the host diagnostic count of claw projectiles requested by this actor.</summary>
    public int SpawnedClawCount { get; internal set; }
    /// <summary>Gets the host diagnostic count of landing-dust effects requested by this actor.</summary>
    public int LandingDustCount { get; internal set; }

    /// <summary>
    /// Changes <see cref="Speed"/> by <paramref name="delta"/> 1/256-pixel units, wrapping as the cartridge's
    /// sixteen-bit speed does.
    /// </summary>
    internal void ChangeSpeed(int delta) => Speed = new SnesSignedEightEight(unchecked((ushort)(Speed.RawValue + delta)));
}
