namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$86 instruction and pre-instruction entry points for enemy projectiles.</summary>
internal static class EnemyProjectileCodePointers
{
    /// <summary><c>RTS_86A327</c> at $86:A327. Gunship liftoff dust clouds move only through their frame lists.</summary>
    public const ushort RTS_86A327 = 0xa327;

    /// <summary><c>RTS_8684FB</c> at $86:84FB. Collision handler's common inert pre-instruction.</summary>
    public const ushort RTS_8684FB = 0x84fb;

    /// <summary><c>RTS_86EC94</c> at $86:EC94. Yapping Maw body links are positioned entirely by bank-$A8 main AI.</summary>
    public const ushort RTS_86EC94 = 0xec94;

    /// <summary><c>RTS_86D0EB</c> at $86:D0EB. Kago bug startup/landed no-op.</summary>
    public const ushort RTS_86D0EB = 0xd0eb;

    /// <summary><c>RTS_868D54</c> at $86:8D54. Draygon wall turret charges before its list enables flight.</summary>
    public const ushort RTS_868D54 = 0x8d54;

    /// <summary><c>RTS_86950C</c> at $86:950C. Center afterburn is stationary while its instruction list blooms.</summary>
    public const ushort RTS_86950C = 0x950c;

    /// <summary><c>RTS_869A44</c> at $86:9A44. Phantoon casual/rain flame resting RTS.</summary>
    public const ushort RTS_869A44 = 0x9a44;

    /// <summary><c>RTS_86BBC6</c> at $86:BBC6. Nuclear Waffle body: position is owned by bank-$A6 main AI.</summary>
    public const ushort RTS_86BBC6 = 0xbbc6;

    /// <summary><c>RTS_86A05B</c> at $86:A05B. Pirate laser startup: three muzzle-flash frames do not move.</summary>
    public const ushort RTS_86A05B = 0xa05b;

    /// <summary><c>RTS_86EFDF</c> at $86:EFDF. Enemy death/pickup subsystem's empty pre-instruction.</summary>
    public const ushort RTS_86EFDF = 0xefdf;

    /// <summary><c>RTS_86A919</c> at $86:A919. Bomb Torizo explosive swipe: stationary authored hit flash.</summary>
    public const ushort RTS_86A919 = 0xa919;

    /// <summary><c>RTS_86DD44</c> at $86:DD44. Spore Spawn stalk: position is written by the boss's main AI.</summary>
    public const ushort RTS_86DD44 = 0xdd44;

    /// <summary><c>RTS_86CAA3</c> at $86:CAA3. Mother Brain's large purple breath is a stationary animation.</summary>
    public const ushort RTS_86CAA3 = 0xcaa3;

    /// <summary><c>RTS_86C76D</c> at $86:C76D. Mother Brain's charging/fired red hand-beam list owns all motion.</summary>
    public const ushort RTS_86C76D = 0xc76d;

    /// <summary><c>PreInst_EnemyProjectile_BombTorizoChozoBreaking_Falling</c> at $86:A8EF. Bomb Torizo hand fragment: fall until room collision.</summary>
    public const ushort PreInst_EnemyProjectile_BombTorizoChozoBreaking_Falling = 0xa8ef;

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsTurrets</c> at $86:BFDF. Mother Brain room turret: rotate, fire, or honor deletion flag.</summary>
    public const ushort PreInstruction_EnemyProjectile_MotherBrainsTurrets = 0xbfdf;

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsTurretBullets</c> at $86:C0E0. Mother Brain turret bullet: flicker, move, and hit non-air blocks.</summary>
    public const ushort PreInstruction_EnemyProjectile_MotherBrainsTurretBullets = 0xc0e0;

    /// <summary><c>PreInstruction_EnemyProj_MotherBrainGlassShattering_Shard</c> at $86:CE9B. Mother Brain glass shard: 8.8 flight, gravity, and sparkles.</summary>
    public const ushort PreInstruction_EnemyProj_MotherBrainGlassShattering_Shard = 0xce9b;

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsTubeFalling</c> at $86:CBE7. Mother Brain ceiling tubes: dust once, then accelerate downward.</summary>
    public const ushort PreInstruction_EnemyProjectile_MotherBrainsTubeFalling = 0xcbe7;

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsDrool</c> at $86:C84D. Mother Brain drool remains attached for its first five maps.</summary>
    public const ushort PreInstruction_EnemyProjectile_MotherBrainsDrool = 0xc84d;

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsDrool_Falling</c> at $86:C886. Released drool accelerates down until the fixed arena floor.</summary>
    public const ushort PreInstruction_EnemyProjectile_MotherBrainsDrool_Falling = 0xc886;

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsOnionRings</c> at $86:C335. Delayed mouth pin, flight, custom collision, and arena cull.</summary>
    public const ushort PreInstruction_EnemyProjectile_MotherBrainsOnionRings = 0xc335;

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsBomb</c> at $86:C4C8. Mother Brain bomb: Samus-bomb scan, gravity, and staged bounces.</summary>
    public const ushort PreInstruction_EnemyProjectile_MotherBrainsBomb = 0xc4c8;

    /// <summary><c>PreInst_EnemyProjectile_MotherBrainRainbowBeam_Charging</c> at $86:C814. Rainbow charge contracts around the live articulated brain slot.</summary>
    public const ushort PreInst_EnemyProjectile_MotherBrainRainbowBeam_Charging = 0xc814;

    /// <summary><c>PreInstruction_EnemyProj_MotherBrainsRainbowBeamExplosion</c> at $86:C94C. Rainbow impact sprites retain their offset from moving Samus.</summary>
    public const ushort PreInstruction_EnemyProj_MotherBrainsRainbowBeamExplosion = 0xc94c;

    /// <summary><c>PreInstruction_DraygonGoop_StuckToSamus</c> at $86:8DCA. Draygon goop: attached to Samus with a 256-frame lifetime.</summary>
    public const ushort PreInstruction_DraygonGoop_StuckToSamus = 0x8dca;

    /// <summary><c>PreInstruction_EnemyProj_DraygonsWallTurretProjectile_Fired</c> at $86:8DFF. Draygon wall turret: aimed full-precision flight and room cull.</summary>
    public const ushort PreInstruction_EnemyProj_DraygonsWallTurretProjectile_Fired = 0x8dff;

    /// <summary><c>PreInstruction_EnemyProjectile_DraygonGoop</c> at $86:8E0F. Draygon goop: flight, proximity-triggered attach list, room cull.</summary>
    public const ushort PreInstruction_EnemyProjectile_DraygonGoop = 0x8e0f;

    /// <summary><c>PreInstruction_EnemyProjectile_Spores</c> at $86:DCEE. Spore Spawn spore: ROM-authored two-byte movement stream.</summary>
    public const ushort PreInstruction_EnemyProjectile_Spores = 0xdcee;

    /// <summary><c>PreInstruction_EnemyProjectile_SporeSpawner</c> at $86:DD46. Spore Spawn ceiling spawner: randomized closed-phase cadence.</summary>
    public const ushort PreInstruction_EnemyProjectile_SporeSpawner = 0xdd46;

    /// <summary><c>PreInstruction_EnemyProjectile_BotwoonsBody</c> at $86:EA80. Botwoon body: orientation, hurt palette, and death dispatcher.</summary>
    public const ushort PreInstruction_EnemyProjectile_BotwoonsBody = 0xea80;

    /// <summary><c>PreInstruction_EnemyProjectile_BotwoonsSpit</c> at $86:EC05. Botwoon spit: full 16.16 vector followed by strict camera cull.</summary>
    public const ushort PreInstruction_EnemyProjectile_BotwoonsSpit = 0xec05;

    /// <summary><c>PreInstruction_EnemyProjectile_BombTorizosChozoOrbs</c> at $86:ACAD. Bomb Torizo Chozo orb: room collision followed by gravity.</summary>
    public const ushort PreInstruction_EnemyProjectile_BombTorizosChozoOrbs = 0xacad;

    /// <summary><c>PreInstruction_EnemyProjectile_GoldenTorizosChozoOrbs</c> at $86:ACFA. Golden Torizo Chozo orb: wall bounce and damped floor bounce.</summary>
    public const ushort PreInstruction_EnemyProjectile_GoldenTorizosChozoOrbs = 0xacfa;

    /// <summary><c>PreInstruction_EnemyProjectile_TorizoSonicBoom</c> at $86:AE6C. Both Torizos' sonic boom: accelerating horizontal room shot.</summary>
    public const ushort PreInstruction_EnemyProjectile_TorizoSonicBoom = 0xae6c;

    /// <summary><c>PreInst_EnemyProjectile_BombTorizoLowHealthDrool_Falling</c> at $86:A887. Bomb Torizo drool: drag, gravity, and room-impact lists.</summary>
    public const ushort PreInst_EnemyProjectile_BombTorizoLowHealthDrool_Falling = 0xa887;

    /// <summary><c>PreInstruction_EnemyProjectile_GoldenTorizoEgg_Bouncing</c> at $86:B043. Golden Torizo egg: timed bounce followed by horizontal launch.</summary>
    public const ushort PreInstruction_EnemyProjectile_GoldenTorizoEgg_Bouncing = 0xb043;

    /// <summary><c>PreInstruction_EnemyProjectile_GoldenTorizoEgg_Hatched</c> at $86:B0B9. Golden Torizo egg: accelerate toward a wall.</summary>
    public const ushort PreInstruction_EnemyProjectile_GoldenTorizoEgg_Hatched = 0xb0b9;

    /// <summary><c>PreInstruction_EnemyProjectile_GoldenTorizoEgg_HitWall</c> at $86:B0DD. Golden Torizo egg: fall to the floor and hatch/impact.</summary>
    public const ushort PreInstruction_EnemyProjectile_GoldenTorizoEgg_HitWall = 0xb0dd;

    /// <summary><c>PreInstruction_EnemyProjectile_GoldenTorizoSuperMissile_Held</c> at $86:B20D. Held Golden Torizo super missile follows the hand joint.</summary>
    public const ushort PreInstruction_EnemyProjectile_GoldenTorizoSuperMissile_Held = 0xb20d;

    /// <summary><c>PreInst_EnemyProjectile_GoldenTorizoSuperMissile_Thrown</c> at $86:B237. Thrown Golden Torizo super missile: gravity and room impact.</summary>
    public const ushort PreInst_EnemyProjectile_GoldenTorizoSuperMissile_Thrown = 0xb237;

    /// <summary><c>PreInstruction_EnemyProjectile_GoldenTorizoEyeBeam</c> at $86:B38A. Golden Torizo eye beam: room collision impact lists.</summary>
    public const ushort PreInstruction_EnemyProjectile_GoldenTorizoEyeBeam = 0xb38a;

    /// <summary><c>PreInst_EnemyProj_TourianStatueBaseDecoration_AllowProcess</c> at $86:BA37. Tourian entrance statue actors follow the HDMA vertical reveal.</summary>
    public const ushort PreInst_EnemyProj_TourianStatueBaseDecoration_AllowProcess = 0xba37;

    /// <summary><c>PreInst_EnemyProj_TourianStatue_Ridley_Phantoon_BaseDecor</c> at $86:BA42. Shared position-only tail used after the finished flag is set.</summary>
    public const ushort PreInst_EnemyProj_TourianStatue_Ridley_Phantoon_BaseDecor = 0xba42;

    /// <summary><c>PreInstruction_EnemyProjectile_ShaktoolsAttack_Front</c> at $86:BE03. Unused Shaktool front circle: independent X/Y room collision.</summary>
    public const ushort PreInstruction_EnemyProjectile_ShaktoolsAttack_Front = 0xbe03;

    /// <summary><c>PreInst_EnemyProjectile_ShaktoolsAttack_MiddleBack_Moving</c> at $86:BE12. Unused middle/back circles live only while their owner slot does.</summary>
    public const ushort PreInst_EnemyProjectile_ShaktoolsAttack_MiddleBack_Moving = 0xbe12;

    /// <summary><c>PreInstruction_EnemyProjectile_MiscDust</c> at $86:E4FE. Generic room-coordinate dust/explosion camera cull.</summary>
    public const ushort PreInstruction_EnemyProjectile_MiscDust = 0xe4fe;

    /// <summary><c>PreInstruction_EnemyProjectile_DragonFireball</c> at $86:B535. Dragon fireball: signed 8.8 arc, gravity, and bottom-only cull.</summary>
    public const ushort PreInstruction_EnemyProjectile_DragonFireball = 0xb535;

    /// <summary><c>PreInstruction_EnemyProjectile_RidleyFireball</c> at $86:940E.</summary>
    public const ushort PreInstruction_EnemyProjectile_RidleyFireball = 0x940e;

    /// <summary><c>PreInstruction_EnemyProjectile_HorizontalAfterburn</c> at $86:950D. $86:950D first uses the raw 8.8 horizontal adder, not the room-collision</summary>
    public const ushort PreInstruction_EnemyProjectile_HorizontalAfterburn = 0x950d;

    /// <summary><c>PreInstruction_EnemyProjectile_VerticalAfterburn</c> at $86:9522. $86:9522 is the transposed path: unrestricted vertical travel followed</summary>
    public const ushort PreInstruction_EnemyProjectile_VerticalAfterburn = 0x9522;

    /// <summary><c>PreInstruction_EnemyProjectile_MetalSkreeParticle</c> at $86:8B5D. Skree particle: signed 8.8 movement, gravity, camera deletion.</summary>
    public const ushort PreInstruction_EnemyProjectile_MetalSkreeParticle = 0x8b5d;

    /// <summary><c>PreInstruction_EnemyProjectile_CrocomiresProjectile_Setup</c> at $86:906B. Crocomire projectile: derive the fired vector after one setup move.</summary>
    public const ushort PreInstruction_EnemyProjectile_CrocomiresProjectile_Setup = 0x906b;

    /// <summary><c>PreInstruction_EnemyProjectile_CrocomiresProjectile_Fired</c> at $86:90B3. Crocomire projectile: X then Y collision deletes the actor.</summary>
    public const ushort PreInstruction_EnemyProjectile_CrocomiresProjectile_Fired = 0x90b3;

    /// <summary><c>PreInstruction_EnemyProjectile_CrocomireSpikeWallPieces</c> at $86:9115. Crocomire spike wall: slot-specific acceleration and fall.</summary>
    public const ushort PreInstruction_EnemyProjectile_CrocomireSpikeWallPieces = 0x9115;

    /// <summary><c>PreInstruction_EnemyProjectile_KraidRocks</c> at $86:9D56. Kraid spat/floor rocks: X/Y collision, drag, and gravity.</summary>
    public const ushort PreInstruction_EnemyProjectile_KraidRocks = 0x9d56;

    /// <summary><c>PreInstruction_EnemyProjectile_KraidCeilingRocks</c> at $86:9D89. Kraid ceiling rocks: vertical collision and masked gravity.</summary>
    public const ushort PreInstruction_EnemyProjectile_KraidCeilingRocks = 0x9d89;

    /// <summary><c>PreInstruction_EnemyProjectile_KraidRockSpit_UsePalette0</c> at $86:9DA5. Shot Kraid spit rock switches to common explosion palette zero.</summary>
    public const ushort PreInstruction_EnemyProjectile_KraidRockSpit_UsePalette0 = 0x9da5;

    /// <summary><c>PreInstruction_EnemyProjectile_PhantoonStartingFlames</c> at $86:9B29. Phantoon intro flame: wait for the body activation word.</summary>
    public const ushort PreInstruction_EnemyProjectile_PhantoonStartingFlames = 0x9b29;

    /// <summary><c>PreInst_EnemyProjectile_PhantoonStartingFlames_Activated</c> at $86:9B41. Phantoon intro flame: orbit while its radius contracts.</summary>
    public const ushort PreInst_EnemyProjectile_PhantoonStartingFlames_Activated = 0x9b41;

    /// <summary><c>PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_Falling</c> at $86:9981. Phantoon casual flame: fall until the first terrain impact.</summary>
    public const ushort PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_Falling = 0x9981;

    /// <summary><c>PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_HitGround</c> at $86:99BF. Phantoon casual flame: eight-frame impact pause.</summary>
    public const ushort PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_HitGround = 0x99bf;

    /// <summary><c>PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_Bouncing</c> at $86:9A01. Phantoon casual flame: two diminishing terrain bounces.</summary>
    public const ushort PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_Bouncing = 0x9a01;

    /// <summary><c>PreInst_EnemyProj_PhantoonDestroyableFlame_Enraged</c> at $86:9A45. Phantoon rage flame: expanding orbit around the body.</summary>
    public const ushort PreInst_EnemyProj_PhantoonDestroyableFlame_Enraged = 0x9a45;

    /// <summary><c>PreInst_EnemyProj_PhantoonDestroyableFlame_Rain</c> at $86:9A94. Phantoon flame rain: delayed fall and terrain impact.</summary>
    public const ushort PreInst_EnemyProj_PhantoonDestroyableFlame_Rain = 0x9a94;

    /// <summary><c>PreInst_EnemyProj_PhantoonDestroyableFlame_Spiral</c> at $86:9ADA. Phantoon spiral: rotating expansion around the body.</summary>
    public const ushort PreInst_EnemyProj_PhantoonDestroyableFlame_Spiral = 0x9ada;

    /// <summary><c>PreInstruction_EnemyProjectile_CrocomireBridgeCrumbling</c> at $86:92BA. Crocomire bridge fragment: gravity until room collision.</summary>
    public const ushort PreInstruction_EnemyProjectile_CrocomireBridgeCrumbling = 0x92ba;

    /// <summary><c>PreInstruction_EnemyProjectile_AlcoonFireball</c> at $86:9EFF. Alcoon fireball: Y then X collision, followed by horizontal drag.</summary>
    public const ushort PreInstruction_EnemyProjectile_AlcoonFireball = 0x9eff;

    /// <summary><c>PreInstruction_EnemyProjectile_PowampSpike</c> at $86:D263. Powamp spike: radial acceleration and X-then-Y room collision.</summary>
    public const ushort PreInstruction_EnemyProjectile_PowampSpike = 0xd263;

    /// <summary><c>PreInstruction_EnemyProjectile_WreckedShipRobotLaser</c> at $86:D3BF. Work Robot laser: clear graphics index, then X/Y room collision.</summary>
    public const ushort PreInstruction_EnemyProjectile_WreckedShipRobotLaser = 0xd3bf;

    /// <summary><c>PreInstruction_EnemyProjectile_FallingSpark</c> at $86:F3F0. Spark projectile: 16.16 gravity, floor bounce, and trail objects.</summary>
    public const ushort PreInstruction_EnemyProjectile_FallingSpark = 0xf3f0;

    /// <summary><c>PreInstruction_EnemyProjectile_CeresFallingTile</c> at $86:9701. Ceres falling tile: accelerating descent and impact cloud.</summary>
    public const ushort PreInstruction_EnemyProjectile_CeresFallingTile = 0x9701;

    /// <summary><c>PreInstruction_EnemyProjectile_MiniKraidSpit</c> at $86:9E1E. Fake Kraid spit: X/Y room collision, then capped gravity.</summary>
    public const ushort PreInstruction_EnemyProjectile_MiniKraidSpit = 0x9e1e;

    /// <summary><c>PreInstruction_EnemyProjectile_MiniKraidSpikes</c> at $86:9E83. Fake Kraid spike: horizontal motion until wall contact.</summary>
    public const ushort PreInstruction_EnemyProjectile_MiniKraidSpikes = 0x9e83;

    /// <summary><c>PreInstruction_EnemyProjectile_Pirate_MotherBrain_Laser_Left</c> at $86:A05C. Space Pirate/Mother Brain laser: move left, then camera cull.</summary>
    public const ushort PreInstruction_EnemyProjectile_Pirate_MotherBrain_Laser_Left = 0xa05c;

    /// <summary><c>PreInst_EnemyProjectile_Pirate_MotherBrain_Laser_Right</c> at $86:A07A. Space Pirate/Mother Brain laser: move right, then camera cull.</summary>
    public const ushort PreInst_EnemyProjectile_Pirate_MotherBrain_Laser_Right = 0xa07a;

    /// <summary><c>PreInstruction_EnemyProjectile_PirateClaw_Left</c> at $86:A0D1. Ninja Space Pirate claw: thrown left, then returns right.</summary>
    public const ushort PreInstruction_EnemyProjectile_PirateClaw_Left = 0xa0d1;

    /// <summary><c>PreInstruction_EnemyProjectile_PirateClaw_Right</c> at $86:A124. Ninja Space Pirate claw: thrown right, then returns left.</summary>
    public const ushort PreInstruction_EnemyProjectile_PirateClaw_Right = 0xa124;

    /// <summary><c>Instruction_EnemyProjectile_Delete</c> at $86:8154. Delete.</summary>
    public const ushort Instruction_EnemyProjectile_Delete = 0x8154;

    /// <summary><c>Instruction_EnemyProjectile_Properties_OrY</c> at $86:8230. OR packed projectile properties with one literal word.</summary>
    public const ushort Instruction_EnemyProjectile_Properties_OrY = 0x8230;

    /// <summary><c>Instruction_EnemyProjectile_Properties_AndY</c> at $86:823C. AND packed projectile properties with one literal word.</summary>
    public const ushort Instruction_EnemyProjectile_Properties_AndY = 0x823c;

    /// <summary><c>UNUSED_Inst_EnemyProj_EnableCollisionWithSamusProj_868248</c> at $86:8248. Enable collision with Samus projectiles.</summary>
    public const ushort UNUSED_Inst_EnemyProj_EnableCollisionWithSamusProj_868248 = 0x8248;

    /// <summary><c>Instruction_EnemyProjectile_DisableCollisionWIthSamusProj</c> at $86:8252. Disable collision with Samus projectiles.</summary>
    public const ushort Instruction_EnemyProjectile_DisableCollisionWIthSamusProj = 0x8252;

    /// <summary><c>Instruction_EnemyProjectile_DisableCollisionWithSamus</c> at $86:825C. Disable collision with Samus.</summary>
    public const ushort Instruction_EnemyProjectile_DisableCollisionWithSamus = 0x825c;

    /// <summary><c>UNUSED_Inst_EnemyProjectile_EnableCollisionWithSamus_868266</c> at $86:8266. Enable collision with Samus.</summary>
    public const ushort UNUSED_Inst_EnemyProjectile_EnableCollisionWithSamus_868266 = 0x8266;

    /// <summary><c>UNUSED_Inst_EnemyProjectile_SetToNotDieOnContact_868270</c> at $86:8270. Retain actor after Samus contact.</summary>
    public const ushort UNUSED_Inst_EnemyProjectile_SetToNotDieOnContact_868270 = 0x8270;

    /// <summary><c>UNUSED_Instruction_EnemyProjectile_SetToDieOnContact_86827A</c> at $86:827A. Delete actor after Samus contact.</summary>
    public const ushort UNUSED_Instruction_EnemyProjectile_SetToDieOnContact_86827A = 0x827a;

    /// <summary><c>Instruction_EnemyProjectile_SetHighPriority</c> at $86:8284. Set low OAM priority; draw queue priority is not split yet.</summary>
    public const ushort Instruction_EnemyProjectile_SetHighPriority = 0x8284;

    /// <summary><c>UNUSED_Instruction_EnemyProjectile_SetLowPriority_86828E</c> at $86:828E. Set high OAM priority.</summary>
    public const ushort UNUSED_Instruction_EnemyProjectile_SetLowPriority_86828E = 0x828e;

    /// <summary><c>Instruction_EnemyProjectile_XYRadiusInY</c> at $86:8298. Set packed {X,Y} collision radii.</summary>
    public const ushort Instruction_EnemyProjectile_XYRadiusInY = 0x8298;

    /// <summary><c>UNUSED_Instruction_EnemyProjectile_XYRadius_0</c> at $86:82A1. Clear both collision radii.</summary>
    public const ushort UNUSED_Instruction_EnemyProjectile_XYRadius_0 = 0x82a1;

    /// <summary><c>UNUSED_Instruction_EnemyProjectile_QueueMusicTrackInY</c> at $86:82FD. Queue music with eight-frame delay; one-byte operand.</summary>
    public const ushort UNUSED_Instruction_EnemyProjectile_QueueMusicTrackInY = 0x82fd;

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max6_868309</c> at $86:8309. Queue SFX library 1, maximum 6.</summary>
    public const ushort UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max6_868309 = 0x8309;

    /// <summary><c>Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6</c> at $86:8312. Queue SFX library 2, maximum 6.</summary>
    public const ushort Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6 = 0x8312;

    /// <summary><c>Instruction_EnemyProjectile_QueueSoundInY_Lib3_Max6</c> at $86:831B. Queue SFX library 3, maximum 6.</summary>
    public const ushort Instruction_EnemyProjectile_QueueSoundInY_Lib3_Max6 = 0x831b;

    /// <summary><c>Instruction_EnemyProjectile_QueueSoundInY_Lib1_Max15</c> at $86:8324. Queue SFX library 1, maximum 15.</summary>
    public const ushort Instruction_EnemyProjectile_QueueSoundInY_Lib1_Max15 = 0x8324;

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max15_86832D</c> at $86:832D. Queue SFX library 2, maximum 15.</summary>
    public const ushort UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max15_86832D = 0x832d;

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib3_Max15_868336</c> at $86:8336. Queue SFX library 3, maximum 15.</summary>
    public const ushort UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib3_Max15_868336 = 0x8336;

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max3_86833F</c> at $86:833F. Queue SFX library 1, maximum 3.</summary>
    public const ushort UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max3_86833F = 0x833f;

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max3_868348</c> at $86:8348. Queue SFX library 2, maximum 3.</summary>
    public const ushort UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max3_868348 = 0x8348;

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib3_Max3_868351</c> at $86:8351. Queue SFX library 3, maximum 3.</summary>
    public const ushort UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib3_Max3_868351 = 0x8351;

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max9_86835A</c> at $86:835A. Queue SFX library 1, maximum 9.</summary>
    public const ushort UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max9_86835A = 0x835a;

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max9_868363</c> at $86:8363. Queue SFX library 2, maximum 9.</summary>
    public const ushort UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max9_868363 = 0x8363;

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max9_86836C</c> at $86:836C. Queue SFX library 3, maximum 9.</summary>
    public const ushort UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max9_86836C = 0x836c;

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max1_868375</c> at $86:8375. Queue SFX library 1, maximum 1.</summary>
    public const ushort UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max1_868375 = 0x8375;

    /// <summary><c>Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max1</c> at $86:837E. Queue SFX library 2, maximum 1.</summary>
    public const ushort Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max1 = 0x837e;

    /// <summary><c>Instruction_EnemyProjectile_QueueSoundInY_Lib3_Max1</c> at $86:8387. Queue SFX library 3, maximum 1.</summary>
    public const ushort Instruction_EnemyProjectile_QueueSoundInY_Lib3_Max1 = 0x8387;

    /// <summary><c>Instruction_EnemyProjectile_Sleep</c> at $86:8159. Sleep forever while pre-instruction movement remains active.</summary>
    public const ushort Instruction_EnemyProjectile_Sleep = 0x8159;

    /// <summary><c>Instruction_EnemyProjectile_PreInstructionInY</c> at $86:8161. Install the operand as pre-instruction.</summary>
    public const ushort Instruction_EnemyProjectile_PreInstructionInY = 0x8161;

    /// <summary><c>Instruction_EnemyProjectile_ClearPreInstruction</c> at $86:816A. Clear pre-instruction to $8170 RTS.</summary>
    public const ushort Instruction_EnemyProjectile_ClearPreInstruction = 0x816a;

    /// <summary><c>Instruction_EnemyProjectile_CallExternalFunctionInY</c> at $86:8171. Call the following 24-bit external function.</summary>
    public const ushort Instruction_EnemyProjectile_CallExternalFunctionInY = 0x8171;

    /// <summary><c>Instruction_SetPreInst_DraygonsWallTurretProjectile_Fired</c> at $86:8CF6.</summary>
    public const ushort Instruction_SetPreInst_DraygonsWallTurretProjectile_Fired = 0x8cf6;

    /// <summary><c>Instruction_DraygonGoop_SamusCollision</c> at $86:8D99.</summary>
    public const ushort Instruction_DraygonGoop_SamusCollision = 0x8d99;

    /// <summary><c>Instruction_EnemyProjectile_GotoY</c> at $86:81AB. Same-bank goto.</summary>
    public const ushort Instruction_EnemyProjectile_GotoY = 0x81ab;

    /// <summary><c>Instruction_EnemyProjectile_GotoY_Y</c> at $86:81B0. Signed-byte same-bank relative goto.</summary>
    public const ushort Instruction_EnemyProjectile_GotoY_Y = 0x81b0;

    /// <summary><c>Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero</c> at $86:81C6. Decrement general timer and take an absolute branch while nonzero.</summary>
    public const ushort Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero = 0x81c6;

    /// <summary><c>UNUSED_Inst_EnemyProj_DecrementTimer_GotoY_YIfNonZero_8681CE</c> at $86:81CE. Decrement general timer and take a signed relative branch while nonzero.</summary>
    public const ushort UNUSED_Inst_EnemyProj_DecrementTimer_GotoY_YIfNonZero_8681CE = 0x81ce;

    /// <summary><c>Instruction_EnemyProjectile_TimerInY</c> at $86:81D5. Initialize the independent general-purpose loop timer.</summary>
    public const ushort Instruction_EnemyProjectile_TimerInY = 0x81d5;

    /// <summary><c>RTS_8681DE</c> at $86:81DE. Deliberate entry at the RTS immediately before $81DF.</summary>
    public const ushort RTS_8681DE = 0x81de;

    /// <summary><c>Instruction_MoveRandomlyWithinXRadius_YRadius</c> at $86:81DF. Randomly offset the actor inside authored X/Y radii.</summary>
    public const ushort Instruction_MoveRandomlyWithinXRadius_YRadius = 0x81df;

    /// <summary><c>Instruction_PreInstructionInY_ExecuteY</c> at $86:A050. Pirate laser: install operand as pre-instruction and run it.</summary>
    public const ushort Instruction_PreInstructionInY_ExecuteY = 0xa050;

    /// <summary><c>Instruction_EnemyProjectile_Torizo_ResetPosition</c> at $86:A3BE. Restore X/Y saved by the sonic-boom collision pre-instruction.</summary>
    public const ushort Instruction_EnemyProjectile_Torizo_ResetPosition = 0xa3be;

    /// <summary><c>UNUSED_Instruction_EnemyProj_MoveHorizontally_GotoY_86AD92</c> at $86:AD92. Move X, then choose one of two lists from velocity sign.</summary>
    public const ushort UNUSED_Instruction_EnemyProj_MoveHorizontally_GotoY_86AD92 = 0xad92;

    /// <summary><c>Instruction_EnemyProjectile_GoldenTorizoEgg_GoToHatched</c> at $86:B13E. Golden Torizo egg: select left/right terminal list.</summary>
    public const ushort Instruction_EnemyProjectile_GoldenTorizoEgg_GoToHatched = 0xb13e;

    /// <summary><c>Instruction_AimSuperMissile_Rightwards</c> at $86:B269. Golden Torizo super missile: velocity toward Samus.</summary>
    public const ushort Instruction_AimSuperMissile_Rightwards = 0xb269;

    /// <summary><c>Instruction_AimSuperMissile_Leftwards</c> at $86:B272. Golden Torizo super missile: velocity away from Samus.</summary>
    public const ushort Instruction_AimSuperMissile_Leftwards = 0xb272;

    /// <summary><c>Instruction_EnemyProjectile_GotoYIfEyeBeamExplosionsDisabled</c> at $86:B3B8. Golden Torizo eye beam: branch while attack flag is clear.</summary>
    public const ushort Instruction_EnemyProjectile_GotoYIfEyeBeamExplosionsDisabled = 0xb3b8;

    /// <summary><c>UNUSED_Instruction_ResetPosition_86B436</c> at $86:B436. Restore X/Y saved in generic projectile variables E/F.</summary>
    public const ushort UNUSED_Instruction_ResetPosition_86B436 = 0xb436;

    /// <summary><c>Instruction_EnemyProjectile_MotherBrainsTurretBullets_GotoY</c> at $86:C173. The bullet initializer stores direction * 2 in variable E. The ROM</summary>
    public const ushort Instruction_EnemyProjectile_MotherBrainsTurretBullets_GotoY = 0xc173;

    /// <summary><c>Instruction_EnemyProjectile_UsePalette0</c> at $86:C1B4. First instruction of the shared touch/shot smoke sequence.</summary>
    public const ushort Instruction_EnemyProjectile_UsePalette0 = 0xc1b4;

    /// <summary><c>Instruction_EnemyProj_MotherBrainsDrool_MoveDownCPixels</c> at $86:C8D0. After changing to the falling pre-instruction, the list lowers the</summary>
    public const ushort Instruction_EnemyProj_MotherBrainsDrool_MoveDownCPixels = 0xc8d0;

    /// <summary><c>Instruction_EnemyProjectile_GotoY_Probability_1_4</c> at $86:A456. Take the authored absolute branch with 25-percent probability.</summary>
    public const ushort Instruction_EnemyProjectile_GotoY_Probability_1_4 = 0xa456;

    /// <summary><c>Instruction_EnemyProjectile_SpawnEnemyDropsWIthYDropChances</c> at $86:AB8A. Shot Torizo orb: choose area-specific header/drop table.</summary>
    public const ushort Instruction_EnemyProjectile_SpawnEnemyDropsWIthYDropChances = 0xab8a;

    /// <summary><c>Instruction_Spawn_HorizontalAfterburn_EnemyProjectiles</c> at $86:95BA. Spawn horizontal right/left afterburn pair.</summary>
    public const ushort Instruction_Spawn_HorizontalAfterburn_EnemyProjectiles = 0x95ba;

    /// <summary><c>Instruction_Spawn_VerticalAfterburn_EnemyProjectiles</c> at $86:95ED. Spawn vertical up/down afterburn pair.</summary>
    public const ushort Instruction_Spawn_VerticalAfterburn_EnemyProjectiles = 0x95ed;

    /// <summary><c>Instruction_SpawnNext_Afterburn_EnemyProjectile</c> at $86:9620. Decrement count and spawn the next actor in this direction.</summary>
    public const ushort Instruction_SpawnNext_Afterburn_EnemyProjectile = 0x9620;

    /// <summary><c>Instruction_SpawnPhantoonDrop</c> at $86:980E. Shot Phantoon flame: request a drop from eye header $E4FF.</summary>
    public const ushort Instruction_SpawnPhantoonDrop = 0x980e;

    /// <summary><c>Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_20</c> at $86:ECE3. Random sprite-object position inside a 64x64 square.</summary>
    public const ushort Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_20 = 0xece3;

    /// <summary><c>Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_10</c> at $86:ED17. Random sprite-object position inside a 32x32 square.</summary>
    public const ushort Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_10 = 0xed17;

    /// <summary><c>Instruction_EnemyProj_EnemyDeathExpl_QueueEnemyKilledSoundFX</c> at $86:EE8B. Queue sound 9 in library two; this opcode has no operand.</summary>
    public const ushort Instruction_EnemyProj_EnemyDeathExpl_QueueEnemyKilledSoundFX = 0xee8b;

    /// <summary><c>Instruction_EnemyProj_EDeathExplo_QueueSmallExplosionSoundFX</c> at $86:EE97. Queue sound $24 in library two; no operand.</summary>
    public const ushort Instruction_EnemyProj_EDeathExplo_QueueSmallExplosionSoundFX = 0xee97;

    /// <summary><c>Instruction_EnemyProj_EDeathExplo_QueueContactKilledSoundFX</c> at $86:EEA3. Queue sound $0B in library two; no operand.</summary>
    public const ushort Instruction_EnemyProj_EDeathExplo_QueueContactKilledSoundFX = 0xeea3;

    /// <summary><c>Instruction_EnemyProjectile_Spores_SetProperties3000</c> at $86:DC5A. Spore impact: replace packed properties with literal $3000.</summary>
    public const ushort Instruction_EnemyProjectile_Spores_SetProperties3000 = 0xdc5a;

    /// <summary><c>Instruction_EnemyProjectile_Spores_SpawnEnemyDrops</c> at $86:DC61. Spore impact: request enemy definition $DF3F's drop table.</summary>
    public const ushort Instruction_EnemyProjectile_Spores_SpawnEnemyDrops = 0xdc61;

    /// <summary><c>Instruction_EnemyProjectile_SporeSpawner_SpawnSpore</c> at $86:DC77. Ceiling spawner: allocate one room-graphics spore here.</summary>
    public const ushort Instruction_EnemyProjectile_SporeSpawner_SpawnSpore = 0xdc77;

    /// <summary><c>Instruction_EnemyProjectile_TorizoLandingDustClouds</c> at $86:AF92. Torizo landing-dust instruction: move actor four pixels up.</summary>
    public const ushort Instruction_EnemyProjectile_TorizoLandingDustClouds = 0xaf92;

    /// <summary><c>Instruction_EnemyProjectile_EnemyDeathExplosion_BecomePickup</c> at $86:EEAF. Random drop selection after an enemy death animation.</summary>
    public const ushort Instruction_EnemyProjectile_EnemyDeathExplosion_BecomePickup = 0xeeaf;

    /// <summary><c>Instruction_EnemyProjectile_Pickup_HandleRespawningEnemy</c> at $86:EF10. Respawn the retained physical enemy slot, when bit $8000 is set.</summary>
    public const ushort Instruction_EnemyProjectile_Pickup_HandleRespawningEnemy = 0xef10;

    /// <summary><c>RTS_868170</c> at $86:8170. The common cleared-pre-instruction RTS.</summary>
    public const ushort RTS_868170 = 0x8170;

    /// <summary><c>PreInstruction_BombTorizoStatueFragment_Stopped</c> at $86:A918. Bomb Torizo statue fragment stopped after floor collision.</summary>
    public const ushort PreInstruction_BombTorizoStatueFragment_Stopped = 0xa918;

}
