using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One named, fixed extended-frame identity; collision data is not editable art.</summary>
internal readonly record struct EnemyExtendedFrameDefinition(
    byte Bank, ushort Pointer, string Name);

/// <summary>
/// Named visual identities for ordinary walking and wall Space Pirate composite
/// frames. The selectors come from compiled instruction catalogs; component
/// hitbox pointers remain gameplay-owned and are absent from the asset.
/// </summary>
internal static class EnemyExtendedFrameDefinitions
{
    internal const int PreviousVersion = 1;
    internal const int Version = 2;
    internal const string FileName = "enemy-walking-pirate-compositions.json";
    internal const byte Bank = 0xb2;
    internal const int MaximumComponents = 8;
    internal const int WalkingFrameCount = 37;
    internal const int WallFrameCount = 18;
    internal const int ExpectedFrameCount = WalkingFrameCount + WallFrameCount;

    private static readonly string[] WalkingNames =
    [
        "walking_pirate_flinch_left", "walking_pirate_flinch_right",
        "walking_pirate_walk_left_0", "walking_pirate_walk_left_1",
        "walking_pirate_walk_left_2", "walking_pirate_walk_left_3",
        "walking_pirate_walk_left_4", "walking_pirate_walk_left_5",
        "walking_pirate_walk_left_6", "walking_pirate_walk_left_7",
        "walking_pirate_fire_left_0", "walking_pirate_fire_left_1",
        "walking_pirate_fire_left_2", "walking_pirate_fire_left_3",
        "walking_pirate_fire_left_4", "walking_pirate_fire_left_5",
        "walking_pirate_look_left_0", "walking_pirate_look_left_1",
        "walking_pirate_look_left_2", "walking_pirate_look_shared",
        "walking_pirate_walk_right_0", "walking_pirate_walk_right_1",
        "walking_pirate_walk_right_2", "walking_pirate_walk_right_3",
        "walking_pirate_walk_right_4", "walking_pirate_walk_right_5",
        "walking_pirate_walk_right_6", "walking_pirate_walk_right_7",
        "walking_pirate_fire_right_0", "walking_pirate_fire_right_1",
        "walking_pirate_fire_right_2", "walking_pirate_fire_right_3",
        "walking_pirate_fire_right_4", "walking_pirate_fire_right_5",
        "walking_pirate_look_right_0", "walking_pirate_look_right_1",
        "walking_pirate_look_right_2",
    ];

    // First-seen order in the eight wall-Pirate instruction programs. The
    // renderer's frame identity is the compiled selector's bank-local pointer;
    // names are only stable keys for presentation overrides.
    private static readonly string[] WallNames =
    [
        "wall_pirate_fire_jump_left_0", "wall_pirate_fire_jump_left_1",
        "wall_pirate_fire_jump_left_2", "wall_pirate_fire_jump_left_3",
        "wall_pirate_climb_left_0", "wall_pirate_climb_left_1",
        "wall_pirate_climb_left_2", "wall_pirate_climb_left_3",
        "wall_pirate_climb_left_4", "wall_pirate_fire_jump_right_0",
        "wall_pirate_fire_jump_right_1", "wall_pirate_fire_jump_right_2",
        "wall_pirate_fire_jump_right_3", "wall_pirate_climb_right_0",
        "wall_pirate_climb_right_1", "wall_pirate_climb_right_2",
        "wall_pirate_climb_right_3", "wall_pirate_climb_right_4",
    ];

    private static readonly EnemyExtendedFrameDefinition[] FrameDefinitions = Build();

    internal static ReadOnlySpan<EnemyExtendedFrameDefinition> Frames => FrameDefinitions;

    private static EnemyExtendedFrameDefinition[] Build()
    {
        var seen = new HashSet<ushort>();
        var frames = new List<EnemyExtendedFrameDefinition>(ExpectedFrameCount);
        for (int index = 0;
             index < WalkingSpacePirateInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = WalkingSpacePirateInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(Bank, operand, out ushort pointer))
                throw new InvalidDataException(
                    $"Walking Pirate frame selector $B2:{operand:X4} is not compiled.");
            if (!seen.Add(pointer))
                continue;
            if (frames.Count == WalkingNames.Length)
                throw new InvalidDataException("Walking Pirate has more frames than names.");
            frames.Add(new EnemyExtendedFrameDefinition(Bank, pointer,
                WalkingNames[frames.Count]));
        }
        if (frames.Count != WalkingFrameCount ||
            WalkingNames.Length != WalkingFrameCount)
            throw new InvalidDataException(
                $"Walking Pirate has {frames.Count} distinct frames; expected {WalkingFrameCount}.");
        int wallCount = 0;
        for (int index = 0;
             index < WallSpacePirateInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = WallSpacePirateInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(Bank, operand, out ushort pointer))
                throw new InvalidDataException(
                    $"Wall Pirate frame selector $B2:{operand:X4} is not compiled.");
            if (!seen.Add(pointer))
                continue;
            if (wallCount == WallNames.Length)
                throw new InvalidDataException("Wall Pirate has more frames than names.");
            frames.Add(new EnemyExtendedFrameDefinition(Bank, pointer,
                WallNames[wallCount++]));
        }
        if (wallCount != WallFrameCount || WallNames.Length != WallFrameCount ||
            frames.Count != ExpectedFrameCount)
            throw new InvalidDataException(
                $"Wall Pirate has {wallCount} distinct frames; expected {WallFrameCount}.");
        return frames.ToArray();
    }
}
