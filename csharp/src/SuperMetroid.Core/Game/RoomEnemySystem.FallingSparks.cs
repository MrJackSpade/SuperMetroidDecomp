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
    /// <summary>Fallback eight-bit projectile frame counter advanced by standalone projectile updates without an NMI frame value.</summary>
    private byte _standaloneEnemyProjectileFrameCounter8;

    /// <summary>Allocates and initializes enemy projectile <c>$86:F498</c>.</summary>
    private void SpawnFallingSpark(RoomEnemySlot source)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        // The common spawn routine first copies definition $F498, including its blank
        // startup map, $F3F0 pre-instruction, $F353 list, radii, property flags, damage,
        // and interaction behavior. Initializer $F391 then owns only the source position,
        // aliased motion words, and RNG-selected horizontal delta below.
        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.FallingSpark,
            unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex)));
        projectile.XPosition = source.XPosition;
        projectile.XSubposition = source.XSubposition;
        projectile.YPosition = unchecked((ushort)(source.YPosition + 8));
        projectile.YSubposition = source.YSubposition;

        // `$86:F391` zeros both ordinary velocity words, then calls the shared RNG. The
        // horizontal table is one record short. The compiled definition retains the
        // final RNG outcome's instruction-byte overread, including its signed delta.
        projectile.XVelocity = 0;
        projectile.YVelocity = 0;
        Func<ushort> nextRandom = _nextRandom ?? throw new InvalidOperationException(
            "Falling Spark initialization requires the shared cartridge RNG.");
        (projectile.Variable1, projectile.Variable0) = FallingSparkLaunchDefinitions.FromRandom(nextRandom());

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
            // The spark's whole velocity word passes through the shared signed-8.8
            // Move_EnemyProjectile_Vertically ($86:897B), including its slope reactions.
            if (MoveProjectileAxis(projectile, level, horizontal: false))
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

    /// <summary>Applies the aliased 16.16 vertical delta, carrying accumulated fractional motion into the Y position.</summary>
    /// <param name="projectile">Falling spark whose vertical position and subposition are advanced.</param>
    private static void AddFallingSparkVerticalVelocity(
        RoomEnemyProjectileSlot projectile)
    {
        uint fraction = (uint)projectile.YSubposition + projectile.XVelocity;
        projectile.YSubposition = unchecked((ushort)fraction);
        projectile.YPosition = unchecked((ushort)(
            projectile.YPosition + projectile.YVelocity + (fraction >> 16)));
    }

    /// <summary>Applies the 16.16 horizontal delta stored in the projectile's generic variables.</summary>
    /// <param name="projectile">Falling spark whose horizontal position and subposition are advanced.</param>
    private static void AddFallingSparkHorizontalVelocity(
        RoomEnemyProjectileSlot projectile)
    {
        uint fraction = (uint)projectile.XSubposition + projectile.Variable0;
        projectile.XSubposition = unchecked((ushort)fraction);
        projectile.XPosition = unchecked((ushort)(
            projectile.XPosition + projectile.Variable1 + (fraction >> 16)));
    }

    /// <summary>Switches a floor-contact spark to its impact animation and configures the aliased velocity words for the upward arc.</summary>
    /// <param name="projectile">Falling spark that has reached the floor.</param>
    private static void BeginFallingSparkFloorImpact(RoomEnemyProjectileSlot projectile)
    {
        projectile.InstructionPointer = FallingSparkInstructionProgramDefinitions.HitFloor;
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

    /// <summary>Emits a falling-spark trail sprite object at the projectile's current position and graphics index.</summary>
    /// <param name="projectile">Falling spark supplying the trail's position and graphics selection.</param>
    private void SpawnFallingSparkTrail(RoomEnemyProjectileSlot projectile)
    {
        _ = SpawnRoomSpriteObject(
            projectile.XPosition,
            projectile.YPosition,
            RoomSpriteObjectKind.FallingSparkTrail,
            projectile.GraphicsIndex);
    }
}
