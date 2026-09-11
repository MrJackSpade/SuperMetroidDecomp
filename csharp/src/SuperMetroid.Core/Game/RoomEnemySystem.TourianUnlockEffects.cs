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
            InitializeEnemyProjectileFromDefinition(projectile, (RoomEnemyProjectileKind)TourianStatueRomData.DescentDust, 0);
            projectile.Variable0 = 128;
            projectile.Variable1 = 188;
        }
    }

    /// <summary>$87:8320/832F use the native bank-$86 definitions and eye-position tables.</summary>
    public void SpawnTourianUnlockEffect(ushort parameter, bool soul)
    {
        if (parameter > 6 || (parameter & 1) != 0) throw new ArgumentOutOfRangeException(nameof(parameter));
        var projectile = AllocateEnemyProjectile();
        if (projectile is null) return;
        InitializeEnemyProjectileFromDefinition(projectile,
            (RoomEnemyProjectileKind)(soul ? TourianStatueRomData.Soul : TourianStatueRomData.EyeGlow), 0);
        projectile.XPosition = ReadWord(_bus!, TourianStatueRomData.EyeX + parameter);
        projectile.YPosition = ReadWord(_bus!, TourianStatueRomData.EyeY + parameter);
        if (soul) projectile.YVelocity = unchecked((ushort)-1024);
        else _cgram!.LoadFromBus(_bus!, TourianStatueRomData.EyeColors + parameter * 4, 4, 249);
    }

    private void SpawnTourianParticleChild(RoomEnemyProjectileSlot parent, ushort definition)
    {
        var projectile = AllocateEnemyProjectile();
        if (projectile is null) return;
        InitializeEnemyProjectileFromDefinition(projectile, (RoomEnemyProjectileKind)definition, 0);
        projectile.XPosition = parent.XPosition;
        projectile.YPosition = parent.YPosition;
        if (definition == TourianStatueRomData.Splash)
            projectile.YPosition = unchecked((ushort)(TourianStatueWaterY - 4));
        if (definition != TourianStatueRomData.Particle) return;
        byte angle = unchecked((byte)((_nextRandom!() & 63) - 32));
        projectile.Variable0 = (ushort)(angle * 2);
        projectile.XVelocity = unchecked((ushort)EnemyTrigonometryTables.SignedNegativeCosineWord(angle + 64));
        projectile.YVelocity = unchecked((ushort)(4 * EnemyTrigonometryTables.SignedNegativeCosineWord(angle)));
    }

    private bool TryStepTourianUnlockEffect(RoomEnemyProjectileSlot projectile)
    {
        switch (projectile.PreInstruction)
        {
            case TourianStatueRomData.SplashMotion:
                projectile.YPosition = unchecked((ushort)(TourianStatueWaterY - 4));
                return true;
            case TourianStatueRomData.SoulMotion:
                (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(projectile.YPosition, projectile.YSubposition, projectile.YVelocity);
                if ((projectile.YPosition & 0x100) != 0)
                {
                    projectile.InstructionPointer = TourianStatueRomData.DeleteProjectile;
                    projectile.InstructionTimer = 1;
                }
                projectile.YVelocity = unchecked((ushort)(projectile.YVelocity - 128));
                return true;
            case TourianStatueRomData.ParticleMotion:
                ushort before = unchecked((ushort)(TourianStatueWaterY - projectile.YPosition));
                (projectile.XPosition, projectile.XSubposition) = AddEightBitVelocity(projectile.XPosition, projectile.XSubposition, projectile.XVelocity);
                (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(projectile.YPosition, projectile.YSubposition, projectile.YVelocity);
                if (((before ^ (TourianStatueWaterY - projectile.YPosition)) & 0x8000) != 0)
                    SpawnTourianParticleChild(projectile, TourianStatueRomData.Splash);
                if ((projectile.YPosition & 0xff00) == 0x100)
                {
                    projectile.InstructionPointer = TourianStatueRomData.DeleteProjectile;
                    projectile.InstructionTimer = 1;
                }
                else projectile.YVelocity += 16;
                return true;
            default: return false;
        }
    }

    private bool TryExecuteTourianUnlockInstruction(RoomEnemyProjectileSlot projectile, ushort code, ref ushort cursor)
    {
        switch (code)
        {
            case TourianStatueRomData.ResetDustPosition:
                projectile.XPosition = projectile.Variable0;
                projectile.YPosition = projectile.Variable1;
                break;
            case TourianStatueRomData.SpawnParticle:
                SpawnTourianParticleChild(projectile, TourianStatueRomData.Particle);
                break;
            case TourianStatueRomData.SpawnTail:
                SpawnTourianParticleChild(projectile, TourianStatueRomData.Tail);
                break;
            case TourianStatueRomData.Earthquake:
                EarthquakeType = 1;
                EarthquakeTimer |= 0x20;
                break;
            case TourianStatueRomData.AddY:
                projectile.YPosition += ReadWord(_bus!, TourianStatueRomData.ProjectileBank | (cursor + 2));
                cursor += 2;
                break;
            default: return false;
        }
        cursor += 2;
        return true;
    }
}
