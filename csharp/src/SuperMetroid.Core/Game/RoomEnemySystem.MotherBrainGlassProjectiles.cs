using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Mother Brain glass shard and sparkle actors from <c>$86:CDC5-$CEFB</c>. PLM bytecode is
/// their producer, but allocation, ROM definitions, instruction lists, movement, RNG, and
/// OAM remain owned by the same eighteen-slot bank-$86 pool as every enemy projectile.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort MotherBrainGlassShardGraphicsIndex = 0x0640;
    private const ushort MotherBrainGlassShardInstructionTable = 0xce41;
    // `$86:CDD0-$CDE1` indexes the 256-word sine cycle by the already-even angle
    // offset in X: horizontal motion reads cosine base `$A0:B443`, while vertical
    // motion reads sine base `$A0:B3C3`. Expressing both through the sine base lets
    // the X call add the native quarter-turn (`+64` samples) exactly once. Using
    // `$B443` as this base silently shifted both components and made late angles read
    // unrelated ROM data beyond the table.

    private static readonly short[] MotherBrainGlassShardXOffsets = [8, -40, -16];
    private static readonly short[] MotherBrainGlassShardYOffsets = [32, 32, 32];

    /// <summary>
    /// Consumes one request emitted by PLM instruction <c>$D30B</c>. The request retains the
    /// executing PLM's block origin because initializer <c>$CDC5</c> calls
    /// <c>CalculatePlmBlockCoords(plm_id)</c> after the shared projectile allocator runs.
    /// </summary>
    public void SpawnMotherBrainGlassProjectile(MotherBrainGlassProjectileRequest request)
    {
        EnsureLoaded();
        if (request.DefinitionPointer != (ushort)RoomEnemyProjectileKind.MotherBrainGlassShard)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                $"Mother Brain glass requested unsupported projectile " +
                $"$86:{request.DefinitionPointer:X4}.");
        }
        if (request.Parameter is not (0 or 2 or 4))
        {
            throw new InvalidDataException(
                $"Mother Brain glass shard parameter ${request.Parameter:X4} is not a " +
                "cartridge X/Y offset-table index.");
        }

        RoomEnemyProjectileSlot? shard = AllocateEnemyProjectile();
        if (shard is null)
            return; // SpawnEprojInner silently loses actors when all eighteen slots are full.

        InitializeEnemyProjectileFromDefinition(
            shard,
            RoomEnemyProjectileKind.MotherBrainGlassShard,
            graphicsIndex: 0);

        // `$86:CDC5` doubles the complete RNG word and masks it to an even nine-bit angle.
        // The low eight-bit angle selects signed sine/cosine velocities; bits 5..8 choose
        // one of sixteen ROM-authored shard animation lists.
        ushort doubledRandom = unchecked((ushort)(_nextRandom!() * 2));
        ushort angleOffset = unchecked((ushort)(doubledRandom & 0x01fe));
        int angle = angleOffset >> 1;
        shard.Variable0 = angleOffset;
        shard.XVelocity = ReadSignedSineSample(angle + 64);
        shard.YVelocity = unchecked((ushort)(
            unchecked((short)ReadSignedSineSample(angle)) * 4));
        int animationIndex = ((angleOffset >> 4) & 0x001e) >> 1;
        shard.InstructionPointer = ReadWord(
            _bus!,
            0x860000 | unchecked((ushort)(
                MotherBrainGlassShardInstructionTable + animationIndex * 2)));
        shard.InstructionTimer = 1;
        shard.GraphicsIndex = MotherBrainGlassShardGraphicsIndex;

        int offsetIndex = request.Parameter >> 1;
        shard.XPosition = unchecked((ushort)(
            request.PlmBlockX * 16 + MotherBrainGlassShardXOffsets[offsetIndex]));
        shard.YPosition = unchecked((ushort)(
            request.PlmBlockY * 16 + MotherBrainGlassShardYOffsets[offsetIndex]));

        // Position jitter consumes two additional RNG samples in this order. It is not
        // cosmetic randomness: later turret and sparkle behavior observes the advanced seed.
        shard.XPosition = unchecked((ushort)(
            shard.XPosition + (_nextRandom!() & 0x000f) - 8));
        shard.YPosition = unchecked((ushort)(
            shard.YPosition + (_nextRandom!() & 0x000f) - 8));
    }

    /// <summary>Ports glass-shard pre-instruction <c>$86:CE9B</c>.</summary>
    private void RunMotherBrainGlassShardPreInstruction(RoomEnemyProjectileSlot shard)
    {
        (shard.XPosition, shard.XSubposition) = AddEightBitVelocity(
            shard.XPosition,
            shard.XSubposition,
            shard.XVelocity);
        (shard.YPosition, shard.YSubposition) = AddEightBitVelocity(
            shard.YPosition,
            shard.YSubposition,
            shard.YVelocity);

        // Native tests the whole-coordinate high byte rather than camera visibility. A shard
        // is deleted as soon as wrapped Y leaves page zero; X may freely leave the viewport.
        if ((shard.YPosition & 0xff00) != 0)
        {
            shard.Clear();
            return;
        }

        shard.YVelocity = unchecked((ushort)(shard.YVelocity + 0x0020));

        // The masked test succeeds for 1/16 of samples. The condition itself consumes one
        // random value every live shard frame; a successful sparkle consumes two more below.
        if ((_nextRandom!() & 0x0420) == 0)
            SpawnMotherBrainGlassSparkle(shard);
    }

    /// <summary>Ports sparkle initializer <c>$86:CE6D</c> using the spawning shard as parameter.</summary>
    private void SpawnMotherBrainGlassSparkle(RoomEnemyProjectileSlot source)
    {
        RoomEnemyProjectileSlot? sparkle = AllocateEnemyProjectile();
        if (sparkle is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            sparkle,
            RoomEnemyProjectileKind.MotherBrainGlassSparkle,
            graphicsIndex: 0);
        sparkle.XPosition = unchecked((ushort)(
            source.XPosition + (_nextRandom!() & 0x001f) - 16));
        sparkle.YPosition = unchecked((ushort)(
            source.YPosition + (_nextRandom!() & 0x001f) - 16));
        sparkle.GraphicsIndex = MotherBrainGlassShardGraphicsIndex;
    }

    private static ushort ReadSignedSineSample(int index) =>
        unchecked((ushort)EnemyTrigonometryTables.SignedNegativeCosineWord(index));
}
