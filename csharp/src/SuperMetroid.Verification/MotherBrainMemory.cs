/// <summary>
/// Native WRAM identities of Mother Brain's encounter state, from the body ($0FA8) and brain
/// ($0FE8) slot variables and her extended enemy RAM.
/// </summary>
internal static class MotherBrainMemory
{
    /// <summary>$0FA8: body function pointer.</summary>
    public const int BodyFunction = 0x0fa8;
    /// <summary>$0FB2: body function timer, also the body's target X during walks.</summary>
    public const int BodyFunctionTimer = 0x0fb2;
    /// <summary>$0FB4: attack cooldown, also the rainbow beam's explosion index.</summary>
    public const int BodyAttackCooldown = 0x0fb4;
    /// <summary>$0FE8: brain function pointer.</summary>
    public const int BrainFunction = 0x0fe8;
    /// <summary>$0FF0: brain sub-function (tube collapse), also the death explosion interval timer.</summary>
    public const int BrainSubFunction = 0x0ff0;
    /// <summary>$0FF2: brain sub-function timer, also the body sub-function timer and death explosion index.</summary>
    public const int BrainSubFunctionTimer = 0x0ff2;
    /// <summary>$7E:7800: form (glass, fake death, phase two, three).</summary>
    public const int Form = 0x7800;
    /// <summary>$7E:7804: body pose written by body bytecode.</summary>
    public const int Pose = 0x7804;
    /// <summary>$7E:7808: hitboxes-enabled word.</summary>
    public const int HitboxesEnabled = 0x7808;
    /// <summary>$7E:780E: walk counter.</summary>
    public const int WalkCounter = 0x780e;
    /// <summary>$7E:7818: neck palette index.</summary>
    public const int NeckPaletteIndex = 0x7818;
    /// <summary>$7E:781A: brain palette index.</summary>
    public const int BrainPaletteIndex = 0x781a;
    /// <summary>$7E:781C: room palette instruction-list pointer.</summary>
    public const int RoomPaletteInstructionList = 0x781c;
    /// <summary>$7E:781E: room palette instruction timer.</summary>
    public const int RoomPaletteInstructionTimer = 0x781e;
    /// <summary>$7E:782E: death (hand) beam attack phase.</summary>
    public const int DeathBeamAttackPhase = 0x782e;
    /// <summary>$7E:7830: attack phase.</summary>
    public const int AttackPhase = 0x7830;
    /// <summary>$7E:7834: onion rings target angle.</summary>
    public const int OnionRingsTargetAngle = 0x7834;
    /// <summary>$7E:783A: delete-turrets-and-Rinkas flag.</summary>
    public const int DeleteTurretsAndRinkas = 0x783a;
    /// <summary>$7E:7826: Baby Metroid attack counter.</summary>
    public const int BabyMetroidAttackCounter = 0x7826;
    /// <summary>$7E:782A: rainbow beam sound queue countdown.</summary>
    public const int RainbowBeamSoundQueueCount = 0x782a;
    /// <summary>$7E:782C: rainbow beam sound playing flag.</summary>
    public const int RainbowBeamSoundPlaying = 0x782c;
    /// <summary>$7E:783E: phase-two corpse state.</summary>
    public const int Phase2CorpseState = 0x783e;
    /// <summary>$7E:7862: health-based palette handling flag.</summary>
    public const int HealthBasedPaletteHandling = 0x7862;
    /// <summary>$7E:7840: brain main shake timer.</summary>
    public const int BrainMainShakeTimer = 0x7840;
    /// <summary>$7E:7842: rainbow beam palette animation index.</summary>
    public const int RainbowPaletteIndex = 0x7842;
    /// <summary>$7E:7844: enable-unpause-hook flag.</summary>
    public const int EnableUnpauseHook = 0x7844;
    /// <summary>$7E:784A: bomb counter.</summary>
    public const int BombCounter = 0x784a;
    /// <summary>$7E:7860: brain palette handling enable flag.</summary>
    public const int BrainPaletteHandling = 0x7860;
    /// <summary>$7E:7864: drool projectile generation enable flag.</summary>
    public const int DroolGeneration = 0x7864;
    /// <summary>$7E:7866: parameter used when spawning drool projectiles.</summary>
    public const int DroolProjectileParameter = 0x7866;
    /// <summary>$7E:7868: small purple breath generation enable flag.</summary>
    public const int SmallPurpleBreathGeneration = 0x7868;
    /// <summary>$7E:786A: active small purple breath flag.</summary>
    public const int SmallPurpleBreathActive = 0x786a;
    /// <summary>$7E:8000: brain instruction timer.</summary>
    public const int BrainInstructionTimer = 0x8000;
    /// <summary>$7E:8002: brain instruction-list pointer.</summary>
    public const int BrainInstructionList = 0x8002;
    /// <summary>$7E:8004: sprite tile transfer entry pointer.</summary>
    public const int SpriteTileTransferEntry = 0x8004;
    /// <summary>$7E:8006-$8013: death beam next X/Y sub-position and position, velocities, angle.</summary>
    public const int DeathBeamNext = 0x8006;
    /// <summary>$7E:8026: rainbow beam angular width.</summary>
    public const int RainbowBeamAngularWidth = 0x8026;
    /// <summary>$7E:802E: grey transition counter.</summary>
    public const int GreyTransitionCounter = 0x802e;
    /// <summary>$7E:8030: fake death explosion timer.</summary>
    public const int FakeDeathExplosionTimer = 0x8030;
    /// <summary>$7E:8032: fake death explosion index.</summary>
    public const int FakeDeathExplosionIndex = 0x8032;
    /// <summary>$7E:8040: lower neck angle.</summary>
    public const int LowerNeckAngle = 0x8040;
    /// <summary>$7E:8042: upper neck angle.</summary>
    public const int UpperNeckAngle = 0x8042;
    /// <summary>$7E:8044: five neck segments, each X, Y and distance.</summary>
    public const int NeckSegments = 0x8044;
    /// <summary>$7E:8062: neck movement enable flag.</summary>
    public const int NeckMovementEnabled = 0x8062;
    /// <summary>$7E:8064: lower neck movement index.</summary>
    public const int LowerNeckMovementIndex = 0x8064;
    /// <summary>$7E:8066: upper neck movement index.</summary>
    public const int UpperNeckMovementIndex = 0x8066;
    /// <summary>$7E:8068: angle delta shared by the neck movement sequence.</summary>
    public const int NeckAngleDelta = 0x8068;
}
