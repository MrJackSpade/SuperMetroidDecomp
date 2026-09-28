using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One named, fixed extended-frame identity; collision data is not editable art.</summary>
internal readonly record struct EnemyExtendedFrameDefinition(
    byte Bank, ushort Pointer, string Name);

/// <summary>
/// Named visual identities for Pirate, Ridley, Draygon, Spore Spawn, Ceres steam,
/// Maridia's Oum snail, Crocomire's tongue and fight-body composite frames,
/// Bomb Torizo's dormant statue frame, Golden Torizo's initial, awakening and
/// walking, turning and jump-back poses, and Kraid's independently animated arm.
/// Their selectors come from compiled instruction catalogs;
/// component hitbox pointers remain gameplay-owned and absent from the asset.
/// </summary>
internal static class EnemyExtendedFrameDefinitions
{
    internal const int FirstVersion = 1;
    internal const int PreviousVersion = 2;
    internal const int PreDisplayBindingsVersion = 3;
    internal const int PirateDisplayBindingsVersion = 4;
    internal const int PreDraygonVersion = 5;
    internal const int PreDraygonFrameCount = 142;
    internal const int PreSporeIdentityVersion = 6;
    internal const int PreCeresSteamVersion = 7;
    internal const int PreCeresSteamFrameCount = 202;
    internal const int PreOumVersion = 8;
    internal const int PreOumFrameCount = 230;
    internal const int PreCrocomireVersion = 9;
    internal const int PreCrocomireFrameCount = 260;
    internal const int PreCrocomireBodyVersion = 10;
    internal const int PreCrocomireBodyFrameCount = 269;
    internal const int PreBombTorizoVersion = 11;
    internal const int PreBombTorizoFrameCount = 319;
    internal const int PreGoldenTorizoVersion = 12;
    internal const int PreGoldenTorizoFrameCount = 320;
    internal const int PreKraidArmVersion = 13;
    internal const int PreKraidArmFrameCount = 321;
    internal const int PreGoldenTorizoAwakeningVersion = 14;
    internal const int PreGoldenTorizoAwakeningFrameCount = 343;
    internal const int PreGoldenTorizoWalkingVersion = 15;
    internal const int PreGoldenTorizoWalkingFrameCount = 349;
    internal const int PreGoldenTorizoRightwardVersion = 16;
    internal const int PreGoldenTorizoRightwardFrameCount = 359;
    internal const int PreTorizoJumpBackVersion = 17;
    internal const int PreTorizoJumpBackFrameCount = 370;
    internal const int PreGoldenTorizoRightOrbVersion = 18;
    internal const int PreGoldenTorizoRightOrbFrameCount = 373;
    internal const int PreGoldenTorizoRightSonicVersion = 19;
    internal const int PreGoldenTorizoRightSonicFrameCount = 379;
    internal const int Version = 20;
    internal const string FileName = "enemy-walking-pirate-compositions.json";
    internal const byte Bank = 0xb2;
    internal const int MaximumComponents = 8;
    internal const int WalkingFrameCount = 37;
    internal const int WallFrameCount = 18;
    internal const int NinjaFrameCount = 76;
    internal const int RidleyFrameCount = 11;
    internal const int DraygonOamFrameCount = 48;
    internal const int SporeSpawnOamFrameCount = 12;
    internal const int CeresSteamFrameCount = 28;
    internal const int OumFrameCount = 30;
    internal const int CrocomireOamFrameCount = 9;
    internal const int CrocomireBodyFrameCount = CrocomireBodyVisualDefinitions.BodyFrameCount;
    internal const int BombTorizoDormantFrameCount = 1;
    internal const int GoldenTorizoInitialFrameCount = 1;
    internal const int KraidArmFrameCount = 22;
    internal const int GoldenTorizoAwakeningFrameCount = 6;
    internal const int GoldenTorizoWalkingFrameCount = 10;
    internal const int GoldenTorizoRightwardFrameCount = 11;
    internal const int TorizoJumpBackFrameCount = 3;
    internal const int GoldenTorizoRightOrbFrameCount = 6;
    internal const int GoldenTorizoRightSonicFrameCount = 21;
    internal const int PirateFrameCount =
        WalkingFrameCount + WallFrameCount + NinjaFrameCount;
    internal const int ExpectedFrameCount =
        PirateFrameCount + RidleyFrameCount +
        DraygonOamFrameCount + SporeSpawnOamFrameCount +
        CeresSteamFrameCount + OumFrameCount + CrocomireOamFrameCount +
        CrocomireBodyFrameCount + BombTorizoDormantFrameCount +
        GoldenTorizoInitialFrameCount + KraidArmFrameCount +
        GoldenTorizoAwakeningFrameCount + GoldenTorizoWalkingFrameCount +
        GoldenTorizoRightwardFrameCount + TorizoJumpBackFrameCount +
        GoldenTorizoRightOrbFrameCount + GoldenTorizoRightSonicFrameCount;

    // Every bank-$A5 Draygon extended frame selected by a compiled instruction
    // that contains ordinary OAM components. The other 34 selected frames carry
    // BG2 tilemap commands and require a separate presentation catalog.
    private static readonly ushort[] DraygonOamPointers =
    [
        0xa2df, 0xa2e9, 0xa2f3, 0xa2fd, 0xa307, 0xa311,
        0xa3c5, 0xa3cf, 0xa3d9, 0xa3e3,
        0xa40b, 0xa41d, 0xa42f, 0xa441, 0xa453, 0xa465, 0xa477, 0xa489,
        0xa4a3, 0xa4c5, 0xa4ef, 0xa521, 0xa55b, 0xa59d,
        0xa607, 0xa611, 0xa61b, 0xa625, 0xa62f, 0xa639,
        0xa6ed, 0xa6f7, 0xa701, 0xa70b,
        0xa779, 0xa78b, 0xa79d, 0xa7af, 0xa7c1, 0xa7d3, 0xa7e5, 0xa7f7,
        0xa811, 0xa833, 0xa85d, 0xa88f, 0xa8c9, 0xa90b,
    ];

    // These twelve bank-$A5 roots are selected by Spore Spawn's instruction
    // programs, not Draygon. Version 6 accidentally published Draygon-prefixed
    // author keys for them; the loader migrates those keys by physical pointer.
    private static readonly ushort[] SporeSpawnOamPointers =
    [
        0xee65, 0xee6f, 0xee79, 0xee8b, 0xee9d, 0xeeaf, 0xeec1,
        0xeed3, 0xeee5, 0xef3d, 0xef4f, 0xef61,
    ];

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
        if (frames.Count != PreDraygonFrameCount ||
            DraygonOamPointers.Length != DraygonOamFrameCount ||
            SporeSpawnOamPointers.Length != SporeSpawnOamFrameCount)
            throw new InvalidDataException(
                "Draygon/Spore Spawn OAM frame catalog prefix changed.");
        foreach (ushort pointer in DraygonOamPointers)
            frames.Add(new EnemyExtendedFrameDefinition(0xa5, pointer,
                $"draygon_oam_{pointer:X4}"));
        foreach (ushort pointer in SporeSpawnOamPointers)
            frames.Add(new EnemyExtendedFrameDefinition(0xa5, pointer,
                $"spore_spawn_oam_{pointer:X4}"));
        if (frames.Count != PreCeresSteamFrameCount ||
            CeresSteamCollisionDefinitions.FramePointers.Length != CeresSteamFrameCount)
            throw new InvalidDataException("Ceres steam extended-frame prefix changed.");
        foreach (ushort pointer in CeresSteamCollisionDefinitions.FramePointers)
            frames.Add(new EnemyExtendedFrameDefinition(CeresSteamCollisionDefinitions.Bank,
                pointer, $"ceres_steam_oam_{pointer:X4}"));
        if (frames.Count != PreOumFrameCount ||
            MaridiaLargeSnailCollisionDefinitions.FramePointers.Length != OumFrameCount)
            throw new InvalidDataException("Oum extended-frame prefix changed.");
        foreach (ushort pointer in MaridiaLargeSnailCollisionDefinitions.FramePointers)
            frames.Add(new EnemyExtendedFrameDefinition(
                MaridiaLargeSnailCollisionDefinitions.Bank,
                pointer, $"oum_oam_{pointer:X4}"));
        if (frames.Count != PreCrocomireFrameCount ||
            CrocomireTongueCollisionDefinitions.FramePointers.Length !=
                CrocomireOamFrameCount)
            throw new InvalidDataException("Crocomire OAM frame prefix changed.");
        foreach (ushort pointer in CrocomireTongueCollisionDefinitions.FramePointers)
            frames.Add(new EnemyExtendedFrameDefinition(
                CrocomireTongueCollisionDefinitions.Bank,
                pointer, $"crocomire_oam_{pointer:X4}"));
        if (frames.Count != PreCrocomireBodyFrameCount)
            throw new InvalidDataException("Crocomire body frame prefix changed.");
        foreach (ushort pointer in CrocomireBodyVisualDefinitions.Frames)
            frames.Add(new EnemyExtendedFrameDefinition(
                CrocomireBodyVisualDefinitions.Bank,
                pointer, $"crocomire_body_oam_{pointer:X4}"));
        if (frames.Count != PreBombTorizoFrameCount)
            throw new InvalidDataException("Bomb Torizo extended-frame prefix changed.");
        frames.Add(new EnemyExtendedFrameDefinition(0xaa, 0x87d0,
            "bomb_torizo_dormant"));
        if (frames.Count != PreGoldenTorizoFrameCount)
            throw new InvalidDataException("Golden Torizo extended-frame prefix changed.");
        frames.Add(new EnemyExtendedFrameDefinition(0xaa, 0xaa30,
            "golden_torizo_initial"));
        if (frames.Count != PreKraidArmFrameCount)
            throw new InvalidDataException("Kraid arm extended-frame prefix changed.");
        var kraidArmPointers = new HashSet<ushort>();
        for (int index = 0;
             index < KraidArmInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = KraidArmInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(0xa7, operand,
                    out ushort pointer))
                throw new InvalidDataException(
                    $"Kraid arm visual operand $A7:{operand:X4} is not compiled.");
            if (kraidArmPointers.Add(pointer))
                frames.Add(new EnemyExtendedFrameDefinition(0xa7, pointer,
                    $"kraid_arm_oam_{pointer:X4}"));
        }
        if (kraidArmPointers.Count != KraidArmFrameCount)
            throw new InvalidDataException(
                $"Kraid arm selects {kraidArmPointers.Count} distinct visual frames, " +
                $"expected {KraidArmFrameCount}.");
        if (frames.Count != PreGoldenTorizoAwakeningFrameCount)
            throw new InvalidDataException("Golden Torizo awakening extended-frame prefix changed.");
        var goldenAwakeningPointers = new HashSet<ushort> { 0xaa30 };
        for (int index = 0;
             index < GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = GoldenTorizoAwakeningInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(0xaa, operand,
                    out ushort pointer))
                throw new InvalidDataException(
                    $"Golden Torizo awakening visual operand $AA:{operand:X4} is not compiled.");
            if (goldenAwakeningPointers.Add(pointer))
                frames.Add(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"golden_torizo_awake_{pointer:X4}"));
        }
        if (goldenAwakeningPointers.Count - 1 != GoldenTorizoAwakeningFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo awakening selects {goldenAwakeningPointers.Count - 1} " +
                $"new visual frames, expected {GoldenTorizoAwakeningFrameCount}.");
        if (frames.Count != PreGoldenTorizoWalkingFrameCount)
            throw new InvalidDataException("Golden Torizo walking extended-frame prefix changed.");
        var goldenWalkingPointers = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoWalkingInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = GoldenTorizoWalkingInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(0xaa, operand,
                    out ushort pointer))
                throw new InvalidDataException(
                    $"Golden Torizo walking visual operand $AA:{operand:X4} is not compiled.");
            if (goldenWalkingPointers.Add(pointer))
                frames.Add(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"golden_torizo_walk_left_{pointer:X4}"));
        }
        if (goldenWalkingPointers.Count != GoldenTorizoWalkingFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo walking selects {goldenWalkingPointers.Count} " +
                $"distinct visual frames, expected {GoldenTorizoWalkingFrameCount}.");
        if (frames.Count != PreGoldenTorizoRightwardFrameCount)
            throw new InvalidDataException("Golden Torizo rightward extended-frame prefix changed.");
        var goldenRightwardPointers = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoRightwardInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = GoldenTorizoRightwardInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(0xaa, operand,
                    out ushort pointer))
                throw new InvalidDataException(
                    $"Golden Torizo rightward visual operand $AA:{operand:X4} is not compiled.");
            if (goldenRightwardPointers.Add(pointer))
                frames.Add(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"golden_torizo_rightward_{pointer:X4}"));
        }
        if (goldenRightwardPointers.Count != GoldenTorizoRightwardFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo rightward selects {goldenRightwardPointers.Count} " +
                $"distinct visual frames, expected {GoldenTorizoRightwardFrameCount}.");
        if (frames.Count != PreTorizoJumpBackFrameCount)
            throw new InvalidDataException("Torizo jump-back extended-frame prefix changed.");
        var jumpBackPointers = new HashSet<ushort>();
        for (int index = 0;
             index < TorizoJumpBackInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = TorizoJumpBackInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(0xaa, operand,
                    out ushort pointer))
                throw new InvalidDataException(
                    $"Torizo jump-back visual operand $AA:{operand:X4} is not compiled.");
            if (jumpBackPointers.Add(pointer))
                frames.Add(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"torizo_jump_back_{pointer:X4}"));
        }
        if (jumpBackPointers.Count != TorizoJumpBackFrameCount)
            throw new InvalidDataException(
                $"Torizo jump-back selects {jumpBackPointers.Count} " +
                $"distinct visual frames, expected {TorizoJumpBackFrameCount}.");
        if (frames.Count != PreGoldenTorizoRightOrbFrameCount)
            throw new InvalidDataException("Golden Torizo right-orb frame prefix changed.");
        var rightOrbPointers = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoRightOrbInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = GoldenTorizoRightOrbInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(0xaa, operand,
                    out ushort pointer))
                throw new InvalidDataException(
                    $"Golden Torizo right-orb visual operand $AA:{operand:X4} is not compiled.");
            if (rightOrbPointers.Add(pointer))
                frames.Add(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"golden_torizo_right_orb_{pointer:X4}"));
        }
        if (rightOrbPointers.Count != GoldenTorizoRightOrbFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo right-orb selects {rightOrbPointers.Count} " +
                $"distinct visual frames, expected {GoldenTorizoRightOrbFrameCount}.");
        if (frames.Count != PreGoldenTorizoRightSonicFrameCount)
            throw new InvalidDataException("Golden Torizo right-sonic frame prefix changed.");
        // AC88 is already the orb attack's editable frame; the sonic list
        // intentionally reuses that identity instead of adding a duplicate.
        var rightSonicPointers = new HashSet<ushort> { 0xac88 };
        for (int index = 0;
             index < GoldenTorizoRightSonicInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = GoldenTorizoRightSonicInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(0xaa, operand,
                    out ushort pointer))
                throw new InvalidDataException(
                    $"Golden Torizo right-sonic visual operand $AA:{operand:X4} is not compiled.");
            if (rightSonicPointers.Add(pointer))
                frames.Add(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"golden_torizo_right_sonic_{pointer:X4}"));
        }
        if (rightSonicPointers.Count - 1 != GoldenTorizoRightSonicFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo right-sonic selects {rightSonicPointers.Count - 1} " +
                $"new visual frames, expected {GoldenTorizoRightSonicFrameCount}.");
        return frames.ToArray();
    }
}
