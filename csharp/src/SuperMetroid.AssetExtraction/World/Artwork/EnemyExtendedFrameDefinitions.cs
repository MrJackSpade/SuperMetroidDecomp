using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Bosses.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

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
    /// <summary>Additional Torizo identities introduced by the completed-family schema.</summary>
    internal const int CompleteTorizoAdditionalFrameCount = 27;
    /// <summary>Installed JSON asset name retained for historical pirate-composition compatibility.</summary>
    internal const string FileName = "enemy-walking-pirate-compositions.json";
    /// <summary>OAM component bound for ordinary extended frames outside Crocomire's skeleton.</summary>
    internal const int MaximumComponents = 8;
    /// <summary>Walking Pirate frames published first in the schema order.</summary>
    internal const int WalkingFrameCount = 37;
    /// <summary>Wall Pirate frames following the walking family.</summary>
    internal const int WallFrameCount = 18;
    /// <summary>Ninja Pirate frames following the wall family.</summary>
    internal const int NinjaFrameCount = 76;
    /// <summary>Draygon frames whose editable content consists solely of BG2 tiles.</summary>
    internal const int DraygonBg2FrameCount = 34;
    /// <summary>Phantoon frames whose editable content consists solely of BG2 tiles.</summary>
    internal const int PhantoonBg2FrameCount = 22;
    /// <summary>Ceres steam OAM frames added after Spore Spawn identities.</summary>
    internal const int CeresSteamFrameCount = 28;
    /// <summary>Oum snail OAM frames supplied by Maridia's instruction catalog.</summary>
    internal const int OumFrameCount = 30;
    /// <summary>Crocomire tongue and claw sprite frames before the body sequence.</summary>
    internal const int CrocomireSpriteFrameCount = 9;
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
        PirateFrameCount + BossSpriteFrameDefinitions.RidleyFrameCount +
        BossSpriteFrameDefinitions.DraygonSpriteFrameCount + BossSpriteFrameDefinitions.SporeSpawnSpriteFrameCount +
        CeresSteamFrameCount + OumFrameCount + CrocomireSpriteFrameCount +
        CrocomireBodyFrameCount + BombTorizoDormantFrameCount +
        GoldenTorizoInitialFrameCount + KraidArmFrameCount +
        GoldenTorizoAwakeningFrameCount + GoldenTorizoWalkingFrameCount +
        GoldenTorizoRightwardFrameCount + TorizoJumpBackFrameCount +
        GoldenTorizoRightOrbFrameCount + GoldenTorizoRightSonicFrameCount +
        TorizoFallingLeftFrameCount + GoldenTorizoLeftFootOrbFrameCount +
        TorizoJumpBackLeftNewFrameCount + GoldenTorizoLeftOrbFrameCount +
        CompleteTorizoAdditionalFrameCount + MotherBrainBodyFrameDefinitions.FrameCount +
        DraygonBg2FrameCount + PhantoonBg2FrameCount + CrocomireSkeletonVisualDefinitions.FrameCount +
        KraidFootVisualDefinitions.FrameCount;

    /// <summary>Indexed view spanning the complete schema-ordered visual catalog.</summary>
    internal static EnemyExtendedFrameSequence Frames => new(0, ExpectedFrameCount);

    /// <summary>The document's frames as runs of one enemy family each, in the document's order (#1623).</summary>
    internal static ImmutableArray<EnemyExtendedFrameRun> FamilyRuns { get; } =
    [
        new(EnemyExtendedFrameFamily.WalkingPirate, WalkingFrameCount),
        new(EnemyExtendedFrameFamily.WallPirate, WallFrameCount),
        new(EnemyExtendedFrameFamily.NinjaPirate, NinjaFrameCount),
        new(EnemyExtendedFrameFamily.RidleyBody, BossSpriteFrameDefinitions.RidleyFrameCount),
        new(EnemyExtendedFrameFamily.Draygon, BossSpriteFrameDefinitions.DraygonSpriteFrameCount),
        new(EnemyExtendedFrameFamily.SporeSpawn, BossSpriteFrameDefinitions.SporeSpawnSpriteFrameCount),
        new(EnemyExtendedFrameFamily.CeresSteam, CeresSteamFrameCount),
        new(EnemyExtendedFrameFamily.Oum, OumFrameCount),
        new(EnemyExtendedFrameFamily.CrocomireTongue, CrocomireSpriteFrameCount),
        new(EnemyExtendedFrameFamily.CrocomireBody, CrocomireBodyFrameCount),
        new(EnemyExtendedFrameFamily.BombTorizo, BombTorizoDormantFrameCount),
        new(EnemyExtendedFrameFamily.GoldenTorizo, GoldenTorizoInitialFrameCount),
        new(EnemyExtendedFrameFamily.KraidArm, KraidArmFrameCount),
        new(EnemyExtendedFrameFamily.GoldenTorizo, GoldenTorizoAwakeningFrameCount + GoldenTorizoWalkingFrameCount + GoldenTorizoRightwardFrameCount),
        new(EnemyExtendedFrameFamily.TorizoJumpBack, TorizoJumpBackFrameCount),
        new(EnemyExtendedFrameFamily.GoldenTorizo, GoldenTorizoRightOrbFrameCount + GoldenTorizoRightSonicFrameCount),
        new(EnemyExtendedFrameFamily.TorizoFallingLeft, TorizoFallingLeftFrameCount),
        new(EnemyExtendedFrameFamily.GoldenTorizo, GoldenTorizoLeftFootOrbFrameCount),
        new(EnemyExtendedFrameFamily.TorizoJumpBack, TorizoJumpBackLeftNewFrameCount),
        new(EnemyExtendedFrameFamily.GoldenTorizo, GoldenTorizoLeftOrbFrameCount),
        new(EnemyExtendedFrameFamily.TorizoCombat, CompleteTorizoAdditionalFrameCount),
        new(EnemyExtendedFrameFamily.MotherBrainBody, MotherBrainBodyFrameDefinitions.FrameCount),
        new(EnemyExtendedFrameFamily.Phantoon, PhantoonBg2FrameCount),
        new(EnemyExtendedFrameFamily.Draygon, DraygonBg2FrameCount),
        new(EnemyExtendedFrameFamily.CrocomireSkeleton, CrocomireSkeletonVisualDefinitions.FrameCount),
        new(EnemyExtendedFrameFamily.KraidFoot, KraidFootVisualDefinitions.FrameCount),
    ];
}
