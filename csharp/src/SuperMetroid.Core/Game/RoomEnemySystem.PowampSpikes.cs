using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$86 projectile $D298 emitted by a Powamp's touch or delayed shot death. All eight
/// actors begin stationary at the body's center, then add the compass direction's signed 8.8
/// acceleration every frame. That produces the original expanding, accelerating burst.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Ports <c>FirePowampSpikesIn8Directions</c> at $A8:C223.</summary>
    /// <returns>
    /// The accumulator <c>FirePowampSpikesIn8Directions</c> leaves: the last spike's native
    /// projectile index, or the body's graphics word when the pool was full. The death
    /// sequence passes it straight to EnemyDeath as the animation.
    /// </returns>
    private ushort SpawnPowampSpikeBurst(RoomEnemySlot body)
    {
        ushort graphics = unchecked((ushort)(body.VramTilesIndex | body.PaletteIndex));
        ushort accumulator = 0;
        // Native Y counts from seven down to zero. Allocation independently searches the
        // eighteen-slot bank-$86 pool from native index $22 downward on every iteration.
        for (int direction = 7; direction >= 0; direction--)
        {
            RoomEnemyProjectileSlot? spike = AllocateEnemyProjectile();
            if (spike is null)
                return graphics;

            // SpawnEnemyProjectileY_ParameterA_XGraphics copies definition $D298 before
            // initializer $D23A zeros velocities and captures the owner position. Reading
            // the record through the shared helper preserves its list, map sentinel, radii,
            // damage, and all property bits instead of maintaining a second hand copy here.
            InitializeEnemyProjectileFromDefinition(
                spike,
                RoomEnemyProjectileKind.PowampSpike,
                unchecked((ushort)(body.VramTilesIndex | body.PaletteIndex)));
            spike.XPosition = body.XPosition;
            spike.YPosition = body.YPosition;
            spike.XSubposition = 0;
            spike.YSubposition = 0;
            spike.XVelocity = 0;
            spike.YVelocity = 0;
            spike.DirectionParameter = unchecked((ushort)direction);
            accumulator = unchecked((ushort)(spike.SlotIndex * 2));
        }
        return accumulator;
    }

    /// <summary>Ports <c>PreInstruction_EnemyProjectile_PowampSpike</c> at $86:D263.</summary>
    private void RunPowampSpikePreInstruction(
        RoomEnemyProjectileSlot spike,
        RoomLevelData level)
    {
        int direction = spike.DirectionParameter;
        if ((uint)direction >= PowampMotionDefinitions.SpikeDirectionCount)
        {
            throw new InvalidDataException(
                $"Powamp spike direction {direction} exceeds the eight-entry ROM table.");
        }

        spike.XVelocity = unchecked((ushort)(
            unchecked((short)spike.XVelocity) + PowampMotionDefinitions.SpikeXAcceleration(direction)));
        if (MoveProjectileAxis(spike, level, horizontal: true))
        {
            BeginPowampSpikeDeletion(spike);
            return;
        }

        spike.YVelocity = unchecked((ushort)(
            unchecked((short)spike.YVelocity) + PowampMotionDefinitions.SpikeYAcceleration(direction)));
        if (MoveProjectileAxis(spike, level, horizontal: false))
            BeginPowampSpikeDeletion(spike);
    }

    /// <summary>Queues the spike's shared delete instruction for the interpreter to consume during this frame.</summary>
    private static void BeginPowampSpikeDeletion(RoomEnemyProjectileSlot spike)
    {
        // The list contains the common delete opcode as its first word. Installing timer
        // one lets the shared instruction interpreter consume it later in this same frame.
        spike.InstructionPointer = PowampSpikeInstructionProgramDefinitions.Delete;
        spike.InstructionTimer = 1;
    }
}
