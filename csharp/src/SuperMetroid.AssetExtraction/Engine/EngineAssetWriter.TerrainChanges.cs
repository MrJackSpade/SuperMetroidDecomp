using System.Text.Json.Nodes;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The changes a boss makes to its room's blocks as it fights and dies, which play out over some updates.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Every state of the room that holds one of the old game's enemies.</summary>
    /// <exception cref="InvalidDataException">No room holds it.</exception>
    private static List<(RoomDefinition Room, RoomStateSettings State)> StatesHolding(EnemyDefinitionId enemy)
    {
        var holding = new List<(RoomDefinition Room, RoomStateSettings State)>();
        var rooms = new HashSet<RoomDefinition>();
        foreach ((RoomDefinition room, RoomStateSettings state) in EngineLayouts.States())
        {
            if (state.EnemyPopulation.Placements.ToArray().Any(placement => placement.DefinitionId == enemy))
                rooms.Add(room);
        }

        if (rooms.Count == 0)
            throw new InvalidDataException($"No room holds {enemy}.");
        // A patch a thing's data names must be in the layout of every state of its room, whether or not it is there in that state.
        foreach ((RoomDefinition room, RoomStateSettings state) in EngineLayouts.States())
        {
            if (rooms.Contains(room))
                holding.Add((room, state));
        }

        return holding;
    }

    /// <summary>
    /// Gives the layouts of a room's states a patch for each frame of one of the old game's timed changes to its
    /// blocks, and returns the change as the Godot game plays it: each patch in turn, standing as long as its frame
    /// did, with the sound its frame asked for.
    /// </summary>
    /// <param name="states">The states of the room.</param>
    /// <param name="name">The name the patches are numbered under.</param>
    /// <param name="block">The block the change is made at.</param>
    /// <param name="change">The old game's change.</param>
    /// <param name="directory">The installed directory that holds its frames.</param>
    /// <param name="drawn">The installed frame, and the kinds of block it makes, of what one of its frames draws.</param>
    /// <param name="leaves">The name of the patch its last frame is, where the Godot project asks for it by a name of its own.</param>
    private JsonObject TerrainChangeAt(
        IReadOnlyList<(RoomDefinition Room, RoomStateSettings State)> states, string name, Position16<RoomBlocks> block, TerrainChange change,
        string directory, Func<RoomObjectDraw, (string Frame, RoomBlockShape Shape)> drawn, string? leaves = null) =>
        TerrainChangeAt(states, name, block, change, draw => (directory, drawn(draw).Frame, drawn(draw).Shape), leaves);

    /// <summary>
    /// As the other <c>TerrainChangeAt</c>, for a change whose frames are not all kept in one installed directory:
    /// <paramref name="drawn"/> says, for what a frame draws, the directory as well.
    /// </summary>
    private JsonObject TerrainChangeAt(
        IReadOnlyList<(RoomDefinition Room, RoomStateSettings State)> states, string name, Position16<RoomBlocks> block, TerrainChange change,
        Func<RoomObjectDraw, (string Directory, string Frame, RoomBlockShape Shape)> drawn, string? leaves = null)
    {
        if (change.Frames.Count == 0)
            throw new InvalidDataException($"The change {name} to a room's blocks draws nothing.");
        var steps = new JsonArray();
        Position16<RoomBlocks> at = block;
        for (int index = 0; index < change.Frames.Count; index++)
        {
            TerrainChangeFrame frame = change.Frames[index];
            at += frame.MoveBefore;
            bool last = index == change.Frames.Count - 1;
            string patch = last && leaves is not null ? leaves : $"{name}_{index + 1}";
            (string directory, string picture, RoomBlockShape shape) = drawn(frame.Draw);
            foreach ((RoomDefinition room, RoomStateSettings state) in states)
                Layouts.Patch(room, state, patch, at, Layouts.PatchCells(directory, (picture, shape)));
            var step = new JsonObject { ["patch"] = patch };
            if (!last)
                step["updates"] = frame.Updates;
            if (frame.SoundBefore is { } sound)
                step["sound"] = EngineNames.SoundOrSilence(sound);
            steps.Add(step);
        }

        return new JsonObject { ["delay"] = change.FirstFrameDelay, ["steps"] = steps };
    }
}
