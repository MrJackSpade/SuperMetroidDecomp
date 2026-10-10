namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>FX surface used by the native unlocking particles and their splash actors.</summary>
    internal ushort TourianStatueWaterY { get; set; }
    internal void SpawnTourianDescentDust()
    {
        for (int count = 0; count < 4; count++)
        {
            var projectile = AllocateEnemyProjectile();
            if (projectile is null) continue;
            InitializeEnemyProjectileFromDefinition(
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
        TourianStatueEyePosition position =
            TourianStatueUnlockDefinitions.EyePosition(parameter);
        var projectile = AllocateEnemyProjectile();
        if (projectile is null) return;
        InitializeEnemyProjectileFromDefinition(
            projectile,
            soul
                ? RoomEnemyProjectileKind.TourianStatueSoul
                : RoomEnemyProjectileKind.TourianStatueEyeGlow,
            0);
        projectile.XPosition = position.X;
        projectile.YPosition = position.Y;
        if (soul) projectile.YVelocity = unchecked((ushort)-1024);
        else
            (TileArtwork?.TourianStatueColors ?? throw new InvalidOperationException(
                "Tourian statue eye requires installed colors."))
                .ApplyEye(_cgram!, parameter);
    }

    private void SpawnTourianParticleChild(RoomEnemyProjectileSlot parent, RoomEnemyProjectileKind kind)
    {
        if (kind is not (RoomEnemyProjectileKind.TourianStatueSplash or RoomEnemyProjectileKind.TourianStatueParticle or
                RoomEnemyProjectileKind.TourianStatueTail))
            throw new InvalidOperationException($"{kind} is not a Tourian statue child projectile.");
        var projectile = AllocateEnemyProjectile();
        if (projectile is null) return;
        InitializeEnemyProjectileFromDefinition(projectile, kind, 0);
        projectile.XPosition = parent.XPosition;
        projectile.YPosition = parent.YPosition;
        if (kind == RoomEnemyProjectileKind.TourianStatueSplash)
            projectile.YPosition = unchecked((ushort)(TourianStatueWaterY - 4));
        if (kind != RoomEnemyProjectileKind.TourianStatueParticle) return;
        byte angle = unchecked((byte)((_nextRandom!() & 63) - 32));
        projectile.Variable0 = (ushort)(angle * 2);
        projectile.XVelocity = unchecked((ushort)EnemyTrigonometryTables.SignedNegativeCosineWord(angle + 64));
        projectile.YVelocity = unchecked((ushort)(4 * EnemyTrigonometryTables.SignedNegativeCosineWord(angle)));
    }

    private bool TryStepTourianUnlockEffect(RoomEnemyProjectileSlot projectile)
    {
        if (!Enum.IsDefined((TourianStatuePreInstruction)projectile.PreInstruction))
            return false;
        switch ((TourianStatuePreInstruction)projectile.PreInstruction)
        {
            case TourianStatuePreInstruction.SplashMotion:
                projectile.YPosition = unchecked((ushort)(TourianStatueWaterY - 4));
                return true;
            case TourianStatuePreInstruction.SoulMotion:
                (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(projectile.YPosition, projectile.YSubposition, projectile.YVelocity);
                if ((projectile.YPosition & 0x100) != 0)
                {
                    projectile.InstructionPointer = TourianStatueRomData.DeleteProjectile;
                    projectile.InstructionTimer = 1;
                }
                projectile.YVelocity = unchecked((ushort)(projectile.YVelocity - 128));
                return true;
            case TourianStatuePreInstruction.ParticleMotion:
                ushort before = unchecked((ushort)(TourianStatueWaterY - projectile.YPosition));
                (projectile.XPosition, projectile.XSubposition) = AddEightBitVelocity(projectile.XPosition, projectile.XSubposition, projectile.XVelocity);
                (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(projectile.YPosition, projectile.YSubposition, projectile.YVelocity);
                if (((before ^ (TourianStatueWaterY - projectile.YPosition)) & 0x8000) != 0)
                    SpawnTourianParticleChild(projectile, RoomEnemyProjectileKind.TourianStatueSplash);
                if ((projectile.YPosition & 0xff00) == 0x100)
                {
                    projectile.InstructionPointer = TourianStatueRomData.DeleteProjectile;
                    projectile.InstructionTimer = 1;
                }
                else projectile.YVelocity += 16;
                return true;
            default:
                throw new InvalidOperationException(
                    $"Undefined {nameof(TourianStatuePreInstruction)} {projectile.PreInstruction:X4}.");
        }
    }

    private bool TryExecuteTourianUnlockInstruction(RoomEnemyProjectileSlot projectile, ushort code, ref ushort cursor)
    {
        if (code == EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6 &&
            projectile.Kind == RoomEnemyProjectileKind.TourianStatueEyeGlow)
        {
            QueueEnemySound(SoundEffectLibrary2Sounds.TourianStatueRelease, maximumQueued: 6);
            // This command has a one-byte sound operand, unlike the word operands below.
            cursor += 3;
            return true;
        }
        if (!Enum.IsDefined((TourianStatueInstruction)code))
            return false;
        switch ((TourianStatueInstruction)code)
        {
            case TourianStatueInstruction.ResetDustPosition:
                projectile.XPosition = projectile.Variable0;
                projectile.YPosition = projectile.Variable1;
                break;
            case TourianStatueInstruction.SpawnParticle:
                SpawnTourianParticleChild(projectile, RoomEnemyProjectileKind.TourianStatueParticle);
                break;
            case TourianStatueInstruction.SpawnTail:
                SpawnTourianParticleChild(projectile, RoomEnemyProjectileKind.TourianStatueTail);
                break;
            case TourianStatueInstruction.Earthquake:
                EarthquakeType = 1;
                EarthquakeTimer |= 0x20;
                break;
            case TourianStatueInstruction.AddY:
                projectile.YPosition += ReadEnemyProjectileInstructionMechanicsWord(
                    projectile,
                    unchecked((ushort)(cursor + 2)));
                cursor += 2;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(TourianStatueInstruction)} {code:X4}.");
        }
        cursor += 2;
        return true;
    }
}
