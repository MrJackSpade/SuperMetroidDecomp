
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Verification access to <see cref="CrocomireBg2Alignment"/> members production does not use.</summary>
internal static class CrocomireBg2ScrollDefinitionsAccess
{
    /// <summary>$A4:BFC4 ExtendedSpritemap_Crocomire_ChargeForward_StepBack_0, first of twelve $32-byte frames.</summary>
    private const ushort ChargeStepFrameStart = 0xbfc4;
    /// <summary>$A4:C47A ExtendedSpritemap_Crocomire_MovingClaws_0, first of five $32-byte frames.</summary>
    private const ushort MovingClawsFrameStart = 0xc47a;

    /// <summary>Indexes Crocomire's twelve charge and five claw-motion BG2 correction frames.</summary>
    extension(CrocomireBg2Alignment)
    {
        /// <summary>Twelve charge/step-back frames followed by five claw-motion frames.</summary>
        internal static int EntryCount => 17;

        /// <summary>The BG2 vertical scroll for body Y <paramref name="bodyY"/> and the frame at native bank-$A4 address <paramref name="pointer"/>.</summary>
        internal static ushort VerticalScroll(ushort bodyY, ushort pointer) =>
            CrocomireBg2Alignment.VerticalScroll(bodyY, RoomEnemySlotFrameAccess.TryFrameAt(CrocomireBodyCollision.Bank, pointer));

        /// <summary>Native pointer order: twelve charge/step-back frames followed by
        /// five moving-claw frames. Each six-component extended frame occupies $32 bytes.</summary>
        internal static CrocomireBg2VerticalCorrection Entry(int index)
        {
            if ((uint)index >= CrocomireBg2Alignment.EntryCount) throw new ArgumentOutOfRangeException(nameof(index));
            ushort frame = (ushort)(index < 12 ? ChargeStepFrameStart + 0x32 * index : MovingClawsFrameStart + 0x32 * (index - 12));
            return new(frame, unchecked((ushort)CrocomireBg2Alignment.Correction(
                RoomEnemySlotFrameAccess.TryFrameAt(CrocomireBodyCollision.Bank, frame))));
        }
    }
}
