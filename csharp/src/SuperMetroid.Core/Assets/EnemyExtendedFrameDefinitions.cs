using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One named, fixed extended-frame identity; collision data is not editable art.</summary>
/// <param name="Bank">ROM bank containing the visual frame data.</param>
/// <param name="Pointer">Bank-local pointer used to identify the frame.</param>
/// <param name="Name">Stable author-facing key used by the extended-frame asset.</param>
internal readonly record struct EnemyExtendedFrameDefinition(
    byte Bank, ushort Pointer, string Name);

/// <summary>
/// Named visual identities for Pirate, Ridley, Draygon, Spore Spawn, Ceres steam,
/// Maridia's Oum snail, Crocomire's tongue, fight-body and skeleton composite frames,
/// Bomb Torizo's dormant statue frame, Golden Torizo's initial, awakening and
/// walking, turning and jump-back poses, Kraid's independently animated arm and foot,
/// Mother Brain's mixed body poses, and Phantoon/Draygon BG2-only poses.
/// Their selectors come from compiled instruction catalogs;
/// component hitbox pointers remain gameplay-owned and absent from the asset.
/// </summary>
internal static class EnemyExtendedFrameDefinitions
{
    /// <summary>Schema version used before this catalog's later additions.</summary>
    internal const int FirstVersion = 1;
    /// <summary>Schema version immediately preceding the display-binding layout.</summary>
    internal const int PreviousVersion = 2;
    /// <summary>Schema version before shared bindings were added for Pirate artwork.</summary>
    internal const int PreDisplayBindingsVersion = 3;
    /// <summary>Schema version before Pirate frame display bindings were introduced.</summary>
    internal const int PirateDisplayBindingsVersion = 4;
    /// <summary>Schema version before Draygon and Spore Spawn OAM frames were cataloged.</summary>
    internal const int PreDraygonVersion = 5;
    /// <summary>Expected catalog length before the Draygon and Spore Spawn additions.</summary>
    internal const int PreDraygonFrameCount = 142;
    /// <summary>Schema version before Spore Spawn received distinct frame identities.</summary>
    internal const int PreSporeIdentityVersion = 6;
    /// <summary>Schema version before Ceres steam frames were included.</summary>
    internal const int PreCeresSteamVersion = 7;
    /// <summary>Expected catalog length before Ceres steam frames were appended.</summary>
    internal const int PreCeresSteamFrameCount = 202;
    /// <summary>Schema version before Oum's extended frames were included.</summary>
    internal const int PreOumVersion = 8;
    /// <summary>Expected catalog length before Oum frames were appended.</summary>
    internal const int PreOumFrameCount = 230;
    /// <summary>Schema version before Crocomire tongue frames were included.</summary>
    internal const int PreCrocomireVersion = 9;
    /// <summary>Expected catalog length before Crocomire tongue frames were appended.</summary>
    internal const int PreCrocomireFrameCount = 260;
    /// <summary>Schema version before Crocomire's body artwork was included.</summary>
    internal const int PreCrocomireBodyVersion = 10;
    /// <summary>Expected catalog length before Crocomire body frames were appended.</summary>
    internal const int PreCrocomireBodyFrameCount = 269;
    /// <summary>Schema version before Bomb Torizo's dormant statue pose was included.</summary>
    internal const int PreBombTorizoVersion = 11;
    /// <summary>Expected catalog length before Bomb Torizo's dormant pose was appended.</summary>
    internal const int PreBombTorizoFrameCount = 319;
    /// <summary>Schema version before Golden Torizo's initial pose was included.</summary>
    internal const int PreGoldenTorizoVersion = 12;
    /// <summary>Expected catalog length before Golden Torizo's initial pose was appended.</summary>
    internal const int PreGoldenTorizoFrameCount = 320;
    /// <summary>Schema version before Kraid's independently animated arm frames were included.</summary>
    internal const int PreKraidArmVersion = 13;
    /// <summary>Expected catalog length before Kraid arm frames were appended.</summary>
    internal const int PreKraidArmFrameCount = 321;
    /// <summary>Schema version before Golden Torizo's awakening poses were included.</summary>
    internal const int PreGoldenTorizoAwakeningVersion = 14;
    /// <summary>Expected catalog length before awakening poses were appended.</summary>
    internal const int PreGoldenTorizoAwakeningFrameCount = 343;
    /// <summary>Schema version before Golden Torizo's walking poses were included.</summary>
    internal const int PreGoldenTorizoWalkingVersion = 15;
    /// <summary>Expected catalog length before walking poses were appended.</summary>
    internal const int PreGoldenTorizoWalkingFrameCount = 349;
    /// <summary>Schema version before Golden Torizo's right-facing poses were included.</summary>
    internal const int PreGoldenTorizoRightwardVersion = 16;
    /// <summary>Expected catalog length before right-facing poses were appended.</summary>
    internal const int PreGoldenTorizoRightwardFrameCount = 359;
    /// <summary>Schema version before Torizo jump-back poses were included.</summary>
    internal const int PreTorizoJumpBackVersion = 17;
    /// <summary>Expected catalog length before jump-back poses were appended.</summary>
    internal const int PreTorizoJumpBackFrameCount = 370;
    /// <summary>Schema version before Golden Torizo's right-orb poses were included.</summary>
    internal const int PreGoldenTorizoRightOrbVersion = 18;
    /// <summary>Expected catalog length before right-orb poses were appended.</summary>
    internal const int PreGoldenTorizoRightOrbFrameCount = 373;
    /// <summary>Schema version before Golden Torizo's right-sonic poses were included.</summary>
    internal const int PreGoldenTorizoRightSonicVersion = 19;
    /// <summary>Expected catalog length before right-sonic poses were appended.</summary>
    internal const int PreGoldenTorizoRightSonicFrameCount = 379;
    /// <summary>Schema version before Torizo's shared falling-left pose was included.</summary>
    internal const int PreTorizoFallingLeftVersion = 20;
    /// <summary>Expected catalog length before the falling-left pose was appended.</summary>
    internal const int PreTorizoFallingLeftFrameCount = 400;
    /// <summary>Schema version before Golden Torizo's left-foot orb poses were included.</summary>
    internal const int PreGoldenTorizoLeftFootOrbVersion = 21;
    /// <summary>Expected catalog length before left-foot orb poses were appended.</summary>
    internal const int PreGoldenTorizoLeftFootOrbFrameCount = 401;
    /// <summary>Schema version before Torizo's left-facing jump-back poses were included.</summary>
    internal const int PreTorizoJumpBackLeftVersion = 22;
    /// <summary>Expected catalog length before left-facing jump-back poses were appended.</summary>
    internal const int PreTorizoJumpBackLeftFrameCount = 406;
    /// <summary>Schema version before Golden Torizo's left-orb poses were included.</summary>
    internal const int PreGoldenTorizoLeftOrbVersion = 23;
    /// <summary>Expected catalog length before left-orb poses were appended.</summary>
    internal const int PreGoldenTorizoLeftOrbFrameCount = 408;
    /// <summary>Schema version before the complete Torizo combat artwork was included.</summary>
    internal const int PreCompleteTorizoVersion = 24;
    /// <summary>Expected catalog length before the remaining Torizo combat frames were appended.</summary>
    internal const int PreCompleteTorizoFrameCount = 420;
    /// <summary>Number of new Torizo combat frames appended after the preceding schema prefix.</summary>
    internal const int CompleteTorizoAdditionalFrameCount = 27;
    /// <summary>Schema version before Mother Brain body poses were included.</summary>
    internal const int PreMotherBrainBodyVersion = 25;
    /// <summary>Expected catalog length before Mother Brain body poses were appended.</summary>
    internal const int PreMotherBrainBodyFrameCount = 447;
    /// <summary>Schema 26 predates shared display bindings for BG2-only boss roots.</summary>
    internal const int PreBg2BossBindingsVersion = 26;
    /// <summary>Expected catalog length before Phantoon and Draygon BG2-only roots were added.</summary>
    internal const int PreBg2BossBindingsFrameCount = 464;
    /// <summary>Schema 27 predates Crocomire's thirty-three corpse/skeleton OAM roots.</summary>
    internal const int PreCrocomireSkeletonVersion = 27;
    /// <summary>Expected catalog length before Crocomire corpse and skeleton roots were added.</summary>
    internal const int PreCrocomireSkeletonFrameCount = 520;
    /// <summary>Schema 28 predates Kraid's thirty-five extended foot roots.</summary>
    internal const int PreKraidFootVersion = 28;
    /// <summary>Expected catalog length before Kraid's extended foot roots were added.</summary>
    internal const int PreKraidFootFrameCount = 553;
    /// <summary>Current schema version written for this extended-frame catalog.</summary>
    internal const int Version = 29;
    /// <summary>Asset filename used to serialize these named frame definitions.</summary>
    internal const string FileName = "enemy-walking-pirate-compositions.json";
    /// <summary>ROM bank containing the Space Pirate visual instruction selections.</summary>
    internal const byte Bank = 0xb2;
    /// <summary>Default upper bound for components in an editable OAM composition.</summary>
    internal const int MaximumComponents = 8;
    /// <summary>Number of distinct frames selected by the walking Pirate instruction list.</summary>
    internal const int WalkingFrameCount = 37;
    /// <summary>Number of distinct frames selected by the wall-climbing Pirate list.</summary>
    internal const int WallFrameCount = 18;
    /// <summary>Number of distinct frames selected by the ninja Pirate instruction list.</summary>
    internal const int NinjaFrameCount = 76;
    /// <summary>Number of Ridley body OAM frames included in this catalog.</summary>
    internal const int RidleyFrameCount = 11;
    /// <summary>Number of Draygon OAM frames included in this catalog.</summary>
    internal const int DraygonOamFrameCount = 48;
    /// <summary>Number of Draygon BG2-only poses included in this catalog.</summary>
    internal const int DraygonBg2FrameCount = 34;
    /// <summary>Number of Phantoon BG2-only poses included in this catalog.</summary>
    internal const int PhantoonBg2FrameCount = 22;
    /// <summary>Number of Spore Spawn OAM frames included in this catalog.</summary>
    internal const int SporeSpawnOamFrameCount = 12;
    /// <summary>Number of Ceres steam OAM frames included in this catalog.</summary>
    internal const int CeresSteamFrameCount = 28;
    /// <summary>Number of Oum large-snail OAM frames included in this catalog.</summary>
    internal const int OumFrameCount = 30;
    /// <summary>Number of Crocomire tongue OAM frames included in this catalog.</summary>
    internal const int CrocomireOamFrameCount = 9;
    /// <summary>Number of Crocomire body frames supplied by its visual definitions.</summary>
    internal const int CrocomireBodyFrameCount = CrocomireBodyVisualDefinitions.BodyFrameCount;
    /// <summary>Number of Bomb Torizo dormant statue frames included in this catalog.</summary>
    internal const int BombTorizoDormantFrameCount = 1;
    /// <summary>Number of Golden Torizo initial poses included in this catalog.</summary>
    internal const int GoldenTorizoInitialFrameCount = 1;
    /// <summary>Number of distinct frames selected for Kraid's arm.</summary>
    internal const int KraidArmFrameCount = 22;
    /// <summary>Number of new Golden Torizo awakening poses in the instruction list.</summary>
    internal const int GoldenTorizoAwakeningFrameCount = 6;
    /// <summary>Number of distinct Golden Torizo walking poses.</summary>
    internal const int GoldenTorizoWalkingFrameCount = 10;
    /// <summary>Number of distinct Golden Torizo rightward poses.</summary>
    internal const int GoldenTorizoRightwardFrameCount = 11;
    /// <summary>Number of distinct Torizo jump-back poses.</summary>
    internal const int TorizoJumpBackFrameCount = 3;
    /// <summary>Number of distinct Golden Torizo right-orb poses.</summary>
    internal const int GoldenTorizoRightOrbFrameCount = 6;
    /// <summary>Number of new Golden Torizo right-sonic poses, excluding a shared frame.</summary>
    internal const int GoldenTorizoRightSonicFrameCount = 21;
    /// <summary>Number of Torizo falling-left poses added to the catalog.</summary>
    internal const int TorizoFallingLeftFrameCount = 1;
    /// <summary>Number of new Golden Torizo left-foot orb poses.</summary>
    internal const int GoldenTorizoLeftFootOrbFrameCount = 5;
    /// <summary>Number of new left-facing Torizo jump-back poses, excluding the shared fall pose.</summary>
    internal const int TorizoJumpBackLeftNewFrameCount = 2;
    /// <summary>Number of distinct Golden Torizo left-orb poses.</summary>
    internal const int GoldenTorizoLeftOrbFrameCount = 12;
    /// <summary>Total number of Pirate frames across walking, wall, and ninja lists.</summary>
    internal const int PirateFrameCount =
        WalkingFrameCount + WallFrameCount + NinjaFrameCount;
    /// <summary>Total number of named frames emitted by <see cref="EnumerateFrames"/>.</summary>
    internal const int ExpectedFrameCount =
        PirateFrameCount + RidleyFrameCount +
        DraygonOamFrameCount + SporeSpawnOamFrameCount +
        CeresSteamFrameCount + OumFrameCount + CrocomireOamFrameCount +
        CrocomireBodyFrameCount + BombTorizoDormantFrameCount +
        GoldenTorizoInitialFrameCount + KraidArmFrameCount +
        GoldenTorizoAwakeningFrameCount + GoldenTorizoWalkingFrameCount +
        GoldenTorizoRightwardFrameCount + TorizoJumpBackFrameCount +
        GoldenTorizoRightOrbFrameCount + GoldenTorizoRightSonicFrameCount +
        TorizoFallingLeftFrameCount + GoldenTorizoLeftFootOrbFrameCount +
        TorizoJumpBackLeftNewFrameCount + GoldenTorizoLeftOrbFrameCount +
        CompleteTorizoAdditionalFrameCount + MotherBrainBodyVisualDefinitions.FrameCount +
        DraygonBg2FrameCount + PhantoonBg2FrameCount + CrocomireSkeletonVisualDefinitions.FrameCount +
        KraidFootVisualDefinitions.FrameCount;

    /// <summary>Sequence descriptor spanning every frame in the catalog's published order.</summary>
    internal static EnemyExtendedFrameSequence Frames => new(0, ExpectedFrameCount);

    /// <summary>These native roots contain BG2 streams only; stock legitimately has no OAM components.</summary>
    internal static bool IsBg2Only(EnemyExtendedFrameDefinition frame) =>
        frame.Bank == PhantoonBg2FrameDefinitions.Bank && PhantoonBg2FrameDefinitions.IsFrame(frame.Pointer) ||
        frame.Bank == DraygonBg2FrameDefinitions.Bank && DraygonBg2FrameDefinitions.IsFrame(frame.Pointer);

    /// <summary>Skeleton fragmentation needs thirteen components; all other OAM families retain their existing bound.</summary>
    internal static int MaximumOamComponents(EnemyExtendedFrameDefinition frame) =>
        CrocomireSkeletonVisualDefinitions.IsFrame(frame.Bank, frame.Pointer)
            ? CrocomireSkeletonVisualDefinitions.MaximumComponents : MaximumComponents;

    /// <summary>Emits the published schema order directly from each family.
    /// Instruction-selected families keep first-seen identity order; local sets
    /// deduplicate selection without caching frame definitions. Prefix counts
    /// preserve legacy schema boundaries and are checked during enumeration.</summary>
    internal static IEnumerable<EnemyExtendedFrameDefinition> EnumerateFrames()
    {
        var seen = new HashSet<ushort>();
        int count = 0;
        var torizoFrames = new HashSet<ushort>();
        EnemyExtendedFrameDefinition Emit(EnemyExtendedFrameDefinition frame)
        {
            count++;
            if (frame.Bank == TorizoInstructionProgramDefinitions.Bank) torizoFrames.Add(frame.Pointer);
            return frame;
        }
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
            if (count == WalkingFrameCount)
                throw new InvalidDataException("Walking Pirate has more frames than names.");
            yield return Emit(new EnemyExtendedFrameDefinition(Bank, pointer,
                PirateArtworkNameDefinitions.Walking(count)));
        }
        if (count != WalkingFrameCount)
            throw new InvalidDataException(
                $"Walking Pirate has {count} distinct frames; expected {WalkingFrameCount}.");
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
            if (wallCount == WallFrameCount)
                throw new InvalidDataException("Wall Pirate has more frames than names.");
            yield return Emit(new EnemyExtendedFrameDefinition(Bank, pointer,
                PirateArtworkNameDefinitions.Wall(wallCount++)));
        }
        if (wallCount != WallFrameCount ||
            count != WalkingFrameCount + WallFrameCount)
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
            yield return Emit(new EnemyExtendedFrameDefinition(Bank, pointer,
                $"ninja_pirate_{pointer:X4}"));
            ninjaCount++;
        }
        if (ninjaCount != NinjaFrameCount || count != PirateFrameCount)
            throw new InvalidDataException(
                $"Ninja Pirate has {ninjaCount} distinct frames; expected {NinjaFrameCount}.");
        for (int index = 0; index < RidleyFrameCount; index++)
        {
            ushort pointer = BossOamFrameDefinitions.RidleyPointer(index);
            yield return Emit(new EnemyExtendedFrameDefinition(0xa6, pointer,
                $"ridley_body_{pointer:X4}"));
        }
        if (count != PreDraygonFrameCount)
            throw new InvalidDataException("Draygon/Spore Spawn OAM frame catalog prefix changed.");
        for (int index = 0; index < DraygonOamFrameCount; index++)
        {
            ushort pointer = BossOamFrameDefinitions.DraygonPointer(index);
            yield return Emit(new EnemyExtendedFrameDefinition(0xa5, pointer,
                $"draygon_oam_{pointer:X4}"));
        }
        for (int index = 0; index < SporeSpawnOamFrameCount; index++)
        {
            ushort pointer = BossOamFrameDefinitions.SporeSpawnPointer(index);
            yield return Emit(new EnemyExtendedFrameDefinition(0xa5, pointer,
                $"spore_spawn_oam_{pointer:X4}"));
        }
        if (count != PreCeresSteamFrameCount ||
            CeresSteamCollisionDefinitions.FramePointers.Length != CeresSteamFrameCount)
            throw new InvalidDataException("Ceres steam extended-frame prefix changed.");
        for (int index = 0; index < CeresSteamCollisionDefinitions.FramePointers.Length; index++)
        {
            ushort pointer = CeresSteamCollisionDefinitions.FramePointers[index];
            yield return Emit(new EnemyExtendedFrameDefinition(CeresSteamCollisionDefinitions.Bank,
                pointer, $"ceres_steam_oam_{pointer:X4}"));
        }
        if (count != PreOumFrameCount ||
            MaridiaLargeSnailCollisionDefinitions.FramePointers.Length != OumFrameCount)
            throw new InvalidDataException("Oum extended-frame prefix changed.");
        for (int index = 0; index < MaridiaLargeSnailCollisionDefinitions.FramePointers.Length; index++)
        {
            ushort pointer = MaridiaLargeSnailCollisionDefinitions.FramePointers[index];
            yield return Emit(new EnemyExtendedFrameDefinition(
                MaridiaLargeSnailCollisionDefinitions.Bank, pointer, $"oum_oam_{pointer:X4}"));
        }
        if (count != PreCrocomireFrameCount ||
            CrocomireTongueCollisionDefinitions.FrameCount !=
                CrocomireOamFrameCount)
            throw new InvalidDataException("Crocomire OAM frame prefix changed.");
        for (int index = 0; index < CrocomireTongueCollisionDefinitions.FrameCount; index++)
        {
            ushort pointer = CrocomireTongueCollisionDefinitions.FramePointer(index);
            yield return Emit(new EnemyExtendedFrameDefinition(
                CrocomireTongueCollisionDefinitions.Bank,
                pointer, $"crocomire_oam_{pointer:X4}"));
        }
        if (count != PreCrocomireBodyFrameCount)
            throw new InvalidDataException("Crocomire body frame prefix changed.");
        foreach (ushort pointer in CrocomireBodyVisualDefinitions.Frames)
            yield return Emit(new EnemyExtendedFrameDefinition(
                CrocomireBodyVisualDefinitions.Bank,
                pointer, $"crocomire_body_oam_{pointer:X4}"));
        if (count != PreBombTorizoFrameCount)
            throw new InvalidDataException("Bomb Torizo extended-frame prefix changed.");
        yield return Emit(new EnemyExtendedFrameDefinition(0xaa, 0x87d0,
            "bomb_torizo_dormant"));
        if (count != PreGoldenTorizoFrameCount)
            throw new InvalidDataException("Golden Torizo extended-frame prefix changed.");
        yield return Emit(new EnemyExtendedFrameDefinition(0xaa, 0xaa30,
            "golden_torizo_initial"));
        if (count != PreKraidArmFrameCount)
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
                yield return Emit(new EnemyExtendedFrameDefinition(0xa7, pointer,
                    $"kraid_arm_oam_{pointer:X4}"));
        }
        if (kraidArmPointers.Count != KraidArmFrameCount)
            throw new InvalidDataException(
                $"Kraid arm selects {kraidArmPointers.Count} distinct visual frames, " +
                $"expected {KraidArmFrameCount}.");
        if (count != PreGoldenTorizoAwakeningFrameCount)
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
                yield return Emit(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"golden_torizo_awake_{pointer:X4}"));
        }
        if (goldenAwakeningPointers.Count - 1 != GoldenTorizoAwakeningFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo awakening selects {goldenAwakeningPointers.Count - 1} " +
                $"new visual frames, expected {GoldenTorizoAwakeningFrameCount}.");
        if (count != PreGoldenTorizoWalkingFrameCount)
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
                yield return Emit(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"golden_torizo_walk_left_{pointer:X4}"));
        }
        if (goldenWalkingPointers.Count != GoldenTorizoWalkingFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo walking selects {goldenWalkingPointers.Count} " +
                $"distinct visual frames, expected {GoldenTorizoWalkingFrameCount}.");
        if (count != PreGoldenTorizoRightwardFrameCount)
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
                yield return Emit(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"golden_torizo_rightward_{pointer:X4}"));
        }
        if (goldenRightwardPointers.Count != GoldenTorizoRightwardFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo rightward selects {goldenRightwardPointers.Count} " +
                $"distinct visual frames, expected {GoldenTorizoRightwardFrameCount}.");
        if (count != PreTorizoJumpBackFrameCount)
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
                yield return Emit(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"torizo_jump_back_{pointer:X4}"));
        }
        if (jumpBackPointers.Count != TorizoJumpBackFrameCount)
            throw new InvalidDataException(
                $"Torizo jump-back selects {jumpBackPointers.Count} " +
                $"distinct visual frames, expected {TorizoJumpBackFrameCount}.");
        if (count != PreGoldenTorizoRightOrbFrameCount)
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
                yield return Emit(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"golden_torizo_right_orb_{pointer:X4}"));
        }
        if (rightOrbPointers.Count != GoldenTorizoRightOrbFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo right-orb selects {rightOrbPointers.Count} " +
                $"distinct visual frames, expected {GoldenTorizoRightOrbFrameCount}.");
        if (count != PreGoldenTorizoRightSonicFrameCount)
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
                yield return Emit(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"golden_torizo_right_sonic_{pointer:X4}"));
        }
        if (rightSonicPointers.Count - 1 != GoldenTorizoRightSonicFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo right-sonic selects {rightSonicPointers.Count - 1} " +
                $"new visual frames, expected {GoldenTorizoRightSonicFrameCount}.");
        if (count != PreTorizoFallingLeftFrameCount)
            throw new InvalidDataException("Torizo falling-left extended-frame prefix changed.");
        ushort fallingLeftOperand = TorizoFallingLeftInstructionProgramDefinitions
            .PresentationWordAddress(0);
        if (!CompiledEnemyVisualSelectors.TryGet(0xaa, fallingLeftOperand,
                out ushort fallingLeftPointer) ||
            fallingLeftPointer != TorizoFallingLeftInstructionProgramDefinitions.FallingFrame)
            throw new InvalidDataException(
                $"Torizo falling-left visual operand $AA:{fallingLeftOperand:X4} is not compiled.");
        yield return Emit(new EnemyExtendedFrameDefinition(0xaa, fallingLeftPointer,
            $"torizo_falling_left_{fallingLeftPointer:X4}"));
        if (count != PreGoldenTorizoLeftFootOrbFrameCount)
            throw new InvalidDataException("Golden Torizo left-foot orb frame prefix changed.");
        // The opening ABEC pose is the sonic attack's existing editable frame.
        var leftFootOrbPointers = new HashSet<ushort> { 0xabec };
        for (int index = 0;
             index < GoldenTorizoLeftFootOrbInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = GoldenTorizoLeftFootOrbInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(0xaa, operand,
                    out ushort pointer))
                throw new InvalidDataException(
                    $"Golden Torizo left-foot orb visual operand $AA:{operand:X4} is not compiled.");
            if (leftFootOrbPointers.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"golden_torizo_left_foot_orb_{pointer:X4}"));
        }
        if (leftFootOrbPointers.Count - 1 != GoldenTorizoLeftFootOrbFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo left-foot orb selects {leftFootOrbPointers.Count - 1} " +
                $"new visual frames, expected {GoldenTorizoLeftFootOrbFrameCount}.");
        if (count != PreTorizoJumpBackLeftFrameCount)
            throw new InvalidDataException("Left-facing Torizo jump-back frame prefix changed.");
        // B014 was installed earlier for the shared falling-left animation.
        var leftJumpBackPointers = new HashSet<ushort> { fallingLeftPointer };
        for (int index = 0;
             index < TorizoJumpBackLeftInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = TorizoJumpBackLeftInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(0xaa, operand,
                    out ushort pointer))
                throw new InvalidDataException(
                    $"Left-facing Torizo jump-back visual operand $AA:{operand:X4} is not compiled.");
            if (leftJumpBackPointers.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"torizo_jump_back_left_{pointer:X4}"));
        }
        if (leftJumpBackPointers.Count - 1 != TorizoJumpBackLeftNewFrameCount)
            throw new InvalidDataException(
                $"Left-facing Torizo jump-back selects {leftJumpBackPointers.Count - 1} " +
                $"new visual frames, expected {TorizoJumpBackLeftNewFrameCount}.");
        if (count != PreGoldenTorizoLeftOrbFrameCount)
            throw new InvalidDataException("Golden Torizo left-orb frame prefix changed.");
        var leftOrbPointers = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoLeftOrbInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = GoldenTorizoLeftOrbInstructionProgramDefinitions
                .PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(0xaa, operand,
                    out ushort pointer))
                throw new InvalidDataException(
                    $"Golden Torizo left-orb visual operand $AA:{operand:X4} is not compiled.");
            if (leftOrbPointers.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(0xaa, pointer,
                    $"golden_torizo_left_orb_{pointer:X4}"));
        }
        if (leftOrbPointers.Count != GoldenTorizoLeftOrbFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo left-orb selects {leftOrbPointers.Count} " +
                $"visual frames, expected {GoldenTorizoLeftOrbFrameCount}.");
        if (count != PreCompleteTorizoFrameCount)
            throw new InvalidDataException("Complete Torizo extended-frame prefix changed.");
        for (int index = 0; index < TorizoInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort operand = TorizoInstructionProgramDefinitions.PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(TorizoInstructionProgramDefinitions.Bank,
                    operand, out ushort pointer))
                throw new InvalidDataException($"Torizo frame operand $AA:{operand:X4} is not compiled.");
            if (torizoFrames.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(TorizoInstructionProgramDefinitions.Bank,
                    pointer, $"torizo_combat_{pointer:X4}"));
        }
        if (count != PreMotherBrainBodyFrameCount)
            throw new InvalidDataException("Complete Torizo artwork coverage changed.");
        for (int index = 0; index < MotherBrainBodyVisualDefinitions.FrameCount; index++)
            yield return Emit(MotherBrainBodyVisualDefinitions.Frame(index));
        if (count != PreBg2BossBindingsFrameCount)
            throw new InvalidDataException("Mother Brain body artwork coverage changed.");
        for (int index = 0; index < PhantoonBg2FrameDefinitions.FrameCount; index++)
        {
            var frame = PhantoonBg2FrameDefinitions.Frame(index);
            yield return Emit(new EnemyExtendedFrameDefinition(PhantoonBg2FrameDefinitions.Bank,
                frame.Pointer, $"phantoon_bg2_{frame.Pointer:X4}"));
        }
        for (int index = 0; index < DraygonBg2FrameDefinitions.FrameCount; index++)
        {
            var frame = DraygonBg2FrameDefinitions.Frame(index);
            yield return Emit(new EnemyExtendedFrameDefinition(DraygonBg2FrameDefinitions.Bank, frame.Pointer, frame.Name));
        }
        if (count != PreCrocomireSkeletonFrameCount)
            throw new InvalidDataException("BG2-only boss display-binding coverage changed.");
        for (int index = 0; index < CrocomireSkeletonVisualDefinitions.FrameCount; index++)
            yield return Emit(CrocomireSkeletonVisualDefinitions.Frame(index));
        if (count != PreKraidFootFrameCount)
            throw new InvalidDataException("Crocomire skeleton display-binding coverage changed.");
        for (int index = 0; index < KraidFootVisualDefinitions.FrameCount; index++)
            yield return Emit(KraidFootVisualDefinitions.Frame(index));
        if (count != ExpectedFrameCount)
            throw new InvalidDataException("Kraid foot display-binding coverage changed.");
    }
}
