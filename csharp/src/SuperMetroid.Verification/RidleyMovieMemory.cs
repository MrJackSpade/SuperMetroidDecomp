/// <summary>Native WRAM identities used only to import the supplied Ridley movie initial-state fixture.</summary>
internal static class RidleyMovieMemory
{
    /// <summary>$0A1F: native Samus movement-type byte used by draw dispatch.</summary>
    public const int SamusMovementType = 0x0a1f;
    /// <summary>$093F: Ceres status; high bit requests Mode 7 position transformation.</summary>
    public const int CeresStatus = 0x093f;
    /// <summary>$92:9263/$92:945D: top/bottom pose-to-base-spritemap tables.</summary>
    public const int TopSpritemapBases = 0x929263, BottomSpritemapBases = 0x92945d;
    /// <summary>$91:B629: eight-byte pose definitions; graphics Y offset is byte four.</summary>
    public const int PoseDefinitions = 0x91b629;
    /// <summary>$90:8D28: landing offsets, read as unaligned sixteen-bit words.</summary>
    public const int LandingDrawOffsets = 0x908d28;
    /// <summary>$90:8D80: signed crouch/posture drawing offsets.</summary>
    public const int PostureDrawOffsets = 0x908d80;
    /// <summary>$90:8DEF: signed drained crouch/fall drawing offsets.</summary>
    public const int DrainedDrawOffsets = 0x908def;

    /// <summary>$0A68: shared shine timer bypassing ordinary invincibility flicker.</summary>
    public const int SamusShineTimer = 0x0a68;

    /// <summary>$0AC8/$0ACA: last selected top/bottom Samus spritemaps.</summary>
    public const int SamusTopSpritemap = 0x0ac8, SamusBottomSpritemap = 0x0aca;
    /// <summary>$0B04/$0B06: last screen-space Samus sprite origin.</summary>
    public const int SamusSpriteX = 0x0b04, SamusSpriteY = 0x0b06;
    /// <summary>$0AA6: packed arm-cannon open and transition flags.</summary>
    public const int CannonFlags = 0x0aa6;
    /// <summary>$0AA8/$0AAA/$0AAC: cannon frame, item-change latch and draw mode.</summary>
    public const int CannonFrame = 0x0aa8, CannonToggle = 0x0aaa, CannonDrawingMode = 0x0aac;

    /// <summary>$C608-$C6C7: three mutable HUD tilemap rows.</summary>
    public const int HudTilemap = 0xc608;
    /// <summary>$0A0E: previously highlighted HUD item.</summary>
    public const int PreviousHudSelection = 0x0a0e;

    /// <summary>$05F7: boss-owned minimap suppression flag.</summary>
    public const int MinimapDisabled = 0x05f7;

    /// <summary>$CD20: complete mutable fifty-byte room scroll allocation.</summary>
    public const int ScrollStorage = 0xcd20;
    /// <summary>$079F: current area selector for the live explored-map plane.</summary>
    public const int CurrentArea = 0x079f;
    /// <summary>$07F7: live current-area explored-map bytes.</summary>
    public const int LiveExploredMap = 0x07f7;
    /// <summary>$CD52: seven saved unpacked explored-map planes.</summary>
    public const int SavedExploredMaps = 0xcd52;
    /// <summary>$D8F8: sixteen save/elevator marker bytes.</summary>
    public const int SaveElevatorMarkers = 0xd8f8;
    /// <summary>$D908: twelve map-station marker bytes.</summary>
    public const int MapStationMarkers = 0xd908;

    /// <summary>$0B0A/$0B0E: ideal layer-one camera targets.</summary>
    public const int IdealCameraX = 0x0b0a, IdealCameraY = 0x0b0e;
    /// <summary>$0B10-$0B17: previous Samus position retained by scrolling.</summary>
    public const int PreviousSamusX = 0x0b10, PreviousSamusXFraction = 0x0b12,
        PreviousSamusY = 0x0b14, PreviousSamusYFraction = 0x0b16;
    /// <summary>$0DA2-$0DA9: camera distance-plus-one whole/fractional X/Y pairs.</summary>
    public const int CameraSpeedX = 0x0da2, CameraSpeedXFraction = 0x0da4,
        CameraSpeedY = 0x0da6, CameraSpeedYFraction = 0x0da8;

    /// <summary>$0DA8: camera Y fractional displacement adjacent to projectile inheritance.</summary>
    public const int ProjectileInheritancePrefix = 0x0da8;
    /// <summary>$0DAA-$0DB9: left/right/up/down projectile-inherited displacement pairs.</summary>
    public const int ProjectileInheritedMovement = 0x0daa;
    /// <summary>$0DBA: slope collision/alignment changed Samus Y.</summary>
    public const int SamusSlopeAdjusted = 0x0dba;
    /// <summary>$182C-$1833: retained solid-enemy indices for left/right/up/down collision.</summary>
    public const int SamusSolidEnemyIndices = 0x182c;

    /// <summary>$0DBC/$0DBE: total horizontal speed after extra momentum and divisor.</summary>
    public const int TotalHorizontalSpeed = 0x0dbc, TotalHorizontalSubspeed = 0x0dbe;
    /// <summary>$0A58: installed Samus movement handler.</summary>
    public const int SamusMovementHandler = 0x0a58;
    /// <summary>$90:A337: normal movement-type dispatcher.</summary>
    public const ushort NormalMovementHandler = 0xa337;
    /// <summary>$90:DF38: knockback movement dispatcher.</summary>
    public const ushort KnockbackMovementHandler = 0xdf38;
    /// <summary>$0A42/$0A44: paired alpha/beta control handlers.</summary>
    public const int SamusAlphaHandler = 0x0a42, SamusBetaHandler = 0x0a44;
    /// <summary>$90:E695/$90:E725: normal Samus alpha/beta handlers.</summary>
    public const ushort NormalAlphaHandler = 0xe695, NormalBetaHandler = 0xe725;
    /// <summary>$90:E713/$90:E8DC: locked Samus alpha/beta handlers.</summary>
    public const ushort LockedAlphaHandler = 0xe713, LockedBetaHandler = 0xe8dc;

    /// <summary>$0A6C: environment-selected horizontal movement table base.</summary>
    public const int HorizontalSpeedTable = 0x0a6c;
    /// <summary>$0B4C: eight-bit horizontal deceleration multiplier.</summary>
    public const int HorizontalDecelerationMultiplier = 0x0b4c;
    /// <summary>$0B40: retained Speed Booster echo sound latch.</summary>
    public const int SpeedEchoSoundLatch = 0x0b40;

    /// <summary>$0DFE: held input history published by the draw and main-loop epilogues.</summary>
    public const int SamusFilteredHeld = 0x0dfe;
    /// <summary>$0E00: Samus draw-time PreviousDrawNewInput state.</summary>
    public const int SamusFilteredNew = 0x0e00;
    /// <summary>$0AF4: Samus draw-time AutoJumpTimer state.</summary>
    public const int SamusAutoJumpTimer = 0x0af4;
    /// <summary>$0A12: Samus draw-time PreviousHealthForHurtCheck state.</summary>
    public const int SamusPreviousHealthForFlash = 0x0a12;

    /// <summary>$0A60: Samus input-handler dispatch word.</summary>
    public const int SamusInputHandler = 0x0a60;
    /// <summary>$90:E913: ordinary Samus pose-input handler.</summary>
    public const ushort SamusNormalInputHandler = 0xe913;
    /// <summary>$90:E926: one-shot HandleAutoJumpHack input handler.</summary>
    public const ushort SamusAutoJumpInputHandler = 0xe926;

    /// <summary>$0AD4: native AtmosphericTimer liquid/atmospheric owner.</summary>
    public const int AtmosphericTimer = 0x0ad4;
    /// <summary>$0ADC: native AtmosphericX liquid/atmospheric owner.</summary>
    public const int AtmosphericX = 0x0adc;
    /// <summary>$0AE4: native AtmosphericY liquid/atmospheric owner.</summary>
    public const int AtmosphericY = 0x0ae4;
    /// <summary>$0AEC: native AtmosphericFrameAndType liquid/atmospheric owner.</summary>
    public const int AtmosphericFrameAndType = 0x0aec;
    /// <summary>$0AD2: native LiquidPhysicsType liquid/atmospheric owner.</summary>
    public const int LiquidPhysicsType = 0x0ad2;
    /// <summary>$0A9C: native AnimationFrameBuffer liquid/atmospheric owner.</summary>
    public const int AnimationFrameBuffer = 0x0a9c;
    /// <summary>$0A4E: native PeriodicSubDamage liquid/atmospheric owner.</summary>
    public const int PeriodicSubDamage = 0x0a4e;
    /// <summary>$0A50: native PeriodicDamage liquid/atmospheric owner.</summary>
    public const int PeriodicDamage = 0x0a50;

    /// <summary>$D658: projectile trail left InstructionTimer, eighteen word-strided slots.</summary>
    public const int TrailLeftInstructionTimer = 0xd658;
    /// <summary>$D67C: projectile trail right InstructionTimer, eighteen word-strided slots.</summary>
    public const int TrailRightInstructionTimer = 0xd67c;
    /// <summary>$D6A0: projectile trail left InstructionPointer, eighteen word-strided slots.</summary>
    public const int TrailLeftInstructionPointer = 0xd6a0;
    /// <summary>$D6C4: projectile trail right InstructionPointer, eighteen word-strided slots.</summary>
    public const int TrailRightInstructionPointer = 0xd6c4;
    /// <summary>$D6E8: projectile trail left TileNumberAttributes, eighteen word-strided slots.</summary>
    public const int TrailLeftTileNumberAttributes = 0xd6e8;
    /// <summary>$D70C: projectile trail right TileNumberAttributes, eighteen word-strided slots.</summary>
    public const int TrailRightTileNumberAttributes = 0xd70c;
    /// <summary>$D730: projectile trail left XPosition, eighteen word-strided slots.</summary>
    public const int TrailLeftXPosition = 0xd730;
    /// <summary>$D754: projectile trail right XPosition, eighteen word-strided slots.</summary>
    public const int TrailRightXPosition = 0xd754;
    /// <summary>$D778: projectile trail left YPosition, eighteen word-strided slots.</summary>
    public const int TrailLeftYPosition = 0xd778;
    /// <summary>$D79C: projectile trail right YPosition, eighteen word-strided slots.</summary>
    public const int TrailRightYPosition = 0xd79c;

    /// <summary>$F380: enemy-projectile shot collision option, zero/destructible, one/dud, two/skip.</summary>
    public const int EnemyProjectileCollisionOption = 0xf380;
    /// <summary>$F3C8: enemy-projectile source enemy header used to resolve drop chances.</summary>
    public const int EnemyProjectileEnemyHeader = 0xf3c8;
    /// <summary>$F410: enemy-projectile killed-enemy index and respawn flag.</summary>
    public const int EnemyProjectileKilledEnemy = 0xf410;

    /// <summary>$0C68: ordinary/bomb projectile pre-instruction words.</summary>
    public const int ProjectilePreInstruction = 0x0c68;
    /// <summary>$90:B169: ProjPreInstr_Empty, retained by active impact animations.</summary>
    public const ushort ProjectileEmptyCallback = 0xb169;
    /// <summary>$90:AF68: ProjPreInstr_Missile, ordinary missile flight.</summary>
    public const ushort ProjectileMissileCallback = 0xaf68;
    /// <summary>$90:AFE5: ProjPreInstr_SuperMissile, main Super Missile flight.</summary>
    public const ushort ProjectileSuperMissileCallback = 0xafe5;
    /// <summary>$90:B075: ProjPreInstr_Func1, Super Missile companion/link motion.</summary>
    public const ushort ProjectileSuperMissileLinkCallback = 0xb075;

    /// <summary>$1AFF: enemy-projectile E, interpreted by its active family.</summary>
    public const int EnemyProjectileVariableE = 0x1aff;
    /// <summary>$1B23: enemy-projectile F, interpreted by its active family.</summary>
    public const int EnemyProjectileVariableF = 0x1b23;
    /// <summary>$1BFB: enemy-projectile G, including collision projectile type.</summary>
    public const int EnemyProjectileVariableG = 0x1bfb;
    /// <summary>$1BD7 bit $1000: enemy-projectile high draw pass.</summary>
    public const ushort EnemyProjectileHighDraw = 0x1000;
    /// <summary>$1BD7 bit $2000: enemy-projectile Samus contact disabled.</summary>
    public const ushort EnemyProjectileNoContact = 0x2000;
    /// <summary>$1BD7 bit $4000: enemy-projectile persists on Samus contact.</summary>
    public const ushort EnemyProjectilePersistent = 0x4000;
    /// <summary>$1BD7 bit $8000: enemy-projectile blocks Samus shots.</summary>
    public const ushort EnemyProjectileShotCollision = 0x8000;

    /// <summary>$0592: bank-$88 power-bomb explosion activation status.</summary>
    public const int PowerBombExplosionStatus = 0x0592;
    /// <summary>$0CEE: armed power-bomb flag, negative from placement through cleanup.</summary>
    public const int PowerBombArmedFlag = 0x0cee;

    /// <summary>$0CCE: native ProjectileCounter firing owner.</summary>
    public const int ProjectileCount = 0x0cce;
    /// <summary>$0DC2: native PreviousBeamChargeCounter firing owner.</summary>
    public const int PreviousCharge = 0x0dc2;
    /// <summary>$18AC: native ProjectileInvincibilityTimer firing owner.</summary>
    public const int ProjectileInteractionImmunity = 0x18ac;
    /// <summary>$0B18: native ChargedShotGlowTimer firing owner.</summary>
    public const int ChargedShotGlow = 0x0b18;
    /// <summary>$0B62: native SamusChargePaletteIndex firing owner.</summary>
    public const int ChargePaletteIndex = 0x0b62;
    /// <summary>$0CD2: native BombCounter firing owner.</summary>
    public const int BombCount = 0x0cd2;
    /// <summary>$0CD4: native BombSpreadChargeTimeoutCounter firing owner.</summary>
    public const int BombSpreadChargeTimeout = 0x0cd4;
    /// <summary>$0B5E: native PoseTransitionShotDirection firing owner.</summary>
    public const int PoseShotDirection = 0x0b5e;
    /// <summary>$0A76: native HyperBeam firing owner.</summary>
    public const int HyperBeam = 0x0a76;
    /// <summary>$0DC0: native ResumeChargingBeamSoundFlag firing owner.</summary>
    public const int ResumeChargeSound = 0x0dc0;

    /// <summary>$0AFE: Samus SamusXRadius native movement state.</summary>
    public const int SamusXRadius = 0x0afe;
    /// <summary>$0B00: Samus SamusYRadius native movement state.</summary>
    public const int SamusYRadius = 0x0b00;
    /// <summary>$0B34: Samus Gravity native movement state.</summary>
    public const int Gravity = 0x0b34;
    /// <summary>$0B32: Samus GravityFraction native movement state.</summary>
    public const int GravityFraction = 0x0b32;
    /// <summary>$0B58: Samus ExtraXDisplacement native movement state.</summary>
    public const int ExtraXDisplacement = 0x0b58;
    /// <summary>$0B56: Samus ExtraXDisplacementFraction native movement state.</summary>
    public const int ExtraXDisplacementFraction = 0x0b56;
    /// <summary>$0B5C: Samus ExtraYDisplacement native movement state.</summary>
    public const int ExtraYDisplacement = 0x0b5c;
    /// <summary>$0B5A: Samus ExtraYDisplacementFraction native movement state.</summary>
    public const int ExtraYDisplacementFraction = 0x0b5a;
    /// <summary>$0A46: Samus SlopeCollisionEnable native movement state.</summary>
    public const int SlopeCollisionEnable = 0x0a46;
    /// <summary>$0A66: Samus SpeedDivisor native movement state.</summary>
    public const int SpeedDivisor = 0x0a66;
    /// <summary>$0A6E: Samus ContactDamageIndex native movement state.</summary>
    public const int ContactDamageIndex = 0x0a6e;
    /// <summary>$0B20: Samus MorphBallBounceState native movement state.</summary>
    public const int MorphBallBounceState = 0x0b20;
    /// <summary>$0A56: Samus BombJumpDirection native movement state.</summary>
    public const int BombJumpDirection = 0x0a56;

    /// <summary>$84:84E6: RTS installed as the default PLM pre-instruction.</summary>
    public const ushort PlmDefaultPreInstruction = 0x84e6;
    /// <summary>$84:BE4B: grey-door condition pre-instruction dispatch table.</summary>
    public const int GreyDoorConditionTable = 0x84be4b;
    /// <summary>Bank $84 contains PLM programs and callback identities.</summary>
    public const int PlmProgramBank = 0x840000;

    /// <summary>$1C87: PLM BlockIndex, word-strided across forty physical slots.</summary>
    public const int PlmBlockIndex = 0x1c87;
    /// <summary>$1CD7: PLM PreInstruction, word-strided across forty physical slots.</summary>
    public const int PlmPreInstruction = 0x1cd7;
    /// <summary>$1D27: PLM InstructionPointer, word-strided across forty physical slots.</summary>
    public const int PlmInstructionPointer = 0x1d27;
    /// <summary>$1D77: PLM LoopTimer, word-strided across forty physical slots.</summary>
    public const int PlmLoopTimer = 0x1d77;
    /// <summary>$1DC7: PLM RoomArgument, word-strided across forty physical slots.</summary>
    public const int PlmRoomArgument = 0x1dc7;
    /// <summary>$DE1C: PLM InstructionTimer, word-strided across forty physical slots.</summary>
    public const int PlmInstructionTimer = 0xde1c;
    /// <summary>$1E17: PLM family variable; grey-door condition table byte offset.</summary>
    public const int PlmFamilyVariable = 0x1e17;
    /// <summary>$DF0C: extra PLM variable; grey-door hit counter.</summary>
    public const int PlmExtraVariable = 0xdf0c;
    /// <summary>$DEBC: PLM LinkInstruction, word-strided across forty physical slots.</summary>
    public const int PlmLinkInstruction = 0xdebc;
    /// <summary>$2000: Ridley TailFunctionIndex, native bank-$A6 controller state.</summary>
    public const int RidleyTailFunctionIndex = 0x2000;
    /// <summary>$2002: Ridley IdleTailWhipEnabled, native bank-$A6 controller state.</summary>
    public const int RidleyIdleTailWhipEnabled = 0x2002;
    /// <summary>$2004: Ridley TailWhipRequest, native bank-$A6 controller state.</summary>
    public const int RidleyTailWhipRequest = 0x2004;
    /// <summary>$2012: Ridley TailExtensionSpeed, native bank-$A6 controller state.</summary>
    public const int RidleyTailExtensionSpeed = 0x2012;
    /// <summary>$2014: Ridley TailAngleDelta, native bank-$A6 controller state.</summary>
    public const int RidleyTailAngleDelta = 0x2014;
    /// <summary>$2016: Ridley TailMinimumClockwiseAngle, native bank-$A6 controller state.</summary>
    public const int RidleyTailMinimumClockwiseAngle = 0x2016;
    /// <summary>$2018: Ridley TailMaximumCounterClockwiseAngle, native bank-$A6 controller state.</summary>
    public const int RidleyTailMaximumCounterClockwiseAngle = 0x2018;
    /// <summary>$201A: Ridley TailWhipTargetClockwiseAngle, native bank-$A6 controller state.</summary>
    public const int RidleyTailWhipTargetClockwiseAngle = 0x201a;
    /// <summary>$201C: Ridley TailWhipTargetCounterClockwiseAngle, native bank-$A6 controller state.</summary>
    public const int RidleyTailWhipTargetCounterClockwiseAngle = 0x201c;
    /// <summary>$201E: Ridley IdealInterSegmentTailAngle, native bank-$A6 controller state.</summary>
    public const int RidleyIdealInterSegmentTailAngle = 0x201e;
    /// <summary>$0FAA: Ridley HorizontalVelocity, native bank-$A6 controller state.</summary>
    public const int RidleyHorizontalVelocity = 0x0faa;
    /// <summary>$0FAC: Ridley VerticalVelocity, native bank-$A6 controller state.</summary>
    public const int RidleyVerticalVelocity = 0x0fac;
    /// <summary>$7802: Ridley FightMode, native bank-$A6 controller state.</summary>
    public const int RidleyFightMode = 0x7802;
    /// <summary>$7804: Ridley MovementAnimationEnabled, native bank-$A6 controller state.</summary>
    public const int RidleyMovementAnimationEnabled = 0x7804;
    /// <summary>$780E: Ridley WingFrame, native bank-$A6 controller state.</summary>
    public const int RidleyWingFrame = 0x780e;
    /// <summary>$7810: Ridley WingAnimationTimerDelta, native bank-$A6 controller state.</summary>
    public const int RidleyWingAnimationTimerDelta = 0x7810;
    /// <summary>$7812: Ridley WingAnimationTimer, native bank-$A6 controller state.</summary>
    public const int RidleyWingAnimationTimer = 0x7812;
    /// <summary>$7820: Ridley FacingDirection, native bank-$A6 controller state.</summary>
    public const int RidleyFacingDirection = 0x7820;
    /// <summary>$7824: Ridley HealthStage, native bank-$A6 controller state.</summary>
    public const int RidleyHealthStage = 0x7824;
    /// <summary>$7828: Ridley GrabXOffset, native bank-$A6 controller state.</summary>
    public const int RidleyGrabXOffset = 0x7828;
    /// <summary>$782A: Ridley GrabYOffset, native bank-$A6 controller state.</summary>
    public const int RidleyGrabYOffset = 0x782a;
    /// <summary>$7838: Ridley TailDamage, native bank-$A6 controller state.</summary>
    public const int RidleyTailDamage = 0x7838;
    /// <summary>$783A: Ridley FeetDistanceIndex, native bank-$A6 controller state.</summary>
    public const int RidleyFeetDistanceIndex = 0x783a;
    /// <summary>$783C: Ridley IntangibilityTimer, native bank-$A6 controller state.</summary>
    public const int RidleyIntangibilityTimer = 0x783c;
    /// <summary>$7E:2020: seven Ridley tail segment records, each ten words.</summary>
    public const int TailSegments = 0x2020;
    /// <summary>Native Ridley tail record stride in bytes.</summary>
    public const int TailSegmentStride = 20;
    /// <summary>$A6:CC1E tail activation uses the high bit of each record's first word.</summary>
    public const ushort TailSegmentActive = 0x8000;

    /// <summary>$19BB: enemy projectile Graphics, word-strided per slot.</summary>
    public const int EnemyProjectileGraphics = 0x19bb;
    /// <summary>$19DF: enemy projectile Timer, word-strided per slot.</summary>
    public const int EnemyProjectileTimer = 0x19df;
    /// <summary>$1A03: enemy projectile PreInstruction, word-strided per slot.</summary>
    public const int EnemyProjectilePreInstruction = 0x1a03;
    /// <summary>$1A27: enemy projectile XFraction, word-strided per slot.</summary>
    public const int EnemyProjectileXFraction = 0x1a27;
    /// <summary>$1A6F: enemy projectile YFraction, word-strided per slot.</summary>
    public const int EnemyProjectileYFraction = 0x1a6f;
    /// <summary>$1AB7: enemy projectile XVelocity, word-strided per slot.</summary>
    public const int EnemyProjectileXVelocity = 0x1ab7;
    /// <summary>$1ADB: enemy projectile YVelocity, word-strided per slot.</summary>
    public const int EnemyProjectileYVelocity = 0x1adb;
    /// <summary>$1B47: enemy projectile Instruction, word-strided per slot.</summary>
    public const int EnemyProjectileInstruction = 0x1b47;
    /// <summary>$1B6B: enemy projectile Spritemap, word-strided per slot.</summary>
    public const int EnemyProjectileSpritemap = 0x1b6b;
    /// <summary>$1B8F: enemy projectile InstructionTimer, word-strided per slot.</summary>
    public const int EnemyProjectileInstructionTimer = 0x1b8f;
    /// <summary>$0B8C: ordinary projectile XFraction, word-strided per slot.</summary>
    public const int ProjectileXFraction = 0x0b8c;
    /// <summary>$0BA0: ordinary projectile YFraction, word-strided per slot.</summary>
    public const int ProjectileYFraction = 0x0ba0;
    /// <summary>$0BDC: ordinary projectile XVelocity, word-strided per slot.</summary>
    public const int ProjectileXVelocity = 0x0bdc;
    /// <summary>$0BF0: ordinary projectile YVelocity, word-strided per slot.</summary>
    public const int ProjectileYVelocity = 0x0bf0;
    /// <summary>$0C04: ordinary projectile Direction, word-strided per slot.</summary>
    public const int ProjectileDirection = 0x0c04;
    /// <summary>$0C40: ordinary projectile Instruction, word-strided per slot.</summary>
    public const int ProjectileInstruction = 0x0c40;
    /// <summary>$0C54: ordinary projectile InstructionTimer, word-strided per slot.</summary>
    public const int ProjectileInstructionTimer = 0x0c54;
    /// <summary>$0C7C: ordinary projectile Variable, word-strided per slot.</summary>
    public const int ProjectileVariable = 0x0c7c;
    /// <summary>$0C90: ordinary projectile TrailTimer, word-strided per slot.</summary>
    public const int ProjectileTrailTimer = 0x0c90;
    /// <summary>$0CA4: ordinary projectile AuxiliaryPhase, word-strided per slot.</summary>
    public const int ProjectileAuxiliaryPhase = 0x0ca4;
    /// <summary>$0CB8: ordinary projectile Spritemap, word-strided per slot.</summary>
    public const int ProjectileSpritemap = 0x0cb8;
    /// <summary>$1962: lava_acid_y_pos, the collision/damage surface.</summary>
    public const int AcidSurface = 0x1962;
    /// <summary>$1970: fx_y_suboffset, low word of the tidal displacement.</summary>
    public const int TideOffsetFraction = 0x1970;
    /// <summary>$1972: fx_y_offset, high word of the tidal displacement.</summary>
    public const int TideOffset = 0x1972;
    /// <summary>$1974: tide_phase, advanced by the signed negative-cosine half-cycle.</summary>
    public const int TidePhase = 0x1974;
    /// <summary>$1976: fx_base_y_subpos, fraction retained under the tide.</summary>
    public const int LiquidBaseFraction = 0x1976;
    /// <summary>$090F: layer-one X subposition used by horizontal scrolling.</summary>
    public const int CameraXFraction = 0x090f;
    /// <summary>$0913: layer-one Y subposition used by vertical scrolling.</summary>
    public const int CameraYFraction = 0x0913;
    /// <summary>$7E:7800: Ridley independent swoop phase timer.</summary>
    public const int RidleySwoopTimer = 0x7800;
    /// <summary>$09E4: native Moonwalk option preserved by the movie snapshot.</summary>
    public const int MoonwalkOption = 0x09e4;
    /// <summary>$099C: bank-$82 door-transition dispatcher, used to align completed loading owners.</summary>
    public const int DoorFunction = 0x099c;
    /// <summary>$82:E3C0: PlaceSamusLoadTiles; decompression overlaps IRQ movement.</summary>
    public const ushort PlaceSamusLoadTiles = 0xe3c0;
    /// <summary>$82:E4A9: LoadMoreThings; initializes destination owners and waits for scrolling.</summary>
    public const ushort LoadMoreThings = 0xe4a9;
    /// <summary>$82:E659: HandleAnimTiles; destination loading and its coroutine have completed.</summary>
    public const ushort HandleAnimTiles = 0xe659;
    /// <summary>$A6:E546: Ridley's first displayed instruction after the E737 fade owner runs.</summary>
    public const ushort RidleyFirstFadeInstruction = 0xe546;
    /// <summary>$A6:E9A5: native Ridley body sprite installed on the first fade update.</summary>
    public const ushort RidleyFirstFadeSpritemap = 0xe9a5;
    /// <summary>$0797: door_transition_flag_enemies; gates the shared Ridley reveal wait.</summary>
    public const int EnemyDoorTransition = 0x0797;
    /// <summary>$0FA8: Ridley's slot-zero var_A, the native AI function.</summary>
    public const int RidleyFunction = 0x0fa8;
    /// <summary>$0FB2: Ridley's slot-zero var_F, the native function timer.</summary>
    public const int RidleyFunctionTimer = 0x0fb2;
    /// <summary>$A6:E967: sleep cursor retained while right-facing Ridley already faces room middle.</summary>
    public const ushort RidleyRightFlyingSleep = 0xe967;
    /// <summary>$20A4: tilemap_stuff[82], solved Ridley tail tip X.</summary>
    public const int TailTipX = 0x20a4;
    /// <summary>$20A6: tilemap_stuff[83], solved Ridley tail tip Y.</summary>
    public const int TailTipY = 0x20a6;
    /// <summary>$0B64: projectile_x_pos, native ordinary projectile X array.</summary>
    public const int ProjectileX = 0x0b64;
    /// <summary>$0B78: projectile_y_pos, native ordinary projectile Y array.</summary>
    public const int ProjectileY = 0x0b78;
    /// <summary>$0BB4: projectile_x_radius.</summary>
    public const int ProjectileXRadius = 0x0bb4;
    /// <summary>$0BC8: projectile_y_radius.</summary>
    public const int ProjectileYRadius = 0x0bc8;
    /// <summary>$0C18: projectile_type.</summary>
    public const int ProjectileType = 0x0c18;
    /// <summary>$0C2C: projectile_damage.</summary>
    public const int ProjectileDamage = 0x0c2c;
    /// <summary>$0CCC: shared beam/bomb cooldown.</summary>
    public const int ProjectileCooldown = 0x0ccc;
    /// <summary>$0CD0: beam flare/charge counter.</summary>
    public const int BeamCharge = 0x0cd0;
    /// <summary>$1997: enemy projectile identities.</summary>
    public const int EnemyProjectileId = 0x1997;
    /// <summary>$1A4B: enemy projectile X array.</summary>
    public const int EnemyProjectileX = 0x1a4b;
    /// <summary>$1A93: enemy projectile Y array.</summary>
    public const int EnemyProjectileY = 0x1a93;
    /// <summary>$1BB3: packed enemy projectile radii.</summary>
    public const int EnemyProjectileRadius = 0x1bb3;
    /// <summary>$1BD7: enemy projectile properties; low twelve bits hold damage.</summary>
    public const int EnemyProjectileProperties = 0x1bd7;
    /// <summary>$079B: room_ptr.</summary>
    public const int Room = 0x079b;
    /// <summary>$0911: layer1_x_pos.</summary>
    public const int CameraX = 0x0911;
    /// <summary>$0915: layer1_y_pos.</summary>
    public const int CameraY = 0x0915;
    /// <summary>$09A2: equipped_items; imported without granting extra equipment.</summary>
    public const int Items = 0x09a2;
    /// <summary>$09A6: equipped_beams.</summary>
    public const int Beams = 0x09a6;
    /// <summary>$09C2: samus_health.</summary>
    public const int Health = 0x09c2;
    /// <summary>$09C4: samus_max_health.</summary>
    public const int MaxHealth = 0x09c4;
    /// <summary>$18A8: general Samus damage immunity countdown.</summary>
    public const int InvincibilityTimer = 0x18a8;
    /// <summary>$18AA: Samus knockback countdown.</summary>
    public const int KnockbackTimer = 0x18aa;
    /// <summary>$0A52: Samus knockback direction.</summary>
    public const int KnockbackDirection = 0x0a52;
    /// <summary>$0A54: horizontal knockback direction.</summary>
    public const int KnockbackXDirection = 0x0a54;
    /// <summary>$0A48: hurt palette/audio recovery countdown.</summary>
    public const int HurtFlashCounter = 0x0a48;
    /// <summary>$0A4C: fractional health word.</summary>
    public const int SubunitHealth = 0x0a4c;
    /// <summary>$09D2: selected HUD weapon.</summary>
    public const int SelectedHudItem = 0x09d2;
    /// <summary>$0A04: auto-cancel HUD selection.</summary>
    public const int AutoCancelHudItemIndex = 0x0a04;
    /// <summary>$09C0: reserve_health_mode, initial reserve policy.</summary>
    public const int ReserveMode = 0x09c0;
    /// <summary>$09D4: samus_max_reserve_health, collected reserve capacity.</summary>
    public const int MaxReserve = 0x09d4;
    /// <summary>$09D6: samus_reserve_health, available reserve energy.</summary>
    public const int Reserve = 0x09d6;
    /// <summary>$0A1C: samus_pose.</summary>
    public const int Pose = 0x0a1c;
    /// <summary>$0A20: samus_prev_pose.</summary>
    public const int PreviousPose = 0x0a20;
    /// <summary>$0A22: previous direction/movement byte pair.</summary>
    public const int PreviousDirection = 0x0a22;
    /// <summary>$0A24: samus_last_different_pose.</summary>
    public const int LastDifferentPose = 0x0a24;
    /// <summary>$0A26: last-different direction/movement byte pair.</summary>
    public const int LastDifferentDirection = 0x0a26;
    /// <summary>$0A94: samus_anim_frame_timer.</summary>
    public const int AnimationTimer = 0x0a94;
    /// <summary>$0A96: samus_anim_frame.</summary>
    public const int Animation = 0x0a96;
    /// <summary>$0AF6: samus_x_pos.</summary>
    public const int X = 0x0af6;
    /// <summary>$0AF8: samus_x_subpos.</summary>
    public const int XFraction = 0x0af8;
    /// <summary>$0AFA: samus_y_pos.</summary>
    public const int Y = 0x0afa;
    /// <summary>$0AFC: samus_y_subpos.</summary>
    public const int YFraction = 0x0afc;
    /// <summary>$0B2C: samus_y_subspeed.</summary>
    public const int VerticalFraction = 0x0b2c;
    /// <summary>$0B2E: samus_y_speed.</summary>
    public const int VerticalSpeed = 0x0b2e;
    /// <summary>$0B36: samus_y_dir.</summary>
    public const int VerticalDirection = 0x0b36;
    /// <summary>$0B3C: samus_has_momentum_flag.</summary>
    public const int Momentum = 0x0b3c;
    /// <summary>$0B3E: speed_boost_counter.</summary>
    public const int BoostCounter = 0x0b3e;
    /// <summary>$0B42: samus_x_extra_run_speed.</summary>
    public const int ExtraSpeed = 0x0b42;
    /// <summary>$0B44: samus_x_extra_run_subspeed.</summary>
    public const int ExtraFraction = 0x0b44;
    /// <summary>$0B46: samus_x_base_speed.</summary>
    public const int BaseSpeed = 0x0b46;
    /// <summary>$0B48: samus_x_base_subspeed.</summary>
    public const int BaseFraction = 0x0b48;
    /// <summary>$0B4A: samus_x_accel_mode.</summary>
    public const int AccelerationMode = 0x0b4a;
    /// <summary>$D870: persistent collected-item bits, including previously collected items.</summary>
    public const int CollectedItemBits = 0xd870;
    /// <summary>$7F:0002: decompressed foreground collision words.</summary>
    public const int Level = 0x10002;
    /// <summary>$7F:6402: decompressed BTS bytes.</summary>
    public const int Bts = 0x16402;
    /// <summary>$05E5: random_number, imported from the original movie.</summary>
    public const int Random = 0x5e5;
    /// <summary>$05B6: nmi_frame_counter_word, imported from the original movie.</summary>
    public const int NmiCounter = 0x5b6;
    /// <summary>$05B5: nmi_frame_counter_byte, imported from the original movie.</summary>
    public const int NmiCounterByte = 0x5b5;
    /// <summary>$008B: joypad1_lastkeys, imported from the original movie.</summary>
    public const int HeldInput = 0x8b;
    /// <summary>$09A4: collected_items, imported from the original movie.</summary>
    public const int CollectedItems = 0x9a4;
    /// <summary>$09A8: collected_beams, imported from the original movie.</summary>
    public const int CollectedBeams = 0x9a8;
    /// <summary>$09C6: samus_missiles, imported from the original movie.</summary>
    public const int Missiles = 0x9c6;
    /// <summary>$09C8: samus_max_missiles, imported from the original movie.</summary>
    public const int MaxMissiles = 0x9c8;
    /// <summary>$09CA: samus_super_missiles, imported from the original movie.</summary>
    public const int SuperMissiles = 0x9ca;
    /// <summary>$09CC: samus_max_super_missiles, imported from the original movie.</summary>
    public const int MaxSuperMissiles = 0x9cc;
    /// <summary>$09CE: samus_power_bombs, imported from the original movie.</summary>
    public const int PowerBombs = 0x9ce;
    /// <summary>$09D0: samus_max_power_bombs, imported from the original movie.</summary>
    public const int MaxPowerBombs = 0x9d0;
    /// <summary>$D828: boss_bits_for_area, imported from the original movie.</summary>
    public const int BossBits = 0xd828;
    /// <summary>$D820: events_that_happened, imported from the original movie.</summary>
    public const int Events = 0xd820;
    /// <summary>$D8B0: opened_door_bit_array, imported from the original movie.</summary>
    public const int OpenedDoors = 0xd8b0;
    /// <summary>$18F0: first HDMA object pre-instruction, imported from the original movie.</summary>
    public const int AcidHdmaPreInstruction = 0x18f0;
    /// <summary>$B3B0: bank-$88 lava/acid BG3 pre-instruction, imported from the original movie.</summary>
    public const int AcidHdmaCallback = 0xb3b0;
    /// <summary>$0F78: enemy_data, imported from the original movie.</summary>
    public const int EnemyBase = 0xf78;
    /// <summary>$7800: enemy extra variables, imported from the original movie.</summary>
    public const int EnemyExtra = 0x7800;
    /// <summary>$7802: Brinstar pipe bug variable 01, imported from the original movie.</summary>
    public const int EnemyExtraPreviousAnimation = 0x7802;
    /// <summary>$1C37: plm_header_ptr, imported from the original movie.</summary>
    public const int PlmHeaders = 0x1c37;
    /// <summary>$0998: game_state, imported from the original movie.</summary>
    public const int GameState = 0x998;
    /// <summary>$8F:B37A: room containing the movie's initial snapshot.</summary>
    public const ushort SourceRoom = 0xb37a;
    /// <summary>$8F:B32E: Ridley's room, entered in the supplied movie.</summary>
    public const ushort RidleyRoom = 0xb32e;
    /// <summary>$80:9459: ReadControllerInput, accepted-NMI input checkpoint.</summary>
    public const int ReadControllerInput = 0x809459;
    /// <summary>$B3:8A25: strong pipe-bug visual cursor in native update 157.</summary>
    public const ushort PipeBugBeforeFadeInstruction = 0x8a25;
    /// <summary>$B3:8A29: strong pipe-bug visual cursor after native update 158.</summary>
    public const ushort PipeBugAfterFadeInstruction = 0x8a29;
    /// <summary>$B3:8A89: strong pipe-bug spritemap before the first source fade step.</summary>
    public const ushort PipeBugBeforeFadeSpritemap = 0x8a89;
    /// <summary>$B3:8A90: strong pipe-bug spritemap after the first source fade step.</summary>
    public const ushort PipeBugAfterFadeSpritemap = 0x8a90;
}
