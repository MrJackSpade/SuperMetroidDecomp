namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-authored bank-$84 instruction streams for yellow, green, and red
/// resident door caps. Each color owns four facing-specific closing and active
/// lists; their physical draw layouts and editable tile choices live elsewhere.
/// </summary>
internal static class ColoredDoorPlmProgramDefinitions
{
    /// <summary>First yellow closing list at $84:BFFD.</summary>
    internal const ushort YellowStart = 0xbffd;
    /// <summary>Last yellow instruction byte at $84:C184.</summary>
    internal const ushort YellowEnd = 0xc184;
    /// <summary>First green closing list at $84:C185.</summary>
    internal const ushort GreenStart = 0xc185;
    /// <summary>Last green instruction byte at $84:C300.</summary>
    internal const ushort GreenEnd = 0xc300;
    /// <summary>First red closing list at $84:C301.</summary>
    internal const ushort RedStart = 0xc301;
    /// <summary>Last red instruction byte at $84:C488, before blue doors.</summary>
    internal const ushort RedEnd = 0xc488;

    /// <summary>$84:BD26: follow the installed link on a power-bomb hit.</summary>
    private const ushort PowerBombCallback = 0xbd26;
    /// <summary>$84:BD88: follow the installed link on a super-missile hit.</summary>
    private const ushort SuperMissileCallback = 0xbd88;
    /// <summary>$84:BD50: follow the installed link on a missile hit.</summary>
    private const ushort MissileCallback = 0xbd50;
    /// <summary>$84:A767: yellow-left closed draw; color/orientation strides are 192/48 bytes.</summary>
    private const ushort ColoredClosedDraw = 0xa767;
    /// <summary>$84:A677: left clear draw; orientation stride twelve bytes.</summary>
    private const ushort ClearDraw = 0xa677;
    /// <summary>$84:A9B3: left blue flash draw; orientation stride sixty bytes.</summary>
    private const ushort BlueFlashDraw = 0xa9b3;

    /// <summary>The three contiguous per-color instruction blobs, in address order.</summary>
    private enum ColorBlob { Yellow, Green, Red }

    private static readonly ColorBlob[] ColorBlobs = Enum.GetValues<ColorBlob>();

    private static ushort LastByteOf(ColorBlob blob) => blob switch
    {
        ColorBlob.Yellow => YellowEnd,
        ColorBlob.Green => GreenEnd,
        ColorBlob.Red => RedEnd,
        _ => throw new InvalidOperationException($"Undefined {nameof(ColorBlob)} {(int)blob}."),
    };

    /// <summary>A word starting on a blob's last byte would cross into the next blob.</summary>
    private static bool IsColorBlobEnd(ushort address)
    {
        foreach (ColorBlob blob in ColorBlobs)
            if (LastByteOf(blob) == address) return true;
        return false;
    }

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        if (address is < YellowStart or > RedEnd || IsColorBlobEnd(address)) return false;
        TryReadMechanicsByte(address, out byte low);
        TryReadMechanicsByte((ushort)(address + 1), out byte high);
        value = (ushort)(low | high << 8);
        return true;
    }

    /// <summary>
    /// Decode all 1,164 native bytes. Green orientation groups span 95 bytes;
    /// red adds a three-byte hit sound. Yellow left/right add a sleep-loop goto,
    /// down adds an initial draw, and up keeps the ordinary 95-byte structure.
    /// Preserve packed-byte alignment and all overlapping words within each color,
    /// but reject words crossing the three original color-blob ends.
    /// No stored program or generated cache remains.
    /// </summary>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = 0;
        if (address is < YellowStart or > RedEnd) return false;
        int color = address < GreenStart ? 0 : address < RedStart ? 1 : 2;
        int first;
        int orientation;
        if (color == 0)
        {
            // The upward list lacks four bytes present in each other yellow list.
            orientation = address < YellowStart + 99 ? 0 : address < YellowStart + 198 ? 1 :
                address < YellowStart + 293 ? 2 : 3;
            first = YellowStart + orientation * 99 - (orientation == 3 ? 4 : 0);
        }
        else
        {
            int start = color == 1 ? GreenStart : RedStart;
            int stride = color == 1 ? 95 : 98;
            orientation = (address - start) / stride;
            first = start + orientation * stride;
        }
        int local = address - first;
        bool yellowDown = color == 0 && orientation == 3;
        bool yellowLoop = color == 0 && orientation < 2;
        bool yellowLeft = color == 0 && orientation == 0;
        int sleep = yellowDown ? 43 : 39;
        int hit = color == 0 && orientation != 2 ? 45 : 41;
        int flash = hit + 5 + (color == 2 ? 3 : 0);
        int open = flash + 28;
        int grey = ColoredClosedDraw + color * 192 + orientation * 48;
        int clear = ClearDraw + orientation * 12;
        int startWord;
        int word;
        if (local == 10 || local == hit + 2 || local == open + 2 || (color == 2 && local == hit + 7))
        {
            value = local == 10 ? (byte)8 : local == hit + 2 ? (byte)(color == 2 ? 5 : 1) :
                local == open + 2 ? (byte)7 : (byte)9;
            return true;
        }
        if (local is >= 11 and < 23)
        {
            int offset = local - 11;
            int frame = 2 - offset / 4;
            word = offset % 4 < 2 ? (frame == 0 ? 1 : 2) : grey + frame * 12;
            startWord = 11 + offset / 2 * 2;
        }
        else if (local >= flash && local < flash + 24)
        {
            int offset = local - flash;
            bool blue = offset % 8 < 4;
            word = offset % 4 < 2 ? (blue ? 3 : 4) : blue ? BlueFlashDraw + orientation * 60 : grey;
            startWord = flash + offset / 2 * 2;
        }
        else if (local >= open + 3 && local < open + 19)
        {
            int offset = local - open - 3;
            int frame = offset / 4 + 1;
            word = offset % 4 < 2 ? (frame == 4 ? (yellowLeft ? 92 : 1) : yellowLeft ? 4 : 6) :
                frame == 4 ? clear : grey + frame * 12;
            startWord = open + 3 + offset / 2 * 2;
        }
        else if (local >= hit)
        {
            if (local < hit + 2) { startWord = hit; word = (ushort)RoomPlmInstruction.IncrementDoorHitCounterAndGoto; }
            else if (local < hit + 5) { startWord = hit + 3; word = first + open; }
            else if (color == 2 && local < flash) { startWord = hit + 5; word = (ushort)RoomPlmInstruction.QueueSoundLibrary3Maximum6; }
            else if (local < flash + 26) { startWord = flash + 24; word = (ushort)RoomPlmInstruction.Goto; }
            else if (local < open) { startWord = flash + 26; word = first + sleep; }
            else if (local < open + 2) { startWord = open; word = (ushort)RoomPlmInstruction.QueueSoundLibrary3Maximum6; }
            else { startWord = open + 19; word = (ushort)RoomPlmInstruction.Delete; }
        }
        else
        {
            startWord = local < 10 ? local & ~1 : ((local - 1) & ~1) + 1;
            word = startWord switch
            {
                0 or 4 => 2,
                2 => clear,
                6 => grey + 36,
                8 => (ushort)RoomPlmInstruction.QueueSoundLibrary3Maximum6,
                23 => (ushort)RoomPlmInstruction.GotoIfDoorBitSet,
                25 => orientation switch
                {
                    0 => BlueDoorPlmProgramDefinitions.ClosedLeft, 1 => BlueDoorPlmProgramDefinitions.ClosedRight,
                    2 => BlueDoorPlmProgramDefinitions.ClosedUp, _ => BlueDoorPlmProgramDefinitions.ClosedDown,
                },
                27 => (ushort)RoomPlmInstruction.LinkInstruction,
                29 => first + hit,
                31 => (ushort)RoomPlmInstruction.InstallPreInstruction,
                33 => color switch { 0 => PowerBombCallback, 1 => SuperMissileCallback, _ => MissileCallback },
                35 => yellowDown ? 2 : 1,
                37 => grey,
                39 => yellowDown ? 1 : (ushort)RoomPlmInstruction.Sleep,
                41 => yellowLoop ? (ushort)RoomPlmInstruction.Goto : grey,
                _ => yellowLoop ? first + sleep : (ushort)RoomPlmInstruction.Sleep, // Local 43.
            };
        }
        value = (byte)(word >> ((local - startWord) * 8));
        return true;
    }
}
