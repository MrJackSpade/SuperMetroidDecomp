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
        projectile.Position = new(new(source.Position.X.Whole, source.Position.X.Fraction), new(unchecked((ushort)(source.Position.Y.Whole + 8)), source.Position.Y.Fraction));

        // `$86:F391` zeros both ordinary velocity words, then calls the shared RNG. The
        // horizontal table is one record short. The compiled definition retains the
        // final RNG outcome's instruction-byte overread, including its signed delta.
        projectile.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
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
        if (unchecked((short)projectile.Velocity.Y.RawValue) >= 0)
        {
            // The spark's whole velocity word passes through the shared signed-8.8
            // Move_EnemyProjectile_Vertically ($86:897B), including its slope reactions.
            if (MoveProjectileAxis(projectile, level, horizontal: false))
            {
                BeginFallingSparkFloorImpact(projectile);
                return;
            }

            uint acceleratedFraction = (uint)projectile.Velocity.X.RawValue + 0x4000;
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)acceleratedFraction)));
            ushort acceleratedWhole = unchecked((ushort)(
                projectile.Velocity.Y.RawValue + (acceleratedFraction >> 16)));

            // CMP #$0004 / BCS deliberately stops storing the whole word once it reaches
            // four. The fractional word still received its $4000 increment immediately
            // before this test, reproducing the cartridge's slightly asymmetric cap.
            if (acceleratedWhole < 4)
                projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(acceleratedWhole));
        }

        AddFallingSparkVerticalVelocity(projectile);
        AddFallingSparkHorizontalVelocity(projectile);

        if ((nmiFrameCounter8 & 3) == 0)
            SpawnFallingSparkTrail(projectile);
    }

    private static void AddFallingSparkVerticalVelocity(
        RoomEnemyProjectileSlot projectile)
    {
        uint fraction = (uint)projectile.Position.Y.Fraction + projectile.Velocity.X.RawValue;
        projectile.Position = projectile.Position.WithFractionY(unchecked((ushort)fraction));
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
            projectile.Position.Y.Whole + projectile.Velocity.Y.RawValue + (fraction >> 16))));
    }

    private static void AddFallingSparkHorizontalVelocity(
        RoomEnemyProjectileSlot projectile)
    {
        uint fraction = (uint)projectile.Position.X.Fraction + projectile.Variable0;
        projectile.Position = projectile.Position.WithFractionX(unchecked((ushort)fraction));
        projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(
            projectile.Position.X.Whole + projectile.Variable1 + (fraction >> 16))));
    }

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
        projectile.Velocity = new(new SnesSignedEightEight(0x8000), new SnesSignedEightEight(0xffff));
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole - 2)));
    }

    private void SpawnFallingSparkTrail(RoomEnemyProjectileSlot projectile)
    {
        _ = SpawnRoomSpriteObject(
            projectile.Position.Whole,
            RoomSpriteObjectKind.FallingSparkTrail,
            projectile.GraphicsIndex);
    }
}
