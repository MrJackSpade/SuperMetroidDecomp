using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// Translates Ridley's body playback to and from the native instruction pointers a recording holds (#1287, #1600). A
/// check that compares only the body's pointer builds this alone; <see cref="RidleyNativeInstructionPointers"/> adds the
/// fragments, fireballs, enemy projectiles and Pipe Bugs a whole-room replay compares (#2419).
/// </summary>
internal sealed class RidleyBodyNativeInstructionPointers
{
    /// <summary>Each body segment's beat and control addresses.</summary>
    private readonly Dictionary<RidleyBodySegment, NativeEnemyList<RidleyBodySegment>> body;

    /// <summary>Walks every typed body segment beside its native list, failing on any mismatch.</summary>
    internal RidleyBodyNativeInstructionPointers(IImportCartridgeSource cartridge) =>
        body = RidleyNativeProgramOracle.BodyLists(cartridge);

    /// <summary>
    /// The native instruction pointer of Ridley's body, from <paramref name="state"/>'s body playback. Before its first play,
    /// an animation his owner started for his facing stands at its left-facing list's entry, which the cartridge starts for
    /// either facing (#2175).
    /// </summary>
    internal ushort Body(RidleyFlightState state) =>
        (state.BodyPlayback.Place.Beat == AnimationPlace.NotEntered
            ? RidleyNativeProgramOracle.FacedStartPointer(state.BodyPlayback.Animation)
            : null)
        ?? NativeEnemyAnimationOracle.PointerOf(body, RidleyNativeProgramOracle.BodyListEntries, RidleyAnimations.StartingAt, state.BodyPlayback);

    /// <summary>The native entry point of body <paramref name="program"/>.</summary>
    internal static ushort BodyEntry(RidleyBodySegment program) =>
        RidleyNativeListAddresses.BodyEntries.Single(item => item.Program == program).Entry;

    /// <summary>
    /// The body playback a recording's native pointer <paramref name="address"/> stands for, for a Ridley facing
    /// <paramref name="facing"/>: the start of the animation for his facing at the left-facing list of an animation his owner
    /// starts by facing (#2175), the start of the animation starting at any other list whose entry it is, otherwise the beat
    /// or the list end it stands at.
    /// </summary>
    internal AnimationPlayback<RidleyBodySegment> BodyPlaybackAt(ushort address, RidleyFacing facing) =>
        RidleyNativeProgramOracle.FacedStartAt(address, facing)
        ?? NativeEnemyAnimationOracle.PlaybackAt(body, RidleyNativeProgramOracle.BodyListEntries, RidleyAnimations.StartingAt, address);
}
