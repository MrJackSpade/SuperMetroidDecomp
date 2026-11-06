using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Where Botwoon's wall PLM is spawned, and the PLM, program and first delay of each wall mutation.</summary>
internal static class BotwoonWallMutationDefinitions
{
    /// <summary>The room block both wall mutations are spawned at: the wall's top block.</summary>
    internal static readonly Position16<RoomBlocks> Block = new(15, 4);

    /// <summary>The room PLM each mutation occupies its slot as.</summary>
    internal static readonly IReadOnlyDictionary<BotwoonWallMutation, PlmHeaderId> Plms =
        new Dictionary<BotwoonWallMutation, PlmHeaderId>
        {
            [BotwoonWallMutation.Clear] = PlmHeaderId.ClearBotwoonWall,
            [BotwoonWallMutation.Crumble] = PlmHeaderId.CrumbleBotwoonWall,
        };

    /// <summary>The wall program each mutation runs.</summary>
    internal static readonly IReadOnlyDictionary<BotwoonWallMutation, BotwoonWallProgram> Programs =
        new Dictionary<BotwoonWallMutation, BotwoonWallProgram>
        {
            [BotwoonWallMutation.Clear] = BotwoonWallProgram.Clear,
            [BotwoonWallMutation.Crumble] = BotwoonWallProgram.Crumble,
        };

    /// <summary>
    /// Frames before each mutation's program takes its first step: the clear starts on the next pass, and the crumble's
    /// setup holds it for 64 frames before the wall begins to fall.
    /// </summary>
    internal static readonly IReadOnlyDictionary<BotwoonWallMutation, int> FirstStepDelays =
        new Dictionary<BotwoonWallMutation, int>
        {
            [BotwoonWallMutation.Clear] = 1,
            [BotwoonWallMutation.Crumble] = 64,
        };
}
