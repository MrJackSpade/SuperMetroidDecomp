using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Event emitted by Kago bug instruction <c>$86:D1CE</c>. Pickup selection remains owned by
/// the shared enemy-drop subsystem; this retains the exact cartridge definition and origin.
/// </summary>
public readonly record struct KagoBugDropRequest();

public sealed partial class RoomEnemySystem
{
    /// <summary>Per-frame acceleration added to a bug's vertical 8.8 velocity.</summary>
    private const ushort KagoBugGravity = 0x00e0;
    /// <summary>Magnitude of the bug's horizontal 8.8 velocity during its jump.</summary>
    private const ushort KagoBugHorizontalSpeed = 0x0200;
    /// <summary>Minimum horizontal separation from the source Kago before shots can hit its bug.</summary>
    private const ushort KagoBugSourceCollisionEnableDistance = 23;
    /// <summary>Separation at which a jumping bug homes toward its source instead of using the speed-derived direction bit.</summary>
    private const ushort KagoBugSourceHomingDistance = 48;
    /// <summary>Library-two sound effect requested when the spawn countdown reaches zero.</summary>
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
        projectile.XPosition = source.XPosition;
        projectile.YPosition = source.YPosition;

        // Kago samples the existing random word and never advances it. SpawnEprojInner has
        // already cleared subpositions, velocities, flags, and collision scratch for us.
        ushort initialIdleTimer = unchecked((ushort)((ReadKagoRandomNumber() & 7) + 1));
        projectile.CollidedProjectileType = initialIdleTimer;
        projectile.Variable0 = unchecked((ushort)(initialIdleTimer + 4));
        projectile.PreInstruction =
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KagoBug_Idle;
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
        int distance = Math.Abs(unchecked((short)(source.XPosition - projectile.XPosition)));
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
        projectile.PreInstruction = EnemyProjectileCodePointers.RTS_86D0EB;
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
            projectile.XVelocity = 0;
            projectile.YVelocity = 0x0100;
            BeginKagoBugFall(projectile);
            return;
        }

        if (MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.YVelocity = 0x0100;
            BeginKagoBugFall(projectile);
            return;
        }

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + KagoBugGravity));
        if (unchecked((short)projectile.YVelocity) >= 0)
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
            projectile.XVelocity = 0;
            return;
        }

        if (MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.PreInstruction = EnemyProjectileCodePointers.RTS_86D0EB;
            projectile.InstructionPointer =
                KagoBugProjectileInstructionProgramDefinitions.Landed;
            projectile.InstructionTimer = 1;
            return;
        }

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + KagoBugGravity));
    }

    /// <summary>Switches a rising bug to its falling instruction list and pre-instruction.</summary>
    /// <param name="projectile">Bug projectile whose vertical motion has entered the falling phase.</param>
    private static void BeginKagoBugFall(RoomEnemyProjectileSlot projectile)
    {
        projectile.PreInstruction =
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KagoBug_Falling;
        projectile.InstructionPointer = KagoBugProjectileInstructionProgramDefinitions.Falling;
        projectile.InstructionTimer = 1;
    }

    /// <summary>Ports instruction <c>$86:D15C</c>, including its unusual RNG reuse.</summary>
    private void StartKagoBugJump(RoomEnemyProjectileSlot projectile)
    {
        StepKagoBugSound(projectile);
        EnableKagoBugShotCollisionWhenSeparated(projectile);

        ushort upwardMagnitude = unchecked((ushort)((ReadKagoRandomNumber() & 0x0300) + 0x0800));
        projectile.YVelocity = unchecked((ushort)-upwardMagnitude);

        RoomEnemySlot source = SlotFromNativeIndex(projectile.Variable1);
        short sourceDelta = unchecked((short)(source.XPosition - projectile.XPosition));
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

        projectile.XVelocity = jumpLeft
            ? unchecked((ushort)-KagoBugHorizontalSpeed)
            : KagoBugHorizontalSpeed;
        projectile.PreInstruction =
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KagoBug_Jumping;
    }

    /// <summary>Ports landed-list instruction <c>$86:D1B6</c>.</summary>
    private void StartKagoBugIdle(RoomEnemyProjectileSlot projectile)
    {
        projectile.CollidedProjectileType = unchecked((ushort)(
            (ReadKagoRandomNumber() & 0x001f) + 1));
        projectile.PreInstruction =
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KagoBug_Idle;
    }

    /// <summary>Ports shot-list drop instruction <c>$86:D1CE</c>.</summary>
    private void RequestKagoBugDrop(RoomEnemyProjectileSlot projectile)
    {
        LastKagoBugDropRequest = new KagoBugDropRequest();
        SpawnEnemyDropFromEnemyHeader(
            projectile.XPosition,
            projectile.YPosition,
            KagoDefinition);
    }

    /// <summary>Reads the current shared random word used by Kago bug timing and jump selection.</summary>
    /// <returns>The random-number state without advancing it.</returns>
    private ushort ReadKagoRandomNumber() =>
        RequireRandomNumber();
}
