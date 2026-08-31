using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-facing view of Kago bug's aliased bank-$86 variables. The ROM calls the same
/// <c>eproj_G</c> word an idle timer until a shot replaces it with the projectile type.
/// </summary>
public readonly struct KagoBugProjectileState
{
    private readonly RoomEnemyProjectileSlot _projectile;

    internal KagoBugProjectileState(RoomEnemyProjectileSlot projectile) =>
        _projectile = projectile;

    /// <summary>Native <c>eproj_F</c>: byte index of the Kago enemy that emitted this bug.</summary>
    public ushort SourceEnemyNativeIndex => _projectile.Variable1;

    /// <summary>Native <c>eproj_E</c>: countdown to library-two sound <c>$6C</c>.</summary>
    public ushort SoundTimer => _projectile.Variable0;

    /// <summary>Native <c>eproj_G</c>, interpreted as the current idle countdown.</summary>
    public ushort IdleTimer => _projectile.CollidedProjectileType;

    /// <summary>
    /// Native <c>eproj_G</c> after a shot collision. This is the exact incoming beam or
    /// missile type; callers distinguish it from an idle timer by the inert shot pre-AI.
    /// </summary>
    public ushort CollidedProjectileType => _projectile.CollidedProjectileType;
}

/// <summary>
/// Event emitted by Kago bug instruction <c>$86:D1CE</c>. Pickup selection remains owned by
/// the shared enemy-drop subsystem; this retains the exact cartridge definition and origin.
/// </summary>
public readonly record struct KagoBugDropRequest(
    ushort X,
    ushort Y,
    ushort EnemyDefinitionPointer,
    ushort EnemyProjectileNativeIndex);

public sealed partial class RoomEnemySystem
{
    internal const ushort KagoBugIdlePreInstruction = 0xd0ca;
    internal const ushort KagoBugJumpingPreInstruction = 0xd0ec;
    internal const ushort KagoBugFallingPreInstruction = 0xd128;
    internal const ushort KagoBugStartJumpInstruction = 0xd15c;
    internal const ushort KagoBugStartIdleInstruction = 0xd1b6;
    internal const ushort KagoBugUsePaletteZeroInstruction = 0xd1c7;
    internal const ushort KagoBugSpawnDropInstruction = 0xd1ce;

    private const ushort KagoBugInitialInstructionList = 0x9c7d;
    private const ushort KagoBugLandedInstructionList = 0xd03c;
    private const ushort KagoBugFallingInstructionList = 0xd04a;
    private const ushort KagoBugJumpStartInstructionList = 0xd052;
    private const ushort KagoBugGravity = 0x00e0;
    private const ushort KagoBugHorizontalSpeed = 0x0200;
    private const ushort KagoBugSourceCollisionEnableDistance = 23;
    private const ushort KagoBugSourceHomingDistance = 48;
    private const ushort KagoBugSoundEffect = 0x006c;

    /// <summary>Last library-two Kago bug sound request produced during this enemy frame.</summary>
    public ushort? LastKagoBugSoundEffect { get; private set; }

    /// <summary>Last Kago bug enemy-drop request produced during this enemy frame.</summary>
    public KagoBugDropRequest? LastKagoBugDropRequest { get; private set; }

    public KagoBugProjectileState InspectKagoBug(RoomEnemyProjectileSlot projectile)
    {
        ArgumentNullException.ThrowIfNull(projectile);
        if (projectile.Kind != RoomEnemyProjectileKind.KagoBug)
            throw new ArgumentException("The selected enemy projectile is not a Kago bug.", nameof(projectile));
        return new KagoBugProjectileState(projectile);
    }

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
        projectile.PreInstruction = KagoBugIdlePreInstruction;
        projectile.InstructionPointer = KagoBugInitialInstructionList;
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

        projectile.InstructionPointer = KagoBugJumpStartInstructionList;
        projectile.InstructionTimer = 1;
        projectile.PreInstruction = 0xd0eb; // Native one-byte RTS/no-op used during startup art.
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
            projectile.PreInstruction = 0xd0eb;
            projectile.InstructionPointer = KagoBugLandedInstructionList;
            projectile.InstructionTimer = 1;
            return;
        }

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + KagoBugGravity));
    }

    private static void BeginKagoBugFall(RoomEnemyProjectileSlot projectile)
    {
        projectile.PreInstruction = KagoBugFallingPreInstruction;
        projectile.InstructionPointer = KagoBugFallingInstructionList;
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
        projectile.PreInstruction = KagoBugJumpingPreInstruction;
    }

    /// <summary>Ports landed-list instruction <c>$86:D1B6</c>.</summary>
    private void StartKagoBugIdle(RoomEnemyProjectileSlot projectile)
    {
        projectile.CollidedProjectileType = unchecked((ushort)(
            (ReadKagoRandomNumber() & 0x001f) + 1));
        projectile.PreInstruction = KagoBugIdlePreInstruction;
    }

    /// <summary>Ports shot-list drop instruction <c>$86:D1CE</c>.</summary>
    private void RequestKagoBugDrop(RoomEnemyProjectileSlot projectile)
    {
        LastKagoBugDropRequest = new KagoBugDropRequest(
            projectile.XPosition,
            projectile.YPosition,
            KagoDefinition,
            checked((ushort)(projectile.SlotIndex * 2)));
        SpawnEnemyDropFromEnemyHeader(
            projectile.XPosition,
            projectile.YPosition,
            KagoDefinition);
    }

    private ushort ReadKagoRandomNumber() =>
        _readRandomNumber?.Invoke() ?? _nextRandom!();
}
