namespace SuperMetroid.Core.Game;

/// <summary>
/// Physical bank-$86 actors emitted by Mother Brain's phase-two head bytecode. Keeping
/// these in the room's shared eighteen-slot pool preserves allocation failure, same-frame
/// projectile processing, collision, and OAM ordering with the existing room turrets.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private static ReadOnlySpan<short> MotherBrainDroolXOffsets =>
        [6, 14, 8, 10, 11, 12];

    private static ReadOnlySpan<short> MotherBrainDroolYOffsets =>
        [20, 18, 23, 19, 25, 18];

    /// <summary>Ports head opcode <c>$A9:9B3C</c> and initializer <c>$86:C843</c>.</summary>
    private void SpawnMotherBrainDrool(MotherBrainEnemyState state)
    {
        if (!state.DroolGenerationEnabled)
            return;

        ushort parameter = unchecked((ushort)(state.DroolProjectileParameter + 1));
        if (parameter >= 6)
            parameter = 0;
        state.DroolProjectileParameter = parameter;

        RoomEnemyProjectileSlot? drool = AllocateEnemyProjectile();
        if (drool is null)
            return;

        RoomEnemyProjectileKind kind = state.NeckAngleDelta < 0x0080
            ? RoomEnemyProjectileKind.MotherBrainDrool
            : RoomEnemyProjectileKind.MotherBrainDyingDrool;
        InitializeEnemyProjectileFromDefinition(drool, kind, graphicsIndex: 0);
        drool.Variable0 = parameter;

        // SpawnEnemyProjectile runs initializer C843 immediately. It falls through into
        // the attached pre-instruction, so debugger-visible coordinates are valid before
        // the later global projectile pass repeats the same attachment calculation.
        RunMotherBrainAttachedDroolPreInstruction(drool);
    }

    /// <summary>Ports head opcode <c>$A9:9B6D</c> and initializer <c>$86:CA6A</c>.</summary>
    private void SpawnMotherBrainPurpleBreathBig(MotherBrainEnemyState state)
    {
        RoomEnemyProjectileSlot? breath = AllocateEnemyProjectile();
        if (breath is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            breath,
            RoomEnemyProjectileKind.MotherBrainPurpleBreathBig,
            graphicsIndex: 0);
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain purple breath requires the linked head record.");
        breath.XPosition = unchecked((ushort)(head.XPosition + 6));
        breath.YPosition = unchecked((ushort)(head.YPosition + 0x0010));
    }

    /// <summary>Exact six-position mouth attachment table at <c>$86:C86E-C885</c>.</summary>
    private void RunMotherBrainAttachedDroolPreInstruction(
        RoomEnemyProjectileSlot drool)
    {
        MotherBrainEnemyState state = _motherBrain ?? throw new InvalidOperationException(
            "Mother Brain drool ran without its multipart encounter state.");
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain drool requires the linked head record.");
        int parameter = drool.Variable0;
        if ((uint)parameter >= 6)
        {
            throw new InvalidDataException(
                $"Mother Brain drool offset parameter {parameter} is outside 0..5.");
        }

        drool.XPosition = unchecked((ushort)(
            head.XPosition + MotherBrainDroolXOffsets[parameter]));
        drool.YPosition = unchecked((ushort)(
            head.YPosition + MotherBrainDroolYOffsets[parameter]));
        drool.XVelocity = 0;
        drool.YVelocity = 0;
    }

    /// <summary>
    /// Ports <c>$86:C886-C8AF</c>. The native actor uses signed 8.8 velocity, adds twelve
    /// units of gravity per frame, and switches to its four-map splash at world Y $D7.
    /// </summary>
    private static void RunMotherBrainFallingDroolPreInstruction(
        RoomEnemyProjectileSlot drool)
    {
        drool.YVelocity = unchecked((ushort)(drool.YVelocity + 0x000c));
        (drool.YPosition, drool.YSubposition) = AddEightBitVelocity(
            drool.YPosition,
            drool.YSubposition,
            drool.YVelocity);
        if (drool.YPosition < 0x00d7)
            return;

        drool.YPosition = unchecked((ushort)(drool.YPosition - 4));
        drool.InstructionPointer = 0xc8e1;
        drool.InstructionTimer = 1;
    }
}
