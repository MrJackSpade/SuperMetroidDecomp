using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian unlock species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class TourianUnlockBehavior
{
    /// <summary>Live room randomness and installed statue eye artwork.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocator and initializer for statue, dust, particle, and splash projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Earthquake state written by the statue release instruction.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Audio queue receiving the statue eye release sound.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>CGRAM owner receiving installed statue eye colors.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Binds the unlock effects to the current room's projectile, audio, FX, and artwork owners.</summary>
    internal TourianUnlockBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEarthquake earthquake, EnemyAudioQueue audio, EnemyPresentation presentation)
    {
        _room = room;
        _projectiles = projectiles;
        _earthquake = earthquake;
        _audio = audio;
        _presentation = presentation;
    }

    /// <summary>How far above the water surface a splash sits.</summary>
    private static readonly Offset16<RoomPixels> SplashAboveSurface = new(0, -4);

    /// <summary>The water surface the unlocking particles cross and their splashes ride; only its Y axis is meaningful.</summary>
    internal Position16<RoomPixels> TourianStatueWaterSurface { get; set; }
    /// <summary>Creates four fixed-position dust actors for the descending Tourian statue sequence.</summary>
    internal void SpawnTourianDescentDust()
    {
        for (int count = 0; count < 4; count++)
        {
            var projectile = _projectiles.AllocateEnemyProjectile();
            if (projectile is null) continue;
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.TourianStatueDescentDust,
                0);
            projectile.Variable0 = 128;
            projectile.Variable1 = 188;
        }
    }

    /// <summary>$87:8320/832F use the native bank-$86 definitions and eye-position tables.</summary>
    public void SpawnTourianUnlockEffect(ushort parameter, bool soul)
    {
        Position16<RoomPixels> position =
            TourianStatueUnlockDefinitions.EyePosition(parameter);
        var projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null) return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            soul
                ? RoomEnemyProjectileKind.TourianStatueSoul
                : RoomEnemyProjectileKind.TourianStatueEyeGlow,
            0);
        projectile.Position = projectile.Position.WithWhole(position);
        if (soul) projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-1024)));
        else
            (_room.Artwork?.TourianStatueColors ?? throw new InvalidOperationException(
                "Tourian statue eye requires installed colors."))
                .ApplyEye(_presentation.Cgram, parameter);
    }

    /// <summary>Spawns a splash, particle, or tail at its parent, applying water height or randomized motion.</summary>
    internal void SpawnTourianParticleChild(RoomEnemyProjectileSlot parent, RoomEnemyProjectileKind kind)
    {
        if (kind is not (RoomEnemyProjectileKind.TourianStatueSplash or RoomEnemyProjectileKind.TourianStatueParticle or
                RoomEnemyProjectileKind.TourianStatueTail))
            throw new InvalidOperationException($"{kind} is not a Tourian statue child projectile.");
        var projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null) return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, 0);
        projectile.Position = projectile.Position.WithWhole(parent.Position.Whole);
        if (kind == RoomEnemyProjectileKind.TourianStatueSplash)
            projectile.Position = projectile.Position.WithWholeY((TourianStatueWaterSurface + SplashAboveSurface).Y);
        if (kind != RoomEnemyProjectileKind.TourianStatueParticle) return;
        byte angle = unchecked((byte)((_room.Random.Generate() & 63) - 32));
        projectile.Variable0 = (ushort)(angle * 2);
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)EnemyTrigonometryTables.SignedNegativeCosineWord(angle + 64))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(4 * EnemyTrigonometryTables.SignedNegativeCosineWord(angle)))));
    }

    /// <summary>Runs native splash, soul, or particle motion and requests deletion at their exit boundaries.</summary>
    internal bool TryStepTourianUnlockEffect(RoomEnemyProjectileSlot projectile)
    {
        if (!Enum.IsDefined((TourianStatuePreInstruction)projectile.PreInstruction))
            return false;
        switch ((TourianStatuePreInstruction)projectile.PreInstruction)
        {
            case TourianStatuePreInstruction.SplashMotion:
                projectile.Position = projectile.Position.WithWholeY((TourianStatueWaterSurface + SplashAboveSurface).Y);
                return true;
            case TourianStatuePreInstruction.SoulMotion:
                projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
                // The soul is deleted while its Y lies in an odd 256-pixel screen row. Rising from the statues'
                // top screen row, that first happens when it wraps above the room's top edge.
                int soulScreenRow = SnesLittleEndian.HighByte(projectile.Position.Y.Whole);
                if (soulScreenRow % 2 != 0)
                {
                    projectile.InstructionPointer = TourianStatueRomData.DeleteProjectile;
                    projectile.InstructionTimer = NativeCountdown.Start(1);
                }
                projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue - 128))));
                return true;
            case TourianStatuePreInstruction.ParticleMotion:
                // A splash marks the update the particle crosses the surface in either direction: the wrapped
                // signed distance from the particle down to the surface changes sign.
                bool submergedBefore = (TourianStatueWaterSurface - projectile.Position.Whole).Y < 0;
                projectile.Position = projectile.Position.Advance(projectile.Velocity.ToSixteenSixteen());
                bool submergedAfter = (TourianStatueWaterSurface - projectile.Position.Whole).Y < 0;
                if (submergedBefore != submergedAfter)
                    SpawnTourianParticleChild(projectile, RoomEnemyProjectileKind.TourianStatueSplash);
                if (SnesLittleEndian.HighByte(projectile.Position.Y.Whole) == 1)
                {
                    projectile.InstructionPointer = TourianStatueRomData.DeleteProjectile;
                    projectile.InstructionTimer = NativeCountdown.Start(1);
                }
                else projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + (16)))));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Undefined {nameof(TourianStatuePreInstruction)} {projectile.PreInstruction:X4}.");
        }
    }

    /// <summary>Handles statue-specific sound, child spawns, earthquake, and vertical-offset instructions.</summary>
    internal bool TryExecuteTourianUnlockInstruction(RoomEnemyProjectileSlot projectile, ushort code, ref ushort cursor)
    {
        // The engine hands each list word to its interceptors; decode it once and dispatch on the typed command.
        if ((EnemyProjectileInstruction)code == EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6 &&
            projectile.Kind == RoomEnemyProjectileKind.TourianStatueEyeGlow)
        {
            _audio.QueueEnemySound(SoundEffectLibrary2Sounds.TourianStatueRelease, maximumQueued: 6);
            // This command has a one-byte sound operand, unlike the word operands below.
            cursor += 3;
            return true;
        }
        if (!Enum.IsDefined((TourianStatueInstruction)code))
            return false;
        switch ((TourianStatueInstruction)code)
        {
            case TourianStatueInstruction.ResetDustPosition:
                projectile.Position = projectile.Position.WithWhole(new(checked((ushort)projectile.Variable0), checked((ushort)projectile.Variable1)));
                break;
            case TourianStatueInstruction.SpawnParticle:
                SpawnTourianParticleChild(projectile, RoomEnemyProjectileKind.TourianStatueParticle);
                break;
            case TourianStatueInstruction.SpawnTail:
                SpawnTourianParticleChild(projectile, RoomEnemyProjectileKind.TourianStatueTail);
                break;
            case TourianStatueInstruction.Earthquake:
                _earthquake.EarthquakeType = TourianStatueRomData.ReleaseEarthquakeType;
                _earthquake.AddTimerBits(TourianStatueRomData.ReleaseEarthquakeTimer);
                break;
            case TourianStatueInstruction.AddY:
                projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole + (EnemyProjectileEngine.ReadEnemyProjectileInstructionMechanicsWord(
                    projectile,
                    unchecked((ushort)(cursor + 2)))))));
                cursor += 2;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(TourianStatueInstruction)} {code:X4}.");
        }
        cursor += 2;
        return true;
    }

    /// <summary>Registers statue-specific pre-instruction and instruction interceptors.</summary>
    internal void RegisterTourianUnlockProjectiles(EnemyProjectileHandlerTable table)
    {
        // The Tourian statue effects use words outside the closed bank-$86 sets.
        table.InterceptPreInstructions(TryStepTourianUnlockEffect);
        table.InterceptInstructions(TryExecuteTourianUnlockInstruction);
    }
}
