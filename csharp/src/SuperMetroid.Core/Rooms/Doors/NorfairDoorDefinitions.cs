namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Norfair rooms' doors, named by the room they lead from.</summary>
[NumberedIdentities("Each numbered door is one door of its room, named by its place in that room's door list; every door has its own destination and is used by name, never iterated or indexed.")]
public static class NorfairDoorDefinitions
{
    /// <summary>The Ice Beam Acid room's door 0.</summary>
    public static DoorDefinition IceBeamAcid0 { get; } = DoorDefinitions.Find(nameof(IceBeamAcid0));

    /// <summary>The Ice Beam Acid room's door 1.</summary>
    public static DoorDefinition IceBeamAcid1 { get; } = DoorDefinitions.Find(nameof(IceBeamAcid1));

    /// <summary>The Cathedral room's door 0.</summary>
    public static DoorDefinition Cathedral0 { get; } = DoorDefinitions.Find(nameof(Cathedral0));

    /// <summary>The Cathedral room's door 1.</summary>
    public static DoorDefinition Cathedral1 { get; } = DoorDefinitions.Find(nameof(Cathedral1));

    /// <summary>The Cathedral Entrance room's door 0.</summary>
    public static DoorDefinition CathedralEntrance0 { get; } = DoorDefinitions.Find(nameof(CathedralEntrance0));

    /// <summary>The Cathedral Entrance room's door 1.</summary>
    public static DoorDefinition CathedralEntrance1 { get; } = DoorDefinitions.Find(nameof(CathedralEntrance1));

    /// <summary>The Business Center room's door 0.</summary>
    public static DoorDefinition BusinessCenter0 { get; } = DoorDefinitions.Find(nameof(BusinessCenter0));

    /// <summary>The Business Center room's door 1.</summary>
    public static DoorDefinition BusinessCenter1 { get; } = DoorDefinitions.Find(nameof(BusinessCenter1));

    /// <summary>The Business Center room's door 2.</summary>
    public static DoorDefinition BusinessCenter2 { get; } = DoorDefinitions.Find(nameof(BusinessCenter2));

    /// <summary>The Business Center room's door 3.</summary>
    public static DoorDefinition BusinessCenter3 { get; } = DoorDefinitions.Find(nameof(BusinessCenter3));

    /// <summary>The Business Center room's door 4.</summary>
    public static DoorDefinition BusinessCenter4 { get; } = DoorDefinitions.Find(nameof(BusinessCenter4));

    /// <summary>The Business Center room's door 6.</summary>
    public static DoorDefinition BusinessCenter6 { get; } = DoorDefinitions.Find(nameof(BusinessCenter6));

    /// <summary>The Business Center room's door 7.</summary>
    public static DoorDefinition BusinessCenter7 { get; } = DoorDefinitions.Find(nameof(BusinessCenter7));

    /// <summary>The Ice Beam Gate room's door 0.</summary>
    public static DoorDefinition IceBeamGate0 { get; } = DoorDefinitions.Find(nameof(IceBeamGate0));

    /// <summary>The Ice Beam Gate room's door 1.</summary>
    public static DoorDefinition IceBeamGate1 { get; } = DoorDefinitions.Find(nameof(IceBeamGate1));

    /// <summary>The Ice Beam Gate room's door 2.</summary>
    public static DoorDefinition IceBeamGate2 { get; } = DoorDefinitions.Find(nameof(IceBeamGate2));

    /// <summary>The Ice Beam Gate room's door 3.</summary>
    public static DoorDefinition IceBeamGate3 { get; } = DoorDefinitions.Find(nameof(IceBeamGate3));

    /// <summary>The Ice Beam Tutorial room's door 0.</summary>
    public static DoorDefinition IceBeamTutorial0 { get; } = DoorDefinitions.Find(nameof(IceBeamTutorial0));

    /// <summary>The Ice Beam Tutorial room's door 1.</summary>
    public static DoorDefinition IceBeamTutorial1 { get; } = DoorDefinitions.Find(nameof(IceBeamTutorial1));

    /// <summary>The Ice Beam room's door 0.</summary>
    public static DoorDefinition IceBeam0 { get; } = DoorDefinitions.Find(nameof(IceBeam0));

    /// <summary>The Ice Beam Snake room's door 0.</summary>
    public static DoorDefinition IceBeamSnake0 { get; } = DoorDefinitions.Find(nameof(IceBeamSnake0));

    /// <summary>The Ice Beam Snake room's door 1.</summary>
    public static DoorDefinition IceBeamSnake1 { get; } = DoorDefinitions.Find(nameof(IceBeamSnake1));

    /// <summary>The Ice Beam Snake room's door 2.</summary>
    public static DoorDefinition IceBeamSnake2 { get; } = DoorDefinitions.Find(nameof(IceBeamSnake2));

    /// <summary>The Crumble Shaft room's door 0.</summary>
    public static DoorDefinition CrumbleShaft0 { get; } = DoorDefinitions.Find(nameof(CrumbleShaft0));

    /// <summary>The Crumble Shaft room's door 1.</summary>
    public static DoorDefinition CrumbleShaft1 { get; } = DoorDefinitions.Find(nameof(CrumbleShaft1));

    /// <summary>The Crocomire Speedway room's door 0.</summary>
    public static DoorDefinition CrocomireSpeedway0 { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedway0));

    /// <summary>The Crocomire Speedway room's door 1.</summary>
    public static DoorDefinition CrocomireSpeedway1 { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedway1));

    /// <summary>The Crocomire Speedway room's door 2.</summary>
    public static DoorDefinition CrocomireSpeedway2 { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedway2));

    /// <summary>The Crocomire Speedway room's door 3.</summary>
    public static DoorDefinition CrocomireSpeedway3 { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedway3));

    /// <summary>The Crocomire Speedway room's door 4.</summary>
    public static DoorDefinition CrocomireSpeedway4 { get; } = DoorDefinitions.Find(nameof(CrocomireSpeedway4));

    /// <summary>The Crocomire room's door 0.</summary>
    public static DoorDefinition Crocomire0 { get; } = DoorDefinitions.Find(nameof(Crocomire0));

    /// <summary>The Crocomire room's door 1.</summary>
    public static DoorDefinition Crocomire1 { get; } = DoorDefinitions.Find(nameof(Crocomire1));

    /// <summary>The Hi-Jump Boots room's door 0.</summary>
    public static DoorDefinition HiJumpBoots0 { get; } = DoorDefinitions.Find(nameof(HiJumpBoots0));

    /// <summary>The Crocomire Escape room's door 0.</summary>
    public static DoorDefinition CrocomireEscape0 { get; } = DoorDefinitions.Find(nameof(CrocomireEscape0));

    /// <summary>The Crocomire Escape room's door 1.</summary>
    public static DoorDefinition CrocomireEscape1 { get; } = DoorDefinitions.Find(nameof(CrocomireEscape1));

    /// <summary>The Hi-Jump Energy Tank room's door 0.</summary>
    public static DoorDefinition HiJumpETank0 { get; } = DoorDefinitions.Find(nameof(HiJumpETank0));

    /// <summary>The Hi-Jump Energy Tank room's door 1.</summary>
    public static DoorDefinition HiJumpETank1 { get; } = DoorDefinitions.Find(nameof(HiJumpETank1));

    /// <summary>The Post-Crocomire Farming room's door 0.</summary>
    public static DoorDefinition PostCrocFarming0 { get; } = DoorDefinitions.Find(nameof(PostCrocFarming0));

    /// <summary>The Post-Crocomire Farming room's door 1.</summary>
    public static DoorDefinition PostCrocFarming1 { get; } = DoorDefinitions.Find(nameof(PostCrocFarming1));

    /// <summary>The Post-Crocomire Farming room's door 2.</summary>
    public static DoorDefinition PostCrocFarming2 { get; } = DoorDefinitions.Find(nameof(PostCrocFarming2));

    /// <summary>The Post-Crocomire Farming room's door 3.</summary>
    public static DoorDefinition PostCrocFarming3 { get; } = DoorDefinitions.Find(nameof(PostCrocFarming3));

    /// <summary>The Post-Crocomire Save room's door 0.</summary>
    public static DoorDefinition PostCrocSave0 { get; } = DoorDefinitions.Find(nameof(PostCrocSave0));

    /// <summary>The Post-Crocomire Power Bombs room's door 0.</summary>
    public static DoorDefinition PostCrocPowerBombs0 { get; } = DoorDefinitions.Find(nameof(PostCrocPowerBombs0));

    /// <summary>The Post-Crocomire Shaft room's door 0.</summary>
    public static DoorDefinition PostCrocShaft0 { get; } = DoorDefinitions.Find(nameof(PostCrocShaft0));

    /// <summary>The Post-Crocomire Shaft room's door 1.</summary>
    public static DoorDefinition PostCrocShaft1 { get; } = DoorDefinitions.Find(nameof(PostCrocShaft1));

    /// <summary>The Post-Crocomire Shaft room's door 2.</summary>
    public static DoorDefinition PostCrocShaft2 { get; } = DoorDefinitions.Find(nameof(PostCrocShaft2));

    /// <summary>The Post-Crocomire Shaft room's door 3.</summary>
    public static DoorDefinition PostCrocShaft3 { get; } = DoorDefinitions.Find(nameof(PostCrocShaft3));

    /// <summary>The Post-Crocomire Missile room's door 0.</summary>
    public static DoorDefinition PostCrocMissile0 { get; } = DoorDefinitions.Find(nameof(PostCrocMissile0));

    /// <summary>The Grapple Tutorial 3 room's door 0.</summary>
    public static DoorDefinition GrappleTutorial30 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial30));

    /// <summary>The Grapple Tutorial 3 room's door 1.</summary>
    public static DoorDefinition GrappleTutorial31 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial31));

    /// <summary>The Post-Crocomire Jump room's door 0.</summary>
    public static DoorDefinition PostCrocJump0 { get; } = DoorDefinitions.Find(nameof(PostCrocJump0));

    /// <summary>The Post-Crocomire Jump room's door 1.</summary>
    public static DoorDefinition PostCrocJump1 { get; } = DoorDefinitions.Find(nameof(PostCrocJump1));

    /// <summary>The Grapple Tutorial 2 room's door 0.</summary>
    public static DoorDefinition GrappleTutorial20 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial20));

    /// <summary>The Grapple Tutorial 2 room's door 1.</summary>
    public static DoorDefinition GrappleTutorial21 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial21));

    /// <summary>The Grapple Tutorial 1 room's door 0.</summary>
    public static DoorDefinition GrappleTutorial10 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial10));

    /// <summary>The Grapple Tutorial 1 room's door 1.</summary>
    public static DoorDefinition GrappleTutorial11 { get; } = DoorDefinitions.Find(nameof(GrappleTutorial11));

    /// <summary>The Grapple Beam room's door 0.</summary>
    public static DoorDefinition GrappleBeam0 { get; } = DoorDefinitions.Find(nameof(GrappleBeam0));

    /// <summary>The Grapple Beam room's door 1.</summary>
    public static DoorDefinition GrappleBeam1 { get; } = DoorDefinitions.Find(nameof(GrappleBeam1));

    /// <summary>The Norfair Reserve Tank room's door.</summary>
    public static DoorDefinition NorfairReserveTank { get; } = DoorDefinitions.Find(nameof(NorfairReserveTank));

    /// <summary>The Green Bubbles Missiles room's door 0.</summary>
    public static DoorDefinition GreenBubblesMissiles0 { get; } = DoorDefinitions.Find(nameof(GreenBubblesMissiles0));

    /// <summary>The Green Bubbles Missiles room's door 1.</summary>
    public static DoorDefinition GreenBubblesMissiles1 { get; } = DoorDefinitions.Find(nameof(GreenBubblesMissiles1));

    /// <summary>The Bubble Mountain room's door 0.</summary>
    public static DoorDefinition BubbleMountain0 { get; } = DoorDefinitions.Find(nameof(BubbleMountain0));

    /// <summary>The Bubble Mountain room's door 1.</summary>
    public static DoorDefinition BubbleMountain1 { get; } = DoorDefinitions.Find(nameof(BubbleMountain1));

    /// <summary>The Bubble Mountain room's door 2.</summary>
    public static DoorDefinition BubbleMountain2 { get; } = DoorDefinitions.Find(nameof(BubbleMountain2));

    /// <summary>The Bubble Mountain room's door 3.</summary>
    public static DoorDefinition BubbleMountain3 { get; } = DoorDefinitions.Find(nameof(BubbleMountain3));

    /// <summary>The Bubble Mountain room's door 4.</summary>
    public static DoorDefinition BubbleMountain4 { get; } = DoorDefinitions.Find(nameof(BubbleMountain4));

    /// <summary>The Bubble Mountain room's door 5.</summary>
    public static DoorDefinition BubbleMountain5 { get; } = DoorDefinitions.Find(nameof(BubbleMountain5));

    /// <summary>The Bubble Mountain room's door 6.</summary>
    public static DoorDefinition BubbleMountain6 { get; } = DoorDefinitions.Find(nameof(BubbleMountain6));

    /// <summary>The Speed Booster Hall room's door 0.</summary>
    public static DoorDefinition SpeedBoosterHall0 { get; } = DoorDefinitions.Find(nameof(SpeedBoosterHall0));

    /// <summary>The Speed Booster Hall room's door 1.</summary>
    public static DoorDefinition SpeedBoosterHall1 { get; } = DoorDefinitions.Find(nameof(SpeedBoosterHall1));

    /// <summary>The Speed Booster room's door 0.</summary>
    public static DoorDefinition SpeedBooster0 { get; } = DoorDefinitions.Find(nameof(SpeedBooster0));

    /// <summary>The Single Chamber room's door 0.</summary>
    public static DoorDefinition SingleChamber0 { get; } = DoorDefinitions.Find(nameof(SingleChamber0));

    /// <summary>The Single Chamber room's door 1.</summary>
    public static DoorDefinition SingleChamber1 { get; } = DoorDefinitions.Find(nameof(SingleChamber1));

    /// <summary>The Single Chamber room's door 2.</summary>
    public static DoorDefinition SingleChamber2 { get; } = DoorDefinitions.Find(nameof(SingleChamber2));

    /// <summary>The Single Chamber room's door 3.</summary>
    public static DoorDefinition SingleChamber3 { get; } = DoorDefinitions.Find(nameof(SingleChamber3));

    /// <summary>The Single Chamber room's door 4.</summary>
    public static DoorDefinition SingleChamber4 { get; } = DoorDefinitions.Find(nameof(SingleChamber4));

    /// <summary>The Double Chamber room's door 0.</summary>
    public static DoorDefinition DoubleChamber0 { get; } = DoorDefinitions.Find(nameof(DoubleChamber0));

    /// <summary>The Double Chamber room's door 1.</summary>
    public static DoorDefinition DoubleChamber1 { get; } = DoorDefinitions.Find(nameof(DoubleChamber1));

    /// <summary>The Double Chamber room's door 2.</summary>
    public static DoorDefinition DoubleChamber2 { get; } = DoorDefinitions.Find(nameof(DoubleChamber2));

    /// <summary>The Wave Beam room's door 0.</summary>
    public static DoorDefinition WaveBeam0 { get; } = DoorDefinitions.Find(nameof(WaveBeam0));

    /// <summary>The Spiky Platforms Tunnel room's door 0.</summary>
    public static DoorDefinition SpikyPlatformsTunnel0 { get; } = DoorDefinitions.Find(nameof(SpikyPlatformsTunnel0));

    /// <summary>The Spiky Platforms Tunnel room's door 1.</summary>
    public static DoorDefinition SpikyPlatformsTunnel1 { get; } = DoorDefinitions.Find(nameof(SpikyPlatformsTunnel1));

    /// <summary>The Volcano room's door 0.</summary>
    public static DoorDefinition Volcano0 { get; } = DoorDefinitions.Find(nameof(Volcano0));

    /// <summary>The Volcano room's door 1.</summary>
    public static DoorDefinition Volcano1 { get; } = DoorDefinitions.Find(nameof(Volcano1));

    /// <summary>The Kronic Boost room's door 0.</summary>
    public static DoorDefinition KronicBoost0 { get; } = DoorDefinitions.Find(nameof(KronicBoost0));

    /// <summary>The Kronic Boost room's door 1.</summary>
    public static DoorDefinition KronicBoost1 { get; } = DoorDefinitions.Find(nameof(KronicBoost1));

    /// <summary>The Kronic Boost room's door 2.</summary>
    public static DoorDefinition KronicBoost2 { get; } = DoorDefinitions.Find(nameof(KronicBoost2));

    /// <summary>The Kronic Boost room's door 3.</summary>
    public static DoorDefinition KronicBoost3 { get; } = DoorDefinitions.Find(nameof(KronicBoost3));

    /// <summary>The Magdollite Tunnel room's door 0.</summary>
    public static DoorDefinition MagdolliteTunnel0 { get; } = DoorDefinitions.Find(nameof(MagdolliteTunnel0));

    /// <summary>The Magdollite Tunnel room's door 1.</summary>
    public static DoorDefinition MagdolliteTunnel1 { get; } = DoorDefinitions.Find(nameof(MagdolliteTunnel1));

    /// <summary>The Purple Shaft room's door 0.</summary>
    public static DoorDefinition PurpleShaft0 { get; } = DoorDefinitions.Find(nameof(PurpleShaft0));

    /// <summary>The Purple Shaft room's door 1.</summary>
    public static DoorDefinition PurpleShaft1 { get; } = DoorDefinitions.Find(nameof(PurpleShaft1));

    /// <summary>The Purple Shaft room's door 2.</summary>
    public static DoorDefinition PurpleShaft2 { get; } = DoorDefinitions.Find(nameof(PurpleShaft2));

    /// <summary>The Lava Dive room's door 0.</summary>
    public static DoorDefinition LavaDive0 { get; } = DoorDefinitions.Find(nameof(LavaDive0));

    /// <summary>The Lava Dive room's door 1.</summary>
    public static DoorDefinition LavaDive1 { get; } = DoorDefinitions.Find(nameof(LavaDive1));

    /// <summary>The Lower Norfair Elevator room's door 0.</summary>
    public static DoorDefinition LowerNorfairElev0 { get; } = DoorDefinitions.Find(nameof(LowerNorfairElev0));

    /// <summary>The Lower Norfair Elevator room's door 1.</summary>
    public static DoorDefinition LowerNorfairElev1 { get; } = DoorDefinitions.Find(nameof(LowerNorfairElev1));

    /// <summary>The Lower Norfair Elevator room's door 3.</summary>
    public static DoorDefinition LowerNorfairElev3 { get; } = DoorDefinitions.Find(nameof(LowerNorfairElev3));

    /// <summary>The Upper Norfair Farming room's door 0.</summary>
    public static DoorDefinition UpperNorfairFarming0 { get; } = DoorDefinitions.Find(nameof(UpperNorfairFarming0));

    /// <summary>The Upper Norfair Farming room's door 1.</summary>
    public static DoorDefinition UpperNorfairFarming1 { get; } = DoorDefinitions.Find(nameof(UpperNorfairFarming1));

    /// <summary>The Upper Norfair Farming room's door 2.</summary>
    public static DoorDefinition UpperNorfairFarming2 { get; } = DoorDefinitions.Find(nameof(UpperNorfairFarming2));

    /// <summary>The Rising Tide room's door 0.</summary>
    public static DoorDefinition RisingTide0 { get; } = DoorDefinitions.Find(nameof(RisingTide0));

    /// <summary>The Rising Tide room's door 1.</summary>
    public static DoorDefinition RisingTide1 { get; } = DoorDefinitions.Find(nameof(RisingTide1));

    /// <summary>The Acid Snakes Tunnel room's door 0.</summary>
    public static DoorDefinition AcidSnakesTunnel0 { get; } = DoorDefinitions.Find(nameof(AcidSnakesTunnel0));

    /// <summary>The Acid Snakes Tunnel room's door 1.</summary>
    public static DoorDefinition AcidSnakesTunnel1 { get; } = DoorDefinitions.Find(nameof(AcidSnakesTunnel1));

    /// <summary>The Acid Snakes Tunnel room's door 2.</summary>
    public static DoorDefinition AcidSnakesTunnel2 { get; } = DoorDefinitions.Find(nameof(AcidSnakesTunnel2));

    /// <summary>The Spiky Acid Snakes Tunnel room's door 0.</summary>
    public static DoorDefinition SpikyAcidSnakesTunnel0 { get; } = DoorDefinitions.Find(nameof(SpikyAcidSnakesTunnel0));

    /// <summary>The Spiky Acid Snakes Tunnel room's door 1.</summary>
    public static DoorDefinition SpikyAcidSnakesTunnel1 { get; } = DoorDefinitions.Find(nameof(SpikyAcidSnakesTunnel1));

    /// <summary>The Upper Norfair Refill room's door 0.</summary>
    public static DoorDefinition UpperNorfairRefill0 { get; } = DoorDefinitions.Find(nameof(UpperNorfairRefill0));

    /// <summary>The Upper Norfair Refill room's door 1.</summary>
    public static DoorDefinition UpperNorfairRefill1 { get; } = DoorDefinitions.Find(nameof(UpperNorfairRefill1));

    /// <summary>The Purple Farming room's door 0.</summary>
    public static DoorDefinition PurpleFarming0 { get; } = DoorDefinitions.Find(nameof(PurpleFarming0));

    /// <summary>The Bat Cave room's door 0.</summary>
    public static DoorDefinition BatCave0 { get; } = DoorDefinitions.Find(nameof(BatCave0));

    /// <summary>The Bat Cave room's door 1.</summary>
    public static DoorDefinition BatCave1 { get; } = DoorDefinitions.Find(nameof(BatCave1));

    /// <summary>The Norfair Map room's door 0.</summary>
    public static DoorDefinition NorfairMap0 { get; } = DoorDefinitions.Find(nameof(NorfairMap0));

    /// <summary>The Bubble Mountain Save room's door 0.</summary>
    public static DoorDefinition BubbleMountainSave0 { get; } = DoorDefinitions.Find(nameof(BubbleMountainSave0));

    /// <summary>The Frog Speedway room's door 0.</summary>
    public static DoorDefinition FrogSpeedway0 { get; } = DoorDefinitions.Find(nameof(FrogSpeedway0));

    /// <summary>The Frog Speedway room's door 1.</summary>
    public static DoorDefinition FrogSpeedway1 { get; } = DoorDefinitions.Find(nameof(FrogSpeedway1));

    /// <summary>The Red Pirate Shaft room's door 0.</summary>
    public static DoorDefinition RedPirateShaft0 { get; } = DoorDefinitions.Find(nameof(RedPirateShaft0));

    /// <summary>The Red Pirate Shaft room's door 1.</summary>
    public static DoorDefinition RedPirateShaft1 { get; } = DoorDefinitions.Find(nameof(RedPirateShaft1));

    /// <summary>The Frog Save room's door 0.</summary>
    public static DoorDefinition FrogSave0 { get; } = DoorDefinitions.Find(nameof(FrogSave0));

    /// <summary>The Frog Save room's door 1.</summary>
    public static DoorDefinition FrogSave1 { get; } = DoorDefinitions.Find(nameof(FrogSave1));

    /// <summary>The Crocomire Save room's door 0.</summary>
    public static DoorDefinition CrocomireSave0 { get; } = DoorDefinitions.Find(nameof(CrocomireSave0));

    /// <summary>The Lower Norfair Elevator Save room's door 0.</summary>
    public static DoorDefinition LNElevSave0 { get; } = DoorDefinitions.Find(nameof(LNElevSave0));

    /// <summary>The Acid Statue room's door 0.</summary>
    public static DoorDefinition AcidStatue0 { get; } = DoorDefinitions.Find(nameof(AcidStatue0));

    /// <summary>The Acid Statue room's door 1.</summary>
    public static DoorDefinition AcidStatue1 { get; } = DoorDefinitions.Find(nameof(AcidStatue1));

    /// <summary>The Main Hall room's door 0.</summary>
    public static DoorDefinition MainHall0 { get; } = DoorDefinitions.Find(nameof(MainHall0));

    /// <summary>The Main Hall room's door 1.</summary>
    public static DoorDefinition MainHall1 { get; } = DoorDefinitions.Find(nameof(MainHall1));

    /// <summary>The Main Hall room's door 2.</summary>
    public static DoorDefinition MainHall2 { get; } = DoorDefinitions.Find(nameof(MainHall2));

    /// <summary>The Golden Torizo room's door 0.</summary>
    public static DoorDefinition GoldenTorizo0 { get; } = DoorDefinitions.Find(nameof(GoldenTorizo0));

    /// <summary>The Golden Torizo room's door 1.</summary>
    public static DoorDefinition GoldenTorizo1 { get; } = DoorDefinitions.Find(nameof(GoldenTorizo1));

    /// <summary>The Fast Ripper room's door 0.</summary>
    public static DoorDefinition FastRipper0 { get; } = DoorDefinitions.Find(nameof(FastRipper0));

    /// <summary>The Fast Ripper room's door 1.</summary>
    public static DoorDefinition FastRipper1 { get; } = DoorDefinitions.Find(nameof(FastRipper1));

    /// <summary>The Golden Torizo Energy Refill room's door 0.</summary>
    public static DoorDefinition GTEnergyRefill0 { get; } = DoorDefinitions.Find(nameof(GTEnergyRefill0));

    /// <summary>The Ridley room's door 0.</summary>
    public static DoorDefinition Ridley0 { get; } = DoorDefinitions.Find(nameof(Ridley0));

    /// <summary>The Ridley room's door 1.</summary>
    public static DoorDefinition Ridley1 { get; } = DoorDefinitions.Find(nameof(Ridley1));

    /// <summary>The Lower Norfair Farming room's door 0.</summary>
    public static DoorDefinition LNFarming0 { get; } = DoorDefinitions.Find(nameof(LNFarming0));

    /// <summary>The Lower Norfair Farming room's door 1.</summary>
    public static DoorDefinition LNFarming1 { get; } = DoorDefinitions.Find(nameof(LNFarming1));

    /// <summary>The Fast Pillars Setup room's door 0.</summary>
    public static DoorDefinition FastPillarsSetup0 { get; } = DoorDefinitions.Find(nameof(FastPillarsSetup0));

    /// <summary>The Fast Pillars Setup room's door 1.</summary>
    public static DoorDefinition FastPillarsSetup1 { get; } = DoorDefinitions.Find(nameof(FastPillarsSetup1));

    /// <summary>The Fast Pillars Setup room's door 2.</summary>
    public static DoorDefinition FastPillarsSetup2 { get; } = DoorDefinitions.Find(nameof(FastPillarsSetup2));

    /// <summary>The Fast Pillars Setup room's door 3.</summary>
    public static DoorDefinition FastPillarsSetup3 { get; } = DoorDefinitions.Find(nameof(FastPillarsSetup3));

    /// <summary>The Fast Pillars Setup room's door 4.</summary>
    public static DoorDefinition FastPillarsSetup4 { get; } = DoorDefinitions.Find(nameof(FastPillarsSetup4));

    /// <summary>The Mickey Mouse room's door 0.</summary>
    public static DoorDefinition MickeyMouse0 { get; } = DoorDefinitions.Find(nameof(MickeyMouse0));

    /// <summary>The Mickey Mouse room's door 1.</summary>
    public static DoorDefinition MickeyMouse1 { get; } = DoorDefinitions.Find(nameof(MickeyMouse1));

    /// <summary>The Pillar room's door 0.</summary>
    public static DoorDefinition Pillar0 { get; } = DoorDefinitions.Find(nameof(Pillar0));

    /// <summary>The Pillar room's door 1.</summary>
    public static DoorDefinition Pillar1 { get; } = DoorDefinitions.Find(nameof(Pillar1));

    /// <summary>The Plowerhouse room's door 0.</summary>
    public static DoorDefinition Plowerhouse0 { get; } = DoorDefinitions.Find(nameof(Plowerhouse0));

    /// <summary>The Plowerhouse room's door 1.</summary>
    public static DoorDefinition Plowerhouse1 { get; } = DoorDefinitions.Find(nameof(Plowerhouse1));

    /// <summary>The Worst Room in the Game's door 0.</summary>
    public static DoorDefinition WorstRoomInTheGame0 { get; } = DoorDefinitions.Find(nameof(WorstRoomInTheGame0));

    /// <summary>The Worst Room in the Game's door 1.</summary>
    public static DoorDefinition WorstRoomInTheGame1 { get; } = DoorDefinitions.Find(nameof(WorstRoomInTheGame1));

    /// <summary>The Worst Room in the Game's door 2.</summary>
    public static DoorDefinition WorstRoomInTheGame2 { get; } = DoorDefinitions.Find(nameof(WorstRoomInTheGame2));

    /// <summary>The Amphitheatre room's door 0.</summary>
    public static DoorDefinition Amphitheatre0 { get; } = DoorDefinitions.Find(nameof(Amphitheatre0));

    /// <summary>The Amphitheatre room's door 1.</summary>
    public static DoorDefinition Amphitheatre1 { get; } = DoorDefinitions.Find(nameof(Amphitheatre1));

    /// <summary>The Lower Norfair Spring Ball Maze room's door 0.</summary>
    public static DoorDefinition LNSpringBallMaze0 { get; } = DoorDefinitions.Find(nameof(LNSpringBallMaze0));

    /// <summary>The Lower Norfair Spring Ball Maze room's door 1.</summary>
    public static DoorDefinition LNSpringBallMaze1 { get; } = DoorDefinitions.Find(nameof(LNSpringBallMaze1));

    /// <summary>The Lower Norfair Spring Ball Maze room's door 2.</summary>
    public static DoorDefinition LNSpringBallMaze2 { get; } = DoorDefinitions.Find(nameof(LNSpringBallMaze2));

    /// <summary>The Lower Norfair Escape Power Bombs room's door 0.</summary>
    public static DoorDefinition LNEscapePowerBombs0 { get; } = DoorDefinitions.Find(nameof(LNEscapePowerBombs0));

    /// <summary>The Lower Norfair Escape Power Bombs room's door 1.</summary>
    public static DoorDefinition LNEscapePowerBombs1 { get; } = DoorDefinitions.Find(nameof(LNEscapePowerBombs1));

    /// <summary>The Red Kihunter Shaft room's door 0.</summary>
    public static DoorDefinition RedKihunterShaft0 { get; } = DoorDefinitions.Find(nameof(RedKihunterShaft0));

    /// <summary>The Red Kihunter Shaft room's door 1.</summary>
    public static DoorDefinition RedKihunterShaft1 { get; } = DoorDefinitions.Find(nameof(RedKihunterShaft1));

    /// <summary>The Red Kihunter Shaft room's door 2.</summary>
    public static DoorDefinition RedKihunterShaft2 { get; } = DoorDefinitions.Find(nameof(RedKihunterShaft2));

    /// <summary>The Red Kihunter Shaft room's door 3.</summary>
    public static DoorDefinition RedKihunterShaft3 { get; } = DoorDefinitions.Find(nameof(RedKihunterShaft3));

    /// <summary>The Wasteland room's door 0.</summary>
    public static DoorDefinition Wasteland0 { get; } = DoorDefinitions.Find(nameof(Wasteland0));

    /// <summary>The Wasteland room's door 1.</summary>
    public static DoorDefinition Wasteland1 { get; } = DoorDefinitions.Find(nameof(Wasteland1));

    /// <summary>The Metal Pirates room's door 0.</summary>
    public static DoorDefinition MetalPirates0 { get; } = DoorDefinitions.Find(nameof(MetalPirates0));

    /// <summary>The Metal Pirates room's door 1.</summary>
    public static DoorDefinition MetalPirates1 { get; } = DoorDefinitions.Find(nameof(MetalPirates1));

    /// <summary>The Three Musketeers room's door 0.</summary>
    public static DoorDefinition ThreeMusketeers0 { get; } = DoorDefinitions.Find(nameof(ThreeMusketeers0));

    /// <summary>The Three Musketeers room's door 1.</summary>
    public static DoorDefinition ThreeMusketeers1 { get; } = DoorDefinitions.Find(nameof(ThreeMusketeers1));

    /// <summary>The Ridley Energy Tank room's door 0.</summary>
    public static DoorDefinition RidleyETank0 { get; } = DoorDefinitions.Find(nameof(RidleyETank0));

    /// <summary>The Screw Attack room's door 0.</summary>
    public static DoorDefinition ScrewAttack0 { get; } = DoorDefinitions.Find(nameof(ScrewAttack0));

    /// <summary>The Screw Attack room's door 1.</summary>
    public static DoorDefinition ScrewAttack1 { get; } = DoorDefinitions.Find(nameof(ScrewAttack1));

    /// <summary>The Screw Attack room's door 2.</summary>
    public static DoorDefinition ScrewAttack2 { get; } = DoorDefinitions.Find(nameof(ScrewAttack2));

    /// <summary>The Lower Norfair Fireflea room's door 0.</summary>
    public static DoorDefinition LNFireflea0 { get; } = DoorDefinitions.Find(nameof(LNFireflea0));

    /// <summary>The Lower Norfair Fireflea room's door 1.</summary>
    public static DoorDefinition LNFireflea1 { get; } = DoorDefinitions.Find(nameof(LNFireflea1));

    /// <summary>The Lower Norfair Fireflea room's door 2.</summary>
    public static DoorDefinition LNFireflea2 { get; } = DoorDefinitions.Find(nameof(LNFireflea2));

    /// <summary>The Lower Norfair Save room's door 0.</summary>
    public static DoorDefinition LNSave0 { get; } = DoorDefinitions.Find(nameof(LNSave0));
}
