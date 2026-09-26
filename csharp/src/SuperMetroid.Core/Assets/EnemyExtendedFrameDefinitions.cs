using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One named, fixed extended-frame identity; collision data is not editable art.</summary>
internal readonly record struct EnemyExtendedFrameDefinition(
    byte Bank, ushort Pointer, string Name);

/// <summary>
/// Named visual identities for Pirate and Ridley composite frames. The Pirate
/// selectors come from compiled instruction catalogs; component
/// hitbox pointers remain gameplay-owned and are absent from the asset.
/// </summary>
internal static class EnemyExtendedFrameDefinitions
{
    internal const int FirstVersion = 1;
    internal const int PreviousVersion = 2;
    internal const int PreDisplayBindingsVersion = 3;
    internal const int PirateDisplayBindingsVersion = 4;
    internal const int Version = 5;
    internal const string FileName = "enemy-walking-pirate-compositions.json";
    internal const byte Bank = 0xb2;
    internal const int MaximumComponents = 8;
    internal const int WalkingFrameCount = 37;
    internal const int WallFrameCount = 18;
    internal const int NinjaFrameCount = 76;
    internal const int RidleyFrameCount = 11;
    internal const int PirateFrameCount =
        WalkingFrameCount + WallFrameCount + NinjaFrameCount;
    internal const int ExpectedFrameCount =
        PirateFrameCount + RidleyFrameCount;

    // Distinct extended body frames selected by Ridley's bank-$A6 instruction
    // programs at $A6:E53E-$E824. These physical identities cover the Ceres
    // and Lower Norfair body poses; collision pointers stay in engine data.
    private static readonly ushort[] RidleyBodyPointers =
    [
        0xe983, 0xe9a5, 0xe9c7, 0xe9e9, 0xea0b, 0xea2d,
        0xea4f, 0xea71, 0xea93, 0xeab5, 0xead7,
    ];

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
            frames.Count != WalkingFrameCount + WallFrameCount)
            throw new InvalidDataException(
                $"Wall Pirate has {wallCount} distinct frames; expected {WallFrameCount}.");
        int ninjaCount = 0;
        for (int index = 0;
             index < NinjaSpacePirateInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = NinjaSpacePirateInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(Bank, operand, out ushort pointer))
                throw new InvalidDataException(
                    $"Ninja Pirate frame selector $B2:{operand:X4} is not compiled.");
            if (!seen.Add(pointer))
                continue;
            // Ninja programs reuse many selected frames in different attacks.
            // The bank-local frame identity is a stable, unambiguous author key
            // without assigning a possibly misleading attack-specific name.
            frames.Add(new EnemyExtendedFrameDefinition(Bank, pointer,
                $"ninja_pirate_{pointer:X4}"));
            ninjaCount++;
        }
        if (ninjaCount != NinjaFrameCount || frames.Count != PirateFrameCount)
            throw new InvalidDataException(
                $"Ninja Pirate has {ninjaCount} distinct frames; expected {NinjaFrameCount}.");
        if (RidleyBodyPointers.Length != RidleyFrameCount)
            throw new InvalidDataException("Ridley extended-body frame count changed.");
        foreach (ushort pointer in RidleyBodyPointers)
            frames.Add(new EnemyExtendedFrameDefinition(0xa6, pointer,
                $"ridley_body_{pointer:X4}"));
        return frames.ToArray();
    }
}
