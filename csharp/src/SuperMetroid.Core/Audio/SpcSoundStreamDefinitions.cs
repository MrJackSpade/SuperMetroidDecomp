namespace SuperMetroid.Core.Audio;

/// <summary>Native SPC instruction-list sets selected by one-based sound commands.</summary>
internal static class SpcSoundStreamDefinitions
{
    /// <summary>SPC library 1 instruction-list identities.</summary>
    internal static class Library1
    {
        /// <summary>SPC $2B71, sound1InstructionLists_sound1; ROM pointer entry $CF96F5. Power bomb explosion</summary>
        internal const ushort Sound01PowerBombExplosion = 0x2b71;
        /// <summary>SPC $2BAF, sound1InstructionLists_sound2; ROM pointer entry $CF96F7. Silence</summary>
        internal const ushort Sound02Silence = 0x2baf;
        /// <summary>SPC $2BB7, sound1InstructionLists_sound3; ROM pointer entry $CF96F9. Missile</summary>
        internal const ushort Sound03Missile = 0x2bb7;
        /// <summary>SPC $2BC4, sound1InstructionLists_sound4; ROM pointer entry $CF96FB. Super missile</summary>
        internal const ushort Sound04SuperMissile = 0x2bc4;
        /// <summary>SPC $2BD1, sound1InstructionLists_sound5; ROM pointer entry $CF96FD. Grapple start</summary>
        internal const ushort Sound05GrappleStart = 0x2bd1;
        /// <summary>SPC $2BFC, sound1InstructionLists_sound6; ROM pointer entry $CF96FF. Grappling</summary>
        internal const ushort Sound06Grappling = 0x2bfc;
        /// <summary>SPC $2C2F, sound1InstructionLists_sound7; ROM pointer entry $CF9701. Grapple end</summary>
        internal const ushort Sound07GrappleEnd = 0x2c2f;
        /// <summary>SPC $2C37, sound1InstructionLists_sound8; ROM pointer entry $CF9703. Charging beam</summary>
        internal const ushort Sound08ChargingBeam = 0x2c37;
        /// <summary>SPC $2CFF, sound1InstructionLists_sound9; ROM pointer entry $CF9705. X-ray</summary>
        internal const ushort Sound09XRay = 0x2cff;
        /// <summary>SPC $2D12, sound1InstructionLists_soundA; ROM pointer entry $CF9707. X-ray end</summary>
        internal const ushort Sound0AXRayEnd = 0x2d12;
        /// <summary>SPC $2D1A, sound1InstructionLists_soundB; ROM pointer entry $CF9709. Uncharged power beam</summary>
        internal const ushort Sound0BUnchargedPowerBeam = 0x2d1a;
        /// <summary>SPC $2D27, sound1InstructionLists_soundC; ROM pointer entry $CF970B. Uncharged ice beam</summary>
        internal const ushort Sound0CUnchargedIceBeam = 0x2d27;
        /// <summary>SPC $2D4B, sound1InstructionLists_soundD; ROM pointer entry $CF970D. Uncharged wave beam</summary>
        internal const ushort Sound0DUnchargedWaveBeam = 0x2d4b;
        /// <summary>SPC $2D5D, sound1InstructionLists_soundE; ROM pointer entry $CF970F. Uncharged ice + wave beam</summary>
        internal const ushort Sound0EUnchargedIceWaveBeam = 0x2d5d;
        /// <summary>SPC $2D5F, sound1InstructionLists_soundF; ROM pointer entry $CF9711. Uncharged spazer beam</summary>
        internal const ushort Sound0FUnchargedSpazerBeam = 0x2d5f;
        /// <summary>SPC $2D76, sound1InstructionLists_sound10; ROM pointer entry $CF9713. Uncharged spazer + ice beam</summary>
        internal const ushort Sound10UnchargedSpazerIceBeam = 0x2d76;
        /// <summary>SPC $2D95, sound1InstructionLists_sound11; ROM pointer entry $CF9715. Uncharged spazer + ice + wave beam</summary>
        internal const ushort Sound11UnchargedSpazerIceWaveBeam = 0x2d95;
        /// <summary>SPC $2D97, sound1InstructionLists_sound12; ROM pointer entry $CF9717. Uncharged spazer + wave beam</summary>
        internal const ushort Sound12UnchargedSpazerWaveBeam = 0x2d97;
        /// <summary>SPC $2D99, sound1InstructionLists_sound13; ROM pointer entry $CF9719. Uncharged plasma beam</summary>
        internal const ushort Sound13UnchargedPlasmaBeam = 0x2d99;
        /// <summary>SPC $2DA6, sound1InstructionLists_sound14; ROM pointer entry $CF971B. Uncharged plasma + ice beam</summary>
        internal const ushort Sound14UnchargedPlasmaIceBeam = 0x2da6;
        /// <summary>SPC $2DA8, sound1InstructionLists_sound15; ROM pointer entry $CF971D. Uncharged plasma + ice + wave beam</summary>
        internal const ushort Sound15UnchargedPlasmaIceWaveBeam = 0x2da8;
        /// <summary>SPC $2DAA, sound1InstructionLists_sound16; ROM pointer entry $CF971F. Uncharged plasma + wave beam</summary>
        internal const ushort Sound16UnchargedPlasmaWaveBeam = 0x2daa;
        /// <summary>SPC $2DAC, sound1InstructionLists_sound17; ROM pointer entry $CF9721. Charged power beam</summary>
        internal const ushort Sound17ChargedPowerBeam = 0x2dac;
        /// <summary>SPC $2DC8, sound1InstructionLists_sound18; ROM pointer entry $CF9723. Charged ice beam</summary>
        internal const ushort Sound18ChargedIceBeam = 0x2dc8;
        /// <summary>SPC $2DE7, sound1InstructionLists_sound19; ROM pointer entry $CF9725. Charged wave beam</summary>
        internal const ushort Sound19ChargedWaveBeam = 0x2de7;
        /// <summary>SPC $2DFE, sound1InstructionLists_sound1A; ROM pointer entry $CF9727. Charged ice + wave beam</summary>
        internal const ushort Sound1AChargedIceWaveBeam = 0x2dfe;
        /// <summary>SPC $2E00, sound1InstructionLists_sound1B; ROM pointer entry $CF9729. Charged spazer beam</summary>
        internal const ushort Sound1BChargedSpazerBeam = 0x2e00;
        /// <summary>SPC $2E17, sound1InstructionLists_sound1C; ROM pointer entry $CF972B. Charged spazer + ice beam</summary>
        internal const ushort Sound1CChargedSpazerIceBeam = 0x2e17;
        /// <summary>SPC $2E19, sound1InstructionLists_sound1D; ROM pointer entry $CF972D. Charged spazer + ice + wave beam</summary>
        internal const ushort Sound1DChargedSpazerIceWaveBeam = 0x2e19;
        /// <summary>SPC $2E1B, sound1InstructionLists_sound1E; ROM pointer entry $CF972F. Charged spazer + wave beam</summary>
        internal const ushort Sound1EChargedSpazerWaveBeam = 0x2e1b;
        /// <summary>SPC $2E1D, sound1InstructionLists_sound1F; ROM pointer entry $CF9731. Charged plasma beam / hyper beam</summary>
        internal const ushort Sound1FChargedPlasmaBeamHyperBeam = 0x2e1d;
        /// <summary>SPC $2E34, sound1InstructionLists_sound20; ROM pointer entry $CF9733. Charged plasma + ice beam</summary>
        internal const ushort Sound20ChargedPlasmaIceBeam = 0x2e34;
        /// <summary>SPC $2E36, sound1InstructionLists_sound21; ROM pointer entry $CF9735. Charged plasma + ice + wave beam</summary>
        internal const ushort Sound21ChargedPlasmaIceWaveBeam = 0x2e36;
        /// <summary>SPC $2E38, sound1InstructionLists_sound22; ROM pointer entry $CF9737. Charged plasma + wave beam</summary>
        internal const ushort Sound22ChargedPlasmaWaveBeam = 0x2e38;
        /// <summary>SPC $2E3A, sound1InstructionLists_sound23; ROM pointer entry $CF9739. Ice SBA</summary>
        internal const ushort Sound23IceSBA = 0x2e3a;
        /// <summary>SPC $2E68, sound1InstructionLists_sound24; ROM pointer entry $CF973B. Ice SBA end</summary>
        internal const ushort Sound24IceSBAEnd = 0x2e68;
        /// <summary>SPC $2EAF, sound1InstructionLists_sound25; ROM pointer entry $CF973D. Spazer SBA</summary>
        internal const ushort Sound25SpazerSBA = 0x2eaf;
        /// <summary>SPC $2ED0, sound1InstructionLists_sound26; ROM pointer entry $CF973F. Spazer SBA end</summary>
        internal const ushort Sound26SpazerSBAEnd = 0x2ed0;
        /// <summary>SPC $2EF6, sound1InstructionLists_sound27; ROM pointer entry $CF9741. Plasma SBA</summary>
        internal const ushort Sound27PlasmaSBA = 0x2ef6;
        /// <summary>SPC $2F2C, sound1InstructionLists_sound28; ROM pointer entry $CF9743. Wave SBA</summary>
        internal const ushort Sound28WaveSBA = 0x2f2c;
        /// <summary>SPC $2F37, sound1InstructionLists_sound29; ROM pointer entry $CF9745. Wave SBA end</summary>
        internal const ushort Sound29WaveSBAEnd = 0x2f37;
        /// <summary>SPC $2F3F, sound1InstructionLists_sound2A; ROM pointer entry $CF9747. Selected save file</summary>
        internal const ushort Sound2ASelectedSaveFile = 0x2f3f;
        /// <summary>SPC $2F47, sound1InstructionLists_sound2B; ROM pointer entry $CF9749. (Empty)</summary>
        internal const ushort Sound2BEmpty = 0x2f47;
        /// <summary>SPC $2F4A, sound1InstructionLists_sound2C; ROM pointer entry $CF974B. (Empty)</summary>
        internal const ushort Sound2CEmpty = 0x2f4a;
        /// <summary>SPC $2F4D, sound1InstructionLists_sound2D; ROM pointer entry $CF974D. (Empty)</summary>
        internal const ushort Sound2DEmpty = 0x2f4d;
        /// <summary>SPC $2F50, sound1InstructionLists_sound2E; ROM pointer entry $CF974F. Saving</summary>
        internal const ushort Sound2ESaving = 0x2f50;
        /// <summary>SPC $2FB0, sound1InstructionLists_sound2F; ROM pointer entry $CF9751. Underwater space jump (without gravity suit)</summary>
        internal const ushort Sound2FUnderwaterSpaceJumpWithoutGravitySuit = 0x2fb0;
        /// <summary>SPC $2FB8, sound1InstructionLists_sound30; ROM pointer entry $CF9753. Resumed spin jump</summary>
        internal const ushort Sound30ResumedSpinJump = 0x2fb8;
        /// <summary>SPC $2FC3, sound1InstructionLists_sound31; ROM pointer entry $CF9755. Spin jump</summary>
        internal const ushort Sound31SpinJump = 0x2fc3;
        /// <summary>SPC $2FDD, sound1InstructionLists_sound32; ROM pointer entry $CF9757. Spin jump end</summary>
        internal const ushort Sound32SpinJumpEnd = 0x2fdd;
        /// <summary>SPC $2FE5, sound1InstructionLists_sound33; ROM pointer entry $CF9759. Screw attack</summary>
        internal const ushort Sound33ScrewAttack = 0x2fe5;
        /// <summary>SPC $3040, sound1InstructionLists_sound34; ROM pointer entry $CF975B. Screw attack end</summary>
        internal const ushort Sound34ScrewAttackEnd = 0x3040;
        /// <summary>SPC $3048, sound1InstructionLists_sound35; ROM pointer entry $CF975D. Samus damaged</summary>
        internal const ushort Sound35SamusDamaged = 0x3048;
        /// <summary>SPC $3055, sound1InstructionLists_sound36; ROM pointer entry $CF975F. Scrolling map</summary>
        internal const ushort Sound36ScrollingMap = 0x3055;
        /// <summary>SPC $305D, sound1InstructionLists_sound37; ROM pointer entry $CF9761. Toggle reserve mode / moved cursor</summary>
        internal const ushort Sound37ToggleReserveModeMovedCursor = 0x305d;
        /// <summary>SPC $3065, sound1InstructionLists_sound38; ROM pointer entry $CF9763. Pause menu transition / toggled equipment</summary>
        internal const ushort Sound38PauseMenuTransitionToggledEquipment = 0x3065;
        /// <summary>SPC $3070, sound1InstructionLists_sound39; ROM pointer entry $CF9765. Switch HUD item</summary>
        internal const ushort Sound39SwitchHUDItem = 0x3070;
        /// <summary>SPC $3078, sound1InstructionLists_sound3A; ROM pointer entry $CF9767. (Empty)</summary>
        internal const ushort Sound3AEmpty = 0x3078;
        /// <summary>SPC $307B, sound1InstructionLists_sound3B; ROM pointer entry $CF9769. Hexagon map -&gt; square map transition</summary>
        internal const ushort Sound3BHexagonMapSquareMapTransition = 0x307b;
        /// <summary>SPC $308B, sound1InstructionLists_sound3C; ROM pointer entry $CF976B. Square map -&gt; hexagon map transition</summary>
        internal const ushort Sound3CSquareMapHexagonMapTransition = 0x308b;
        /// <summary>SPC $309B, sound1InstructionLists_sound3D; ROM pointer entry $CF976D. Dud shot</summary>
        internal const ushort Sound3DDudShot = 0x309b;
        /// <summary>SPC $30A8, sound1InstructionLists_sound3E; ROM pointer entry $CF976F. Space jump</summary>
        internal const ushort Sound3ESpaceJump = 0x30a8;
        /// <summary>SPC $30D6, sound1InstructionLists_sound3F; ROM pointer entry $CF9771. Resumed space jump</summary>
        internal const ushort Sound3FResumedSpaceJump = 0x30d6;
        /// <summary>SPC $30E1, sound1InstructionLists_sound40; ROM pointer entry $CF9773. Mother Brain&apos;s rainbow beam</summary>
        internal const ushort Sound40MotherBrainSRainbowBeam = 0x30e1;
        /// <summary>SPC $312A, sound1InstructionLists_sound41; ROM pointer entry $CF9775. Resume charging beam</summary>
        internal const ushort Sound41ResumeChargingBeam = 0x312a;
        /// <summary>SPC $313F, sound1InstructionLists_sound42; ROM pointer entry $CF9777. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound42 = 0x313f;
    }

    /// <summary>SPC library 2 instruction-list identities.</summary>
    internal static class Library2
    {
        /// <summary>SPC $3AB1, sound2InstructionLists_sound1; ROM pointer entry $CFA5BB. Collected small health drop</summary>
        internal const ushort Sound01CollectedSmallHealthDrop = 0x3ab1;
        /// <summary>SPC $3AC3, sound2InstructionLists_sound2; ROM pointer entry $CFA5BD. Collected big health drop</summary>
        internal const ushort Sound02CollectedBigHealthDrop = 0x3ac3;
        /// <summary>SPC $3AD5, sound2InstructionLists_sound3; ROM pointer entry $CFA5BF. Collected missile drop</summary>
        internal const ushort Sound03CollectedMissileDrop = 0x3ad5;
        /// <summary>SPC $3AF1, sound2InstructionLists_sound4; ROM pointer entry $CFA5C1. Collected super missile drop</summary>
        internal const ushort Sound04CollectedSuperMissileDrop = 0x3af1;
        /// <summary>SPC $3AF3, sound2InstructionLists_sound5; ROM pointer entry $CFA5C3. Collected power bomb drop</summary>
        internal const ushort Sound05CollectedPowerBombDrop = 0x3af3;
        /// <summary>SPC $3AF5, sound2InstructionLists_sound6; ROM pointer entry $CFA5C5. Block destroyed by contact damage</summary>
        internal const ushort Sound06BlockDestroyedByContactDamage = 0x3af5;
        /// <summary>SPC $3B0C, sound2InstructionLists_sound7; ROM pointer entry $CFA5C7. (Super) missile hit wall</summary>
        internal const ushort Sound07SuperMissileHitWall = 0x3b0c;
        /// <summary>SPC $3B28, sound2InstructionLists_sound8; ROM pointer entry $CFA5C9. Bomb explosion</summary>
        internal const ushort Sound08BombExplosion = 0x3b28;
        /// <summary>SPC $3B2A, sound2InstructionLists_sound9; ROM pointer entry $CFA5CB. Enemy killed</summary>
        internal const ushort Sound09EnemyKilled = 0x3b2a;
        /// <summary>SPC $3B3A, sound2InstructionLists_soundA; ROM pointer entry $CFA5CD. Block crumbled or destroyed by shot</summary>
        internal const ushort Sound0ABlockCrumbledOrDestroyedByShot = 0x3b3a;
        /// <summary>SPC $3B42, sound2InstructionLists_soundB; ROM pointer entry $CFA5CF. Enemy killed by contact damage</summary>
        internal const ushort Sound0BEnemyKilledByContactDamage = 0x3b42;
        /// <summary>SPC $3B5E, sound2InstructionLists_soundC; ROM pointer entry $CFA5D1. Beam hit wall</summary>
        internal const ushort Sound0CBeamHitWall = 0x3b5e;
        /// <summary>SPC $3B73, sound2InstructionLists_soundD; ROM pointer entry $CFA5D3. Splashed into water</summary>
        internal const ushort Sound0DSplashedIntoWater = 0x3b73;
        /// <summary>SPC $3B85, sound2InstructionLists_soundE; ROM pointer entry $CFA5D5. Splashed out of water</summary>
        internal const ushort Sound0ESplashedOutOfWater = 0x3b85;
        /// <summary>SPC $3B92, sound2InstructionLists_soundF; ROM pointer entry $CFA5D7. Low pitched air bubbles</summary>
        internal const ushort Sound0FLowPitchedAirBubbles = 0x3b92;
        /// <summary>SPC $3BA9, sound2InstructionLists_sound10; ROM pointer entry $CFA5D9. Lava/acid damaging Samus</summary>
        internal const ushort Sound10LavaAcidDamagingSamus = 0x3ba9;
        /// <summary>SPC $3BB4, sound2InstructionLists_sound11; ROM pointer entry $CFA5DB. High pitched air bubbles</summary>
        internal const ushort Sound11HighPitchedAirBubbles = 0x3bb4;
        /// <summary>SPC $3BC1, sound2InstructionLists_sound12; ROM pointer entry $CFA5DD. Plays at random in heated rooms</summary>
        internal const ushort Sound12PlaysAtRandomInHeatedRooms = 0x3bc1;
        /// <summary>SPC $3BE7, sound2InstructionLists_sound13; ROM pointer entry $CFA5DF. Plays at random in heated rooms</summary>
        internal const ushort Sound13PlaysAtRandomInHeatedRooms = 0x3be7;
        /// <summary>SPC $3C08, sound2InstructionLists_sound14; ROM pointer entry $CFA5E1. Plays at random in heated rooms</summary>
        internal const ushort Sound14PlaysAtRandomInHeatedRooms = 0x3c08;
        /// <summary>SPC $3C33, sound2InstructionLists_sound15; ROM pointer entry $CFA5E3. Maridia elevatube</summary>
        internal const ushort Sound15MaridiaElevatube = 0x3c33;
        /// <summary>SPC $3C3B, sound2InstructionLists_sound16; ROM pointer entry $CFA5E5. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound16 = 0x3c3b;
        /// <summary>SPC $3C43, sound2InstructionLists_sound17; ROM pointer entry $CFA5E7. Morph ball eye&apos;s ray</summary>
        internal const ushort Sound17MorphBallEyeSRay = 0x3c43;
        /// <summary>SPC $3C56, sound2InstructionLists_sound18; ROM pointer entry $CFA5E9. Beacon</summary>
        internal const ushort Sound18Beacon = 0x3c56;
        /// <summary>SPC $3C90, sound2InstructionLists_sound19; ROM pointer entry $CFA5EB. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound19 = 0x3c90;
        /// <summary>SPC $3CFF, sound2InstructionLists_sound1A; ROM pointer entry $CFA5ED. n00b tube shattering</summary>
        internal const ushort Sound1AN00bTubeShattering = 0x3cff;
        /// <summary>SPC $3D46, sound2InstructionLists_sound1B; ROM pointer entry $CFA5EF. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound1B = 0x3d46;
        /// <summary>SPC $3D4E, sound2InstructionLists_sound1C; ROM pointer entry $CFA5F1. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound1C = 0x3d4e;
        /// <summary>SPC $3D65, sound2InstructionLists_sound1D; ROM pointer entry $CFA5F3. Dachora cry</summary>
        internal const ushort Sound1DDachoraCry = 0x3d65;
        /// <summary>SPC $3D81, sound2InstructionLists_sound1E; ROM pointer entry $CFA5F5. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound1E = 0x3d81;
        /// <summary>SPC $3D9B, sound2InstructionLists_sound1F; ROM pointer entry $CFA5F7. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound1F = 0x3d9b;
        /// <summary>SPC $3DA8, sound2InstructionLists_sound20; ROM pointer entry $CFA5F9. Shot fly</summary>
        internal const ushort Sound20ShotFly = 0x3da8;
        /// <summary>SPC $3DBF, sound2InstructionLists_sound21; ROM pointer entry $CFA5FB. Shot skree / wall/ninja space pirate</summary>
        internal const ushort Sound21ShotSkreeWallNinjaSpacePirate = 0x3dbf;
        /// <summary>SPC $3DD6, sound2InstructionLists_sound22; ROM pointer entry $CFA5FD. Shot pipe bug / high-rising slow-falling enemy</summary>
        internal const ushort Sound22ShotPipeBugHighRisingSlowFallingEnemy = 0x3dd6;
        /// <summary>SPC $3DED, sound2InstructionLists_sound23; ROM pointer entry $CFA5FF. Shot slug / sidehopper / zoomer</summary>
        internal const ushort Sound23ShotSlugSidehopperZoomer = 0x3ded;
        /// <summary>SPC $3E04, sound2InstructionLists_sound24; ROM pointer entry $CFA601. Small explosion (enemy death)</summary>
        internal const ushort Sound24SmallExplosionEnemyDeath = 0x3e04;
        /// <summary>SPC $3E20, sound2InstructionLists_sound25; ROM pointer entry $CFA603. Ceres door explosion (also used by Mother Brain)</summary>
        internal const ushort Sound25CeresDoorExplosionAlsoUsedByMotherBrain = 0x3e20;
        /// <summary>SPC $3E41, sound2InstructionLists_sound26; ROM pointer entry $CFA605. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound26 = 0x3e41;
        /// <summary>SPC $3E66, sound2InstructionLists_sound27; ROM pointer entry $CFA607. Shot torizo</summary>
        internal const ushort Sound27ShotTorizo = 0x3e66;
        /// <summary>SPC $3E94, sound2InstructionLists_sound28; ROM pointer entry $CFA609. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound28 = 0x3e94;
        /// <summary>SPC $3EB0, sound2InstructionLists_sound29; ROM pointer entry $CFA60B. Mother Brain rising into phase 2</summary>
        internal const ushort Sound29MotherBrainRisingIntoPhase2 = 0x3eb0;
        /// <summary>SPC $3ECC, sound2InstructionLists_sound2A; ROM pointer entry $CFA60D. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound2A = 0x3ecc;
        /// <summary>SPC $3ECE, sound2InstructionLists_sound2B; ROM pointer entry $CFA60F. Ridley&apos;s fireball hit surface</summary>
        internal const ushort Sound2BRidleySFireballHitSurface = 0x3ece;
        /// <summary>SPC $3F03, sound2InstructionLists_sound2C; ROM pointer entry $CFA611. Shot Spore Spawn</summary>
        internal const ushort Sound2CShotSporeSpawn = 0x3f03;
        /// <summary>SPC $3F18, sound2InstructionLists_sound2D; ROM pointer entry $CFA613. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound2D = 0x3f18;
        /// <summary>SPC $3F20, sound2InstructionLists_sound2E; ROM pointer entry $CFA615. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound2E = 0x3f20;
        /// <summary>SPC $3F44, sound2InstructionLists_sound2F; ROM pointer entry $CFA617. Yapping maw</summary>
        internal const ushort Sound2FYappingMaw = 0x3f44;
        /// <summary>SPC $3F5E, sound2InstructionLists_sound30; ROM pointer entry $CFA619. Shot super-desgeega</summary>
        internal const ushort Sound30ShotSuperDesgeega = 0x3f5e;
        /// <summary>SPC $3F75, sound2InstructionLists_sound31; ROM pointer entry $CFA61B. Brinstar plant chewing</summary>
        internal const ushort Sound31BrinstarPlantChewing = 0x3f75;
        /// <summary>SPC $3F82, sound2InstructionLists_sound32; ROM pointer entry $CFA61D. Etecoon wall-jump</summary>
        internal const ushort Sound32EtecoonWallJump = 0x3f82;
        /// <summary>SPC $3F8A, sound2InstructionLists_sound33; ROM pointer entry $CFA61F. Etecoon cry</summary>
        internal const ushort Sound33EtecoonCry = 0x3f8a;
        /// <summary>SPC $3F97, sound2InstructionLists_sound34; ROM pointer entry $CFA621. Spike shooting plant spikes</summary>
        internal const ushort Sound34SpikeShootingPlantSpikes = 0x3f97;
        /// <summary>SPC $3F9F, sound2InstructionLists_sound35; ROM pointer entry $CFA623. Etecoon&apos;s theme</summary>
        internal const ushort Sound35EtecoonSTheme = 0x3f9f;
        /// <summary>SPC $401F, sound2InstructionLists_sound36; ROM pointer entry $CFA625. Shot rio / Norfair lava-jumping enemy / lava seahorse</summary>
        internal const ushort Sound36ShotRioNorfairLavaJumpingEnemyLavaSeahorse = 0x401f;
        /// <summary>SPC $4036, sound2InstructionLists_sound37; ROM pointer entry $CFA627. Refill/map station engaged</summary>
        internal const ushort Sound37RefillMapStationEngaged = 0x4036;
        /// <summary>SPC $4066, sound2InstructionLists_sound38; ROM pointer entry $CFA629. Refill/map station disengaged</summary>
        internal const ushort Sound38RefillMapStationDisengaged = 0x4066;
        /// <summary>SPC $407C, sound2InstructionLists_sound39; ROM pointer entry $CFA62B. Dachora speed booster</summary>
        internal const ushort Sound39DachoraSpeedBooster = 0x407c;
        /// <summary>SPC $407E, sound2InstructionLists_sound3A; ROM pointer entry $CFA62D. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound3A = 0x407e;
        /// <summary>SPC $4086, sound2InstructionLists_sound3B; ROM pointer entry $CFA62F. Dachora shinespark</summary>
        internal const ushort Sound3BDachoraShinespark = 0x4086;
        /// <summary>SPC $4088, sound2InstructionLists_sound3C; ROM pointer entry $CFA631. Dachora shinespark ended</summary>
        internal const ushort Sound3CDachoraShinesparkEnded = 0x4088;
        /// <summary>SPC $408A, sound2InstructionLists_sound3D; ROM pointer entry $CFA633. Dachora stored shinespark</summary>
        internal const ushort Sound3DDachoraStoredShinespark = 0x408a;
        /// <summary>SPC $408C, sound2InstructionLists_sound3E; ROM pointer entry $CFA635. Shot Maridia spikey shells / Norfair erratic fireball / ripped / kamer / Maridia snail / yapping maw / Wrecked Ship orbs</summary>
        internal const ushort Sound3EShotMaridiaSpikeyShellsNorfairErraticFireballRippedKamerMaridiaSnailYappingMawWreckedShipOrbs = 0x408c;
        /// <summary>SPC $409E, sound2InstructionLists_sound3F; ROM pointer entry $CFA637. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound3F = 0x409e;
        /// <summary>SPC $40A6, sound2InstructionLists_sound40; ROM pointer entry $CFA639. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound40 = 0x40a6;
        /// <summary>SPC $40B1, sound2InstructionLists_sound41; ROM pointer entry $CFA63B. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound41 = 0x40b1;
        /// <summary>SPC $40B4, sound2InstructionLists_sound42; ROM pointer entry $CFA63D. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound42 = 0x40b4;
        /// <summary>SPC $40BC, sound2InstructionLists_sound43; ROM pointer entry $CFA63F. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound43 = 0x40bc;
        /// <summary>SPC $40CE, sound2InstructionLists_sound44; ROM pointer entry $CFA641. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound44 = 0x40ce;
        /// <summary>SPC $40D1, sound2InstructionLists_sound45; ROM pointer entry $CFA643. Typewriter stroke - Ceres self destruct sequence</summary>
        internal const ushort Sound45TypewriterStrokeCeresSelfDestructSequence = 0x40d1;
        /// <summary>SPC $40DE, sound2InstructionLists_sound46; ROM pointer entry $CFA645. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound46 = 0x40de;
        /// <summary>SPC $40F5, sound2InstructionLists_sound47; ROM pointer entry $CFA647. Shot waver</summary>
        internal const ushort Sound47ShotWaver = 0x40f5;
        /// <summary>SPC $410C, sound2InstructionLists_sound48; ROM pointer entry $CFA649. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound48 = 0x410c;
        /// <summary>SPC $412C, sound2InstructionLists_sound49; ROM pointer entry $CFA64B. Shot fish / crab / Maridia refill candy</summary>
        internal const ushort Sound49ShotFishCrabMaridiaRefillCandy = 0x412c;
        /// <summary>SPC $4143, sound2InstructionLists_sound4A; ROM pointer entry $CFA64D. Shot mini-Draygon</summary>
        internal const ushort Sound4AShotMiniDraygon = 0x4143;
        /// <summary>SPC $415A, sound2InstructionLists_sound4B; ROM pointer entry $CFA64F. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound4B = 0x415a;
        /// <summary>SPC $4162, sound2InstructionLists_sound4C; ROM pointer entry $CFA651. Ki-hunter / eye door acid spit</summary>
        internal const ushort Sound4CKiHunterEyeDoorAcidSpit = 0x4162;
        /// <summary>SPC $416F, sound2InstructionLists_sound4D; ROM pointer entry $CFA653. Gunship hover</summary>
        internal const ushort Sound4DGunshipHover = 0x416f;
        /// <summary>SPC $41A9, sound2InstructionLists_sound4E; ROM pointer entry $CFA655. Ceres Ridley getaway</summary>
        internal const ushort Sound4ECeresRidleyGetaway = 0x41a9;
        /// <summary>SPC $41C7, sound2InstructionLists_sound4F; ROM pointer entry $CFA657. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound4F = 0x41c7;
        /// <summary>SPC $41D9, sound2InstructionLists_sound50; ROM pointer entry $CFA659. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound50 = 0x41d9;
        /// <summary>SPC $41EE, sound2InstructionLists_sound51; ROM pointer entry $CFA65B. Shot Wrecked Ship ghost</summary>
        internal const ushort Sound51ShotWreckedShipGhost = 0x41ee;
        /// <summary>SPC $421C, sound2InstructionLists_sound52; ROM pointer entry $CFA65D. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound52 = 0x421c;
        /// <summary>SPC $4224, sound2InstructionLists_sound53; ROM pointer entry $CFA65F. Shot mini-Crocomire</summary>
        internal const ushort Sound53ShotMiniCrocomire = 0x4224;
        /// <summary>SPC $4236, sound2InstructionLists_sound54; ROM pointer entry $CFA661. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound54 = 0x4236;
        /// <summary>SPC $424D, sound2InstructionLists_sound55; ROM pointer entry $CFA663. Shot beetom</summary>
        internal const ushort Sound55ShotBeetom = 0x424d;
        /// <summary>SPC $4268, sound2InstructionLists_sound56; ROM pointer entry $CFA665. Acquired suit</summary>
        internal const ushort Sound56AcquiredSuit = 0x4268;
        /// <summary>SPC $4288, sound2InstructionLists_sound57; ROM pointer entry $CFA667. Shot door/gate with dud shot / shot reflec</summary>
        internal const ushort Sound57ShotDoorGateWithDudShotShotReflec = 0x4288;
        /// <summary>SPC $429A, sound2InstructionLists_sound58; ROM pointer entry $CFA669. Shot mochtroid</summary>
        internal const ushort Sound58ShotMochtroid = 0x429a;
        /// <summary>SPC $42AF, sound2InstructionLists_sound59; ROM pointer entry $CFA66B. Ridley&apos;s roar</summary>
        internal const ushort Sound59RidleySRoar = 0x42af;
        /// <summary>SPC $42BF, sound2InstructionLists_sound5A; ROM pointer entry $CFA66D. Shot metroid</summary>
        internal const ushort Sound5AShotMetroid = 0x42bf;
        /// <summary>SPC $42D4, sound2InstructionLists_sound5B; ROM pointer entry $CFA66F. Skree launches attack</summary>
        internal const ushort Sound5BSkreeLaunchesAttack = 0x42d4;
        /// <summary>SPC $42D6, sound2InstructionLists_sound5C; ROM pointer entry $CFA671. Skree hits the ground</summary>
        internal const ushort Sound5CSkreeHitsTheGround = 0x42d6;
        /// <summary>SPC $42FE, sound2InstructionLists_sound5D; ROM pointer entry $CFA673. Sidehopper jumped</summary>
        internal const ushort Sound5DSidehopperJumped = 0x42fe;
        /// <summary>SPC $4310, sound2InstructionLists_sound5E; ROM pointer entry $CFA675. Sidehopper landed</summary>
        internal const ushort Sound5ESidehopperLanded = 0x4310;
        /// <summary>SPC $4322, sound2InstructionLists_sound5F; ROM pointer entry $CFA677. Shot Lower Norfair rio / desgeega / Norfair slow fireball / walking lava seahorse / Botwoon</summary>
        internal const ushort Sound5FShotLowerNorfairRioDesgeegaNorfairSlowFireballWalkingLavaSeahorseBotwoon = 0x4322;
        /// <summary>SPC $4334, sound2InstructionLists_sound60; ROM pointer entry $CFA679. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound60 = 0x4334;
        /// <summary>SPC $433C, sound2InstructionLists_sound61; ROM pointer entry $CFA67B. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound61 = 0x433c;
        /// <summary>SPC $4347, sound2InstructionLists_sound62; ROM pointer entry $CFA67D. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound62 = 0x4347;
        /// <summary>SPC $4352, sound2InstructionLists_sound63; ROM pointer entry $CFA67F. Mother Brain&apos;s ketchup beam</summary>
        internal const ushort Sound63MotherBrainSKetchupBeam = 0x4352;
        /// <summary>SPC $43C1, sound2InstructionLists_sound64; ROM pointer entry $CFA681. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound64 = 0x43c1;
        /// <summary>SPC $43CC, sound2InstructionLists_sound65; ROM pointer entry $CFA683. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound65 = 0x43cc;
        /// <summary>SPC $43E8, sound2InstructionLists_sound66; ROM pointer entry $CFA685. Shot ki-hunter / walking space pirate</summary>
        internal const ushort Sound66ShotKiHunterWalkingSpacePirate = 0x43e8;
        /// <summary>SPC $43FA, sound2InstructionLists_sound67; ROM pointer entry $CFA687. Space pirate / Mother Brain laser</summary>
        internal const ushort Sound67SpacePirateMotherBrainLaser = 0x43fa;
        /// <summary>SPC $4422, sound2InstructionLists_sound68; ROM pointer entry $CFA689. Shot Wrecked Ship robot</summary>
        internal const ushort Sound68ShotWreckedShipRobot = 0x4422;
        /// <summary>SPC $442F, sound2InstructionLists_sound69; ROM pointer entry $CFA68B. Shot Shaktool</summary>
        internal const ushort Sound69ShotShaktool = 0x442f;
        /// <summary>SPC $4441, sound2InstructionLists_sound6A; ROM pointer entry $CFA68D. Shot Maridia floater</summary>
        internal const ushort Sound6AShotMaridiaFloater = 0x4441;
        /// <summary>SPC $4443, sound2InstructionLists_sound6B; ROM pointer entry $CFA68F. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound6B = 0x4443;
        /// <summary>SPC $4446, sound2InstructionLists_sound6C; ROM pointer entry $CFA691. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound6C = 0x4446;
        /// <summary>SPC $444E, sound2InstructionLists_sound6D; ROM pointer entry $CFA693. Ceres tiles falling from ceiling</summary>
        internal const ushort Sound6DCeresTilesFallingFromCeiling = 0x444e;
        /// <summary>SPC $446F, sound2InstructionLists_sound6E; ROM pointer entry $CFA695. Shot Mother Brain phase 1</summary>
        internal const ushort Sound6EShotMotherBrainPhase1 = 0x446f;
        /// <summary>SPC $447F, sound2InstructionLists_sound6F; ROM pointer entry $CFA697. Mother Brain&apos;s cry - low pitch</summary>
        internal const ushort Sound6FMotherBrainSCryLowPitch = 0x447f;
        /// <summary>SPC $448F, sound2InstructionLists_sound70; ROM pointer entry $CFA699. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound70 = 0x448f;
        /// <summary>SPC $449C, sound2InstructionLists_sound71; ROM pointer entry $CFA69B. Silence</summary>
        internal const ushort Sound71Silence = 0x449c;
        /// <summary>SPC $44A4, sound2InstructionLists_sound72; ROM pointer entry $CFA69D. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound72 = 0x44a4;
        /// <summary>SPC $44B9, sound2InstructionLists_sound73; ROM pointer entry $CFA69F. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound73 = 0x44b9;
        /// <summary>SPC $44CE, sound2InstructionLists_sound74; ROM pointer entry $CFA6A1. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound74 = 0x44ce;
        /// <summary>SPC $44ED, sound2InstructionLists_sound75; ROM pointer entry $CFA6A3. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound75 = 0x44ed;
        /// <summary>SPC $4598, sound2InstructionLists_sound76; ROM pointer entry $CFA6A5. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound76 = 0x4598;
        /// <summary>SPC $459A, sound2InstructionLists_sound77; ROM pointer entry $CFA6A7. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound77 = 0x459a;
        /// <summary>SPC $4618, sound2InstructionLists_sound78; ROM pointer entry $CFA6A9. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound78 = 0x4618;
        /// <summary>SPC $462D, sound2InstructionLists_sound79; ROM pointer entry $CFA6AB. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound79 = 0x462d;
        /// <summary>SPC $4642, sound2InstructionLists_sound7A; ROM pointer entry $CFA6AD. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound7A = 0x4642;
        /// <summary>SPC $4657, sound2InstructionLists_sound7B; ROM pointer entry $CFA6AF. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound7B = 0x4657;
        /// <summary>SPC $466C, sound2InstructionLists_sound7C; ROM pointer entry $CFA6B1. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound7C = 0x466c;
        /// <summary>SPC $4679, sound2InstructionLists_sound7D; ROM pointer entry $CFA6B3. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound7D = 0x4679;
        /// <summary>SPC $4686, sound2InstructionLists_sound7E; ROM pointer entry $CFA6B5. Mother Brain&apos;s cry - high pitch</summary>
        internal const ushort Sound7EMotherBrainSCryHighPitch = 0x4686;
        /// <summary>SPC $468E, sound2InstructionLists_sound7F; ROM pointer entry $CFA6B7. Mother Brain charging her rainbow</summary>
        internal const ushort Sound7FMotherBrainChargingHerRainbow = 0x468e;
    }

    /// <summary>SPC library 3 instruction-list identities.</summary>
    internal static class Library3
    {
        /// <summary>SPC $4EED, sound3InstructionLists_sound1; ROM pointer entry $CFBA97. Silence</summary>
        internal const ushort Sound01Silence = 0x4eed;
        /// <summary>SPC $4EF5, sound3InstructionLists_sound2; ROM pointer entry $CFBA99. Low health beep</summary>
        internal const ushort Sound02LowHealthBeep = 0x4ef5;
        /// <summary>SPC $4F00, sound3InstructionLists_sound3; ROM pointer entry $CFBA9B. Speed booster</summary>
        internal const ushort Sound03SpeedBooster = 0x4f00;
        /// <summary>SPC $4F4B, sound3InstructionLists_sound4; ROM pointer entry $CFBA9D. Samus landed hard</summary>
        internal const ushort Sound04SamusLandedHard = 0x4f4b;
        /// <summary>SPC $4F5B, sound3InstructionLists_sound5; ROM pointer entry $CFBA9F. Samus landed / wall-jumped</summary>
        internal const ushort Sound05SamusLandedWallJumped = 0x4f5b;
        /// <summary>SPC $4F6B, sound3InstructionLists_sound6; ROM pointer entry $CFBAA1. Samus&apos; footsteps</summary>
        internal const ushort Sound06SamusFootsteps = 0x4f6b;
        /// <summary>SPC $4F73, sound3InstructionLists_sound7; ROM pointer entry $CFBAA3. Door opened</summary>
        internal const ushort Sound07DoorOpened = 0x4f73;
        /// <summary>SPC $4F89, sound3InstructionLists_sound8; ROM pointer entry $CFBAA5. Door closed</summary>
        internal const ushort Sound08DoorClosed = 0x4f89;
        /// <summary>SPC $4F9F, sound3InstructionLists_sound9; ROM pointer entry $CFBAA7. Missile door shot with missile</summary>
        internal const ushort Sound09MissileDoorShotWithMissile = 0x4f9f;
        /// <summary>SPC $4FB6, sound3InstructionLists_soundA; ROM pointer entry $CFBAA9. Enemy frozen</summary>
        internal const ushort Sound0AEnemyFrozen = 0x4fb6;
        /// <summary>SPC $4FE1, sound3InstructionLists_soundB; ROM pointer entry $CFBAAB. Elevator</summary>
        internal const ushort Sound0BElevator = 0x4fe1;
        /// <summary>SPC $4FF7, sound3InstructionLists_soundC; ROM pointer entry $CFBAAD. Stored shinespark</summary>
        internal const ushort Sound0CStoredShinespark = 0x4ff7;
        /// <summary>SPC $4FFF, sound3InstructionLists_soundD; ROM pointer entry $CFBAAF. Typewriter stroke - intro</summary>
        internal const ushort Sound0DTypewriterStrokeIntro = 0x4fff;
        /// <summary>SPC $500C, sound3InstructionLists_soundE; ROM pointer entry $CFBAB1. Gate opening/closing</summary>
        internal const ushort Sound0EGateOpeningClosing = 0x500c;
        /// <summary>SPC $5047, sound3InstructionLists_soundF; ROM pointer entry $CFBAB3. Shinespark</summary>
        internal const ushort Sound0FShinespark = 0x5047;
        /// <summary>SPC $50A8, sound3InstructionLists_sound10; ROM pointer entry $CFBAB5. Shinespark ended</summary>
        internal const ushort Sound10ShinesparkEnded = 0x50a8;
        /// <summary>SPC $50C4, sound3InstructionLists_sound11; ROM pointer entry $CFBAB7. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound11 = 0x50c4;
        /// <summary>SPC $50D1, sound3InstructionLists_sound12; ROM pointer entry $CFBAB9. (Empty)</summary>
        internal const ushort Sound12Empty = 0x50d1;
        /// <summary>SPC $50D4, sound3InstructionLists_sound13; ROM pointer entry $CFBABB. Mother Brain&apos;s projectile hits surface</summary>
        internal const ushort Sound13MotherBrainSProjectileHitsSurface = 0x50d4;
        /// <summary>SPC $50F0, sound3InstructionLists_sound14; ROM pointer entry $CFBABD. Gunship elevator activated</summary>
        internal const ushort Sound14GunshipElevatorActivated = 0x50f0;
        /// <summary>SPC $511A, sound3InstructionLists_sound15; ROM pointer entry $CFBABF. Gunship elevator deactivated</summary>
        internal const ushort Sound15GunshipElevatorDeactivated = 0x511a;
        /// <summary>SPC $5130, sound3InstructionLists_sound16; ROM pointer entry $CFBAC1. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound16 = 0x5130;
        /// <summary>SPC $5142, sound3InstructionLists_sound17; ROM pointer entry $CFBAC3. Mother Brain&apos;s blue rings</summary>
        internal const ushort Sound17MotherBrainSBlueRings = 0x5142;
        /// <summary>SPC $5175, sound3InstructionLists_sound18; ROM pointer entry $CFBAC5. (Empty)</summary>
        internal const ushort Sound18Empty = 0x5175;
        /// <summary>SPC $5178, sound3InstructionLists_sound19; ROM pointer entry $CFBAC7. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound19 = 0x5178;
        /// <summary>SPC $5188, sound3InstructionLists_sound1A; ROM pointer entry $CFBAC9. (Empty)</summary>
        internal const ushort Sound1AEmpty = 0x5188;
        /// <summary>SPC $518B, sound3InstructionLists_sound1B; ROM pointer entry $CFBACB. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound1B = 0x518b;
        /// <summary>SPC $51B9, sound3InstructionLists_sound1C; ROM pointer entry $CFBACD. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound1C = 0x51b9;
        /// <summary>SPC $51C1, sound3InstructionLists_sound1D; ROM pointer entry $CFBACF. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound1D = 0x51c1;
        /// <summary>SPC $51D4, sound3InstructionLists_sound1E; ROM pointer entry $CFBAD1. Earthquake (Kraid)</summary>
        internal const ushort Sound1EEarthquakeKraid = 0x51d4;
        /// <summary>SPC $51F0, sound3InstructionLists_sound1F; ROM pointer entry $CFBAD3. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound1F = 0x51f0;
        /// <summary>SPC $51FD, sound3InstructionLists_sound20; ROM pointer entry $CFBAD5. (Empty)</summary>
        internal const ushort Sound20Empty = 0x51fd;
        /// <summary>SPC $5200, sound3InstructionLists_sound21; ROM pointer entry $CFBAD7. Ridley whips its tail</summary>
        internal const ushort Sound21RidleyWhipsItsTail = 0x5200;
        /// <summary>SPC $5208, sound3InstructionLists_sound22; ROM pointer entry $CFBAD9. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound22 = 0x5208;
        /// <summary>SPC $5229, sound3InstructionLists_sound23; ROM pointer entry $CFBADB. Baby metroid cry 1</summary>
        internal const ushort Sound23BabyMetroidCry1 = 0x5229;
        /// <summary>SPC $5231, sound3InstructionLists_sound24; ROM pointer entry $CFBADD. Baby metroid cry - Ceres</summary>
        internal const ushort Sound24BabyMetroidCryCeres = 0x5231;
        /// <summary>SPC $5239, sound3InstructionLists_sound25; ROM pointer entry $CFBADF. Silence (clear speed booster / elevator sound)</summary>
        internal const ushort Sound25SilenceClearSpeedBoosterElevatorSound = 0x5239;
        /// <summary>SPC $5241, sound3InstructionLists_sound26; ROM pointer entry $CFBAE1. Baby metroid cry 2</summary>
        internal const ushort Sound26BabyMetroidCry2 = 0x5241;
        /// <summary>SPC $524E, sound3InstructionLists_sound27; ROM pointer entry $CFBAE3. Baby metroid cry 3</summary>
        internal const ushort Sound27BabyMetroidCry3 = 0x524e;
        /// <summary>SPC $5256, sound3InstructionLists_sound28; ROM pointer entry $CFBAE5. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound28 = 0x5256;
        /// <summary>SPC $5277, sound3InstructionLists_sound29; ROM pointer entry $CFBAE7. Phantoon related</summary>
        internal const ushort Sound29PhantoonRelated = 0x5277;
        /// <summary>SPC $5293, sound3InstructionLists_sound2A; ROM pointer entry $CFBAE9. Pause menu ambient beep</summary>
        internal const ushort Sound2APauseMenuAmbientBeep = 0x5293;
        /// <summary>SPC $52A5, sound3InstructionLists_sound2B; ROM pointer entry $CFBAEB. Unnamed sound command; original instruction-list identity.</summary>
        internal const ushort Sound2B = 0x52a5;
        /// <summary>SPC $52B0, sound3InstructionLists_sound2C; ROM pointer entry $CFBAED. Ceres door opening</summary>
        internal const ushort Sound2CCeresDoorOpening = 0x52b0;
        /// <summary>SPC $52C6, sound3InstructionLists_sound2D; ROM pointer entry $CFBAEF. Gaining/losing incremental health</summary>
        internal const ushort Sound2DGainingLosingIncrementalHealth = 0x52c6;
        /// <summary>SPC $52F1, sound3InstructionLists_sound2E; ROM pointer entry $CFBAF1. Mother Brain&apos;s glass shattering</summary>
        internal const ushort Sound2EMotherBrainSGlassShattering = 0x52f1;
        /// <summary>SPC $530B, sound3InstructionLists_sound2F; ROM pointer entry $CFBAF3. (Empty)</summary>
        internal const ushort Sound2FEmpty = 0x530b;
    }

}
