using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// What a cinematic family's native lists do that its owner carries out (#2197): each owner instruction they hold and each
/// routine they install, by the family's own cue.
/// </summary>
/// <typeparam name="TCue">The family's cue.</typeparam>
/// <param name="Instructions">Each owner instruction word the lists hold, by the cue it becomes.</param>
/// <param name="Installs">Each routine the lists install, by the cue that starts its motion.</param>
internal sealed record NativeCinematicCues<TCue>(IReadOnlyDictionary<ushort, TCue> Instructions, IReadOnlyDictionary<ushort, TCue> Installs)
    where TCue : notnull
{
    /// <summary>The cues of a family whose lists hold no owner instruction and install no routine.</summary>
    internal static NativeCinematicCues<TCue> None { get; } = new(new Dictionary<ushort, TCue>(), new Dictionary<ushort, TCue>());

    /// <summary>
    /// Each routine the lists install whose own updates raise the family's cues (#2489), by the reader of that routine,
    /// which runs on every update of the actor from the next on, before its list.
    /// </summary>
    internal IReadOnlyDictionary<ushort, Func<IImportCartridgeSource, INativeCinematicRoutine<TCue>>> Routines { get; init; } =
        new Dictionary<ushort, Func<IImportCartridgeSource, INativeCinematicRoutine<TCue>>>();
}
