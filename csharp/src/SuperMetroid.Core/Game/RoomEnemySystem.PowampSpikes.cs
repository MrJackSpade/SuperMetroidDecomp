using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$86 projectile $D298 emitted by a Powamp's touch or delayed shot death. All eight
/// actors begin stationary at the body's center, then add the direction table's signed 8.8
/// acceleration every frame. That produces the original expanding, accelerating burst.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort PowampSpikeInstructionList = 0xd208;
    private const ushort PowampSpikeDeleteInstructionList = 0xd218;
    private const ushort PowampSpikePreInstruction = 0xd263;
    private const ushort PowampSpikeRadius = 4;
    private const ushort PowampSpikeDamage = 0x0014;
    private const short PowampSpikeAcceleration = 0x0020;

    private static readonly short[] PowampSpikeXAccelerations =
        [0, PowampSpikeAcceleration, PowampSpikeAcceleration, PowampSpikeAcceleration,
         0, -PowampSpikeAcceleration, -PowampSpikeAcceleration, -PowampSpikeAcceleration];
    private static readonly short[] PowampSpikeYAccelerations =
        [-PowampSpikeAcceleration, -PowampSpikeAcceleration, 0, PowampSpikeAcceleration,
         PowampSpikeAcceleration, PowampSpikeAcceleration, 0, -PowampSpikeAcceleration];

    /// <summary>Ports <c>FirePowampSpikesIn8Directions</c> at $A8:C223.</summary>
    private void SpawnPowampSpikeBurst(RoomEnemySlot body)
    {
        // Native Y counts from seven down to zero. Allocation independently searches the
        // eighteen-slot bank-$86 pool from native index $22 downward on every iteration.
        for (int direction = 7; direction >= 0; direction--)
        {
            RoomEnemyProjectileSlot? spike = AllocateEnemyProjectile();
            if (spike is null)
                return;

            spike.Kind = RoomEnemyProjectileKind.PowampSpike;
            spike.XPosition = body.XPosition;
            spike.YPosition = body.YPosition;
            spike.XSubposition = 0;
            spike.YSubposition = 0;
            spike.XVelocity = 0;
            spike.YVelocity = 0;
            spike.DirectionParameter = unchecked((ushort)direction);
            spike.InstructionPointer = PowampSpikeInstructionList;
            spike.InstructionTimer = 1;
            spike.PreInstruction = PowampSpikePreInstruction;
            spike.GraphicsIndex = unchecked((ushort)(body.VramTilesIndex | body.PaletteIndex));
            spike.XRadius = PowampSpikeRadius;
            spike.YRadius = PowampSpikeRadius;
            spike.Damage = PowampSpikeDamage;
            spike.InvincibilityFrames = CommonEnemyProjectileInvincibilityFrames;
            spike.CanDamageSamus = true;
        }
    }

    /// <summary>Ports <c>PreInstruction_EnemyProjectile_PowampSpike</c> at $86:D263.</summary>
    private void RunPowampSpikePreInstruction(
        RoomEnemyProjectileSlot spike,
        RoomLevelData level)
    {
        int direction = spike.DirectionParameter;
        if ((uint)direction >= PowampSpikeXAccelerations.Length)
        {
            throw new InvalidDataException(
                $"Powamp spike direction {direction} exceeds the eight-entry ROM table.");
        }

        spike.XVelocity = unchecked((ushort)(
            unchecked((short)spike.XVelocity) + PowampSpikeXAccelerations[direction]));
        if (MoveProjectileAxis(spike, level, horizontal: true))
        {
            BeginPowampSpikeDeletion(spike);
            return;
        }

        spike.YVelocity = unchecked((ushort)(
            unchecked((short)spike.YVelocity) + PowampSpikeYAccelerations[direction]));
        if (MoveProjectileAxis(spike, level, horizontal: false))
            BeginPowampSpikeDeletion(spike);
    }

    private static void BeginPowampSpikeDeletion(RoomEnemyProjectileSlot spike)
    {
        // The list contains the common delete opcode as its first word. Installing timer
        // one lets the shared instruction interpreter consume it later in this same frame.
        spike.InstructionPointer = PowampSpikeDeleteInstructionList;
        spike.InstructionTimer = 1;
    }
}
