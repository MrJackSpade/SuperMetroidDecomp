using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Areas.Maridia;

/// <summary>Cartridge values and helpers of <see cref="MaridiaElevatubePlmDefinitions"/> that only verification reads.</summary>
internal static class MaridiaElevatubePlmDefinitionsConstants
{
    /// <summary><c>$84:B8F0</c>: sixteen-frame delay and one-block draw.</summary>
    internal const ushort AnimationScript = RoomPlmAnimationScriptsConstants.MaridiaElevatube;
    /// <summary><c>$84:B8F6</c>: library-two elevatube sound operand.</summary>
    internal static readonly SoundEffectId SoundId = Library2Sound.Elevatube;
    /// <summary>Supplies the delay, draw pointer, sound command, and deletion word at their native instruction addresses.</summary>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = address switch
        {
            AnimationScript => 16,
            AnimationScript + 2 => PlmDrawSourceDefinitions.MaridiaElevatubeBlock.Offset,
            AnimationScript + 4 => (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum6,
            AnimationScript + 7 => (ushort)RoomPlmInstruction.Delete,
            _ => 0,
        };
        return address is AnimationScript or AnimationScript + 2 or
            AnimationScript + 4 or AnimationScript + 7;
    }
    /// <summary>Supplies the elevatube sound operand at the byte following its library-two command.</summary>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (address == AnimationScript + 6)
        {
            value = checked((byte)SoundEffectCommands.Command(SoundId));
            return true;
        }
        value = 0;
        return false;
    }
}
