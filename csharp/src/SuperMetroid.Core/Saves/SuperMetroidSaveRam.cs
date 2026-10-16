using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// Cartridge-compatible owner of Super Metroid's three battery-backed save slots.
/// </summary>
/// <remarks>
/// <c>$81:8000-$81:812A</c> stores one contiguous WRAM mirror ($D7C0-$DE1B), then writes
/// both a checksum and its complement into two redundant SRAM directories. Keeping that
/// physical layout remains the in-memory cartridge ABI for legacy emulator-save imports;
/// durable JSON stores meaningful state and rebuilds padding and checksum directories.
/// </remarks>
[CpuBusBoundary("codec: native SRAM save slots, checksum directories and completion marker ($81:8000-$81:812A)")]
public sealed class SuperMetroidSaveRam
{
    /// <summary>The retail format's fixed number of checksum-protected save slots.</summary>
    public const int SlotCount = SaveRamLayout.SlotCount;

    /// <summary>The byte length of one native slot payload, including reserved layout space.</summary>
    public const int SlotByteCount = SaveRamLayout.SlotByteCount;

    /// <summary>The global SRAM offset of the selected-slot word used by the file-select menu.</summary>
    public const int SelectedSlotOffset = SaveRamLayout.SelectedSlotOffset;

    /// <summary>Stock area-map rules bound to this codec for packed explored-map translation.</summary>
    [NonSerialized] private IAreaMapSource? mapPresentation;
    /// <summary>Mutable address space containing the three cartridge-format SRAM slots.</summary>
    private readonly ISnesMemoryBus bus;

    /// <summary>Creates an SRAM codec over a mutable SNES address space.</summary>
    /// <param name="bus">Address space that supplies cartridge SRAM reads and writes.</param>
    /// <param name="mapPresentation">Optional area-map metadata used to translate explored-map bits.</param>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not expose mutable memory.</exception>
    public SuperMetroidSaveRam(ISnesAddressSpace bus, IAreaMapSource? mapPresentation)
    {
        this.mapPresentation = mapPresentation;
        Ensure.NotNull(bus);
        this.bus = bus as ISnesMemoryBus ??
            throw new ArgumentException("Save slots require an SRAM-capable address space.", nameof(bus));
    }

    /// <summary>Provides bounded byte access to the native SRAM image.</summary>
    private SramBytes Image => new(bus, SaveRamLayout.Image);

    /// <summary>Installs map metadata after restoring a codec that did not serialize its catalog.</summary>
    internal void BindMapPresentation(IAreaMapSource? maps) => mapPresentation = maps;

    /// <summary>
    /// <c>LoadFromSRAM</c>'s corruption test: either redundant checksum pair matches the
    /// slot's contents. This reads no decoded fields, so it needs no map presentation.
    /// </summary>
    public bool IsSlotValid(int slot)
    {
        SaveRamCodec.ValidateSlot(slot);
        return SaveRamCodec.IsSlotValid(ReadImage(), slot);
    }

    /// <summary>Returns a decoded slot only when either redundant checksum pair is valid.</summary>
    public SuperMetroidSaveSlot? ReadSlot(int slot)
    {
        SaveRamCodec.ValidateSlot(slot);
        byte[] image = ReadImage();
        return SaveRamCodec.IsSlotValid(image, slot) ? SaveRamCodec.DecodeSlot(image, slot, mapPresentation) : null;
    }

    /// <summary>
    /// Ports <c>SaveToSRAM</c> for the translated state currently owned by C# objects.
    /// The slot payload and four directory words are byte-for-byte cartridge structures.
    /// </summary>
    public void SaveSlot(int slot, SuperMetroidSaveSnapshot snapshot)
    {
        Ensure.NotNull(snapshot);
        SaveRamCodec.ValidateSlot(slot);
        byte[] payload = SaveRamCodec.EncodePayload(snapshot, mapPresentation);
        byte[] image = ReadImage();
        SaveRamCodec.WriteSlot(image, slot, payload);
        WriteImage(image);
    }

    /// <summary>The carry <c>DetermineNumberOfDemoSets</c> ($80:8261) returns at boot (<see cref="SaveRamCodec"/>).</summary>
    public bool DetermineBootMainLoopCarry() => SaveRamCodec.DetermineBootMainLoopCarry(ReadImage());

    /// <summary>
    /// Boot's <c>DetermineNumberOfDemoSets</c> ($80:8261), called once from $80:8564. When all
    /// three slots fail <c>LoadFromSRAM</c> it overwrites the completion marker with
    /// <see cref="SaveRamLayout.CorruptSramMarker"/> ($80:8283-8290) and enables three demo
    /// sets; otherwise the full completion marker enables the fourth. The cartridge never
    /// re-evaluates the count before reset.
    /// </summary>
    /// <returns>The attract-demo set count fixed for the session.</returns>
    public int DetermineNumberOfDemoSets()
    {
        byte[] image = ReadImage();
        if (!SaveRamCodec.IsSlotValid(image, 0) && !SaveRamCodec.IsSlotValid(image, 1) && !SaveRamCodec.IsSlotValid(image, 2))
        {
            SaveRamCodec.WriteMarker(image, SaveRamLayout.CorruptSramMarker);
            WriteImage(image);
            return DemoSetUnlockRomData.DefaultSetCount;
        }
        return SaveRamCodec.HasCompletedGame(image) ? DemoSetUnlockRomData.SetCount : DemoSetUnlockRomData.DefaultSetCount;
    }

    /// <summary>True for the native completed-game signature, independent of slot checksums.</summary>
    public bool HasCompletedGame => SaveRamCodec.HasCompletedGame(ReadImage());

    /// <summary>Encodes named completion state without retaining arbitrary signature bytes.</summary>
    public void SetGameCompleted(bool completed)
    {
        byte[] image = ReadImage();
        SaveRamCodec.WriteCompletion(image, completed);
        WriteImage(image);
    }

    /// <summary>Stores the menu's selected-slot word and complement at $1FEC/$1FEE.</summary>
    public void SelectSlot(int slot)
    {
        byte[] image = ReadImage();
        SaveRamCodec.WriteSelectedSlot(image, slot);
        WriteImage(image);
    }

    /// <summary>
    /// Copies the cartridge's complete checksummed payload and both redundant directory
    /// pairs, matching file-copy menu index 13 at <c>$81:9A2C</c> byte for byte.
    /// </summary>
    public void CopySlot(int sourceSlot, int destinationSlot)
    {
        SaveRamCodec.ValidateSlot(sourceSlot);
        SaveRamCodec.ValidateSlot(destinationSlot);
        if (sourceSlot == destinationSlot)
            throw new ArgumentException("File copy requires two different SRAM slots.");
        if (ReadSlot(sourceSlot) is null)
            throw new InvalidOperationException($"Cannot copy empty save slot {sourceSlot}.");

        // Do not decode and re-encode here. The native menu copies all $65C bytes,
        // including untranslated/reserved fields which a domain snapshot cannot preserve.
        byte[] image = ReadImage();
        image.AsSpan(SaveRamLayout.SlotOffset(sourceSlot), SlotByteCount)
            .CopyTo(image.AsSpan(SaveRamLayout.SlotOffset(destinationSlot), SlotByteCount));
        foreach (int directory in (ReadOnlySpan<int>)[SaveRamLayout.PrimaryChecksumOffset, SaveRamLayout.PrimaryComplementOffset,
                     SaveRamLayout.BackupChecksumOffset, SaveRamLayout.BackupComplementOffset])
        {
            image.AsSpan(directory + sourceSlot * 2, 2).CopyTo(image.AsSpan(directory + destinationSlot * 2, 2));
        }
        WriteImage(image);
    }

    /// <summary>
    /// Clears one complete native slot and its four checksum-directory words, matching
    /// file-clear menu index 25 at <c>$81:9C9E</c>.
    /// </summary>
    public void ClearSlot(int slot)
    {
        SaveRamCodec.ValidateSlot(slot);
        byte[] image = ReadImage();
        image.AsSpan(SaveRamLayout.SlotOffset(slot), SlotByteCount).Clear();
        SaveRamCodec.WriteDirectory(image, slot, 0, 0);
        WriteImage(image);
    }

    /// <summary>Reads the persistent menu selection with the cartridge's validity rule (<see cref="SaveRamCodec"/>).</summary>
    public int ReadSelectedSlot() => SaveRamCodec.ReadSelectedSlot(ReadImage());

    /// <summary>Copies the whole SRAM image out, in address order.</summary>
    private byte[] ReadImage()
    {
        SramBytes sram = Image;
        var image = new byte[sram.Length];
        for (int index = 0; index < image.Length; index++)
            image[index] = sram.Read(index);
        return image;
    }

    /// <summary>Writes <paramref name="image"/> over the whole SRAM window, in address order.</summary>
    private void WriteImage(ReadOnlySpan<byte> image)
    {
        SramBytes sram = Image;
        for (int index = 0; index < image.Length; index++)
            sram.Write(index, image[index]);
    }
}
