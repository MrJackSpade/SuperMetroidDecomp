using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid's two reusable fingernail actors (`$E43F/$E47F`). Their delayed activation,
/// cartridge-selected 16.16 velocities, room collision, body-contour reflection, and spawn
/// alternation remain independent for the two physical enemy slots.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private void RunKraidNailMain(RoomEnemySlot nail, RoomLevelData? level)
    {
        KraidEnemyState state = RequireKraidState(nail);
        if (_slots[0].Health == 0)
        {
            nail.Properties = nail.Properties.With(
                EnemyProperties.Deleted | EnemyProperties.Invisible);
            return;
        }

        KraidPartState part = state.Parts[nail.SlotIndex];
        switch ((KraidAiFunction)nail.VariableA)
        {
            case KraidAiFunction.HandleFunctionTimer:
                TickKraidFunctionTimer(nail, part);
                return;

            case KraidAiFunction.FingernailInitialize:
                InitializeKraidNailFlight(nail, part);
                return;

            case KraidAiFunction.FingernailWaitForLint:
                if (unchecked((short)(_slots[2].XPosition - 256)) >= 0)
                {
                    nail.VariableA = (ushort)part.NextFunction;
                    nail.Properties = nail.Properties.Without(
                        EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
                }
                return;

            case KraidAiFunction.FingernailFire:
                if (level is null)
                {
                    throw new InvalidOperationException(
                        "Active Kraid fingernail movement requires room collision data.");
                }
                TickKraidNailFlight(nail, level);
                return;

            default:
                throw new InvalidDataException(
                    $"Kraid fingernail function $A7:{nail.VariableA:X4} is not translated.");
        }
    }

    private void InitializeKraidNailFlight(RoomEnemySlot nail, KraidPartState part)
    {
        // The pair coordinates its next launch through the other actor's previous
        // velocity and spawn flag. Read that actor before overwriting this one's state.
        int siblingIndex = nail.SlotIndex == 6 ? 7 : 6;
        RoomEnemySlot sibling = _slots[siblingIndex];
        KraidPartState siblingPart = RequireKraidState(nail).Parts[siblingIndex];
        ushort random = RequireRandomNumber();
        (nail.VariableB, nail.VariableC, nail.VariableD, nail.VariableE) =
            KraidNailLaunchDefinitions.FromSiblingVelocity(sibling.VariableE);
        nail.Parameter1 = 1;
        nail.Properties = nail.Properties.Without(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        nail.InstructionTimer = 1;
        nail.CurrentInstruction = KraidNailInstruction;
        nail.VariableA = (ushort)KraidAiFunction.FingernailFire;

        if ((random & 1) == 0 || siblingPart.AlternateSpawnFlag == 1)
        {
            part.AlternateSpawnFlag = 0;
            RoomEnemySlot body = _slots[0];
            nail.XPosition = unchecked((ushort)(
                (body.XPosition - body.XRadius - nail.XRadius) & 0xfff0));
            nail.YPosition = unchecked((ushort)(_slots[1].YPosition + 128));
            return;
        }

        part.AlternateSpawnFlag = 1;
        nail.XPosition = 50;
        nail.YPosition = 240;
        nail.VariableB = 0;
        nail.VariableC = 1;
        nail.VariableD = 0;
        nail.VariableE = 0;
        nail.VariableA = (ushort)KraidAiFunction.FingernailWaitForLint;
        part.NextFunction = KraidAiFunction.FingernailFire;
        nail.Properties = nail.Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
    }

    private void TickKraidNailFlight(RoomEnemySlot nail, RoomLevelData level)
    {
        int horizontal = CombineKraidVelocity(nail.VariableB, nail.VariableC);
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, nail, horizontal))
        {
            (nail.VariableB, nail.VariableC) =
                NegateKraidVelocity(nail.VariableB, nail.VariableC);
        }
        else if (KraidBodyContourReflectsNail(nail))
        {
            (nail.VariableB, nail.VariableC) =
                NegateKraidVelocity(nail.VariableB, nail.VariableC);
        }

        int vertical = CombineKraidVelocity(nail.VariableD, nail.VariableE);
        if (MoveEnemyVertically(level, nail, vertical))
        {
            (nail.VariableD, nail.VariableE) =
                NegateKraidVelocity(nail.VariableD, nail.VariableE);
        }
    }

    private bool KraidBodyContourReflectsNail(RoomEnemySlot nail)
    {
        RoomEnemySlot body = _slots[0];
        int tableOffset = 0;
        for (; tableOffset < 0x80; tableOffset += 4)
        {
            short yOffset = unchecked((short)ReadWord(
                _bus!, EnemyRomTablePointers.Kraid.NailPositionOffsetWords + tableOffset + 2));
            if (unchecked((short)(yOffset + body.YPosition - nail.YPosition)) < 0)
                break;
        }
        if (tableOffset >= 0x80)
            return false;
        ushort contourX = unchecked((ushort)(
            body.XPosition + ReadWord(
                _bus!, EnemyRomTablePointers.Kraid.NailPositionOffsetWords + tableOffset)));
        return unchecked((short)(nail.XPosition + nail.XRadius - contourX)) >= 0 &&
            unchecked((short)nail.VariableC) >= 0;
    }

    private static int CombineKraidVelocity(ushort low, ushort high) =>
        unchecked((int)(((uint)high << 16) | low));

    private static (ushort Low, ushort High) NegateKraidVelocity(ushort low, ushort high)
    {
        int negated = unchecked(-CombineKraidVelocity(low, high));
        return (
            unchecked((ushort)negated),
            unchecked((ushort)((uint)negated >> 16)));
    }
}
