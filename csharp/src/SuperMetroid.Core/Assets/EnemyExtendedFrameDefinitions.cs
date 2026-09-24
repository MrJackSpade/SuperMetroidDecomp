using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One named, fixed extended-frame identity; collision data is not editable art.</summary>
internal readonly record struct EnemyExtendedFrameDefinition(
    byte Bank, ushort Pointer, string Name);

/// <summary>
/// Named visual identities for walking Space Pirate composite frames. The frame
/// selectors come from the compiled instruction catalog; native component hitbox
/// pointers remain gameplay-owned and are intentionally absent from the asset.
/// </summary>
internal static class EnemyExtendedFrameDefinitions
{
    internal const int Version = 1;
    internal const string FileName = "enemy-walking-pirate-compositions.json";
    internal const byte Bank = 0xb2;
    internal const int MaximumComponents = 8;
    internal const int ExpectedFrameCount = 37;

    private static readonly string[] Names =
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
            if (frames.Count == Names.Length)
                throw new InvalidDataException("Walking Pirate has more frames than names.");
            frames.Add(new EnemyExtendedFrameDefinition(Bank, pointer,
                Names[frames.Count]));
        }
        if (frames.Count != ExpectedFrameCount || Names.Length != ExpectedFrameCount)
            throw new InvalidDataException(
                $"Walking Pirate has {frames.Count} distinct frames; expected {ExpectedFrameCount}.");
        return frames.ToArray();
    }
}
