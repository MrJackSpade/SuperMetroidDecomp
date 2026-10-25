namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Every supported schema of the editable extended-enemy-frame compositions, in schema order.</summary>
internal static class EnemyExtendedFrameSchema
{
    /// <summary>Original 37 walking Pirate frames, merged with stock for newer families.</summary>
    internal const int First = 1;
    /// <summary>Walking and wall Pirate frames before Ninja compositions joined the asset.</summary>
    internal const int Previous = 2;
    /// <summary>All Pirate frames before authored display-frame rebinding.</summary>
    internal const int PreDisplayBindings = 3;
    /// <summary>Pirate display bindings with no later boss-frame families.</summary>
    internal const int PirateDisplayBindings = 4;
    /// <summary>Pirate and Ridley frames before Draygon OAM compositions.</summary>
    internal const int PreDraygon = 5;
    /// <summary>Pre-Ceres frame count with older Spore Spawn names repaired during load.</summary>
    internal const int PreSporeIdentity = 6;
    /// <summary>Draygon and Spore Spawn frames before Ceres steam was added.</summary>
    internal const int PreCeresSteam = 7;
    /// <summary>Ceres steam frames before Oum's Maridia compositions.</summary>
    internal const int PreOum = 8;
    /// <summary>Oum frames before Crocomire's tongue and claw poses.</summary>
    internal const int PreCrocomire = 9;
    /// <summary>Crocomire's initial OAM poses before its body animation frames.</summary>
    internal const int PreCrocomireBody = 10;
    /// <summary>Crocomire body poses before the dormant Bomb Torizo statue.</summary>
    internal const int PreBombTorizo = 11;
    /// <summary>Dormant Bomb Torizo included; Golden Torizo's initial pose absent.</summary>
    internal const int PreGoldenTorizo = 12;
    /// <summary>Golden Torizo initial pose included; Kraid arm frames absent.</summary>
    internal const int PreKraidArm = 13;
    /// <summary>Kraid arm frames included; Golden Torizo awakening absent.</summary>
    internal const int PreGoldenTorizoAwakening = 14;
    /// <summary>Golden Torizo awakening included; walking poses absent.</summary>
    internal const int PreGoldenTorizoWalking = 15;
    /// <summary>Golden Torizo walking poses included; rightward turn absent.</summary>
    internal const int PreGoldenTorizoRightward = 16;
    /// <summary>Golden Torizo rightward turn included; jump-back poses absent.</summary>
    internal const int PreTorizoJumpBack = 17;
    /// <summary>Torizo jump-back poses included; right orb poses absent.</summary>
    internal const int PreGoldenTorizoRightOrb = 18;
    /// <summary>Golden Torizo right orb poses included; sonic attack absent.</summary>
    internal const int PreGoldenTorizoRightSonic = 19;
    /// <summary>Golden Torizo right sonic attack included; left-fall pose absent.</summary>
    internal const int PreTorizoFallingLeft = 20;
    /// <summary>Torizo's left-fall pose included; left-foot orb poses absent.</summary>
    internal const int PreGoldenTorizoLeftFootOrb = 21;
    /// <summary>Golden Torizo left-foot orb included; new left jump-back poses absent.</summary>
    internal const int PreTorizoJumpBackLeft = 22;
    /// <summary>Additional left jump-back poses included; left orb sequence absent.</summary>
    internal const int PreGoldenTorizoLeftOrb = 23;
    /// <summary>Golden Torizo left orb poses included; final Torizo additions absent.</summary>
    internal const int PreCompleteTorizo = 24;
    /// <summary>Complete Torizo family before Mother Brain body compositions.</summary>
    internal const int PreMotherBrainBody = 25;
    /// <summary>Schema 26 predates shared display bindings for BG2-only boss roots.</summary>
    internal const int PreBg2BossBindings = 26;
    /// <summary>Schema 27 predates Crocomire's thirty-three corpse/skeleton OAM roots.</summary>
    internal const int PreCrocomireSkeleton = 27;
    /// <summary>Schema 28 predates Kraid's thirty-five extended foot roots.</summary>
    internal const int PreKraidFoot = 28;
    /// <summary>The current schema.</summary>
    internal const int Current = 29;

    /// <summary>Whether <paramref name="version"/> is a revision this catalog accepts.</summary>
    internal static bool IsSupported(int version) => version is >= First and <= Current;
}
