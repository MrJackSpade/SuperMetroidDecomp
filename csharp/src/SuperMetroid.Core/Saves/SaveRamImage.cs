namespace SuperMetroid.Core.Saves;

/// <summary>
/// Imports and exports the cartridge's complete 8-KiB SRAM image (#629): legacy emulator
/// saves, replay reset headers and recorder seeds carry SRAM as these exact bytes.
/// </summary>
[CpuBusBoundary("codec: whole native SRAM image import/export")]
public static class SaveRamImage
{
    /// <summary>Writes <paramref name="image"/> over the whole SRAM window, in address order.</summary>
    public static void Load(ISnesMemoryBus bus, ReadOnlySpan<byte> image)
    {
        var sram = new SramBytes(bus, SaveRamLayout.Image);
        Ensure.LengthEqual(image, sram.Length);
        for (int index = 0; index < image.Length; index++)
            sram.Write(index, image[index]);
    }

    /// <summary>Copies the whole SRAM window out, in address order.</summary>
    public static byte[] Capture(ISnesMemoryBus bus)
    {
        var sram = new SramBytes(bus, SaveRamLayout.Image);
        var image = new byte[sram.Length];
        for (int index = 0; index < image.Length; index++)
            image[index] = sram.Read(index);
        return image;
    }

    /// <summary>Zeroes the whole SRAM window, in address order.</summary>
    public static void Clear(ISnesMemoryBus bus)
    {
        var sram = new SramBytes(bus, SaveRamLayout.Image);
        for (int index = 0; index < sram.Length; index++)
            sram.Write(index, 0);
    }
}
