using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Event emitted by Kago bug instruction <c>$86:D1CE</c>. Pickup selection remains owned by
/// the shared enemy-drop subsystem; this retains the exact cartridge definition and origin.
/// </summary>
public readonly record struct KagoBugDropRequest();

public sealed partial class RoomEnemySystem
{
    private const ushort KagoBugGravity = 0x00e0;
    private const ushort KagoBugHorizontalSpeed = 0x0200;
    private const ushort KagoBugSourceCollisionEnableDistance = 23;
    private const ushort KagoBugSourceHomingDistance = 48;
    private const ushort KagoBugSoundEffect = 0x006c;

    /// <summary>Last library-two Kago bug sound request produced during this enemy frame.</summary>
    public ushort? LastKagoBugSoundEffect { get; private set; }

    /// <summary>Last Kago bug enemy-drop request produced during this enemy frame.</summary>
    public KagoBugDropRequest? LastKagoBugDropRequest { get; private set; }

    /// <summary>Ports <c>EprojInit_KagosBugs</c> at <c>$86:D088</c>.</summary>
    private bool SpawnKagoBug(RoomEnemySlot source)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return false;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.KagoBug,
            unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex)));

        projectile.Variable1 = source.NativeIndex;
        projectile.Position = projectile.Position.WithWhole(source.Position.Whole);

        // Kago samples the existing random word and never advances it. SpawnEprojInner has
        // already cleared subpositions, velocities, flags, and collision scratch for us.
        ushort initialIdleTimer = unchecked((ushort)((ReadKagoRandomNumber() & 7) + 1));
        projectile.CollidedProjectileType = initialIdleTimer;
        projectile.Variable0 = unchecked((ushort)(initialIdleTimer + 4));
        projectile.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.KagoBug_Idle;
        projectile.InstructionPointer =
            KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug;
        return true;
    }

    /// <summary>Ports the library-two sound countdown shared by every live bug state.</summary>
    private void StepKagoBugSound(RoomEnemyProjectileSlot projectile)
    {
        if (projectile.Variable0 == 0)
            return;

        projectile.Variable0 = unchecked((ushort)(projectile.Variable0 - 1));
        if (projectile.Variable0 == 0)
            LastKagoBugSoundEffect = KagoBugSoundEffect;
    }

    /// <summary>
    /// Sets property $8000 once the bug is at least 23 pixels from its source Kago. That
    /// delay prevents the shot which opened the shell from immediately destroying the bug.
    /// </summary>
    private void EnableKagoBugShotCollisionWhenSeparated(RoomEnemyProjectileSlot projectile)
    {
        RoomEnemySlot source = SlotFromNativeIndex(projectile.Variable1);
        int distance = Math.Abs(unchecked((short)(source.Position.X.Whole - projectile.Position.X.Whole)));
        if (distance >= KagoBugSourceCollisionEnableDistance)
            projectile.BlocksSamusProjectiles = true;
    }

    /// <summary>Ports idle pre-instruction <c>$86:D0CA</c>.</summary>
    private void RunKagoBugIdle(RoomEnemyProjectileSlot projectile)
    {
        StepKagoBugSound(projectile);
        EnableKagoBugShotCollisionWhenSeparated(projectile);

        // The transition occurs only when the counter was already zero on entry. A value
        // of one therefore decrements to zero and remains idle until the following frame.
        if (projectile.CollidedProjectileType != 0)
        {
            projectile.CollidedProjectileType = unchecked((ushort)(
                projectile.CollidedProjectileType - 1));
            return;
        }

        projectile.InstructionPointer =
            KagoBugProjectileInstructionProgramDefinitions.JumpStart;
        projectile.InstructionTimer = 1;
        projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_86D0EB;
    }

    /// <summary>Ports airborne rising pre-instruction <c>$86:D0EC</c>.</summary>
    private void RunKagoBugJumping(RoomEnemyProjectileSlot projectile, RoomLevelData level)
    {
        StepKagoBugSound(projectile);
        EnableKagoBugShotCollisionWhenSeparated(projectile);

        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            // A wall ends the rising phase immediately in the cartridge: both X velocity
            // and the upward component are replaced before selecting the falling list.
            projectile.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0x0100));
            BeginKagoBugFall(projectile);
            return;
        }

        if (MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(0x0100));
            BeginKagoBugFall(projectile);
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + KagoBugGravity))));
        if (unchecked((short)projectile.Velocity.Y.RawValue) >= 0)
            BeginKagoBugFall(projectile);
    }

    /// <summary>Ports falling pre-instruction <c>$86:D128</c>.</summary>
    private void RunKagoBugFalling(RoomEnemyProjectileSlot projectile, RoomLevelData level)
    {
        StepKagoBugSound(projectile);
        EnableKagoBugShotCollisionWhenSeparated(projectile);

        // The native routine is an if/else-if chain. A wall consumes this frame, zeros X,
        // and deliberately postpones both vertical motion and gravity until the next frame.
        if (MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(0));
            return;
        }

        if (MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_86D0EB;
            projectile.InstructionPointer =
                KagoBugProjectileInstructionProgramDefinitions.Landed;
            projectile.InstructionTimer = 1;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + KagoBugGravity))));
    }

    private static void BeginKagoBugFall(RoomEnemyProjectileSlot projectile)
    {
        projectile.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.KagoBug_Falling;
        projectile.InstructionPointer = KagoBugProjectileInstructionProgramDefinitions.Falling;
        projectile.InstructionTimer = 1;
    }

    /// <summary>Ports instruction <c>$86:D15C</c>, including its unusual RNG reuse.</summary>
    private void StartKagoBugJump(RoomEnemyProjectileSlot projectile)
    {
        StepKagoBugSound(projectile);
        EnableKagoBugShotCollisionWhenSeparated(projectile);

        ushort upwardMagnitude = unchecked((ushort)((ReadKagoRandomNumber() & 0x0300) + 0x0800));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-upwardMagnitude)));

        RoomEnemySlot source = SlotFromNativeIndex(projectile.Variable1);
        short sourceDelta = unchecked((short)(source.Position.X.Whole - projectile.Position.X.Whole));
        bool jumpLeft;
        if (Math.Abs((int)sourceDelta) >= KagoBugSourceHomingDistance)
        {
            // Outside the 48-pixel band the bug always jumps back toward its parent shell.
            jumpLeft = sourceDelta < 0;
        }
        else
        {
            // Inside that band the cartridge reuses bit $0100 of the chosen vertical speed
            // as a direction bit instead of requesting a second random sample.
            jumpLeft = (upwardMagnitude & 0x0100) != 0;
        }

        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(jumpLeft
            ? unchecked((ushort)-KagoBugHorizontalSpeed)
            : KagoBugHorizontalSpeed));
        projectile.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.KagoBug_Jumping;
    }

    /// <summary>Ports landed-list instruction <c>$86:D1B6</c>.</summary>
    private void StartKagoBugIdle(RoomEnemyProjectileSlot projectile)
    {
        projectile.CollidedProjectileType = unchecked((ushort)(
            (ReadKagoRandomNumber() & 0x001f) + 1));
        projectile.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.KagoBug_Idle;
    }

    /// <summary>Ports shot-list drop instruction <c>$86:D1CE</c>.</summary>
    private void RequestKagoBugDrop(RoomEnemyProjectileSlot projectile)
    {
        LastKagoBugDropRequest = new KagoBugDropRequest();
        SpawnEnemyDropFromEnemyHeader(
            projectile.Position.Whole,
            EnemyDefinitionId.Kago);
    }

    private ushort ReadKagoRandomNumber() =>
        RequireRandomNumber();
}
