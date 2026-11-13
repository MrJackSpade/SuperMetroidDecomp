using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.PipeBugs.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// Translates the Pipe Bugs' typed animation playbacks to and from the native instruction pointers a recording holds
/// (#1600), so an imported native state resumes the same animation and replay comparisons keep checking it. Each beat
/// sits at the native instruction the ROM oracle matched it with, and the end of a phase's play at the control
/// instruction the pointer rests on after it.
/// </summary>
internal sealed class PipeBugNativeInstructionPointers
{
    /// <summary>Each segment's native list.</summary>
    private readonly Dictionary<PipeBugSegment, NativeEnemyList<PipeBugSegment>> lists;

    /// <summary>Walks every typed Pipe Bug segment beside its native list, failing on any mismatch.</summary>
    internal PipeBugNativeInstructionPointers(IImportCartridgeSource cartridge) =>
        lists = NativeEnemyAnimationOracle.Lists(cartridge, PipeBugNativeListAddresses.Bank, PipeBugNativeListAddresses.Entries,
            PipeBugAnimations.Segments, PipeBugFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(_ => null,
                (word, _, _, at) => throw new InvalidDataException($"{at}: the Pipe Bug lists use no owner instruction ${word:X4}.")));

    /// <summary>The native instruction pointer <paramref name="playback"/> stands for.</summary>
    internal ushort Pointer(AnimationPlayback<PipeBugSegment> playback) =>
        NativeEnemyAnimationOracle.PointerOf(lists, PipeBugNativeListAddresses.Entries, PipeBugAnimations.StartingAt, playback);

    /// <summary>The playback native pointer <paramref name="address"/> stands for: an animation's start when it is a segment entry.</summary>
    internal AnimationPlayback<PipeBugSegment> PlaybackAt(ushort address) =>
        NativeEnemyAnimationOracle.PlaybackAt(lists, PipeBugNativeListAddresses.Entries, PipeBugAnimations.StartingAt, address);
}
