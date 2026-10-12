using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Every supported schema of the editable extended-enemy-frame compositions, in schema order.</summary>
internal enum EnemyExtendedFrameSchema
{
    /// <summary>Historical schema 1.</summary>
    First = 1,
    /// <summary>Historical schema 2.</summary>
    Previous = 2,
    /// <summary>Historical schema 3.</summary>
    PreDisplayBindings = 3,
    /// <summary>Historical schema 4.</summary>
    PirateDisplayBindings = 4,
    /// <summary>Historical schema 5.</summary>
    PreDraygon = 5,
    /// <summary>Historical schema 6.</summary>
    PreSporeIdentity = 6,
    /// <summary>Historical schema 7.</summary>
    PreCeresSteam = 7,
    /// <summary>Historical schema 8.</summary>
    PreOum = 8,
    /// <summary>Historical schema 9.</summary>
    PreCrocomire = 9,
    /// <summary>Historical schema 10.</summary>
    PreCrocomireBody = 10,
    /// <summary>Historical schema 11.</summary>
    PreBombTorizo = 11,
    /// <summary>Historical schema 12.</summary>
    PreGoldenTorizo = 12,
    /// <summary>Historical schema 13.</summary>
    PreKraidArm = 13,
    /// <summary>Historical schema 14.</summary>
    PreGoldenTorizoAwakening = 14,
    /// <summary>Historical schema 15.</summary>
    PreGoldenTorizoWalking = 15,
    /// <summary>Historical schema 16.</summary>
    PreGoldenTorizoRightward = 16,
    /// <summary>Historical schema 17.</summary>
    PreTorizoJumpBack = 17,
    /// <summary>Historical schema 18.</summary>
    PreGoldenTorizoRightOrb = 18,
    /// <summary>Historical schema 19.</summary>
    PreGoldenTorizoRightSonic = 19,
    /// <summary>Historical schema 20.</summary>
    PreTorizoFallingLeft = 20,
    /// <summary>Historical schema 21.</summary>
    PreGoldenTorizoLeftFootOrb = 21,
    /// <summary>Historical schema 22.</summary>
    PreTorizoJumpBackLeft = 22,
    /// <summary>Historical schema 23.</summary>
    PreGoldenTorizoLeftOrb = 23,
    /// <summary>Historical schema 24.</summary>
    PreCompleteTorizo = 24,
    /// <summary>Historical schema 25.</summary>
    PreMotherBrainBody = 25,
    /// <summary>Schema 26 predates shared display bindings for BG2-only boss roots.</summary>
    PreBg2BossBindings = 26,
    /// <summary>Schema 27 predates Crocomire's thirty-three corpse/skeleton OAM roots.</summary>
    PreCrocomireSkeleton = 27,
    /// <summary>Schema 28 predates Kraid's thirty-five extended foot roots.</summary>
    PreKraidFoot = 28,
    /// <summary>The current schema.</summary>
    Current = 29,
}

/// <summary>One named, fixed extended-frame identity; collision data is not editable art.</summary>
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
    internal const int PreDraygonFrameCount = 142;
    internal const int PreCeresSteamFrameCount = 202;
    internal const int PreOumFrameCount = 230;
    internal const int PreCrocomireFrameCount = 260;
    internal const int PreCrocomireBodyFrameCount = 269;
    internal const int PreBombTorizoFrameCount = 319;
    internal const int PreGoldenTorizoFrameCount = 320;
    internal const int PreKraidArmFrameCount = 321;
    internal const int PreGoldenTorizoAwakeningFrameCount = 343;
    internal const int PreGoldenTorizoWalkingFrameCount = 349;
    internal const int PreGoldenTorizoRightwardFrameCount = 359;
    internal const int PreTorizoJumpBackFrameCount = 370;
    internal const int PreGoldenTorizoRightOrbFrameCount = 373;
    internal const int PreGoldenTorizoRightSonicFrameCount = 379;
    internal const int PreTorizoFallingLeftFrameCount = 400;
    internal const int PreGoldenTorizoLeftFootOrbFrameCount = 401;
    internal const int PreTorizoJumpBackLeftFrameCount = 406;
    internal const int PreGoldenTorizoLeftOrbFrameCount = 408;
    internal const int PreCompleteTorizoFrameCount = 420;
    internal const int CompleteTorizoAdditionalFrameCount = 27;
    internal const int PreMotherBrainBodyFrameCount = 447;
    internal const int PreBg2BossBindingsFrameCount = 464;
    internal const int PreCrocomireSkeletonFrameCount = 520;
    internal const int PreKraidFootFrameCount = 553;
    internal const string FileName = "enemy-walking-pirate-compositions.json";
    internal const byte Bank = 0xb2;
    internal const int MaximumComponents = 8;
    internal const int WalkingFrameCount = 37;
    internal const int WallFrameCount = 18;
    internal const int NinjaFrameCount = 76;
    internal const int RidleyFrameCount = 11;
    internal const int DraygonOamFrameCount = 48;
    internal const int DraygonBg2FrameCount = 34;
    internal const int PhantoonBg2FrameCount = 22;
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
    internal const int TorizoFallingLeftFrameCount = 1;
    internal const int GoldenTorizoLeftFootOrbFrameCount = 5;
    internal const int TorizoJumpBackLeftNewFrameCount = 2;
    internal const int GoldenTorizoLeftOrbFrameCount = 12;
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
        GoldenTorizoRightOrbFrameCount + GoldenTorizoRightSonicFrameCount +
        TorizoFallingLeftFrameCount + GoldenTorizoLeftFootOrbFrameCount +
        TorizoJumpBackLeftNewFrameCount + GoldenTorizoLeftOrbFrameCount +
        CompleteTorizoAdditionalFrameCount + MotherBrainBodyVisualDefinitions.FrameCount +
        DraygonBg2FrameCount + PhantoonBg2FrameCount + CrocomireSkeletonVisualDefinitions.FrameCount +
        KraidFootVisualDefinitions.FrameCount;

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
