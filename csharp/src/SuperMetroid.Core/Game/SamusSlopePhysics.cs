using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// ROM-backed primitives used by Samus's non-square slope collision path in bank $94.
/// </summary>
/// <remarks>
/// Super Metroid does not reduce slopes to a line equation. BTS bits select one of 32
/// sixteen-sample height profiles, while a separate table scales grounded horizontal
/// displacement. These methods deliberately read those cartridge tables instead of
/// substituting floating-point geometry that only looks similar.
/// </remarks>
public static class SamusSlopePhysics
{
    /// <summary>
    /// Ports <c>BlockColl_Horiz_Slope_NonSquare</c> at <c>$94:84D6</c>.
    /// </summary>
    /// <param name="behavior">
    /// Native BTS byte. Bits 0–4 choose the profile; bit 7 marks a ceiling slope.
    /// </param>
    /// <param name="displacement">Signed 16.16 horizontal displacement.</param>
    /// <param name="verticalSpeed">
    /// Unsigned high/low pair of <c>samus_y_speed</c>/<c>samus_y_subspeed</c>. Any nonzero
    /// value suppresses grounded slope scaling exactly as the native routine does.
    /// </param>
    public static int ScaleGroundedHorizontalDisplacement(
        ISnesAddressSpace bus,
        byte behavior,
        int displacement,
        uint verticalSpeed)
        => ScaleGroundedHorizontalDisplacement(
            bus,
            new RoomBlockBehavior(behavior),
            displacement,
            verticalSpeed);

    /// <summary>Typed BTS overload used by live room collision.</summary>
    public static int ScaleGroundedHorizontalDisplacement(
        ISnesAddressSpace bus,
        RoomBlockBehavior bts,
        int displacement,
        uint verticalSpeed)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // Ceiling slopes and airborne Samus leave the incoming amount untouched. This is
        // the routine's complete early-return condition, not a host-side policy choice.
        if (bts.SlopeFlipsVertically || verticalSpeed != 0)
            return displacement;

        int shape = bts.SlopeShape;

        // Each shape owns two words. $94:84D6 chooses the second word at index
        // 2*shape+1. For Landing Site BTS $12 that word is $00C0 (three quarters).
        int multiplierAddress =
            SamusMovementRomData.Slopes.HorizontalMultipliers + (2 * shape + 1) * 2;
        ushort multiplier = ReadWord(bus, multiplierAddress);

        // The 65816 discards the displacement's lowest eight fractional bits before the
        // unsigned 16×16 multiply. For negative values it negates that truncated 16-bit
        // magnitude, multiplies, and negates the 32-bit result afterward.
        if (displacement >= 0)
        {
            ushort truncated = unchecked((ushort)(displacement >> 8));
            return unchecked((int)((uint)truncated * multiplier));
        }
        else
        {
            ushort truncated = unchecked((ushort)(displacement >> 8));
            ushort magnitude = unchecked((ushort)-truncated);
            return unchecked(-(int)((uint)magnitude * multiplier));
        }
    }

    /// <summary>
    /// Returns the five-bit height sample selected by BTS shape/mirroring and Samus X.
    /// Values may be 0–20; masking with $1F is part of every native consumer.
    /// </summary>
    public static byte ReadAlignmentHeight(
        ISnesAddressSpace bus,
        byte behavior,
        ushort xPosition)
        => ReadAlignmentHeight(bus, new RoomBlockBehavior(behavior), xPosition);

    /// <summary>Typed BTS overload used by live room collision.</summary>
    public static byte ReadAlignmentHeight(
        ISnesAddressSpace bus,
        RoomBlockBehavior bts,
        ushort xPosition)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // BTS bit $40 mirrors the profile horizontally. XORing the complete coordinate by
        // $000F is equivalent to flipping its low nibble while retaining native word math.
        ushort sampledX = bts.SlopeFlipsHorizontally
            ? unchecked((ushort)(xPosition ^ 0x000f))
            : xPosition;
        int tableIndex = 16 * bts.SlopeShape + (sampledX & 0x0f);
        return unchecked((byte)(
            bus.ReadByte(SamusMovementRomData.Slopes.AlignmentHeights + tableIndex) & 0x1f));
    }

    /// <summary>
    /// Ports <c>Samus_AlignYPosSlope</c> at <c>$94:87F4</c>, the post-horizontal correction
    /// for non-square floor and ceiling slopes.
    /// </summary>
    /// <remarks>
    /// This routine does not perform the preceding horizontal collision scan. It consumes
    /// the position that scan accepted and adjusts only whole-pixel Y, exactly like the
    /// original call immediately after <c>Samus_MoveRight_NoSolidColl</c>.
    /// </remarks>
    public static SlopeAlignmentResult AlignYPosition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        ushort xPosition,
        ushort yPosition,
        ushort yRadius,
        bool horizontalSlopeCollisionEnabled = true)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        if (!horizontalSlopeCollisionEnabled)
            return new SlopeAlignmentResult(yPosition, Adjusted: false, FloorBlock: null, CeilingBlock: null);

        bool adjusted = false;
        RoomCollisionBlock? floorBlock = null;
        RoomCollisionBlock? ceilingBlock = null;

        // $94:87F4 first probes the center-X block containing Samus's bottom pixel. Only
        // type 1, shapes >=5, and BTS bit $80 clear participate as floor slopes.
        ushort bottomPosition = unchecked((ushort)(yPosition + yRadius - 1));
        if (TryGetBlockAtPixel(level, xPosition, bottomPosition, out RoomCollisionBlock bottom) &&
            bottom.CollisionType == RoomCollisionType.Slope &&
            bottom.Bts.IsNonSquareSlope)
        {
            floorBlock = bottom;
            if (!bottom.Bts.SlopeFlipsVertically)
            {
                int height = ReadAlignmentHeight(bus, bottom.Bts, xPosition);
                int bottomNibble = bottomPosition & 0x0f;
                short correction = unchecked((short)(height - bottomNibble - 1));
                if (correction < 0)
                {
                    yPosition = unchecked((ushort)(yPosition + correction));
                    adjusted = true;
                }
            }
        }

        // Recompute the top probe after any floor correction. Ceiling slopes use BTS bit
        // $80 set and reverse the same signed correction, matching $94:887A-$94:88F3.
        ushort topPosition = unchecked((ushort)(yPosition - yRadius));
        if (TryGetBlockAtPixel(level, xPosition, topPosition, out RoomCollisionBlock top) &&
            top.CollisionType == RoomCollisionType.Slope &&
            top.Bts.IsNonSquareSlope)
        {
            ceilingBlock = top;
            if (top.Bts.SlopeFlipsVertically)
            {
                int height = ReadAlignmentHeight(bus, top.Bts, xPosition);
                int invertedTopNibble = (topPosition & 0x0f) ^ 0x0f;
                short correction = unchecked((short)(height - invertedTopNibble - 1));
                if (correction <= 0)
                {
                    yPosition = unchecked((ushort)(yPosition - correction));
                    adjusted = true;
                }
            }
        }

        return new SlopeAlignmentResult(yPosition, adjusted, floorBlock, ceilingBlock);
    }

    private static bool TryGetBlockAtPixel(
        RoomLevelData level,
        ushort xPosition,
        ushort yPosition,
        out RoomCollisionBlock block)
    {
        int blockX = xPosition >> 4;
        int blockY = yPosition >> 4;
        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)blockY >= (uint)level.HeightInBlocks)
        {
            block = default;
            return false;
        }

        block = level.GetCollisionBlock(blockX, blockY);
        return true;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}

/// <summary>Debugger-visible result of bank-$94's post-horizontal slope correction.</summary>
public readonly record struct SlopeAlignmentResult(
    ushort YPosition,
    bool Adjusted,
    RoomCollisionBlock? FloorBlock,
    RoomCollisionBlock? CeilingBlock);
