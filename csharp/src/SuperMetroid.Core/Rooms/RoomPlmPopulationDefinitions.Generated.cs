#nullable enable
namespace SuperMetroid.Core.Rooms;

/// <summary>Ordered named room setup cases; regenerate with tools/generate-room-plm-population-definitions.ps1.</summary>
internal static partial class RoomPlmPopulationDefinitions
{
    internal static bool TryPlace(ushort pointer, Action<ushort, byte, byte, ushort>? place)
    {
        switch (pointer)
        {
            case 0x8000:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 30, 40, 0x92B0);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 30, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 30, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 30, 37, 0x8000);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingLeft, 142, 70, 0x0000);
                place?.Invoke(RoomPlmHeaders.YellowDoorFacingLeft, 142, 22, 0x0001);
                return true;
            case 0x8026:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 30, 40, 0x92B0);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 30, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 30, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 30, 37, 0x8000);
                place?.Invoke(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired, 8, 8, 0x0008);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 38, 0x9002);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 142, 70, 0x9003);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 142, 22, 0x9004);
                return true;
            case 0x8058:
                return true;
            case 0x805A:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 63, 11, 0x9389);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 67, 11, 0x938C);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 22, 13, 0x938F);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 23, 13, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 24, 13, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 25, 13, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 22, 11, 0x9396);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 23, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 24, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 25, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 14, 9, 0x9399);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 14, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 14, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 28, 10, 0x939C);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 28, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 28, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 28, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 28, 6, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 28, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 50, 11, 0x939F);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 60, 11, 0x939F);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 58, 17, 0x93A2);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 59, 17, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 60, 17, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 61, 17, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 62, 17, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 52, 14, 0x93A5);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 30, 54, 0x0005);
                return true;
            case 0x8104:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 63, 11, 0x9389);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 67, 11, 0x938C);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 22, 13, 0x938F);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 23, 13, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 24, 13, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 25, 13, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 22, 11, 0x9396);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 23, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 24, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 25, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 14, 9, 0x9399);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 14, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 14, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 28, 10, 0x939C);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 28, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 28, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 28, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 28, 6, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 28, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 50, 11, 0x939F);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 60, 11, 0x939F);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 58, 17, 0x93A2);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 59, 17, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 60, 17, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 61, 17, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 62, 17, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 52, 14, 0x93A5);
                place?.Invoke(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired, 8, 8, 0x000A);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x9006);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 17, 38, 0x9007);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 17, 54, 0x9008);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 30, 54, 0x9009);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingUp, 22, 77, 0x900A);
                return true;
            case 0x81CC:
                place?.Invoke(RoomPlmHeaders.ExposedPowerBombTank, 29, 7, 0x0000);
                return true;
            case 0x81D4:
                place?.Invoke(RoomPlmHeaders.SaveStation, 5, 11, 0x0001);
                return true;
            case 0x81DC:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 94, 54, 0x900B);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingLeft, 126, 70, 0x000C);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 2, 91, 0x0001);
                place?.Invoke(RoomPlmHeaders.ShotBlockMissileTank, 28, 3, 0x0002);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 1, 47, 0x0003);
                return true;
            case 0x81FC:
                return true;
            case 0x81FE:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 23, 15, 0x94C2);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 24, 15, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 22, 12, 0x94C7);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 23, 12, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 24, 12, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 25, 12, 0x8000);
                place?.Invoke(RoomPlmHeaders.YellowDoorFacingLeft, 46, 6, 0x000D);
                place?.Invoke(RoomPlmHeaders.YellowDoorFacingUp, 22, 45, 0x000E);
                return true;
            case 0x8230:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 13, 0x94FA);
                return true;
            case 0x8238:
                return true;
            case 0x823A:
                return true;
            case 0x823C:
                return true;
            case 0x823E:
                place?.Invoke(RoomPlmHeaders.YellowDoorFacingUp, 6, 13, 0x000F);
                return true;
            case 0x8246:
                return true;
            case 0x8248:
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 14, 9, 0x0004);
                return true;
            case 0x8250:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 13, 0x9658);
                place?.Invoke(RoomPlmHeaders.YellowDoorFacingDown, 6, 2, 0x0010);
                return true;
            case 0x825E:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 84, 12, 0x968C);
                place?.Invoke(RoomPlmHeaders.ExposedEnergyTank, 83, 8, 0x0005);
                return true;
            case 0x826C:
                return true;
            case 0x826E:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 29, 5, 0x9747);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 32, 5, 0x9744);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 29, 118, 0x974D);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 32, 118, 0x974A);
                place?.Invoke(RoomPlmHeaders.LeftwardsScrollExtension, 18, 133, 0x8000);
                place?.Invoke(RoomPlmHeaders.LeftwardsScrollExtension, 19, 133, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 133, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 134, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 135, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 136, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 20, 137, 0x9753);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 134, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 135, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 136, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 15, 137, 0x9750);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 134, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 135, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 136, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 13, 137, 0x9756);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 134, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 135, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 136, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 137, 0x9759);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 46, 6, 0x9011);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 134, 0x9012);
                place?.Invoke(RoomPlmHeaders.YellowDoorFacingLeft, 46, 118, 0x0013);
                return true;
            case 0x830C:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 29, 5, 0x9747);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 32, 5, 0x9744);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 29, 118, 0x974D);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 32, 118, 0x974A);
                place?.Invoke(RoomPlmHeaders.LeftwardsScrollExtension, 18, 133, 0x8000);
                place?.Invoke(RoomPlmHeaders.LeftwardsScrollExtension, 19, 133, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 133, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 134, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 135, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 136, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 20, 137, 0x9753);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 134, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 135, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 136, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 15, 137, 0x9750);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 134, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 135, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 136, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 13, 137, 0x9756);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 134, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 135, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 136, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 137, 0x9759);
                place?.Invoke(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired, 8, 8, 0x000C);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 46, 6, 0x9014);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 46, 118, 0x9015);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 30, 134, 0x9016);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 134, 0x9017);
                return true;
            case 0x83B6:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 4, 15, 0x97AB);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 6, 15, 0x97AB);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 5, 9, 0x97B0);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x9018);
                return true;
            case 0x83D0:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 4, 15, 0x97AB);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 6, 15, 0x97AB);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 5, 9, 0x97B0);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 46, 6, 0x0C19);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x0C1A);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 5, 26, 0x0006);
                return true;
            case 0x83F6:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 13, 0x9801);
                return true;
            case 0x83FE:
                place?.Invoke(RoomPlmHeaders.BombTorizoGreyDoor, 1, 6, 0x081B);
                place?.Invoke(RoomPlmHeaders.ChozoBombs, 12, 10, 0x0007);
                place?.Invoke(RoomPlmHeaders.BombTorizoHand, 13, 11, 0x0000);
                return true;
            case 0x8412:
                place?.Invoke(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired, 8, 8, 0x000E);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x181C);
                return true;
            case 0x8420:
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 46, 6, 0x001D);
                return true;
            case 0x8428:
                place?.Invoke(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired, 8, 8, 0x0010);
                return true;
            case 0x8430:
                return true;
            case 0x8432:
                place?.Invoke(RoomPlmHeaders.ExposedEnergyTank, 7, 42, 0x0008);
                return true;
            case 0x843A:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 13, 0x9966);
                return true;
            case 0x8442:
                return true;
            case 0x8444:
                place?.Invoke(RoomPlmHeaders.MapStation, 11, 10, 0x8000);
                return true;
            case 0x844C:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 67, 0x99F3);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 67, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 9, 70, 0x99F6);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 10, 70, 0x8000);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 13, 27, 0x0009);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 2, 27, 0x000A);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 14, 102, 0x001E);
                return true;
            case 0x8478:
                place?.Invoke(RoomPlmHeaders.ExposedSuperMissileTank, 59, 9, 0x000B);
                return true;
            case 0x8480:
                return true;
            case 0x8482:
                return true;
            case 0x8484:
                return true;
            case 0x8486:
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 4, 7, 0x000C);
                return true;
            case 0x848E:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 113, 0x9B46);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 16, 168, 0x9B4B);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 33, 168, 0x9B4B);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 13, 172, 0x9B52);
                place?.Invoke(RoomPlmHeaders.ElevatorPlatform, 6, 44, 0x8000);
                place?.Invoke(RoomPlmHeaders.ChozoPowerBombTank, 60, 122, 0x000D);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingRight, 1, 86, 0x001F);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingRight, 1, 70, 0x0020);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 14, 70, 0x0021);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 14, 102, 0x0022);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingRight, 1, 102, 0x0023);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 14, 118, 0x9024);
                return true;
            case 0x84D8:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 23, 11, 0x9B98);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 24, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.ChozoSuperMissileTank, 26, 135, 0x000E);
                return true;
            case 0x84EC:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 46, 6, 0x0C25);
                return true;
            case 0x84F4:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 41, 14, 0x9BF9);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 42, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 43, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 4, 19, 0x9C00);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 4, 15, 0x9BF9);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 46, 22, 0x0026);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 19, 27, 0x000F);
                place?.Invoke(RoomPlmHeaders.ExposedSuperMissileTank, 4, 6, 0x0010);
                return true;
            case 0x8526:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 14, 11, 0x9C32);
                place?.Invoke(RoomPlmHeaders.ChozoReserveTank, 11, 7, 0x0011);
                place?.Invoke(RoomPlmHeaders.ShotBlockMissileTank, 30, 7, 0x0012);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 25, 7, 0x0013);
                return true;
            case 0x8540:
                place?.Invoke(RoomPlmHeaders.MapStation, 5, 10, 0x8000);
                return true;
            case 0x8548:
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingRight, 1, 38, 0x0027);
                return true;
            case 0x8550:
                place?.Invoke(RoomPlmHeaders.MissileStation, 4, 10, 0x0014);
                return true;
            case 0x8558:
                place?.Invoke(RoomPlmHeaders.DownwardsScrollExtension, 69, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.LeftwardsScrollExtension, 69, 13, 0x8000);
                place?.Invoke(RoomPlmHeaders.LeftwardsScrollExtension, 70, 13, 0x8000);
                place?.Invoke(RoomPlmHeaders.LeftwardsScrollExtension, 71, 13, 0x8000);
                place?.Invoke(RoomPlmHeaders.LeftwardsScrollExtension, 72, 13, 0x8000);
                place?.Invoke(RoomPlmHeaders.LeftwardsScrollExtension, 73, 13, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 74, 13, 0x9D11);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 64, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 64, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 64, 10, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 64, 11, 0x9D14);
                place?.Invoke(RoomPlmHeaders.DownwardsScrollExtension, 75, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.DownwardsScrollExtension, 75, 10, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 75, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 74, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 73, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 72, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 71, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 70, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 69, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 68, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 68, 10, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 68, 11, 0x9D14);
                return true;
            case 0x85E4:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 46, 107, 0x9D84);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 32, 122, 0x9D8B);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 61, 87, 0x9D8E);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 64, 87, 0x9D91);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 31, 8, 0x9D96);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 33, 8, 0x9D99);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 36, 50, 0x0015);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 34, 103, 0x0016);
                place?.Invoke(RoomPlmHeaders.ChozoChargeBeam, 37, 118, 0x0017);
                place?.Invoke(RoomPlmHeaders.YellowDoorFacingLeft, 62, 70, 0x0028);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingLeft, 62, 102, 0x0029);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 62, 6, 0x002A);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingRight, 1, 150, 0x002B);
                return true;
            case 0x8634:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x0C2C);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingDown, 54, 3, 0x0C2D);
                return true;
            case 0x8642:
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingUp, 6, 46, 0x002E);
                return true;
            case 0x864A:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 14, 0x9E40);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 11, 0x9E49);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 30, 6, 0x0C2F);
                place?.Invoke(RoomPlmHeaders.ExposedPowerBombTank, 6, 23, 0x0018);
                return true;
            case 0x8664:
                place?.Invoke(RoomPlmHeaders.DownwardGate, 100, 55, 0x8000);
                place?.Invoke(RoomPlmHeaders.DownwardGateShotBlock, 100, 55, 0x0000);
                place?.Invoke(RoomPlmHeaders.YellowDoorFacingLeft, 30, 6, 0x0030);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 61, 24, 0x0019);
                return true;
            case 0x867E:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 31, 43, 0x9F05);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 35, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 36, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 37, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 40, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 41, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 42, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 43, 0x9F08);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 88, 33, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 87, 33, 0x9F0B);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 88, 10, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 87, 10, 0x9F0B);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 88, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 87, 38, 0x9F0E);
                place?.Invoke(RoomPlmHeaders.ExposedMorphBall, 69, 41, 0x001A);
                return true;
            case 0x86E6:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 31, 43, 0x9F05);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 35, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 36, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 37, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 40, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 41, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 42, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 43, 0x9F08);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 88, 33, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 87, 33, 0x9F0B);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 88, 10, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 87, 10, 0x9F0B);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 88, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 87, 38, 0x9F0E);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 38, 0x0C31);
                place?.Invoke(RoomPlmHeaders.ExposedPowerBombTank, 40, 42, 0x001B);
                return true;
            case 0x8754:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 4, 11, 0x9F5F);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 5, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 6, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 7, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 10, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 11, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 14, 6, 0x0032);
                return true;
            case 0x878C:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 39, 11, 0x9FB7);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 39, 38, 0x9FB7);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 46, 41, 0x001C);
                place?.Invoke(RoomPlmHeaders.ShotBlockEnergyTank, 28, 34, 0x001D);
                return true;
            case 0x87A6:
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingLeft, 94, 6, 0x0033);
                return true;
            case 0x87AE:
                return true;
            case 0x87B0:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 11, 0xA04A);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.ExposedEnergyTank, 5, 9, 0x001E);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingRight, 1, 6, 0x0034);
                return true;
            case 0x87D0:
                place?.Invoke(RoomPlmHeaders.ExposedSuperMissileTank, 7, 9, 0x001F);
                return true;
            case 0x87D8:
                place?.Invoke(RoomPlmHeaders.EnergyStation, 4, 10, 0x0020);
                return true;
            case 0x87E0:
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingRight, 1, 6, 0x0035);
                return true;
            case 0x87E8:
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 10, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 15, 11, 0xA104);
                place?.Invoke(RoomPlmHeaders.ExposedEnergyTank, 4, 9, 0x0021);
                return true;
            case 0x8802:
                place?.Invoke(RoomPlmHeaders.ChozoMissileTank, 4, 7, 0x0022);
                return true;
            case 0x880A:
                place?.Invoke(RoomPlmHeaders.DownwardGate, 17, 4, 0x8000);
                place?.Invoke(RoomPlmHeaders.DownwardGateShotBlock, 17, 4, 0x0002);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 22, 0x0C36);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 30, 22, 0x0C37);
                return true;
            case 0x8824:
                place?.Invoke(RoomPlmHeaders.ExposedEnergyTank, 11, 9, 0x0023);
                return true;
            case 0x882C:
                place?.Invoke(RoomPlmHeaders.SaveStation, 5, 11, 0x0000);
                return true;
            case 0x8834:
                return true;
            case 0x8836:
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 7, 9, 0x0024);
                place?.Invoke(RoomPlmHeaders.ShotBlockMissileTank, 5, 12, 0x0025);
                return true;
            case 0x8844:
                place?.Invoke(RoomPlmHeaders.SaveStation, 5, 11, 0x0001);
                return true;
            case 0x884C:
                place?.Invoke(RoomPlmHeaders.SaveStation, 5, 11, 0x0002);
                return true;
            case 0x8854:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 9, 106, 0xA28E);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 10, 106, 0x8000);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingRight, 1, 150, 0x0038);
                place?.Invoke(RoomPlmHeaders.YellowDoorFacingRight, 1, 102, 0x0039);
                return true;
            case 0x886E:
                place?.Invoke(RoomPlmHeaders.RedDoorFacingRight, 1, 6, 0x003A);
                return true;
            case 0x8876:
                place?.Invoke(RoomPlmHeaders.ChozoXrayScope, 5, 7, 0x0026);
                return true;
            case 0x887E:
                return true;
            case 0x8880:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 5, 94, 0xA36F);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 6, 94, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 16, 55, 0xA374);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 13, 55, 0xA379);
                place?.Invoke(RoomPlmHeaders.ElevatorPlatform, 6, 44, 0x8000);
                place?.Invoke(RoomPlmHeaders.DownwardGate, 38, 53, 0x8000);
                place?.Invoke(RoomPlmHeaders.DownwardGateShotBlock, 38, 53, 0x000A);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingRight, 1, 54, 0x003B);
                place?.Invoke(RoomPlmHeaders.YellowDoorFacingRight, 1, 86, 0x003C);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingRight, 1, 118, 0x003D);
                return true;
            case 0x88BE:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 14, 0xA3A9);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.ExposedPowerBombTank, 4, 19, 0x0027);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 30, 6, 0x0C3E);
                return true;
            case 0x88D8:
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 4, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 6, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 10, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 15, 12, 0xA3DA);
                place?.Invoke(RoomPlmHeaders.ChozoPowerBombTank, 20, 9, 0x0028);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 2, 8, 0x0029);
                return true;
            case 0x891C:
                return true;
            case 0x891E:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 16, 17, 0xA439);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 17, 17, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 18, 17, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 19, 17, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 16, 20, 0xA43E);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 17, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 18, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 19, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 17, 0xA439);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 20, 0xA43E);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 2, 17, 0xA439);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 2, 20, 0xA43E);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingLeft, 30, 6, 0x003F);
                return true;
            case 0x896E:
                place?.Invoke(RoomPlmHeaders.ChozoSpazerBeam, 11, 9, 0x002A);
                return true;
            case 0x8976:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 2, 11, 0xA4A2);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 2, 25, 0xA4A9);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 25, 0xA4AE);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 21, 25, 0xA4A9);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 22, 0x0040);
                return true;
            case 0x8996:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 14, 6, 0x0C41);
                place?.Invoke(RoomPlmHeaders.ShotBlockEnergyTank, 5, 4, 0x002B);
                return true;
            case 0x89A4:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 23, 11, 0xA50F);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 23, 14, 0xA514);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 4, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 6, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 10, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 11, 11, 0xA519);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 39, 12, 0xA51C);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 46, 12, 0xA51C);
                place?.Invoke(RoomPlmHeaders.ShotBlockMissileTank, 46, 8, 0x002C);
                return true;
            case 0x89F4:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 94, 6, 0x0C42);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x0C43);
                return true;
            case 0x8A02:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 3, 18, 0xA59C);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 4, 18, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 5, 18, 0x8000);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingLeft, 14, 6, 0x0044);
                place?.Invoke(RoomPlmHeaders.EyeDoorFacingLeft, 30, 22, 0x0045);
                place?.Invoke(RoomPlmHeaders.EyeDoorBottomFacingLeft, 30, 25, 0x0045);
                place?.Invoke(RoomPlmHeaders.EyeDoorEyeFacingLeft, 30, 23, 0x0045);
                return true;
            case 0x8A2E:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 30, 22, 0x0046);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 22, 0x0047);
                return true;
            case 0x8A3C:
                return true;
            case 0x8A3E:
                place?.Invoke(RoomPlmHeaders.EnergyStation, 4, 10, 0x002D);
                return true;
            case 0x8A46:
                place?.Invoke(RoomPlmHeaders.MissileStation, 9, 10, 0x002E);
                place?.Invoke(RoomPlmHeaders.EnergyStation, 7, 10, 0x002F);
                return true;
            case 0x8A54:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x9448);
                return true;
            case 0x8A5C:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 12, 0xA6D6);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 12, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 16, 9, 0xA6D9);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 16, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 16, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 18, 9, 0xA6DC);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 18, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 18, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 18, 6, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 19, 11, 0xA6DF);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 20, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 10, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 6, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 4, 0x8000);
                return true;
            case 0x8ACA:
                place?.Invoke(RoomPlmHeaders.ChozoVariaSuit, 7, 9, 0x0030);
                return true;
            case 0x8AD2:
                place?.Invoke(RoomPlmHeaders.SaveStation, 7, 11, 0x0003);
                return true;
            case 0x8ADA:
                place?.Invoke(RoomPlmHeaders.SaveStation, 7, 11, 0x0004);
                return true;
            case 0x8AE2:
                return true;
            case 0x8AE4:
                place?.Invoke(RoomPlmHeaders.ShotBlockMissileTank, 34, 28, 0x0031);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingLeft, 46, 22, 0x0049);
                return true;
            case 0x8AF2:
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 46, 6, 0x004A);
                return true;
            case 0x8AFA:
                place?.Invoke(RoomPlmHeaders.ElevatorPlatform, 6, 44, 0x8000);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingRight, 1, 54, 0x004B);
                place?.Invoke(RoomPlmHeaders.YellowDoorFacingRight, 1, 70, 0x004C);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingRight, 1, 86, 0x004D);
                return true;
            case 0x8B14:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 55, 45, 0xA860);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 56, 45, 0x8000);
                return true;
            case 0x8B22:
                return true;
            case 0x8B24:
                place?.Invoke(RoomPlmHeaders.ChozoIceBeam, 12, 7, 0x0032);
                return true;
            case 0x8B2C:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 15, 23, 0xA8EC);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 13, 23, 0xA8EF);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 17, 23, 0xA8F2);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 19, 23, 0xA8F5);
                return true;
            case 0x8B46:
                place?.Invoke(RoomPlmHeaders.ShotBlockMissileTank, 1, 8, 0x0033);
                return true;
            case 0x8B4E:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 158, 40, 0xA980);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 158, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 158, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 158, 37, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 158, 36, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 190, 41, 0xA987);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 190, 40, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 190, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 194, 41, 0xA98A);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 194, 40, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 194, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 194, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingUp, 198, 45, 0x004E);
                return true;
            case 0x8B9E:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingDown, 54, 2, 0x044F);
                place?.Invoke(RoomPlmHeaders.ExposedEnergyTank, 125, 6, 0x0034);
                return true;
            case 0x8BAC:
                place?.Invoke(RoomPlmHeaders.ChozoHiJumpBoots, 3, 10, 0x0035);
                return true;
            case 0x8BB4:
                place?.Invoke(RoomPlmHeaders.DownwardGate, 6, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.DownwardGateShotBlock, 6, 5, 0x000A);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 13, 9, 0x0036);
                return true;
            case 0x8BC8:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 16, 12, 0xAA75);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 15, 5, 0xAA70);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 28, 0xAA7C);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 11, 18, 0xAA7F);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 30, 6, 0x0C50);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 9, 6, 0x0037);
                place?.Invoke(RoomPlmHeaders.ExposedEnergyTank, 23, 8, 0x0038);
                return true;
            case 0x8BF4:
                place?.Invoke(RoomPlmHeaders.RedDoorFacingRight, 1, 6, 0x0051);
                return true;
            case 0x8BFC:
                place?.Invoke(RoomPlmHeaders.SaveStation, 7, 11, 0x0000);
                return true;
            case 0x8C04:
                place?.Invoke(RoomPlmHeaders.ExposedPowerBombTank, 7, 8, 0x0039);
                return true;
            case 0x8C0C:
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 14, 54, 0x0052);
                return true;
            case 0x8C14:
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 60, 9, 0x003A);
                return true;
            case 0x8C1C:
                place?.Invoke(RoomPlmHeaders.DownwardGate, 42, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.DownwardGateShotBlock, 42, 5, 0x0008);
                return true;
            case 0x8C2A:
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 67, 9, 0x003B);
                return true;
            case 0x8C32:
                return true;
            case 0x8C34:
                return true;
            case 0x8C36:
                place?.Invoke(RoomPlmHeaders.ChozoGrappleBeam, 4, 39, 0x003C);
                return true;
            case 0x8C3E:
                place?.Invoke(RoomPlmHeaders.ChozoReserveTank, 2, 7, 0x003D);
                place?.Invoke(RoomPlmHeaders.ShotBlockMissileTank, 7, 11, 0x003E);
                return true;
            case 0x8C4C:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 16, 5, 0xACB0);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 20, 10, 0x003F);
                return true;
            case 0x8C5A:
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingRight, 1, 6, 0x0053);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingLeft, 30, 6, 0x0054);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 20, 60, 0x0040);
                return true;
            case 0x8C6E:
                place?.Invoke(RoomPlmHeaders.SpeedBoosterEscape, 0, 0, 0x8000);
                place?.Invoke(RoomPlmHeaders.ShotBlockMissileTank, 188, 19, 0x0041);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 190, 22, 0x0055);
                return true;
            case 0x8C82:
                place?.Invoke(RoomPlmHeaders.ChozoSpeedBooster, 11, 6, 0x0042);
                return true;
            case 0x8C8A:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 13, 8, 0xADA7);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 16, 8, 0xADAA);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 77, 10, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 77, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 77, 12, 0xADAA);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 14, 22, 0x0056);
                return true;
            case 0x8CB0:
                place?.Invoke(RoomPlmHeaders.DownwardGate, 26, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.DownwardGateShotBlock, 26, 5, 0x0000);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 31, 9, 0x0043);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 62, 6, 0x0057);
                return true;
            case 0x8CCA:
                place?.Invoke(RoomPlmHeaders.ChozoWaveBeam, 11, 6, 0x0044);
                return true;
            case 0x8CD2:
                return true;
            case 0x8CD4:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 32, 41, 0xAE66);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 31, 41, 0xAE6B);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 25, 36, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 25, 37, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 25, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 25, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 25, 40, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 25, 41, 0xAE6E);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 38, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 38, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 38, 40, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 38, 41, 0xAE71);
                return true;
            case 0x8D1E:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 15, 23, 0xAEA9);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 17, 23, 0xAEAC);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 21, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 22, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 23, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 24, 0xAEB1);
                place?.Invoke(RoomPlmHeaders.DownwardGate, 7, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.DownwardGateShotBlock, 7, 20, 0x0000);
                place?.Invoke(RoomPlmHeaders.YellowDoorFacingRight, 17, 38, 0x0058);
                return true;
            case 0x8D56:
                return true;
            case 0x8D58:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 2, 11, 0xAF0F);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 3, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 4, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 11, 11, 0xAF0F);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 12, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 13, 11, 0x8000);
                return true;
            case 0x8D7E:
                return true;
            case 0x8D80:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 11, 0xAF6F);
                return true;
            case 0x8D88:
                place?.Invoke(RoomPlmHeaders.DownwardGate, 6, 21, 0x8000);
                place?.Invoke(RoomPlmHeaders.DownwardGateShotBlock, 6, 21, 0x0000);
                return true;
            case 0x8D96:
                return true;
            case 0x8D98:
                return true;
            case 0x8D9A:
                return true;
            case 0x8D9C:
                place?.Invoke(RoomPlmHeaders.EnergyStation, 7, 10, 0x0045);
                return true;
            case 0x8DA4:
                return true;
            case 0x8DA6:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 9, 12, 0xB0A7);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 10, 12, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 9, 16, 0xB0AC);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 10, 16, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 6, 19, 0xB0B1);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 7, 19, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 19, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 19, 0x8000);
                return true;
            case 0x8DD8:
                place?.Invoke(RoomPlmHeaders.MapStation, 5, 10, 0x8000);
                return true;
            case 0x8DE0:
                place?.Invoke(RoomPlmHeaders.SaveStation, 5, 11, 0x0001);
                return true;
            case 0x8DE8:
                return true;
            case 0x8DEA:
                return true;
            case 0x8DEC:
                place?.Invoke(RoomPlmHeaders.SaveStation, 7, 11, 0x0002);
                return true;
            case 0x8DF4:
                place?.Invoke(RoomPlmHeaders.SaveStation, 7, 11, 0x0003);
                return true;
            case 0x8DFC:
                place?.Invoke(RoomPlmHeaders.SaveStation, 5, 11, 0x0004);
                return true;
            case 0x8E04:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 12, 30, 0xB224);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 12, 35, 0xB22D);
                return true;
            case 0x8E12:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 71, 8, 0xB27D);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 72, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 71, 35, 0xB27D);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 72, 35, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 71, 41, 0xB280);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 72, 41, 0x8000);
                place?.Invoke(RoomPlmHeaders.ElevatorPlatform, 70, 42, 0x8000);
                return true;
            case 0x8E3E:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 4, 12, 0xB2D1);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 5, 12, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 6, 12, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 7, 12, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 12, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 12, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 10, 12, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 11, 12, 0x8000);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 13, 8, 0x0046);
                place?.Invoke(RoomPlmHeaders.ShotBlockSuperMissileTank, 21, 8, 0x0047);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 30, 22, 0x0859);
                return true;
            case 0x8E82:
                place?.Invoke(RoomPlmHeaders.DownwardGate, 52, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.DownwardGateShotBlock, 52, 5, 0x0008);
                return true;
            case 0x8E90:
                place?.Invoke(RoomPlmHeaders.EnergyStation, 8, 10, 0x0048);
                return true;
            case 0x8E98:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 14, 6, 0x005A);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 22, 0x005B);
                return true;
            case 0x8EA6:
                place?.Invoke(RoomPlmHeaders.EyeDoorFacingRight, 1, 6, 0x005C);
                place?.Invoke(RoomPlmHeaders.EyeDoorBottomFacingRight, 1, 9, 0x005C);
                place?.Invoke(RoomPlmHeaders.EyeDoorEyeFacingRight, 1, 7, 0x005C);
                return true;
            case 0x8EBA:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 33, 0xB3D9);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 33, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 27, 0xB3DC);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 27, 0x8000);
                return true;
            case 0x8ED4:
                return true;
            case 0x8ED6:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 48, 22, 0xB445);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 50, 22, 0xB448);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 45, 22, 0xB44B);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 43, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 43, 21, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 43, 22, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 43, 23, 0xB44E);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 56, 54, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 56, 55, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 56, 56, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 56, 57, 0xB451);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 61, 52, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 60, 52, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 59, 52, 0xB454);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 54, 0x0C5D);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 39, 27, 0x0049);
                return true;
            case 0x8F38:
                return true;
            case 0x8F3A:
                return true;
            case 0x8F3C:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 3, 16, 0xB4E0);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 4, 16, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 5, 16, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 6, 16, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 7, 16, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 16, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 16, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 10, 16, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 11, 16, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 12, 16, 0x8000);
                return true;
            case 0x8F7A:
                return true;
            case 0x8F7C:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 12, 9, 0xB547);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 12, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 12, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 12, 6, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 3, 18, 0xB54E);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 4, 18, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 5, 18, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 6, 18, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 7, 18, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 18, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 18, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 10, 18, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 47, 3, 0xB555);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 43, 7, 0x004A);
                return true;
            case 0x8FD2:
                place?.Invoke(RoomPlmHeaders.ExposedPowerBombTank, 12, 8, 0x004B);
                return true;
            case 0x8FDA:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 17, 72, 0xB5C3);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 70, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 8, 70, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 71, 0xB5C8);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 31, 72, 0xB5C3);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 36, 72, 0xB5C8);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 6, 14, 0xB5CD);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 7, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 6, 11, 0xB5D2);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 7, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.YellowDoorFacingUp, 38, 77, 0x005E);
                return true;
            case 0x9036:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 23, 13, 0xB612);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 24, 13, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 21, 8, 0xB615);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 22, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 23, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 24, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 25, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 26, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 79, 9, 0xB61A);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 81, 9, 0xB622);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 15, 11, 0xB61D);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 60, 9, 0xB622);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 63, 9, 0xB61A);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 20, 7, 0xB625);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 6, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 20, 4, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 9, 9, 0xB628);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 9, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 9, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 9, 6, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 9, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingRight, 17, 38, 0x005F);
                place?.Invoke(RoomPlmHeaders.ExposedPowerBombTank, 7, 8, 0x004C);
                return true;
            case 0x90C8:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x0C60);
                return true;
            case 0x90D0:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 11, 42, 0xB68D);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 41, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 40, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 37, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 15, 42, 0xB690);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 20, 42, 0xB695);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 8, 41, 0x004D);
                return true;
            case 0x9108:
                place?.Invoke(RoomPlmHeaders.ShotBlockEnergyTank, 14, 11, 0x004E);
                return true;
            case 0x9110:
                place?.Invoke(RoomPlmHeaders.ChozoScrewAttack, 11, 40, 0x004F);
                return true;
            case 0x9118:
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 31, 54, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 31, 55, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 31, 56, 0xB72D);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 45, 57, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 44, 57, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 43, 57, 0xB730);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 6, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 15, 9, 0xB737);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 27, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 26, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 25, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 24, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 23, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 22, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 21, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 20, 11, 0xB73C);
                place?.Invoke(RoomPlmHeaders.ExposedEnergyTank, 42, 81, 0x0050);
                return true;
            case 0x918C:
                place?.Invoke(RoomPlmHeaders.SaveStation, 7, 11, 0x0005);
                return true;
            case 0xC215:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 31, 45, 0xC9EC);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 65, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 65, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 65, 40, 0xC9F1);
                return true;
            case 0xC22F:
                return true;
            case 0xC231:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x0080);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 110, 6, 0x0081);
                place?.Invoke(RoomPlmHeaders.WreckedShipAttic, 8, 8, 0x8000);
                return true;
            case 0xC245:
                return true;
            case 0xC247:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 62, 87, 0xCB7A);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 81, 105, 0xCB7D);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 65, 87, 0xCB80);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 78, 105, 0xCB83);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 71, 109, 0xCB86);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 2, 89, 0x0080);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 65, 102, 0x0082);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 78, 70, 0x0083);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingUp, 70, 125, 0x0084);
                return true;
            case 0xC27F:
                return true;
            case 0xC281:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 16, 0xCC24);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 16, 0x8000);
                return true;
            case 0xC28F:
                return true;
            case 0xC291:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 64, 12, 0xCCC0);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 70, 12, 0xCCC5);
                place?.Invoke(RoomPlmHeaders.EyeDoorFacingLeft, 78, 6, 0x0085);
                place?.Invoke(RoomPlmHeaders.EyeDoorBottomFacingLeft, 78, 9, 0x0085);
                place?.Invoke(RoomPlmHeaders.EyeDoorEyeFacingLeft, 78, 7, 0x0085);
                return true;
            case 0xC2B1:
                return true;
            case 0xC2B3:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x0086);
                return true;
            case 0xC2BB:
                return true;
            case 0xC2BD:
                return true;
            case 0xC2BF:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 47, 7, 0xCE3D);
                return true;
            case 0xC2C7:
                return true;
            case 0xC2C9:
                place?.Invoke(RoomPlmHeaders.SaveStation, 7, 11, 0x0000);
                return true;
            case 0xC2D1:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 31, 45, 0xC9EC);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 65, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 65, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 65, 40, 0xC9F1);
                place?.Invoke(RoomPlmHeaders.ChozoReserveTank, 83, 11, 0x0081);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 60, 38, 0x0082);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 22, 0x9087);
                return true;
            case 0xC2FD:
                return true;
            case 0xC2FF:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x0C88);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 110, 6, 0x0C89);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingUp, 70, 14, 0x0C8A);
                place?.Invoke(RoomPlmHeaders.WreckedShipAttic, 8, 8, 0x8000);
                return true;
            case 0xC319:
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 45, 8, 0x0083);
                return true;
            case 0xC321:
                return true;
            case 0xC323:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 16, 0xCC24);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 16, 0x8000);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingRight, 1, 6, 0x008B);
                return true;
            case 0xC337:
                place?.Invoke(RoomPlmHeaders.ExposedEnergyTank, 3, 6, 0x0084);
                return true;
            case 0xC33F:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 64, 12, 0xCCC0);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 70, 12, 0xCCC5);
                return true;
            case 0xC34D:
                place?.Invoke(RoomPlmHeaders.MapStation, 5, 10, 0x8000);
                return true;
            case 0xC355:
                return true;
            case 0xC357:
                place?.Invoke(RoomPlmHeaders.ExposedSuperMissileTank, 2, 7, 0x0085);
                return true;
            case 0xC35F:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 47, 7, 0xCE3D);
                place?.Invoke(RoomPlmHeaders.ExposedSuperMissileTank, 56, 9, 0x0086);
                return true;
            case 0xC36D:
                place?.Invoke(RoomPlmHeaders.ChozoGravitySuit, 7, 9, 0x0087);
                return true;
            case 0xC375:
                place?.Invoke(RoomPlmHeaders.SaveStation, 7, 11, 0x0000);
                return true;
            case 0xC37D:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 3, 20, 0xCF4C);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 4, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 5, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 6, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 9, 20, 0xCF4C);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 10, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 11, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 12, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 5, 29, 0xCF4F);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 6, 29, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 7, 29, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 29, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 29, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 10, 29, 0x8000);
                place?.Invoke(RoomPlmHeaders.NoobTube, 2, 21, 0x0080);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 14, 38, 0x008C);
                return true;
            case 0xC3DF:
                return true;
            case 0xC3E1:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 5, 9, 0xCFB5);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 5, 16, 0xCFBC);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 5, 22, 0xCFC1);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 10, 3, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 10, 4, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 10, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 10, 6, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 10, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 10, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 10, 9, 0xCFC6);
                place?.Invoke(RoomPlmHeaders.DownwardGate, 22, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.DownwardGateShotBlock, 22, 5, 0x000A);
                return true;
            case 0xC42B:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 37, 41, 0xD012);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 30, 118, 0x008D);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 14, 53, 0x0088);
                place?.Invoke(RoomPlmHeaders.ExposedSuperMissileTank, 22, 40, 0x0089);
                return true;
            case 0xC445:
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 31, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 31, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 31, 40, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 31, 41, 0xD052);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 48, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 48, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 48, 40, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 48, 41, 0xD052);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 62, 38, 0x008E);
                return true;
            case 0xC47D:
                place?.Invoke(RoomPlmHeaders.ExposedEnergyTank, 31, 10, 0x008A);
                place?.Invoke(RoomPlmHeaders.ShotBlockMissileTank, 44, 29, 0x008B);
                return true;
            case 0xC48B:
                place?.Invoke(RoomPlmHeaders.DownwardGate, 14, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.DownwardGateShotBlock, 14, 7, 0x0008);
                return true;
            case 0xC499:
                return true;
            case 0xC49B:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 32, 6, 0xD135);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 30, 6, 0xD138);
                return true;
            case 0xC4A9:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 31, 0xD16A);
                place?.Invoke(RoomPlmHeaders.ExposedSuperMissileTank, 4, 38, 0x008C);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 11, 39, 0x008D);
                return true;
            case 0xC4BD:
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 32, 22, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 32, 23, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 32, 24, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 32, 25, 0xD1A0);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 22, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 23, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 15, 24, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 15, 25, 0xD1A0);
                return true;
            case 0xC4EF:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 7, 45, 0xD1D8);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 45, 0x8000);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingLeft, 30, 54, 0x008F);
                return true;
            case 0xC503:
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 31, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 31, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 31, 40, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 31, 41, 0xD216);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 29, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 29, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 29, 40, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 29, 41, 0xD219);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 44, 40, 0x008E);
                return true;
            case 0xC53B:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 14, 0xD24D);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 8, 17, 0xD24D);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 14, 22, 0x0090);
                return true;
            case 0xC54F:
                return true;
            case 0xC551:
                return true;
            case 0xC553:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x0C91);
                place?.Invoke(RoomPlmHeaders.ChozoPlasmaBeam, 25, 38, 0x008F);
                return true;
            case 0xC561:
                return true;
            case 0xC563:
                place?.Invoke(RoomPlmHeaders.ElevatorPlatform, 6, 44, 0x8000);
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 14, 70, 0x0092);
                return true;
            case 0xC571:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 46, 22, 0x0093);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingUp, 6, 45, 0x0094);
                return true;
            case 0xC57F:
                return true;
            case 0xC581:
                place?.Invoke(RoomPlmHeaders.MapStation, 11, 10, 0x8000);
                return true;
            case 0xC589:
                place?.Invoke(RoomPlmHeaders.SaveStation, 7, 11, 0x0001);
                return true;
            case 0xC591:
                return true;
            case 0xC593:
                return true;
            case 0xC595:
                return true;
            case 0xC597:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 3, 20, 0xD4BD);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 4, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 5, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 6, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 7, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 10, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 11, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 12, 20, 0x8000);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingDown, 6, 2, 0x0095);
                return true;
            case 0xC5DB:
                return true;
            case 0xC5DD:
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 6, 4, 0x0090);
                place?.Invoke(RoomPlmHeaders.ChozoReserveTank, 15, 4, 0x0091);
                return true;
            case 0xC5EB:
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 3, 7, 0x0092);
                place?.Invoke(RoomPlmHeaders.ExposedPowerBombTank, 25, 16, 0x0093);
                return true;
            case 0xC5F9:
                return true;
            case 0xC5FB:
                return true;
            case 0xC5FD:
                place?.Invoke(RoomPlmHeaders.RedDoorFacingRight, 1, 38, 0x0096);
                place?.Invoke(RoomPlmHeaders.ExposedMissileTank, 76, 9, 0x0094);
                place?.Invoke(RoomPlmHeaders.ExposedSuperMissileTank, 92, 8, 0x0095);
                return true;
            case 0xC611:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 14, 6, 0x0097);
                return true;
            case 0xC619:
                return true;
            case 0xC61B:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 6, 50, 0xD67D);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 7, 50, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 50, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 50, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 13, 12, 0xD67D);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 10, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 6, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 5, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 13, 4, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 18, 14, 0xD68A);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 18, 13, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 18, 12, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 18, 11, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 18, 10, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 18, 9, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 18, 8, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 18, 7, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 21, 43, 0xD695);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 22, 43, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 23, 43, 0x8000);
                return true;
            case 0xC6AD:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 5, 34, 0xD6C8);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 6, 34, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 7, 34, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 34, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 34, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 10, 18, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 5, 27, 0xD6CB);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 6, 27, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 7, 27, 0x8000);
                return true;
            case 0xC6E5:
                place?.Invoke(RoomPlmHeaders.ChozoSpringBall, 24, 22, 0x0096);
                return true;
            case 0xC6ED:
                return true;
            case 0xC6EF:
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 110, 6, 0x0098);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingLeft, 78, 38, 0x0099);
                place?.Invoke(RoomPlmHeaders.GreenDoorFacingLeft, 110, 22, 0x009A);
                return true;
            case 0xC703:
                place?.Invoke(RoomPlmHeaders.SaveStation, 5, 11, 0x0002);
                return true;
            case 0xC70B:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 4, 14, 0xD7DF);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 5, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 6, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 7, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 8, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 9, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 10, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.RightwardsScrollExtension, 11, 14, 0x8000);
                place?.Invoke(RoomPlmHeaders.EyeDoorFacingRight, 1, 38, 0x009B);
                place?.Invoke(RoomPlmHeaders.EyeDoorBottomFacingRight, 1, 41, 0x009B);
                place?.Invoke(RoomPlmHeaders.EyeDoorEyeFacingRight, 1, 39, 0x009B);
                place?.Invoke(RoomPlmHeaders.ShotBlockMissileTank, 28, 6, 0x0097);
                return true;
            case 0xC755:
                place?.Invoke(RoomPlmHeaders.ExposedEnergyTank, 50, 5, 0x0098);
                return true;
            case 0xC75D:
                place?.Invoke(RoomPlmHeaders.SaveStation, 7, 11, 0x0003);
                return true;
            case 0xC765:
                place?.Invoke(RoomPlmHeaders.MissileStation, 8, 10, 0x0099);
                return true;
            case 0xC76D:
                return true;
            case 0xC76F:
                return true;
            case 0xC771:
                return true;
            case 0xC773:
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 16, 41, 0xD951);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 11, 41, 0xD956);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 40, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 39, 0x8000);
                place?.Invoke(RoomPlmHeaders.UpwardsScrollExtension, 11, 38, 0x8000);
                place?.Invoke(RoomPlmHeaders.ScrollTrigger, 14, 41, 0xD95B);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 22, 0x009C);
                return true;
            case 0xC79F:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x049D);
                return true;
            case 0xC7A7:
                place?.Invoke(RoomPlmHeaders.ChozoSpaceJump, 4, 8, 0x009A);
                return true;
            case 0xC7AF:
                place?.Invoke(RoomPlmHeaders.EnergyStation, 8, 10, 0x009B);
                return true;
            case 0xC7B7:
                return true;
            case 0xC7B9:
                return true;
            case 0xC7BB:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 30, 6, 0x009E);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 22, 0x009F);
                place?.Invoke(RoomPlmHeaders.DraygonCannonFacingRightDestroyed, 2, 11, 0x8802);
                place?.Invoke(RoomPlmHeaders.DraygonCannonFacingRight, 2, 18, 0x8804);
                place?.Invoke(RoomPlmHeaders.DraygonCannonFacingLeft, 29, 15, 0x8806);
                place?.Invoke(RoomPlmHeaders.DraygonCannonFacingLeft, 29, 21, 0x8808);
                return true;
            case 0xC7E1:
                place?.Invoke(RoomPlmHeaders.ElevatorPlatform, 6, 44, 0x8000);
                return true;
            case 0xC7E9:
                place?.Invoke(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired, 8, 8, 0x0012);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x0CA0);
                return true;
            case 0xC7F7:
                place?.Invoke(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired, 8, 8, 0x0014);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 14, 22, 0x0CA1);
                return true;
            case 0xC805:
                place?.Invoke(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired, 8, 8, 0x0016);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 94, 6, 0x0CA2);
                return true;
            case 0xC813:
                place?.Invoke(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired, 8, 8, 0x0018);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingUp, 6, 30, 0x0CA3);
                return true;
            case 0xC821:
                return true;
            case 0xC823:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 30, 6, 0x90A4);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 6, 0x0CA5);
                return true;
            case 0xC831:
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingLeft, 62, 6, 0x90A6);
                return true;
            case 0xC839:
                place?.Invoke(RoomPlmHeaders.RedDoorFacingLeft, 14, 22, 0x00A7);
                return true;
            case 0xC841:
                place?.Invoke(RoomPlmHeaders.MissileStation, 8, 10, 0x009C);
                place?.Invoke(RoomPlmHeaders.EnergyStation, 6, 10, 0x009D);
                return true;
            case 0xC84F:
                place?.Invoke(RoomPlmHeaders.MotherBrainGlass, 9, 5, 0x8000);
                return true;
            case 0xC857:
                place?.Invoke(RoomPlmHeaders.EyeDoorFacingLeft, 62, 6, 0x00A8);
                place?.Invoke(RoomPlmHeaders.EyeDoorBottomFacingLeft, 62, 9, 0x00A8);
                place?.Invoke(RoomPlmHeaders.EyeDoorEyeFacingLeft, 62, 7, 0x00A8);
                return true;
            case 0xC86B:
                place?.Invoke(RoomPlmHeaders.RedDoorFacingRight, 1, 38, 0x00A9);
                return true;
            case 0xC873:
                place?.Invoke(RoomPlmHeaders.SaveStation, 5, 11, 0x0000);
                return true;
            case 0xC87B:
                place?.Invoke(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired, 8, 8, 0x0000);
                place?.Invoke(RoomPlmHeaders.MotherBrainEscapeRoomGate, 31, 6, 0x8000);
                return true;
            case 0xC889:
                place?.Invoke(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired, 8, 8, 0x0002);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingDown, 6, 3, 0x90AA);
                return true;
            case 0xC897:
                place?.Invoke(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired, 8, 8, 0x0004);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 22, 0x90AB);
                return true;
            case 0xC8A5:
                place?.Invoke(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired, 8, 8, 0x0006);
                place?.Invoke(RoomPlmHeaders.GreyDoorFacingRight, 1, 54, 0x90AC);
                return true;
            case 0xC8B3:
                place?.Invoke(RoomPlmHeaders.SaveStation, 7, 11, 0x0001);
                return true;
            case 0xC8BB:
                return true;
            case 0xC8BD:
                return true;
            case 0xC8BF:
                return true;
            case 0xC8C1:
                return true;
            case 0xC8C3:
                return true;
            case 0xC8C5:
                return true;
            default: return false;
        }
    }
}
