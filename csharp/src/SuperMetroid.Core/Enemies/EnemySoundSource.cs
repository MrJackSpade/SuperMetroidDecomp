namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy-pass sound source whose request joins the update's sound requests only when the enemy pass ends, through
/// <see cref="EnemyAudioQueue.RequestSoundAtUpdateEnd"/>. A source holds one request per update: its latest request
/// replaces an earlier one. The room enemy system publishes the sources in one fixed order after every
/// enemy has run, which is the order the sound queue admits them in.
/// </summary>
internal enum EnemySoundSource
{
    /// <summary>Boyon's bounce.</summary>
    Boyon,
    /// <summary>Mama Turtle's spin and wall sounds.</summary>
    MamaTurtle,
    /// <summary>Cacatac's spike volley.</summary>
    Cacatac,
    /// <summary>A boulder's impact and break.</summary>
    Boulder,
    /// <summary>An Etecoon's wake, jump and wall sounds.</summary>
    Etecoon,
    /// <summary>Dachora's activation, charge, launch and impacts.</summary>
    Dachora,
    /// <summary>Evir's spit.</summary>
    Evir,
    /// <summary>The morph ball eye's activation and deactivation.</summary>
    MorphBallEye,
    /// <summary>A yapping maw's attack.</summary>
    YappingMaw,
    /// <summary>A hopper's jump and landing.</summary>
    Hoppers,
    /// <summary>Metaree's dive and burrow.</summary>
    Metaree,
    /// <summary>Alcoon's emergence and fire.</summary>
    Alcoon,
    /// <summary>Fune and Namihe's spit.</summary>
    FuneNamihe,
    /// <summary>A Kago bug's sound.</summary>
    Kago,
    /// <summary>Kzan's landing.</summary>
    Kzan,
    /// <summary>Nuclear Waffle's turn.</summary>
    NuclearWaffle,
    /// <summary>Fake Kraid's sounds.</summary>
    FakeKraid,
    /// <summary>A work robot's footsteps and laser.</summary>
    WorkRobot,
    /// <summary>Every space pirate's sounds: walking, wall and ninja pirates share this one source.</summary>
    SpacePirate,
    /// <summary>Rio's dive.</summary>
    Rio,
    /// <summary>The Norfair lava-jumping enemy's jump.</summary>
    NorfairLavaJumpingEnemy,
    /// <summary>The Norfair Rio's dive.</summary>
    NorfairRio,
    /// <summary>The Lower Norfair Rio's return.</summary>
    LowerNorfairRio,
    /// <summary>The Maridia large snail's splash and shot.</summary>
    MaridiaLargeSnail,
    /// <summary>Magdollite's cue sounds.</summary>
    Magdollite,
    /// <summary>A shutter's activation.</summary>
    Shutters,
    /// <summary>Yard's bounce.</summary>
    Yard,
    /// <summary>A Zebetite's sounds.</summary>
    Zebetite,
    /// <summary>A Metroid's animation cry and recoil.</summary>
    Metroid,
    /// <summary>
    /// A collected enemy drop. Unlike every other source, this request is not dropped when an enemy pass begins: it lasts
    /// until the next enemy-projectile pass cancels it.
    /// </summary>
    EnemyPickup,
    /// <summary>
    /// The death programs' killed cry and explosion sounds. Unlike every other source, this request is not dropped when
    /// an enemy pass begins: it lasts until the next enemy-projectile pass cancels it.
    /// </summary>
    EnemyDeath,
    /// <summary>A shot rejected by an enemy projectile or an invulnerable body.</summary>
    EnemyProjectileDud,
    /// <summary>Botwoon's sounds.</summary>
    Botwoon,
    /// <summary>Crocomire's sounds.</summary>
    Crocomire,
    /// <summary>The dead Torizo's dust.</summary>
    DeadTorizo,
    /// <summary>The dust of dead sidehoppers and Tourian corpses.</summary>
    DeadMonsters,
    /// <summary>The Shitroid's sounds.</summary>
    Shitroid,
    /// <summary>Spore Spawn's sounds.</summary>
    SporeSpawn,
    /// <summary>Kraid's sounds.</summary>
    Kraid,
    /// <summary>Ridley's death sounds.</summary>
    Ridley,
    /// <summary>Draygon's combat sounds.</summary>
    DraygonCombat,
    /// <summary>Draygon's death cry.</summary>
    DraygonDeathCry,
    /// <summary>Phantoon's materialization.</summary>
    PhantoonMaterialization,
    /// <summary>Phantoon's combat sounds.</summary>
    PhantoonCombat,
    /// <summary>Mother Brain's body sounds.</summary>
    MotherBrainBody,
    /// <summary>Mother Brain's rainbow beam.</summary>
    MotherBrainRainbowBeam,
    /// <summary>Mother Brain's instruction cues.</summary>
    MotherBrainCue,
}
