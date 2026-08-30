using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$86 projectile half of Phantoon's intro and ordinary flame patterns. These actors
/// occupy the shared eighteen-slot pool and retain their cartridge definition pointers,
/// collision properties, bytecode, radii, and integer sine/cosine rounding.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort PhantoonFlameDeleteInstruction = 0x97f8;
    private const ushort PhantoonStartingFlameWaitingPreInstruction = 0x9b29;
    private const ushort PhantoonStartingFlameOrbitPreInstruction = 0x9b41;

    private bool SpawnPhantoonStartingFlame(RoomEnemySlot body, byte directionIndex)
    {
        RoomEnemyProjectileSlot? flame = AllocateEnemyProjectile();
        if (flame is null)
            return false;

        InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonStartingFlame,
            unchecked((ushort)(body.PaletteIndex | body.VramTilesIndex)));
        flame.XSubposition = 0;
        flame.YSubposition = 0;
        flame.XVelocity = 0;
        flame.YVelocity = 0;
        flame.Variable0 = unchecked((byte)(directionIndex * 0x20));
        PositionPhantoonFlameAroundBody(flame, body, flame.Variable0, radius: 0x30);
        flame.PreInstruction = PhantoonStartingFlameWaitingPreInstruction;
        return true;
    }

    /// <summary>
    /// Initializes destroyable flame $86:9C29. The high parameter byte chooses one of the
    /// four native producers: casual fall, rage split, vertical rain, or spiral.
    /// </summary>
    private bool SpawnPhantoonDestroyableFlame(RoomEnemySlot body, ushort parameter)
    {
        RoomEnemyProjectileSlot? flame = AllocateEnemyProjectile();
        if (flame is null)
            return false;

        InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonDestroyableFlame,
            unchecked((ushort)(body.PaletteIndex | body.VramTilesIndex)));
        flame.XSubposition = 0;
        flame.YSubposition = 0;
        flame.YVelocity = 0;
        byte type = unchecked((byte)(parameter >> 8));
        byte index = unchecked((byte)parameter);
        switch (type)
        {
            case 0:
                flame.XVelocity = 0;
                flame.XPosition = body.XPosition;
                flame.YPosition = unchecked((ushort)(body.YPosition + 32));
                flame.InstructionPointer = 0x97b4;
                flame.PreInstruction = 0x9a45;
                break;

            case 2:
                flame.XVelocity = index >= 8 ? unchecked((ushort)-2) : (ushort)2;
                flame.Variable0 = _bus!.ReadByte(0x8698b4 + index);
                flame.XPosition = body.XPosition;
                flame.YPosition = unchecked((ushort)(body.YPosition + 32));
                flame.PreInstruction = 0x9a45;
                break;

            case 4:
                if (index > 8)
                    throw new InvalidDataException($"Phantoon rain column {index} exceeds eight.");
                flame.XVelocity = unchecked((ushort)((parameter & 0x00f0) >> 1));
                flame.XPosition = _bus!.ReadByte(0x8698f7 + index);
                flame.YPosition = 40;
                flame.PreInstruction = 0x9a94;
                break;

            case 6:
                if (index > 7)
                    throw new InvalidDataException($"Phantoon spiral direction {index} exceeds seven.");
                flame.XVelocity = 128;
                flame.Variable0 = _bus!.ReadByte(0x869979 + index);
                flame.XPosition = body.XPosition;
                flame.YPosition = unchecked((ushort)(body.YPosition + 16));
                flame.PreInstruction = 0x9ada;
                break;

            default:
                throw new InvalidDataException(
                    $"Phantoon flame parameter ${parameter:X4} selects invalid type {type}.");
        }
        return true;
    }

    private void RunPhantoonStartingFlameWaiting(RoomEnemyProjectileSlot flame)
    {
        PhantoonEnemyState? state = _phantoonState;
        if (state is null || state.Body.VariableB == 0)
            return;
        flame.PreInstruction = PhantoonStartingFlameOrbitPreInstruction;
        flame.XVelocity = 180;
        flame.YVelocity = 48;
    }

    private void RunPhantoonStartingFlameOrbit(
        RoomEnemyProjectileSlot flame,
        byte nmiFrameCounter8)
    {
        if (_phantoonState is not { } state)
        {
            flame.Clear();
            return;
        }

        if (flame.XVelocity != 0)
        {
            flame.XVelocity = unchecked((ushort)(flame.XVelocity - 1));
        }
        else if ((nmiFrameCounter8 & 1) != 0)
        {
            flame.YVelocity = unchecked((ushort)(flame.YVelocity - 1));
            if (flame.YVelocity == 0)
            {
                flame.XPosition = state.Body.XPosition;
                flame.YPosition = unchecked((ushort)(state.Body.YPosition + 16));
                flame.InstructionPointer = PhantoonFlameDeleteInstruction;
                flame.InstructionTimer = 1;
                return;
            }
        }

        flame.Variable0 = unchecked((byte)(flame.Variable0 + 1));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            flame.Variable0,
            unchecked((byte)flame.YVelocity));
    }

    private void RunPhantoonCasualFlame(RoomEnemyProjectileSlot flame)
    {
        if (_phantoonState is not { } state)
        {
            flame.Clear();
            return;
        }

        flame.YVelocity = unchecked((ushort)(flame.YVelocity + 4));
        flame.Variable0 = unchecked((byte)(flame.Variable0 + (byte)flame.XVelocity));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            flame.Variable0,
            unchecked((byte)flame.YVelocity));
        DeletePhantoonFlameOutsideRoom(flame);
    }

    private static void RunPhantoonRainFlame(
        RoomEnemyProjectileSlot flame,
        RoomLevelData level)
    {
        if (flame.XVelocity != 0)
        {
            flame.XVelocity = unchecked((ushort)(flame.XVelocity - 1));
            return;
        }

        flame.YVelocity = unchecked((ushort)(flame.YVelocity + 16));
        if (!MoveProjectileAxis(flame, level, horizontal: false))
            return;
        flame.InstructionPointer = 0x97ac;
        flame.InstructionTimer = 1;
        flame.YPosition = unchecked((ushort)(flame.YPosition + 8));
        flame.PreInstruction = 0x84fb;
    }

    private void RunPhantoonSpiralFlame(RoomEnemyProjectileSlot flame)
    {
        if (_phantoonState is not { } state)
        {
            flame.Clear();
            return;
        }

        flame.YVelocity = unchecked((ushort)(flame.YVelocity + 2));
        flame.Variable0 = unchecked((byte)(flame.Variable0 + 2));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            flame.Variable0,
            unchecked((byte)flame.YVelocity));
        DeletePhantoonFlameOutsideRoom(flame);
    }

    private void PositionPhantoonFlameAroundBody(
        RoomEnemyProjectileSlot flame,
        RoomEnemySlot body,
        ushort angle,
        ushort radius)
    {
        // $86:9BA2 defines angle zero at the top and advances clockwise. These helpers read
        // the same cartridge sine bytes and retain their negative-fraction truncation quirk.
        int xOffset = ReadEightBitSineProduct(angle, radius);
        int yOffset = -ReadEightBitCosineProduct(angle, radius);
        flame.XPosition = unchecked((ushort)(body.XPosition + xOffset));
        flame.YPosition = unchecked((ushort)(body.YPosition + 16 + yOffset));
    }

    private static void DeletePhantoonFlameOutsideRoom(RoomEnemyProjectileSlot flame)
    {
        if (unchecked((short)flame.XPosition) < 0 || flame.XPosition >= 256 ||
            unchecked((short)flame.YPosition) < 0 || flame.YPosition >= 256)
        {
            flame.InstructionPointer = PhantoonFlameDeleteInstruction;
            flame.InstructionTimer = 1;
        }
    }
}
