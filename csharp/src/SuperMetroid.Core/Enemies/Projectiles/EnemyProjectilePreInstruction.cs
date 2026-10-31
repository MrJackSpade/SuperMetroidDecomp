namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Bank-$86 enemy-projectile pre-instructions the shared projectile pass dispatches; Tourian unlock effects are handled before it.</summary>
internal enum EnemyProjectilePreInstruction
{
    /// <summary>No pre-instruction is installed.</summary>
    None,

    /// <summary>
    /// <c>InitAI_PreInstruction_EnemyProjectile_PrePhantoonRoom</c> at $86:A3A3: zeroes
    /// BG2YOffset ($0923) as both the initialization and the per-frame pre-instruction.
    /// </summary>
        PrePhantoonRoom,

    /// <summary><c>RTS_868170</c> at $86:8170. The common cleared-pre-instruction RTS.</summary>
        RTS_868170,

    /// <summary><c>RTS_86A327</c> at $86:A327. Gunship liftoff dust clouds move only through their frame lists.</summary>
        RTS_86A327,

    /// <summary><c>RTS_8684FB</c> at $86:84FB. Collision handler's common inert pre-instruction.</summary>
        RTS_8684FB,

    /// <summary><c>RTS_86EC94</c> at $86:EC94. Yapping Maw body links are positioned entirely by bank-$A8 main AI.</summary>
        RTS_86EC94,

    /// <summary><c>RTS_86D0EB</c> at $86:D0EB. Kago bug startup/landed no-op.</summary>
        RTS_86D0EB,

    /// <summary>
    /// <c>RTS_86CFF7</c> at $86:CFF7: the acid spit header's idle pre-instruction, a bare
    /// RTS until the instruction list installs a start-moving callback.
    /// </summary>
        RTS_86CFF7,

    /// <summary>
    /// <c>RTS_86B4B0</c> at $86:B4B0: the old Tourian escape shaft fake-wall explosion's
    /// pre-instruction, a bare RTS its list clears on the first tick.
    /// </summary>
        RTS_86B4B0,

    /// <summary><c>RTS_868D54</c> at $86:8D54. Draygon wall turret charges before its list enables flight.</summary>
        RTS_868D54,

    /// <summary><c>RTS_86950C</c> at $86:950C. Center afterburn is stationary while its instruction list blooms.</summary>
        RTS_86950C,

    /// <summary><c>RTS_869A44</c> at $86:9A44. Phantoon casual/rain flame resting RTS.</summary>
        RTS_869A44,

    /// <summary><c>RTS_86BBC6</c> at $86:BBC6. Nuclear Waffle body: position is owned by bank-$A6 main AI.</summary>
        RTS_86BBC6,

    /// <summary><c>RTS_86A05B</c> at $86:A05B. Pirate laser startup: three muzzle-flash frames do not move.</summary>
        RTS_86A05B,

    /// <summary><c>RTS_86EFDF</c> at $86:EFDF. Enemy death/pickup subsystem's empty pre-instruction.</summary>
        RTS_86EFDF,

    /// <summary><c>RTS_86A919</c> at $86:A919. Bomb Torizo explosive swipe: stationary authored hit flash.</summary>
        RTS_86A919,

    /// <summary><c>PreInstruction_BombTorizoStatueFragment_Stopped</c> at $86:A918. Bomb Torizo statue fragment stopped after floor collision.</summary>
        BombTorizoStatueFragment_Stopped,

    /// <summary><c>RTS_86DD44</c> at $86:DD44. Spore Spawn stalk: position is written by the boss's main AI.</summary>
        RTS_86DD44,

    /// <summary><c>RTS_86CAA3</c> at $86:CAA3. Mother Brain's large purple breath is a stationary animation.</summary>
        RTS_86CAA3,

    /// <summary><c>RTS_86C76D</c> at $86:C76D. Mother Brain's charging/fired red hand-beam list owns all motion.</summary>
        RTS_86C76D,

    /// <summary><c>RTS_86E6D1</c> at $86:E6D1. Save-station electricity is stationary after its PLM-relative initializer.</summary>
        RTS_86E6D1,

    /// <summary>Initializer-selected no-op pre-instruction at <c>$86:E604</c>.</summary>
        DownwardGateInertPreInstruction,

    /// <summary><c>$86:E508</c>, native <c>RTS_86E508</c>: no-op callback for PLM dust/smoke; also used by the translated eye-door attack and sweat actors to suspend motion while their impact instructions finish.</summary>
        EyeDoorSmokeInertPreInstruction,

    /// <summary>Signed 8.8 vertical gate movement pre-instruction at <c>$86:E605</c>.</summary>
        DownwardGateMovementPreInstruction,

    /// <summary><c>$86:B6B9</c>, native <c>PreInstruction_EnemyProjectile_EyeDoorProjectile_Moving</c>: moves with block collision, accumulates angle-selected acceleration, and selects the explosion sequence on impact or an opened door bit.</summary>
        EyeDoorProjectilePreInstruction,

    /// <summary><c>$86:B714</c>, native <c>PreInstruction_EnemyProjectile_EyeDoorSweat</c>: moves the sweat drop, adds <c>$000C</c> to its 8.8 Y velocity per update, and selects its impact sequence on a downward floor collision.</summary>
        EyeDoorSweatPreInstruction,

    /// <summary><c>PreInst_EnemyProjectile_BombTorizoChozoBreaking_Falling</c> at $86:A8EF. Bomb Torizo hand fragment: fall until room collision.</summary>
        BombTorizoChozoBreaking_Falling,

    /// <summary><c>PreInstruction_EnemyProjectile_Pickup</c> at $86:EFE0.</summary>
        Pickup,

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsTurrets</c> at $86:BFDF. Mother Brain room turret: rotate, fire, or honor deletion flag.</summary>
        MotherBrainsTurrets,

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsTurretBullets</c> at $86:C0E0. Mother Brain turret bullet: flicker, move, and hit non-air blocks.</summary>
        MotherBrainsTurretBullets,

    /// <summary><c>PreInstruction_EnemyProj_MotherBrainGlassShattering_Shard</c> at $86:CE9B. Mother Brain glass shard: 8.8 flight, gravity, and sparkles.</summary>
        MotherBrainGlassShattering_Shard,

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsTubeFalling</c> at $86:CBE7. Mother Brain ceiling tubes: dust once, then accelerate downward.</summary>
        MotherBrainsTubeFalling,

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsDrool</c> at $86:C84D. Mother Brain drool remains attached for its first five maps.</summary>
        MotherBrainsDrool,

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsDrool_Falling</c> at $86:C886. Released drool accelerates down until the fixed arena floor.</summary>
        MotherBrainsDrool_Falling,

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsOnionRings</c> at $86:C335. Delayed mouth pin, flight, custom collision, and arena cull.</summary>
        MotherBrainsOnionRings,

    /// <summary><c>PreInstruction_EnemyProjectile_MotherBrainsBomb</c> at $86:C4C8. Mother Brain bomb: Samus-bomb scan, gravity, and staged bounces.</summary>
        MotherBrainsBomb,

    /// <summary><c>PreInst_EnemyProjectile_MotherBrainRainbowBeam_Charging</c> at $86:C814. Rainbow charge contracts around the live articulated brain slot.</summary>
        MotherBrainRainbowBeam_Charging,

    /// <summary><c>PreInstruction_EnemyProj_MotherBrainsRainbowBeamExplosion</c> at $86:C94C. Rainbow impact sprites retain their offset from moving Samus.</summary>
        MotherBrainsRainbowBeamExplosion,

    /// <summary>$86:C914, reattaches a death explosion to the current body position each frame.</summary>
        MotherBrainDeathExplosionPreInstruction,

    /// <summary>$86:C9D2, fragment drag, gravity, and thirty-three-call lifetime.</summary>
        MotherBrainDeathDoorFragmentPreInstruction,

    /// <summary>$86:CAFA pins the subtitle to physical screen coordinates.</summary>
        MotherBrainDeathSubtitlePreInstruction,

    /// <summary><c>PreInstruction_DraygonGoop_StuckToSamus</c> at $86:8DCA. Draygon goop: attached to Samus with a 256-frame lifetime.</summary>
        DraygonGoop_StuckToSamus,

    /// <summary><c>PreInstruction_EnemyProj_DraygonsWallTurretProjectile_Fired</c> at $86:8DFF. Draygon wall turret: aimed full-precision flight and room cull.</summary>
        DraygonsWallTurretProjectile_Fired,

    /// <summary><c>PreInstruction_EnemyProjectile_DraygonGoop</c> at $86:8E0F. Draygon goop: flight, proximity-triggered attach list, room cull.</summary>
        DraygonGoop,

    /// <summary><c>PreInstruction_EnemyProjectile_Spores</c> at $86:DCEE. Spore Spawn spore: ROM-authored two-byte movement stream.</summary>
        Spores,

    /// <summary><c>PreInstruction_EnemyProjectile_SporeSpawner</c> at $86:DD46. Spore Spawn ceiling spawner: randomized closed-phase cadence.</summary>
        SporeSpawner,

    /// <summary><c>PreInstruction_EnemyProjectile_BotwoonsBody</c> at $86:EA80. Botwoon body: orientation, hurt palette, and death dispatcher.</summary>
        BotwoonsBody,

    /// <summary><c>PreInstruction_EnemyProjectile_BotwoonsSpit</c> at $86:EC05. Botwoon spit: full 16.16 vector followed by strict camera cull.</summary>
        BotwoonsSpit,

    /// <summary><c>PreInstruction_EnemyProjectile_BombTorizosChozoOrbs</c> at $86:ACAD. Bomb Torizo Chozo orb: room collision followed by gravity.</summary>
        BombTorizosChozoOrbs,

    /// <summary><c>PreInstruction_EnemyProjectile_GoldenTorizosChozoOrbs</c> at $86:ACFA. Golden Torizo Chozo orb: wall bounce and damped floor bounce.</summary>
        GoldenTorizosChozoOrbs,

    /// <summary><c>PreInstruction_EnemyProjectile_TorizoSonicBoom</c> at $86:AE6C. Both Torizos' sonic boom: accelerating horizontal room shot.</summary>
        TorizoSonicBoom,

    /// <summary><c>PreInst_EnemyProjectile_BombTorizoLowHealthDrool_Falling</c> at $86:A887. Bomb Torizo drool: drag, gravity, and room-impact lists.</summary>
        BombTorizoLowHealthDrool_Falling,

    /// <summary><c>PreInstruction_EnemyProjectile_GoldenTorizoEgg_Bouncing</c> at $86:B043. Golden Torizo egg: timed bounce followed by horizontal launch.</summary>
        GoldenTorizoEgg_Bouncing,

    /// <summary><c>PreInstruction_EnemyProjectile_GoldenTorizoEgg_Hatched</c> at $86:B0B9. Golden Torizo egg: accelerate toward a wall.</summary>
        GoldenTorizoEgg_Hatched,

    /// <summary><c>PreInstruction_EnemyProjectile_GoldenTorizoEgg_HitWall</c> at $86:B0DD. Golden Torizo egg: fall to the floor and hatch/impact.</summary>
        GoldenTorizoEgg_HitWall,

    /// <summary><c>PreInstruction_EnemyProjectile_GoldenTorizoSuperMissile_Held</c> at $86:B20D. Held Golden Torizo super missile follows the hand joint.</summary>
        GoldenTorizoSuperMissile_Held,

    /// <summary><c>PreInst_EnemyProjectile_GoldenTorizoSuperMissile_Thrown</c> at $86:B237. Thrown Golden Torizo super missile: gravity and room impact.</summary>
        GoldenTorizoSuperMissile_Thrown,

    /// <summary><c>PreInstruction_EnemyProjectile_GoldenTorizoEyeBeam</c> at $86:B38A. Golden Torizo eye beam: room collision impact lists.</summary>
        GoldenTorizoEyeBeam,

    /// <summary><c>PreInst_EnemyProj_TourianStatueBaseDecoration_AllowProcess</c> at $86:BA37. Tourian entrance statue actors follow the statue's per-line vertical reveal.</summary>
        TourianStatueBaseDecoration_AllowProcess,

    /// <summary><c>PreInst_EnemyProj_TourianStatue_Ridley_Phantoon_BaseDecor</c> at $86:BA42. Shared position-only tail used after the finished flag is set.</summary>
        TourianStatue_Ridley_Phantoon_BaseDecor,

    /// <summary><c>PreInstruction_EnemyProjectile_ShaktoolsAttack_Front</c> at $86:BE03. Unused Shaktool front circle: independent X/Y room collision.</summary>
        ShaktoolsAttack_Front,

    /// <summary><c>PreInst_EnemyProjectile_ShaktoolsAttack_MiddleBack_Moving</c> at $86:BE12. Unused middle/back circles live only while their owner slot does.</summary>
        ShaktoolsAttack_MiddleBack_Moving,

    /// <summary><c>PreInstruction_EnemyProjectile_MiscDust</c> at $86:E4FE. Generic room-coordinate dust/explosion camera cull.</summary>
        MiscDust,

    /// <summary><c>PreInstruction_EnemyProjectile_DragonFireball</c> at $86:B535. Dragon fireball: signed 8.8 arc, gravity, and bottom-only cull.</summary>
        DragonFireball,

    /// <summary><c>PreInstruction_EnemyProjectile_RidleyFireball</c> at $86:940E.</summary>
        RidleyFireball,

    /// <summary><c>$86:D7BF</c>: flicker the n00b-tube crack actor.</summary>
        NoobTubeCrackFlickering,

    /// <summary><c>$86:D7DE</c>: move the detached n00b-tube crack downward.</summary>
        NoobTubeCrackFalling,

    /// <summary><c>$86:D7FD</c>: move a newly emitted n00b-tube shard.</summary>
        NoobTubeShardFlying,

    /// <summary><c>$86:D83D</c>: rotate and fall after a n00b-tube shard's initial flight.</summary>
        NoobTubeShardFalling,

    /// <summary><c>$86:D89F</c>: rotate and fall a released-air bubble.</summary>
        NoobTubeBubbleFalling,

    /// <summary><c>$86:D8DF</c>: move a released-air bubble vertically.</summary>
        NoobTubeBubbleFlying,

    /// <summary><c>PreInstruction_EnemyProjectile_HorizontalAfterburn</c> at $86:950D. $86:950D first uses the raw 8.8 horizontal adder, not the room-collision</summary>
        HorizontalAfterburn,

    /// <summary><c>PreInstruction_EnemyProjectile_VerticalAfterburn</c> at $86:9522. $86:9522 is the transposed path: unrestricted vertical travel followed</summary>
        VerticalAfterburn,

    /// <summary><c>PreInstruction_EnemyProjectile_MetalSkreeParticle</c> at $86:8B5D. Skree particle: signed 8.8 movement, gravity, camera deletion.</summary>
        MetalSkreeParticle,

    /// <summary><c>PreInstruction_EnemyProjectile_CrocomiresProjectile_Setup</c> at $86:906B. Crocomire projectile: derive the fired vector after one setup move.</summary>
        CrocomiresProjectile_Setup,

    /// <summary><c>PreInstruction_EnemyProjectile_CrocomiresProjectile_Fired</c> at $86:90B3. Crocomire projectile: X then Y collision deletes the actor.</summary>
        CrocomiresProjectile_Fired,

    /// <summary><c>PreInstruction_EnemyProjectile_CrocomireSpikeWallPieces</c> at $86:9115. Crocomire spike wall: slot-specific acceleration and fall.</summary>
        CrocomireSpikeWallPieces,

    /// <summary><c>PreInstruction_EnemyProjectile_KraidRocks</c> at $86:9D56. Kraid spat/floor rocks: X/Y collision, drag, and gravity.</summary>
        KraidRocks,

    /// <summary><c>PreInstruction_EnemyProjectile_KraidCeilingRocks</c> at $86:9D89. Kraid ceiling rocks: vertical collision and masked gravity.</summary>
        KraidCeilingRocks,

    /// <summary><c>PreInstruction_EnemyProjectile_KraidRockSpit_UsePalette0</c> at $86:9DA5. Shot Kraid spit rock switches to common explosion palette zero.</summary>
        KraidRockSpit_UsePalette0,

    /// <summary><c>PreInstruction_EnemyProjectile_PhantoonStartingFlames</c> at $86:9B29. Phantoon intro flame: wait for the body activation word.</summary>
        PhantoonStartingFlames,

    /// <summary><c>PreInst_EnemyProjectile_PhantoonStartingFlames_Activated</c> at $86:9B41. Phantoon intro flame: orbit while its radius contracts.</summary>
        PhantoonStartingFlames_Activated,

    /// <summary><c>PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_Falling</c> at $86:9981. Phantoon casual flame: fall until the first terrain impact.</summary>
        PhantoonDestroyableFlame_Casual_Falling,

    /// <summary><c>PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_HitGround</c> at $86:99BF. Phantoon casual flame: eight-frame impact pause.</summary>
        PhantoonDestroyableFlame_Casual_HitGround,

    /// <summary><c>PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_Bouncing</c> at $86:9A01. Phantoon casual flame: two diminishing terrain bounces.</summary>
        PhantoonDestroyableFlame_Casual_Bouncing,

    /// <summary><c>PreInst_EnemyProj_PhantoonDestroyableFlame_Enraged</c> at $86:9A45. Phantoon rage flame: expanding orbit around the body.</summary>
        PhantoonDestroyableFlame_Enraged,

    /// <summary><c>PreInst_EnemyProj_PhantoonDestroyableFlame_Rain</c> at $86:9A94. Phantoon flame rain: delayed fall and terrain impact.</summary>
        PhantoonDestroyableFlame_Rain,

    /// <summary><c>PreInst_EnemyProj_PhantoonDestroyableFlame_Spiral</c> at $86:9ADA. Phantoon spiral: rotating expansion around the body.</summary>
        PhantoonDestroyableFlame_Spiral,

    /// <summary><c>PreInstruction_EnemyProjectile_CrocomireBridgeCrumbling</c> at $86:92BA. Crocomire bridge fragment: gravity until room collision.</summary>
        CrocomireBridgeCrumbling,

    /// <summary><c>PreInstruction_EnemyProjectile_AlcoonFireball</c> at $86:9EFF. Alcoon fireball: Y then X collision, followed by horizontal drag.</summary>
        AlcoonFireball,

    /// <summary><c>PreInstruction_EnemyProj_KiHunterAcidSpit_Moving</c> at $86:CFF8.</summary>
        KiHunterAcid_Moving,

    /// <summary><c>PreInstruction_EnemyProjectile_KiHunterAcid_Left</c> at $86:CFD5.</summary>
        KiHunterAcid_Left,

    /// <summary><c>PreInstruction_EnemyProjectile_KiHunterAcid_Right</c> at $86:CFE6.</summary>
        KiHunterAcid_Right,

    /// <summary><c>PreInstruction_EnemyProjectile_PowampSpike</c> at $86:D263. Powamp spike: radial acceleration and X-then-Y room collision.</summary>
        PowampSpike,

    /// <summary><c>PreInstruction_EnemyProjectile_WreckedShipRobotLaser</c> at $86:D3BF. Work Robot laser: clear graphics index, then X/Y room collision.</summary>
        WreckedShipRobotLaser,

    /// <summary><c>PreInstruction_EnemyProjectile_StokeFireball</c> at $86:DB5B.</summary>
        StokeFireball,

    /// <summary><c>PreInstruction_EnemyProjectile_CacatacSpike</c> at $86:D9DB.</summary>
        CacatacSpike,

    /// <summary><c>PreInstruction_EnemyProjectile_PolypRock</c> at $86:BC0F.</summary>
        PolypRock,

    /// <summary><c>PreInstruction_EnemyProjectile_NamiFuneFireball</c> at $86:DF39.</summary>
        NamiFuneFireball,

    /// <summary><c>PreInstruction_EnemyProjectile_MagdolliteLava</c> at $86:E049.</summary>
        MagdolliteLava,

    /// <summary><c>PreInstruction_EnemyProjectile_KagoBug_Idle</c> at $86:D0CA.</summary>
        KagoBug_Idle,

    /// <summary><c>PreInstruction_EnemyProjectile_KagoBug_Jumping</c> at $86:D0EC.</summary>
        KagoBug_Jumping,

    /// <summary><c>PreInstruction_EnemyProjectile_KagoBug_Falling</c> at $86:D128.</summary>
        KagoBug_Falling,

    /// <summary><c>PreInstruction_EnemyProjectile_FallingSpark</c> at $86:F3F0. Spark projectile: 16.16 gravity, floor bounce, and trail objects.</summary>
        FallingSpark,

    /// <summary><c>PreInstruction_EnemyProjectile_CeresFallingTile</c> at $86:9701. Ceres falling tile: accelerating descent and impact cloud.</summary>
        CeresFallingTile,

    /// <summary><c>PreInstruction_EnemyProjectile_MiniKraidSpit</c> at $86:9E1E. Fake Kraid spit: X/Y room collision, then capped gravity.</summary>
        MiniKraidSpit,

    /// <summary><c>PreInstruction_EnemyProjectile_MiniKraidSpikes</c> at $86:9E83. Fake Kraid spike: horizontal motion until wall contact.</summary>
        MiniKraidSpikes,

    /// <summary><c>PreInstruction_EnemyProjectile_Pirate_MotherBrain_Laser_Left</c> at $86:A05C. Space Pirate/Mother Brain laser: move left, then camera cull.</summary>
        Pirate_MotherBrain_Laser_Left,

    /// <summary><c>PreInst_EnemyProjectile_Pirate_MotherBrain_Laser_Right</c> at $86:A07A. Space Pirate/Mother Brain laser: move right, then camera cull.</summary>
        Pirate_MotherBrain_Laser_Right,

    /// <summary><c>PreInstruction_EnemyProjectile_PirateClaw_Left</c> at $86:A0D1. Ninja Space Pirate claw: thrown left, then returns right.</summary>
        PirateClaw_Left,

    /// <summary><c>PreInstruction_EnemyProjectile_PirateClaw_Right</c> at $86:A124. Ninja Space Pirate claw: thrown right, then returns left.</summary>
        PirateClaw_Right,

    /// <summary><c>PreInstruction_EnemyProjectile_TourianStatueUnlockingSplash</c> at $86:B977: the splash holds at the water surface.</summary>
        TourianStatueSplashMotion,

    /// <summary><c>PreInstruction_EnemyProjectile_TourianStatueUnlockingParticle</c> at $86:B982: a particle falls and splashes.</summary>
        TourianStatueParticleMotion,

    /// <summary><c>PreInstruction_EnemyProjectile_TourianStatueSoul</c> at $86:B9FD: a soul rises out of the room.</summary>
        TourianStatueSoulMotion,
}
