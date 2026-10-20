using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

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
    /// <summary>Legacy prefix before Draygon OAM identities joined the editable catalog.</summary>
    internal const int PreDraygonFrameCount = 142;
    /// <summary>Legacy prefix before Ceres steam extended frames were added.</summary>
    internal const int PreCeresSteamFrameCount = 202;
    /// <summary>Legacy prefix before Oum's thirty editable OAM frames.</summary>
    internal const int PreOumFrameCount = 230;
    /// <summary>Legacy prefix before Crocomire's initial tongue and claw frames.</summary>
    internal const int PreCrocomireFrameCount = 260;
    /// <summary>Legacy prefix before Crocomire's body animation roots.</summary>
    internal const int PreCrocomireBodyFrameCount = 269;
    /// <summary>Legacy prefix before the dormant Bomb Torizo statue pose.</summary>
    internal const int PreBombTorizoFrameCount = 319;
    /// <summary>Legacy prefix before Golden Torizo's initial OAM pose.</summary>
    internal const int PreGoldenTorizoFrameCount = 320;
    /// <summary>Legacy prefix before Kraid's independently animated arm frames.</summary>
    internal const int PreKraidArmFrameCount = 321;
    /// <summary>Legacy prefix before Golden Torizo's awakening sequence.</summary>
    internal const int PreGoldenTorizoAwakeningFrameCount = 343;
    /// <summary>Legacy prefix before Golden Torizo's walking sequence.</summary>
    internal const int PreGoldenTorizoWalkingFrameCount = 349;
    /// <summary>Legacy prefix before Golden Torizo's rightward turn poses.</summary>
    internal const int PreGoldenTorizoRightwardFrameCount = 359;
    /// <summary>Legacy prefix before Torizo jump-back component frames.</summary>
    internal const int PreTorizoJumpBackFrameCount = 370;
    /// <summary>Legacy prefix before Golden Torizo's right orb poses.</summary>
    internal const int PreGoldenTorizoRightOrbFrameCount = 373;
    /// <summary>Legacy prefix before Golden Torizo's right sonic attack sequence.</summary>
    internal const int PreGoldenTorizoRightSonicFrameCount = 379;
    /// <summary>Legacy prefix before Torizo's left-fall pose.</summary>
    internal const int PreTorizoFallingLeftFrameCount = 400;
    /// <summary>Legacy prefix before Golden Torizo's left-foot orb frames.</summary>
    internal const int PreGoldenTorizoLeftFootOrbFrameCount = 401;
    /// <summary>Legacy prefix before the two new left jump-back frames.</summary>
    internal const int PreTorizoJumpBackLeftFrameCount = 406;
    /// <summary>Legacy prefix before Golden Torizo's left orb frames.</summary>
    internal const int PreGoldenTorizoLeftOrbFrameCount = 408;
    /// <summary>Legacy prefix before the final twenty-seven Torizo frames completed the family.</summary>
    internal const int PreCompleteTorizoFrameCount = 420;
    /// <summary>Additional Torizo identities introduced by the completed-family schema.</summary>
    internal const int CompleteTorizoAdditionalFrameCount = 27;
    /// <summary>Legacy prefix before Mother Brain's mixed body component poses.</summary>
    internal const int PreMotherBrainBodyFrameCount = 447;
    /// <summary>Legacy prefix before BG2-only Draygon and Phantoon display bindings.</summary>
    internal const int PreBg2BossBindingsFrameCount = 464;
    /// <summary>Legacy prefix before Crocomire's corpse and skeleton composite frames.</summary>
    internal const int PreCrocomireSkeletonFrameCount = 520;
    /// <summary>Legacy prefix before Kraid's thirty-five extended foot frames.</summary>
    internal const int PreKraidFootFrameCount = 553;
    /// <summary>Installed JSON asset name retained for historical pirate-composition compatibility.</summary>
    internal const string FileName = "enemy-walking-pirate-compositions.json";
    /// <summary>Native instruction bank for the initial Pirate frame family.</summary>
    internal const byte Bank = 0xb2;
    /// <summary>OAM component bound for ordinary extended frames outside Crocomire's skeleton.</summary>
    internal const int MaximumComponents = 8;
    /// <summary>Walking Pirate frames published first in the schema order.</summary>
    internal const int WalkingFrameCount = 37;
    /// <summary>Wall Pirate frames following the walking family.</summary>
    internal const int WallFrameCount = 18;
    /// <summary>Ninja Pirate frames following the wall family.</summary>
    internal const int NinjaFrameCount = 76;
    /// <summary>Side-facing Ridley body frames in this visual catalog.</summary>
    internal const int RidleyFrameCount = 11;
    /// <summary>Draygon OAM component frames, separate from its BG2-only poses.</summary>
    internal const int DraygonOamFrameCount = 48;
    /// <summary>Draygon frames whose editable content consists solely of BG2 tiles.</summary>
    internal const int DraygonBg2FrameCount = 34;
    /// <summary>Phantoon frames whose editable content consists solely of BG2 tiles.</summary>
    internal const int PhantoonBg2FrameCount = 22;
    /// <summary>Spore Spawn OAM frames selected by its animation instructions.</summary>
    internal const int SporeSpawnOamFrameCount = 12;
    /// <summary>Ceres steam OAM frames added after Spore Spawn identities.</summary>
    internal const int CeresSteamFrameCount = 28;
    /// <summary>Oum snail OAM frames supplied by Maridia's instruction catalog.</summary>
    internal const int OumFrameCount = 30;
    /// <summary>Crocomire tongue and claw OAM frames before the body sequence.</summary>
    internal const int CrocomireOamFrameCount = 9;
    /// <summary>Body frame count supplied by Crocomire's visual definitions.</summary>
    internal const int CrocomireBodyFrameCount = CrocomireBodyVisualDefinitions.BodyFrameCount;
    /// <summary>Single dormant Bomb Torizo statue composition.</summary>
    internal const int BombTorizoDormantFrameCount = 1;
    /// <summary>Single Golden Torizo pose preceding its awakening animation.</summary>
    internal const int GoldenTorizoInitialFrameCount = 1;
    /// <summary>Kraid arm OAM frames used independently of body art.</summary>
    internal const int KraidArmFrameCount = 22;
    /// <summary>Golden Torizo awakening component poses.</summary>
    internal const int GoldenTorizoAwakeningFrameCount = 6;
    /// <summary>Golden Torizo walking component poses.</summary>
    internal const int GoldenTorizoWalkingFrameCount = 10;
    /// <summary>Golden Torizo rightward turn component poses.</summary>
    internal const int GoldenTorizoRightwardFrameCount = 11;
    /// <summary>Torizo backward-jump component poses.</summary>
    internal const int TorizoJumpBackFrameCount = 3;
    /// <summary>Golden Torizo right-facing orb component poses.</summary>
    internal const int GoldenTorizoRightOrbFrameCount = 6;
    /// <summary>Golden Torizo right-facing sonic attack poses.</summary>
    internal const int GoldenTorizoRightSonicFrameCount = 21;
    /// <summary>Single left-falling Torizo component pose.</summary>
    internal const int TorizoFallingLeftFrameCount = 1;
    /// <summary>Golden Torizo left-foot orb component poses.</summary>
    internal const int GoldenTorizoLeftFootOrbFrameCount = 5;
    /// <summary>Additional left-facing Torizo jump-back poses.</summary>
    internal const int TorizoJumpBackLeftNewFrameCount = 2;
    /// <summary>Golden Torizo left-facing orb component poses.</summary>
    internal const int GoldenTorizoLeftOrbFrameCount = 12;
    /// <summary>Total initial Pirate prefix before boss families.</summary>
    internal const int PirateFrameCount =
        WalkingFrameCount + WallFrameCount + NinjaFrameCount;
    /// <summary>Expected complete schema length, checked against family enumeration.</summary>
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

    /// <summary>Indexed view spanning the complete schema-ordered visual catalog.</summary>
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
            if (frame.Bank == TorizoFrameDefinitions.Bank) torizoFrames.Add(frame.Pointer);
            return frame;
        }
        foreach (ushort pointer in WalkingSpacePirateAnimations.Spritemaps(Enum.GetValues<WalkingSpacePirateProgram>()))
        {
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
        foreach (ushort pointer in WallSpacePirateAnimations.Spritemaps(Enum.GetValues<WallSpacePirateProgram>()))
        {
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
        foreach (ushort pointer in NinjaSpacePirateAnimations.Spritemaps(Enum.GetValues<NinjaSpacePirateProgram>()))
        {
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
        yield return Emit(new EnemyExtendedFrameDefinition(0xaa, 0xaa30,
            "golden_torizo_initial"));
        var kraidArmPointers = new HashSet<ushort>();
        foreach (ushort pointer in KraidAnimations.Spritemaps(KraidPartProgram.ArmNormal, KraidPartProgram.ArmNormalPause,
                     KraidPartProgram.ArmSlow, KraidPartProgram.ArmRisingOrSinking, KraidPartProgram.ArmDyingOrPreparingToLunge))
        {
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
        foreach (ushort pointer in TorizoAnimations.Spritemaps(
                     TorizoBodyProgram.GoldenInitial0Resumed,
                     TorizoBodyProgram.GoldenInitial1,
                     TorizoBodyProgram.GoldenInitial2,
                     TorizoBodyProgram.GoldenInitial3))
        {
            if (goldenAwakeningPointers.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(TorizoFrameDefinitions.Bank, pointer,
                    $"golden_torizo_awake_{pointer:X4}"));
        }
        if (goldenAwakeningPointers.Count - 1 != GoldenTorizoAwakeningFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo awakening selects {goldenAwakeningPointers.Count - 1} " +
                $"new visual frames, expected {GoldenTorizoAwakeningFrameCount}.");
        if (count != PreGoldenTorizoWalkingFrameCount)
            throw new InvalidDataException("Golden Torizo walking extended-frame prefix changed.");
        var goldenWalkingPointers = new HashSet<ushort>();
        foreach (ushort pointer in TorizoAnimations.Spritemaps(
                     TorizoBodyProgram.GoldenWalkingLeftRightLegMoving,
                     TorizoBodyProgram.GoldenWalkingLeftLeftLegMoving))
        {
            if (goldenWalkingPointers.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(TorizoFrameDefinitions.Bank, pointer,
                    $"golden_torizo_walk_left_{pointer:X4}"));
        }
        if (goldenWalkingPointers.Count != GoldenTorizoWalkingFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo walking selects {goldenWalkingPointers.Count} " +
                $"distinct visual frames, expected {GoldenTorizoWalkingFrameCount}.");
        if (count != PreGoldenTorizoRightwardFrameCount)
            throw new InvalidDataException("Golden Torizo rightward extended-frame prefix changed.");
        var goldenRightwardPointers = new HashSet<ushort>();
        foreach (ushort pointer in TorizoAnimations.Spritemaps(
                     TorizoBodyProgram.GoldenDodgeTurningRight,
                     TorizoBodyProgram.GoldenTurningRight,
                     TorizoBodyProgram.GoldenWalkingRightLeftLegMoving,
                     TorizoBodyProgram.GoldenWalkingRightRightLegMoving))
        {
            if (goldenRightwardPointers.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(TorizoFrameDefinitions.Bank, pointer,
                    $"golden_torizo_rightward_{pointer:X4}"));
        }
        if (goldenRightwardPointers.Count != GoldenTorizoRightwardFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo rightward selects {goldenRightwardPointers.Count} " +
                $"distinct visual frames, expected {GoldenTorizoRightwardFrameCount}.");
        if (count != PreTorizoJumpBackFrameCount)
            throw new InvalidDataException("Torizo jump-back extended-frame prefix changed.");
        var jumpBackPointers = new HashSet<ushort>();
        foreach (ushort pointer in TorizoAnimations.Spritemaps(
                     TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd0,
                     TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd1,
                     TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd2,
                     TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd3,
                     TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd4,
                     TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd0,
                     TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd1,
                     TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd2,
                     TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd3,
                     TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd4))
        {
            if (jumpBackPointers.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(TorizoFrameDefinitions.Bank, pointer,
                    $"torizo_jump_back_{pointer:X4}"));
        }
        if (jumpBackPointers.Count != TorizoJumpBackFrameCount)
            throw new InvalidDataException(
                $"Torizo jump-back selects {jumpBackPointers.Count} " +
                $"distinct visual frames, expected {TorizoJumpBackFrameCount}.");
        if (count != PreGoldenTorizoRightOrbFrameCount)
            throw new InvalidDataException("Golden Torizo right-orb frame prefix changed.");
        var rightOrbPointers = new HashSet<ushort>();
        foreach (ushort pointer in TorizoAnimations.Spritemaps(
                     TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftRightFootFwd0,
                     TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftRightFootFwd1))
        {
            if (rightOrbPointers.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(TorizoFrameDefinitions.Bank, pointer,
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
        foreach (ushort pointer in TorizoAnimations.Spritemaps(
                     TorizoBodyProgram.GoldenSonicBoomsFacingRightLeftFootFwd0,
                     TorizoBodyProgram.GoldenSonicBoomsFacingRightLeftFootFwd1,
                     TorizoBodyProgram.GoldenSonicBoomsFacingRightRightFootFwd0,
                     TorizoBodyProgram.GoldenSonicBoomsFacingRightRightFootFwd1))
        {
            if (rightSonicPointers.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(TorizoFrameDefinitions.Bank, pointer,
                    $"golden_torizo_right_sonic_{pointer:X4}"));
        }
        if (rightSonicPointers.Count - 1 != GoldenTorizoRightSonicFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo right-sonic selects {rightSonicPointers.Count - 1} " +
                $"new visual frames, expected {GoldenTorizoRightSonicFrameCount}.");
        if (count != PreTorizoFallingLeftFrameCount)
            throw new InvalidDataException("Torizo falling-left extended-frame prefix changed.");
        // The falling-left lists show one pose throughout.
        ushort fallingLeftPointer = TorizoAnimations.Spritemaps(TorizoBodyProgram.FacingLeftFalling0,
            TorizoBodyProgram.FacingLeftFalling1, TorizoBodyProgram.FacingLeftFalling2).Distinct().Single();
        yield return Emit(new EnemyExtendedFrameDefinition(TorizoFrameDefinitions.Bank, fallingLeftPointer,
            $"torizo_falling_left_{fallingLeftPointer:X4}"));
        // The opening ABEC pose is the sonic attack's existing editable frame.
        var leftFootOrbPointers = new HashSet<ushort> { 0xabec };
        foreach (ushort pointer in TorizoAnimations.Spritemaps(
                     TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftLeftFootFwd0,
                     TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftLeftFootFwd1))
        {
            if (leftFootOrbPointers.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(TorizoFrameDefinitions.Bank, pointer,
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
        foreach (ushort pointer in TorizoAnimations.Spritemaps(
                     TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd0,
                     TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd1,
                     TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd2,
                     TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd3,
                     TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd4,
                     TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd0,
                     TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd1,
                     TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd2,
                     TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd3,
                     TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd4))
        {
            if (leftJumpBackPointers.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(TorizoFrameDefinitions.Bank, pointer,
                    $"torizo_jump_back_left_{pointer:X4}"));
        }
        if (leftJumpBackPointers.Count - 1 != TorizoJumpBackLeftNewFrameCount)
            throw new InvalidDataException(
                $"Left-facing Torizo jump-back selects {leftJumpBackPointers.Count - 1} " +
                $"new visual frames, expected {TorizoJumpBackLeftNewFrameCount}.");
        if (count != PreGoldenTorizoLeftOrbFrameCount)
            throw new InvalidDataException("Golden Torizo left-orb frame prefix changed.");
        var leftOrbPointers = new HashSet<ushort>();
        foreach (ushort pointer in TorizoAnimations.Spritemaps(
                     TorizoBodyProgram.GoldenSpewChozoOrbsFaceLeftRightFootFwd0,
                     TorizoBodyProgram.GoldenSpewChozoOrbsFaceLeftRightFootFwd1,
                     TorizoBodyProgram.GoldenSpewChozoOrbsFacingLeftLeftFootFwd0,
                     TorizoBodyProgram.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1))
        {
            if (leftOrbPointers.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(TorizoFrameDefinitions.Bank, pointer,
                    $"golden_torizo_left_orb_{pointer:X4}"));
        }
        if (leftOrbPointers.Count != GoldenTorizoLeftOrbFrameCount)
            throw new InvalidDataException(
                $"Golden Torizo left-orb selects {leftOrbPointers.Count} " +
                $"visual frames, expected {GoldenTorizoLeftOrbFrameCount}.");
        if (count != PreCompleteTorizoFrameCount)
            throw new InvalidDataException("Complete Torizo extended-frame prefix changed.");
        foreach (ushort pointer in TorizoAnimations.Spritemaps(TorizoAnimations.AllPrograms))
        {
            if (torizoFrames.Add(pointer))
                yield return Emit(new EnemyExtendedFrameDefinition(TorizoFrameDefinitions.Bank,
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
