using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// Named visual identities for Pirate, Ridley, Draygon, Spore Spawn, Ceres steam,
/// Maridia's Oum snail, Crocomire's tongue, fight-body and skeleton composite frames,
/// Bomb Torizo's dormant statue frame, the Torizos' third stand-up pose, Golden Torizo's awakening and
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
    internal const int WalkingFrameCount = 35;
    /// <summary>Wall Pirate frames following the walking family.</summary>
    internal const int WallFrameCount = 18;
    /// <summary>Ninja Pirate frames following the wall family.</summary>
    internal const int NinjaFrameCount = 75;
    /// <summary>Draygon frames whose editable content consists solely of BG2 tiles.</summary>
    internal const int DraygonBg2FrameCount = 32;
    /// <summary>Phantoon frames whose editable content consists solely of BG2 tiles.</summary>
    internal const int PhantoonBg2FrameCount = 18;
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
    /// <summary>The Torizos' third stand-up pose facing left, which Golden Torizo shows first, before its awakening.</summary>
    internal const int TorizoFacingLeftStandingUpThirdFrameCount = 1;
    /// <summary>Kraid arm OAM frames used independently of body art.</summary>
    internal const int KraidArmFrameCount = 12;
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
    /// <summary>The Torizos' one left-facing springing pose, which their leaps and falls show.</summary>
    internal const int TorizoFacingLeftSpringingFrameCount = 1;
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
        TorizoFacingLeftStandingUpThirdFrameCount + KraidArmFrameCount +
        GoldenTorizoAwakeningFrameCount + GoldenTorizoWalkingFrameCount +
        GoldenTorizoRightwardFrameCount + TorizoJumpBackFrameCount +
        GoldenTorizoRightOrbFrameCount + GoldenTorizoRightSonicFrameCount +
        TorizoFacingLeftSpringingFrameCount + GoldenTorizoLeftFootOrbFrameCount +
        TorizoJumpBackLeftNewFrameCount + GoldenTorizoLeftOrbFrameCount +
        CompleteTorizoAdditionalFrameCount + MotherBrainBodyFrameDefinitions.FrameCount +
        DraygonBg2FrameCount + PhantoonBg2FrameCount + CrocomireSkeletonVisualDefinitions.FrameCount +
        KraidFootVisualDefinitions.FrameCount;

    /// <summary>The frames of <paramref name="family"/>, as the document's first run of that family holds them.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The document holds no run of the family.</exception>
    internal static IEnumerable<EnemyFrameName> FramesOf(EnemyExtendedFrameFamily family)
    {
        foreach (EnemyExtendedFrameRun run in FamilyRuns)
        {
            if (run.Family == family)
                return run.Frames.Select(Named);
        }
        throw new ArgumentOutOfRangeException(nameof(family), family, "The document holds no frames of the family.");
    }

    /// <summary><paramref name="frame"/> with its document name.</summary>
    private static EnemyFrameName Named(EnemyFrameId frame) => new(frame, EnemyFrameNameDefinitions.Names[frame]);

    /// <summary>$10: the high byte of Ceres steam's spritemap headers, cartridge drawing metadata after the component count (#1415).</summary>
    private const int CeresSteamHeaderHighByte = 16;

    /// <summary>The document's frames as runs of one enemy family each, in the document's order (#1623).</summary>
    internal static ImmutableArray<EnemyExtendedFrameRun> FamilyRuns { get; } =
    [
        new(EnemyExtendedFrameFamily.WalkingPirate, EnemyExtendedFrameDocumentOrder.WalkingPirate),
        new(EnemyExtendedFrameFamily.WallPirate, EnemyExtendedFrameDocumentOrder.WallPirate),
        new(EnemyExtendedFrameFamily.NinjaPirate, EnemyExtendedFrameDocumentOrder.NinjaPirate),
        new(EnemyExtendedFrameFamily.RidleyBody, EnemyExtendedFrameDocumentOrder.RidleyBody),
        new(EnemyExtendedFrameFamily.Draygon, EnemyExtendedFrameDocumentOrder.DraygonSprites),
        new(EnemyExtendedFrameFamily.SporeSpawn, EnemyExtendedFrameDocumentOrder.SporeSpawn),
        new(EnemyExtendedFrameFamily.CeresSteam, EnemyExtendedFrameDocumentOrder.CeresSteam, HeaderHighByte: CeresSteamHeaderHighByte),
        new(EnemyExtendedFrameFamily.Oum, EnemyExtendedFrameDocumentOrder.Oum),
        new(EnemyExtendedFrameFamily.CrocomireTongue, EnemyExtendedFrameDocumentOrder.CrocomireTongue),
        new(EnemyExtendedFrameFamily.CrocomireBody, EnemyExtendedFrameDocumentOrder.CrocomireBody, EnemyExtendedFrameLayers.SpritesBesideBg2),
        new(EnemyExtendedFrameFamily.BombTorizo, EnemyExtendedFrameDocumentOrder.BombTorizoDormant),
        new(EnemyExtendedFrameFamily.GoldenTorizo, EnemyExtendedFrameDocumentOrder.TorizoFacingLeftStandingUpThird),
        new(EnemyExtendedFrameFamily.KraidArm, EnemyExtendedFrameDocumentOrder.KraidArm),
        new(EnemyExtendedFrameFamily.GoldenTorizo, EnemyExtendedFrameDocumentOrder.GoldenTorizoAwakeningWalkingAndTurning),
        new(EnemyExtendedFrameFamily.TorizoJumpBack, EnemyExtendedFrameDocumentOrder.TorizoJumpBack),
        new(EnemyExtendedFrameFamily.GoldenTorizo, EnemyExtendedFrameDocumentOrder.GoldenTorizoRightOrbAndSonic),
        new(EnemyExtendedFrameFamily.TorizoFacingLeftSpringing, EnemyExtendedFrameDocumentOrder.TorizoFacingLeftSpringing),
        new(EnemyExtendedFrameFamily.GoldenTorizo, EnemyExtendedFrameDocumentOrder.GoldenTorizoLeftFootOrb),
        new(EnemyExtendedFrameFamily.TorizoJumpBack, EnemyExtendedFrameDocumentOrder.TorizoJumpBackLeft),
        new(EnemyExtendedFrameFamily.GoldenTorizo, EnemyExtendedFrameDocumentOrder.GoldenTorizoLeftOrb),
        new(EnemyExtendedFrameFamily.TorizoCombat, EnemyExtendedFrameDocumentOrder.TorizoCombat),
        new(EnemyExtendedFrameFamily.MotherBrainBody, EnemyExtendedFrameDocumentOrder.MotherBrainBody, EnemyExtendedFrameLayers.SpritesBesideBg2,
            MotherBrainBodyBg2FrameDefinitions.MaximumComponents),
        new(EnemyExtendedFrameFamily.Phantoon, EnemyExtendedFrameDocumentOrder.PhantoonBg2, EnemyExtendedFrameLayers.Bg2Only, PhantoonBg2FrameDefinitions.MaximumComponents),
        new(EnemyExtendedFrameFamily.Draygon, EnemyExtendedFrameDocumentOrder.DraygonBg2, EnemyExtendedFrameLayers.Bg2Only, DraygonBg2FrameDefinitions.MaximumComponents),
        new(EnemyExtendedFrameFamily.CrocomireSkeleton, EnemyExtendedFrameDocumentOrder.CrocomireSkeleton,
            MaximumComponents: CrocomireSkeletonVisualDefinitions.MaximumComponents),
        new(EnemyExtendedFrameFamily.KraidFoot, EnemyExtendedFrameDocumentOrder.KraidFoot),
    ];

    /// <summary>Every extended frame, each with its document name, in the document's order: each family run's frames in turn.</summary>
    internal static ImmutableArray<EnemyFrameName> Frames { get; } = [.. FamilyRuns.SelectMany(run => run.Frames).Select(Named)];
}
