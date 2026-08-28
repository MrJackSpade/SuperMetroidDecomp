using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$86 falling-spark projectile <c>$F498</c> plus the bank-$B4 sprite-object-30 trail
/// it emits. The velocity field names look strange because the original deliberately uses
/// the projectile X velocity words as a 16.16 *vertical* accumulator while generic variables
/// zero/one hold the 16.16 horizontal delta.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort FallingSparkInstructionList = 0xf353;
    private const ushort FallingSparkFloorInstructionList = 0xf363;
    private const ushort FallingSparkPreInstruction = 0xf3f0;
    private byte _standaloneEnemyProjectileFrameCounter8;

    /// <summary>Allocates and initializes enemy projectile <c>$86:F498</c>.</summary>
    private void SpawnFallingSpark(RoomEnemySlot source)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        projectile.Kind = RoomEnemyProjectileKind.FallingSpark;
        projectile.XPosition = source.XPosition;
        projectile.XSubposition = source.XSubposition;
        projectile.YPosition = unchecked((ushort)(source.YPosition + 8));
        projectile.YSubposition = source.YSubposition;

        // `$86:F391` zeros both ordinary velocity words, then calls the shared RNG. The
        // horizontal table is one word short: index $1C reads the first two instruction
        // words at $F3F0/$F3F2. Direct bus reads retain that retail overread exactly.
        projectile.XVelocity = 0;
        projectile.YVelocity = 0;
        Func<ushort> nextRandom = _nextRandom ?? throw new InvalidOperationException(
            "Falling Spark initialization requires the shared cartridge RNG.");
        int randomTableOffset = nextRandom() & 0x001c;
        projectile.Variable1 = ReadWord(_bus!, 0x86f3d4 + randomTableOffset);
        projectile.Variable0 = ReadWord(_bus!, 0x86f3d6 + randomTableOffset);

        projectile.InstructionPointer = FallingSparkInstructionList;
        projectile.InstructionTimer = 1;
        projectile.PreInstruction = FallingSparkPreInstruction;
        projectile.GraphicsIndex = unchecked((ushort)(
            source.VramTilesIndex | source.PaletteIndex));
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.Damage = 5;
        projectile.InvincibilityFrames = 96;
        projectile.CanDamageSamus = true;
    }

    /// <summary>Ports pre-instruction <c>$86:F3F0</c> without renaming its aliased fields.</summary>
    private void RunFallingSparkPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level,
        byte nmiFrameCounter8)
    {
        // A nonnegative vertical whole word first passes through the generic signed-8.8
        // collision helper. The helper's accepted subpixel movement is *additional* to the
        // manual 16.16 movement below; omitting it shifts the floor-contact frame.
        if (unchecked((short)projectile.YVelocity) >= 0)
        {
            if (MoveFallingSparkThroughVerticalCollisionHelper(projectile, level))
            {
                BeginFallingSparkFloorImpact(projectile);
                return;
            }

            uint acceleratedFraction = (uint)projectile.XVelocity + 0x4000;
            projectile.XVelocity = unchecked((ushort)acceleratedFraction);
            ushort acceleratedWhole = unchecked((ushort)(
                projectile.YVelocity + (acceleratedFraction >> 16)));

            // CMP #$0004 / BCS deliberately stops storing the whole word once it reaches
            // four. The fractional word still received its $4000 increment immediately
            // before this test, reproducing the cartridge's slightly asymmetric cap.
            if (acceleratedWhole < 4)
                projectile.YVelocity = acceleratedWhole;
        }

        AddFallingSparkVerticalVelocity(projectile);
        AddFallingSparkHorizontalVelocity(projectile);

        if ((nmiFrameCounter8 & 3) == 0)
            SpawnFallingSparkTrail(projectile);
    }

    /// <summary>
    /// Exact Spark use of common <c>Move_EnemyProjectile_Vertically</c>. Here YVelocity is
    /// only the signed whole half of a separate 16.16 accumulator, yet the common routine
    /// interprets it as signed 8.8; that odd double movement is observable and intentional.
    /// </summary>
    private static bool MoveFallingSparkThroughVerticalCollisionHelper(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        short helperVelocity = unchecked((short)projectile.YVelocity);
        int fixedPosition = (projectile.YPosition << 16) | projectile.YSubposition;
        fixedPosition = unchecked(fixedPosition + (helperVelocity << 8));
        ushort candidatePosition = unchecked((ushort)(fixedPosition >> 16));
        ushort candidateSubposition = unchecked((ushort)fixedPosition);

        ushort collisionEdge = unchecked((ushort)(candidatePosition +
            (helperVelocity < 0 ? -projectile.YRadius : projectile.YRadius - 1)));
        int firstHorizontalBlock = (projectile.XPosition - projectile.XRadius) >> 4;
        int lastHorizontalBlock =
            (projectile.XPosition + projectile.XRadius - 1) >> 4;
        bool collided = false;
        for (int blockX = firstHorizontalBlock; blockX <= lastHorizontalBlock; blockX++)
        {
            if (ProjectileProbeHitsRoom(
                    level,
                    unchecked((ushort)(blockX << 4)),
                    collisionEdge))
            {
                collided = true;
                break;
            }
        }

        if (!collided)
        {
            projectile.YPosition = candidatePosition;
            projectile.YSubposition = candidateSubposition;
            return false;
        }

        // The common helper clears subposition and clamps only when the candidate boundary
        // lies beyond the current origin in the movement direction. Retaining those CMP
        // guards prevents collision with a linked/irregular block from pulling the actor
        // backward across a tile boundary.
        projectile.YSubposition = 0;
        ushort clampedPosition;
        if (helperVelocity >= 0)
        {
            clampedPosition = unchecked((ushort)(
                (collisionEdge & 0xfff0) - projectile.YRadius));
            if (clampedPosition >= projectile.YPosition)
                projectile.YPosition = clampedPosition;
        }
        else
        {
            clampedPosition = unchecked((ushort)(
                (collisionEdge | 0x000f) + projectile.YRadius + 1));
            if (clampedPosition <= projectile.YPosition)
                projectile.YPosition = clampedPosition;
        }
        return true;
    }

    private static void AddFallingSparkVerticalVelocity(
        RoomEnemyProjectileSlot projectile)
    {
        uint fraction = (uint)projectile.YSubposition + projectile.XVelocity;
        projectile.YSubposition = unchecked((ushort)fraction);
        projectile.YPosition = unchecked((ushort)(
            projectile.YPosition + projectile.YVelocity + (fraction >> 16)));
    }

    private static void AddFallingSparkHorizontalVelocity(
        RoomEnemyProjectileSlot projectile)
    {
        uint fraction = (uint)projectile.XSubposition + projectile.Variable0;
        projectile.XSubposition = unchecked((ushort)fraction);
        projectile.XPosition = unchecked((ushort)(
            projectile.XPosition + projectile.Variable1 + (fraction >> 16)));
    }

    private static void BeginFallingSparkFloorImpact(RoomEnemyProjectileSlot projectile)
    {
        projectile.InstructionPointer = FallingSparkFloorInstructionList;
        projectile.InstructionTimer = 1;

        // Two paired ASL/ROL operations multiply the signed 16.16 horizontal delta by
        // four without losing carry between its fraction and whole words.
        for (int shift = 0; shift < 2; shift++)
        {
            uint doubledFraction = (uint)projectile.Variable0 << 1;
            projectile.Variable0 = unchecked((ushort)doubledFraction);
            projectile.Variable1 = unchecked((ushort)(
                ((uint)projectile.Variable1 << 1) | (doubledFraction >> 16)));
        }

        // The same aliased pair now represents vertical -0.5 in 16.16. Because the whole
        // word is negative, later frames skip collision and gravity while the impact spark
        // arcs upward through its eleven-frame blinking animation.
        projectile.XVelocity = 0x8000;
        projectile.YVelocity = 0xffff;
        projectile.YPosition = unchecked((ushort)(projectile.YPosition - 2));
    }

    private void SpawnFallingSparkTrail(RoomEnemyProjectileSlot projectile)
    {
        _ = SpawnRoomSpriteObject(
            projectile.XPosition,
            projectile.YPosition,
            RoomSpriteObjectKind.FallingSparkTrail,
            projectile.GraphicsIndex);
    }
}
