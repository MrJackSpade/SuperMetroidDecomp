using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Verification's native view of <see cref="RoomObjectKind"/> (#1414): the bank-$84 header pointer naming each member.
/// Core's values are arbitrary; a retail oracle, a native snapshot or a synthetic cartridge fixture translates
/// through this table. The generator of the compiled room PLM populations reads it too.
/// </summary>
internal static class RoomObjectNativeWords
{
    /// <summary>The bank-$84 header pointer of each PLM; the empty slot's zero names no object.</summary>
    internal static readonly NativeWordIdentities<RoomObjectKind> Headers = new(new Dictionary<RoomObjectKind, ushort>
    {
        [RoomObjectKind.RightwardsScrollExtension] = 0xb63b,
        [RoomObjectKind.LeftwardsScrollExtension] = 0xb63f,
        [RoomObjectKind.DownwardsScrollExtension] = 0xb643,
        [RoomObjectKind.UpwardsScrollExtension] = 0xb647,
        [RoomObjectKind.MapStation] = 0xb6d3,
        [RoomObjectKind.EnergyStation] = 0xb6df,
        [RoomObjectKind.MissileStation] = 0xb6eb,
        [RoomObjectKind.ScrollTrigger] = 0xb703,
        [RoomObjectKind.ElevatorPlatform] = 0xb70b,
        [RoomObjectKind.SaveStation] = 0xb76f,
        [RoomObjectKind.SpeedBoosterEscape] = 0xb8ac,
        [RoomObjectKind.BombTorizoGreyDoor] = 0xbaf4,
        [RoomObjectKind.WreckedShipAtticHeldPlace] = 0xbb05,
        [RoomObjectKind.DownwardGate] = 0xc82a,
        [RoomObjectKind.DownwardGateShotBlock] = 0xc836,
        [RoomObjectKind.GreyDoorFacingLeft] = 0xc842,
        [RoomObjectKind.GreyDoorFacingRight] = 0xc848,
        [RoomObjectKind.GreyDoorFacingUp] = 0xc84e,
        [RoomObjectKind.GreyDoorFacingDown] = 0xc854,
        [RoomObjectKind.YellowDoorFacingLeft] = 0xc85a,
        [RoomObjectKind.YellowDoorFacingRight] = 0xc860,
        [RoomObjectKind.YellowDoorFacingUp] = 0xc866,
        [RoomObjectKind.YellowDoorFacingDown] = 0xc86c,
        [RoomObjectKind.GreenDoorFacingLeft] = 0xc872,
        [RoomObjectKind.GreenDoorFacingRight] = 0xc878,
        [RoomObjectKind.GreenDoorFacingUp] = 0xc87e,
        [RoomObjectKind.GreenDoorFacingDown] = 0xc884,
        [RoomObjectKind.RedDoorFacingLeft] = 0xc88a,
        [RoomObjectKind.RedDoorFacingRight] = 0xc890,
        [RoomObjectKind.MotherBrainEscapeRoomGate] = 0xc8ca,
        [RoomObjectKind.MotherBrainGlass] = 0xd6de,
        [RoomObjectKind.BombTorizoHand] = 0xd6ea,
        [RoomObjectKind.NoobTube] = 0xd70c,
        [RoomObjectKind.MetroidRoomClearance] = 0xdb44,
        [RoomObjectKind.EyeDoorEyeFacingRight] = 0xdb48,
        [RoomObjectKind.EyeDoorFacingRight] = 0xdb4c,
        [RoomObjectKind.EyeDoorBottomFacingRight] = 0xdb52,
        [RoomObjectKind.EyeDoorEyeFacingLeft] = 0xdb56,
        [RoomObjectKind.EyeDoorFacingLeft] = 0xdb5a,
        [RoomObjectKind.EyeDoorBottomFacingLeft] = 0xdb60,
        [RoomObjectKind.DraygonCannonFacingRight] = 0xdf59,
        [RoomObjectKind.DraygonCannonFacingRightDestroyed] = 0xdf65,
        [RoomObjectKind.DraygonCannonFacingLeft] = 0xdf71,
        [RoomObjectKind.ExposedEnergyTank] = 0xeed7,
        [RoomObjectKind.ExposedMissileTank] = 0xeedb,
        [RoomObjectKind.ExposedSuperMissileTank] = 0xeedf,
        [RoomObjectKind.ExposedPowerBombTank] = 0xeee3,
        [RoomObjectKind.ExposedMorphBall] = 0xef23,
        [RoomObjectKind.ChozoMissileTank] = 0xef2f,
        [RoomObjectKind.ChozoSuperMissileTank] = 0xef33,
        [RoomObjectKind.ChozoPowerBombTank] = 0xef37,
        [RoomObjectKind.ChozoBombs] = 0xef3b,
        [RoomObjectKind.ChozoChargeBeam] = 0xef3f,
        [RoomObjectKind.ChozoIceBeam] = 0xef43,
        [RoomObjectKind.ChozoHiJumpBoots] = 0xef47,
        [RoomObjectKind.ChozoSpeedBooster] = 0xef4b,
        [RoomObjectKind.ChozoWaveBeam] = 0xef4f,
        [RoomObjectKind.ChozoSpazerBeam] = 0xef53,
        [RoomObjectKind.ChozoSpringBall] = 0xef57,
        [RoomObjectKind.ChozoVariaSuit] = 0xef5b,
        [RoomObjectKind.ChozoGravitySuit] = 0xef5f,
        [RoomObjectKind.ChozoXrayScope] = 0xef63,
        [RoomObjectKind.ChozoPlasmaBeam] = 0xef67,
        [RoomObjectKind.ChozoGrappleBeam] = 0xef6b,
        [RoomObjectKind.ChozoSpaceJump] = 0xef6f,
        [RoomObjectKind.ChozoScrewAttack] = 0xef73,
        [RoomObjectKind.ChozoReserveTank] = 0xef7b,
        [RoomObjectKind.ShotBlockEnergyTank] = 0xef7f,
        [RoomObjectKind.ShotBlockMissileTank] = 0xef83,
        [RoomObjectKind.ShotBlockSuperMissileTank] = 0xef87,
    });

    /// <summary>The bank-$84 header pointer of <paramref name="header"/>; zero for the empty slot.</summary>
    internal static ushort Word(RoomObjectKind header) => Headers.Word(header);

    /// <summary>The PLM a bank-$84 header pointer names; zero is the empty slot.</summary>
    /// <exception cref="InvalidDataException">The word names no PLM header.</exception>
    internal static RoomObjectKind Header(ushort word) => Headers.Decode(word, "bank-$84 PLM header");

    /// <summary>The PLM a bank-$84 header pointer names, when it names one.</summary>
    internal static bool TryHeader(int word, out RoomObjectKind header)
    {
        header = default;
        return word is >= 0 and <= ushort.MaxValue && Headers.TryDecode((ushort)word, out header);
    }
}
