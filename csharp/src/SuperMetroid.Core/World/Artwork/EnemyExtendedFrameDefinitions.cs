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
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.World.Artwork;

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
        PirateFrameCount + BossOamFrameDefinitions.RidleyFrameCount +
        BossOamFrameDefinitions.DraygonOamFrameCount + BossOamFrameDefinitions.SporeSpawnOamFrameCount +
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

    /// <summary>These frames contain BG2 streams only; stock legitimately has no OAM components.</summary>
    internal static bool IsBg2Only(EnemySpritemapDefinition frame) =>
        PhantoonBg2FrameDefinitions.IsFrame(frame.Frame) || DraygonBg2FrameDefinitions.IsFrame(frame.Frame);

    /// <summary>Skeleton fragmentation needs thirteen components; all other OAM families retain their existing bound.</summary>
    internal static int MaximumOamComponents(EnemySpritemapDefinition frame) =>
        CrocomireSkeletonVisualDefinitions.IsFrame(frame.Frame)
            ? CrocomireSkeletonVisualDefinitions.MaximumComponents : MaximumComponents;
}
