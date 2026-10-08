namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Beetom's crawling, hopping, and draining programs.
/// Their interleaved spritemap operands select installed Beetom artwork;
/// crawling, hopping, draining, and their timing remain compiled here.
/// </summary>
internal abstract class BeetomInstructionProgramDefinitions
{
    /// <summary><c>InstList_Beetom_Crawling_FacingLeft_0</c> at $A8:B696.</summary>
    internal const ushort CrawlingLeft = 0xb696;

    /// <summary><c>InstList_Beetom_Hop_FacingLeft</c> at $A8:B6AC.</summary>
    internal const ushort HopLeft = 0xb6ac;

    /// <summary><c>InstList_Beetom_DrainingSamus_FacingLeft_0</c> at $A8:B6CC.</summary>
    internal const ushort DrainingLeft = 0xb6cc;

    /// <summary><c>InstList_Beetom_Crawling_FacingRight_0</c> at $A8:B6F2.</summary>
    internal const ushort CrawlingRight = 0xb6f2;

    /// <summary><c>InstList_Beetom_Hop_FacingRight</c> at $A8:B708.</summary>
    internal const ushort HopRight = 0xb708;

    /// <summary><c>InstList_Beetom_DrainingSamus_FacingRight_0</c> at $A8:B728.</summary>
    internal const ushort DrainingRight = 0xb728;

    /// <summary>The repeating left-crawl frame list at $A8:B698.</summary>
    internal const ushort CrawlingLeftLoop = 0xb698;

    /// <summary>The repeating left-drain frame list at $A8:B6DE.</summary>
    internal const ushort DrainingLeftLoop = 0xb6de;

    internal const int FacingStride = CrawlingRight - CrawlingLeft;

    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - CrawlingLeft;
        if ((uint)offset >= 2 * FacingStride) return false;
        int local = offset % FacingStride;
        if (local < HopLeft - CrawlingLeft) return local >= 4 && local <= 16 && local % 4 == 0;
        local -= HopLeft - CrawlingLeft;
        if (local < DrainingLeft - HopLeft) return local >= 4 && local <= 16 && local % 4 == 0;
        local -= DrainingLeft - HopLeft;
        return (local <= 14 && local % 4 == 2) || (local >= 20 && local <= 32 && local % 4 == 0);
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - CrawlingLeft;
        if ((uint)offset < 2 * FacingStride && IsMechanicsPosition(offset % FacingStride))
        {
            int local = offset % FacingStride;
            int facingOffset = offset / FacingStride * FacingStride;
            if (local < HopLeft - CrawlingLeft)
            {
                if (local == 0) return CommonEnemyInstructionCodes.DisableOffScreenProcessing;
                if (local == 18) return CommonEnemyInstructionCodes.Goto;
                if (local == 20) return (ushort)(CrawlingLeftLoop + facingOffset);
                return 10;
            }
            if (local < DrainingLeft - CrawlingLeft)
            {
                int hop = local - (HopLeft - CrawlingLeft);
                if (hop == 0) return CommonEnemyInstructionCodes.EnableOffScreenProcessing;
                if (hop == 18) return CommonEnemyInstructionCodes.Sleep;
                // Four ticks in the repeated hop pose around the eight-tick middle pose;
                // restore the crawl pose for one tick before sleeping.
                return (ushort)(hop == 14 ? 1 : hop == 6 ? 8 : 4);
            }
            int drain = local - (DrainingLeft - CrawlingLeft);
            if (drain == 12) return 0x30; // Hold the final approach pose before the drain loop.
            if (drain == 16) return EnemyInstructionCodePointers.Instruction_Beetom_Nothing;
            if (drain == 34) return CommonEnemyInstructionCodes.Goto;
            if (drain == 36) return (ushort)(DrainingLeftLoop + facingOffset);
            return 5;
        }
        throw new InvalidDataException($"Beetom instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    internal static bool IsMechanicsPosition(int local)
    {
        if (local < HopLeft - CrawlingLeft)
            return local == 0 || (local >= 2 && local <= 14 && local % 4 == 2) || local is 18 or 20;
        if (local < DrainingLeft - CrawlingLeft)
        {
            int hop = local - (HopLeft - CrawlingLeft);
            return hop == 0 || (hop >= 2 && hop <= 14 && hop % 4 == 2) || hop == 18;
        }
        int drain = local - (DrainingLeft - CrawlingLeft);
        return (drain <= 16 && drain % 4 == 0) ||
            (drain >= 18 && drain <= 30 && drain % 4 == 2) || drain is 34 or 36;
    }
}
