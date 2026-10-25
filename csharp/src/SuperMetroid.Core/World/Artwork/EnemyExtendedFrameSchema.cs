using System.Collections.Immutable;

namespace SuperMetroid.Core.World.Artwork;

/// <summary>A supported schema of the editable extended-enemy-frame compositions, with the named frames its documents carry.</summary>
/// <param name="Revision">The revision number a document stores.</param>
/// <param name="FrameCount">The named frames a document of this revision carries.</param>
internal sealed record EnemyExtendedFrameSchema(int Revision, int FrameCount)
{
    /// <summary>Original 37 walking Pirate frames, merged with stock for newer families.</summary>
    internal static readonly EnemyExtendedFrameSchema First = new(1, EnemyExtendedFrameDefinitions.WalkingFrameCount);
    /// <summary>Walking and wall Pirate frames before Ninja compositions joined the asset.</summary>
    internal static readonly EnemyExtendedFrameSchema Previous = new(2, EnemyExtendedFrameDefinitions.WalkingFrameCount + EnemyExtendedFrameDefinitions.WallFrameCount);
    /// <summary>All Pirate frames before authored display-frame rebinding.</summary>
    internal static readonly EnemyExtendedFrameSchema PreDisplayBindings = new(3, EnemyExtendedFrameDefinitions.PirateFrameCount);
    /// <summary>Pirate display bindings with no later boss-frame families.</summary>
    internal static readonly EnemyExtendedFrameSchema PirateDisplayBindings = new(4, EnemyExtendedFrameDefinitions.PirateFrameCount);
    /// <summary>Pirate and Ridley frames before Draygon OAM compositions.</summary>
    internal static readonly EnemyExtendedFrameSchema PreDraygon = new(5, EnemyExtendedFrameDefinitions.PreDraygonFrameCount);
    /// <summary>Pre-Ceres frame count with older Spore Spawn names repaired during load.</summary>
    internal static readonly EnemyExtendedFrameSchema PreSporeIdentity = new(6, EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount);
    /// <summary>Draygon and Spore Spawn frames before Ceres steam was added.</summary>
    internal static readonly EnemyExtendedFrameSchema PreCeresSteam = new(7, EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount);
    /// <summary>Ceres steam frames before Oum's Maridia compositions.</summary>
    internal static readonly EnemyExtendedFrameSchema PreOum = new(8, EnemyExtendedFrameDefinitions.PreOumFrameCount);
    /// <summary>Oum frames before Crocomire's tongue and claw poses.</summary>
    internal static readonly EnemyExtendedFrameSchema PreCrocomire = new(9, EnemyExtendedFrameDefinitions.PreCrocomireFrameCount);
    /// <summary>Crocomire's initial OAM poses before its body animation frames.</summary>
    internal static readonly EnemyExtendedFrameSchema PreCrocomireBody = new(10, EnemyExtendedFrameDefinitions.PreCrocomireBodyFrameCount);
    /// <summary>Crocomire body poses before the dormant Bomb Torizo statue.</summary>
    internal static readonly EnemyExtendedFrameSchema PreBombTorizo = new(11, EnemyExtendedFrameDefinitions.PreBombTorizoFrameCount);
    /// <summary>Dormant Bomb Torizo included; Golden Torizo's initial pose absent.</summary>
    internal static readonly EnemyExtendedFrameSchema PreGoldenTorizo = new(12, EnemyExtendedFrameDefinitions.PreGoldenTorizoFrameCount);
    /// <summary>Golden Torizo initial pose included; Kraid arm frames absent.</summary>
    internal static readonly EnemyExtendedFrameSchema PreKraidArm = new(13, EnemyExtendedFrameDefinitions.PreKraidArmFrameCount);
    /// <summary>Kraid arm frames included; Golden Torizo awakening absent.</summary>
    internal static readonly EnemyExtendedFrameSchema PreGoldenTorizoAwakening = new(14, EnemyExtendedFrameDefinitions.PreGoldenTorizoAwakeningFrameCount);
    /// <summary>Golden Torizo awakening included; walking poses absent.</summary>
    internal static readonly EnemyExtendedFrameSchema PreGoldenTorizoWalking = new(15, EnemyExtendedFrameDefinitions.PreGoldenTorizoWalkingFrameCount);
    /// <summary>Golden Torizo walking poses included; rightward turn absent.</summary>
    internal static readonly EnemyExtendedFrameSchema PreGoldenTorizoRightward = new(16, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightwardFrameCount);
    /// <summary>Golden Torizo rightward turn included; jump-back poses absent.</summary>
    internal static readonly EnemyExtendedFrameSchema PreTorizoJumpBack = new(17, EnemyExtendedFrameDefinitions.PreTorizoJumpBackFrameCount);
    /// <summary>Torizo jump-back poses included; right orb poses absent.</summary>
    internal static readonly EnemyExtendedFrameSchema PreGoldenTorizoRightOrb = new(18, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightOrbFrameCount);
    /// <summary>Golden Torizo right orb poses included; sonic attack absent.</summary>
    internal static readonly EnemyExtendedFrameSchema PreGoldenTorizoRightSonic = new(19, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightSonicFrameCount);
    /// <summary>Golden Torizo right sonic attack included; left-fall pose absent.</summary>
    internal static readonly EnemyExtendedFrameSchema PreTorizoFallingLeft = new(20, EnemyExtendedFrameDefinitions.PreTorizoFallingLeftFrameCount);
    /// <summary>Torizo's left-fall pose included; left-foot orb poses absent.</summary>
    internal static readonly EnemyExtendedFrameSchema PreGoldenTorizoLeftFootOrb = new(21, EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftFootOrbFrameCount);
    /// <summary>Golden Torizo left-foot orb included; new left jump-back poses absent.</summary>
    internal static readonly EnemyExtendedFrameSchema PreTorizoJumpBackLeft = new(22, EnemyExtendedFrameDefinitions.PreTorizoJumpBackLeftFrameCount);
    /// <summary>Additional left jump-back poses included; left orb sequence absent.</summary>
    internal static readonly EnemyExtendedFrameSchema PreGoldenTorizoLeftOrb = new(23, EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftOrbFrameCount);
    /// <summary>Golden Torizo left orb poses included; final Torizo additions absent.</summary>
    internal static readonly EnemyExtendedFrameSchema PreCompleteTorizo = new(24, EnemyExtendedFrameDefinitions.PreCompleteTorizoFrameCount);
    /// <summary>Complete Torizo family before Mother Brain body compositions.</summary>
    internal static readonly EnemyExtendedFrameSchema PreMotherBrainBody = new(25, EnemyExtendedFrameDefinitions.PreMotherBrainBodyFrameCount);
    /// <summary>Schema 26 predates shared display bindings for BG2-only boss roots.</summary>
    internal static readonly EnemyExtendedFrameSchema PreBg2BossBindings = new(26, EnemyExtendedFrameDefinitions.PreBg2BossBindingsFrameCount);
    /// <summary>Schema 27 predates Crocomire's thirty-three corpse/skeleton OAM roots.</summary>
    internal static readonly EnemyExtendedFrameSchema PreCrocomireSkeleton = new(27, EnemyExtendedFrameDefinitions.PreCrocomireSkeletonFrameCount);
    /// <summary>Schema 28 predates Kraid's thirty-five extended foot roots.</summary>
    internal static readonly EnemyExtendedFrameSchema PreKraidFoot = new(28, EnemyExtendedFrameDefinitions.PreKraidFootFrameCount);
    /// <summary>The current schema.</summary>
    internal static readonly EnemyExtendedFrameSchema Current = new(29, EnemyExtendedFrameDefinitions.ExpectedFrameCount);

    /// <summary>Every accepted revision, oldest first.</summary>
    internal static readonly ImmutableArray<EnemyExtendedFrameSchema> Supported =
    [
        First,
        Previous,
        PreDisplayBindings,
        PirateDisplayBindings,
        PreDraygon,
        PreSporeIdentity,
        PreCeresSteam,
        PreOum,
        PreCrocomire,
        PreCrocomireBody,
        PreBombTorizo,
        PreGoldenTorizo,
        PreKraidArm,
        PreGoldenTorizoAwakening,
        PreGoldenTorizoWalking,
        PreGoldenTorizoRightward,
        PreTorizoJumpBack,
        PreGoldenTorizoRightOrb,
        PreGoldenTorizoRightSonic,
        PreTorizoFallingLeft,
        PreGoldenTorizoLeftFootOrb,
        PreTorizoJumpBackLeft,
        PreGoldenTorizoLeftOrb,
        PreCompleteTorizo,
        PreMotherBrainBody,
        PreBg2BossBindings,
        PreCrocomireSkeleton,
        PreKraidFoot,
        Current,
    ];

    /// <summary>
    /// Whether documents of this revision author their own display bindings: every revision after
    /// <see cref="PreDisplayBindings"/>; earlier revisions inherit the stock bindings.
    /// </summary>
    internal bool OwnsDisplayBindings => Supported.IndexOf(this) > Supported.IndexOf(PreDisplayBindings);

    /// <summary>The accepted revision a document stores as <paramref name="revision"/>, or null when none is.</summary>
    internal static EnemyExtendedFrameSchema? FromRevision(int revision) => Supported.FirstOrDefault(schema => schema.Revision == revision);
}
