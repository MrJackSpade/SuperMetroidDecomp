using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Cartridge-derived primitives used by Samus's non-square slope collision path in bank $94.
/// </summary>
/// <remarks>
/// Super Metroid does not reduce slopes to a line equation. BTS bits select one of 32
/// sixteen-sample height profiles, while a separate table scales grounded horizontal
/// displacement. Compiled profiles and multipliers preserve the exact discrete geometry
/// and integer movement calculations without reading cartridge tables at runtime.
/// </remarks>
public static class SamusSlopePhysics
{
    /// <summary>
    /// Ports <c>BlockColl_Horiz_Slope_NonSquare</c> at <c>$94:84D6</c>.
    /// </summary>
    /// <param name="bus">Compatibility address space forwarded to the typed overload for null validation; scaling uses compiled multipliers and performs no cartridge reads.</param>
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
        Ensure.NotNull(bus);

        // Ceiling slopes and airborne Samus leave the incoming amount untouched. This is
        // the routine's complete early-return condition, not a host-side policy choice.
        if (bts.SlopeFlipsVertically || verticalSpeed != 0)
            return displacement;

        ushort multiplier = SlopeSpeedDefinitions.HorizontalMultiplier(bts.SlopeShape);

        // The 65816 discards the displacement's lowest eight fractional bits before the
        // unsigned 16×16 multiply. For negative values it negates that truncated 16-bit
        // magnitude, multiplies, and negates the 32-bit result afterward.
        if (displacement >= 0)
        {
            ushort truncated = unchecked((ushort)(SnesEightEight.WholeOf(displacement)));
            return unchecked((int)((uint)truncated * multiplier));
        }
        else
        {
            ushort truncated = unchecked((ushort)(SnesEightEight.WholeOf(displacement)));
            ushort magnitude = unchecked((ushort)-truncated);
            return unchecked(-(int)((uint)magnitude * multiplier));
        }
    }

    /// <summary>Typed BTS overload used by live room collision.</summary>
    public static byte ReadAlignmentHeight(
        ISnesAddressSpace bus,
        RoomBlockBehavior bts,
        ushort xPosition)
    {
        Ensure.NotNull(bus);

        // BTS bit $40 mirrors the profile horizontally. XORing the complete coordinate by
        // $000F is equivalent to flipping its low nibble while retaining native word math.
        ushort sampledX = bts.SlopeFlipsHorizontally
            ? unchecked((ushort)(xPosition ^ 15))
            : xPosition;
        return SlopeHeightDefinitions.Read(bts.SlopeShape, sampledX & 15);
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
        Position16<RoomPixels> position,
        ushort yRadius,
        bool horizontalSlopeCollisionEnabled = true)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(level);
        ushort xPosition = position.X;
        ushort yPosition = position.Y;

        if (!horizontalSlopeCollisionEnabled)
            return new SlopeAlignmentResult(yPosition, Adjusted: false);

        bool adjusted = false;
        RoomCollisionBlock? floorBlock = null;
        RoomCollisionBlock? ceilingBlock = null;

        // $94:87F4 first probes the center-X block containing Samus's bottom pixel. Only
        // type 1, shapes >=5, and BTS bit $80 clear participate as floor slopes.
        ushort bottomPosition = unchecked((ushort)(yPosition + yRadius - 1));
        if (TryGetBlockAtPixel(level, new(xPosition, bottomPosition), out RoomCollisionBlock bottom) &&
            bottom.CollisionType == RoomCollisionType.Slope &&
            bottom.Bts.IsNonSquareSlope)
        {
            floorBlock = bottom;
            if (!bottom.Bts.SlopeFlipsVertically)
            {
                int height = ReadAlignmentHeight(bus, bottom.Bts, xPosition);
                int bottomNibble = bottomPosition & 15;
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
        if (TryGetBlockAtPixel(level, new(xPosition, topPosition), out RoomCollisionBlock top) &&
            top.CollisionType == RoomCollisionType.Slope &&
            top.Bts.IsNonSquareSlope)
        {
            ceilingBlock = top;
            if (top.Bts.SlopeFlipsVertically)
            {
                int height = ReadAlignmentHeight(bus, top.Bts, xPosition);
                int invertedTopNibble = (topPosition & 15) ^ 15;
                short correction = unchecked((short)(height - invertedTopNibble - 1));
                if (correction <= 0)
                {
                    yPosition = unchecked((ushort)(yPosition - correction));
                    adjusted = true;
                }
            }
        }

        return new SlopeAlignmentResult(yPosition, adjusted);
    }

    /// <summary>Converts a pixel probe to its containing room block; probes
    /// beyond the room bounds return false without reading collision data.</summary>
    private static bool TryGetBlockAtPixel(
        RoomLevelData level,
        Position16<RoomPixels> pixel,
        out RoomCollisionBlock block)
    {
        int blockX = pixel.X >> 4;
        int blockY = pixel.Y >> 4;
        if ((uint)blockX >= (uint)level.Size.X ||
            (uint)blockY >= (uint)level.Size.Y)
        {
            block = default;
            return false;
        }

        block = level.GetCollisionBlock(new(unchecked((ushort)blockX), unchecked((ushort)blockY)));
        return true;
    }

}
