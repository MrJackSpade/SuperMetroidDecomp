
namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Cartridge values and helpers of <see cref="CrocomireTongueCollisionDefinitions"/> that only verification reads.</summary>
internal static class CrocomireTongueCollisionDefinitionsConstants
{
    /// <summary>Native bank containing the selected Crocomire extended frames.</summary>
    internal const byte Bank = 0xa4;
    /// <summary>Four fight frames and five melting frames are selected by the tongue actor.</summary>
    internal static int FrameCount => 9;
    /// <summary>Four fight frames followed by five melting frames. The native
    /// count word and one eight-byte component give a ten-byte frame stride.</summary>
    internal static ushort FramePointer(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        return (ushort)(index < 4
            ? CrocomireNativeWords.TongueFrames.Word(CrocomireTongueFrame.Fight0) + 10 * index
            : CrocomireNativeWords.TongueFrames.Word(CrocomireTongueFrame.MeltingPose0) + 10 * (index - 4));
    }
}
