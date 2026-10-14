namespace SuperMetroid.Core.Assets;

/// <summary>Every supported schema of the editable extended-enemy-frame compositions, in schema order.</summary>
internal enum EnemyExtendedFrameSchema
{
    /// <summary>Original 37 walking Pirate frames, merged with stock for newer families.</summary>
    First = 1,
    /// <summary>Walking and wall Pirate frames before Ninja compositions joined the asset.</summary>
    Previous = 2,
    /// <summary>All Pirate frames before authored display-frame rebinding.</summary>
    PreDisplayBindings = 3,
    /// <summary>Pirate display bindings with no later boss-frame families.</summary>
    PirateDisplayBindings = 4,
    /// <summary>Pirate and Ridley frames before Draygon OAM compositions.</summary>
    PreDraygon = 5,
    /// <summary>Pre-Ceres frame count with older Spore Spawn names repaired during load.</summary>
    PreSporeIdentity = 6,
    /// <summary>Draygon and Spore Spawn frames before Ceres steam was added.</summary>
    PreCeresSteam = 7,
    /// <summary>Ceres steam frames before Oum's Maridia compositions.</summary>
    PreOum = 8,
    /// <summary>Oum frames before Crocomire's tongue and claw poses.</summary>
    PreCrocomire = 9,
    /// <summary>Crocomire's initial OAM poses before its body animation frames.</summary>
    PreCrocomireBody = 10,
    /// <summary>Crocomire body poses before the dormant Bomb Torizo statue.</summary>
    PreBombTorizo = 11,
    /// <summary>Dormant Bomb Torizo included; Golden Torizo's initial pose absent.</summary>
    PreGoldenTorizo = 12,
    /// <summary>Golden Torizo initial pose included; Kraid arm frames absent.</summary>
    PreKraidArm = 13,
    /// <summary>Kraid arm frames included; Golden Torizo awakening absent.</summary>
    PreGoldenTorizoAwakening = 14,
    /// <summary>Golden Torizo awakening included; walking poses absent.</summary>
    PreGoldenTorizoWalking = 15,
    /// <summary>Golden Torizo walking poses included; rightward turn absent.</summary>
    PreGoldenTorizoRightward = 16,
    /// <summary>Golden Torizo rightward turn included; jump-back poses absent.</summary>
    PreTorizoJumpBack = 17,
    /// <summary>Torizo jump-back poses included; right orb poses absent.</summary>
    PreGoldenTorizoRightOrb = 18,
    /// <summary>Golden Torizo right orb poses included; sonic attack absent.</summary>
    PreGoldenTorizoRightSonic = 19,
    /// <summary>Golden Torizo right sonic attack included; left-fall pose absent.</summary>
    PreTorizoFallingLeft = 20,
    /// <summary>Torizo's left-fall pose included; left-foot orb poses absent.</summary>
    PreGoldenTorizoLeftFootOrb = 21,
    /// <summary>Golden Torizo left-foot orb included; new left jump-back poses absent.</summary>
    PreTorizoJumpBackLeft = 22,
    /// <summary>Additional left jump-back poses included; left orb sequence absent.</summary>
    PreGoldenTorizoLeftOrb = 23,
    /// <summary>Golden Torizo left orb poses included; final Torizo additions absent.</summary>
    PreCompleteTorizo = 24,
    /// <summary>Complete Torizo family before Mother Brain body compositions.</summary>
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
