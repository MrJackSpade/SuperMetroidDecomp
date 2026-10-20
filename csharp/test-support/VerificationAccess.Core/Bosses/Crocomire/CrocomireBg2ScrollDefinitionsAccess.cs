
namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Verification access to <see cref="CrocomireBg2ScrollDefinitions"/> members production does not use.</summary>
internal static class CrocomireBg2ScrollDefinitionsAccess
{
    /// <summary>Indexes Crocomire's twelve charge and five claw-motion BG2 correction frames.</summary>
    extension(CrocomireBg2ScrollDefinitions)
    {
        /// <summary>Twelve charge/step-back frames followed by five claw-motion frames.</summary>
        internal static int EntryCount => 17;

        /// <summary>Native pointer order: twelve charge/step-back frames followed by
        /// five moving-claw frames. Each six-component extended frame occupies $32 bytes.</summary>
        internal static CrocomireBg2VerticalCorrection Entry(int index)
        {
            if ((uint)index >= CrocomireBg2ScrollDefinitions.EntryCount) throw new IndexOutOfRangeException();
            ushort frame = (ushort)(index < 12 ? CrocomireBg2ScrollDefinitions.ChargeStepFrameStart + 0x32 * index
                : CrocomireBg2ScrollDefinitions.MovingClawsFrameStart + 0x32 * (index - 12));
            return new(frame, unchecked((ushort)CrocomireBg2ScrollDefinitions.Correction(frame)));
        }
    }
}
