namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Schema generations of the editable enemy-projectile composition document.</summary>
public enum EnemyProjectileSpritemapVersion
{
    /// <summary>Initial schema generation containing only the two Ceres elevator-pad compositions and elevator platform, without debris or timed-program bindings.</summary>
    CeresOnly = 1,
    /// <summary>Schema 2: the Ceres and Skree/Metaree debris compositions, before any timed-program bindings.</summary>
    PreProgramFrames = 2,
    /// <summary>First schema generation requiring named visual frames for the translated shared projectile instruction mechanics.</summary>
    FirstProgramFrame = 3,
    /// <summary>Legacy schema 4's translated family set before Alcoon fireball bindings; loading omits those operands from the later pre-Golden-Torizo set.</summary>
    PreAlcoon = 4,
    /// <summary>Legacy schema 5 including Alcoon fireballs but preceding Golden Torizo super-missile and eye-beam program frames.</summary>
    PreGoldenTorizo = 5,
    /// <summary>Legacy schema 6 including Golden Torizo super missiles and eye beams but preceding its egg program frames.</summary>
    PreGoldenTorizoEgg = 6,
    /// <summary>Legacy schema 7 including Golden Torizo eggs but preceding the additional Torizo drool, swipe, sonic-boom, dust, explosion, and orb effects.</summary>
    PreTorizoEffects = 7,
    /// <summary>Legacy schema 8 including Torizo effects but preceding generic enemy death and pickup program frames.</summary>
    PreGenericEnemyDeath = 8,
    /// <summary>Legacy schema 9 containing generic enemy death/pickup frames but preceding the environment and additional attack-effect bindings.</summary>
    PreEnvironmentAndAttack = 9,
    /// <summary>Legacy schema 10 containing environment/attack effects but preceding Mother Brain and Bomb Torizo statue program-frame bindings.</summary>
    PreMotherBrainAndStatue = 10,
    /// <summary>Schema 11: all earlier projectile families, before Work Robot laser bindings.</summary>
    PreWorkRobot = 11,
    /// <summary>Schema before the single-frame Polyp lava-rock composition was installed.</summary>
    PrePolypRock = 12,
    /// <summary>Current schema generation 13, including the single-frame Polyp lava rock and all earlier projectile presentation bindings.</summary>
    Current = 13,
}
