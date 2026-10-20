namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Norfair rooms' doors, named by the room they lead from.</summary>
[NumberedIdentities("Each numbered door is one door of its room, named by its place in that room's door list; every door has its own destination and is used by name, never iterated or indexed.")]
public static class NorfairDoorDefinitions
{
    /// <summary>The <c>Door_IceBeamAcid_0</c> door.</summary>
    public static CartridgeDoorHeader IceBeamAcid0 { get; } = DoorDefinitions.Find(nameof(IceBeamAcid0));

    /// <summary>The <c>Door_IceBeamAcid_1</c> door.</summary>
    public static CartridgeDoorHeader IceBeamAcid1 { get; } = DoorDefinitions.Find(nameof(IceBeamAcid1));

    /// <summary>The <c>Door_Cathedral_0</c> door.</summary>
    public static CartridgeDoorHeader Cathedral0 { get; } = DoorDefinitions.Find(nameof(Cathedral0));

    /// <summary>The <c>Door_Cathedral_1</c> door.</summary>
    public static CartridgeDoorHeader Cathedral1 { get; } = DoorDefinitions.Find(nameof(Cathedral1));

    /// <summary>The <c>Door_CathedralEntrance_0</c> door.</summary>
    public static CartridgeDoorHeader CathedralEntrance0 { get; } = DoorDefinitions.Find(nameof(CathedralEntrance0));

    /// <summary>The <c>Door_CathedralEntrance_1</c> door.</summary>
    public static CartridgeDoorHeader CathedralEntrance1 { get; } = DoorDefinitions.Find(nameof(CathedralEntrance1));

    /// <summary>The <c>Door_BusinessCenter_0</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter0 { get; } = DoorDefinitions.Find(nameof(BusinessCenter0));

    /// <summary>The <c>Door_BusinessCenter_1</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter1 { get; } = DoorDefinitions.Find(nameof(BusinessCenter1));

    /// <summary>The <c>Door_BusinessCenter_2</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter2 { get; } = DoorDefinitions.Find(nameof(BusinessCenter2));

    /// <summary>The <c>Door_BusinessCenter_3</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter3 { get; } = DoorDefinitions.Find(nameof(BusinessCenter3));

    /// <summary>The <c>Door_BusinessCenter_4</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter4 { get; } = DoorDefinitions.Find(nameof(BusinessCenter4));

    /// <summary>The <c>Door_BusinessCenter_6</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter6 { get; } = DoorDefinitions.Find(nameof(BusinessCenter6));

    /// <summary>The <c>Door_BusinessCenter_7</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter7 { get; } = DoorDefinitions.Find(nameof(BusinessCenter7));

    /// <summary>The <c>Door_IceBeamGate_0</c> door.</summary>
    public static CartridgeDoorHeader IceBeamGate0 { get; } = DoorDefinitions.Find(nameof(IceBeamGate0));

    /// <summary>The <c>Door_IceBeamGate_1</c> door.</summary>
    public static CartridgeDoorHeader IceBeamGate1 { get; } = DoorDefinitions.Find(nameof(IceBeamGate1));

    /// <summary>The <c>Door_IceBeamGate_2</c> door.</summary>
    public static CartridgeDoorHeader IceBeamGate2 { get; } = DoorDefinitions.Find(nameof(IceBeamGate2));

    /// <summary>The <c>Door_IceBeamGate_3</c> door.</summary>
    public static CartridgeDoorHeader IceBeamGate3 { get; } = DoorDefinitions.Find(nameof(IceBeamGate3));

    /// <summary>The <c>Door_IceBeamTutorial_0</c> door.</summary>
    public static CartridgeDoorHeader IceBeamTutorial0 { get; } = DoorDefinitions.Find(nameof(IceBeamTutorial0));

    /// <summary>The <c>Door_IceBeamTutorial_1</c> door.</summary>
    public static CartridgeDoorHeader IceBeamTutorial1 { get; } = DoorDefinitions.Find(nameof(IceBeamTutorial1));

    /// <summary>The <c>Door_IceBeam_0</c> door.</summary>
    public static CartridgeDoorHeader IceBeam0 { get; } = DoorDefinitions.Find(nameof(IceBeam0));

    /// <summary>The <c>Door_IceBeamSnake_0</c> door.</summary>
    public static CartridgeDoorHeader IceBeamSnake0 { get; } = DoorDefinitions.Find(nameof(IceBeamSnake0));

    /// <summary>The <c>Door_IceBeamSnake_1</c> door.</summary>
    public static CartridgeDoorHeader IceBeamSnake1 { get; } = DoorDefinitions.Find(nameof(IceBeamSnake1));

    /// <summary>The <c>Door_IceBeamSnake_2</c> door.</summary>
    public static CartridgeDoorHeader IceBeamSnake2 { get; } = DoorDefinitions.Find(nameof(IceBeamSnake2));

    /// <summary>The <c>Door_CrumbleShaft_0</c> door.</summary>
    public static CartridgeDoorHeader CrumbleShaft0 { get; } = DoorDefinitions.Find(nameof(CrumbleShaft0));

    /// <summary>The <c>Door_CrumbleShaft_1</c> door.</summary>
    public static CartridgeDoorHeader CrumbleShaft1 { get; } = DoorDefinitions.Find(nameof(CrumbleShaft1));

    /// <summary>The <c>Door_CrocomireSpeedway_0</c> door.</summary>
    public static CartridgeDoorHeader CrocomireSpeedway0 { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedway0));

    /// <summary>The <c>Door_CrocomireSpeedway_1</c> door.</summary>
    public static CartridgeDoorHeader CrocomireSpeedway1 { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedway1));

    /// <summary>The <c>Door_CrocomireSpeedway_2</c> door.</summary>
    public static CartridgeDoorHeader CrocomireSpeedway2 { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedway2));

    /// <summary>The <c>Door_CrocomireSpeedway_3</c> door.</summary>
    public static CartridgeDoorHeader CrocomireSpeedway3 { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedway3));

    /// <summary>The <c>Door_CrocomireSpeedway_4</c> door.</summary>
    public static CartridgeDoorHeader CrocomireSpeedway4 { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedway4));

    /// <summary>The <c>Door_Crocomire_0</c> door.</summary>
    public static CartridgeDoorHeader Crocomire0 { get; } = DoorDefinitions.Find(nameof(Crocomire0));

    /// <summary>The <c>Door_Crocomire_1</c> door.</summary>
    public static CartridgeDoorHeader Crocomire1 { get; } = DoorDefinitions.Find(nameof(Crocomire1));

    /// <summary>The <c>Door_HiJumpBoots_0</c> door.</summary>
    public static CartridgeDoorHeader HiJumpBoots0 { get; } = DoorDefinitions.Find(nameof(HiJumpBoots0));

    /// <summary>The <c>Door_CrocomireEscape_0</c> door.</summary>
    public static CartridgeDoorHeader CrocomireEscape0 { get; } = DoorDefinitions.Find(nameof(CrocomireEscape0));

    /// <summary>The <c>Door_CrocomireEscape_1</c> door.</summary>
    public static CartridgeDoorHeader CrocomireEscape1 { get; } = DoorDefinitions.Find(nameof(CrocomireEscape1));

    /// <summary>The <c>Door_HiJumpETank_0</c> door.</summary>
    public static CartridgeDoorHeader HiJumpETank0 { get; } = DoorDefinitions.Find(nameof(HiJumpETank0));

    /// <summary>The <c>Door_HiJumpETank_1</c> door.</summary>
    public static CartridgeDoorHeader HiJumpETank1 { get; } = DoorDefinitions.Find(nameof(HiJumpETank1));

    /// <summary>The <c>Door_PostCrocFarming_0</c> door.</summary>
    public static CartridgeDoorHeader PostCrocFarming0 { get; } = DoorDefinitions.Find(nameof(PostCrocFarming0));

    /// <summary>The <c>Door_PostCrocFarming_1</c> door.</summary>
    public static CartridgeDoorHeader PostCrocFarming1 { get; } = DoorDefinitions.Find(nameof(PostCrocFarming1));

    /// <summary>The <c>Door_PostCrocFarming_2</c> door.</summary>
    public static CartridgeDoorHeader PostCrocFarming2 { get; } = DoorDefinitions.Find(nameof(PostCrocFarming2));

    /// <summary>The <c>Door_PostCrocFarming_3</c> door.</summary>
    public static CartridgeDoorHeader PostCrocFarming3 { get; } = DoorDefinitions.Find(nameof(PostCrocFarming3));

    /// <summary>The <c>Door_PostCrocSave_0</c> door.</summary>
    public static CartridgeDoorHeader PostCrocSave0 { get; } = DoorDefinitions.Find(nameof(PostCrocSave0));

    /// <summary>The <c>Door_PostCrocPowerBombs_0</c> door.</summary>
    public static CartridgeDoorHeader PostCrocPowerBombs0 { get; } = DoorDefinitions.Find(nameof(PostCrocPowerBombs0));

    /// <summary>The <c>Door_PostCrocShaft_0</c> door.</summary>
    public static CartridgeDoorHeader PostCrocShaft0 { get; } = DoorDefinitions.Find(nameof(PostCrocShaft0));

    /// <summary>The <c>Door_PostCrocShaft_1</c> door.</summary>
    public static CartridgeDoorHeader PostCrocShaft1 { get; } = DoorDefinitions.Find(nameof(PostCrocShaft1));

    /// <summary>The <c>Door_PostCrocShaft_2</c> door.</summary>
    public static CartridgeDoorHeader PostCrocShaft2 { get; } = DoorDefinitions.Find(nameof(PostCrocShaft2));

    /// <summary>The <c>Door_PostCrocShaft_3</c> door.</summary>
    public static CartridgeDoorHeader PostCrocShaft3 { get; } = DoorDefinitions.Find(nameof(PostCrocShaft3));

    /// <summary>The <c>Door_PostCrocMissile_0</c> door.</summary>
    public static CartridgeDoorHeader PostCrocMissile0 { get; } = DoorDefinitions.Find(nameof(PostCrocMissile0));

    /// <summary>The <c>Door_GrappleTutorial3_0</c> door.</summary>
    public static CartridgeDoorHeader GrappleTutorial30 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial30));

    /// <summary>The <c>Door_GrappleTutorial3_1</c> door.</summary>
    public static CartridgeDoorHeader GrappleTutorial31 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial31));

    /// <summary>The <c>Door_PostCrocJump_0</c> door.</summary>
    public static CartridgeDoorHeader PostCrocJump0 { get; } = DoorDefinitions.Find(nameof(PostCrocJump0));

    /// <summary>The <c>Door_PostCrocJump_1</c> door.</summary>
    public static CartridgeDoorHeader PostCrocJump1 { get; } = DoorDefinitions.Find(nameof(PostCrocJump1));

    /// <summary>The <c>Door_GrappleTutorial2_0</c> door.</summary>
    public static CartridgeDoorHeader GrappleTutorial20 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial20));

    /// <summary>The <c>Door_GrappleTutorial2_1</c> door.</summary>
    public static CartridgeDoorHeader GrappleTutorial21 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial21));

    /// <summary>The <c>Door_GrappleTutorial1_0</c> door.</summary>
    public static CartridgeDoorHeader GrappleTutorial10 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial10));

    /// <summary>The <c>Door_GrappleTutorial1_1</c> door.</summary>
    public static CartridgeDoorHeader GrappleTutorial11 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial11));

    /// <summary>The <c>Door_GrappleBeam_0</c> door.</summary>
    public static CartridgeDoorHeader GrappleBeam0 { get; } = DoorDefinitions.Find(nameof(GrappleBeam0));

    /// <summary>The <c>Door_GrappleBeam_1</c> door.</summary>
    public static CartridgeDoorHeader GrappleBeam1 { get; } = DoorDefinitions.Find(nameof(GrappleBeam1));

    /// <summary>The <c>Door_NorfairReserveTank</c> door.</summary>
    public static CartridgeDoorHeader NorfairReserveTank { get; } = DoorDefinitions.Find(nameof(NorfairReserveTank));

    /// <summary>The <c>Door_GreenBubblesMissiles_0</c> door.</summary>
    public static CartridgeDoorHeader GreenBubblesMissiles0 { get; } = DoorDefinitions.Find(nameof(GreenBubblesMissiles0));

    /// <summary>The <c>Door_GreenBubblesMissiles_1</c> door.</summary>
    public static CartridgeDoorHeader GreenBubblesMissiles1 { get; } = DoorDefinitions.Find(nameof(GreenBubblesMissiles1));

    /// <summary>The <c>Door_BubbleMountain_0</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain0 { get; } = DoorDefinitions.Find(nameof(BubbleMountain0));

    /// <summary>The <c>Door_BubbleMountain_1</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain1 { get; } = DoorDefinitions.Find(nameof(BubbleMountain1));

    /// <summary>The <c>Door_BubbleMountain_2</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain2 { get; } = DoorDefinitions.Find(nameof(BubbleMountain2));

    /// <summary>The <c>Door_BubbleMountain_3</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain3 { get; } = DoorDefinitions.Find(nameof(BubbleMountain3));

    /// <summary>The <c>Door_BubbleMountain_4</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain4 { get; } = DoorDefinitions.Find(nameof(BubbleMountain4));

    /// <summary>The <c>Door_BubbleMountain_5</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain5 { get; } = DoorDefinitions.Find(nameof(BubbleMountain5));

    /// <summary>The <c>Door_BubbleMountain_6</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain6 { get; } = DoorDefinitions.Find(nameof(BubbleMountain6));

    /// <summary>The <c>Door_SpeedBoosterHall_0</c> door.</summary>
    public static CartridgeDoorHeader SpeedBoosterHall0 { get; } = DoorDefinitions.Find(nameof(SpeedBoosterHall0));

    /// <summary>The <c>Door_SpeedBoosterHall_1</c> door.</summary>
    public static CartridgeDoorHeader SpeedBoosterHall1 { get; } = DoorDefinitions.Find(nameof(SpeedBoosterHall1));

    /// <summary>The <c>Door_SpeedBooster_0</c> door.</summary>
    public static CartridgeDoorHeader SpeedBooster0 { get; } = DoorDefinitions.Find(nameof(SpeedBooster0));

    /// <summary>The <c>Door_SingleChamber_0</c> door.</summary>
    public static CartridgeDoorHeader SingleChamber0 { get; } = DoorDefinitions.Find(nameof(SingleChamber0));

    /// <summary>The <c>Door_SingleChamber_1</c> door.</summary>
    public static CartridgeDoorHeader SingleChamber1 { get; } = DoorDefinitions.Find(nameof(SingleChamber1));

    /// <summary>The <c>Door_SingleChamber_2</c> door.</summary>
    public static CartridgeDoorHeader SingleChamber2 { get; } = DoorDefinitions.Find(nameof(SingleChamber2));

    /// <summary>The <c>Door_SingleChamber_3</c> door.</summary>
    public static CartridgeDoorHeader SingleChamber3 { get; } = DoorDefinitions.Find(nameof(SingleChamber3));

    /// <summary>The <c>Door_SingleChamber_4</c> door.</summary>
    public static CartridgeDoorHeader SingleChamber4 { get; } = DoorDefinitions.Find(nameof(SingleChamber4));

    /// <summary>The <c>Door_DoubleChamber_0</c> door.</summary>
    public static CartridgeDoorHeader DoubleChamber0 { get; } = DoorDefinitions.Find(nameof(DoubleChamber0));

    /// <summary>The <c>Door_DoubleChamber_1</c> door.</summary>
    public static CartridgeDoorHeader DoubleChamber1 { get; } = DoorDefinitions.Find(nameof(DoubleChamber1));

    /// <summary>The <c>Door_DoubleChamber_2</c> door.</summary>
    public static CartridgeDoorHeader DoubleChamber2 { get; } = DoorDefinitions.Find(nameof(DoubleChamber2));

    /// <summary>The <c>Door_WaveBeam_0</c> door.</summary>
    public static CartridgeDoorHeader WaveBeam0 { get; } = DoorDefinitions.Find(nameof(WaveBeam0));

    /// <summary>The <c>Door_SpikyPlatformsTunnel_0</c> door.</summary>
    public static CartridgeDoorHeader SpikyPlatformsTunnel0 { get; } = DoorDefinitions.Find(nameof(SpikyPlatformsTunnel0));

    /// <summary>The <c>Door_SpikyPlatformsTunnel_1</c> door.</summary>
    public static CartridgeDoorHeader SpikyPlatformsTunnel1 { get; } = DoorDefinitions.Find(nameof(SpikyPlatformsTunnel1));

    /// <summary>The <c>Door_Volcano_0</c> door.</summary>
    public static CartridgeDoorHeader Volcano0 { get; } = DoorDefinitions.Find(nameof(Volcano0));

    /// <summary>The <c>Door_Volcano_1</c> door.</summary>
    public static CartridgeDoorHeader Volcano1 { get; } = DoorDefinitions.Find(nameof(Volcano1));

    /// <summary>The <c>Door_KronicBoost_0</c> door.</summary>
    public static CartridgeDoorHeader KronicBoost0 { get; } = DoorDefinitions.Find(nameof(KronicBoost0));

    /// <summary>The <c>Door_KronicBoost_1</c> door.</summary>
    public static CartridgeDoorHeader KronicBoost1 { get; } = DoorDefinitions.Find(nameof(KronicBoost1));

    /// <summary>The <c>Door_KronicBoost_2</c> door.</summary>
    public static CartridgeDoorHeader KronicBoost2 { get; } = DoorDefinitions.Find(nameof(KronicBoost2));

    /// <summary>The <c>Door_KronicBoost_3</c> door.</summary>
    public static CartridgeDoorHeader KronicBoost3 { get; } = DoorDefinitions.Find(nameof(KronicBoost3));

    /// <summary>The <c>Door_MagdolliteTunnel_0</c> door.</summary>
    public static CartridgeDoorHeader MagdolliteTunnel0 { get; } = DoorDefinitions.Find(nameof(MagdolliteTunnel0));

    /// <summary>The <c>Door_MagdolliteTunnel_1</c> door.</summary>
    public static CartridgeDoorHeader MagdolliteTunnel1 { get; } = DoorDefinitions.Find(nameof(MagdolliteTunnel1));

    /// <summary>The <c>Door_PurpleShaft_0</c> door.</summary>
    public static CartridgeDoorHeader PurpleShaft0 { get; } = DoorDefinitions.Find(nameof(PurpleShaft0));

    /// <summary>The <c>Door_PurpleShaft_1</c> door.</summary>
    public static CartridgeDoorHeader PurpleShaft1 { get; } = DoorDefinitions.Find(nameof(PurpleShaft1));

    /// <summary>The <c>Door_PurpleShaft_2</c> door.</summary>
    public static CartridgeDoorHeader PurpleShaft2 { get; } = DoorDefinitions.Find(nameof(PurpleShaft2));

    /// <summary>The <c>Door_LavaDive_0</c> door.</summary>
    public static CartridgeDoorHeader LavaDive0 { get; } = DoorDefinitions.Find(nameof(LavaDive0));

    /// <summary>The <c>Door_LavaDive_1</c> door.</summary>
    public static CartridgeDoorHeader LavaDive1 { get; } = DoorDefinitions.Find(nameof(LavaDive1));

    /// <summary>The <c>Door_LowerNorfairElev_0</c> door.</summary>
    public static CartridgeDoorHeader LowerNorfairElev0 { get; } = DoorDefinitions.Find(nameof(LowerNorfairElev0));

    /// <summary>The <c>Door_LowerNorfairElev_1</c> door.</summary>
    public static CartridgeDoorHeader LowerNorfairElev1 { get; } = DoorDefinitions.Find(nameof(LowerNorfairElev1));

    /// <summary>The <c>Door_LowerNorfairElev_3</c> door.</summary>
    public static CartridgeDoorHeader LowerNorfairElev3 { get; } = DoorDefinitions.Find(nameof(LowerNorfairElev3));

    /// <summary>The <c>Door_UpperNorfairFarming_0</c> door.</summary>
    public static CartridgeDoorHeader UpperNorfairFarming0 { get; } = DoorDefinitions.Find(nameof(UpperNorfairFarming0));

    /// <summary>The <c>Door_UpperNorfairFarming_1</c> door.</summary>
    public static CartridgeDoorHeader UpperNorfairFarming1 { get; } = DoorDefinitions.Find(nameof(UpperNorfairFarming1));

    /// <summary>The <c>Door_UpperNorfairFarming_2</c> door.</summary>
    public static CartridgeDoorHeader UpperNorfairFarming2 { get; } = DoorDefinitions.Find(nameof(UpperNorfairFarming2));

    /// <summary>The <c>Door_RisingTide_0</c> door.</summary>
    public static CartridgeDoorHeader RisingTide0 { get; } = DoorDefinitions.Find(nameof(RisingTide0));

    /// <summary>The <c>Door_RisingTide_1</c> door.</summary>
    public static CartridgeDoorHeader RisingTide1 { get; } = DoorDefinitions.Find(nameof(RisingTide1));

    /// <summary>The <c>Door_AcidSnakesTunnel_0</c> door.</summary>
    public static CartridgeDoorHeader AcidSnakesTunnel0 { get; } = DoorDefinitions.Find(nameof(AcidSnakesTunnel0));

    /// <summary>The <c>Door_AcidSnakesTunnel_1</c> door.</summary>
    public static CartridgeDoorHeader AcidSnakesTunnel1 { get; } = DoorDefinitions.Find(nameof(AcidSnakesTunnel1));

    /// <summary>The <c>Door_AcidSnakesTunnel_2</c> door.</summary>
    public static CartridgeDoorHeader AcidSnakesTunnel2 { get; } = DoorDefinitions.Find(nameof(AcidSnakesTunnel2));

    /// <summary>The <c>Door_SpikyAcidSnakesTunnel_0</c> door.</summary>
    public static CartridgeDoorHeader SpikyAcidSnakesTunnel0 { get; } = DoorDefinitions.Find(nameof(SpikyAcidSnakesTunnel0));

    /// <summary>The <c>Door_SpikyAcidSnakesTunnel_1</c> door.</summary>
    public static CartridgeDoorHeader SpikyAcidSnakesTunnel1 { get; } = DoorDefinitions.Find(nameof(SpikyAcidSnakesTunnel1));

    /// <summary>The <c>Door_UpperNorfairRefill_0</c> door.</summary>
    public static CartridgeDoorHeader UpperNorfairRefill0 { get; } = DoorDefinitions.Find(nameof(UpperNorfairRefill0));

    /// <summary>The <c>Door_UpperNorfairRefill_1</c> door.</summary>
    public static CartridgeDoorHeader UpperNorfairRefill1 { get; } = DoorDefinitions.Find(nameof(UpperNorfairRefill1));

    /// <summary>The <c>Door_PurpleFarming_0</c> door.</summary>
    public static CartridgeDoorHeader PurpleFarming0 { get; } = DoorDefinitions.Find(nameof(PurpleFarming0));

    /// <summary>The <c>Door_BatCave_0</c> door.</summary>
    public static CartridgeDoorHeader BatCave0 { get; } = DoorDefinitions.Find(nameof(BatCave0));

    /// <summary>The <c>Door_BatCave_1</c> door.</summary>
    public static CartridgeDoorHeader BatCave1 { get; } = DoorDefinitions.Find(nameof(BatCave1));

    /// <summary>The <c>Door_NorfairMap_0</c> door.</summary>
    public static CartridgeDoorHeader NorfairMap0 { get; } = DoorDefinitions.Find(nameof(NorfairMap0));

    /// <summary>The <c>Door_BubbleMountainSave_0</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountainSave0 { get; } = DoorDefinitions.Find(nameof(BubbleMountainSave0));

    /// <summary>The <c>Door_FrogSpeedway_0</c> door.</summary>
    public static CartridgeDoorHeader FrogSpeedway0 { get; } = DoorDefinitions.Find(nameof(FrogSpeedway0));

    /// <summary>The <c>Door_FrogSpeedway_1</c> door.</summary>
    public static CartridgeDoorHeader FrogSpeedway1 { get; } = DoorDefinitions.Find(nameof(FrogSpeedway1));

    /// <summary>The <c>Door_RedPirateShaft_0</c> door.</summary>
    public static CartridgeDoorHeader RedPirateShaft0 { get; } = DoorDefinitions.Find(nameof(RedPirateShaft0));

    /// <summary>The <c>Door_RedPirateShaft_1</c> door.</summary>
    public static CartridgeDoorHeader RedPirateShaft1 { get; } = DoorDefinitions.Find(nameof(RedPirateShaft1));

    /// <summary>The <c>Door_FrogSave_0</c> door.</summary>
    public static CartridgeDoorHeader FrogSave0 { get; } = DoorDefinitions.Find(nameof(FrogSave0));

    /// <summary>The <c>Door_FrogSave_1</c> door.</summary>
    public static CartridgeDoorHeader FrogSave1 { get; } = DoorDefinitions.Find(nameof(FrogSave1));

    /// <summary>The <c>Door_CrocomireSave_0</c> door.</summary>
    public static CartridgeDoorHeader CrocomireSave0 { get; } = DoorDefinitions.Find(nameof(CrocomireSave0));

    /// <summary>The <c>Door_LNElevSave_0</c> door.</summary>
    public static CartridgeDoorHeader LNElevSave0 { get; } = DoorDefinitions.Find(nameof(LNElevSave0));

    /// <summary>The <c>Door_AcidStatue_0</c> door.</summary>
    public static CartridgeDoorHeader AcidStatue0 { get; } = DoorDefinitions.Find(nameof(AcidStatue0));

    /// <summary>The <c>Door_AcidStatue_1</c> door.</summary>
    public static CartridgeDoorHeader AcidStatue1 { get; } = DoorDefinitions.Find(nameof(AcidStatue1));

    /// <summary>The <c>Door_MainHall_0</c> door.</summary>
    public static CartridgeDoorHeader MainHall0 { get; } = DoorDefinitions.Find(nameof(MainHall0));

    /// <summary>The <c>Door_MainHall_1</c> door.</summary>
    public static CartridgeDoorHeader MainHall1 { get; } = DoorDefinitions.Find(nameof(MainHall1));

    /// <summary>The <c>Door_MainHall_2</c> door.</summary>
    public static CartridgeDoorHeader MainHall2 { get; } = DoorDefinitions.Find(nameof(MainHall2));

    /// <summary>The <c>Door_GoldenTorizo_0</c> door.</summary>
    public static CartridgeDoorHeader GoldenTorizo0 { get; } = DoorDefinitions.Find(nameof(GoldenTorizo0));

    /// <summary>The <c>Door_GoldenTorizo_1</c> door.</summary>
    public static CartridgeDoorHeader GoldenTorizo1 { get; } = DoorDefinitions.Find(nameof(GoldenTorizo1));

    /// <summary>The <c>Door_FastRipper_0</c> door.</summary>
    public static CartridgeDoorHeader FastRipper0 { get; } = DoorDefinitions.Find(nameof(FastRipper0));

    /// <summary>The <c>Door_FastRipper_1</c> door.</summary>
    public static CartridgeDoorHeader FastRipper1 { get; } = DoorDefinitions.Find(nameof(FastRipper1));

    /// <summary>The <c>Door_GTEnergyRefill_0</c> door.</summary>
    public static CartridgeDoorHeader GTEnergyRefill0 { get; } = DoorDefinitions.Find(nameof(GTEnergyRefill0));

    /// <summary>The <c>Door_Ridley_0</c> door.</summary>
    public static CartridgeDoorHeader Ridley0 { get; } = DoorDefinitions.Find(nameof(Ridley0));

    /// <summary>The <c>Door_Ridley_1</c> door.</summary>
    public static CartridgeDoorHeader Ridley1 { get; } = DoorDefinitions.Find(nameof(Ridley1));

    /// <summary>The <c>Door_LNFarming_0</c> door.</summary>
    public static CartridgeDoorHeader LNFarming0 { get; } = DoorDefinitions.Find(nameof(LNFarming0));

    /// <summary>The <c>Door_LNFarming_1</c> door.</summary>
    public static CartridgeDoorHeader LNFarming1 { get; } = DoorDefinitions.Find(nameof(LNFarming1));

    /// <summary>The <c>Door_FastPillarsSetup_0</c> door.</summary>
    public static CartridgeDoorHeader FastPillarsSetup0 { get; } = DoorDefinitions.Find(nameof(FastPillarsSetup0));

    /// <summary>The <c>Door_FastPillarsSetup_1</c> door.</summary>
    public static CartridgeDoorHeader FastPillarsSetup1 { get; } = DoorDefinitions.Find(nameof(FastPillarsSetup1));

    /// <summary>The <c>Door_FastPillarsSetup_2</c> door.</summary>
    public static CartridgeDoorHeader FastPillarsSetup2 { get; } = DoorDefinitions.Find(nameof(FastPillarsSetup2));

    /// <summary>The <c>Door_FastPillarsSetup_3</c> door.</summary>
    public static CartridgeDoorHeader FastPillarsSetup3 { get; } = DoorDefinitions.Find(nameof(FastPillarsSetup3));

    /// <summary>The <c>Door_FastPillarsSetup_4</c> door.</summary>
    public static CartridgeDoorHeader FastPillarsSetup4 { get; } = DoorDefinitions.Find(nameof(FastPillarsSetup4));

    /// <summary>The <c>Door_MickeyMouse_0</c> door.</summary>
    public static CartridgeDoorHeader MickeyMouse0 { get; } = DoorDefinitions.Find(nameof(MickeyMouse0));

    /// <summary>The <c>Door_MickeyMouse_1</c> door.</summary>
    public static CartridgeDoorHeader MickeyMouse1 { get; } = DoorDefinitions.Find(nameof(MickeyMouse1));

    /// <summary>The <c>Door_Pillar_0</c> door.</summary>
    public static CartridgeDoorHeader Pillar0 { get; } = DoorDefinitions.Find(nameof(Pillar0));

    /// <summary>The <c>Door_Pillar_1</c> door.</summary>
    public static CartridgeDoorHeader Pillar1 { get; } = DoorDefinitions.Find(nameof(Pillar1));

    /// <summary>The <c>Door_Plowerhouse_0</c> door.</summary>
    public static CartridgeDoorHeader Plowerhouse0 { get; } = DoorDefinitions.Find(nameof(Plowerhouse0));

    /// <summary>The <c>Door_Plowerhouse_1</c> door.</summary>
    public static CartridgeDoorHeader Plowerhouse1 { get; } = DoorDefinitions.Find(nameof(Plowerhouse1));

    /// <summary>The <c>Door_WorstRoomInTheGame_0</c> door.</summary>
    public static CartridgeDoorHeader WorstRoomInTheGame0 { get; } = DoorDefinitions.Find(nameof(WorstRoomInTheGame0));

    /// <summary>The <c>Door_WorstRoomInTheGame_1</c> door.</summary>
    public static CartridgeDoorHeader WorstRoomInTheGame1 { get; } = DoorDefinitions.Find(nameof(WorstRoomInTheGame1));

    /// <summary>The <c>Door_WorstRoomInTheGame_2</c> door.</summary>
    public static CartridgeDoorHeader WorstRoomInTheGame2 { get; } = DoorDefinitions.Find(nameof(WorstRoomInTheGame2));

    /// <summary>The <c>Door_Amphitheatre_0</c> door.</summary>
    public static CartridgeDoorHeader Amphitheatre0 { get; } = DoorDefinitions.Find(nameof(Amphitheatre0));

    /// <summary>The <c>Door_Amphitheatre_1</c> door.</summary>
    public static CartridgeDoorHeader Amphitheatre1 { get; } = DoorDefinitions.Find(nameof(Amphitheatre1));

    /// <summary>The <c>Door_LNSpringBallMaze_0</c> door.</summary>
    public static CartridgeDoorHeader LNSpringBallMaze0 { get; } = DoorDefinitions.Find(nameof(LNSpringBallMaze0));

    /// <summary>The <c>Door_LNSpringBallMaze_1</c> door.</summary>
    public static CartridgeDoorHeader LNSpringBallMaze1 { get; } = DoorDefinitions.Find(nameof(LNSpringBallMaze1));

    /// <summary>The <c>Door_LNSpringBallMaze_2</c> door.</summary>
    public static CartridgeDoorHeader LNSpringBallMaze2 { get; } = DoorDefinitions.Find(nameof(LNSpringBallMaze2));

    /// <summary>The <c>Door_LNEscapePowerBombs_0</c> door.</summary>
    public static CartridgeDoorHeader LNEscapePowerBombs0 { get; } = DoorDefinitions.Find(nameof(LNEscapePowerBombs0));

    /// <summary>The <c>Door_LNEscapePowerBombs_1</c> door.</summary>
    public static CartridgeDoorHeader LNEscapePowerBombs1 { get; } = DoorDefinitions.Find(nameof(LNEscapePowerBombs1));

    /// <summary>The <c>Door_RedKihunterShaft_0</c> door.</summary>
    public static CartridgeDoorHeader RedKihunterShaft0 { get; } = DoorDefinitions.Find(nameof(RedKihunterShaft0));

    /// <summary>The <c>Door_RedKihunterShaft_1</c> door.</summary>
    public static CartridgeDoorHeader RedKihunterShaft1 { get; } = DoorDefinitions.Find(nameof(RedKihunterShaft1));

    /// <summary>The <c>Door_RedKihunterShaft_2</c> door.</summary>
    public static CartridgeDoorHeader RedKihunterShaft2 { get; } = DoorDefinitions.Find(nameof(RedKihunterShaft2));

    /// <summary>The <c>Door_RedKihunterShaft_3</c> door.</summary>
    public static CartridgeDoorHeader RedKihunterShaft3 { get; } = DoorDefinitions.Find(nameof(RedKihunterShaft3));

    /// <summary>The <c>Door_Wasteland_0</c> door.</summary>
    public static CartridgeDoorHeader Wasteland0 { get; } = DoorDefinitions.Find(nameof(Wasteland0));

    /// <summary>The <c>Door_Wasteland_1</c> door.</summary>
    public static CartridgeDoorHeader Wasteland1 { get; } = DoorDefinitions.Find(nameof(Wasteland1));

    /// <summary>The <c>Door_MetalPirates_0</c> door.</summary>
    public static CartridgeDoorHeader MetalPirates0 { get; } = DoorDefinitions.Find(nameof(MetalPirates0));

    /// <summary>The <c>Door_MetalPirates_1</c> door.</summary>
    public static CartridgeDoorHeader MetalPirates1 { get; } = DoorDefinitions.Find(nameof(MetalPirates1));

    /// <summary>The <c>Door_ThreeMusketeers_0</c> door.</summary>
    public static CartridgeDoorHeader ThreeMusketeers0 { get; } = DoorDefinitions.Find(nameof(ThreeMusketeers0));

    /// <summary>The <c>Door_ThreeMusketeers_1</c> door.</summary>
    public static CartridgeDoorHeader ThreeMusketeers1 { get; } = DoorDefinitions.Find(nameof(ThreeMusketeers1));

    /// <summary>The <c>Door_RidleyETank_0</c> door.</summary>
    public static CartridgeDoorHeader RidleyETank0 { get; } = DoorDefinitions.Find(nameof(RidleyETank0));

    /// <summary>The <c>Door_ScrewAttack_0</c> door.</summary>
    public static CartridgeDoorHeader ScrewAttack0 { get; } = DoorDefinitions.Find(nameof(ScrewAttack0));

    /// <summary>The <c>Door_ScrewAttack_1</c> door.</summary>
    public static CartridgeDoorHeader ScrewAttack1 { get; } = DoorDefinitions.Find(nameof(ScrewAttack1));

    /// <summary>The <c>Door_ScrewAttack_2</c> door.</summary>
    public static CartridgeDoorHeader ScrewAttack2 { get; } = DoorDefinitions.Find(nameof(ScrewAttack2));

    /// <summary>The <c>Door_LNFireflea_0</c> door.</summary>
    public static CartridgeDoorHeader LNFireflea0 { get; } = DoorDefinitions.Find(nameof(LNFireflea0));

    /// <summary>The <c>Door_LNFireflea_1</c> door.</summary>
    public static CartridgeDoorHeader LNFireflea1 { get; } = DoorDefinitions.Find(nameof(LNFireflea1));

    /// <summary>The <c>Door_LNFireflea_2</c> door.</summary>
    public static CartridgeDoorHeader LNFireflea2 { get; } = DoorDefinitions.Find(nameof(LNFireflea2));

    /// <summary>The <c>Door_LNSave_0</c> door.</summary>
    public static CartridgeDoorHeader LNSave0 { get; } = DoorDefinitions.Find(nameof(LNSave0));
}
