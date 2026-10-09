using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Cartridge values of <see cref="AnimatedTileInstructionListPointers"/> that only verification reads.</summary>
internal static class AnimatedTileInstructionListPointersConstants
{
    /// <summary>Leftward treadmill four-frame loop at $87:81F9.</summary>
    public const ushort WreckedShipTreadmillLeftwardsLoop = 0x81f9;
    /// <summary>Rightward treadmill four-frame loop at $87:81E3.</summary>
    public const ushort WreckedShipTreadmillRightwardsLoop = 0x81e3;

    extension(AnimatedTileInstructionListPointers)
    {
        /// <inheritdoc cref="AnimatedTileInstructionListPointersConstants.WreckedShipTreadmillLeftwardsLoop"/>
        internal static ushort WreckedShipTreadmillLeftwardsLoop => AnimatedTileInstructionListPointersConstants.WreckedShipTreadmillLeftwardsLoop;
        /// <inheritdoc cref="AnimatedTileInstructionListPointersConstants.WreckedShipTreadmillRightwardsLoop"/>
        internal static ushort WreckedShipTreadmillRightwardsLoop => AnimatedTileInstructionListPointersConstants.WreckedShipTreadmillRightwardsLoop;
    }
}

/// <summary>Cartridge values of <see cref="AreaAnimatedTileObjectDefinitions"/> that only verification reads.</summary>
internal static class AreaAnimatedTileObjectDefinitionsConstants
{
    /// <summary>
    /// The cartridge stores an eighth non-retail list after the seven typed areas.
    /// Production <see cref="AreaAnimatedTileObjectDefinitions.Read(AreaId, int)"/> deliberately accepts only typed retail
    /// areas, while verification retains this row so every native table word is audited.
    /// </summary>
    public const int NativeAreaCount = 8;
    /// <summary>
    /// Native <c>kArea_AnimtilesObjectListPtrs</c> at <c>$83:AC56</c>; eight words point
    /// to the eight native area lists below.
    /// </summary>
    public const int NativeListPointerTable = 0x83ac56;

    extension(AreaAnimatedTileObjectDefinitions)
    {
        /// <inheritdoc cref="AreaAnimatedTileObjectDefinitionsConstants.NativeAreaCount"/>
        internal static int NativeAreaCount => AreaAnimatedTileObjectDefinitionsConstants.NativeAreaCount;
        /// <inheritdoc cref="AreaAnimatedTileObjectDefinitionsConstants.NativeListPointerTable"/>
        internal static int NativeListPointerTable => AreaAnimatedTileObjectDefinitionsConstants.NativeListPointerTable;
    }
}

/// <summary>Cartridge values of <see cref="AttractDemoRomData"/> that only verification reads.</summary>
internal static class AttractDemoRomDataConstants
{
    /// <summary>$70:1FE0, completion marker checked by VerifySRAM to unlock set four.</summary>
    public const int CompletionMarkerAddress = (SuperMetroid.Core.Game.SaveRamLayout.SramBank << 16) | SuperMetroid.Core.Game.SaveRamLayout.CompletionMarkerOffset;
    /// <summary>CheckForNextDemo's room-list termination word.</summary>
    public const ushort EndOfSet = 0xffff;
    /// <summary>LoadDemoData's bank and sixteen-byte equipment record stride.</summary>
    public const int EquipmentBank = 0x910000;
    /// <summary>LoadDemoData's bank and sixteen-byte equipment record stride.</summary>
    public const int EquipmentRecordBytes = 16;
    /// <summary>$91:8885, DemoData_Pointers: four equipment/input-object list pointers.</summary>
    public const int EquipmentSetPointers = 0x918885;
    /// <summary>LoadDemoRoomData's bank and eighteen-byte record stride.</summary>
    public const int RoomBank = 0x820000;
    /// <summary>LoadDemoRoomData's bank and eighteen-byte record stride.</summary>
    public const int RoomRecordBytes = 18;
    /// <summary>$82:876C, DemoRoomData_pointers: four room-list pointers terminated by $FFFF.</summary>
    public const int RoomSetPointers = 0x82876c;
    /// <summary>$91:89FD, DemoSamusSetup_Pointers: four Samus-initializer list pointers.</summary>
    public const int SamusSetupSetPointers = 0x9189fd;

    extension(AttractDemoRomData)
    {
        /// <inheritdoc cref="AttractDemoRomDataConstants.CompletionMarkerAddress"/>
        internal static int CompletionMarkerAddress => AttractDemoRomDataConstants.CompletionMarkerAddress;
        /// <inheritdoc cref="AttractDemoRomDataConstants.EndOfSet"/>
        internal static ushort EndOfSet => AttractDemoRomDataConstants.EndOfSet;
        /// <inheritdoc cref="AttractDemoRomDataConstants.EquipmentBank"/>
        internal static int EquipmentBank => AttractDemoRomDataConstants.EquipmentBank;
        /// <inheritdoc cref="AttractDemoRomDataConstants.EquipmentRecordBytes"/>
        internal static int EquipmentRecordBytes => AttractDemoRomDataConstants.EquipmentRecordBytes;
        /// <inheritdoc cref="AttractDemoRomDataConstants.EquipmentSetPointers"/>
        internal static int EquipmentSetPointers => AttractDemoRomDataConstants.EquipmentSetPointers;
        /// <inheritdoc cref="AttractDemoRomDataConstants.RoomBank"/>
        internal static int RoomBank => AttractDemoRomDataConstants.RoomBank;
        /// <inheritdoc cref="AttractDemoRomDataConstants.RoomRecordBytes"/>
        internal static int RoomRecordBytes => AttractDemoRomDataConstants.RoomRecordBytes;
        /// <inheritdoc cref="AttractDemoRomDataConstants.RoomSetPointers"/>
        internal static int RoomSetPointers => AttractDemoRomDataConstants.RoomSetPointers;
        /// <inheritdoc cref="AttractDemoRomDataConstants.SamusSetupSetPointers"/>
        internal static int SamusSetupSetPointers => AttractDemoRomDataConstants.SamusSetupSetPointers;
    }
}

/// <summary>Cartridge values of <see cref="AudioRomData.Apu"/> that only verification reads.</summary>
internal static class AudioRomDataApuConstants
{
    public const byte LibraryThreePort = 3;

    extension(AudioRomData.Apu)
    {
        /// <inheritdoc cref="AudioRomDataApuConstants.LibraryThreePort"/>
        internal static byte LibraryThreePort => AudioRomDataApuConstants.LibraryThreePort;
    }
}

/// <summary>Cartridge values of <see cref="BabyMetroidCutsceneColorRomData"/> that only verification reads.</summary>
internal static class BabyMetroidCutsceneColorRomDataConstants
{
    /// <summary>$AD:E8E2, seven fade-frame pointers; index zero is never displayed.</summary>
    public const int FadePointerTable = 0xade8e2;

    extension(BabyMetroidCutsceneColorRomData)
    {
        /// <inheritdoc cref="BabyMetroidCutsceneColorRomDataConstants.FadePointerTable"/>
        internal static int FadePointerTable => BabyMetroidCutsceneColorRomDataConstants.FadePointerTable;
    }
}

/// <summary>Cartridge values of <see cref="BabyMetroidRouteDefinitions"/> that only verification reads.</summary>
internal static class BabyMetroidRouteDefinitionsConstants
{
    /// <summary>First SNES address occupied by the route records.</summary>
    public const int SourceAddress = 0xa9ca24;
    /// <summary>
    /// Complete source length, including the terminal <c>$CA66</c> word at
    /// <c>$A9:CA64-$CA65</c> observed through the final record's overlapping +8 read.
    /// </summary>
    public const int SourceByteLength = 66;

    extension(BabyMetroidRouteDefinitions)
    {
        /// <inheritdoc cref="BabyMetroidRouteDefinitionsConstants.SourceAddress"/>
        internal static int SourceAddress => BabyMetroidRouteDefinitionsConstants.SourceAddress;
        /// <inheritdoc cref="BabyMetroidRouteDefinitionsConstants.SourceByteLength"/>
        internal static int SourceByteLength => BabyMetroidRouteDefinitionsConstants.SourceByteLength;
    }
}

/// <summary>Cartridge values of <see cref="BeaconPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class BeaconPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 100;
    /// <summary>The room palette-FX definition at <c>$8D:F781</c>.</summary>
    public const ushort DefinitionPointer = 0xf781;

    extension(BeaconPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="BeaconPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => BeaconPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="BeaconPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => BeaconPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="BeetomInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class BeetomInstructionProgramDefinitionsConstants
{
    /// <summary>The repeating right-crawl frame list at $A8:B6F4.</summary>
    public const ushort CrawlingRightLoop = 0xb6f4;
    /// <summary>The repeating right-drain frame list at $A8:B73A.</summary>
    public const ushort DrainingRightLoop = 0xb73a;
    /// <summary>The terminal left-hop sleep instruction at $A8:B6BE.</summary>
    public const ushort HopLeftSleep = 0xb6be;
    /// <summary>The terminal right-hop sleep instruction at $A8:B71A.</summary>
    public const ushort HopRightSleep = 0xb71a;

    extension(BeetomInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="BeetomInstructionProgramDefinitionsConstants.CrawlingRightLoop"/>
        internal static ushort CrawlingRightLoop => BeetomInstructionProgramDefinitionsConstants.CrawlingRightLoop;
        /// <inheritdoc cref="BeetomInstructionProgramDefinitionsConstants.DrainingRightLoop"/>
        internal static ushort DrainingRightLoop => BeetomInstructionProgramDefinitionsConstants.DrainingRightLoop;
        /// <inheritdoc cref="BeetomInstructionProgramDefinitionsConstants.HopLeftSleep"/>
        internal static ushort HopLeftSleep => BeetomInstructionProgramDefinitionsConstants.HopLeftSleep;
        /// <inheritdoc cref="BeetomInstructionProgramDefinitionsConstants.HopRightSleep"/>
        internal static ushort HopRightSleep => BeetomInstructionProgramDefinitionsConstants.HopRightSleep;
    }
}

/// <summary>Cartridge values of <see cref="BlueDoorPlmProgramDefinitions"/> that only verification reads.</summary>
internal static class BlueDoorPlmProgramDefinitionsConstants
{
    /// <summary>Facing-down closing list at $84:C531.</summary>
    public const ushort ClosingDown = 0xc531;
    /// <summary>Facing-left closing list at $84:C49E.</summary>
    public const ushort ClosingLeft = 0xc49e;
    /// <summary>Facing-right closing list at $84:C4CF.</summary>
    public const ushort ClosingRight = 0xc4cf;
    /// <summary>Facing-up closing list at $84:C500.</summary>
    public const ushort ClosingUp = 0xc500;

    extension(BlueDoorPlmProgramDefinitions)
    {
        /// <inheritdoc cref="BlueDoorPlmProgramDefinitionsConstants.ClosingDown"/>
        internal static ushort ClosingDown => BlueDoorPlmProgramDefinitionsConstants.ClosingDown;
        /// <inheritdoc cref="BlueDoorPlmProgramDefinitionsConstants.ClosingLeft"/>
        internal static ushort ClosingLeft => BlueDoorPlmProgramDefinitionsConstants.ClosingLeft;
        /// <inheritdoc cref="BlueDoorPlmProgramDefinitionsConstants.ClosingRight"/>
        internal static ushort ClosingRight => BlueDoorPlmProgramDefinitionsConstants.ClosingRight;
        /// <inheritdoc cref="BlueDoorPlmProgramDefinitionsConstants.ClosingUp"/>
        internal static ushort ClosingUp => BlueDoorPlmProgramDefinitionsConstants.ClosingUp;
    }
}

/// <summary>Cartridge values of <see cref="BotwoonHealthPaletteDefinitions"/> that only verification reads.</summary>
internal static class BotwoonHealthPaletteDefinitionsConstants
{
    /// <summary><c>BotwoonHealthThresholdsForPaletteChange</c> at $B3:981B.</summary>
    public const int NativeThresholdAddress = 0xb3981b;

    extension(BotwoonHealthPaletteDefinitions)
    {
        /// <inheritdoc cref="BotwoonHealthPaletteDefinitionsConstants.NativeThresholdAddress"/>
        internal static int NativeThresholdAddress => BotwoonHealthPaletteDefinitionsConstants.NativeThresholdAddress;
    }
}

/// <summary>Cartridge values of <see cref="BotwoonInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class BotwoonInstructionProgramDefinitionsConstants
{
    /// <summary>
    /// <c>UNUSED_InstList_Botwoon_Hidden_AimingUp_FacingLeft_B3942F</c> at $B3:942F.
    /// </summary>
    public const ushort FirstAdjacentProgram = 0x942f;
    /// <summary><c>InstList_Botwoon_Spit_AimingDown_FacingRight</c> at $B3:93DF.</summary>
    public const ushort SpittingDown = 0x93df;
    /// <summary><c>InstList_Botwoon_Spit_AimingDownLeft</c> at $B3:93BF.</summary>
    public const ushort SpittingDownLeft = 0x93bf;
    /// <summary><c>InstList_Botwoon_Spit_AimingDownRight</c> at $B3:93EF.</summary>
    public const ushort SpittingDownRight = 0x93ef;
    /// <summary><c>InstList_Botwoon_Spit_AimingLeft</c> at $B3:93AF.</summary>
    public const ushort SpittingLeft = 0x93af;
    /// <summary><c>InstList_Botwoon_Spit_AimingRight</c> at $B3:93FF.</summary>
    public const ushort SpittingRight = 0x93ff;
    /// <summary><c>InstList_Botwoon_Spit_AimingUp_FacingRight</c> at $B3:941F.</summary>
    public const ushort SpittingUp = 0x941f;
    /// <summary><c>InstList_Botwoon_Spit_AimingUpRight</c> at $B3:940F.</summary>
    public const ushort SpittingUpRight = 0x940f;
    /// <summary>
    /// <c>UNSUED_InstList_Botwoon_MouthClosed_AimDown_FaceLeft_B39359</c> at $B3:9359.
    /// </summary>
    public const ushort UnusedMovingHorizontal = 0x9359;
    /// <summary>
    /// <c>UNUSED_InstList_Botwoon_Spit_AimingDown_FacingLeft_B393CF</c> at $B3:93CF.
    /// </summary>
    public const ushort UnusedSpittingHorizontal = 0x93cf;

    extension(BotwoonInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="BotwoonInstructionProgramDefinitionsConstants.FirstAdjacentProgram"/>
        internal static ushort FirstAdjacentProgram => BotwoonInstructionProgramDefinitionsConstants.FirstAdjacentProgram;
        /// <inheritdoc cref="BotwoonInstructionProgramDefinitionsConstants.SpittingDown"/>
        internal static ushort SpittingDown => BotwoonInstructionProgramDefinitionsConstants.SpittingDown;
        /// <inheritdoc cref="BotwoonInstructionProgramDefinitionsConstants.SpittingDownLeft"/>
        internal static ushort SpittingDownLeft => BotwoonInstructionProgramDefinitionsConstants.SpittingDownLeft;
        /// <inheritdoc cref="BotwoonInstructionProgramDefinitionsConstants.SpittingDownRight"/>
        internal static ushort SpittingDownRight => BotwoonInstructionProgramDefinitionsConstants.SpittingDownRight;
        /// <inheritdoc cref="BotwoonInstructionProgramDefinitionsConstants.SpittingLeft"/>
        internal static ushort SpittingLeft => BotwoonInstructionProgramDefinitionsConstants.SpittingLeft;
        /// <inheritdoc cref="BotwoonInstructionProgramDefinitionsConstants.SpittingRight"/>
        internal static ushort SpittingRight => BotwoonInstructionProgramDefinitionsConstants.SpittingRight;
        /// <inheritdoc cref="BotwoonInstructionProgramDefinitionsConstants.SpittingUp"/>
        internal static ushort SpittingUp => BotwoonInstructionProgramDefinitionsConstants.SpittingUp;
        /// <inheritdoc cref="BotwoonInstructionProgramDefinitionsConstants.SpittingUpRight"/>
        internal static ushort SpittingUpRight => BotwoonInstructionProgramDefinitionsConstants.SpittingUpRight;
        /// <inheritdoc cref="BotwoonInstructionProgramDefinitionsConstants.UnusedMovingHorizontal"/>
        internal static ushort UnusedMovingHorizontal => BotwoonInstructionProgramDefinitionsConstants.UnusedMovingHorizontal;
        /// <inheritdoc cref="BotwoonInstructionProgramDefinitionsConstants.UnusedSpittingHorizontal"/>
        internal static ushort UnusedSpittingHorizontal => BotwoonInstructionProgramDefinitionsConstants.UnusedSpittingHorizontal;
    }
}

/// <summary>Cartridge values of <see cref="BotwoonSpeedDefinitions"/> that only verification reads.</summary>
internal static class BotwoonSpeedDefinitionsConstants
{
    /// <summary>$B3:94BB, BotwoonSpeedTable: three speed/body-travel-time pairs.</summary>
    public const int MovementReferenceAddress = 0xb394bb;

    extension(BotwoonSpeedDefinitions)
    {
        /// <inheritdoc cref="BotwoonSpeedDefinitionsConstants.MovementReferenceAddress"/>
        internal static int MovementReferenceAddress => BotwoonSpeedDefinitionsConstants.MovementReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="BotwoonWallPlmDrawDefinitions"/> that only verification reads.</summary>
internal static class BotwoonWallPlmDrawDefinitionsConstants
{
    /// <summary><c>$84:9325</c>: first byte of the following unused draw list.</summary>
    public const ushort EndExclusive = 0x9325;

    extension(BotwoonWallPlmDrawDefinitions)
    {
        /// <inheritdoc cref="BotwoonWallPlmDrawDefinitionsConstants.EndExclusive"/>
        internal static ushort EndExclusive => BotwoonWallPlmDrawDefinitionsConstants.EndExclusive;
    }
}

/// <summary>Cartridge values of <see cref="BotwoonWallPlmProgramDefinitions"/> that only verification reads.</summary>
internal static class BotwoonWallPlmProgramDefinitionsConstants
{
    /// <summary><c>$84:AB6D</c>: first byte of the following Kraid program.</summary>
    public const ushort ClearEndExclusive = 0xab6d;
    /// <summary><c>$84:AB51</c>: first byte of the following scroll callback.</summary>
    public const ushort CrumbleEndExclusive = RoomPlmInstructionCodes.SetBotwoonScrollsBlue;

    extension(BotwoonWallPlmProgramDefinitions)
    {
        /// <inheritdoc cref="BotwoonWallPlmProgramDefinitionsConstants.ClearEndExclusive"/>
        internal static ushort ClearEndExclusive => BotwoonWallPlmProgramDefinitionsConstants.ClearEndExclusive;
        /// <inheritdoc cref="BotwoonWallPlmProgramDefinitionsConstants.CrumbleEndExclusive"/>
        internal static ushort CrumbleEndExclusive => BotwoonWallPlmProgramDefinitionsConstants.CrumbleEndExclusive;
    }
}

/// <summary>Cartridge values of <see cref="BrinstarBlueSporePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class BrinstarBlueSporePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 140;

    extension(BrinstarBlueSporePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="BrinstarBlueSporePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => BrinstarBlueSporePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}

/// <summary>Cartridge values of <see cref="CartridgeDoorHeader"/> that only verification reads.</summary>
internal static class CartridgeDoorHeaderConstants
{
    /// <summary>Encoded size of one retail bank-$83 door header.</summary>
    public const int SizeInBytes = DoorHeaderRomDataConstants.RecordByteCount;

    extension(CartridgeDoorHeader)
    {
        /// <inheritdoc cref="CartridgeDoorHeaderConstants.SizeInBytes"/>
        internal static int SizeInBytes => CartridgeDoorHeaderConstants.SizeInBytes;
    }
}

/// <summary>Cartridge values of <see cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary><c>PalFxDef_CutsceneCeresNavigationLightsBg</c> at <c>$8D:E1B8</c>.</summary>
    public const ushort BackgroundNavigationLightsDefinitionPointer = 0xe1b8;
    /// <summary>The complete gunship-engine loop lasts two frames.</summary>
    public const int GunshipEngineCycleFrames = 2;
    /// <summary><c>PalFxDef_CutsceneGunshipEngine</c> at <c>$8D:E1A8</c>.</summary>
    public const ushort GunshipEngineDefinitionPointer = 0xe1a8;
    /// <summary>The complete Ceres navigation-light loop lasts 56 frames.</summary>
    public const int NavigationLightsCycleFrames = 56;
    /// <summary><c>PalFxDef_CutsceneCeresNavigationLightsSprite</c> at <c>$8D:E1AC</c>.</summary>
    public const ushort SpriteNavigationLightsDefinitionPointer = 0xe1ac;

    extension(CeresCinematicLightPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.BackgroundNavigationLightsDefinitionPointer"/>
        internal static ushort BackgroundNavigationLightsDefinitionPointer => CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.BackgroundNavigationLightsDefinitionPointer;
        /// <inheritdoc cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.GunshipEngineCycleFrames"/>
        internal static int GunshipEngineCycleFrames => CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.GunshipEngineCycleFrames;
        /// <inheritdoc cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.GunshipEngineDefinitionPointer"/>
        internal static ushort GunshipEngineDefinitionPointer => CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.GunshipEngineDefinitionPointer;
        /// <inheritdoc cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.NavigationLightsCycleFrames"/>
        internal static int NavigationLightsCycleFrames => CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.NavigationLightsCycleFrames;
        /// <inheritdoc cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.SpriteNavigationLightsDefinitionPointer"/>
        internal static ushort SpriteNavigationLightsDefinitionPointer => CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.SpriteNavigationLightsDefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="CeresDestructionRomData.Assets"/> that only verification reads.</summary>
internal static class CeresDestructionRomDataAssetsConstants
{
    public const int SharedObjectCharacters = 0x9ad200;

    extension(CeresDestructionRomData.Assets)
    {
        /// <inheritdoc cref="CeresDestructionRomDataAssetsConstants.SharedObjectCharacters"/>
        internal static int SharedObjectCharacters => CeresDestructionRomDataAssetsConstants.SharedObjectCharacters;
    }
}

/// <summary>Cartridge values of <see cref="CeresDoorInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class CeresDoorInstructionProgramDefinitionsConstants
{
    /// <summary>
    /// <c>InstList_CeresDoor_Normal_FacingLeft_3</c> at $A6:F5EA
    /// makes the door tangible and visible before its closed wait.
    /// </summary>
    public const ushort ClosedFacingLeft = 0xf5ea;

    extension(CeresDoorInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="CeresDoorInstructionProgramDefinitionsConstants.ClosedFacingLeft"/>
        internal static ushort ClosedFacingLeft => CeresDoorInstructionProgramDefinitionsConstants.ClosedFacingLeft;
    }
}

/// <summary>Cartridge values of <see cref="CeresElevatorArrivalDefinitions"/> that only verification reads.</summary>
internal static class CeresElevatorArrivalDefinitionsConstants
{
    /// <summary>Bank containing the two definitions and their instruction programs.</summary>
    public const int BankBase = 0x860000;
    /// <summary>$86:8154, Instruction_Delete opcode stored at $86:A28B.</summary>
    public const ushort DeleteOpcode = 0x8154;
    /// <summary>$86:81AB, Instruction_Goto opcode used by both looping programs.</summary>
    public const ushort GotoOpcode = 0x81ab;

    extension(CeresElevatorArrivalDefinitions)
    {
        /// <inheritdoc cref="CeresElevatorArrivalDefinitionsConstants.BankBase"/>
        internal static int BankBase => CeresElevatorArrivalDefinitionsConstants.BankBase;
        /// <inheritdoc cref="CeresElevatorArrivalDefinitionsConstants.DeleteOpcode"/>
        internal static ushort DeleteOpcode => CeresElevatorArrivalDefinitionsConstants.DeleteOpcode;
        /// <inheritdoc cref="CeresElevatorArrivalDefinitionsConstants.GotoOpcode"/>
        internal static ushort GotoOpcode => CeresElevatorArrivalDefinitionsConstants.GotoOpcode;
    }
}

/// <summary>Cartridge values of <see cref="CeresExplosionDefinitions"/> that only verification reads.</summary>
internal static class CeresExplosionDefinitionsConstants
{
    /// <summary>Bank containing the native definitions and placement tables.</summary>
    public const int NativeBank = 0x8b0000;

    extension(CeresExplosionDefinitions)
    {
        /// <inheritdoc cref="CeresExplosionDefinitionsConstants.NativeBank"/>
        internal static int NativeBank => CeresExplosionDefinitionsConstants.NativeBank;
    }
}

/// <summary>Cartridge values of <see cref="CeresRidleyEyeFadeDefinitions"/> that only verification reads.</summary>
internal static class CeresRidleyEyeFadeDefinitionsConstants
{
    /// <summary>
    /// <c>RidleyFadeIn_EyeGlowIntensity</c> at $A6:E269: 64 palette-row selectors
    /// followed by the $FF completion marker at $A6:E2A9.
    /// </summary>
    public const int NativeScheduleAddress = 0xa6e269;

    extension(CeresRidleyEyeFadeDefinitions)
    {
        /// <inheritdoc cref="CeresRidleyEyeFadeDefinitionsConstants.NativeScheduleAddress"/>
        internal static int NativeScheduleAddress => CeresRidleyEyeFadeDefinitionsConstants.NativeScheduleAddress;
    }
}

/// <summary>Cartridge values of <see cref="CeresShaftRotationDefinitions"/> that only verification reads.</summary>
internal static class CeresShaftRotationDefinitionsConstants
{
    /// <summary>$89:AD5F, RoomCode_CeresElevatorShaft timer/sine/cosine records.</summary>
    /// <remarks>
    /// Issues #625 and #949: all 138 trigonometric words have an exact, correction-free generation model.
    /// For record i=0..68 let n=i-34 and angle=n/256 radians. Independently round
    /// 256*sin(angle) and 256*cos(angle) to nearest integer. Across this bounded domain,
    /// sine simplifies to n and cosine to 256-floor((n*n+255)/512), using integer division.
    /// The latter is also the quadratic small-angle approximation with half-way decrements
    /// rounded downward. It is NOT round(sqrt(65536-n*n)): that alternative returns 255
    /// at n=+/-16, while the native independently quantized coordinates have cosine 256.
    /// The #1165 research checked every coefficient against NTSC J/U v1.0 ROM, pinned
    /// RoomMainASM_CeresElevatorShaft assembly, and Read, using decimal Taylor bounds to
    /// certify each trig rounding. All 65,536 phase values were checked, including the 138
    /// valid aliases created by wrapped 16-bit multiplication before indexing.
    /// This proves a compatible generator, not the original authoring tool. The existing
    /// coefficient logic matches it; the separate timer ramp is described below.
    /// </remarks>
    public const int ReferenceAddress = 0x89ad5f;

    extension(CeresShaftRotationDefinitions)
    {
        /// <inheritdoc cref="CeresShaftRotationDefinitionsConstants.ReferenceAddress"/>
        internal static int ReferenceAddress => CeresShaftRotationDefinitionsConstants.ReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="CeresSteamInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class CeresSteamInstructionProgramDefinitionsConstants
{
    /// <summary>
    /// <c>InstList_CeresSteam_Down_2</c> at $A6:F0C9; seven three-tick
    /// frames loop to the hidden hold at $F0C1.
    /// </summary>
    public const ushort DownActive = 0xf0c9;
    /// <summary>
    /// <c>InstList_CeresSteam_Left_2</c> at $A6:F095; seven three-tick frames
    /// loop to the local hidden hold at $F08D.
    /// </summary>
    public const ushort LeftActive = 0xf095;
    /// <summary>
    /// <c>InstList_CeresSteam_Right_2</c> at $A6:F0FD; seven three-tick
    /// frames loop to the hidden hold at $F0F5.
    /// </summary>
    public const ushort RightActive = 0xf0fd;
    /// <summary>
    /// <c>InstList_CeresSteam_Up_2</c> at $A6:F061 runs seven three-tick
    /// frames and returns to the hidden hold at $F059. The interpreter thus
    /// traverses only the three local states $F04D, $F059, and $F061.
    /// </summary>
    public const ushort UpActive = 0xf061;

    extension(CeresSteamInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="CeresSteamInstructionProgramDefinitionsConstants.DownActive"/>
        internal static ushort DownActive => CeresSteamInstructionProgramDefinitionsConstants.DownActive;
        /// <inheritdoc cref="CeresSteamInstructionProgramDefinitionsConstants.LeftActive"/>
        internal static ushort LeftActive => CeresSteamInstructionProgramDefinitionsConstants.LeftActive;
        /// <inheritdoc cref="CeresSteamInstructionProgramDefinitionsConstants.RightActive"/>
        internal static ushort RightActive => CeresSteamInstructionProgramDefinitionsConstants.RightActive;
        /// <inheritdoc cref="CeresSteamInstructionProgramDefinitionsConstants.UpActive"/>
        internal static ushort UpActive => CeresSteamInstructionProgramDefinitionsConstants.UpActive;
    }
}

/// <summary>Cartridge values of <see cref="ChargeFlareSpriteDefinitions"/> that only verification reads.</summary>
internal static class ChargeFlareSpriteDefinitionsConstants
{
    /// <summary>$93:A1A1, FlareSpritemapPointers: the first 54 selectors are charge-flare components.</summary>
    public const int SelectorTable = 0x93A1A1;

    extension(ChargeFlareSpriteDefinitions)
    {
        /// <inheritdoc cref="ChargeFlareSpriteDefinitionsConstants.SelectorTable"/>
        internal static int SelectorTable => ChargeFlareSpriteDefinitionsConstants.SelectorTable;
    }
}

/// <summary>Cartridge values of <see cref="CinematicCodePointers"/> that only verification reads.</summary>
internal static class CinematicCodePointersConstants
{
    public const ushort CreditsObject_Instruction_DecrementTimerAndGoto = 0x9a0d;
    public const ushort CreditsObject_Instruction_Delete = 0x99fe;
    public const ushort CreditsObject_Instruction_EndCredits = 0xf6fe;
    public const ushort CreditsObject_Instruction_SetTimer = 0x9a17;
    public const ushort Ending_Instruction_ClearItemPercentageSubtitle = 0xe780;
    public const ushort Ending_Instruction_DrawItemPercentage = 0xe627;
    public const ushort Ending_Instruction_DrawItemPercentageSubtitle = 0xe769;
    /// <summary><c>IndirectInstructionFunction_DoNothing</c> at $8B:8849.</summary>
    public const ushort IndirectInstruction_DoNothing = 0x8849;
    /// <summary><c>IndirectInstructionFunction_DrawTextCharacter</c> at $8B:884D.</summary>
    public const ushort IndirectInstruction_DrawTextCharacter = 0x884d;
    /// <summary><c>IndirectInstructionFunction_DrawToCinematicBGTilemap</c> at $8B:88B7.</summary>
    public const ushort IndirectInstruction_DrawToBackgroundTilemap = 0x88b7;

    extension(CinematicCodePointers)
    {
        /// <inheritdoc cref="CinematicCodePointersConstants.CreditsObject_Instruction_DecrementTimerAndGoto"/>
        internal static ushort CreditsObject_Instruction_DecrementTimerAndGoto => CinematicCodePointersConstants.CreditsObject_Instruction_DecrementTimerAndGoto;
        /// <inheritdoc cref="CinematicCodePointersConstants.CreditsObject_Instruction_Delete"/>
        internal static ushort CreditsObject_Instruction_Delete => CinematicCodePointersConstants.CreditsObject_Instruction_Delete;
        /// <inheritdoc cref="CinematicCodePointersConstants.CreditsObject_Instruction_EndCredits"/>
        internal static ushort CreditsObject_Instruction_EndCredits => CinematicCodePointersConstants.CreditsObject_Instruction_EndCredits;
        /// <inheritdoc cref="CinematicCodePointersConstants.CreditsObject_Instruction_SetTimer"/>
        internal static ushort CreditsObject_Instruction_SetTimer => CinematicCodePointersConstants.CreditsObject_Instruction_SetTimer;
        /// <inheritdoc cref="CinematicCodePointersConstants.Ending_Instruction_ClearItemPercentageSubtitle"/>
        internal static ushort Ending_Instruction_ClearItemPercentageSubtitle => CinematicCodePointersConstants.Ending_Instruction_ClearItemPercentageSubtitle;
        /// <inheritdoc cref="CinematicCodePointersConstants.Ending_Instruction_DrawItemPercentage"/>
        internal static ushort Ending_Instruction_DrawItemPercentage => CinematicCodePointersConstants.Ending_Instruction_DrawItemPercentage;
        /// <inheritdoc cref="CinematicCodePointersConstants.Ending_Instruction_DrawItemPercentageSubtitle"/>
        internal static ushort Ending_Instruction_DrawItemPercentageSubtitle => CinematicCodePointersConstants.Ending_Instruction_DrawItemPercentageSubtitle;
        /// <inheritdoc cref="CinematicCodePointersConstants.IndirectInstruction_DoNothing"/>
        internal static ushort IndirectInstruction_DoNothing => CinematicCodePointersConstants.IndirectInstruction_DoNothing;
        /// <inheritdoc cref="CinematicCodePointersConstants.IndirectInstruction_DrawTextCharacter"/>
        internal static ushort IndirectInstruction_DrawTextCharacter => CinematicCodePointersConstants.IndirectInstruction_DrawTextCharacter;
        /// <inheritdoc cref="CinematicCodePointersConstants.IndirectInstruction_DrawToBackgroundTilemap"/>
        internal static ushort IndirectInstruction_DrawToBackgroundTilemap => CinematicCodePointersConstants.IndirectInstruction_DrawToBackgroundTilemap;
    }
}

/// <summary>Cartridge values of <see cref="CinematicCodePointers.Lists"/> that only verification reads.</summary>
internal static class CinematicCodePointersListsConstants
{
    public const ushort CeresExplosionLargeAsteroids = 0xce4b;
    public const ushort CeresPurpleSpaceVortex = 0xcc57;
    public const ushort CeresSmallAsteroids = 0xcc4f;
    public const ushort CeresStars = 0xcda3;
    public const ushort IntroRinkaSpawner = 0xce0d;
    public const ushort MetroidEggParticleStride = 8;
    public const ushort MetroidEggSlimeDrops = 0xcd69;

    extension(CinematicCodePointers.Lists)
    {
        /// <inheritdoc cref="CinematicCodePointersListsConstants.CeresExplosionLargeAsteroids"/>
        internal static ushort CeresExplosionLargeAsteroids => CinematicCodePointersListsConstants.CeresExplosionLargeAsteroids;
        /// <inheritdoc cref="CinematicCodePointersListsConstants.CeresPurpleSpaceVortex"/>
        internal static ushort CeresPurpleSpaceVortex => CinematicCodePointersListsConstants.CeresPurpleSpaceVortex;
        /// <inheritdoc cref="CinematicCodePointersListsConstants.CeresSmallAsteroids"/>
        internal static ushort CeresSmallAsteroids => CinematicCodePointersListsConstants.CeresSmallAsteroids;
        /// <inheritdoc cref="CinematicCodePointersListsConstants.CeresStars"/>
        internal static ushort CeresStars => CinematicCodePointersListsConstants.CeresStars;
        /// <inheritdoc cref="CinematicCodePointersListsConstants.IntroRinkaSpawner"/>
        internal static ushort IntroRinkaSpawner => CinematicCodePointersListsConstants.IntroRinkaSpawner;
        /// <inheritdoc cref="CinematicCodePointersListsConstants.MetroidEggParticleStride"/>
        internal static ushort MetroidEggParticleStride => CinematicCodePointersListsConstants.MetroidEggParticleStride;
        /// <inheritdoc cref="CinematicCodePointersListsConstants.MetroidEggSlimeDrops"/>
        internal static ushort MetroidEggSlimeDrops => CinematicCodePointersListsConstants.MetroidEggSlimeDrops;
    }
}

/// <summary>Cartridge values of <see cref="CommonEnemyEmptyExtendedFrameDefinitions"/> that only verification reads.</summary>
internal static class CommonEnemyEmptyExtendedFrameDefinitionsConstants
{
    /// <summary>Bank-local one-point hitbox list at $8059.</summary>
    public const ushort PointHitboxList = 0x8059;

    extension(CommonEnemyEmptyExtendedFrameDefinitions)
    {
        /// <inheritdoc cref="CommonEnemyEmptyExtendedFrameDefinitionsConstants.PointHitboxList"/>
        internal static ushort PointHitboxList => CommonEnemyEmptyExtendedFrameDefinitionsConstants.PointHitboxList;
    }
}

/// <summary>Cartridge values of <see cref="CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Both complete loops last 98 frames.</summary>
    public const int CycleFrames = 98;

    extension(CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}

/// <summary>Cartridge values of <see cref="CrocomireArenaPlmDrawDefinitions"/> that only verification reads.</summary>
internal static class CrocomireArenaPlmDrawDefinitionsConstants
{
    /// <summary><c>$84:9BF7</c>: first byte of the following eye-door draw region.</summary>
    public const ushort EndExclusive = 0x9bf7;

    extension(CrocomireArenaPlmDrawDefinitions)
    {
        /// <inheritdoc cref="CrocomireArenaPlmDrawDefinitionsConstants.EndExclusive"/>
        internal static ushort EndExclusive => CrocomireArenaPlmDrawDefinitionsConstants.EndExclusive;
    }
}

/// <summary>Cartridge values of <see cref="CrocomireInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class CrocomireInstructionProgramDefinitionsConstants
{
    /// <summary>First deliberately excluded unreferenced body program at $A4:BBAE.</summary>
    public const ushort FirstExcludedUnreferencedProgram = 0xbbae;
    /// <summary>First independently owned Crocomire-tongue program at $A4:BE56.</summary>
    public const ushort FirstTongueProgram = 0xbe56;

    extension(CrocomireInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="CrocomireInstructionProgramDefinitionsConstants.FirstExcludedUnreferencedProgram"/>
        internal static ushort FirstExcludedUnreferencedProgram => CrocomireInstructionProgramDefinitionsConstants.FirstExcludedUnreferencedProgram;
        /// <inheritdoc cref="CrocomireInstructionProgramDefinitionsConstants.FirstTongueProgram"/>
        internal static ushort FirstTongueProgram => CrocomireInstructionProgramDefinitionsConstants.FirstTongueProgram;
    }
}

/// <summary>Cartridge values of <see cref="CrocomireMeltingDefinitions"/> that only verification reads.</summary>
internal static class CrocomireMeltingDefinitionsConstants
{
    /// <summary>
    /// <c>TilePixelColumnBitmasks</c> at <c>$A4:9BBD-$A4:9BC4</c>. Native indexes
    /// these by chronological table cursor rather than selected X column.
    /// </summary>
    /// <remarks>
    /// Exact bounded rule for cursor 0..48:
    /// <c>$FF XOR ($80 &gt;&gt; (cursor &amp; 7))</c>. All eight physical bytes
    /// match the pinned NTSC J/U v1.0 ROM; the rule reproduces all 49 cursor
    /// results and the complete production erase silhouette. The cartridge
    /// selects the mask by chronological cursor, not by the separately selected
    /// X column. This proof covers only the masks, not that column permutation.
    /// Independently reviewed for #1165 against all original NTSC bytes and pinned
    /// bank_A4.asm. Implement the exact bit-clear rule; keep cursor bounds and the
    /// native chronological-index bug. No column permutation is inferred.
    /// </remarks>
    public const int MaskReferenceAddress = 0xa49bbd;

    extension(CrocomireMeltingDefinitions)
    {
        /// <inheritdoc cref="CrocomireMeltingDefinitionsConstants.MaskReferenceAddress"/>
        internal static int MaskReferenceAddress => CrocomireMeltingDefinitionsConstants.MaskReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="CrocomireMeltingTransferDefinitions"/> that only verification reads.</summary>
internal static class CrocomireMeltingTransferDefinitionsConstants
{
    /// <summary>Exclusive end of the two native records.</summary>
    public const int NativeByteCount = 0x00b4;
    /// <summary>Native bank-$A4 base for independent ROM-oracle comparison.</summary>
    public const int NativeSourceAddress = 0xa49bc5;

    extension(CrocomireMeltingTransferDefinitions)
    {
        /// <inheritdoc cref="CrocomireMeltingTransferDefinitionsConstants.NativeByteCount"/>
        internal static int NativeByteCount => CrocomireMeltingTransferDefinitionsConstants.NativeByteCount;
        /// <inheritdoc cref="CrocomireMeltingTransferDefinitionsConstants.NativeSourceAddress"/>
        internal static int NativeSourceAddress => CrocomireMeltingTransferDefinitionsConstants.NativeSourceAddress;
    }
}

/// <summary>Cartridge values of <see cref="CrystalFlashPaletteTimingDefinitions"/> that only verification reads.</summary>
internal static class CrystalFlashPaletteTimingDefinitionsConstants
{
    /// <summary>
    /// Timer words in the ten four-byte pointer/timer records at $91:DC00-$91:DC27.
    /// The first timer is at $91:DC02 and subsequent timers have a four-byte stride.
    /// </summary>
    public const int NativeFirstTimerAddress = 0x91dc02;

    extension(CrystalFlashPaletteTimingDefinitions)
    {
        /// <inheritdoc cref="CrystalFlashPaletteTimingDefinitionsConstants.NativeFirstTimerAddress"/>
        internal static int NativeFirstTimerAddress => CrystalFlashPaletteTimingDefinitionsConstants.NativeFirstTimerAddress;
    }
}

/// <summary>Cartridge values of <see cref="DachoraColorRomData"/> that only verification reads.</summary>
internal static class DachoraColorRomDataConstants
{
    /// <summary>Four native shine-image pointers at $A7:F92D.</summary>
    public const int ShinePointerTable = 0xa7f92d;
    /// <summary>Four native speed-image pointers at $A7:F787.</summary>
    public const int SpeedPointerTable = 0xa7f787;

    extension(DachoraColorRomData)
    {
        /// <inheritdoc cref="DachoraColorRomDataConstants.ShinePointerTable"/>
        internal static int ShinePointerTable => DachoraColorRomDataConstants.ShinePointerTable;
        /// <inheritdoc cref="DachoraColorRomDataConstants.SpeedPointerTable"/>
        internal static int SpeedPointerTable => DachoraColorRomDataConstants.SpeedPointerTable;
    }
}

/// <summary>Cartridge values of <see cref="DachoraInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class DachoraInstructionProgramDefinitionsConstants
{
    /// <summary>The retail-unused left-facing charge program at $A7:F3F1.</summary>
    public const ushort UnusedChargeLeft = 0xf3f1;

    extension(DachoraInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="DachoraInstructionProgramDefinitionsConstants.UnusedChargeLeft"/>
        internal static ushort UnusedChargeLeft => DachoraInstructionProgramDefinitionsConstants.UnusedChargeLeft;
    }
}

/// <summary>Cartridge values of <see cref="DeadSidehopperInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class DeadSidehopperInstructionProgramDefinitionsConstants
{
    /// <summary>The first following program, <c>InstList_CorpseZoomer_Param1_0</c>, at $A9:ECF5.</summary>
    public const ushort FirstAdjacentProgram = 0xecf5;

    extension(DeadSidehopperInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="DeadSidehopperInstructionProgramDefinitionsConstants.FirstAdjacentProgram"/>
        internal static ushort FirstAdjacentProgram => DeadSidehopperInstructionProgramDefinitionsConstants.FirstAdjacentProgram;
    }
}

/// <summary>Cartridge values of <see cref="DeadTorizoInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class DeadTorizoInstructionProgramDefinitionsConstants
{
    /// <summary><c>Spritemaps_CorpseTorizo</c> begins after the program at $A9:D6E2.</summary>
    public const ushort FirstAdjacentPresentationData = 0xd6e2;

    extension(DeadTorizoInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="DeadTorizoInstructionProgramDefinitionsConstants.FirstAdjacentPresentationData"/>
        internal static ushort FirstAdjacentPresentationData => DeadTorizoInstructionProgramDefinitionsConstants.FirstAdjacentPresentationData;
    }
}

/// <summary>Cartridge values of <see cref="DeadTorizoVramTransferDefinitions"/> that only verification reads.</summary>
internal static class DeadTorizoVramTransferDefinitionsConstants
{
    /// <summary><c>DeadTorizo_VRAMWriteTable_Even</c> at $A9:D549.</summary>
    public const ushort EvenTable = 0xd549;
    /// <summary><c>DeadTorizo_VRAMWriteTable_Odd</c> at $A9:D583.</summary>
    public const ushort OddTable = 0xd583;
    /// <summary>Four 16-bit descriptor words per native record.</summary>
    public const int RecordByteCount = 8;

    extension(DeadTorizoVramTransferDefinitions)
    {
        /// <inheritdoc cref="DeadTorizoVramTransferDefinitionsConstants.EvenTable"/>
        internal static ushort EvenTable => DeadTorizoVramTransferDefinitionsConstants.EvenTable;
        /// <inheritdoc cref="DeadTorizoVramTransferDefinitionsConstants.OddTable"/>
        internal static ushort OddTable => DeadTorizoVramTransferDefinitionsConstants.OddTable;
        /// <inheritdoc cref="DeadTorizoVramTransferDefinitionsConstants.RecordByteCount"/>
        internal static int RecordByteCount => DeadTorizoVramTransferDefinitionsConstants.RecordByteCount;
    }
}

/// <summary>Cartridge values of <see cref="DeadTourianCorpseInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class DeadTourianCorpseInstructionProgramDefinitionsConstants
{
    /// <summary>The first dead-monster spritemap after the programs, at $A9:ED25.</summary>
    public const ushort FirstAdjacentPresentationData = 0xed25;

    extension(DeadTourianCorpseInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="DeadTourianCorpseInstructionProgramDefinitionsConstants.FirstAdjacentPresentationData"/>
        internal static ushort FirstAdjacentPresentationData => DeadTourianCorpseInstructionProgramDefinitionsConstants.FirstAdjacentPresentationData;
    }
}

/// <summary>Cartridge values of <see cref="DeadTourianCorpseVisualDefinitions"/> that only verification reads.</summary>
internal static class DeadTourianCorpseVisualDefinitionsConstants
{
    public const int CorpseFrameCount = 8;
    public const int FrameCount = CorpseFrameCount + SidehopperFrameCount;
    public const int SidehopperFrameCount = 5;

    extension(DeadTourianCorpseVisualDefinitions)
    {
        /// <inheritdoc cref="DeadTourianCorpseVisualDefinitionsConstants.CorpseFrameCount"/>
        internal static int CorpseFrameCount => DeadTourianCorpseVisualDefinitionsConstants.CorpseFrameCount;
        /// <inheritdoc cref="DeadTourianCorpseVisualDefinitionsConstants.FrameCount"/>
        internal static int FrameCount => DeadTourianCorpseVisualDefinitionsConstants.FrameCount;
        /// <inheritdoc cref="DeadTourianCorpseVisualDefinitionsConstants.SidehopperFrameCount"/>
        internal static int SidehopperFrameCount => DeadTourianCorpseVisualDefinitionsConstants.SidehopperFrameCount;
    }
}

/// <summary>Cartridge values of <see cref="DemoInputRomData"/> that only verification reads.</summary>
internal static class DemoInputRomDataConstants
{
    public const int BankBase = 0x910000;

    extension(DemoInputRomData)
    {
        /// <inheritdoc cref="DemoInputRomDataConstants.BankBase"/>
        internal static int BankBase => DemoInputRomDataConstants.BankBase;
    }
}

/// <summary>Cartridge values of <see cref="DoorClosingPlmRomData"/> that only verification reads.</summary>
internal static class DoorClosingPlmRomDataConstants
{
    /// <summary>$8F:E68A-$8F:E6A1, the twelve direction-selected fallback headers.</summary>
    public const int HeaderTableAddress = 0x8fe68a;

    extension(DoorClosingPlmRomData)
    {
        /// <inheritdoc cref="DoorClosingPlmRomDataConstants.HeaderTableAddress"/>
        internal static int HeaderTableAddress => DoorClosingPlmRomDataConstants.HeaderTableAddress;
    }
}

/// <summary>Cartridge values of <see cref="DoorDefinitions"/> that only verification reads.</summary>
internal static class DoorDefinitionsConstants
{
    /// <summary>Number of physical door records across the two native bank-$83 blocks.</summary>
    public const int HeaderCount = 597;

    extension(DoorDefinitions)
    {
        /// <inheritdoc cref="DoorDefinitionsConstants.HeaderCount"/>
        internal static int HeaderCount => DoorDefinitionsConstants.HeaderCount;
    }
}

/// <summary>Cartridge values of <see cref="DoorHeaderRomData"/> that only verification reads.</summary>
internal static class DoorHeaderRomDataConstants
{
    /// <summary>Last Ceres door, <c>Door_CeresRidley</c>.</summary>
    public const ushort PostFxBlockEnd = 0xabb8;
    /// <summary>First Wrecked Ship door, <c>Door_BowlingAlley_0</c>.</summary>
    public const ushort PostFxBlockStart = 0xa18c;
    /// <summary>Last Lower Norfair door, <c>Door_LNSave_0</c>.</summary>
    public const ushort PreFxBlockEnd = 0x9ab6;
    /// <summary>First Crateria door, <c>Door_LandingSite_LandingCutscene</c>.</summary>
    public const ushort PreFxBlockStart = 0x88fe;
    /// <summary>Encoded byte length of one physical door record.</summary>
    public const int RecordByteCount = 12;

    extension(DoorHeaderRomData)
    {
        /// <inheritdoc cref="DoorHeaderRomDataConstants.PostFxBlockEnd"/>
        internal static ushort PostFxBlockEnd => DoorHeaderRomDataConstants.PostFxBlockEnd;
        /// <inheritdoc cref="DoorHeaderRomDataConstants.PostFxBlockStart"/>
        internal static ushort PostFxBlockStart => DoorHeaderRomDataConstants.PostFxBlockStart;
        /// <inheritdoc cref="DoorHeaderRomDataConstants.PreFxBlockEnd"/>
        internal static ushort PreFxBlockEnd => DoorHeaderRomDataConstants.PreFxBlockEnd;
        /// <inheritdoc cref="DoorHeaderRomDataConstants.PreFxBlockStart"/>
        internal static ushort PreFxBlockStart => DoorHeaderRomDataConstants.PreFxBlockStart;
        /// <inheritdoc cref="DoorHeaderRomDataConstants.RecordByteCount"/>
        internal static int RecordByteCount => DoorHeaderRomDataConstants.RecordByteCount;
    }
}

/// <summary>Cartridge values of <see cref="DownwardGateEnemyProjectileRomData"/> that only verification reads.</summary>
internal static class DownwardGateEnemyProjectileRomDataConstants
{
    public const ushort ClosedSleepInstruction = DownwardGateProjectileInstructionProgramDefinitions.ClosedSleep;

    extension(DownwardGateEnemyProjectileRomData)
    {
        /// <inheritdoc cref="DownwardGateEnemyProjectileRomDataConstants.ClosedSleepInstruction"/>
        internal static ushort ClosedSleepInstruction => DownwardGateEnemyProjectileRomDataConstants.ClosedSleepInstruction;
    }
}

/// <summary>Cartridge values of <see cref="DraygonHealthPaletteDefinitions"/> that only verification reads.</summary>
internal static class DraygonHealthPaletteDefinitionsConstants
{
    /// <summary>
    /// <c>DraygonHealthBasedPaletteThresholds</c> at $A5:96EF. Eight reachable words are
    /// followed by an unreachable $FFFF terminator at $A5:96FF.
    /// </summary>
    public const int NativeThresholdAddress = 0xa596ef;

    extension(DraygonHealthPaletteDefinitions)
    {
        /// <inheritdoc cref="DraygonHealthPaletteDefinitionsConstants.NativeThresholdAddress"/>
        internal static int NativeThresholdAddress => DraygonHealthPaletteDefinitionsConstants.NativeThresholdAddress;
    }
}

/// <summary>Cartridge values of <see cref="DraygonIntroDanceDefinitions"/> that only verification reads.</summary>
internal static class DraygonIntroDanceDefinitionsConstants
{
    /// <summary>
    /// The four reachable words of <c>MovementLatencyForEachEvirSpriteObject</c> at
    /// <c>$A5:A19F-$A5:A1A6</c>, ordered by sprite slots 28 through 31.
    /// </summary>
    /// <remarks>The native loop at $A5:A13E indexes with 2*(slot-28).</remarks>
    public const int NativeMovementLatencyAddress = 0xa5a19f;
    /// <summary>Native start of <c>DraygonFightIntroDanceData</c> at <c>$A5:CE07</c>.</summary>
    public const int NativeMovementStreamAddress = 0xa5ce07;

    extension(DraygonIntroDanceDefinitions)
    {
        /// <inheritdoc cref="DraygonIntroDanceDefinitionsConstants.NativeMovementLatencyAddress"/>
        internal static int NativeMovementLatencyAddress => DraygonIntroDanceDefinitionsConstants.NativeMovementLatencyAddress;
        /// <inheritdoc cref="DraygonIntroDanceDefinitionsConstants.NativeMovementStreamAddress"/>
        internal static int NativeMovementStreamAddress => DraygonIntroDanceDefinitionsConstants.NativeMovementStreamAddress;
    }
}

/// <summary>Cartridge values of <see cref="ElevatorInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class ElevatorInstructionProgramDefinitionsConstants
{
    /// <summary>The controller-input table immediately after the program, at $A3:94E2.</summary>
    public const ushort FirstAdjacentMechanicsData = 0x94e2;

    extension(ElevatorInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="ElevatorInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData"/>
        internal static ushort FirstAdjacentMechanicsData => ElevatorInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData;
    }
}

/// <summary>Cartridge values of <see cref="EndingCreditsRomData.Assets"/> that only verification reads.</summary>
internal static class EndingCreditsRomDataAssetsConstants
{
    /// <summary>$95:A82F, decompressed to $7F:0000 for the flyaway's high-byte Mode-7 character lane.</summary>
    public const int FlyawayCharacters = 0x95a82f;

    extension(EndingCreditsRomData.Assets)
    {
        /// <inheritdoc cref="EndingCreditsRomDataAssetsConstants.FlyawayCharacters"/>
        internal static int FlyawayCharacters => EndingCreditsRomDataAssetsConstants.FlyawayCharacters;
    }
}

/// <summary>Cartridge values of <see cref="EndingLogoDefinitions"/> that only verification reads.</summary>
internal static class EndingLogoDefinitionsConstants
{
    /// <summary>$8B:E554..E569 spawns the upper/lower S and upper/lower circle in this order.</summary>
    public const int ActorCount = 4;
    /// <summary>Bank containing the native six-byte cinematic-object definitions.</summary>
    public const int NativeDefinitionBank = 0x8b0000;

    extension(EndingLogoDefinitions)
    {
        /// <inheritdoc cref="EndingLogoDefinitionsConstants.ActorCount"/>
        internal static int ActorCount => EndingLogoDefinitionsConstants.ActorCount;
        /// <inheritdoc cref="EndingLogoDefinitionsConstants.NativeDefinitionBank"/>
        internal static int NativeDefinitionBank => EndingLogoDefinitionsConstants.NativeDefinitionBank;
    }
}

/// <summary>Cartridge values of <see cref="EndingLogoPalettePointerDefinitions"/> that only verification reads.</summary>
internal static class EndingLogoPalettePointerDefinitionsConstants
{
    /// <summary>$8B:E5E7, sixteen pairs of reverse-copy bank-$8C palette pointers.</summary>
    public const int NativeTableAddress = 0x8be5e7;

    extension(EndingLogoPalettePointerDefinitions)
    {
        /// <inheritdoc cref="EndingLogoPalettePointerDefinitionsConstants.NativeTableAddress"/>
        internal static int NativeTableAddress => EndingLogoPalettePointerDefinitionsConstants.NativeTableAddress;
    }
}

/// <summary>Cartridge values of <see cref="EndingPostShotUploadDefinitions"/> that only verification reads.</summary>
internal static class EndingPostShotUploadDefinitionsConstants
{
    /// <summary>Native record size: length, long source with padding, destination word.</summary>
    public const int RecordBytes = 8;
    /// <summary>First eight-byte transfer record at <c>$8B:E45A</c>.</summary>
    public const int TableAddress = 0x8be45a;

    extension(EndingPostShotUploadDefinitions)
    {
        /// <inheritdoc cref="EndingPostShotUploadDefinitionsConstants.RecordBytes"/>
        internal static int RecordBytes => EndingPostShotUploadDefinitionsConstants.RecordBytes;
        /// <inheritdoc cref="EndingPostShotUploadDefinitionsConstants.TableAddress"/>
        internal static int TableAddress => EndingPostShotUploadDefinitionsConstants.TableAddress;
    }
}

/// <summary>Cartridge values of <see cref="EndingRewardActorDefinitions"/> that only verification reads.</summary>
internal static class EndingRewardActorDefinitionsConstants
{
    /// <summary>Bank containing the native six-byte cinematic-object definitions.</summary>
    public const int NativeDefinitionBank = 0x8b0000;

    extension(EndingRewardActorDefinitions)
    {
        /// <inheritdoc cref="EndingRewardActorDefinitionsConstants.NativeDefinitionBank"/>
        internal static int NativeDefinitionBank => EndingRewardActorDefinitionsConstants.NativeDefinitionBank;
    }
}

/// <summary>Cartridge values of <see cref="EndingRewardGraphicsUploadDefinitions"/> that only verification reads.</summary>
internal static class EndingRewardGraphicsUploadDefinitionsConstants
{
    /// <summary>$8B:F6D8: Func216 destination VRAM word-address table.</summary>
    public const int DestinationTable = 0x8bf6d8;
    /// <summary>$8B:F6B8: Func216 source-address table, relative to WRAM bank $7F.</summary>
    public const int SourceTable = 0x8bf6b8;

    extension(EndingRewardGraphicsUploadDefinitions)
    {
        /// <inheritdoc cref="EndingRewardGraphicsUploadDefinitionsConstants.DestinationTable"/>
        internal static int DestinationTable => EndingRewardGraphicsUploadDefinitionsConstants.DestinationTable;
        /// <inheritdoc cref="EndingRewardGraphicsUploadDefinitionsConstants.SourceTable"/>
        internal static int SourceTable => EndingRewardGraphicsUploadDefinitionsConstants.SourceTable;
    }
}

/// <summary>Cartridge values of <see cref="EnemyAiCodePointers.BankB2"/> that only verification reads.</summary>
internal static class EnemyAiCodePointersBankB2Constants
{
    /// <summary>ExtendedSpritemap_CommonB2_Nothing at $B2:804F; one point hitbox selecting the common touch/shot callbacks.</summary>
    public const ushort EmptyExtendedSpritemap = 0x804f;

    extension(EnemyAiCodePointers.BankB2)
    {
        /// <inheritdoc cref="EnemyAiCodePointersBankB2Constants.EmptyExtendedSpritemap"/>
        internal static ushort EmptyExtendedSpritemap => EnemyAiCodePointersBankB2Constants.EmptyExtendedSpritemap;
    }
}

/// <summary>Cartridge values of <see cref="EnemyDropChanceDefinitions"/> that only verification reads.</summary>
internal static class EnemyDropChanceDefinitionsConstants
{
    /// <summary>Pointer of the final six-byte item-drop probability record.</summary>
    public const ushort LastPointer = 0xf4b2;
    /// <summary>Bank-$B4 address used by native enemy drop probability pointers.</summary>
    public const int NativeBank = 0xb40000;

    extension(EnemyDropChanceDefinitions)
    {
        /// <inheritdoc cref="EnemyDropChanceDefinitionsConstants.LastPointer"/>
        internal static ushort LastPointer => EnemyDropChanceDefinitionsConstants.LastPointer;
        /// <inheritdoc cref="EnemyDropChanceDefinitionsConstants.NativeBank"/>
        internal static int NativeBank => EnemyDropChanceDefinitionsConstants.NativeBank;
    }
}

/// <summary>Cartridge values of <see cref="EnemyRomTablePointers.Ceres"/> that only verification reads.</summary>
internal static class EnemyRomTablePointersCeresConstants
{
    /// <summary>Two Ceres door VRAM-transfer list pointers at $A6:F900 (4 bytes).</summary>
    public const int DoorTransferPointers = 0xa6f900;
    /// <summary>Ridley rotation divisors at $A6:D712.</summary>
    public const int RidleyRotationDivisorBytes = 0xa6d712;
    /// <summary>Ridley tail-tip spritemap pointers at $A6:DCBA.</summary>
    public const int TailTipSpritemapPointers = 0xa6dcba;
    /// <summary>Ridley wing spritemap pointers at $A6:DB02.</summary>
    public const int WingSpritemapPointers = 0xa6db02;

    extension(EnemyRomTablePointers.Ceres)
    {
        /// <inheritdoc cref="EnemyRomTablePointersCeresConstants.DoorTransferPointers"/>
        internal static int DoorTransferPointers => EnemyRomTablePointersCeresConstants.DoorTransferPointers;
        /// <inheritdoc cref="EnemyRomTablePointersCeresConstants.RidleyRotationDivisorBytes"/>
        internal static int RidleyRotationDivisorBytes => EnemyRomTablePointersCeresConstants.RidleyRotationDivisorBytes;
        /// <inheritdoc cref="EnemyRomTablePointersCeresConstants.TailTipSpritemapPointers"/>
        internal static int TailTipSpritemapPointers => EnemyRomTablePointersCeresConstants.TailTipSpritemapPointers;
        /// <inheritdoc cref="EnemyRomTablePointersCeresConstants.WingSpritemapPointers"/>
        internal static int WingSpritemapPointers => EnemyRomTablePointersCeresConstants.WingSpritemapPointers;
    }
}

/// <summary>Cartridge values of <see cref="EnemyRomTablePointers.Kraid"/> that only verification reads.</summary>
internal static class EnemyRomTablePointersKraidConstants
{
    /// <summary>Kraid ceiling-rock X-position words at $A7:ACB3.</summary>
    public const int CeilingRockXWords = 0xa7acb3;
    /// <summary>Kraid combat timer word at $A7:974A.</summary>
    public const int CombatTimerWord = 0xa7974a;
    /// <summary>Kraid death explosion Y/function records at $A7:C5E7.</summary>
    public const int DeathExplosionRecords = 0xa7c5e7;
    /// <summary>Kraid death initial timer word at $A7:9764.</summary>
    public const int DeathInitialTimerWord = 0xa79764;
    /// <summary>Fake Kraid's ordinary enemy-population record at $A1:A0EA.</summary>
    public const int FakeKraidPopulationRecord = 0xa1a0ea;
    /// <summary>Kraid hitbox left-coordinate records at $A7:B163.</summary>
    public const int HitboxLeftWords = 0xa7b163;
    /// <summary>Kraid hitbox top-coordinate records at $A7:B165.</summary>
    public const int HitboxTopWords = 0xa7b165;
    /// <summary>Initial nail spritemap pointer word at $A7:8B0C.</summary>
    public const int InitialNailSpritemapWord = 0xa78b0c;
    /// <summary>Kraid roar/growth initial timer words at $A7:96D2.</summary>
    public const int InitialTimerWords = 0xa796d2;
    /// <summary>$A7:BE46, Function_KraidNail_Initialize.downwardsVelocityPointers; selected when the sibling's Y velocity is nonnegative.</summary>
    public const int NailDownwardVelocityPointers = 0xa7be46;
    /// <summary>Kraid fingernail position-offset words at $A7:BF1D.</summary>
    public const int NailPositionOffsetWords = 0xa7bf1d;
    /// <summary>$A7:BE3E, Function_KraidNail_Initialize.upwardsVelocityPointers; selected when the sibling's Y velocity is negative.</summary>
    public const int NailUpwardVelocityPointers = 0xa7be3e;
    /// <summary>Eight ordered Kraid room-population records at $A1:9EB5.</summary>
    public const int PopulationRecords = 0xa19eb5;
    /// <summary>Kraid spat-rock X velocity words at $A7:BC65.</summary>
    public const int RockXVelocityWords = 0xa7bc65;
    /// <summary>Kraid second-phase movement record table at $A7:BA7D.</summary>
    public const int SecondPhaseMovementRecords = 0xa7ba7d;

    extension(EnemyRomTablePointers.Kraid)
    {
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.CeilingRockXWords"/>
        internal static int CeilingRockXWords => EnemyRomTablePointersKraidConstants.CeilingRockXWords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.CombatTimerWord"/>
        internal static int CombatTimerWord => EnemyRomTablePointersKraidConstants.CombatTimerWord;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.DeathExplosionRecords"/>
        internal static int DeathExplosionRecords => EnemyRomTablePointersKraidConstants.DeathExplosionRecords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.DeathInitialTimerWord"/>
        internal static int DeathInitialTimerWord => EnemyRomTablePointersKraidConstants.DeathInitialTimerWord;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.FakeKraidPopulationRecord"/>
        internal static int FakeKraidPopulationRecord => EnemyRomTablePointersKraidConstants.FakeKraidPopulationRecord;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.HitboxLeftWords"/>
        internal static int HitboxLeftWords => EnemyRomTablePointersKraidConstants.HitboxLeftWords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.HitboxTopWords"/>
        internal static int HitboxTopWords => EnemyRomTablePointersKraidConstants.HitboxTopWords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.InitialNailSpritemapWord"/>
        internal static int InitialNailSpritemapWord => EnemyRomTablePointersKraidConstants.InitialNailSpritemapWord;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.InitialTimerWords"/>
        internal static int InitialTimerWords => EnemyRomTablePointersKraidConstants.InitialTimerWords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.NailDownwardVelocityPointers"/>
        internal static int NailDownwardVelocityPointers => EnemyRomTablePointersKraidConstants.NailDownwardVelocityPointers;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.NailPositionOffsetWords"/>
        internal static int NailPositionOffsetWords => EnemyRomTablePointersKraidConstants.NailPositionOffsetWords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.NailUpwardVelocityPointers"/>
        internal static int NailUpwardVelocityPointers => EnemyRomTablePointersKraidConstants.NailUpwardVelocityPointers;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.PopulationRecords"/>
        internal static int PopulationRecords => EnemyRomTablePointersKraidConstants.PopulationRecords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.RockXVelocityWords"/>
        internal static int RockXVelocityWords => EnemyRomTablePointersKraidConstants.RockXVelocityWords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.SecondPhaseMovementRecords"/>
        internal static int SecondPhaseMovementRecords => EnemyRomTablePointersKraidConstants.SecondPhaseMovementRecords;
    }
}

/// <summary>Cartridge values of <see cref="EnemyRomTablePointers.Ridley"/> that only verification reads.</summary>
internal static class EnemyRomTablePointersRidleyConstants
{
    /// <summary>Ascending pogo target-X words at $A6:B63B.</summary>
    public const int AscendingPogoTargetXWords = 0xa6b63b;
    /// <summary>Carry-anchor X-position words selected by facing at $A6:BBEB.</summary>
    public const int CarryAnchorXWords = 0xa6bbeb;
    /// <summary>Carry-release X-position words selected by facing at $A6:BC62.</summary>
    public const int CarryReleaseXWords = 0xa6bc62;
    /// <summary>Claw X-offset words selected by facing at $A6:B9D5.</summary>
    public const int ClawXOffsetWords = 0xa6b9d5;
    /// <summary>Claw Y-offset words selected by foot separation at $A6:B9DB.</summary>
    public const int ClawYOffsetWords = 0xa6b9db;
    /// <summary>Descending pogo target-X words at $A6:B60D.</summary>
    public const int DescendingPogoTargetXWords = 0xa6b60d;
    /// <summary>Ground-attack target-X words at $A6:B6C8.</summary>
    public const int GroundAttackTargetXWords = 0xa6b6c8;
    /// <summary>Health-stage movement-divisor indexes at $A6:BB4E.</summary>
    public const int HealthMovementDivisorIndexWords = 0xa6bb4e;
    /// <summary>Three fourteen-color health-palette records at $A6:E46A (84 bytes).</summary>
    public const int HealthPaletteWords = 0xa6e46a;
    /// <summary>Four hover/pogo health-stage acceleration-divisor indexes at $A6:B439.</summary>
    public const int HoverMovementDivisorIndexWords = 0xa6b439;
    /// <summary>Pogo downward acceleration words at $A6:B959.</summary>
    public const int PogoDownwardAccelerationWords = 0xa6b959;
    /// <summary>Pogo horizontal-path pointer words at $A6:B965.</summary>
    public const int PogoHorizontalPathPointers = 0xa6b965;
    /// <summary>Pogo upward acceleration words at $A6:B94D.</summary>
    public const int PogoUpwardAccelerationWords = 0xa6b94d;
    /// <summary>Pogo vertical-path pointer words at $A6:B96D.</summary>
    public const int PogoVerticalPathPointers = 0xa6b96d;
    /// <summary>Tail rotation divisor bytes at $A6:D61F.</summary>
    public const int TailRotationDivisorBytes = 0xa6d61f;

    extension(EnemyRomTablePointers.Ridley)
    {
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.AscendingPogoTargetXWords"/>
        internal static int AscendingPogoTargetXWords => EnemyRomTablePointersRidleyConstants.AscendingPogoTargetXWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.CarryAnchorXWords"/>
        internal static int CarryAnchorXWords => EnemyRomTablePointersRidleyConstants.CarryAnchorXWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.CarryReleaseXWords"/>
        internal static int CarryReleaseXWords => EnemyRomTablePointersRidleyConstants.CarryReleaseXWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.ClawXOffsetWords"/>
        internal static int ClawXOffsetWords => EnemyRomTablePointersRidleyConstants.ClawXOffsetWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.ClawYOffsetWords"/>
        internal static int ClawYOffsetWords => EnemyRomTablePointersRidleyConstants.ClawYOffsetWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.DescendingPogoTargetXWords"/>
        internal static int DescendingPogoTargetXWords => EnemyRomTablePointersRidleyConstants.DescendingPogoTargetXWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.GroundAttackTargetXWords"/>
        internal static int GroundAttackTargetXWords => EnemyRomTablePointersRidleyConstants.GroundAttackTargetXWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.HealthMovementDivisorIndexWords"/>
        internal static int HealthMovementDivisorIndexWords => EnemyRomTablePointersRidleyConstants.HealthMovementDivisorIndexWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.HealthPaletteWords"/>
        internal static int HealthPaletteWords => EnemyRomTablePointersRidleyConstants.HealthPaletteWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.HoverMovementDivisorIndexWords"/>
        internal static int HoverMovementDivisorIndexWords => EnemyRomTablePointersRidleyConstants.HoverMovementDivisorIndexWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.PogoDownwardAccelerationWords"/>
        internal static int PogoDownwardAccelerationWords => EnemyRomTablePointersRidleyConstants.PogoDownwardAccelerationWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.PogoHorizontalPathPointers"/>
        internal static int PogoHorizontalPathPointers => EnemyRomTablePointersRidleyConstants.PogoHorizontalPathPointers;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.PogoUpwardAccelerationWords"/>
        internal static int PogoUpwardAccelerationWords => EnemyRomTablePointersRidleyConstants.PogoUpwardAccelerationWords;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.PogoVerticalPathPointers"/>
        internal static int PogoVerticalPathPointers => EnemyRomTablePointersRidleyConstants.PogoVerticalPathPointers;
        /// <inheritdoc cref="EnemyRomTablePointersRidleyConstants.TailRotationDivisorBytes"/>
        internal static int TailRotationDivisorBytes => EnemyRomTablePointersRidleyConstants.TailRotationDivisorBytes;
    }
}

/// <summary>Cartridge values of <see cref="EnemyRomTablePointers.TourianStatue"/> that only verification reads.</summary>
internal static class EnemyRomTablePointersTourianStatueConstants
{
    /// <summary>Statue instruction-list table at $AA:D810.</summary>
    public const int InstructionListWords = 0xaad810;

    extension(EnemyRomTablePointers.TourianStatue)
    {
        /// <inheritdoc cref="EnemyRomTablePointersTourianStatueConstants.InstructionListWords"/>
        internal static int InstructionListWords => EnemyRomTablePointersTourianStatueConstants.InstructionListWords;
    }
}

/// <summary>Cartridge values of <see cref="EnemyRomTablePointers.WorkRobot"/> that only verification reads.</summary>
internal static class EnemyRomTablePointersWorkRobotConstants
{
    /// <summary>Parameterized instruction-list pointer words at $A8:CC30.</summary>
    public const int InitialInstructionListWords = 0xa8cc30;

    extension(EnemyRomTablePointers.WorkRobot)
    {
        /// <inheritdoc cref="EnemyRomTablePointersWorkRobotConstants.InitialInstructionListWords"/>
        internal static int InitialInstructionListWords => EnemyRomTablePointersWorkRobotConstants.InitialInstructionListWords;
    }
}

/// <summary>Cartridge values of <see cref="EnemyVulnerabilityDefinitions"/> that only verification reads.</summary>
internal static class EnemyVulnerabilityDefinitionsConstants
{
    /// <summary>Bank-$B4 address used by native vulnerability pointers.</summary>
    public const int NativeBank = 0xb40000;

    extension(EnemyVulnerabilityDefinitions)
    {
        /// <inheritdoc cref="EnemyVulnerabilityDefinitionsConstants.NativeBank"/>
        internal static int NativeBank => EnemyVulnerabilityDefinitionsConstants.NativeBank;
    }
}

/// <summary>Cartridge values of <see cref="EscapeDachoraInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class EscapeDachoraInstructionProgramDefinitionsConstants
{
    /// <summary><c>InstList_DachoraEscape_GotoY_IfAcidLessThanCE</c>, adjacent code at $B3:EAA8.</summary>
    public const ushort FirstAdjacentCodeRoutine = 0xeaa8;

    extension(EscapeDachoraInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="EscapeDachoraInstructionProgramDefinitionsConstants.FirstAdjacentCodeRoutine"/>
        internal static ushort FirstAdjacentCodeRoutine => EscapeDachoraInstructionProgramDefinitionsConstants.FirstAdjacentCodeRoutine;
    }
}

/// <summary>Cartridge values of <see cref="EscapeEtecoonInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class EscapeEtecoonInstructionProgramDefinitionsConstants
{
    /// <summary><c>Instruction_EtecoonEscape_XPositionPlusY</c>, adjacent code at $B3:E610.</summary>
    public const ushort FirstAdjacentCodeRoutine = 0xe610;

    extension(EscapeEtecoonInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="EscapeEtecoonInstructionProgramDefinitionsConstants.FirstAdjacentCodeRoutine"/>
        internal static ushort FirstAdjacentCodeRoutine => EscapeEtecoonInstructionProgramDefinitionsConstants.FirstAdjacentCodeRoutine;
    }
}

/// <summary>Cartridge values of <see cref="EscapeTimerPresentationDefinitions"/> that only verification reads.</summary>
internal static class EscapeTimerPresentationDefinitionsConstants
{
    /// <summary>Bank-$80 table at <c>$80:9FD4</c> containing one spritemap pointer per decimal digit.</summary>
    public const int DigitPointerTable = 0x809fd4;

    extension(EscapeTimerPresentationDefinitions)
    {
        /// <inheritdoc cref="EscapeTimerPresentationDefinitionsConstants.DigitPointerTable"/>
        internal static int DigitPointerTable => EscapeTimerPresentationDefinitionsConstants.DigitPointerTable;
    }
}

/// <summary>Cartridge values of <see cref="EtecoonInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class EtecoonInstructionProgramDefinitionsConstants
{
    /// <summary>The first Etecoon movement constant after the programs, at $A7:E900.</summary>
    public const ushort FirstAdjacentMechanicsData = 0xe900;

    extension(EtecoonInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="EtecoonInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData"/>
        internal static ushort FirstAdjacentMechanicsData => EtecoonInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData;
    }
}

/// <summary>Cartridge values of <see cref="EvirInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class EvirInstructionProgramDefinitionsConstants
{
    /// <summary>First native Evir instruction callback at $A8:878F.</summary>
    public const ushort AdjacentCallbackCode = 0x878f;

    extension(EvirInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="EvirInstructionProgramDefinitionsConstants.AdjacentCallbackCode"/>
        internal static ushort AdjacentCallbackCode => EvirInstructionProgramDefinitionsConstants.AdjacentCallbackCode;
    }
}

/// <summary>Cartridge values of <see cref="EvirVisualDefinitions"/> that only verification reads.</summary>
internal static class EvirVisualDefinitionsConstants
{
    public const int FrameCount = 24;

    extension(EvirVisualDefinitions)
    {
        /// <inheritdoc cref="EvirVisualDefinitionsConstants.FrameCount"/>
        internal static int FrameCount => EvirVisualDefinitionsConstants.FrameCount;
    }
}

/// <summary>Cartridge values of <see cref="ExplodingZebesFadePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class ExplodingZebesFadePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete one-shot fade lasts 56 frames.</summary>
    public const int CycleFrames = 56;
    /// <summary>The palette-FX definition at <c>$8D:E1C4</c>.</summary>
    public const ushort DefinitionPointer = 0xe1c4;

    extension(ExplodingZebesFadePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="ExplodingZebesFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => ExplodingZebesFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="ExplodingZebesFadePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => ExplodingZebesFadePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="ExploredMapPackingDefinitions"/> that only verification reads.</summary>
internal static class ExploredMapPackingDefinitionsConstants
{
    /// <summary>$81:8131, six live area byte counts followed by unused Ceres.</summary>
    public const int NativeByteCountTable = 0x818131;
    /// <summary>$81:8138, native packed SRAM destination offsets.</summary>
    public const int NativeDestinationOffsetTable = 0x818138;
    /// <summary>$81:82D6, pointers to native sparse byte-index lists.</summary>
    public const int NativeSourcePointerTable = 0x8182d6;

    extension(ExploredMapPackingDefinitions)
    {
        /// <inheritdoc cref="ExploredMapPackingDefinitionsConstants.NativeByteCountTable"/>
        internal static int NativeByteCountTable => ExploredMapPackingDefinitionsConstants.NativeByteCountTable;
        /// <inheritdoc cref="ExploredMapPackingDefinitionsConstants.NativeDestinationOffsetTable"/>
        internal static int NativeDestinationOffsetTable => ExploredMapPackingDefinitionsConstants.NativeDestinationOffsetTable;
        /// <inheritdoc cref="ExploredMapPackingDefinitionsConstants.NativeSourcePointerTable"/>
        internal static int NativeSourcePointerTable => ExploredMapPackingDefinitionsConstants.NativeSourcePointerTable;
    }
}

/// <summary>Cartridge values of <see cref="FileSelectMapRomData"/> that only verification reads.</summary>
internal static class FileSelectMapRomDataConstants
{
    /// <summary><c>$81:AAA0</c>, FileSelectMapArea_IndexTable: display-order to geographic-area mapping.</summary>
    public const int DisplayAreaIndices = 0x81aaa0;
    /// <summary>Four signed 16.16 edge velocities, stored low word then high word.</summary>
    public const int VelocityRecordBytes = 16;
    /// <summary><c>$81:AA94</c>, RoomSelectMap_ExpandingSquare_Timers: completion occurs on signed underflow.</summary>
    public const int WindowTimers = 0x81aa94;
    /// <summary><c>$81:AA34</c>, RoomSelectMap_ExpandingSquare_Velocities: four low/high pairs per area.</summary>
    public const int WindowVelocities = 0x81aa34;

    extension(FileSelectMapRomData)
    {
        /// <inheritdoc cref="FileSelectMapRomDataConstants.DisplayAreaIndices"/>
        internal static int DisplayAreaIndices => FileSelectMapRomDataConstants.DisplayAreaIndices;
        /// <inheritdoc cref="FileSelectMapRomDataConstants.VelocityRecordBytes"/>
        internal static int VelocityRecordBytes => FileSelectMapRomDataConstants.VelocityRecordBytes;
        /// <inheritdoc cref="FileSelectMapRomDataConstants.WindowTimers"/>
        internal static int WindowTimers => FileSelectMapRomDataConstants.WindowTimers;
        /// <inheritdoc cref="FileSelectMapRomDataConstants.WindowVelocities"/>
        internal static int WindowVelocities => FileSelectMapRomDataConstants.WindowVelocities;
    }
}

/// <summary>Cartridge values of <see cref="FirefleaFxDefinitions"/> that only verification reads.</summary>
internal static class FirefleaFxDefinitionsConstants
{
    /// <summary>$88:B070 Fireflea_Darkness_Shades, six words plus one opcode alias.</summary>
    public const int DarknessReferenceAddress = 0x88b070;
    /// <summary>$88:B058 Fireflea_Flashing_Shades, twelve packed unsigned words.</summary>
    public const int FlashReferenceAddress = 0x88b058;

    extension(FirefleaFxDefinitions)
    {
        /// <inheritdoc cref="FirefleaFxDefinitionsConstants.DarknessReferenceAddress"/>
        internal static int DarknessReferenceAddress => FirefleaFxDefinitionsConstants.DarknessReferenceAddress;
        /// <inheritdoc cref="FirefleaFxDefinitionsConstants.FlashReferenceAddress"/>
        internal static int FlashReferenceAddress => FirefleaFxDefinitionsConstants.FlashReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="FirefleaInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class FirefleaInstructionProgramDefinitionsConstants
{
    /// <summary>The adjacent unused Fireflea data block at $A3:8D03.</summary>
    public const ushort AdjacentUnusedData = 0x8d03;

    extension(FirefleaInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="FirefleaInstructionProgramDefinitionsConstants.AdjacentUnusedData"/>
        internal static ushort AdjacentUnusedData => FirefleaInstructionProgramDefinitionsConstants.AdjacentUnusedData;
    }
}

/// <summary>Cartridge values of <see cref="GameOverBabyAnimationDefinitions"/> that only verification reads.</summary>
internal static class GameOverBabyAnimationDefinitionsConstants
{
    /// <summary>Sixty six-byte frame records plus three two-byte callbacks.</summary>
    public const int InstructionCount = (2 + 4 + 3) * 4 + 3 * 8;

    extension(GameOverBabyAnimationDefinitions)
    {
        /// <inheritdoc cref="GameOverBabyAnimationDefinitionsConstants.InstructionCount"/>
        internal static int InstructionCount => GameOverBabyAnimationDefinitionsConstants.InstructionCount;
    }
}

/// <summary>Cartridge values of <see cref="GameOverRomData.BabyAnimation"/> that only verification reads.</summary>
internal static class GameOverRomDataBabyAnimationConstants
{
    public const ushort FirstInstruction = 0xbc27;

    extension(GameOverRomData.BabyAnimation)
    {
        /// <inheritdoc cref="GameOverRomDataBabyAnimationConstants.FirstInstruction"/>
        internal static ushort FirstInstruction => GameOverRomDataBabyAnimationConstants.FirstInstruction;
    }
}

/// <summary>Cartridge values of <see cref="GoldenTorizoAwakeningInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoAwakeningInstructionProgramDefinitionsConstants
{
    /// <summary>First word after the awakening handoff at $AA:CACE.</summary>
    public const ushort End = 0xcace;

    extension(GoldenTorizoAwakeningInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoAwakeningInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoAwakeningInstructionProgramDefinitionsConstants.End;
    }
}

/// <summary>Cartridge values of <see cref="GoldenTorizoEggInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoEggInstructionProgramDefinitionsConstants
{
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEgg_Hatched_Left_1</c> at $86:B152.</summary>
    public const ushort HatchedLeftLoop = 0xb152;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEgg_Hatched_Right_1</c> at $86:B16D.</summary>
    public const ushort HatchedRightLoop = 0xb16d;

    extension(GoldenTorizoEggInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoEggInstructionProgramDefinitionsConstants.HatchedLeftLoop"/>
        internal static ushort HatchedLeftLoop => GoldenTorizoEggInstructionProgramDefinitionsConstants.HatchedLeftLoop;
        /// <inheritdoc cref="GoldenTorizoEggInstructionProgramDefinitionsConstants.HatchedRightLoop"/>
        internal static ushort HatchedRightLoop => GoldenTorizoEggInstructionProgramDefinitionsConstants.HatchedRightLoop;
    }
}

/// <summary>Cartridge values of <see cref="GoldenTorizoInitialInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoInitialInstructionProgramDefinitionsConstants
{
    /// <summary>The first bank-$AA sleep instruction at $AA:C9E0.</summary>
    public const ushort Sleep = 0xc9e0;

    extension(GoldenTorizoInitialInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoInitialInstructionProgramDefinitionsConstants.Sleep"/>
        internal static ushort Sleep => GoldenTorizoInitialInstructionProgramDefinitionsConstants.Sleep;
    }
}

/// <summary>Cartridge values of <see cref="GoldenTorizoJumpLandingInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoJumpLandingInstructionProgramDefinitionsConstants
{
    public const ushort End = 0xcdd7;

    extension(GoldenTorizoJumpLandingInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoJumpLandingInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoJumpLandingInstructionProgramDefinitionsConstants.End;
    }
}

/// <summary>Cartridge values of <see cref="GoldenTorizoLeftFootOrbInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoLeftFootOrbInstructionProgramDefinitionsConstants
{
    /// <summary>First byte after the left-foot-forward orb list, $AA:CC99.</summary>
    public const ushort End = GoldenTorizoRightOrbInstructionProgramDefinitions.Start;

    extension(GoldenTorizoLeftFootOrbInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoLeftFootOrbInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoLeftFootOrbInstructionProgramDefinitionsConstants.End;
    }
}

/// <summary>Cartridge values of <see cref="GoldenTorizoLeftOrbInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoLeftOrbInstructionProgramDefinitionsConstants
{
    public const ushort End = 0xcb83;

    extension(GoldenTorizoLeftOrbInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoLeftOrbInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoLeftOrbInstructionProgramDefinitionsConstants.End;
    }
}

/// <summary>Cartridge values of <see cref="GoldenTorizoLeftTurnInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoLeftTurnInstructionProgramDefinitionsConstants
{
    /// <summary>First byte after the left-turn lists, $AA:D20D.</summary>
    public const ushort End = GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg;
    /// <summary>
    /// <c>ExtendedSpritemaps_Torizo_FacingScreen_Turning_Dodging</c> at
    /// $AA:A4F0, also used by both turning-right lists.
    /// </summary>
    public const ushort FacingScreenFrame = 0xa4f0;

    extension(GoldenTorizoLeftTurnInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoLeftTurnInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoLeftTurnInstructionProgramDefinitionsConstants.End;
        /// <inheritdoc cref="GoldenTorizoLeftTurnInstructionProgramDefinitionsConstants.FacingScreenFrame"/>
        internal static ushort FacingScreenFrame => GoldenTorizoLeftTurnInstructionProgramDefinitionsConstants.FacingScreenFrame;
    }
}

/// <summary>Cartridge values of <see cref="GoldenTorizoRightOrbInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoRightOrbInstructionProgramDefinitionsConstants
{
    public const ushort End = 0xccdb;

    extension(GoldenTorizoRightOrbInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoRightOrbInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoRightOrbInstructionProgramDefinitionsConstants.End;
    }
}

/// <summary>Cartridge values of <see cref="GoldenTorizoRightSonicInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoRightSonicInstructionProgramDefinitionsConstants
{
    public const ushort End = 0xcdaf;

    extension(GoldenTorizoRightSonicInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoRightSonicInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoRightSonicInstructionProgramDefinitionsConstants.End;
    }
}

/// <summary>Cartridge values of <see cref="GoldenTorizoRightwardInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoRightwardInstructionProgramDefinitionsConstants
{
    public const ushort End = 0xd369;
    public const ushort Start = GoldenTorizoCombatInstructionPointers.DodgeTurningRight;

    extension(GoldenTorizoRightwardInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoRightwardInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoRightwardInstructionProgramDefinitionsConstants.End;
        /// <inheritdoc cref="GoldenTorizoRightwardInstructionProgramDefinitionsConstants.Start"/>
        internal static ushort Start => GoldenTorizoRightwardInstructionProgramDefinitionsConstants.Start;
    }
}

/// <summary>Cartridge values of <see cref="GoldenTorizoWalkingInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoWalkingInstructionProgramDefinitionsConstants
{
    public const ushort End = 0xd2ad;
    public const ushort Start = GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg;

    extension(GoldenTorizoWalkingInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoWalkingInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoWalkingInstructionProgramDefinitionsConstants.End;
        /// <inheritdoc cref="GoldenTorizoWalkingInstructionProgramDefinitionsConstants.Start"/>
        internal static ushort Start => GoldenTorizoWalkingInstructionProgramDefinitionsConstants.Start;
    }
}

/// <summary>Cartridge values of <see cref="GrappleFiringDefinitions"/> that only verification reads.</summary>
internal static class GrappleFiringDefinitionsConstants
{
    /// <summary>$9B:C104 GrappleBeamFireAngles, ten full-turn unsigned angle words.</summary>
    public const int AngleReferenceAddress = 0x9bc104;
    /// <summary>$9B:C122, GrappleBeamFireOffsets_NotRunning_OriginX: physical hand X offsets.</summary>
    public const int OriginXReferenceAddress = 0x9bc122;
    /// <summary>$9B:C136, GrappleBeamFireOffsets_NotRunning_OriginY: physical hand Y offsets.</summary>
    public const int OriginYReferenceAddress = 0x9bc136;
    /// <summary>$9B:C172, GrappleBeamFireOffsets_Running_OriginX: identical X-offset alias.</summary>
    public const int RunningOriginXReferenceAddress = 0x9bc172;
    /// <summary>$9B:C186, GrappleBeamFireOffsets_Running_OriginY: running physical hand Y offsets.</summary>
    public const int RunningOriginYReferenceAddress = 0x9bc186;
    /// <summary>$9B:C0DB GrappleBeamFireVelocityTable.X, ten signed 8.8 velocity words.</summary>
    public const int XVelocityReferenceAddress = 0x9bc0db;
    /// <summary>$9B:C0EF GrappleBeamFireVelocityTable.Y, ten signed 8.8 velocity words.</summary>
    public const int YVelocityReferenceAddress = 0x9bc0ef;

    extension(GrappleFiringDefinitions)
    {
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.AngleReferenceAddress"/>
        internal static int AngleReferenceAddress => GrappleFiringDefinitionsConstants.AngleReferenceAddress;
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.OriginXReferenceAddress"/>
        internal static int OriginXReferenceAddress => GrappleFiringDefinitionsConstants.OriginXReferenceAddress;
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.OriginYReferenceAddress"/>
        internal static int OriginYReferenceAddress => GrappleFiringDefinitionsConstants.OriginYReferenceAddress;
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.RunningOriginXReferenceAddress"/>
        internal static int RunningOriginXReferenceAddress => GrappleFiringDefinitionsConstants.RunningOriginXReferenceAddress;
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.RunningOriginYReferenceAddress"/>
        internal static int RunningOriginYReferenceAddress => GrappleFiringDefinitionsConstants.RunningOriginYReferenceAddress;
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.XVelocityReferenceAddress"/>
        internal static int XVelocityReferenceAddress => GrappleFiringDefinitionsConstants.XVelocityReferenceAddress;
        /// <inheritdoc cref="GrappleFiringDefinitionsConstants.YVelocityReferenceAddress"/>
        internal static int YVelocityReferenceAddress => GrappleFiringDefinitionsConstants.YVelocityReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="GunshipMotionDefinitions"/> that only verification reads.</summary>
internal static class GunshipMotionDefinitionsConstants
{
    /// <summary>$A2:A622 ShipBrakesMovementData: seventeen signed word Y deltas.</summary>
    public const int BrakeReferenceAddress = 0xa2a622;
    /// <summary>$A2:A7D0 ProcessShipHover.YVelocity: four signed bytes at stride two.</summary>
    public const int HoverDeltaReferenceAddress = 0xa2a7d0;
    /// <summary>$A2:A7CF ProcessShipHover.timer: four unsigned bytes at stride two.</summary>
    public const int HoverTimerReferenceAddress = 0xa2a7cf;

    extension(GunshipMotionDefinitions)
    {
        /// <inheritdoc cref="GunshipMotionDefinitionsConstants.BrakeReferenceAddress"/>
        internal static int BrakeReferenceAddress => GunshipMotionDefinitionsConstants.BrakeReferenceAddress;
        /// <inheritdoc cref="GunshipMotionDefinitionsConstants.HoverDeltaReferenceAddress"/>
        internal static int HoverDeltaReferenceAddress => GunshipMotionDefinitionsConstants.HoverDeltaReferenceAddress;
        /// <inheritdoc cref="GunshipMotionDefinitionsConstants.HoverTimerReferenceAddress"/>
        internal static int HoverTimerReferenceAddress => GunshipMotionDefinitionsConstants.HoverTimerReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="HZoomerInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class HZoomerInstructionProgramDefinitionsConstants
{
    /// <summary><c>Instruction_HZoomer_FunctionInY</c> immediately before the programs.</summary>
    public const ushort AdjacentFunctionCode = 0xdfc2;

    extension(HZoomerInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="HZoomerInstructionProgramDefinitionsConstants.AdjacentFunctionCode"/>
        internal static ushort AdjacentFunctionCode => HZoomerInstructionProgramDefinitionsConstants.AdjacentFunctionCode;
    }
}

/// <summary>Cartridge values of <see cref="HopperInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class HopperInstructionProgramDefinitionsConstants
{
    /// <summary><c>InstList_DessgeegaLarge_Landed_UpsideDown</c> at $A3:B25D.</summary>
    public const ushort LargeDessgeegaLandedCeiling = 0xb25d;
    /// <summary>The final hopper physics-table word immediately before the first program.</summary>
    public const ushort LastAdjacentPhysicsWord = 0xaa74;

    extension(HopperInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="HopperInstructionProgramDefinitionsConstants.LargeDessgeegaLandedCeiling"/>
        internal static ushort LargeDessgeegaLandedCeiling => HopperInstructionProgramDefinitionsConstants.LargeDessgeegaLandedCeiling;
        /// <inheritdoc cref="HopperInstructionProgramDefinitionsConstants.LastAdjacentPhysicsWord"/>
        internal static ushort LastAdjacentPhysicsWord => HopperInstructionProgramDefinitionsConstants.LastAdjacentPhysicsWord;
    }
}

/// <summary>Cartridge values of <see cref="HyperBeamPaletteFxProgramDefinitions"/> that only verification reads.</summary>
internal static class HyperBeamPaletteFxProgramDefinitionsConstants
{
    /// <summary>Number of BGR555 colors in each presentation payload.</summary>
    public const int ColorsPerFrame = 8;
    /// <summary>Native address of the destination-selection command at <c>$8D:D900</c>.</summary>
    public const int NativeEntryControlAddress = 0x8dd900;
    /// <summary>Native address of the first frame timer at <c>$8D:D904</c>.</summary>
    public const int NativeFirstFrameTimerAddress = 0x8dd904;
    /// <summary>Native address of the terminal loop command at <c>$8D:D9CC</c>.</summary>
    public const int NativeLoopControlAddress = 0x8dd9cc;

    extension(HyperBeamPaletteFxProgramDefinitions)
    {
        /// <inheritdoc cref="HyperBeamPaletteFxProgramDefinitionsConstants.ColorsPerFrame"/>
        internal static int ColorsPerFrame => HyperBeamPaletteFxProgramDefinitionsConstants.ColorsPerFrame;
        /// <inheritdoc cref="HyperBeamPaletteFxProgramDefinitionsConstants.NativeEntryControlAddress"/>
        internal static int NativeEntryControlAddress => HyperBeamPaletteFxProgramDefinitionsConstants.NativeEntryControlAddress;
        /// <inheritdoc cref="HyperBeamPaletteFxProgramDefinitionsConstants.NativeFirstFrameTimerAddress"/>
        internal static int NativeFirstFrameTimerAddress => HyperBeamPaletteFxProgramDefinitionsConstants.NativeFirstFrameTimerAddress;
        /// <inheritdoc cref="HyperBeamPaletteFxProgramDefinitionsConstants.NativeLoopControlAddress"/>
        internal static int NativeLoopControlAddress => HyperBeamPaletteFxProgramDefinitionsConstants.NativeLoopControlAddress;
    }
}

/// <summary>Cartridge values of <see cref="HyperBeamPaletteFxState"/> that only verification reads.</summary>
internal static class HyperBeamPaletteFxStateConstants
{
    /// <summary>Number of colors copied by each Hyper Beam record.</summary>
    public const int ColorsPerFrame = HyperBeamPaletteFxProgramDefinitionsConstants.ColorsPerFrame;
    /// <summary>Number of timed color records in the Hyper Beam loop.</summary>
    public const int FrameCount = HyperBeamPaletteFxProgramDefinitions.FrameCount;

    extension(HyperBeamPaletteFxState)
    {
        /// <inheritdoc cref="HyperBeamPaletteFxStateConstants.ColorsPerFrame"/>
        internal static int ColorsPerFrame => HyperBeamPaletteFxStateConstants.ColorsPerFrame;
        /// <inheritdoc cref="HyperBeamPaletteFxStateConstants.FrameCount"/>
        internal static int FrameCount => HyperBeamPaletteFxStateConstants.FrameCount;
    }
}

/// <summary>Cartridge values of <see cref="IntroBabyActorDefinitions"/> that only verification reads.</summary>
internal static class IntroBabyActorDefinitionsConstants
{
    /// <summary>Bank containing the native actor definitions and initializer routines.</summary>
    public const int NativeBank = 0x8b0000;

    extension(IntroBabyActorDefinitions)
    {
        /// <inheritdoc cref="IntroBabyActorDefinitionsConstants.NativeBank"/>
        internal static int NativeBank => IntroBabyActorDefinitionsConstants.NativeBank;
    }
}

/// <summary>Cartridge values of <see cref="IntroBabyDiscoveryCollisionDefinitions"/> that only verification reads.</summary>
internal static class IntroBabyDiscoveryCollisionDefinitionsConstants
{
    /// <summary>$8C:C083, first byte copied by the discovery room setup.</summary>
    public const int SourceAddress = 0x8cc083;
    /// <summary>The native setup copies exactly $300 bytes.</summary>
    public const int SourceByteCount = IntroBabyDiscoveryCollisionDefinitions.Columns * IntroBabyDiscoveryCollisionDefinitions.SourceRows * sizeof(ushort);

    extension(IntroBabyDiscoveryCollisionDefinitions)
    {
        /// <inheritdoc cref="IntroBabyDiscoveryCollisionDefinitionsConstants.SourceAddress"/>
        internal static int SourceAddress => IntroBabyDiscoveryCollisionDefinitionsConstants.SourceAddress;
        /// <inheritdoc cref="IntroBabyDiscoveryCollisionDefinitionsConstants.SourceByteCount"/>
        internal static int SourceByteCount => IntroBabyDiscoveryCollisionDefinitionsConstants.SourceByteCount;
    }
}

/// <summary>Cartridge values of <see cref="IntroCinematicRomData.Assets"/> that only verification reads.</summary>
internal static class IntroCinematicRomDataAssetsConstants
{
    public const int JapaneseFontTwo = 0x95d713;
    public const int MotherBrainLevelData = 0x8cbec3;

    extension(IntroCinematicRomData.Assets)
    {
        /// <inheritdoc cref="IntroCinematicRomDataAssetsConstants.JapaneseFontTwo"/>
        internal static int JapaneseFontTwo => IntroCinematicRomDataAssetsConstants.JapaneseFontTwo;
        /// <inheritdoc cref="IntroCinematicRomDataAssetsConstants.MotherBrainLevelData"/>
        internal static int MotherBrainLevelData => IntroCinematicRomDataAssetsConstants.MotherBrainLevelData;
    }
}

/// <summary>Cartridge values of <see cref="IntroCinematicRomData.Banks"/> that only verification reads.</summary>
internal static class IntroCinematicRomDataBanksConstants
{
    public const int CinematicCode = 0x8b0000;

    extension(IntroCinematicRomData.Banks)
    {
        /// <inheritdoc cref="IntroCinematicRomDataBanksConstants.CinematicCode"/>
        internal static int CinematicCode => IntroCinematicRomDataBanksConstants.CinematicCode;
    }
}

/// <summary>Cartridge values of <see cref="IntroCinematicRomData.Flashback"/> that only verification reads.</summary>
internal static class IntroCinematicRomDataFlashbackConstants
{
    public const int MotherBrainLevelByteCount = 448;

    extension(IntroCinematicRomData.Flashback)
    {
        /// <inheritdoc cref="IntroCinematicRomDataFlashbackConstants.MotherBrainLevelByteCount"/>
        internal static int MotherBrainLevelByteCount => IntroCinematicRomDataFlashbackConstants.MotherBrainLevelByteCount;
    }
}

/// <summary>Cartridge values of <see cref="IntroDiscoveryActorSpriteDefinitions"/> that only verification reads.</summary>
internal static class IntroDiscoveryActorSpriteDefinitionsConstants
{
    /// <summary>$8C:90FE, exclusive end of the reused nineteen-part asteroid composition.</summary>
    public const ushort BabyEnd = 0x90fe;
    /// <summary>$8C:8FE0, exclusive end of the three small confused-baby compositions.</summary>
    public const ushort BabySmallEnd = 0x8fe0;
    /// <summary>$8C:8F7E, exclusive end of the sixteen consecutive egg compositions.</summary>
    public const ushort EggEnd = 0x8f7e;

    extension(IntroDiscoveryActorSpriteDefinitions)
    {
        /// <inheritdoc cref="IntroDiscoveryActorSpriteDefinitionsConstants.BabyEnd"/>
        internal static ushort BabyEnd => IntroDiscoveryActorSpriteDefinitionsConstants.BabyEnd;
        /// <inheritdoc cref="IntroDiscoveryActorSpriteDefinitionsConstants.BabySmallEnd"/>
        internal static ushort BabySmallEnd => IntroDiscoveryActorSpriteDefinitionsConstants.BabySmallEnd;
        /// <inheritdoc cref="IntroDiscoveryActorSpriteDefinitionsConstants.EggEnd"/>
        internal static ushort EggEnd => IntroDiscoveryActorSpriteDefinitionsConstants.EggEnd;
    }
}

/// <summary>Cartridge values of <see cref="IntroEggEffectDefinitions"/> that only verification reads.</summary>
internal static class IntroEggEffectDefinitionsConstants
{
    /// <summary>Bank containing the seven native cinematic-object definitions.</summary>
    public const int NativeDefinitionBank = 0x8b0000;

    extension(IntroEggEffectDefinitions)
    {
        /// <inheritdoc cref="IntroEggEffectDefinitionsConstants.NativeDefinitionBank"/>
        internal static int NativeDefinitionBank => IntroEggEffectDefinitionsConstants.NativeDefinitionBank;
    }
}

/// <summary>Cartridge values of <see cref="IntroEggEffectSpriteDefinitions"/> that only verification reads.</summary>
internal static class IntroEggEffectSpriteDefinitionsConstants
{
    /// <summary>$8C:8FCB, exclusive end after eleven consecutive seven-byte records.</summary>
    public const ushort End = 0x8fcb;

    extension(IntroEggEffectSpriteDefinitions)
    {
        /// <inheritdoc cref="IntroEggEffectSpriteDefinitionsConstants.End"/>
        internal static ushort End => IntroEggEffectSpriteDefinitionsConstants.End;
    }
}

/// <summary>Cartridge values of <see cref="IntroEggMotionDefinitions"/> that only verification reads.</summary>
internal static class IntroEggMotionDefinitionsConstants
{
    /// <summary>
    /// <c>$8B:A97C</c>, the six interleaved shell-fragment X/Y origins before the
    /// cartridge adds its fixed $10/$3B placement biases.
    /// </summary>
    public const int InitialPositionReferenceAddress = 0x8ba97c;

    extension(IntroEggMotionDefinitions)
    {
        /// <inheritdoc cref="IntroEggMotionDefinitionsConstants.InitialPositionReferenceAddress"/>
        internal static int InitialPositionReferenceAddress => IntroEggMotionDefinitionsConstants.InitialPositionReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="IntroMotherBrainDefinitions"/> that only verification reads.</summary>
internal static class IntroMotherBrainDefinitionsConstants
{
    /// <summary><c>$8B:B9CA</c>, five large-explosion initial instruction timers.</summary>
    public const int BigTimerReferenceAddress = 0x8bb9ca;
    /// <summary><c>$8B:B9B6</c>, five signed large-explosion X offsets.</summary>
    public const int BigXOffsetReferenceAddress = 0x8bb9b6;
    /// <summary><c>$8B:B9C0</c>, five signed large-explosion Y offsets.</summary>
    public const int BigYOffsetReferenceAddress = 0x8bb9c0;
    /// <summary>Bank containing the native cinematic-object definitions and placement tables.</summary>
    public const int NativeBank = 0x8b0000;
    /// <summary><c>$8B:BA09</c>, three small-explosion initial instruction timers.</summary>
    public const int SmallTimerReferenceAddress = 0x8bba09;
    /// <summary><c>$8B:B9FD</c>, three signed small-explosion X offsets.</summary>
    public const int SmallXOffsetReferenceAddress = 0x8bb9fd;
    /// <summary><c>$8B:BA03</c>, three signed small-explosion Y offsets.</summary>
    public const int SmallYOffsetReferenceAddress = 0x8bba03;

    extension(IntroMotherBrainDefinitions)
    {
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.BigTimerReferenceAddress"/>
        internal static int BigTimerReferenceAddress => IntroMotherBrainDefinitionsConstants.BigTimerReferenceAddress;
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.BigXOffsetReferenceAddress"/>
        internal static int BigXOffsetReferenceAddress => IntroMotherBrainDefinitionsConstants.BigXOffsetReferenceAddress;
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.BigYOffsetReferenceAddress"/>
        internal static int BigYOffsetReferenceAddress => IntroMotherBrainDefinitionsConstants.BigYOffsetReferenceAddress;
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.NativeBank"/>
        internal static int NativeBank => IntroMotherBrainDefinitionsConstants.NativeBank;
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.SmallTimerReferenceAddress"/>
        internal static int SmallTimerReferenceAddress => IntroMotherBrainDefinitionsConstants.SmallTimerReferenceAddress;
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.SmallXOffsetReferenceAddress"/>
        internal static int SmallXOffsetReferenceAddress => IntroMotherBrainDefinitionsConstants.SmallXOffsetReferenceAddress;
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.SmallYOffsetReferenceAddress"/>
        internal static int SmallYOffsetReferenceAddress => IntroMotherBrainDefinitionsConstants.SmallYOffsetReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="IntroMotherBrainExplosionInstructionDefinitions"/> that only verification reads.</summary>
internal static class IntroMotherBrainExplosionInstructionDefinitionsConstants
{
    /// <summary>One native six-frame large loop occupies 52 frames including blank hold.</summary>
    public const int BigLoopFrames = 52;
    /// <summary>One native six-frame small loop occupies 34 frames including blank hold.</summary>
    public const int SmallLoopFrames = 34;

    extension(IntroMotherBrainExplosionInstructionDefinitions)
    {
        /// <inheritdoc cref="IntroMotherBrainExplosionInstructionDefinitionsConstants.BigLoopFrames"/>
        internal static int BigLoopFrames => IntroMotherBrainExplosionInstructionDefinitionsConstants.BigLoopFrames;
        /// <inheritdoc cref="IntroMotherBrainExplosionInstructionDefinitionsConstants.SmallLoopFrames"/>
        internal static int SmallLoopFrames => IntroMotherBrainExplosionInstructionDefinitionsConstants.SmallLoopFrames;
    }
}

/// <summary>Cartridge values of <see cref="IntroMotherBrainExplosionSpriteDefinitions"/> that only verification reads.</summary>
internal static class IntroMotherBrainExplosionSpriteDefinitionsConstants
{
    /// <summary>$8C:98D2, exclusive end of the twelve consecutive composition records.</summary>
    public const ushort End = 0x98d2;

    extension(IntroMotherBrainExplosionSpriteDefinitions)
    {
        /// <inheritdoc cref="IntroMotherBrainExplosionSpriteDefinitionsConstants.End"/>
        internal static ushort End => IntroMotherBrainExplosionSpriteDefinitionsConstants.End;
    }
}

/// <summary>Cartridge values of <see cref="IntroMotherBrainSpriteDefinitions"/> that only verification reads.</summary>
internal static class IntroMotherBrainSpriteDefinitionsConstants
{
    /// <summary>$8C:8C2F, second nine-part Mother Brain frame.</summary>
    public const ushort FrameOne = IntroMotherBrainSpriteDefinitions.FrameZero + 2 + 5 * IntroMotherBrainSpriteDefinitions.StockPartCount;
    /// <summary>$8C:8C5E, third nine-part Mother Brain frame.</summary>
    public const ushort FrameTwo = FrameOne + 2 + 5 * IntroMotherBrainSpriteDefinitions.StockPartCount;

    extension(IntroMotherBrainSpriteDefinitions)
    {
        /// <inheritdoc cref="IntroMotherBrainSpriteDefinitionsConstants.FrameOne"/>
        internal static ushort FrameOne => IntroMotherBrainSpriteDefinitionsConstants.FrameOne;
        /// <inheritdoc cref="IntroMotherBrainSpriteDefinitionsConstants.FrameTwo"/>
        internal static ushort FrameTwo => IntroMotherBrainSpriteDefinitionsConstants.FrameTwo;
    }
}

/// <summary>Cartridge values of <see cref="IntroRinkaDefinitions"/> that only verification reads.</summary>
internal static class IntroRinkaDefinitionsConstants
{
    /// <summary><c>$8B:B8B5</c>, four Rinka initial X positions.</summary>
    public const int InitialXReferenceAddress = 0x8bb8b5;
    /// <summary><c>$8B:B8BD</c>, four Rinka initial Y positions before the fixed eight-pixel subtraction.</summary>
    public const int InitialYReferenceAddress = 0x8bb8bd;
    /// <summary>Bank containing the native actor definitions and physical initializer tables.</summary>
    public const int NativeBank = 0x8b0000;
    /// <summary>Number of parameter-selected Rinka initializer rows at <c>$8B:B8B5</c>.</summary>
    public const int RinkaCount = 4;
    /// <summary><c>$8B:B985</c>, four signed whole-pixel X velocity components.</summary>
    public const int XWholeVelocityReferenceAddress = 0x8bb985;

    extension(IntroRinkaDefinitions)
    {
        /// <inheritdoc cref="IntroRinkaDefinitionsConstants.InitialXReferenceAddress"/>
        internal static int InitialXReferenceAddress => IntroRinkaDefinitionsConstants.InitialXReferenceAddress;
        /// <inheritdoc cref="IntroRinkaDefinitionsConstants.InitialYReferenceAddress"/>
        internal static int InitialYReferenceAddress => IntroRinkaDefinitionsConstants.InitialYReferenceAddress;
        /// <inheritdoc cref="IntroRinkaDefinitionsConstants.NativeBank"/>
        internal static int NativeBank => IntroRinkaDefinitionsConstants.NativeBank;
        /// <inheritdoc cref="IntroRinkaDefinitionsConstants.RinkaCount"/>
        internal static int RinkaCount => IntroRinkaDefinitionsConstants.RinkaCount;
        /// <inheritdoc cref="IntroRinkaDefinitionsConstants.XWholeVelocityReferenceAddress"/>
        internal static int XWholeVelocityReferenceAddress => IntroRinkaDefinitionsConstants.XWholeVelocityReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="IntroRinkaSpriteDefinitions"/> that only verification reads.</summary>
internal static class IntroRinkaSpriteDefinitionsConstants
{
    /// <summary>$8C:8CCF, exclusive end after three consecutive 22-byte records.</summary>
    public const ushort End = 0x8ccf;

    extension(IntroRinkaSpriteDefinitions)
    {
        /// <inheritdoc cref="IntroRinkaSpriteDefinitionsConstants.End"/>
        internal static ushort End => IntroRinkaSpriteDefinitionsConstants.End;
    }
}

/// <summary>Cartridge values of <see cref="IntroScientistSpriteDefinitions"/> that only verification reads.</summary>
internal static class IntroScientistSpriteDefinitionsConstants
{
    /// <summary>$8C:8D6F, exclusive end of ten consecutive compositions.</summary>
    public const ushort End = 0x8d6f;

    extension(IntroScientistSpriteDefinitions)
    {
        /// <inheritdoc cref="IntroScientistSpriteDefinitionsConstants.End"/>
        internal static ushort End => IntroScientistSpriteDefinitionsConstants.End;
    }
}

/// <summary>Cartridge values of <see cref="KraidArmCollisionDefinitions"/> that only verification reads.</summary>
internal static class KraidArmCollisionDefinitionsConstants
{
    public const int FrameCount = 22;

    extension(KraidArmCollisionDefinitions)
    {
        /// <inheritdoc cref="KraidArmCollisionDefinitionsConstants.FrameCount"/>
        internal static int FrameCount => KraidArmCollisionDefinitionsConstants.FrameCount;
    }
}

/// <summary>Cartridge values of <see cref="KraidArmInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class KraidArmInstructionProgramDefinitionsConstants
{
    /// <summary>First adjacent Kraid-lint instruction program at $A7:8AFE.</summary>
    public const ushort AdjacentLintProgram = 0x8afe;

    extension(KraidArmInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="KraidArmInstructionProgramDefinitionsConstants.AdjacentLintProgram"/>
        internal static ushort AdjacentLintProgram => KraidArmInstructionProgramDefinitionsConstants.AdjacentLintProgram;
    }
}

/// <summary>Cartridge values of <see cref="KraidBackgroundRomData"/> that only verification reads.</summary>
internal static class KraidBackgroundRomDataConstants
{
    /// <summary>
    /// First untouched word left by <c>$A7:AB19-$AB2C</c> after the lower stream is
    /// decompressed directly into the working tilemap.
    /// </summary>
    public const int PreservedLowerTailFirstWord = 0x0700;

    extension(KraidBackgroundRomData)
    {
        /// <inheritdoc cref="KraidBackgroundRomDataConstants.PreservedLowerTailFirstWord"/>
        internal static int PreservedLowerTailFirstWord => KraidBackgroundRomDataConstants.PreservedLowerTailFirstWord;
    }
}

/// <summary>Cartridge values of <see cref="KraidLintInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class KraidLintInstructionProgramDefinitionsConstants
{
    /// <summary>The first adjacent Kraid fingernail program at $A7:8B0A.</summary>
    public const ushort FirstAdjacentFootProgram = 0x8b0a;

    extension(KraidLintInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="KraidLintInstructionProgramDefinitionsConstants.FirstAdjacentFootProgram"/>
        internal static ushort FirstAdjacentFootProgram => KraidLintInstructionProgramDefinitionsConstants.FirstAdjacentFootProgram;
    }
}

/// <summary>Cartridge values of <see cref="KraidNailInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class KraidNailInstructionProgramDefinitionsConstants
{
    /// <summary>First adjacent unused extended-spritemap record at $A7:8B2E.</summary>
    public const ushort AdjacentPresentationData = 0x8b2e;

    extension(KraidNailInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="KraidNailInstructionProgramDefinitionsConstants.AdjacentPresentationData"/>
        internal static ushort AdjacentPresentationData => KraidNailInstructionProgramDefinitionsConstants.AdjacentPresentationData;
    }
}

/// <summary>Cartridge values of <see cref="KraidRoomPlmDrawDefinitions"/> that only verification reads.</summary>
internal static class KraidRoomPlmDrawDefinitionsConstants
{
    /// <summary><c>$84:93EF</c>: start of the following Phantoon draw region.</summary>
    public const ushort EndExclusive = 0x93ef;

    extension(KraidRoomPlmDrawDefinitions)
    {
        /// <inheritdoc cref="KraidRoomPlmDrawDefinitionsConstants.EndExclusive"/>
        internal static ushort EndExclusive => KraidRoomPlmDrawDefinitionsConstants.EndExclusive;
    }
}

/// <summary>Cartridge values of <see cref="KraidRoomPlmProgramDefinitions"/> that only verification reads.</summary>
internal static class KraidRoomPlmProgramDefinitionsConstants
{
    /// <summary><c>$84:ABE3</c>: first byte of following Mother Brain PLM program.</summary>
    public const ushort EndExclusive = 0xabe3;

    extension(KraidRoomPlmProgramDefinitions)
    {
        /// <inheritdoc cref="KraidRoomPlmProgramDefinitionsConstants.EndExclusive"/>
        internal static ushort EndExclusive => KraidRoomPlmProgramDefinitionsConstants.EndExclusive;
    }
}

/// <summary>Cartridge values of <see cref="LandingSiteRomData"/> that only verification reads.</summary>
internal static class LandingSiteRomDataConstants
{
    /// <summary>Synthetic bank-$83 door used by the intro landing cutscene.</summary>
    public const ushort LandingCutsceneDoorPointer = 0x88fe;

    extension(LandingSiteRomData)
    {
        /// <inheritdoc cref="LandingSiteRomDataConstants.LandingCutsceneDoorPointer"/>
        internal static ushort LandingCutsceneDoorPointer => LandingSiteRomDataConstants.LandingCutsceneDoorPointer;
    }
}

/// <summary>Cartridge values of <see cref="LoadStationRomData"/> that only verification reads.</summary>
internal static class LoadStationRomDataConstants
{
    /// <summary>$80:CC19, exclusive end of the Ceres list.</summary>
    public const int DataEnd = 0x80cc19;
    /// <summary>$80:C4B5, seven area pointers followed by the end pointer for the Ceres list.</summary>
    public const int PointerTable = 0x80c4b5;

    extension(LoadStationRomData)
    {
        /// <inheritdoc cref="LoadStationRomDataConstants.DataEnd"/>
        internal static int DataEnd => LoadStationRomDataConstants.DataEnd;
        /// <inheritdoc cref="LoadStationRomDataConstants.PointerTable"/>
        internal static int PointerTable => LoadStationRomDataConstants.PointerTable;
    }
}

/// <summary>Cartridge values of <see cref="MamaTurtleEnemyDefinitionCatalog"/> that only verification reads.</summary>
internal static class MamaTurtleEnemyDefinitionCatalogConstants
{
    /// <summary>First source byte occupied by the two contiguous native headers.</summary>
    public const int SourceAddress = 0xa0cf3f;
    /// <summary>Total byte length of the two native 64-byte headers.</summary>
    public const int SourceByteLength = 128;

    extension(MamaTurtleEnemyDefinitionCatalog)
    {
        /// <inheritdoc cref="MamaTurtleEnemyDefinitionCatalogConstants.SourceAddress"/>
        internal static int SourceAddress => MamaTurtleEnemyDefinitionCatalogConstants.SourceAddress;
        /// <inheritdoc cref="MamaTurtleEnemyDefinitionCatalogConstants.SourceByteLength"/>
        internal static int SourceByteLength => MamaTurtleEnemyDefinitionCatalogConstants.SourceByteLength;
    }
}

/// <summary>Cartridge values of <see cref="MamaTurtleShellContourDefinitions"/> that only verification reads.</summary>
internal static class MamaTurtleShellContourDefinitionsConstants
{
    /// <summary>Number of signed words in the two 24-pixel contour halves.</summary>
    public const int EntryCount = 48;
    /// <summary>First signed contour word at <c>$A2:8E80</c>.</summary>
    public const int SourceAddress = 0xa28e80;

    extension(MamaTurtleShellContourDefinitions)
    {
        /// <inheritdoc cref="MamaTurtleShellContourDefinitionsConstants.EntryCount"/>
        internal static int EntryCount => MamaTurtleShellContourDefinitionsConstants.EntryCount;
        /// <inheritdoc cref="MamaTurtleShellContourDefinitionsConstants.SourceAddress"/>
        internal static int SourceAddress => MamaTurtleShellContourDefinitionsConstants.SourceAddress;
    }
}

/// <summary>Cartridge values of <see cref="MapAnimationRomData"/> that only verification reads.</summary>
internal static class MapAnimationRomDataConstants
{
    /// <summary>$82:C100, SpritePalette_IndexValues[3], used by DrawPauseScreenSpriteAnim for highlights and map arrows.</summary>
    public const int AnimatedSpritePalette = 0x82c100;
    /// <summary>$82:C10C contains fourteen three-byte highlight records before the loop sentinel.</summary>
    public const int PaletteFrameCount = 14;

    extension(MapAnimationRomData)
    {
        /// <inheritdoc cref="MapAnimationRomDataConstants.AnimatedSpritePalette"/>
        internal static int AnimatedSpritePalette => MapAnimationRomDataConstants.AnimatedSpritePalette;
        /// <inheritdoc cref="MapAnimationRomDataConstants.PaletteFrameCount"/>
        internal static int PaletteFrameCount => MapAnimationRomDataConstants.PaletteFrameCount;
    }
}

/// <summary>Cartridge values of <see cref="MapStaticPalettesRomData"/> that only verification reads.</summary>
internal static class MapStaticPalettesRomDataConstants
{
    /// <summary>$81:A546 starts executable foreground-load code after all world-map palette copy records.</summary>
    public const int WorldPaletteDataEnd = 0x81a546;

    extension(MapStaticPalettesRomData)
    {
        /// <inheritdoc cref="MapStaticPalettesRomDataConstants.WorldPaletteDataEnd"/>
        internal static int WorldPaletteDataEnd => MapStaticPalettesRomDataConstants.WorldPaletteDataEnd;
    }
}

/// <summary>Cartridge values of <see cref="MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>$8D:F795: native Maridia1 sand-pit palette-FX definition; subsequent selected definitions occupy four bytes each.</summary>
    public const ushort SandPitDefinition = 0xF795;

    extension(MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitionsConstants.SandPitDefinition"/>
        internal static ushort SandPitDefinition => MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitionsConstants.SandPitDefinition;
    }
}

/// <summary>Cartridge values of <see cref="MaridiaLargeSnailInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class MaridiaLargeSnailInstructionProgramDefinitionsConstants
{
    /// <summary>The selector table immediately following Oum's programs, at $A2:CB77.</summary>
    public const ushort FirstAdjacentMechanicsData = 0xcb77;

    extension(MaridiaLargeSnailInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="MaridiaLargeSnailInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData"/>
        internal static ushort FirstAdjacentMechanicsData => MaridiaLargeSnailInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData;
    }
}

/// <summary>Cartridge values of <see cref="MenuShoulderButtonArtwork"/> that only verification reads.</summary>
internal static class MenuShoulderButtonArtworkConstants
{
    public const int StoredGlyphByteCount = 2 * sizeof(ushort);

    extension(MenuShoulderButtonArtwork)
    {
        /// <inheritdoc cref="MenuShoulderButtonArtworkConstants.StoredGlyphByteCount"/>
        internal static int StoredGlyphByteCount => MenuShoulderButtonArtworkConstants.StoredGlyphByteCount;
    }
}

/// <summary>Cartridge values of <see cref="MetroidInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class MetroidInstructionProgramDefinitionsConstants
{
    /// <summary><c>BombedOffVelocities</c>, adjacent non-instruction data at $A3:EA3F.</summary>
    public const ushort AdjacentBombedOffVelocities = 0xea3f;
    /// <summary><c>Instruction_Metroid_PlayRandomMetroidSFX</c> entry at $A3:EA1F.</summary>
    public const ushort ChasingSoundCallback = 0xea1f;

    extension(MetroidInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="MetroidInstructionProgramDefinitionsConstants.AdjacentBombedOffVelocities"/>
        internal static ushort AdjacentBombedOffVelocities => MetroidInstructionProgramDefinitionsConstants.AdjacentBombedOffVelocities;
        /// <inheritdoc cref="MetroidInstructionProgramDefinitionsConstants.ChasingSoundCallback"/>
        internal static ushort ChasingSoundCallback => MetroidInstructionProgramDefinitionsConstants.ChasingSoundCallback;
    }
}

/// <summary>Cartridge values of <see cref="MochtroidInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class MochtroidInstructionProgramDefinitionsConstants
{
    /// <summary>The shake-velocity table immediately after the programs, at $A3:A76D.</summary>
    public const ushort FirstAdjacentMechanicsData = 0xa76d;

    extension(MochtroidInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="MochtroidInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData"/>
        internal static ushort FirstAdjacentMechanicsData => MochtroidInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData;
    }
}

/// <summary>Cartridge values of <see cref="MochtroidVisualDefinitions"/> that only verification reads.</summary>
internal static class MochtroidVisualDefinitionsConstants
{
    public const int FrameCount = 6;

    extension(MochtroidVisualDefinitions)
    {
        /// <inheritdoc cref="MochtroidVisualDefinitionsConstants.FrameCount"/>
        internal static int FrameCount => MochtroidVisualDefinitionsConstants.FrameCount;
    }
}

/// <summary>Cartridge values of <see cref="MorphBallEyeInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class MorphBallEyeInstructionProgramDefinitionsConstants
{
    /// <summary><c>EyeConstants</c>, adjacent non-instruction data at $A8:9050.</summary>
    public const ushort AdjacentProximityDefinitions = 0x9050;

    extension(MorphBallEyeInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="MorphBallEyeInstructionProgramDefinitionsConstants.AdjacentProximityDefinitions"/>
        internal static ushort AdjacentProximityDefinitions => MorphBallEyeInstructionProgramDefinitionsConstants.AdjacentProximityDefinitions;
    }
}

/// <summary>Cartridge values of <see cref="MotherBrainBabyInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainBabyInstructionProgramDefinitionsConstants
{
    /// <summary>ProcessMotherBrainInvincibilityPalette at $A9:CFD4, adjacent executable code outside the Baby list.</summary>
    public const ushort FirstAdjacentMovementCode = 0xcfd4;

    extension(MotherBrainBabyInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="MotherBrainBabyInstructionProgramDefinitionsConstants.FirstAdjacentMovementCode"/>
        internal static ushort FirstAdjacentMovementCode => MotherBrainBabyInstructionProgramDefinitionsConstants.FirstAdjacentMovementCode;
    }
}

/// <summary>Cartridge values of <see cref="MotherBrainContactHitboxDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainContactHitboxDefinitionsConstants
{
    /// <summary><c>$A9:B427</c>, two rectangles attached to the standing body.</summary>
    public const int BodySourceAddress = 0xa9b427;
    /// <summary><c>$A9:B439</c>, two rectangles attached to the independently moving brain.</summary>
    public const int BrainSourceAddress = 0xa9b439;
    /// <summary><c>$A9:B44B</c>, one rectangle reused by neck joints one through three.</summary>
    public const int NeckSourceAddress = 0xa9b44b;

    extension(MotherBrainContactHitboxDefinitions)
    {
        /// <inheritdoc cref="MotherBrainContactHitboxDefinitionsConstants.BodySourceAddress"/>
        internal static int BodySourceAddress => MotherBrainContactHitboxDefinitionsConstants.BodySourceAddress;
        /// <inheritdoc cref="MotherBrainContactHitboxDefinitionsConstants.BrainSourceAddress"/>
        internal static int BrainSourceAddress => MotherBrainContactHitboxDefinitionsConstants.BrainSourceAddress;
        /// <inheritdoc cref="MotherBrainContactHitboxDefinitionsConstants.NeckSourceAddress"/>
        internal static int NeckSourceAddress => MotherBrainContactHitboxDefinitionsConstants.NeckSourceAddress;
    }
}

/// <summary>Cartridge values of <see cref="MotherBrainCorpseRottingState"/> that only verification reads.</summary>
internal static class MotherBrainCorpseRottingStateConstants
{
    /// <summary>Six tile rows times <c>$E0</c> bytes per row.</summary>
    public const int GraphicsBufferSize = 0x0540;

    extension(MotherBrainCorpseRottingState)
    {
        /// <inheritdoc cref="MotherBrainCorpseRottingStateConstants.GraphicsBufferSize"/>
        internal static int GraphicsBufferSize => MotherBrainCorpseRottingStateConstants.GraphicsBufferSize;
    }
}

/// <summary>Cartridge values of <see cref="MotherBrainDeathRomData"/> that only verification reads.</summary>
internal static class MotherBrainDeathRomDataConstants
{
    /// <summary>$AD:E9E8 pointer table, fourteen-color body fades followed by a null word.</summary>
    public const int BodyFadeTable = 0xade9e8;
    /// <summary>$AD:F107 pointer table, fifteen-color decapitated-head fades followed by null.</summary>
    public const int CorpseFadeTable = 0xadf107;

    extension(MotherBrainDeathRomData)
    {
        /// <inheritdoc cref="MotherBrainDeathRomDataConstants.BodyFadeTable"/>
        internal static int BodyFadeTable => MotherBrainDeathRomDataConstants.BodyFadeTable;
        /// <inheritdoc cref="MotherBrainDeathRomDataConstants.CorpseFadeTable"/>
        internal static int CorpseFadeTable => MotherBrainDeathRomDataConstants.CorpseFadeTable;
    }
}

/// <summary>Cartridge values of <see cref="MotherBrainFakeDeathPaletteRomData"/> that only verification reads.</summary>
internal static class MotherBrainFakeDeathPaletteRomDataConstants
{
    /// <summary>The resurrection uses the same $AD:ED9C pointer table as the later revival fade.</summary>
    public const int FromGreyPointerTable = MotherBrainDrainedPaletteRomData.FromGreyTable;

    extension(MotherBrainFakeDeathPaletteRomData)
    {
        /// <inheritdoc cref="MotherBrainFakeDeathPaletteRomDataConstants.FromGreyPointerTable"/>
        internal static int FromGreyPointerTable => MotherBrainFakeDeathPaletteRomDataConstants.FromGreyPointerTable;
    }
}

/// <summary>Cartridge values of <see cref="MotherBrainFallingTubePopulationDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainFallingTubePopulationDefinitionsConstants
{
    /// <summary>Bank $A9 containing the native tube-collapse placement records.</summary>
    public const int NativeBank = 0xa90000;

    extension(MotherBrainFallingTubePopulationDefinitions)
    {
        /// <inheritdoc cref="MotherBrainFallingTubePopulationDefinitionsConstants.NativeBank"/>
        internal static int NativeBank => MotherBrainFallingTubePopulationDefinitionsConstants.NativeBank;
    }
}

/// <summary>Cartridge values of <see cref="MotherBrainHeadInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainHeadInstructionProgramDefinitionsConstants
{
    /// <summary>$A9:9DF5, final current pointer in the dedicated Baby interpreter.</summary>
    public const ushort BabyAttackActiveEnd = 0x9df5;
    /// <summary>$A9:9DB1, Baby-targeted four-ring head list.</summary>
    public const ushort BabyAttackStart = 0x9db1;
    /// <summary>$A9:9F32, final current pointer in the dedicated bomb interpreter.</summary>
    public const ushort BombActiveEnd = 0x9f32;
    /// <summary>$A9:9F00, phase-three bomb head list.</summary>
    public const ushort BombStart = 0x9f00;
    /// <summary>$A9:9CE1, final current pointer in the dedicated neutral interpreter.</summary>
    public const ushort NeutralActiveEnd = 0x9ce1;

    extension(MotherBrainHeadInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="MotherBrainHeadInstructionProgramDefinitionsConstants.BabyAttackActiveEnd"/>
        internal static ushort BabyAttackActiveEnd => MotherBrainHeadInstructionProgramDefinitionsConstants.BabyAttackActiveEnd;
        /// <inheritdoc cref="MotherBrainHeadInstructionProgramDefinitionsConstants.BabyAttackStart"/>
        internal static ushort BabyAttackStart => MotherBrainHeadInstructionProgramDefinitionsConstants.BabyAttackStart;
        /// <inheritdoc cref="MotherBrainHeadInstructionProgramDefinitionsConstants.BombActiveEnd"/>
        internal static ushort BombActiveEnd => MotherBrainHeadInstructionProgramDefinitionsConstants.BombActiveEnd;
        /// <inheritdoc cref="MotherBrainHeadInstructionProgramDefinitionsConstants.BombStart"/>
        internal static ushort BombStart => MotherBrainHeadInstructionProgramDefinitionsConstants.BombStart;
        /// <inheritdoc cref="MotherBrainHeadInstructionProgramDefinitionsConstants.NeutralActiveEnd"/>
        internal static ushort NeutralActiveEnd => MotherBrainHeadInstructionProgramDefinitionsConstants.NeutralActiveEnd;
    }
}

/// <summary>Cartridge values of <see cref="MotherBrainLegTileTransferDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainLegTileTransferDefinitionsConstants
{
    /// <summary>The full $A9:8F8F address used only by cartridge parity checks.</summary>
    public const int NativeListAddress = 0xa98f8f;

    extension(MotherBrainLegTileTransferDefinitions)
    {
        /// <inheritdoc cref="MotherBrainLegTileTransferDefinitionsConstants.NativeListAddress"/>
        internal static int NativeListAddress => MotherBrainLegTileTransferDefinitionsConstants.NativeListAddress;
    }
}

/// <summary>Cartridge values of <see cref="MotherBrainRoomPaletteProgramDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainRoomPaletteProgramDefinitionsConstants
{
    public const int MechanicsWordCount = MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount + 2;

    extension(MotherBrainRoomPaletteProgramDefinitions)
    {
        /// <inheritdoc cref="MotherBrainRoomPaletteProgramDefinitionsConstants.MechanicsWordCount"/>
        internal static int MechanicsWordCount => MotherBrainRoomPaletteProgramDefinitionsConstants.MechanicsWordCount;
    }
}

/// <summary>Cartridge values of <see cref="MotherBrainTileTransferDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainTileTransferDefinitionsConstants
{
    /// <summary>$A9:8FE5, four size/source/destination records loading Baby graphics +$400 onward.</summary>
    public const int BabyTileList = 0xa98fe5;
    /// <summary>Each native record contains a word size, long source, and word VRAM destination.</summary>
    public const int RecordSize = 7;

    extension(MotherBrainTileTransferDefinitions)
    {
        /// <inheritdoc cref="MotherBrainTileTransferDefinitionsConstants.BabyTileList"/>
        internal static int BabyTileList => MotherBrainTileTransferDefinitionsConstants.BabyTileList;
        /// <inheritdoc cref="MotherBrainTileTransferDefinitionsConstants.RecordSize"/>
        internal static int RecordSize => MotherBrainTileTransferDefinitionsConstants.RecordSize;
    }
}

/// <summary>Cartridge values of <see cref="NinjaSpacePiratePaletteDefinitions"/> that only verification reads.</summary>
internal static class NinjaSpacePiratePaletteDefinitionsConstants
{
    /// <summary>Native source of the shared gold-Pirate color words at $B2:8727.</summary>
    public const int SharedGoldPirateSource = 0xb28727;

    extension(NinjaSpacePiratePaletteDefinitions)
    {
        /// <inheritdoc cref="NinjaSpacePiratePaletteDefinitionsConstants.SharedGoldPirateSource"/>
        internal static int SharedGoldPirateSource => NinjaSpacePiratePaletteDefinitionsConstants.SharedGoldPirateSource;
    }
}

/// <summary>Cartridge values of <see cref="NintendoLogoFadePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The unused boot-logo palette-FX definition at <c>$8D:E198</c>.</summary>
    public const ushort BootLogoDefinitionPointer = 0xe198;
    /// <summary>The copyright palette-FX definition at <c>$8D:E19C</c>.</summary>
    public const ushort CopyrightDefinitionPointer = 0xe19c;
    /// <summary>Either entry runs the shared fade for 24 frames.</summary>
    public const int CycleFrames = NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount * NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FrameDuration;

    extension(NintendoLogoFadePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants.BootLogoDefinitionPointer"/>
        internal static ushort BootLogoDefinitionPointer => NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants.BootLogoDefinitionPointer;
        /// <inheritdoc cref="NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants.CopyrightDefinitionPointer"/>
        internal static ushort CopyrightDefinitionPointer => NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants.CopyrightDefinitionPointer;
        /// <inheritdoc cref="NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}

/// <summary>Cartridge values of <see cref="NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class NorfairEnvironmentalPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The shared complete-cycle duration.</summary>
    public const int CycleFrames = 116;

    extension(NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="NorfairEnvironmentalPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => NorfairEnvironmentalPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}

/// <summary>Cartridge values of <see cref="NorfairRioInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class NorfairRioInstructionProgramDefinitionsConstants
{
    /// <summary><c>GerutaConstants</c>, adjacent non-instruction data at $A2:C1B7.</summary>
    public const ushort AdjacentMovementDefinitions = 0xc1b7;

    extension(NorfairRioInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="NorfairRioInstructionProgramDefinitionsConstants.AdjacentMovementDefinitions"/>
        internal static ushort AdjacentMovementDefinitions => NorfairRioInstructionProgramDefinitionsConstants.AdjacentMovementDefinitions;
    }
}

/// <summary>Cartridge values of <see cref="NuclearWaffleProjectileInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class NuclearWaffleProjectileInstructionProgramDefinitionsConstants
{
    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the body loop at $86:BB8E.
    /// </summary>
    public const ushort LoopCommand = 0xbb8e;

    extension(NuclearWaffleProjectileInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="NuclearWaffleProjectileInstructionProgramDefinitionsConstants.LoopCommand"/>
        internal static ushort LoopCommand => NuclearWaffleProjectileInstructionProgramDefinitionsConstants.LoopCommand;
    }
}

/// <summary>Cartridge values of <see cref="OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Both complete loops last 64 frames.</summary>
    public const int CycleFrames = 64;

    extension(OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}

/// <summary>Cartridge values of <see cref="OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete loop lasts 42 frames.</summary>
    public const int CycleFrames = 42;
    /// <summary><c>PalFxDef_Crateria8</c> at <c>$8D:FFD9</c>.</summary>
    public const ushort DefinitionPointer = 0xffd9;

    extension(OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="OwtchMovementDefinitions"/> that only verification reads.</summary>
internal static class OwtchMovementDefinitionsConstants
{
    /// <summary>$A2:A3DD OwtchConstants_XDistanceRanges, eight unsigned patrol half-widths.</summary>
    public const int DistanceReferenceAddress = 0xa2a3dd;
    /// <summary>$A2:A3ED OwtchConstants_undergroundTimers, six unsigned burial durations.</summary>
    public const int TimerReferenceAddress = 0xa2a3ed;

    extension(OwtchMovementDefinitions)
    {
        /// <inheritdoc cref="OwtchMovementDefinitionsConstants.DistanceReferenceAddress"/>
        internal static int DistanceReferenceAddress => OwtchMovementDefinitionsConstants.DistanceReferenceAddress;
        /// <inheritdoc cref="OwtchMovementDefinitionsConstants.TimerReferenceAddress"/>
        internal static int TimerReferenceAddress => OwtchMovementDefinitionsConstants.TimerReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="PaletteFxHeatInstructionListDefinitions"/> that only verification reads.</summary>
internal static class PaletteFxHeatInstructionListDefinitionsConstants
{
    /// <summary>
    /// <c>PreInstruction_PaletteFXObject_SamusInHeat.InstListPointers.gravity</c> at
    /// <c>$8D:E3E0</c>.
    /// </summary>
    public const ushort GravitySourceTable = 0xe3e0;
    /// <summary>
    /// <c>PreInstruction_PaletteFXObject_SamusInHeat.InstListPointers.power</c> at
    /// <c>$8D:E420</c>.
    /// </summary>
    public const ushort PowerSourceTable = 0xe420;
    /// <summary>
    /// <c>PreInstruction_PaletteFXObject_SamusInHeat.InstListPointers.varia</c> at
    /// <c>$8D:E400</c>.
    /// </summary>
    public const ushort VariaSourceTable = 0xe400;

    extension(PaletteFxHeatInstructionListDefinitions)
    {
        /// <inheritdoc cref="PaletteFxHeatInstructionListDefinitionsConstants.GravitySourceTable"/>
        internal static ushort GravitySourceTable => PaletteFxHeatInstructionListDefinitionsConstants.GravitySourceTable;
        /// <inheritdoc cref="PaletteFxHeatInstructionListDefinitionsConstants.PowerSourceTable"/>
        internal static ushort PowerSourceTable => PaletteFxHeatInstructionListDefinitionsConstants.PowerSourceTable;
        /// <inheritdoc cref="PaletteFxHeatInstructionListDefinitionsConstants.VariaSourceTable"/>
        internal static ushort VariaSourceTable => PaletteFxHeatInstructionListDefinitionsConstants.VariaSourceTable;
    }
}

/// <summary>Cartridge values of <see cref="PauseMenuLayout"/> that only verification reads.</summary>
internal static class PauseMenuLayoutConstants
{
    /// <summary>Number of decimal digits written by $82:8F70.</summary>
    public const int ReserveSupplyDigitCount = 3;
    /// <summary>
    /// Tilemap word for decimal zero used by $82:8F70; decimal digit values are added to
    /// this word without altering its palette or priority fields.
    /// </summary>
    public const ushort ReserveSupplyDigitZeroTile = 0x0804;
    /// <summary>
    /// Byte offset of the reserve-supply hundreds digit in the mutable equipment tilemap,
    /// matching <c>EquipmentScreenBG1Tilemap+$310</c> at $82:8FCE.
    /// </summary>
    public const int ReserveSupplyDigitsByteOffset = 0x0310;

    extension(PauseMenuLayout)
    {
        /// <inheritdoc cref="PauseMenuLayoutConstants.ReserveSupplyDigitCount"/>
        internal static int ReserveSupplyDigitCount => PauseMenuLayoutConstants.ReserveSupplyDigitCount;
        /// <inheritdoc cref="PauseMenuLayoutConstants.ReserveSupplyDigitZeroTile"/>
        internal static ushort ReserveSupplyDigitZeroTile => PauseMenuLayoutConstants.ReserveSupplyDigitZeroTile;
        /// <inheritdoc cref="PauseMenuLayoutConstants.ReserveSupplyDigitsByteOffset"/>
        internal static int ReserveSupplyDigitsByteOffset => PauseMenuLayoutConstants.ReserveSupplyDigitsByteOffset;
    }
}

/// <summary>Cartridge values of <see cref="PauseMenuRomData"/> that only verification reads.</summary>
internal static class PauseMenuRomDataConstants
{
    /// <summary>Equipment-set lookup table at $82:B257.</summary>
    public const int EquipmentSetTable = 0x82b257;

    extension(PauseMenuRomData)
    {
        /// <inheritdoc cref="PauseMenuRomDataConstants.EquipmentSetTable"/>
        internal static int EquipmentSetTable => PauseMenuRomDataConstants.EquipmentSetTable;
    }
}

/// <summary>Cartridge values of <see cref="PauseReserveTransferRomData"/> that only verification reads.</summary>
internal static class PauseReserveTransferRomDataConstants
{
    /// <summary>$82:BF04, ReserveTank_TransferEnergyPerFrame, consumed as a ROM word.</summary>
    public const int TransferAmount = 0x82bf04;

    extension(PauseReserveTransferRomData)
    {
        /// <inheritdoc cref="PauseReserveTransferRomDataConstants.TransferAmount"/>
        internal static int TransferAmount => PauseReserveTransferRomDataConstants.TransferAmount;
    }
}

/// <summary>Cartridge values of <see cref="PhantoonBg2FrameDefinitions"/> that only verification reads.</summary>
internal static class PhantoonBg2FrameDefinitionsConstants
{
    public const ushort VramBase = EnemyBg2FrameLayout.VramBase;
    public const ushort WorkingRamBase = EnemyBg2FrameLayout.WorkingRamBase;

    extension(PhantoonBg2FrameDefinitions)
    {
        /// <inheritdoc cref="PhantoonBg2FrameDefinitionsConstants.VramBase"/>
        internal static ushort VramBase => PhantoonBg2FrameDefinitionsConstants.VramBase;
        /// <inheritdoc cref="PhantoonBg2FrameDefinitionsConstants.WorkingRamBase"/>
        internal static ushort WorkingRamBase => PhantoonBg2FrameDefinitionsConstants.WorkingRamBase;
    }
}

/// <summary>Cartridge values of <see cref="PhantoonColorRomData"/> that only verification reads.</summary>
internal static class PhantoonColorRomDataConstants
{
    public const int PowerOnDestination = 0;

    extension(PhantoonColorRomData)
    {
        /// <inheritdoc cref="PhantoonColorRomDataConstants.PowerOnDestination"/>
        internal static int PowerOnDestination => PhantoonColorRomDataConstants.PowerOnDestination;
    }
}

/// <summary>Cartridge values of <see cref="PhantoonInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class PhantoonInstructionProgramDefinitionsConstants
{
    /// <summary>First casual-flame timer word following the instruction block at $A7:CCFD.</summary>
    public const ushort AdjacentCasualFlameTimers = 0xccfd;

    extension(PhantoonInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="PhantoonInstructionProgramDefinitionsConstants.AdjacentCasualFlameTimers"/>
        internal static ushort AdjacentCasualFlameTimers => PhantoonInstructionProgramDefinitionsConstants.AdjacentCasualFlameTimers;
    }
}

/// <summary>Cartridge values of <see cref="PlanetZebesTextPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class PlanetZebesTextPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Each complete one-shot fade lasts 24 frames.</summary>
    public const int CycleFrames = PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount * PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameDuration;
    /// <summary>$8D:E1B0: native fade-in definition, immediately followed by the four-byte fade-out definition.</summary>
    public const ushort FadeInDefinition = 0xE1B0;

    extension(PlanetZebesTextPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="PlanetZebesTextPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => PlanetZebesTextPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="PlanetZebesTextPaletteFxProgramMechanicsDefinitionsConstants.FadeInDefinition"/>
        internal static ushort FadeInDefinition => PlanetZebesTextPaletteFxProgramMechanicsDefinitionsConstants.FadeInDefinition;
    }
}

/// <summary>Cartridge values of <see cref="PlatformInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class PlatformInstructionProgramDefinitionsConstants
{
    /// <summary>The first callback implementation immediately after the programs.</summary>
    public const ushort FirstAdjacentCallback = 0x9c6b;

    extension(PlatformInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="PlatformInstructionProgramDefinitionsConstants.FirstAdjacentCallback"/>
        internal static ushort FirstAdjacentCallback => PlatformInstructionProgramDefinitionsConstants.FirstAdjacentCallback;
    }
}

/// <summary>Cartridge values of <see cref="PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class PostCreditsIconGlarePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete icon glare lasts fourteen frames.</summary>
    public const int CycleFrames = PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameCount * PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameDuration;
    /// <summary>The palette-FX definition at <c>$8D:E200</c>.</summary>
    public const ushort DefinitionPointer = 0xe200;

    extension(PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="PostCreditsIconGlarePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => PostCreditsIconGlarePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="PostCreditsIconGlarePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => PostCreditsIconGlarePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="PowampInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class PowampInstructionProgramDefinitionsConstants
{
    /// <summary>The terminal common sleep word at $A8:C19D.</summary>
    public const ushort BalloonDeflatedSleep = 0xc19d;
    /// <summary><c>InstList_Powamp_Balloon_Inflate_2</c> at $A8:C18B.</summary>
    public const ushort BalloonInflate2 = 0xc18b;
    /// <summary>The first non-program word after Powamp's instruction streams, at $A8:C19F.</summary>
    public const ushort FirstAdjacentConstant = 0xc19f;

    extension(PowampInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="PowampInstructionProgramDefinitionsConstants.BalloonDeflatedSleep"/>
        internal static ushort BalloonDeflatedSleep => PowampInstructionProgramDefinitionsConstants.BalloonDeflatedSleep;
        /// <inheritdoc cref="PowampInstructionProgramDefinitionsConstants.BalloonInflate2"/>
        internal static ushort BalloonInflate2 => PowampInstructionProgramDefinitionsConstants.BalloonInflate2;
        /// <inheritdoc cref="PowampInstructionProgramDefinitionsConstants.FirstAdjacentConstant"/>
        internal static ushort FirstAdjacentConstant => PowampInstructionProgramDefinitionsConstants.FirstAdjacentConstant;
    }
}

/// <summary>Cartridge values of <see cref="PuyoInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class PuyoInstructionProgramDefinitionsConstants
{
    /// <summary>The first word of <c>PuyoHopTable</c>, immediately after the programs at $A2:9A07.</summary>
    public const ushort FirstAdjacentDefinition = 0x9a07;
    /// <summary>The final airborne-pose sleep opcode at $A2:9A05.</summary>
    public const ushort LastSleepOpcode = 0x9a05;

    extension(PuyoInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="PuyoInstructionProgramDefinitionsConstants.FirstAdjacentDefinition"/>
        internal static ushort FirstAdjacentDefinition => PuyoInstructionProgramDefinitionsConstants.FirstAdjacentDefinition;
        /// <inheritdoc cref="PuyoInstructionProgramDefinitionsConstants.LastSleepOpcode"/>
        internal static ushort LastSleepOpcode => PuyoInstructionProgramDefinitionsConstants.LastSleepOpcode;
    }
}

/// <summary>Cartridge values of <see cref="RedBrinstarGlowPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class RedBrinstarGlowPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 140;
    /// <summary>Red Brinstar background-glow palette-FX definition at $8D:F77D.</summary>
    public const ushort DefinitionPointer = 0xf77d;

    extension(RedBrinstarGlowPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="RedBrinstarGlowPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => RedBrinstarGlowPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="RedBrinstarGlowPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => RedBrinstarGlowPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="ResidentDoorClosingDefinitions"/> that only verification reads.</summary>
internal static class ResidentDoorClosingDefinitionsConstants
{
    /// <summary>Number of retail resident door/gate headers with a secondary closing list.</summary>
    public const int Count = 20;

    extension(ResidentDoorClosingDefinitions)
    {
        /// <inheritdoc cref="ResidentDoorClosingDefinitionsConstants.Count"/>
        internal static int Count => ResidentDoorClosingDefinitionsConstants.Count;
    }
}

/// <summary>Cartridge values of <see cref="RioInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class RioInstructionProgramDefinitionsConstants
{
    /// <summary>The first mechanics constant after Rio's programs, at $A2:BBBB.</summary>
    public const ushort FirstAdjacentMechanicsData = 0xbbbb;

    extension(RioInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="RioInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData"/>
        internal static ushort FirstAdjacentMechanicsData => RioInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData;
    }
}

/// <summary>Cartridge values of <see cref="RoomAssetRomData.LibraryBackground"/> that only verification reads.</summary>
internal static class RoomAssetRomDataLibraryBackgroundConstants
{
    /// <summary>Bank containing room-authored library-background command lists.</summary>
    public const int CommandBank = 0x8f0000;

    extension(RoomAssetRomData.LibraryBackground)
    {
        /// <inheritdoc cref="RoomAssetRomDataLibraryBackgroundConstants.CommandBank"/>
        internal static int CommandBank => RoomAssetRomDataLibraryBackgroundConstants.CommandBank;
    }
}

/// <summary>Cartridge values of <see cref="RoomAssetRomData.LibraryBackground.TourianStatueGhost"/> that only verification reads.</summary>
internal static class RoomAssetRomDataLibraryBackgroundTourianStatueGhostConstants
{
    /// <summary>Native destination VRAM word for the ghost character upload.</summary>
    public const ushort VramDestinationWord = 0x6d00;

    extension(RoomAssetRomData.LibraryBackground.TourianStatueGhost)
    {
        /// <inheritdoc cref="RoomAssetRomDataLibraryBackgroundTourianStatueGhostConstants.VramDestinationWord"/>
        internal static ushort VramDestinationWord => RoomAssetRomDataLibraryBackgroundTourianStatueGhostConstants.VramDestinationWord;
    }
}

/// <summary>Cartridge values of <see cref="RoomAssetRomData.Tilesets"/> that only verification reads.</summary>
internal static class RoomAssetRomDataTilesetsConstants
{
    /// <summary>Offset of the compressed 16x16 block-definition address.</summary>
    public const int BlockDefinitionsAddressOffset = 0;
    /// <summary>Offset of the compressed 4bpp character address.</summary>
    public const int CharacterAddressOffset = 3;
    /// <summary>Bank containing both <see cref="PointerTableAddress"/> and its records.</summary>
    public const int DefinitionBank = 0x8f0000;
    /// <summary>Offset of the compressed palette address.</summary>
    public const int PaletteAddressOffset = 6;
    /// <summary><c>$8F:E7A7 tileset_table</c>, a table of bank-$8F word pointers.</summary>
    public const int PointerTableAddress = 0x8fe7a7;

    extension(RoomAssetRomData.Tilesets)
    {
        /// <inheritdoc cref="RoomAssetRomDataTilesetsConstants.BlockDefinitionsAddressOffset"/>
        internal static int BlockDefinitionsAddressOffset => RoomAssetRomDataTilesetsConstants.BlockDefinitionsAddressOffset;
        /// <inheritdoc cref="RoomAssetRomDataTilesetsConstants.CharacterAddressOffset"/>
        internal static int CharacterAddressOffset => RoomAssetRomDataTilesetsConstants.CharacterAddressOffset;
        /// <inheritdoc cref="RoomAssetRomDataTilesetsConstants.DefinitionBank"/>
        internal static int DefinitionBank => RoomAssetRomDataTilesetsConstants.DefinitionBank;
        /// <inheritdoc cref="RoomAssetRomDataTilesetsConstants.PaletteAddressOffset"/>
        internal static int PaletteAddressOffset => RoomAssetRomDataTilesetsConstants.PaletteAddressOffset;
        /// <inheritdoc cref="RoomAssetRomDataTilesetsConstants.PointerTableAddress"/>
        internal static int PointerTableAddress => RoomAssetRomDataTilesetsConstants.PointerTableAddress;
    }
}

/// <summary>Cartridge values of <see cref="RoomBlockBehaviorValues"/> that only verification reads.</summary>
internal static class RoomBlockBehaviorValuesConstants
{
    /// <summary>Down-facing blue-door shootable-cap dispatcher.</summary>
    public static readonly RoomBlockBehavior BlueDoorFacingDown = new(0x43);
    /// <summary>Up-facing blue-door shootable-cap dispatcher.</summary>
    public static readonly RoomBlockBehavior BlueDoorFacingUp = new(0x42);

    extension(RoomBlockBehaviorValues)
    {
        /// <inheritdoc cref="RoomBlockBehaviorValuesConstants.BlueDoorFacingDown"/>
        internal static RoomBlockBehavior BlueDoorFacingDown => RoomBlockBehaviorValuesConstants.BlueDoorFacingDown;
        /// <inheritdoc cref="RoomBlockBehaviorValuesConstants.BlueDoorFacingUp"/>
        internal static RoomBlockBehavior BlueDoorFacingUp => RoomBlockBehaviorValuesConstants.BlueDoorFacingUp;
    }
}

/// <summary>Cartridge values of <see cref="RoomEnemyRomLayout"/> that only verification reads.</summary>
internal static class RoomEnemyRomLayoutConstants
{
    /// <summary>Bank containing room enemy population records.</summary>
    public const int PopulationBank = 0xa10000;

    extension(RoomEnemyRomLayout)
    {
        /// <inheritdoc cref="RoomEnemyRomLayoutConstants.PopulationBank"/>
        internal static int PopulationBank => RoomEnemyRomLayoutConstants.PopulationBank;
    }
}

/// <summary>Cartridge values of <see cref="RoomEnemySystem"/> that only verification reads.</summary>
internal static class RoomEnemySystemConstants
{
    public const int BlueBrinstarFaceBlockPaletteTable = 0xa8e7cc;
    public const ushort BombTorizoInitialInstruction = 0xb879;
    public const ushort CacatacSpikePreInstruction = EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_CacatacSpike;
    public const ushort GrowingShutterNoOpAi = EnemyAiCodePointers.BankA0.NoOp;
    public const ushort KiHunterAcidInitialLeftPreInstruction = EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Left;
    public const ushort KiHunterAcidInitialRightPreInstruction = EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Right;
    public const ushort NinjaPiratePaletteNormal = 0x0200;
    public const ushort NinjaPirateSoundClawKickOrDive = 0x0066;
    public const ushort PhantoonEyeHitboxBodyInstruction = PhantoonInstructionProgramDefinitions.EyeHitboxBody;
    public const ushort StokeProjectilePreInstruction = EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_StokeFireball;

    extension(RoomEnemySystem)
    {
        /// <inheritdoc cref="RoomEnemySystemConstants.BlueBrinstarFaceBlockPaletteTable"/>
        internal static int BlueBrinstarFaceBlockPaletteTable => RoomEnemySystemConstants.BlueBrinstarFaceBlockPaletteTable;
        /// <inheritdoc cref="RoomEnemySystemConstants.BombTorizoInitialInstruction"/>
        internal static ushort BombTorizoInitialInstruction => RoomEnemySystemConstants.BombTorizoInitialInstruction;
        /// <inheritdoc cref="RoomEnemySystemConstants.CacatacSpikePreInstruction"/>
        internal static ushort CacatacSpikePreInstruction => RoomEnemySystemConstants.CacatacSpikePreInstruction;
        /// <inheritdoc cref="RoomEnemySystemConstants.GrowingShutterNoOpAi"/>
        internal static ushort GrowingShutterNoOpAi => RoomEnemySystemConstants.GrowingShutterNoOpAi;
        /// <inheritdoc cref="RoomEnemySystemConstants.KiHunterAcidInitialLeftPreInstruction"/>
        internal static ushort KiHunterAcidInitialLeftPreInstruction => RoomEnemySystemConstants.KiHunterAcidInitialLeftPreInstruction;
        /// <inheritdoc cref="RoomEnemySystemConstants.KiHunterAcidInitialRightPreInstruction"/>
        internal static ushort KiHunterAcidInitialRightPreInstruction => RoomEnemySystemConstants.KiHunterAcidInitialRightPreInstruction;
        /// <inheritdoc cref="RoomEnemySystemConstants.NinjaPiratePaletteNormal"/>
        internal static ushort NinjaPiratePaletteNormal => RoomEnemySystemConstants.NinjaPiratePaletteNormal;
        /// <inheritdoc cref="RoomEnemySystemConstants.NinjaPirateSoundClawKickOrDive"/>
        internal static ushort NinjaPirateSoundClawKickOrDive => RoomEnemySystemConstants.NinjaPirateSoundClawKickOrDive;
        /// <inheritdoc cref="RoomEnemySystemConstants.PhantoonEyeHitboxBodyInstruction"/>
        internal static ushort PhantoonEyeHitboxBodyInstruction => RoomEnemySystemConstants.PhantoonEyeHitboxBodyInstruction;
        /// <inheritdoc cref="RoomEnemySystemConstants.StokeProjectilePreInstruction"/>
        internal static ushort StokeProjectilePreInstruction => RoomEnemySystemConstants.StokeProjectilePreInstruction;
    }
}

/// <summary>Cartridge values of <see cref="RoomFxRomData.Earthquake"/> that only verification reads.</summary>
internal static class RoomFxRomDataEarthquakeConstants
{
    public const int BgDisplacementTableAddress = 0xa0872d;
    public const int BytesPerType = 8;

    extension(RoomFxRomData.Earthquake)
    {
        /// <inheritdoc cref="RoomFxRomDataEarthquakeConstants.BgDisplacementTableAddress"/>
        internal static int BgDisplacementTableAddress => RoomFxRomDataEarthquakeConstants.BgDisplacementTableAddress;
        /// <inheritdoc cref="RoomFxRomDataEarthquakeConstants.BytesPerType"/>
        internal static int BytesPerType => RoomFxRomDataEarthquakeConstants.BytesPerType;
    }
}

/// <summary>Cartridge values of <see cref="RoomFxRomData.Layer3AnimatedTiles"/> that only verification reads.</summary>
internal static class RoomFxRomDataLayer3AnimatedTilesConstants
{
    /// <summary>First acid character frame at $87:A6A4.</summary>
    public const ushort AcidFirstFrame = 0xa6a4;
    /// <summary>First lava character frame at $87:A564.</summary>
    public const ushort LavaFirstFrame = 0xa564;
    /// <summary>First rain character frame at $87:A874.</summary>
    public const ushort RainFirstFrame = 0xa874;

    extension(RoomFxRomData.Layer3AnimatedTiles)
    {
        /// <inheritdoc cref="RoomFxRomDataLayer3AnimatedTilesConstants.AcidFirstFrame"/>
        internal static ushort AcidFirstFrame => RoomFxRomDataLayer3AnimatedTilesConstants.AcidFirstFrame;
        /// <inheritdoc cref="RoomFxRomDataLayer3AnimatedTilesConstants.LavaFirstFrame"/>
        internal static ushort LavaFirstFrame => RoomFxRomDataLayer3AnimatedTilesConstants.LavaFirstFrame;
        /// <inheritdoc cref="RoomFxRomDataLayer3AnimatedTilesConstants.RainFirstFrame"/>
        internal static ushort RainFirstFrame => RoomFxRomDataLayer3AnimatedTilesConstants.RainFirstFrame;
    }
}

/// <summary>Cartridge values of <see cref="RoomFxRomData.Tables"/> that only verification reads.</summary>
internal static class RoomFxRomDataTablesConstants
{
    public const int AreaPaletteFxObjectListPointers = 0x83ac46;

    extension(RoomFxRomData.Tables)
    {
        /// <inheritdoc cref="RoomFxRomDataTablesConstants.AreaPaletteFxObjectListPointers"/>
        internal static int AreaPaletteFxObjectListPointers => RoomFxRomDataTablesConstants.AreaPaletteFxObjectListPointers;
    }
}

/// <summary>Cartridge values of <see cref="RoomHeaderPointers"/> that only verification reads.</summary>
internal static class RoomHeaderPointersConstants
{
    /// <summary>Blue Brinstar elevator room at $8F:97B5.</summary>
    public const ushort BlueBrinstarElevatorRoom = 0x97b5;
    /// <summary>RoomHeader_Botwoon at $8F:D95E; both room states share the grey-door population.</summary>
    public const ushort Botwoon = 0xd95e;
    /// <summary>RoomHeader_Hellway at $8F:A2F7, Brinstar room $23.</summary>
    public const ushort Hellway = 0xa2f7;
    /// <summary>RoomHeader_BotwoonHallway at $8F:D617; contains the Mochtroid pipe-clip setup.</summary>
    public const ushort BotwoonHallway = 0xd617;
    /// <summary>Bank-$8F:9CB3, Brinstar room $08; includes the Dachora Speed Booster floor and shaft.</summary>
    public const ushort BrinstarRoom08 = 0x9cb3;
    /// <summary>RoomHeader_Caterpillar at $8F:A322, Brinstar room $24.</summary>
    public const ushort Caterpillar = 0xa322;
    /// <summary>Crab Maze at $8F:957D; its ordinary Scisers cycle all four surface poses.</summary>
    public const ushort CrabMaze = 0x957d;
    /// <summary>Crateria room $1E at $8F:9A44; contains the Blue Brinstar face block.</summary>
    public const ushort CrateriaFaceBlockRoom = 0x9a44;
    /// <summary>Crateria room $1A at $8F:9969; its Kago population uses the shared three-frame cycle.</summary>
    public const ushort CrateriaKagoRoom = 0x9969;
    /// <summary>Crateria room $1F at $8F:9A90; contains the Morph Ball surveillance eye.</summary>
    public const ushort CrateriaMorphBallEyeRoom = 0x9a90;
    /// <summary>RoomHeader_Crocomire at $8F:A98D; includes the west Power Bomb route door.</summary>
    public const ushort Crocomire = 0xa98d;
    /// <summary>Dead-Torizo corpse room at $8F:DC65 in Tourian.</summary>
    public const ushort DeadTorizoCorpse = 0xdc65;
    /// <summary>RoomHeader_Draygon at $8F:DA60; the four-record Maridia boss encounter.</summary>
    public const ushort Draygon = 0xda60;
    /// <summary>RoomHeader_EastTunnel at $8F:CF80; Maridia room $03 with the frozen-enemy gate setup.</summary>
    public const ushort EastTunnel = 0xcf80;
    /// <summary>RoomHeader_FrogSpeedway at $8F:B106; Norfair room $30, eight screens wide.</summary>
    public const ushort FrogSpeedway = 0xb106;
    /// <summary>Gauntlet east at $8F:92B3; contains the Crateria Yapping Maws.</summary>
    public const ushort GauntletEast = 0x92b3;
    /// <summary>RoomHeader_GoldenTorizo at $8F:B283; lower-right Samus position triggers statue awakening.</summary>
    public const ushort GoldenTorizo = 0xb283;
    /// <summary>Green Brinstar main shaft at $8F:9AD9 (area $01, room $00).</summary>
    public const ushort GreenBrinstarMainShaft = 0x9ad9;
    /// <summary>RoomHeader_GreenHillZone at $8F:9E52; contains the blue-left gate used by Grapple and Speed Booster gate glitches.</summary>
    public const ushort GreenHillZone = 0x9e52;
    /// <summary>RoomHeader_Kraid at $8F:A59F; its live state relocates BG3 HUD characters to VRAM word $2000.</summary>
    public const ushort Kraid = 0xa59f;
    /// <summary>RoomHeader_KronicBoost at $8F:AE74; Norfair room $22 with the reported blue gate.</summary>
    public const ushort KronicBoost = 0xae74;
    /// <summary>RoomHeader_MainHall at $8F:B236, Lower Norfair's elevator destination.</summary>
    public const ushort LowerNorfairMainHall = 0xb236;
    /// <summary>Mother Brain's chamber at $8F:DD58; its live state loads population $A1:E321.</summary>
    public const ushort MotherBrainChamber = 0xdd58;
    /// <summary>Retail Norfair room $1E at $8F:ADAD; its lower floor includes half-height square slopes.</summary>
    public const ushort NorfairRoom1E = 0xadad;
    /// <summary>RoomHeader_Phantoon at $8F:CD13; the Wrecked Ship boss encounter.</summary>
    public const ushort Phantoon = 0xcd13;
    /// <summary>RoomHeader_PinkBrinstarHopper at $8F:A130; the high-speed right-facing gate-glitch room.</summary>
    public const ushort PinkBrinstarHopper = 0xa130;
    /// <summary>Pre-moat room at $8F:948C; contains the Crateria KiHunter pair.</summary>
    public const ushort PreMoat = 0x948c;
    /// <summary>RoomHeader_RedTower at $8F:A253, Brinstar room $20.</summary>
    public const ushort RedTower = 0xa253;
    /// <summary>Tourian room $01 at $8F:DAE1; its initial state has four ordinary Metroids.</summary>
    public const ushort TourianMetroidRoom = 0xdae1;
    /// <summary>West Ocean at $8F:93FE (area $00, room $05), using the ocean sky main routine.</summary>
    public const ushort WestOcean = 0x93fe;

    extension(RoomHeaderPointers)
    {
        /// <inheritdoc cref="RoomHeaderPointersConstants.BlueBrinstarElevatorRoom"/>
        internal static ushort BlueBrinstarElevatorRoom => RoomHeaderPointersConstants.BlueBrinstarElevatorRoom;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Botwoon"/>
        internal static ushort Botwoon => RoomHeaderPointersConstants.Botwoon;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Hellway"/>
        internal static ushort Hellway => RoomHeaderPointersConstants.Hellway;
        /// <inheritdoc cref="RoomHeaderPointersConstants.BotwoonHallway"/>
        internal static ushort BotwoonHallway => RoomHeaderPointersConstants.BotwoonHallway;
        /// <inheritdoc cref="RoomHeaderPointersConstants.BrinstarRoom08"/>
        internal static ushort BrinstarRoom08 => RoomHeaderPointersConstants.BrinstarRoom08;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Caterpillar"/>
        internal static ushort Caterpillar => RoomHeaderPointersConstants.Caterpillar;
        /// <inheritdoc cref="RoomHeaderPointersConstants.CrabMaze"/>
        internal static ushort CrabMaze => RoomHeaderPointersConstants.CrabMaze;
        /// <inheritdoc cref="RoomHeaderPointersConstants.CrateriaFaceBlockRoom"/>
        internal static ushort CrateriaFaceBlockRoom => RoomHeaderPointersConstants.CrateriaFaceBlockRoom;
        /// <inheritdoc cref="RoomHeaderPointersConstants.CrateriaKagoRoom"/>
        internal static ushort CrateriaKagoRoom => RoomHeaderPointersConstants.CrateriaKagoRoom;
        /// <inheritdoc cref="RoomHeaderPointersConstants.CrateriaMorphBallEyeRoom"/>
        internal static ushort CrateriaMorphBallEyeRoom => RoomHeaderPointersConstants.CrateriaMorphBallEyeRoom;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Crocomire"/>
        internal static ushort Crocomire => RoomHeaderPointersConstants.Crocomire;
        /// <inheritdoc cref="RoomHeaderPointersConstants.DeadTorizoCorpse"/>
        internal static ushort DeadTorizoCorpse => RoomHeaderPointersConstants.DeadTorizoCorpse;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Draygon"/>
        internal static ushort Draygon => RoomHeaderPointersConstants.Draygon;
        /// <inheritdoc cref="RoomHeaderPointersConstants.EastTunnel"/>
        internal static ushort EastTunnel => RoomHeaderPointersConstants.EastTunnel;
        /// <inheritdoc cref="RoomHeaderPointersConstants.FrogSpeedway"/>
        internal static ushort FrogSpeedway => RoomHeaderPointersConstants.FrogSpeedway;
        /// <inheritdoc cref="RoomHeaderPointersConstants.GauntletEast"/>
        internal static ushort GauntletEast => RoomHeaderPointersConstants.GauntletEast;
        /// <inheritdoc cref="RoomHeaderPointersConstants.GoldenTorizo"/>
        internal static ushort GoldenTorizo => RoomHeaderPointersConstants.GoldenTorizo;
        /// <inheritdoc cref="RoomHeaderPointersConstants.GreenBrinstarMainShaft"/>
        internal static ushort GreenBrinstarMainShaft => RoomHeaderPointersConstants.GreenBrinstarMainShaft;
        /// <inheritdoc cref="RoomHeaderPointersConstants.GreenHillZone"/>
        internal static ushort GreenHillZone => RoomHeaderPointersConstants.GreenHillZone;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Kraid"/>
        internal static ushort Kraid => RoomHeaderPointersConstants.Kraid;
        /// <inheritdoc cref="RoomHeaderPointersConstants.KronicBoost"/>
        internal static ushort KronicBoost => RoomHeaderPointersConstants.KronicBoost;
        /// <inheritdoc cref="RoomHeaderPointersConstants.LowerNorfairMainHall"/>
        internal static ushort LowerNorfairMainHall => RoomHeaderPointersConstants.LowerNorfairMainHall;
        /// <inheritdoc cref="RoomHeaderPointersConstants.MotherBrainChamber"/>
        internal static ushort MotherBrainChamber => RoomHeaderPointersConstants.MotherBrainChamber;
        /// <inheritdoc cref="RoomHeaderPointersConstants.NorfairRoom1E"/>
        internal static ushort NorfairRoom1E => RoomHeaderPointersConstants.NorfairRoom1E;
        /// <inheritdoc cref="RoomHeaderPointersConstants.Phantoon"/>
        internal static ushort Phantoon => RoomHeaderPointersConstants.Phantoon;
        /// <inheritdoc cref="RoomHeaderPointersConstants.PinkBrinstarHopper"/>
        internal static ushort PinkBrinstarHopper => RoomHeaderPointersConstants.PinkBrinstarHopper;
        /// <inheritdoc cref="RoomHeaderPointersConstants.PreMoat"/>
        internal static ushort PreMoat => RoomHeaderPointersConstants.PreMoat;
        /// <inheritdoc cref="RoomHeaderPointersConstants.RedTower"/>
        internal static ushort RedTower => RoomHeaderPointersConstants.RedTower;
        /// <inheritdoc cref="RoomHeaderPointersConstants.TourianMetroidRoom"/>
        internal static ushort TourianMetroidRoom => RoomHeaderPointersConstants.TourianMetroidRoom;
        /// <inheritdoc cref="RoomHeaderPointersConstants.WestOcean"/>
        internal static ushort WestOcean => RoomHeaderPointersConstants.WestOcean;
    }
}

/// <summary>Cartridge values of <see cref="RoomHeaderRomData"/> that only verification reads.</summary>
internal static class RoomHeaderRomDataConstants
{
    /// <summary>SNES bank containing room headers, inline selectors, and room-state records.</summary>
    public const int BankAddress = 0x8f0000;

    extension(RoomHeaderRomData)
    {
        /// <inheritdoc cref="RoomHeaderRomDataConstants.BankAddress"/>
        internal static int BankAddress => RoomHeaderRomDataConstants.BankAddress;
    }
}

/// <summary>Cartridge values of <see cref="RoomIdentities"/> that only verification reads.</summary>
internal static class RoomIdentitiesConstants
{
    /// <summary>Crateria's Landing Site, room pair <c>$00/$00</c>.</summary>
    public static readonly RoomIdentity LandingSite = new(AreaId.Crateria, 0x00);

    extension(RoomIdentities)
    {
        /// <inheritdoc cref="RoomIdentitiesConstants.LandingSite"/>
        internal static RoomIdentity LandingSite => RoomIdentitiesConstants.LandingSite;
    }
}

/// <summary>Cartridge values of <see cref="RoomPaletteFxDefinitions"/> that only verification reads.</summary>
internal static class RoomPaletteFxDefinitionsConstants
{
    /// <summary>$8D:E194, first definition in the contiguous cinematic/Samus group.</summary>
    public const ushort CinematicDefinitionsBegin = 0xe194;
    /// <summary>$8D:E200, final definition in the contiguous cinematic/Samus group.</summary>
    public const ushort CinematicDefinitionsEnd = 0xe200;
    public const int DefinitionByteCount = 4;
    /// <summary>$8D:F745, first definition in the contiguous room-effect group.</summary>
    public const ushort RoomDefinitionsBegin = 0xf745;
    /// <summary>$8D:F7A5, final definition in the contiguous room-effect group.</summary>
    public const ushort RoomDefinitionsEnd = 0xf7a5;
    /// <summary>$8D:F761, Norfair Samus-in-heat palette-FX owner selected by area lists.</summary>
    public const ushort SamusInHeat = 0xf761;
    /// <summary>$8D:FFC9, first definition in the contiguous Tourian escape group.</summary>
    public const ushort TourianDefinitionsBegin = 0xffc9;
    /// <summary>$8D:FFED, final definition in the contiguous Tourian escape group.</summary>
    public const ushort TourianDefinitionsEnd = 0xffed;

    extension(RoomPaletteFxDefinitions)
    {
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.CinematicDefinitionsBegin"/>
        internal static ushort CinematicDefinitionsBegin => RoomPaletteFxDefinitionsConstants.CinematicDefinitionsBegin;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.CinematicDefinitionsEnd"/>
        internal static ushort CinematicDefinitionsEnd => RoomPaletteFxDefinitionsConstants.CinematicDefinitionsEnd;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.DefinitionByteCount"/>
        internal static int DefinitionByteCount => RoomPaletteFxDefinitionsConstants.DefinitionByteCount;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.RoomDefinitionsBegin"/>
        internal static ushort RoomDefinitionsBegin => RoomPaletteFxDefinitionsConstants.RoomDefinitionsBegin;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.RoomDefinitionsEnd"/>
        internal static ushort RoomDefinitionsEnd => RoomPaletteFxDefinitionsConstants.RoomDefinitionsEnd;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.SamusInHeat"/>
        internal static ushort SamusInHeat => RoomPaletteFxDefinitionsConstants.SamusInHeat;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.TourianDefinitionsBegin"/>
        internal static ushort TourianDefinitionsBegin => RoomPaletteFxDefinitionsConstants.TourianDefinitionsBegin;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.TourianDefinitionsEnd"/>
        internal static ushort TourianDefinitionsEnd => RoomPaletteFxDefinitionsConstants.TourianDefinitionsEnd;
    }
}

/// <summary>Cartridge values of <see cref="RoomPlmHeaderDefinitions"/> that only verification reads.</summary>
internal static class RoomPlmHeaderDefinitionsConstants
{
    public const int RetailHeaderCount = 70;

    extension(RoomPlmHeaderDefinitions)
    {
        /// <inheritdoc cref="RoomPlmHeaderDefinitionsConstants.RetailHeaderCount"/>
        internal static int RetailHeaderCount => RoomPlmHeaderDefinitionsConstants.RetailHeaderCount;
    }
}

/// <summary>Cartridge values of <see cref="RoomPlmHeaders"/> that only verification reads.</summary>
internal static class RoomPlmHeadersConstants
{
    public const ushort ChozoMorphBall = 0xef77;
    public const ushort EnergyStationLeftAccess = 0xb6e7;
    public const ushort EnergyStationRightAccess = 0xb6e3;
    public const ushort ExposedBombs = 0xeee7;
    public const ushort ExposedChargeBeam = 0xeeeb;
    public const ushort ExposedGrappleBeam = 0xef17;
    public const ushort ExposedGravitySuit = 0xef0b;
    public const ushort ExposedHiJumpBoots = 0xeef3;
    public const ushort ExposedIceBeam = 0xeeef;
    public const ushort ExposedPlasmaBeam = 0xef13;
    public const ushort ExposedReserveTank = 0xef27;
    public const ushort ExposedScrewAttack = 0xef1f;
    public const ushort ExposedSpaceJump = 0xef1b;
    public const ushort ExposedSpazerBeam = 0xeeff;
    public const ushort ExposedSpeedBooster = 0xeef7;
    public const ushort ExposedSpringBall = 0xef03;
    public const ushort ExposedVariaSuit = 0xef07;
    public const ushort ExposedWaveBeam = 0xeefb;
    public const ushort ExposedXrayScope = 0xef0f;
    /// <summary>Collision-side permanent-item detector at $84:EED3.</summary>
    public const ushort ItemCollisionDetection = 0xeed3;
    public const ushort MapStationLeftAccess = 0xb6db;
    public const ushort MapStationRightAccess = 0xb6d7;
    public const ushort MissileStationLeftAccess = 0xb6f3;
    public const ushort MissileStationRightAccess = 0xb6ef;
    /// <summary>Collision-side save-station trigger at $84:B76B.</summary>
    public const ushort SaveStationTrigger = 0xb76b;
    public const ushort ScrollTriggerCollision = 0xb6ff;
    public const ushort ShotBlockBombs = 0xef8f;
    public const ushort ShotBlockChargeBeam = 0xef93;
    public const ushort ShotBlockGrappleBeam = 0xefbf;
    public const ushort ShotBlockGravitySuit = 0xefb3;
    public const ushort ShotBlockHiJumpBoots = 0xef9b;
    public const ushort ShotBlockIceBeam = 0xef97;
    public const ushort ShotBlockMorphBall = 0xefcb;
    public const ushort ShotBlockPlasmaBeam = 0xefbb;
    public const ushort ShotBlockPowerBombTank = 0xef8b;
    public const ushort ShotBlockReserveTank = 0xefcf;
    public const ushort ShotBlockScrewAttack = 0xefc7;
    public const ushort ShotBlockSpaceJump = 0xefc3;
    public const ushort ShotBlockSpazerBeam = 0xefa7;
    public const ushort ShotBlockSpeedBooster = 0xef9f;
    public const ushort ShotBlockSpringBall = 0xefab;
    public const ushort ShotBlockVariaSuit = 0xefaf;
    public const ushort ShotBlockWaveBeam = 0xefa3;
    public const ushort ShotBlockXrayScope = 0xefb7;

    extension(RoomPlmHeaders)
    {
        /// <inheritdoc cref="RoomPlmHeadersConstants.ChozoMorphBall"/>
        internal static ushort ChozoMorphBall => RoomPlmHeadersConstants.ChozoMorphBall;
        /// <inheritdoc cref="RoomPlmHeadersConstants.EnergyStationLeftAccess"/>
        internal static ushort EnergyStationLeftAccess => RoomPlmHeadersConstants.EnergyStationLeftAccess;
        /// <inheritdoc cref="RoomPlmHeadersConstants.EnergyStationRightAccess"/>
        internal static ushort EnergyStationRightAccess => RoomPlmHeadersConstants.EnergyStationRightAccess;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedBombs"/>
        internal static ushort ExposedBombs => RoomPlmHeadersConstants.ExposedBombs;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedChargeBeam"/>
        internal static ushort ExposedChargeBeam => RoomPlmHeadersConstants.ExposedChargeBeam;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedGrappleBeam"/>
        internal static ushort ExposedGrappleBeam => RoomPlmHeadersConstants.ExposedGrappleBeam;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedGravitySuit"/>
        internal static ushort ExposedGravitySuit => RoomPlmHeadersConstants.ExposedGravitySuit;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedHiJumpBoots"/>
        internal static ushort ExposedHiJumpBoots => RoomPlmHeadersConstants.ExposedHiJumpBoots;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedIceBeam"/>
        internal static ushort ExposedIceBeam => RoomPlmHeadersConstants.ExposedIceBeam;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedPlasmaBeam"/>
        internal static ushort ExposedPlasmaBeam => RoomPlmHeadersConstants.ExposedPlasmaBeam;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedReserveTank"/>
        internal static ushort ExposedReserveTank => RoomPlmHeadersConstants.ExposedReserveTank;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedScrewAttack"/>
        internal static ushort ExposedScrewAttack => RoomPlmHeadersConstants.ExposedScrewAttack;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedSpaceJump"/>
        internal static ushort ExposedSpaceJump => RoomPlmHeadersConstants.ExposedSpaceJump;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedSpazerBeam"/>
        internal static ushort ExposedSpazerBeam => RoomPlmHeadersConstants.ExposedSpazerBeam;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedSpeedBooster"/>
        internal static ushort ExposedSpeedBooster => RoomPlmHeadersConstants.ExposedSpeedBooster;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedSpringBall"/>
        internal static ushort ExposedSpringBall => RoomPlmHeadersConstants.ExposedSpringBall;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedVariaSuit"/>
        internal static ushort ExposedVariaSuit => RoomPlmHeadersConstants.ExposedVariaSuit;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedWaveBeam"/>
        internal static ushort ExposedWaveBeam => RoomPlmHeadersConstants.ExposedWaveBeam;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ExposedXrayScope"/>
        internal static ushort ExposedXrayScope => RoomPlmHeadersConstants.ExposedXrayScope;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ItemCollisionDetection"/>
        internal static ushort ItemCollisionDetection => RoomPlmHeadersConstants.ItemCollisionDetection;
        /// <inheritdoc cref="RoomPlmHeadersConstants.MapStationLeftAccess"/>
        internal static ushort MapStationLeftAccess => RoomPlmHeadersConstants.MapStationLeftAccess;
        /// <inheritdoc cref="RoomPlmHeadersConstants.MapStationRightAccess"/>
        internal static ushort MapStationRightAccess => RoomPlmHeadersConstants.MapStationRightAccess;
        /// <inheritdoc cref="RoomPlmHeadersConstants.MissileStationLeftAccess"/>
        internal static ushort MissileStationLeftAccess => RoomPlmHeadersConstants.MissileStationLeftAccess;
        /// <inheritdoc cref="RoomPlmHeadersConstants.MissileStationRightAccess"/>
        internal static ushort MissileStationRightAccess => RoomPlmHeadersConstants.MissileStationRightAccess;
        /// <inheritdoc cref="RoomPlmHeadersConstants.SaveStationTrigger"/>
        internal static ushort SaveStationTrigger => RoomPlmHeadersConstants.SaveStationTrigger;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ScrollTriggerCollision"/>
        internal static ushort ScrollTriggerCollision => RoomPlmHeadersConstants.ScrollTriggerCollision;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockBombs"/>
        internal static ushort ShotBlockBombs => RoomPlmHeadersConstants.ShotBlockBombs;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockChargeBeam"/>
        internal static ushort ShotBlockChargeBeam => RoomPlmHeadersConstants.ShotBlockChargeBeam;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockGrappleBeam"/>
        internal static ushort ShotBlockGrappleBeam => RoomPlmHeadersConstants.ShotBlockGrappleBeam;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockGravitySuit"/>
        internal static ushort ShotBlockGravitySuit => RoomPlmHeadersConstants.ShotBlockGravitySuit;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockHiJumpBoots"/>
        internal static ushort ShotBlockHiJumpBoots => RoomPlmHeadersConstants.ShotBlockHiJumpBoots;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockIceBeam"/>
        internal static ushort ShotBlockIceBeam => RoomPlmHeadersConstants.ShotBlockIceBeam;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockMorphBall"/>
        internal static ushort ShotBlockMorphBall => RoomPlmHeadersConstants.ShotBlockMorphBall;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockPlasmaBeam"/>
        internal static ushort ShotBlockPlasmaBeam => RoomPlmHeadersConstants.ShotBlockPlasmaBeam;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockPowerBombTank"/>
        internal static ushort ShotBlockPowerBombTank => RoomPlmHeadersConstants.ShotBlockPowerBombTank;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockReserveTank"/>
        internal static ushort ShotBlockReserveTank => RoomPlmHeadersConstants.ShotBlockReserveTank;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockScrewAttack"/>
        internal static ushort ShotBlockScrewAttack => RoomPlmHeadersConstants.ShotBlockScrewAttack;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockSpaceJump"/>
        internal static ushort ShotBlockSpaceJump => RoomPlmHeadersConstants.ShotBlockSpaceJump;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockSpazerBeam"/>
        internal static ushort ShotBlockSpazerBeam => RoomPlmHeadersConstants.ShotBlockSpazerBeam;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockSpeedBooster"/>
        internal static ushort ShotBlockSpeedBooster => RoomPlmHeadersConstants.ShotBlockSpeedBooster;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockSpringBall"/>
        internal static ushort ShotBlockSpringBall => RoomPlmHeadersConstants.ShotBlockSpringBall;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockVariaSuit"/>
        internal static ushort ShotBlockVariaSuit => RoomPlmHeadersConstants.ShotBlockVariaSuit;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockWaveBeam"/>
        internal static ushort ShotBlockWaveBeam => RoomPlmHeadersConstants.ShotBlockWaveBeam;
        /// <inheritdoc cref="RoomPlmHeadersConstants.ShotBlockXrayScope"/>
        internal static ushort ShotBlockXrayScope => RoomPlmHeadersConstants.ShotBlockXrayScope;
    }
}

/// <summary>Cartridge values of <see cref="RoomPlmInstructionCodes"/> that only verification reads.</summary>
internal static class RoomPlmInstructionCodesConstants
{
    /// <summary><c>$84:8764 Instruction_PLM_LoadItemPLMGfx</c>: load an item's graphics set.</summary>
    public const ushort LoadItemGraphics = 0x8764;

    extension(RoomPlmInstructionCodes)
    {
        /// <inheritdoc cref="RoomPlmInstructionCodesConstants.LoadItemGraphics"/>
        internal static ushort LoadItemGraphics => RoomPlmInstructionCodesConstants.LoadItemGraphics;
    }
}

/// <summary>Cartridge values of <see cref="RoomPlmPopulationDefinitions"/> that only verification reads.</summary>
internal static class RoomPlmPopulationDefinitionsConstants
{
    public const int RetailRecordCount = 941;

    extension(RoomPlmPopulationDefinitions)
    {
        /// <inheritdoc cref="RoomPlmPopulationDefinitionsConstants.RetailRecordCount"/>
        internal static int RetailRecordCount => RoomPlmPopulationDefinitionsConstants.RetailRecordCount;
    }
}

/// <summary>Cartridge values of <see cref="RoomShakeDefinitions"/> that only verification reads.</summary>
internal static class RoomShakeDefinitionsConstants
{
    /// <summary>$A0:872D BGShakeDisplacements.BG1X/BG1Y, at eight-byte record stride.</summary>
    public const int Bg1ReferenceAddress = 0xa0872d;
    /// <summary>$A0:8731 BGShakeDisplacements.BG2X/BG2Y, at eight-byte record stride.</summary>
    public const int Bg2ReferenceAddress = 0xa08731;
    /// <summary>$86:846B Get_Values_for_Screen_Shaking.horizontalX/Y, at four-byte stride.</summary>
    public const int ProjectileReferenceAddress = 0x86846b;

    extension(RoomShakeDefinitions)
    {
        /// <inheritdoc cref="RoomShakeDefinitionsConstants.Bg1ReferenceAddress"/>
        internal static int Bg1ReferenceAddress => RoomShakeDefinitionsConstants.Bg1ReferenceAddress;
        /// <inheritdoc cref="RoomShakeDefinitionsConstants.Bg2ReferenceAddress"/>
        internal static int Bg2ReferenceAddress => RoomShakeDefinitionsConstants.Bg2ReferenceAddress;
        /// <inheritdoc cref="RoomShakeDefinitionsConstants.ProjectileReferenceAddress"/>
        internal static int ProjectileReferenceAddress => RoomShakeDefinitionsConstants.ProjectileReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="RoomStateDefinitions"/> that only verification reads.</summary>
internal static class RoomStateDefinitionsConstants
{
    /// <summary>Number of distinct room states selected by the 262 retail rooms.</summary>
    public const int RetailStateCount = 323;

    extension(RoomStateDefinitions)
    {
        /// <inheritdoc cref="RoomStateDefinitionsConstants.RetailStateCount"/>
        internal static int RetailStateCount => RoomStateDefinitionsConstants.RetailStateCount;
    }
}

/// <summary>Cartridge values of <see cref="SamusArmCannonDefinitions"/> that only verification reads.</summary>
internal static class SamusArmCannonDefinitionsConstants
{
    /// <summary>
    /// <c>$90:C7D9</c>, six bytes indexed by the selected HUD item. The native symbol is
    /// <c>ArmCannonOpenFlags</c>; one requests an open cover and zero a closed cover.
    /// </summary>
    public const int OpenFlagTable = 0x90c7d9;

    extension(SamusArmCannonDefinitions)
    {
        /// <inheritdoc cref="SamusArmCannonDefinitionsConstants.OpenFlagTable"/>
        internal static int OpenFlagTable => SamusArmCannonDefinitionsConstants.OpenFlagTable;
    }
}

/// <summary>Cartridge values of <see cref="SamusBeamPreInstructionCodes"/> that only verification reads.</summary>
internal static class SamusBeamPreInstructionCodesConstants
{
    /// <summary>$90:BA3E, FireChargedBeam's low-nibble-indexed callback words.</summary>
    public const int ChargedTable = 0x90ba3e;
    /// <summary>$90:B96E, FireUnchargedBeam's low-nibble-indexed callback words.</summary>
    public const int UnchargedTable = 0x90b96e;

    extension(SamusBeamPreInstructionCodes)
    {
        /// <inheritdoc cref="SamusBeamPreInstructionCodesConstants.ChargedTable"/>
        internal static int ChargedTable => SamusBeamPreInstructionCodesConstants.ChargedTable;
        /// <inheritdoc cref="SamusBeamPreInstructionCodesConstants.UnchargedTable"/>
        internal static int UnchargedTable => SamusBeamPreInstructionCodesConstants.UnchargedTable;
    }
}

/// <summary>Cartridge values of <see cref="SamusBombSpreadRomData"/> that only verification reads.</summary>
internal static class SamusBombSpreadRomDataConstants
{
    /// <summary>Five 16-bit fuse words beginning at $90:D8CF.</summary>
    public const int FuseTimers = 0x90d8cf;
    /// <summary>Five native direction/magnitude X-velocity words beginning at $90:D8D9.</summary>
    public const int XVelocities = 0x90d8d9;
    /// <summary>Five whole-pixel initial Y-speed words beginning at $90:D8E3.</summary>
    public const int YSpeeds = 0x90d8e3;
    /// <summary>Five fractional initial Y-speed words beginning at $90:D8ED.</summary>
    public const int YSubspeeds = 0x90d8ed;

    extension(SamusBombSpreadRomData)
    {
        /// <inheritdoc cref="SamusBombSpreadRomDataConstants.FuseTimers"/>
        internal static int FuseTimers => SamusBombSpreadRomDataConstants.FuseTimers;
        /// <inheritdoc cref="SamusBombSpreadRomDataConstants.XVelocities"/>
        internal static int XVelocities => SamusBombSpreadRomDataConstants.XVelocities;
        /// <inheritdoc cref="SamusBombSpreadRomDataConstants.YSpeeds"/>
        internal static int YSpeeds => SamusBombSpreadRomDataConstants.YSpeeds;
        /// <inheritdoc cref="SamusBombSpreadRomDataConstants.YSubspeeds"/>
        internal static int YSubspeeds => SamusBombSpreadRomDataConstants.YSubspeeds;
    }
}

/// <summary>Cartridge values of <see cref="SamusComboRomData"/> that only verification reads.</summary>
internal static class SamusComboRomDataConstants
{
    /// <summary>IcePlasmaSBAProjectileOriginAngles at $90:CD08.</summary>
    public const int OriginAngles = 0x90cd08;
    /// <summary>SineCosineTables_8bitSine_SignExtended at $A0:B443, positive half-wave words.</summary>
    public const int PositiveSine = 0xa0b443;

    extension(SamusComboRomData)
    {
        /// <inheritdoc cref="SamusComboRomDataConstants.OriginAngles"/>
        internal static int OriginAngles => SamusComboRomDataConstants.OriginAngles;
        /// <inheritdoc cref="SamusComboRomDataConstants.PositiveSine"/>
        internal static int PositiveSine => SamusComboRomDataConstants.PositiveSine;
    }
}

/// <summary>Cartridge values of <see cref="SamusDeathExplosionTimingDefinitions"/> that only verification reads.</summary>
internal static class SamusDeathExplosionTimingDefinitionsConstants
{
    /// <summary>
    /// First timer byte in the nine two-byte timer/palette-index records at
    /// <c>$9B:B823-$9B:B834</c>.
    /// </summary>
    public const int NativeFirstTimerAddress = 0x9bb823;
    /// <summary>Bytes occupied by each interleaved timer/palette-index record.</summary>
    public const ushort RecordByteCount = 2;

    extension(SamusDeathExplosionTimingDefinitions)
    {
        /// <inheritdoc cref="SamusDeathExplosionTimingDefinitionsConstants.NativeFirstTimerAddress"/>
        internal static int NativeFirstTimerAddress => SamusDeathExplosionTimingDefinitionsConstants.NativeFirstTimerAddress;
        /// <inheritdoc cref="SamusDeathExplosionTimingDefinitionsConstants.RecordByteCount"/>
        internal static ushort RecordByteCount => SamusDeathExplosionTimingDefinitionsConstants.RecordByteCount;
    }
}

/// <summary>Cartridge values of <see cref="SamusGrappleRomData.Connections"/> that only verification reads.</summary>
internal static class SamusGrappleRomDataConnectionsConstants
{
    /// <summary>Number of shot directions represented by each four-byte connection table.</summary>
    public const int DirectionCount = SamusGrappleRomData.Firing.DirectionCount;
    /// <summary>Bytes in one special-angle record.</summary>
    public const int SpecialAngleRecordByteCount = 10;
    /// <summary>Number of fixed-angle special connection records.</summary>
    public const int SpecialAngleRecordCount = 8;
    /// <summary>Special-angle records used for fixed, swing, and wall-grab reactions.</summary>
    public const int SpecialAngleTable = 0x9bc43e;

    extension(SamusGrappleRomData.Connections)
    {
        /// <inheritdoc cref="SamusGrappleRomDataConnectionsConstants.DirectionCount"/>
        internal static int DirectionCount => SamusGrappleRomDataConnectionsConstants.DirectionCount;
        /// <inheritdoc cref="SamusGrappleRomDataConnectionsConstants.SpecialAngleRecordByteCount"/>
        internal static int SpecialAngleRecordByteCount => SamusGrappleRomDataConnectionsConstants.SpecialAngleRecordByteCount;
        /// <inheritdoc cref="SamusGrappleRomDataConnectionsConstants.SpecialAngleRecordCount"/>
        internal static int SpecialAngleRecordCount => SamusGrappleRomDataConnectionsConstants.SpecialAngleRecordCount;
        /// <inheritdoc cref="SamusGrappleRomDataConnectionsConstants.SpecialAngleTable"/>
        internal static int SpecialAngleTable => SamusGrappleRomDataConnectionsConstants.SpecialAngleTable;
    }
}

/// <summary>Cartridge values of <see cref="SamusGrappleRomData.Firing"/> that only verification reads.</summary>
internal static class SamusGrappleRomDataFiringConstants
{
    /// <summary>Initial angles for ten shot directions.</summary>
    public const int Angles = 0x9bc104;
    /// <summary>Non-running grapple-point X origins.</summary>
    public const int DefaultOriginX = 0x9bc122;
    /// <summary>Non-running grapple-point Y origins.</summary>
    public const int DefaultOriginY = 0x9bc136;
    /// <summary>Left-facing flare spritemap offsets in bank $93.</summary>
    public const int LeftFlareSpritemapOffsets = 0x93a22b;
    /// <summary>Right-facing flare spritemap offsets in bank $93.</summary>
    public const int RightFlareSpritemapOffsets = 0x93a225;
    /// <summary>Running grapple-point X origins.</summary>
    public const int RunningOriginX = 0x9bc172;
    /// <summary>Running grapple-point Y origins.</summary>
    public const int RunningOriginY = 0x9bc186;
    /// <summary>Initial X velocities for ten shot directions.</summary>
    public const int XVelocities = 0x9bc0db;
    /// <summary>Initial Y velocities for ten shot directions.</summary>
    public const int YVelocities = 0x9bc0ef;

    extension(SamusGrappleRomData.Firing)
    {
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.Angles"/>
        internal static int Angles => SamusGrappleRomDataFiringConstants.Angles;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.DefaultOriginX"/>
        internal static int DefaultOriginX => SamusGrappleRomDataFiringConstants.DefaultOriginX;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.DefaultOriginY"/>
        internal static int DefaultOriginY => SamusGrappleRomDataFiringConstants.DefaultOriginY;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.LeftFlareSpritemapOffsets"/>
        internal static int LeftFlareSpritemapOffsets => SamusGrappleRomDataFiringConstants.LeftFlareSpritemapOffsets;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.RightFlareSpritemapOffsets"/>
        internal static int RightFlareSpritemapOffsets => SamusGrappleRomDataFiringConstants.RightFlareSpritemapOffsets;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.RunningOriginX"/>
        internal static int RunningOriginX => SamusGrappleRomDataFiringConstants.RunningOriginX;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.RunningOriginY"/>
        internal static int RunningOriginY => SamusGrappleRomDataFiringConstants.RunningOriginY;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.XVelocities"/>
        internal static int XVelocities => SamusGrappleRomDataFiringConstants.XVelocities;
        /// <inheritdoc cref="SamusGrappleRomDataFiringConstants.YVelocities"/>
        internal static int YVelocities => SamusGrappleRomDataFiringConstants.YVelocities;
    }
}

/// <summary>Cartridge values of <see cref="SamusGrappleRomData.Physics"/> that only verification reads.</summary>
internal static class SamusGrappleRomDataPhysicsConstants
{
    /// <summary><c>$A0:B3C3</c>, signed sine values indexed by <c>SnesAngle</c>.</summary>
    public const int SignedSineTable = 0xa0b3c3;

    extension(SamusGrappleRomData.Physics)
    {
        /// <inheritdoc cref="SamusGrappleRomDataPhysicsConstants.SignedSineTable"/>
        internal static int SignedSineTable => SamusGrappleRomDataPhysicsConstants.SignedSineTable;
    }
}

/// <summary>Cartridge values of <see cref="SamusGrappleRomData.Rendering"/> that only verification reads.</summary>
internal static class SamusGrappleRomDataRenderingConstants
{
    /// <summary>Left-facing body offsets indexed by swing frame.</summary>
    public const int LeftPoseOffsetsByFrame = 0x9bc2c2;
    /// <summary>$9B:C342/C344: inclusive begin and exclusive end of the Grapple endpoint's strided character range.</summary>
    public const int PointTilePointers = 0x9bc342;
    /// <summary>Right-facing body offsets indexed by swing frame.</summary>
    public const int RightPoseOffsetsByFrame = 0x9bc302;
    /// <summary>Bytes uploaded for four Grapple segment characters.</summary>
    public const ushort SegmentTileByteCount = 0x80;
    /// <summary>Pointers to folded-angle rope-segment character data.</summary>
    public const int SegmentTilePointers = 0x9bc346;

    extension(SamusGrappleRomData.Rendering)
    {
        /// <inheritdoc cref="SamusGrappleRomDataRenderingConstants.LeftPoseOffsetsByFrame"/>
        internal static int LeftPoseOffsetsByFrame => SamusGrappleRomDataRenderingConstants.LeftPoseOffsetsByFrame;
        /// <inheritdoc cref="SamusGrappleRomDataRenderingConstants.PointTilePointers"/>
        internal static int PointTilePointers => SamusGrappleRomDataRenderingConstants.PointTilePointers;
        /// <inheritdoc cref="SamusGrappleRomDataRenderingConstants.RightPoseOffsetsByFrame"/>
        internal static int RightPoseOffsetsByFrame => SamusGrappleRomDataRenderingConstants.RightPoseOffsetsByFrame;
        /// <inheritdoc cref="SamusGrappleRomDataRenderingConstants.SegmentTileByteCount"/>
        internal static ushort SegmentTileByteCount => SamusGrappleRomDataRenderingConstants.SegmentTileByteCount;
        /// <inheritdoc cref="SamusGrappleRomDataRenderingConstants.SegmentTilePointers"/>
        internal static int SegmentTilePointers => SamusGrappleRomDataRenderingConstants.SegmentTilePointers;
    }
}

/// <summary>Cartridge values of <see cref="SamusHudRomData"/> that only verification reads.</summary>
internal static class SamusHudRomDataConstants
{
    /// <summary>$90:DD05 movement-type HUD handler pointer table.</summary>
    public const int MovementHandlers = 0x90dd05;
    /// <summary>$90:DDAA posture-transition flags: zero enters the standard handler.</summary>
    public const int TransitionFlags = 0x90ddaa;

    extension(SamusHudRomData)
    {
        /// <inheritdoc cref="SamusHudRomDataConstants.MovementHandlers"/>
        internal static int MovementHandlers => SamusHudRomDataConstants.MovementHandlers;
        /// <inheritdoc cref="SamusHudRomDataConstants.TransitionFlags"/>
        internal static int TransitionFlags => SamusHudRomDataConstants.TransitionFlags;
    }
}

/// <summary>Cartridge values of <see cref="SamusLoadingSuitPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class SamusLoadingSuitPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Every suit-loading program lasts 265 frames.</summary>
    public const int CycleFrames = 265;

    extension(SamusLoadingSuitPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="SamusLoadingSuitPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => SamusLoadingSuitPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}

/// <summary>Cartridge values of <see cref="SamusMovementRomData.HorizontalMotion"/> that only verification reads.</summary>
internal static class SamusMovementRomDataHorizontalMotionConstants
{
    /// <summary><c>$91:B61F</c>, initial/next low byte for each Speed Booster stage.</summary>
    public const int SpeedBoostCounterLowBytes = 0x91b61f;

    extension(SamusMovementRomData.HorizontalMotion)
    {
        /// <inheritdoc cref="SamusMovementRomDataHorizontalMotionConstants.SpeedBoostCounterLowBytes"/>
        internal static int SpeedBoostCounterLowBytes => SamusMovementRomDataHorizontalMotionConstants.SpeedBoostCounterLowBytes;
    }
}

/// <summary>Cartridge values of <see cref="SamusMovementRomData.Poses"/> that only verification reads.</summary>
internal static class SamusMovementRomDataPosesConstants
{
    /// <summary><c>$91:B010</c>, one bank-$91 animation-delay-list pointer per pose.</summary>
    public const int AnimationDelayListPointers = 0x91b010;
    /// <summary><c>$91:9EE2</c>, one bank-$91 input-transition-list pointer per pose.</summary>
    public const int TransitionListPointers = 0x919ee2;

    extension(SamusMovementRomData.Poses)
    {
        /// <inheritdoc cref="SamusMovementRomDataPosesConstants.AnimationDelayListPointers"/>
        internal static int AnimationDelayListPointers => SamusMovementRomDataPosesConstants.AnimationDelayListPointers;
        /// <inheritdoc cref="SamusMovementRomDataPosesConstants.TransitionListPointers"/>
        internal static int TransitionListPointers => SamusMovementRomDataPosesConstants.TransitionListPointers;
    }
}

/// <summary>Cartridge values of <see cref="SamusMovementRomData.VerticalMotion"/> that only verification reads.</summary>
internal static class SamusMovementRomDataVerticalMotionConstants
{
    /// <summary>$90:9EB5 YSpeedWhenBouncingInMorphBall; reference address for the shared ball rebound.</summary>
    public const int BallBounceSpeed = 0x909eb5;
    /// <summary>$90:9EB7 YSubSpeedWhenBouncingInMorphBall; reference address for the shared ball rebound fraction.</summary>
    public const int BallBounceSubspeed = 0x909eb7;
    /// <summary>Bomb-jump launch speed selected by the current liquid medium.</summary>
    public const int BombJumpSpeeds = 0x909ef5;
    /// <summary>Bomb-jump launch subspeed selected by the current liquid medium.</summary>
    public const int BombJumpSubspeeds = 0x909efb;
    /// <summary>Dry-air, water, and lava/acid whole gravity words.</summary>
    public const int GravityAccelerations = 0x909ea7;
    /// <summary>Dry-air, water, and lava/acid fractional gravity words.</summary>
    public const int GravitySubaccelerations = 0x909ea1;
    /// <summary>Hi-Jump speed selected by the current liquid medium.</summary>
    public const int HiJumpSpeeds = 0x909ec5;
    /// <summary>Hi-Jump subspeed selected by the current liquid medium.</summary>
    public const int HiJumpSubspeeds = 0x909ecb;
    /// <summary>Hi-Jump wall-jump speed selected by the current liquid medium.</summary>
    public const int HiWallJumpSpeeds = 0x909edd;
    /// <summary>Hi-Jump wall-jump subspeed selected by the current liquid medium.</summary>
    public const int HiWallJumpSubspeeds = 0x909ee3;
    /// <summary>Knockback launch speed selected by the current liquid medium.</summary>
    public const int KnockbackSpeeds = 0x909ee9;
    /// <summary>Knockback launch subspeed selected by the current liquid medium.</summary>
    public const int KnockbackSubspeeds = 0x909eef;
    /// <summary>Normal-jump speed selected by the current liquid medium.</summary>
    public const int NormalJumpSpeeds = 0x909eb9;
    /// <summary>Normal-jump subspeed selected by the current liquid medium.</summary>
    public const int NormalJumpSubspeeds = 0x909ebf;
    /// <summary>Wall-jump speed selected by the current liquid medium.</summary>
    public const int WallJumpSpeeds = 0x909ed1;
    /// <summary>Wall-jump subspeed selected by the current liquid medium.</summary>
    public const int WallJumpSubspeeds = 0x909ed7;

    extension(SamusMovementRomData.VerticalMotion)
    {
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.BallBounceSpeed"/>
        internal static int BallBounceSpeed => SamusMovementRomDataVerticalMotionConstants.BallBounceSpeed;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.BallBounceSubspeed"/>
        internal static int BallBounceSubspeed => SamusMovementRomDataVerticalMotionConstants.BallBounceSubspeed;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.BombJumpSpeeds"/>
        internal static int BombJumpSpeeds => SamusMovementRomDataVerticalMotionConstants.BombJumpSpeeds;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.BombJumpSubspeeds"/>
        internal static int BombJumpSubspeeds => SamusMovementRomDataVerticalMotionConstants.BombJumpSubspeeds;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.GravityAccelerations"/>
        internal static int GravityAccelerations => SamusMovementRomDataVerticalMotionConstants.GravityAccelerations;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.GravitySubaccelerations"/>
        internal static int GravitySubaccelerations => SamusMovementRomDataVerticalMotionConstants.GravitySubaccelerations;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.HiJumpSpeeds"/>
        internal static int HiJumpSpeeds => SamusMovementRomDataVerticalMotionConstants.HiJumpSpeeds;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.HiJumpSubspeeds"/>
        internal static int HiJumpSubspeeds => SamusMovementRomDataVerticalMotionConstants.HiJumpSubspeeds;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.HiWallJumpSpeeds"/>
        internal static int HiWallJumpSpeeds => SamusMovementRomDataVerticalMotionConstants.HiWallJumpSpeeds;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.HiWallJumpSubspeeds"/>
        internal static int HiWallJumpSubspeeds => SamusMovementRomDataVerticalMotionConstants.HiWallJumpSubspeeds;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.KnockbackSpeeds"/>
        internal static int KnockbackSpeeds => SamusMovementRomDataVerticalMotionConstants.KnockbackSpeeds;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.KnockbackSubspeeds"/>
        internal static int KnockbackSubspeeds => SamusMovementRomDataVerticalMotionConstants.KnockbackSubspeeds;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.NormalJumpSpeeds"/>
        internal static int NormalJumpSpeeds => SamusMovementRomDataVerticalMotionConstants.NormalJumpSpeeds;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.NormalJumpSubspeeds"/>
        internal static int NormalJumpSubspeeds => SamusMovementRomDataVerticalMotionConstants.NormalJumpSubspeeds;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.WallJumpSpeeds"/>
        internal static int WallJumpSpeeds => SamusMovementRomDataVerticalMotionConstants.WallJumpSpeeds;
        /// <inheritdoc cref="SamusMovementRomDataVerticalMotionConstants.WallJumpSubspeeds"/>
        internal static int WallJumpSubspeeds => SamusMovementRomDataVerticalMotionConstants.WallJumpSubspeeds;
    }
}

/// <summary>Cartridge values of <see cref="SamusPaletteRomData.Banks"/> that only verification reads.</summary>
internal static class SamusPaletteRomDataBanksConstants
{
    /// <summary>Bank <c>$91</c>, which owns Samus palette pointer lists.</summary>
    public const int Movement = 0x910000;

    extension(SamusPaletteRomData.Banks)
    {
        /// <inheritdoc cref="SamusPaletteRomDataBanksConstants.Movement"/>
        internal static int Movement => SamusPaletteRomDataBanksConstants.Movement;
    }
}

/// <summary>Cartridge values of <see cref="SamusPaletteRomData.Common"/> that only verification reads.</summary>
internal static class SamusPaletteRomDataCommonConstants
{
    /// <summary><c>$91:D727</c>, Power/Varia/Gravity normal-palette pointers.</summary>
    /// <remarks>
    /// Issue #859 / #625: the pinned NTSC J/U v1.0 ROM has three little-endian
    /// words <c>$9400,$9520,$9800</c> at byte offsets 0, 2, 4. They select
    /// the authored normal Power, Varia, and Gravity palettes in bank
    /// <c>$9B</c>. Every production suit selector reaches only those even
    /// offsets; Gravity has priority over Varia when both equipment bits
    /// are set. The X-ray and projectile catalogs alias this same physical
    /// table. NormalSuitPalettePointer implements the three semantic suit
    /// cases directly; no stored pointer lookup is retained.
    /// </remarks>
    public const int NormalSuitPointers = 0x91d727;

    extension(SamusPaletteRomData.Common)
    {
        /// <inheritdoc cref="SamusPaletteRomDataCommonConstants.NormalSuitPointers"/>
        internal static int NormalSuitPointers => SamusPaletteRomDataCommonConstants.NormalSuitPointers;
    }
}

/// <summary>Cartridge values of <see cref="SamusPaletteRomData.CrystalFlash"/> that only verification reads.</summary>
internal static class SamusPaletteRomDataCrystalFlashConstants
{
    /// <summary><c>$90:C3C9</c>, twelve beam-loadout palette pointers.</summary>
    /// <remarks>
    /// This is the same physical table as
    /// <see cref="SamusProjectileRomData.Beams.PalettePointers"/>:
    /// Ice, then Plasma, then Wave, then Spazer, then Power priority
    /// exactly selects one of five bank-$90 palettes for indices 0..11.
    /// Crystal Flash restoration uses the masked equipped-beam word to
    /// read one of these pointers before copying sixteen colors.
    /// Investigation: #625 / #900.
    /// </remarks>
    public const int BeamPalettePointers = 0x90c3c9;

    extension(SamusPaletteRomData.CrystalFlash)
    {
        /// <inheritdoc cref="SamusPaletteRomDataCrystalFlashConstants.BeamPalettePointers"/>
        internal static int BeamPalettePointers => SamusPaletteRomDataCrystalFlashConstants.BeamPalettePointers;
    }
}

/// <summary>Cartridge values of <see cref="SamusPaletteRomData.FullBodyCycles"/> that only verification reads.</summary>
internal static class SamusPaletteRomDataFullBodyCyclesConstants
{
    /// <summary><c>$91:DB75</c>, suit-indexed active-shinespark palette lists.</summary>
    /// <remarks>
    /// Issue #882 / #625: the pinned NTSC J/U v1.0 ROM's three
    /// little-endian words are exactly <c>$DB7B+8*s</c> for suit
    /// index <c>s=0..2</c>, matching the native bank-$91 listing.
    /// Palette handler six selects Power, Varia, or Gravity with
    /// byte offset <c>2*s</c>, then reads four phase pointers at
    /// offsets <c>0,2,4,6</c> and wraps to zero. Each selected
    /// bank-$9B target supplies sixteen Samus OBJ colors. This
    /// formula describes list addresses; nested pointers and
    /// colors are separate proof targets.
    ///
    /// Issue #883 / #625: the twelve nested words at
    /// <c>$91:DB7B..DB92</c> are exactly
    /// <c>$9C20+$0200*s+$0020*p</c> for suit index
    /// <c>s=0..2</c> and phase <c>p=0..3</c>. Every word matches
    /// the pinned ROM and native bank-$91 listing. Palette handler
    /// six cycles byte offsets <c>0,2,4,6</c> and wraps; each
    /// target supplies sixteen bank-$9B colors. The formula
    /// describes target addresses, not their authored colors.
    ///
    /// Issue #884 / #625: Power active-shinespark colors at $9B:9C20..9C9F; shade zero equals normal Power and Screw Attack shade zero.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// Issue #885 / #625: Varia active-shinespark colors at $9B:9E20..9E9F; shade zero equals Screw Attack shade zero.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// Issue #886 / #625: Gravity active-shinespark colors at $9B:A020..A09F; these duplicate the earlier $9B:95C0..963F allocation.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// </remarks>
    public const int ActiveShinesparkLists = 0x91db75;
    /// <summary><c>$91:D99E</c>, ten full-body Hyper Beam palette pointers.</summary>
    /// <remarks>
    /// Issue #867 / #625: the pinned NTSC J/U v1.0 ROM's ten
    /// little-endian words are exactly <c>$A360-$0020*i</c> for
    /// <c>i=0..9</c>, ending at <c>$A240</c>. Each points to a distinct
    /// sixteen-color bank-$9B palette. Rainbow Samus starts at zero,
    /// reads byte offset <c>2*i</c>, and wraps after index nine; the
    /// managed caller also bounds restored index values with modulo ten.
    /// The stride describes pointer selection; installed artwork now supplies
    /// the authored target colors without reading those cartridge bytes at runtime.
    ///
    /// Hyper Beam target colors at $9B:A240..A37F have an independent
    /// #1165 review in SamusHyperBeamColorCatalog. This pointer mapping
    /// provides no retention justification for their remaining color inputs.
    /// </remarks>
    public const int HyperBeamPointers = 0x91d99e;
    /// <summary><c>$91:DA4A</c>, suit-indexed Screw Attack palette lists.</summary>
    /// <remarks>
    /// Issue #869 / #625: the pinned NTSC J/U v1.0 ROM stores three
    /// little-endian pointers <c>$DA50+$000C*i</c> for Power, Varia,
    /// and Gravity suit index <c>i=0..2</c>. The production selector
    /// reaches only byte offsets 0, 2, 4, with Gravity priority.
    /// Each target is a separate six-word bank-$91 palette-pointer
    /// list, indexed by byte offsets 0, 2, 4, 6, 8, 10; this proof
    /// covers only the top-level three-word selector.
    ///
    /// Issue #870 / #625: all eighteen nested words in
    /// <c>$91:DA50..DA73</c> match
    /// <c>$9CA0+$0200*s+$0020*min(p,6-p)</c> for suit <c>s=0..2</c>
    /// and phase <c>p=0..5</c>. The six phases visit shade offsets
    /// 0, 1, 2, 3, 2, 1 before wrapping. The selected bank-$9B target
    /// remains a live authored sixteen-color palette; this formula
    /// describes only the bounded pointer matrix.
    ///
    /// Issue #871 / #625: Power Screw Attack colors at $9B:9CA0..9D1F; shade zero matches normal Power $9B:9400.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// Issue #872 / #625: Varia Screw Attack colors at $9B:9EA0..9F1F; shade zero differs from normal Varia only at transparent index zero.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// Issue #873 / #625: Gravity Screw Attack colors at $9B:A0A0..A11F; shade zero differs from normal Gravity at color slots1/12.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// </remarks>
    public const int ScrewAttackLists = 0x91da4a;
    /// <summary><c>$91:D998</c>, suit-indexed Speed Booster flash palettes.</summary>
    /// <remarks>
    /// Issue #866 / #625: the pinned NTSC J/U v1.0 ROM's three
    /// little-endian pointers at byte offsets <c>2*i</c>, <c>i=0..2</c>,
    /// are exactly <c>$9B80+$0200*i</c>. They select the Power, Varia,
    /// and Gravity speed-boost shades that also appear in loading
    /// palette programs #856–#858. The Metroid-attachment palette
    /// caller reaches only byte offsets 0, 2, 4, with Gravity priority,
    /// and copies sixteen target colors into Samus OBJ CGRAM 192..207.
    /// This is a bounded pointer relationship; the target colors remain
    /// authored cartridge data.
    /// </remarks>
    public const int SpeedBoostPointers = 0x91d998;
    /// <summary><c>$91:DAA9</c>, suit-indexed active Speed Booster palette lists.</summary>
    /// <remarks>
    /// Issue #874 / #625: the pinned NTSC J/U v1.0 ROM's three
    /// little-endian words are exactly <c>$DAAF+8*s</c> for suit
    /// index <c>s=0..2</c>, matching the native bank-$91 listing.
    /// The caller selects Power, Varia, or Gravity with byte offset
    /// <c>2*s</c>, reads four bank-$9B palette pointers from the chosen
    /// bank-$91 list at phase offsets <c>0,2,4,6</c>, and then pins
    /// the last phase. This stride describes only the three list
    /// addresses; the nested pointers and colors are separate data.
    ///
    /// Issue #875 / #625: the twelve nested words at
    /// <c>$91:DAAF..DAC6</c> are exactly
    /// <c>$9B20+$0200*s+$0020*p</c> for suit index <c>s=0..2</c>
    /// and phase <c>p=0..3</c>. Every word matches the pinned ROM and
    /// native bank-$91 listing. The caller indexes the four phase
    /// pointers with byte offsets <c>0,2,4,6</c> and pins phase three;
    /// each target is a complete sixteen-color bank-$9B palette.
    /// The formula describes target addresses, not target colors.
    ///
    /// Issue #876 / #625: Power Speed Booster colors at $9B:9B20..9B9F; the three bright rows also supply the loading palette.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// Issue #877 / #625: Varia Speed Booster colors at $9B:9D20..9D9F; the three bright rows also supply the loading palette.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// Issue #878 / #625: Gravity Speed Booster colors at $9B:9F20..9F9F; these duplicate the earlier $9B:9540..95BF allocation.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// </remarks>
    public const int SpeedBoosterLists = 0x91daa9;
    /// <summary><c>$91:DB10</c>, suit-indexed stored-shine palette lists.</summary>
    /// <remarks>
    /// Issue #879 / #625: the pinned NTSC J/U v1.0 ROM's three
    /// little-endian words are exactly <c>$DB16+12*s</c> for suit
    /// index <c>s=0..2</c>, matching the native bank-$91 listing.
    /// Palette handler one selects Power, Varia, or Gravity with
    /// byte offset <c>2*s</c>, reads the selected bank-$91 list at
    /// six phase offsets <c>0,2,4,6,8,10</c>, and wraps to zero.
    /// The caller copies sixteen colors from each bank-$9B target.
    /// This formula describes list addresses only; the nested
    /// pointers and target colors are separate proof targets.
    ///
    /// Issue #880 / #625: the eighteen nested words at
    /// <c>$91:DB16..DB39</c> are exactly
    /// <c>$9BA0+$0200*s+$0020*min(p,6-p)</c> for suit index
    /// <c>s=0..2</c> and phase <c>p=0..5</c>. Every word matches
    /// the pinned ROM and native bank-$91 listing. Palette handler
    /// one cycles six byte offsets <c>0,2,4,6,8,10</c>, so each
    /// suit visits color rows <c>0,1,2,3,2,1</c> before wrapping.
    /// Each target is a complete sixteen-color bank-$9B palette;
    /// the formula does not describe its authored colors.
    ///
    /// Issue #881 / #625: the 192 BGR555 words in four distinct
    /// stored-shine rows per suit at <c>$9B:9BA0..9C1F</c>,
    /// <c>$9B:9DA0..9E1F</c>, and <c>$9B:9FA0..A01F</c> follow an
    /// exact reuse rule. For suit <c>s=0..2</c>, row <c>p=0..3</c>,
    /// and color <c>c=1..15</c>, the word equals the same color in
    /// the death-sequence/beam-charge row at
    /// <c>$9B:9820+$0100*s+$0040*p</c>; color zero is always
    /// <c>$0000</c>. Direct comparison of all 192 pinned ROM words
    /// has zero mismatches, consistent with the native bank-$9B
    /// listing. The pointer matrix visits rows <c>0,1,2,3,2,1</c>.
    /// The source rows remain authored cartridge palettes.
    /// </remarks>
    public const int StoredShineLists = 0x91db10;

    extension(SamusPaletteRomData.FullBodyCycles)
    {
        /// <inheritdoc cref="SamusPaletteRomDataFullBodyCyclesConstants.ActiveShinesparkLists"/>
        internal static int ActiveShinesparkLists => SamusPaletteRomDataFullBodyCyclesConstants.ActiveShinesparkLists;
        /// <inheritdoc cref="SamusPaletteRomDataFullBodyCyclesConstants.HyperBeamPointers"/>
        internal static int HyperBeamPointers => SamusPaletteRomDataFullBodyCyclesConstants.HyperBeamPointers;
        /// <inheritdoc cref="SamusPaletteRomDataFullBodyCyclesConstants.ScrewAttackLists"/>
        internal static int ScrewAttackLists => SamusPaletteRomDataFullBodyCyclesConstants.ScrewAttackLists;
        /// <inheritdoc cref="SamusPaletteRomDataFullBodyCyclesConstants.SpeedBoostPointers"/>
        internal static int SpeedBoostPointers => SamusPaletteRomDataFullBodyCyclesConstants.SpeedBoostPointers;
        /// <inheritdoc cref="SamusPaletteRomDataFullBodyCyclesConstants.SpeedBoosterLists"/>
        internal static int SpeedBoosterLists => SamusPaletteRomDataFullBodyCyclesConstants.SpeedBoosterLists;
        /// <inheritdoc cref="SamusPaletteRomDataFullBodyCyclesConstants.StoredShineLists"/>
        internal static int StoredShineLists => SamusPaletteRomDataFullBodyCyclesConstants.StoredShineLists;
    }
}

/// <summary>Cartridge values of <see cref="SamusPaletteRomData.HyperBeamFx"/> that only verification reads.</summary>
internal static class SamusPaletteRomDataHyperBeamFxConstants
{
    /// <summary>Instruction <c>$C595</c>: finish the current timed palette record.</summary>
    public const ushort Done = PaletteFxInstructionCodes.Wait;
    /// <summary>Bytes occupied by a duration, eight colors, and the done opcode.</summary>
    public const int FrameByteCount = 20;
    /// <summary>Number of timed color records in the loop.</summary>
    public const int FrameCount = 10;
    /// <summary>Instruction <c>$C61E</c>: jump to the instruction pointer in Y.</summary>
    public const ushort Goto = PaletteFxInstructionCodes.Goto;
    /// <summary>Initial instruction list stored by the object definition.</summary>
    public const ushort InitialList = 0xd900;
    /// <summary>Instruction <c>$C655</c>: select palette-buffer byte index from Y.</summary>
    public const ushort SetColorIndex = PaletteFxInstructionCodes.SetColorIndex;

    extension(SamusPaletteRomData.HyperBeamFx)
    {
        /// <inheritdoc cref="SamusPaletteRomDataHyperBeamFxConstants.Done"/>
        internal static ushort Done => SamusPaletteRomDataHyperBeamFxConstants.Done;
        /// <inheritdoc cref="SamusPaletteRomDataHyperBeamFxConstants.FrameByteCount"/>
        internal static int FrameByteCount => SamusPaletteRomDataHyperBeamFxConstants.FrameByteCount;
        /// <inheritdoc cref="SamusPaletteRomDataHyperBeamFxConstants.FrameCount"/>
        internal static int FrameCount => SamusPaletteRomDataHyperBeamFxConstants.FrameCount;
        /// <inheritdoc cref="SamusPaletteRomDataHyperBeamFxConstants.Goto"/>
        internal static ushort Goto => SamusPaletteRomDataHyperBeamFxConstants.Goto;
        /// <inheritdoc cref="SamusPaletteRomDataHyperBeamFxConstants.InitialList"/>
        internal static ushort InitialList => SamusPaletteRomDataHyperBeamFxConstants.InitialList;
        /// <inheritdoc cref="SamusPaletteRomDataHyperBeamFxConstants.SetColorIndex"/>
        internal static ushort SetColorIndex => SamusPaletteRomDataHyperBeamFxConstants.SetColorIndex;
    }
}

/// <summary>Cartridge values of <see cref="SamusPoseIds"/> that only verification reads.</summary>
internal static class SamusPoseIdsConstants
{
    public const byte UnusedPoseDe = (byte)SamusPoseId.UnusedPoseDe;
    public const byte UnusedPoseDf = (byte)SamusPoseId.UnusedPoseDf;

    extension(SamusPoseIds)
    {
        /// <inheritdoc cref="SamusPoseIdsConstants.UnusedPoseDe"/>
        internal static byte UnusedPoseDe => SamusPoseIdsConstants.UnusedPoseDe;
        /// <inheritdoc cref="SamusPoseIdsConstants.UnusedPoseDf"/>
        internal static byte UnusedPoseDf => SamusPoseIdsConstants.UnusedPoseDf;
    }
}

/// <summary>Cartridge values of <see cref="SamusProjectileRomData.Beams"/> that only verification reads.</summary>
internal static class SamusProjectileRomDataBeamsConstants
{
    /// <summary>Charged firing sound IDs indexed by beam combination.</summary>
    public const int ChargedSounds = 0x90c2a7;
    /// <summary>Damage word followed by one instruction pointer per direction.</summary>
    public const int DataRecordByteCount = sizeof(ushort) + SamusProjectileRomData.Origins.DirectionCount * sizeof(ushort);
    /// <summary>Duration, spritemap, radii, and animation word in one instruction record.</summary>
    public const int InstructionRecordByteCount = 8;
    /// <summary>Uncharged firing sound IDs indexed by beam combination.</summary>
    public const int UnchargedSounds = 0x90c28f;

    extension(SamusProjectileRomData.Beams)
    {
        /// <inheritdoc cref="SamusProjectileRomDataBeamsConstants.ChargedSounds"/>
        internal static int ChargedSounds => SamusProjectileRomDataBeamsConstants.ChargedSounds;
        /// <inheritdoc cref="SamusProjectileRomDataBeamsConstants.DataRecordByteCount"/>
        internal static int DataRecordByteCount => SamusProjectileRomDataBeamsConstants.DataRecordByteCount;
        /// <inheritdoc cref="SamusProjectileRomDataBeamsConstants.InstructionRecordByteCount"/>
        internal static int InstructionRecordByteCount => SamusProjectileRomDataBeamsConstants.InstructionRecordByteCount;
        /// <inheritdoc cref="SamusProjectileRomDataBeamsConstants.UnchargedSounds"/>
        internal static int UnchargedSounds => SamusProjectileRomDataBeamsConstants.UnchargedSounds;
    }
}

/// <summary>Cartridge values of <see cref="SamusProjectileRomData.NonBeam"/> that only verification reads.</summary>
internal static class SamusProjectileRomDataNonBeamConstants
{
    /// <summary>Bytes in one direction's acceleration/subacceleration record.</summary>
    public const int AccelerationRecordByteCount = 4;

    extension(SamusProjectileRomData.NonBeam)
    {
        /// <inheritdoc cref="SamusProjectileRomDataNonBeamConstants.AccelerationRecordByteCount"/>
        internal static int AccelerationRecordByteCount => SamusProjectileRomDataNonBeamConstants.AccelerationRecordByteCount;
    }
}

/// <summary>Cartridge values of <see cref="SamusProjectileRomData.Trails"/> that only verification reads.</summary>
internal static class SamusProjectileRomDataTrailsConstants
{
    /// <summary>Number of trail pointer entries before the paired right table.</summary>
    public const int InstructionPointerCount = (SamusProjectileRomData.Trails.RightInstructionPointers - SamusProjectileRomData.Trails.LeftInstructionPointers) / sizeof(ushort);

    extension(SamusProjectileRomData.Trails)
    {
        /// <inheritdoc cref="SamusProjectileRomDataTrailsConstants.InstructionPointerCount"/>
        internal static int InstructionPointerCount => SamusProjectileRomDataTrailsConstants.InstructionPointerCount;
    }
}

/// <summary>Cartridge values of <see cref="SamusRenderingRomData.Body"/> that only verification reads.</summary>
internal static class SamusRenderingRomDataBodyConstants
{
    /// <summary><c>$92:945D</c>, base bottom-spritemap index for each pose.</summary>
    public const int BottomSpritemapBaseIndices = 0x92945d;
    /// <summary>Number of colors copied into Samus's OBJ palette.</summary>
    public const int SuitPaletteColorCount = 16;
    /// <summary><c>$92:9263</c>, base top-spritemap index for each pose.</summary>
    public const int TopSpritemapBaseIndices = 0x929263;

    extension(SamusRenderingRomData.Body)
    {
        /// <inheritdoc cref="SamusRenderingRomDataBodyConstants.BottomSpritemapBaseIndices"/>
        internal static int BottomSpritemapBaseIndices => SamusRenderingRomDataBodyConstants.BottomSpritemapBaseIndices;
        /// <inheritdoc cref="SamusRenderingRomDataBodyConstants.SuitPaletteColorCount"/>
        internal static int SuitPaletteColorCount => SamusRenderingRomDataBodyConstants.SuitPaletteColorCount;
        /// <inheritdoc cref="SamusRenderingRomDataBodyConstants.TopSpritemapBaseIndices"/>
        internal static int TopSpritemapBaseIndices => SamusRenderingRomDataBodyConstants.TopSpritemapBaseIndices;
    }
}

/// <summary>Cartridge values of <see cref="SamusRenderingRomData.TileTransfers"/> that only verification reads.</summary>
internal static class SamusRenderingRomDataTileTransfersConstants
{
    /// <summary>Bytes in one pose/frame selector: top set/position and bottom set/position.</summary>
    public const int AnimationRecordByteCount = 4;
    /// <summary>Number of bottom-half graphics sets before the animation pointer table.</summary>
    public const int BottomDefinitionSetCount = (SamusRenderingRomData.TileTransfers.AnimationDefinitionListPointers - SamusRenderingRomData.TileTransfers.BottomDefinitionListPointers) / sizeof(ushort);
    /// <summary>Number of top-half graphics sets before the bottom pointer table.</summary>
    public const int TopDefinitionSetCount = (SamusRenderingRomData.TileTransfers.BottomDefinitionListPointers - SamusRenderingRomData.TileTransfers.TopDefinitionListPointers) / sizeof(ushort);

    extension(SamusRenderingRomData.TileTransfers)
    {
        /// <inheritdoc cref="SamusRenderingRomDataTileTransfersConstants.AnimationRecordByteCount"/>
        internal static int AnimationRecordByteCount => SamusRenderingRomDataTileTransfersConstants.AnimationRecordByteCount;
        /// <inheritdoc cref="SamusRenderingRomDataTileTransfersConstants.BottomDefinitionSetCount"/>
        internal static int BottomDefinitionSetCount => SamusRenderingRomDataTileTransfersConstants.BottomDefinitionSetCount;
        /// <inheritdoc cref="SamusRenderingRomDataTileTransfersConstants.TopDefinitionSetCount"/>
        internal static int TopDefinitionSetCount => SamusRenderingRomDataTileTransfersConstants.TopDefinitionSetCount;
    }
}

/// <summary>Cartridge values of <see cref="SamusSpecialSequenceRomData.SuitPickup"/> that only verification reads.</summary>
internal static class SamusSpecialSequenceRomDataSuitPickupConstants
{
    /// <summary><c>$88:E3C9</c>, 128-byte symmetric light-beam curve.</summary>
    public const int BeamCurve = 0x88e3c9;

    extension(SamusSpecialSequenceRomData.SuitPickup)
    {
        /// <inheritdoc cref="SamusSpecialSequenceRomDataSuitPickupConstants.BeamCurve"/>
        internal static int BeamCurve => SamusSpecialSequenceRomDataSuitPickupConstants.BeamCurve;
    }
}

/// <summary>Cartridge values of <see cref="SamusXrayRomData.Palette"/> that only verification reads.</summary>
internal static class SamusXrayRomDataPaletteConstants
{
    /// <summary><c>$91:D727</c>, suit-indexed normal Samus palette pointers.</summary>
    /// <remarks>Alias of <see cref="SamusPaletteRomDataCommonConstants.NormalSuitPointers"/>;
    /// the three-word proof is issue #859 / #625.</remarks>
    public const int NormalSuitPointers = SamusPaletteRomDataCommonConstants.NormalSuitPointers;
    /// <summary>Bank expanded around a normal-suit palette pointer.</summary>
    public const int PaletteBank = SamusPaletteRomData.Banks.Palette;
    /// <summary>Number of colors copied when restoring the normal suit palette.</summary>
    public const int SuitColorCount = SamusPaletteRomData.Common.ColorsPerObjPalette;
    /// <summary><c>$9B:A3C0</c>, X-ray visor palette words.</summary>
    /// <remarks>Alias of <see cref="SamusPaletteRomData.Visor.Colors"/>;
    /// the six-word proof is issue #865 / #625.</remarks>
    public const int VisorWords = SamusPaletteRomData.Visor.Colors;

    extension(SamusXrayRomData.Palette)
    {
        /// <inheritdoc cref="SamusXrayRomDataPaletteConstants.NormalSuitPointers"/>
        internal static int NormalSuitPointers => SamusXrayRomDataPaletteConstants.NormalSuitPointers;
        /// <inheritdoc cref="SamusXrayRomDataPaletteConstants.PaletteBank"/>
        internal static int PaletteBank => SamusXrayRomDataPaletteConstants.PaletteBank;
        /// <inheritdoc cref="SamusXrayRomDataPaletteConstants.SuitColorCount"/>
        internal static int SuitColorCount => SamusXrayRomDataPaletteConstants.SuitColorCount;
        /// <inheritdoc cref="SamusXrayRomDataPaletteConstants.VisorWords"/>
        internal static int VisorWords => SamusXrayRomDataPaletteConstants.VisorWords;
    }
}

/// <summary>Cartridge values of <see cref="SamusXrayRomData.Window"/> that only verification reads.</summary>
internal static class SamusXrayRomDataWindowConstants
{
    /// <summary><c>$91:C9D4</c>, 129 absolute tangent words in unsigned 8.8 format.</summary>
    /// <remarks>
    /// Physical alias of <see cref="AbsoluteTangentDefinitions.Sample"/>.
    /// The exact bounded stock algorithm and all 129-word parity proof are
    /// documented there. Investigation: #625 / #906.
    /// </remarks>
    public const int AbsoluteTangentTable = 0x91c9d4;
    /// <summary>Number of words in the inclusive quarter-turn tangent table.</summary>
    public const int AbsoluteTangentWordCount = 129;

    extension(SamusXrayRomData.Window)
    {
        /// <inheritdoc cref="SamusXrayRomDataWindowConstants.AbsoluteTangentTable"/>
        internal static int AbsoluteTangentTable => SamusXrayRomDataWindowConstants.AbsoluteTangentTable;
        /// <inheritdoc cref="SamusXrayRomDataWindowConstants.AbsoluteTangentWordCount"/>
        internal static int AbsoluteTangentWordCount => SamusXrayRomDataWindowConstants.AbsoluteTangentWordCount;
    }
}

/// <summary>Cartridge values of <see cref="SaveRamLayout"/> that only verification reads.</summary>
internal static class SaveRamLayoutConstants
{
    public const int ButtonConfigWordCount = 11;
    /// <summary>Size of the unassigned $7E:D8F0-$7E:D8F7 allocation.</summary>
    public const int ProgressionPaddingByteCount = 8;
    /// <summary>Unassigned eight-byte SRAM-mirror allocation at $7E:D8F0-$7E:D8F7.</summary>
    public const int ProgressionPaddingWramAddress = SaveRamLayout.OpenedDoorBitsWramAddress + Bank80SystemState.DoorBitByteCount;
    public const int SelectedSlotComplementOffset = SaveRamLayout.SelectedSlotOffset + WordByteCount;
    public const int WordByteCount = 2;

    extension(SaveRamLayout)
    {
        /// <inheritdoc cref="SaveRamLayoutConstants.ButtonConfigWordCount"/>
        internal static int ButtonConfigWordCount => SaveRamLayoutConstants.ButtonConfigWordCount;
        /// <inheritdoc cref="SaveRamLayoutConstants.ProgressionPaddingByteCount"/>
        internal static int ProgressionPaddingByteCount => SaveRamLayoutConstants.ProgressionPaddingByteCount;
        /// <inheritdoc cref="SaveRamLayoutConstants.ProgressionPaddingWramAddress"/>
        internal static int ProgressionPaddingWramAddress => SaveRamLayoutConstants.ProgressionPaddingWramAddress;
        /// <inheritdoc cref="SaveRamLayoutConstants.SelectedSlotComplementOffset"/>
        internal static int SelectedSlotComplementOffset => SaveRamLayoutConstants.SelectedSlotComplementOffset;
        /// <inheritdoc cref="SaveRamLayoutConstants.WordByteCount"/>
        internal static int WordByteCount => SaveRamLayoutConstants.WordByteCount;
    }
}

/// <summary>Cartridge values of <see cref="SaveStationAnimationDefinitions"/> that only verification reads.</summary>
internal static class SaveStationAnimationDefinitionsConstants
{
    /// <summary>Native address of the compiled loop-count operand for parity verification.</summary>
    public const int NativeSaveAnimationLoopsAddress = 0x84aff9;

    extension(SaveStationAnimationDefinitions)
    {
        /// <inheritdoc cref="SaveStationAnimationDefinitionsConstants.NativeSaveAnimationLoopsAddress"/>
        internal static int NativeSaveAnimationLoopsAddress => SaveStationAnimationDefinitionsConstants.NativeSaveAnimationLoopsAddress;
    }
}

/// <summary>Cartridge values of <see cref="SbugInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class SbugInstructionProgramDefinitionsConstants
{
    /// <summary><c>$A3:A0E9</c>, down-facing animation loop.</summary>
    public const ushort Down = 0xa0e9;
    /// <summary><c>$A3:A0D5</c>, down-left-facing animation loop.</summary>
    public const ushort DownLeft = 0xa0d5;
    /// <summary><c>$A3:A0FD</c>, down-right-facing animation loop.</summary>
    public const ushort DownRight = 0xa0fd;
    /// <summary><c>$A3:A0C1</c>, left-facing animation loop.</summary>
    public const ushort Left = 0xa0c1;
    /// <summary><c>$A3:A099</c>, up-facing animation loop.</summary>
    public const ushort Up = 0xa099;
    /// <summary><c>$A3:A0AD</c>, up-left-facing animation loop.</summary>
    public const ushort UpLeft = 0xa0ad;
    /// <summary><c>$A3:A085</c>, up-right-facing animation loop.</summary>
    public const ushort UpRight = 0xa085;

    extension(SbugInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.Down"/>
        internal static ushort Down => SbugInstructionProgramDefinitionsConstants.Down;
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.DownLeft"/>
        internal static ushort DownLeft => SbugInstructionProgramDefinitionsConstants.DownLeft;
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.DownRight"/>
        internal static ushort DownRight => SbugInstructionProgramDefinitionsConstants.DownRight;
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.Left"/>
        internal static ushort Left => SbugInstructionProgramDefinitionsConstants.Left;
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.Up"/>
        internal static ushort Up => SbugInstructionProgramDefinitionsConstants.Up;
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.UpLeft"/>
        internal static ushort UpLeft => SbugInstructionProgramDefinitionsConstants.UpLeft;
        /// <inheritdoc cref="SbugInstructionProgramDefinitionsConstants.UpRight"/>
        internal static ushort UpRight => SbugInstructionProgramDefinitionsConstants.UpRight;
    }
}

/// <summary>Cartridge values of <see cref="SciserInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class SciserInstructionProgramDefinitionsConstants
{
    /// <summary>The final elevator return opcode immediately before Sciser's palette.</summary>
    public const ushort AdjacentPreviousCode = 0x95eb;

    extension(SciserInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="SciserInstructionProgramDefinitionsConstants.AdjacentPreviousCode"/>
        internal static ushort AdjacentPreviousCode => SciserInstructionProgramDefinitionsConstants.AdjacentPreviousCode;
    }
}

/// <summary>Cartridge values of <see cref="ShaktoolInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class ShaktoolInstructionProgramDefinitionsConstants
{
    /// <summary><c>InstList_Shaktool_Head_AimingDownLeft</c> at $AA:DADC.</summary>
    public const ushort HeadAimingDownLeft = 0xdadc;
    /// <summary><c>InstList_Shaktool_Head_AimingDownRight</c> at $AA:DACC.</summary>
    public const ushort HeadAimingDownRight = 0xdacc;
    /// <summary><c>InstList_Shaktool_Head_AimingRight</c> at $AA:DAC4.</summary>
    public const ushort HeadAimingRight = 0xdac4;
    /// <summary><c>InstList_Shaktool_Head_AimingUp</c> at $AA:DAB4.</summary>
    public const ushort HeadAimingUp = 0xdab4;
    /// <summary><c>InstList_Shaktool_Head_AimingUpLeft</c> at $AA:DAAC.</summary>
    public const ushort HeadAimingUpLeft = 0xdaac;
    /// <summary><c>InstList_Shaktool_Head_AimingUpRight</c> at $AA:DABC.</summary>
    public const ushort HeadAimingUpRight = 0xdabc;

    extension(ShaktoolInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="ShaktoolInstructionProgramDefinitionsConstants.HeadAimingDownLeft"/>
        internal static ushort HeadAimingDownLeft => ShaktoolInstructionProgramDefinitionsConstants.HeadAimingDownLeft;
        /// <inheritdoc cref="ShaktoolInstructionProgramDefinitionsConstants.HeadAimingDownRight"/>
        internal static ushort HeadAimingDownRight => ShaktoolInstructionProgramDefinitionsConstants.HeadAimingDownRight;
        /// <inheritdoc cref="ShaktoolInstructionProgramDefinitionsConstants.HeadAimingRight"/>
        internal static ushort HeadAimingRight => ShaktoolInstructionProgramDefinitionsConstants.HeadAimingRight;
        /// <inheritdoc cref="ShaktoolInstructionProgramDefinitionsConstants.HeadAimingUp"/>
        internal static ushort HeadAimingUp => ShaktoolInstructionProgramDefinitionsConstants.HeadAimingUp;
        /// <inheritdoc cref="ShaktoolInstructionProgramDefinitionsConstants.HeadAimingUpLeft"/>
        internal static ushort HeadAimingUpLeft => ShaktoolInstructionProgramDefinitionsConstants.HeadAimingUpLeft;
        /// <inheritdoc cref="ShaktoolInstructionProgramDefinitionsConstants.HeadAimingUpRight"/>
        internal static ushort HeadAimingUpRight => ShaktoolInstructionProgramDefinitionsConstants.HeadAimingUpRight;
    }
}

/// <summary>Cartridge values of <see cref="ShaktoolSegmentDefinitions"/> that only verification reads.</summary>
internal static class ShaktoolSegmentDefinitionsConstants
{
    /// <summary>$AA:DEDB ShaktoolPieceData_functionPointer, seven callbacks also used by reset.</summary>
    public const int NativeCallbackAddress = 0xaadedb;
    /// <summary>$AA:DEB1 ShaktoolPieceData_initialNeighborAngle, seven initial joint angles.</summary>
    public const int NativeInitialAngleAddress = 0xaadeb1;
    /// <summary>$AA:DEBF ShaktoolPieceData_initialInstListPointer, seven initial lists.</summary>
    public const int NativeInstructionAddress = 0xaadebf;
    /// <summary>$AA:DECD ShaktoolPieceData_layerControl, seven drawing layers.</summary>
    public const int NativeLayerAddress = 0xaadecd;
    /// <summary>$AA:DEA3 ShaktoolPieceData_RAMOffset, seven owner-relative enemy offsets.</summary>
    public const int NativeOwnerOffsetAddress = 0xaadea3;
    /// <summary>$AA:DE95 ShaktoolPieceData_properties, seven property words.</summary>
    public const int NativePropertiesAddress = 0xaade95;

    extension(ShaktoolSegmentDefinitions)
    {
        /// <inheritdoc cref="ShaktoolSegmentDefinitionsConstants.NativeCallbackAddress"/>
        internal static int NativeCallbackAddress => ShaktoolSegmentDefinitionsConstants.NativeCallbackAddress;
        /// <inheritdoc cref="ShaktoolSegmentDefinitionsConstants.NativeInitialAngleAddress"/>
        internal static int NativeInitialAngleAddress => ShaktoolSegmentDefinitionsConstants.NativeInitialAngleAddress;
        /// <inheritdoc cref="ShaktoolSegmentDefinitionsConstants.NativeInstructionAddress"/>
        internal static int NativeInstructionAddress => ShaktoolSegmentDefinitionsConstants.NativeInstructionAddress;
        /// <inheritdoc cref="ShaktoolSegmentDefinitionsConstants.NativeLayerAddress"/>
        internal static int NativeLayerAddress => ShaktoolSegmentDefinitionsConstants.NativeLayerAddress;
        /// <inheritdoc cref="ShaktoolSegmentDefinitionsConstants.NativeOwnerOffsetAddress"/>
        internal static int NativeOwnerOffsetAddress => ShaktoolSegmentDefinitionsConstants.NativeOwnerOffsetAddress;
        /// <inheritdoc cref="ShaktoolSegmentDefinitionsConstants.NativePropertiesAddress"/>
        internal static int NativePropertiesAddress => ShaktoolSegmentDefinitionsConstants.NativePropertiesAddress;
    }
}

/// <summary>Cartridge values of <see cref="SharedCrawlerInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class SharedCrawlerInstructionProgramDefinitionsConstants
{
    /// <summary>The first word of the adjacent initial-list pointer table at $A3:E2CC.</summary>
    public const ushort AdjacentInitialSelectorTable = 0xe2cc;

    extension(SharedCrawlerInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="SharedCrawlerInstructionProgramDefinitionsConstants.AdjacentInitialSelectorTable"/>
        internal static ushort AdjacentInitialSelectorTable => SharedCrawlerInstructionProgramDefinitionsConstants.AdjacentInitialSelectorTable;
    }
}

/// <summary>Cartridge values of <see cref="ShitroidInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class ShitroidInstructionProgramDefinitionsConstants
{
    /// <summary>The first callback implementation after the programs, at $A9:F990.</summary>
    public const ushort FirstAdjacentCallbackCode = 0xf990;

    extension(ShitroidInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="ShitroidInstructionProgramDefinitionsConstants.FirstAdjacentCallbackCode"/>
        internal static ushort FirstAdjacentCallbackCode => ShitroidInstructionProgramDefinitionsConstants.FirstAdjacentCallbackCode;
    }
}

/// <summary>Cartridge values of <see cref="SlopeHeightDefinitions"/> that only verification reads.</summary>
internal static class SlopeHeightDefinitionsConstants
{
    /// <summary>
    /// $94:8B2B SlopeDefinitions_SlopeTopXOffsetByYPixel: top Y offset at column X,
    /// despite the swapped-axis native label. Thirty-two sixteen-column profiles.
    /// </summary>
    public const int ReferenceAddress = 0x948b2b;

    extension(SlopeHeightDefinitions)
    {
        /// <inheritdoc cref="SlopeHeightDefinitionsConstants.ReferenceAddress"/>
        internal static int ReferenceAddress => SlopeHeightDefinitionsConstants.ReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="SmCompressionFormat"/> that only verification reads.</summary>
internal static class SmCompressionFormatConstants
{
    public const int MaximumLongLength = 1024;
    public const int MaximumShortLength = 32;

    extension(SmCompressionFormat)
    {
        /// <inheritdoc cref="SmCompressionFormatConstants.MaximumLongLength"/>
        internal static int MaximumLongLength => SmCompressionFormatConstants.MaximumLongLength;
        /// <inheritdoc cref="SmCompressionFormatConstants.MaximumShortLength"/>
        internal static int MaximumShortLength => SmCompressionFormatConstants.MaximumShortLength;
    }
}

/// <summary>Cartridge values of <see cref="SnesPpuLayout"/> that only verification reads.</summary>
internal static class SnesPpuLayoutConstants
{
    /// <summary>Room scanlines below the HUD.</summary>
    public const int GameplayViewportHeightPixels = SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels;

    extension(SnesPpuLayout)
    {
        /// <inheritdoc cref="SnesPpuLayoutConstants.GameplayViewportHeightPixels"/>
        internal static int GameplayViewportHeightPixels => SnesPpuLayoutConstants.GameplayViewportHeightPixels;
    }
}

/// <summary>Cartridge values of <see cref="SoundEffectLibrary1Sounds"/> that only verification reads.</summary>
internal static class SoundEffectLibrary1SoundsConstants
{
    /// <summary>Uncharged Power Beam projectile launch.</summary>
    public static readonly SoundEffectId PowerBeam = new(SoundEffectLibrary.Library1, 0x0b);

    extension(SoundEffectLibrary1Sounds)
    {
        /// <inheritdoc cref="SoundEffectLibrary1SoundsConstants.PowerBeam"/>
        internal static SoundEffectId PowerBeam => SoundEffectLibrary1SoundsConstants.PowerBeam;
    }
}

/// <summary>Cartridge values of <see cref="SpacetimeBeamCopyDefinitions"/> that only verification reads.</summary>
internal static class SpacetimeBeamCopyDefinitionsConstants
{
    /// <summary>$00:FFFF ends the immutable source before the long word carries into low WRAM.</summary>
    public const int LastSourceAddress = 0x00ffff;

    extension(SpacetimeBeamCopyDefinitions)
    {
        /// <inheritdoc cref="SpacetimeBeamCopyDefinitionsConstants.LastSourceAddress"/>
        internal static int LastSourceAddress => SpacetimeBeamCopyDefinitionsConstants.LastSourceAddress;
    }
}

/// <summary>Cartridge values of <see cref="SpcDriverData.Ram"/> that only verification reads.</summary>
internal static class SpcDriverDataRamConstants
{
    public const int MusicTrackPointerTable = 0x5820;

    extension(SpcDriverData.Ram)
    {
        /// <inheritdoc cref="SpcDriverDataRamConstants.MusicTrackPointerTable"/>
        internal static int MusicTrackPointerTable => SpcDriverDataRamConstants.MusicTrackPointerTable;
    }
}

/// <summary>Cartridge values of <see cref="SpcMusicTables"/> that only verification reads.</summary>
internal static class SpcMusicTablesConstants
{
    /// <summary>$CF:8A6E uploaded driver kBaseNoteFreqs, thirteen little-endian pitch words.</summary>
    public const int BaseNoteReferenceAddress = 0xcf8a6e;
    /// <summary>$CF:87A8 uploaded driver kEffectByteLength, opcode E0 through FE.</summary>
    public const int EffectLengthReferenceAddress = 0xcf87a8;
    /// <summary>$CF:80EC uploaded driver kNoteGateOffPct, eight unsigned gate fractions.</summary>
    public const int NoteGateReferenceAddress = 0xcf80ec;
    /// <summary>$CF:80F4 uploaded driver kNoteVol, sixteen unsigned volume fractions.</summary>
    public const int NoteVolumeReferenceAddress = 0xcf80f4;

    extension(SpcMusicTables)
    {
        /// <inheritdoc cref="SpcMusicTablesConstants.BaseNoteReferenceAddress"/>
        internal static int BaseNoteReferenceAddress => SpcMusicTablesConstants.BaseNoteReferenceAddress;
        /// <inheritdoc cref="SpcMusicTablesConstants.EffectLengthReferenceAddress"/>
        internal static int EffectLengthReferenceAddress => SpcMusicTablesConstants.EffectLengthReferenceAddress;
        /// <inheritdoc cref="SpcMusicTablesConstants.NoteGateReferenceAddress"/>
        internal static int NoteGateReferenceAddress => SpcMusicTablesConstants.NoteGateReferenceAddress;
        /// <inheritdoc cref="SpcMusicTablesConstants.NoteVolumeReferenceAddress"/>
        internal static int NoteVolumeReferenceAddress => SpcMusicTablesConstants.NoteVolumeReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="SporeSpawnCeilingPlmProgramDefinitions"/> that only verification reads.</summary>
internal static class SporeSpawnCeilingPlmProgramDefinitionsConstants
{
    /// <summary><c>$84:AB27</c>: first byte of adjacent Botwoon setup code, not Spore Spawn data.</summary>
    public const ushort EndExclusive = 0xab27;

    extension(SporeSpawnCeilingPlmProgramDefinitions)
    {
        /// <inheritdoc cref="SporeSpawnCeilingPlmProgramDefinitionsConstants.EndExclusive"/>
        internal static ushort EndExclusive => SporeSpawnCeilingPlmProgramDefinitionsConstants.EndExclusive;
    }
}

/// <summary>Cartridge values of <see cref="SporeSpawnDeathColorDefinitions"/> that only verification reads.</summary>
internal static class SporeSpawnDeathColorDefinitionsConstants
{
    /// <summary>BG palette7 occupies decoded bytes$E0..FF in Palettes_6_GreenBlueBrinstar.</summary>
    public const int BackgroundPaletteByteOffset = 7 * 16 * sizeof(ushort);
    /// <summary>BG palette4 occupies decoded bytes$80..9F in Palettes_6_GreenBlueBrinstar.</summary>
    public const int LevelPaletteByteOffset = 4 * 16 * sizeof(ushort);
    /// <summary>$C2:B264, Palettes_6_GreenBlueBrinstar; selected by both Spore Spawn room states at $8F:9DD9/$9DF3.</summary>
    public const int OriginalRoomPaletteSource = 0xc2b264;

    extension(SporeSpawnDeathColorDefinitions)
    {
        /// <inheritdoc cref="SporeSpawnDeathColorDefinitionsConstants.BackgroundPaletteByteOffset"/>
        internal static int BackgroundPaletteByteOffset => SporeSpawnDeathColorDefinitionsConstants.BackgroundPaletteByteOffset;
        /// <inheritdoc cref="SporeSpawnDeathColorDefinitionsConstants.LevelPaletteByteOffset"/>
        internal static int LevelPaletteByteOffset => SporeSpawnDeathColorDefinitionsConstants.LevelPaletteByteOffset;
        /// <inheritdoc cref="SporeSpawnDeathColorDefinitionsConstants.OriginalRoomPaletteSource"/>
        internal static int OriginalRoomPaletteSource => SporeSpawnDeathColorDefinitionsConstants.OriginalRoomPaletteSource;
    }
}

/// <summary>Cartridge values of <see cref="SquareSlopeDefinitions"/> that only verification reads.</summary>
internal static class SquareSlopeDefinitionsConstants
{
    /// <summary>$A0:C435 SquareSlopeDefinitions_BankA0: solidity and quadrant identity.</summary>
    public const int EnemyReferenceAddress = 0xa0c435;
    /// <summary>$86:8729 SquareSlopeDefinitions_Bank86: identical enemy-projectile copy.</summary>
    public const int ProjectileReferenceAddress = 0x868729;

    extension(SquareSlopeDefinitions)
    {
        /// <inheritdoc cref="SquareSlopeDefinitionsConstants.EnemyReferenceAddress"/>
        internal static int EnemyReferenceAddress => SquareSlopeDefinitionsConstants.EnemyReferenceAddress;
        /// <inheritdoc cref="SquareSlopeDefinitionsConstants.ProjectileReferenceAddress"/>
        internal static int ProjectileReferenceAddress => SquareSlopeDefinitionsConstants.ProjectileReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="StokeProjectileInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class StokeProjectileInstructionProgramDefinitionsConstants
{
    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the projectile loop at $86:DB14.
    /// </summary>
    public const ushort LoopCommand = 0xdb14;

    extension(StokeProjectileInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="StokeProjectileInstructionProgramDefinitionsConstants.LoopCommand"/>
        internal static ushort LoopCommand => StokeProjectileInstructionProgramDefinitionsConstants.LoopCommand;
    }
}

/// <summary>Cartridge values of <see cref="SuitPickupBeamCurveDefinitions"/> that only verification reads.</summary>
internal static class SuitPickupBeamCurveDefinitionsConstants
{
    /// <summary>Native source address of the 128-byte contour at <c>$88:E3C9</c>.</summary>
    public const int NativeCurveAddress = 0x88e3c9;

    extension(SuitPickupBeamCurveDefinitions)
    {
        /// <inheritdoc cref="SuitPickupBeamCurveDefinitionsConstants.NativeCurveAddress"/>
        internal static int NativeCurveAddress => SuitPickupBeamCurveDefinitionsConstants.NativeCurveAddress;
    }
}

/// <summary>Cartridge values of <see cref="TitleLogoFadePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class TitleLogoFadePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete title-logo fade lasts 24 frames.</summary>
    public const int CycleFrames = TitleLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount * TitleLogoFadePaletteFxProgramMechanicsDefinitions.FrameDuration;
    /// <summary>The palette-FX definition at <c>$8D:E194</c>.</summary>
    public const ushort DefinitionPointer = 0xe194;

    extension(TitleLogoFadePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="TitleLogoFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => TitleLogoFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="TitleLogoFadePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => TitleLogoFadePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="TorizoBellyPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class TorizoBellyPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 52;

    extension(TorizoBellyPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="TorizoBellyPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => TorizoBellyPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}

/// <summary>Cartridge values of <see cref="TorizoChozoOrbInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class TorizoChozoOrbInstructionProgramDefinitionsConstants
{
    /// <summary><c>InstList_EnemyProjectile_TorizoChozoOrbs_Right</c> at $86:AB1D.</summary>
    public const ushort MovingRight = 0xab1d;

    extension(TorizoChozoOrbInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="TorizoChozoOrbInstructionProgramDefinitionsConstants.MovingRight"/>
        internal static ushort MovingRight => TorizoChozoOrbInstructionProgramDefinitionsConstants.MovingRight;
    }
}

/// <summary>Cartridge values of <see cref="TorizoFallingLeftInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class TorizoFallingLeftInstructionProgramDefinitionsConstants
{
    /// <summary>First byte after the falling-left list, $AA:BC96.</summary>
    public const ushort End = 0xbc96;

    extension(TorizoFallingLeftInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="TorizoFallingLeftInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => TorizoFallingLeftInstructionProgramDefinitionsConstants.End;
    }
}

/// <summary>Cartridge values of <see cref="TorizoInstructionVramTransferDefinitions"/> that only verification reads.</summary>
internal static class TorizoInstructionVramTransferDefinitionsConstants
{
    public const byte Bank = 0xaa;

    extension(TorizoInstructionVramTransferDefinitions)
    {
        /// <inheritdoc cref="TorizoInstructionVramTransferDefinitionsConstants.Bank"/>
        internal static byte Bank => TorizoInstructionVramTransferDefinitionsConstants.Bank;
    }
}

/// <summary>Cartridge values of <see cref="TorizoJumpBackInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class TorizoJumpBackInstructionProgramDefinitionsConstants
{
    public const ushort End = 0xc188;

    extension(TorizoJumpBackInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="TorizoJumpBackInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => TorizoJumpBackInstructionProgramDefinitionsConstants.End;
    }
}

/// <summary>Cartridge values of <see cref="TorizoJumpBackLeftInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class TorizoJumpBackLeftInstructionProgramDefinitionsConstants
{
    public const ushort End = 0xbd0e;

    extension(TorizoJumpBackLeftInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="TorizoJumpBackLeftInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => TorizoJumpBackLeftInstructionProgramDefinitionsConstants.End;
    }
}

/// <summary>Cartridge values of <see cref="TourianEntranceStatueInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class TourianEntranceStatueInstructionProgramDefinitionsConstants
{
    /// <summary>First unused visible-loop list immediately after the live programs.</summary>
    public const ushort AdjacentUnusedProgram = 0xd7bb;

    extension(TourianEntranceStatueInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="TourianEntranceStatueInstructionProgramDefinitionsConstants.AdjacentUnusedProgram"/>
        internal static ushort AdjacentUnusedProgram => TourianEntranceStatueInstructionProgramDefinitionsConstants.AdjacentUnusedProgram;
    }
}

/// <summary>Cartridge values of <see cref="TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete shared loop lasts 28 frames.</summary>
    public const int CycleFrames = 28;

    extension(TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}

/// <summary>Cartridge values of <see cref="TourianGlowPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class TourianGlowPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The unused Tourian 4 clone definition.</summary>
    public const ushort CloneDefinitionPointer = 0xf7a5;
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 110;
    /// <summary>The live Tourian 2 definition.</summary>
    public const ushort LiveDefinitionPointer = 0xf7a1;

    extension(TourianGlowPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="TourianGlowPaletteFxProgramMechanicsDefinitionsConstants.CloneDefinitionPointer"/>
        internal static ushort CloneDefinitionPointer => TourianGlowPaletteFxProgramMechanicsDefinitionsConstants.CloneDefinitionPointer;
        /// <inheritdoc cref="TourianGlowPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => TourianGlowPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="TourianGlowPaletteFxProgramMechanicsDefinitionsConstants.LiveDefinitionPointer"/>
        internal static ushort LiveDefinitionPointer => TourianGlowPaletteFxProgramMechanicsDefinitionsConstants.LiveDefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="TourianStatueGreyPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class TourianStatueGreyPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Handler frames from initial setup through the terminal delete.</summary>
    public const int FramesThroughDeletion = 1 + TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount * 8 + 1;

    extension(TourianStatueGreyPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="TourianStatueGreyPaletteFxProgramMechanicsDefinitionsConstants.FramesThroughDeletion"/>
        internal static int FramesThroughDeletion => TourianStatueGreyPaletteFxProgramMechanicsDefinitionsConstants.FramesThroughDeletion;
    }
}

/// <summary>Cartridge values of <see cref="TourianStatueRomData"/> that only verification reads.</summary>
internal static class TourianStatueRomDataConstants
{
    /// <summary>$86:BA6A eye glow projectile definition.</summary>
    public const ushort EyeGlow = 0xba6a;
    /// <summary>$86:BA94 ascending soul projectile definition.</summary>
    public const ushort Soul = 0xba94;

    extension(TourianStatueRomData)
    {
        /// <inheritdoc cref="TourianStatueRomDataConstants.EyeGlow"/>
        internal static ushort EyeGlow => TourianStatueRomDataConstants.EyeGlow;
        /// <inheritdoc cref="TourianStatueRomDataConstants.Soul"/>
        internal static ushort Soul => TourianStatueRomDataConstants.Soul;
    }
}

/// <summary>Cartridge values of <see cref="UnusedCinematicFadePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class UnusedCinematicFadePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete unused fade lasts 22 frames.</summary>
    public const int CycleFrames = UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.FrameCount * UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.FrameDuration;
    /// <summary>The unused palette-FX definition at <c>$8D:E1EC</c>.</summary>
    public const ushort DefinitionPointer = 0xe1ec;

    extension(UnusedCinematicFadePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="UnusedCinematicFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => UnusedCinematicFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="UnusedCinematicFadePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => UnusedCinematicFadePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete loop lasts 63 frames.</summary>
    public const int CycleFrames = 63;
    /// <summary><c>PalFxDef_Crateria2</c> at <c>$8D:FFE5</c>.</summary>
    public const ushort DefinitionPointer = 0xffe5;

    extension(UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="ViolaInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class ViolaInstructionProgramDefinitionsConstants
{
    /// <summary>The retail-unused X-flipped Viola program at $A3:B62B.</summary>
    public const ushort UnusedXFlipped = 0xb62b;

    extension(ViolaInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="ViolaInstructionProgramDefinitionsConstants.UnusedXFlipped"/>
        internal static ushort UnusedXFlipped => ViolaInstructionProgramDefinitionsConstants.UnusedXFlipped;
    }
}

/// <summary>Cartridge values of <see cref="WorkRobotPaletteTimingDefinitions"/> that only verification reads.</summary>
internal static class WorkRobotPaletteTimingDefinitionsConstants
{
    /// <summary>First timer word, at $A8:CCC9, in the six ten-byte palette records.</summary>
    public const int NativeFirstTimerAddress = 0xa8ccc9;
    /// <summary>The negative wrap marker at $A8:CCFD following the six records.</summary>
    public const int NativeTerminatorAddress = 0xa8ccfd;

    extension(WorkRobotPaletteTimingDefinitions)
    {
        /// <inheritdoc cref="WorkRobotPaletteTimingDefinitionsConstants.NativeFirstTimerAddress"/>
        internal static int NativeFirstTimerAddress => WorkRobotPaletteTimingDefinitionsConstants.NativeFirstTimerAddress;
        /// <inheritdoc cref="WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress"/>
        internal static int NativeTerminatorAddress => WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress;
    }
}

/// <summary>Cartridge values of <see cref="WorkRobotVisualDefinitions"/> that only verification reads.</summary>
internal static class WorkRobotVisualDefinitionsConstants
{
    public const int FrameCount = 27;

    extension(WorkRobotVisualDefinitions)
    {
        /// <inheritdoc cref="WorkRobotVisualDefinitionsConstants.FrameCount"/>
        internal static int FrameCount => WorkRobotVisualDefinitionsConstants.FrameCount;
    }
}

/// <summary>Cartridge values of <see cref="WreckedShipGhostInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class WreckedShipGhostInstructionProgramDefinitionsConstants
{
    /// <summary>The first non-program word after <c>InstList_Coven</c>, at $A8:9A9C.</summary>
    public const ushort FirstAdjacentConstant = 0x9a9c;

    extension(WreckedShipGhostInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="WreckedShipGhostInstructionProgramDefinitionsConstants.FirstAdjacentConstant"/>
        internal static ushort FirstAdjacentConstant => WreckedShipGhostInstructionProgramDefinitionsConstants.FirstAdjacentConstant;
    }
}

/// <summary>Cartridge values of <see cref="WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class WreckedShipGreenLightPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Powered Wrecked Ship palette-FX definition at $8D:F76D.</summary>
    public const ushort PoweredDefinition = 0xf76d;
    /// <summary>Alternate caller of the powered-light program at $8D:F771.</summary>
    public const ushort PoweredDefinitionAlternate = 0xf771;

    extension(WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="WreckedShipGreenLightPaletteFxProgramMechanicsDefinitionsConstants.PoweredDefinition"/>
        internal static ushort PoweredDefinition => WreckedShipGreenLightPaletteFxProgramMechanicsDefinitionsConstants.PoweredDefinition;
        /// <inheritdoc cref="WreckedShipGreenLightPaletteFxProgramMechanicsDefinitionsConstants.PoweredDefinitionAlternate"/>
        internal static ushort PoweredDefinitionAlternate => WreckedShipGreenLightPaletteFxProgramMechanicsDefinitionsConstants.PoweredDefinitionAlternate;
    }
}

/// <summary>Cartridge values of <see cref="WreckedShipTreadmillRomData"/> that only verification reads.</summary>
internal static class WreckedShipTreadmillRomDataConstants
{
    /// <summary>Second 32-byte graphics frame at $87:8E84.</summary>
    public const int Frame1Source = WreckedShipTreadmillRomData.Frame0Source + WreckedShipTreadmillRomData.TransferByteCount;
    /// <summary>Third 32-byte graphics frame at $87:8EA4.</summary>
    public const int Frame2Source = WreckedShipTreadmillRomData.Frame0Source + 2 * WreckedShipTreadmillRomData.TransferByteCount;
    /// <summary>Fourth 32-byte graphics frame at $87:8EC4.</summary>
    public const int Frame3Source = WreckedShipTreadmillRomData.Frame0Source + 3 * WreckedShipTreadmillRomData.TransferByteCount;

    extension(WreckedShipTreadmillRomData)
    {
        /// <inheritdoc cref="WreckedShipTreadmillRomDataConstants.Frame1Source"/>
        internal static int Frame1Source => WreckedShipTreadmillRomDataConstants.Frame1Source;
        /// <inheritdoc cref="WreckedShipTreadmillRomDataConstants.Frame2Source"/>
        internal static int Frame2Source => WreckedShipTreadmillRomDataConstants.Frame2Source;
        /// <inheritdoc cref="WreckedShipTreadmillRomDataConstants.Frame3Source"/>
        internal static int Frame3Source => WreckedShipTreadmillRomDataConstants.Frame3Source;
    }
}

/// <summary>Cartridge values of <see cref="YappingMawInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class YappingMawInstructionProgramDefinitionsConstants
{
    /// <summary><c>InstListPointers_YappingMaw</c>, adjacent selector data at $A8:A097.</summary>
    public const ushort AdjacentAttackSelectorTable = 0xa097;

    extension(YappingMawInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="YappingMawInstructionProgramDefinitionsConstants.AdjacentAttackSelectorTable"/>
        internal static ushort AdjacentAttackSelectorTable => YappingMawInstructionProgramDefinitionsConstants.AdjacentAttackSelectorTable;
    }
}

/// <summary>Cartridge values of <see cref="ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionFinalePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete one-shot finale lasts 300 frames.</summary>
    public const int CycleFrames = 300;
    /// <summary>The palette-FX definition at <c>$8D:E1CC</c>.</summary>
    public const ushort DefinitionPointer = 0xe1cc;

    extension(ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionFinalePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => ZebesExplosionFinalePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="ZebesExplosionFinalePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => ZebesExplosionFinalePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The palette-FX definition at <c>$8D:E1C8</c>.</summary>
    public const ushort DefinitionPointer = 0xe1c8;

    extension(ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete gunship reveal lasts 384 frames.</summary>
    public const int CycleFrames = ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameCount * ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameDuration;
    /// <summary>The palette-FX definition at <c>$8D:E1E4</c>.</summary>
    public const ushort DefinitionPointer = 0xe1e4;

    extension(ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete one-shot whiteout lasts 210 frames.</summary>
    public const int CycleFrames = 210;
    /// <summary>The space-whiteout definition at <c>$8D:E1D0</c>.</summary>
    public const ushort SpaceWhiteoutDefinitionPointer = 0xe1d0;
    /// <summary>The wide-background definition at <c>$8D:E1E8</c>.</summary>
    public const ushort WideExplosionBackgroundDefinitionPointer = 0xe1e8;

    extension(ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants.SpaceWhiteoutDefinitionPointer"/>
        internal static ushort SpaceWhiteoutDefinitionPointer => ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants.SpaceWhiteoutDefinitionPointer;
        /// <inheritdoc cref="ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants.WideExplosionBackgroundDefinitionPointer"/>
        internal static ushort WideExplosionBackgroundDefinitionPointer => ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants.WideExplosionBackgroundDefinitionPointer;
    }
}

/// <summary>Cartridge values of <see cref="ZebetiteInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class ZebetiteInstructionProgramDefinitionsConstants
{
    /// <summary><c>Spritemap_Zebetite_Big_HealthGreaterThanEqualTo800</c> at $A6:FE08.</summary>
    public const ushort FirstSpritemap = 0xfe08;
    /// <summary><c>InstList_Small_HealthLessThan200</c> at $A6:FE02.</summary>
    public const ushort SmallHealthBelow200 = 0xfe02;

    extension(ZebetiteInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="ZebetiteInstructionProgramDefinitionsConstants.FirstSpritemap"/>
        internal static ushort FirstSpritemap => ZebetiteInstructionProgramDefinitionsConstants.FirstSpritemap;
        /// <inheritdoc cref="ZebetiteInstructionProgramDefinitionsConstants.SmallHealthBelow200"/>
        internal static ushort SmallHealthBelow200 => ZebetiteInstructionProgramDefinitionsConstants.SmallHealthBelow200;
    }
}

/// <summary>Cartridge values of <see cref="ZeroInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class ZeroInstructionProgramDefinitionsConstants
{
    /// <summary>The retail-unused alternate upside-right program at $A3:982B.</summary>
    public const ushort UnusedAlternateUpsideRight = 0x982b;

    extension(ZeroInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="ZeroInstructionProgramDefinitionsConstants.UnusedAlternateUpsideRight"/>
        internal static ushort UnusedAlternateUpsideRight => ZeroInstructionProgramDefinitionsConstants.UnusedAlternateUpsideRight;
    }
}

/// <summary>Cartridge values of <see cref="ZoaAnimationDefinitions"/> that only verification reads.</summary>
internal static class ZoaAnimationDefinitionsConstants
{
    /// <summary>
    /// Left-shooting, left-rising, right-shooting, and right-rising instruction lists at
    /// <c>$A3:B40D-$A3:B414</c>, indexed by <see cref="ZoaAnimationSelector"/>.
    /// </summary>
    public const int ReferenceAddress = 0xa3b40d;

    extension(ZoaAnimationDefinitions)
    {
        /// <inheritdoc cref="ZoaAnimationDefinitionsConstants.ReferenceAddress"/>
        internal static int ReferenceAddress => ZoaAnimationDefinitionsConstants.ReferenceAddress;
    }
}

/// <summary>Cartridge values of <see cref="ZoaSpeedDefinitions"/> that only verification reads.</summary>
internal static class ZoaSpeedDefinitionsConstants
{
    /// <summary>$A3:B415, ZoaXSpeedTable: five whole/fraction records, including the trailing zero record.</summary>
    public const int ReferenceAddress = 0xa3b415;

    extension(ZoaSpeedDefinitions)
    {
        /// <inheritdoc cref="ZoaSpeedDefinitionsConstants.ReferenceAddress"/>
        internal static int ReferenceAddress => ZoaSpeedDefinitionsConstants.ReferenceAddress;
    }
}
