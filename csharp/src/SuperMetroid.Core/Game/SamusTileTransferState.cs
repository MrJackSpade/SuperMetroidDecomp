using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$92 Samus animation-tile selection plus bank-$80's dedicated NMI DMA path.
/// </summary>
/// <remarks>
/// Samus is not stored as a convenient sheet of finished frames. Every animation frame
/// chooses one top-half and (usually) one bottom-half seven-byte transfer definition. NMI
/// then copies each definition in two pieces into fixed OBJ character slots. Modeling that
/// indirection is essential: it lets a debugger expose the same pose, animation record,
/// graphics source, size, and VRAM destination that the cartridge uses.
/// </remarks>
public sealed class SamusTileTransferState
{
    private const int AnimationDefinitionPointerTable = 0x92d94e;
    private const int TopDefinitionPointerTable = 0x92d91e;
    private const int BottomDefinitionPointerTable = 0x92d938;

    /// <summary>Bank-$92 address of the selected seven-byte top-half DMA definition.</summary>
    public int TopDefinitionAddress { get; private set; }

    /// <summary>Bank-$92 address of the selected seven-byte bottom-half DMA definition.</summary>
    public int BottomDefinitionAddress { get; private set; }

    /// <summary>Low byte of WRAM <c>$071D</c>; nonzero enables top-half NMI DMA.</summary>
    public bool TopTransferEnabled { get; private set; }

    /// <summary>Low byte of WRAM <c>$071E</c>; nonzero enables bottom-half NMI DMA.</summary>
    public bool BottomTransferEnabled { get; private set; }

    /// <summary>
    /// Ports <c>Set_SamusTilesDefinitions_ForCurrentAnimation</c> at <c>$92:8000</c>.
    /// </summary>
    public void SelectForPoseFrame(ISnesAddressSpace bus, byte pose, ushort animationFrame)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // One word per pose selects a variable-length list of four-byte animation records.
        // The frame number is 16-bit in WRAM and the original ASL/ASL arithmetic wraps.
        ushort animationList = ReadWord(bus, AddWithinBank(AnimationDefinitionPointerTable, pose * 2));
        ushort animationRecordOffset = unchecked((ushort)(animationList + animationFrame * 4));
        int animationRecord = 0x920000 | animationRecordOffset;

        byte topSet = bus.ReadByte(animationRecord);
        byte topPosition = bus.ReadByte(AddWithinBank(animationRecord, 1));
        byte bottomSet = bus.ReadByte(AddWithinBank(animationRecord, 2));
        byte bottomPosition = bus.ReadByte(AddWithinBank(animationRecord, 3));

        // Each set-table word points to a list of seven-byte definitions. Assembly computes
        // position*7 as position*8-position, a detail made explicit here for readability.
        TopDefinitionAddress = ResolveDefinition(
            bus,
            TopDefinitionPointerTable,
            topSet,
            topPosition);
        TopTransferEnabled = true;

        // $FF means this animation frame has no bottom-half graphics update. Crucially the
        // native routine simply returns; it does not clear a previously enabled flag.
        if (bottomSet != 0xff)
        {
            BottomDefinitionAddress = ResolveDefinition(
                bus,
                BottomDefinitionPointerTable,
                bottomSet,
                bottomPosition);
            BottomTransferEnabled = true;
        }
    }

    /// <summary>
    /// Ports <c>TransferSamusTilesToVRAM</c> at <c>$80:9376</c> for one accepted NMI.
    /// </summary>
    public void TransferToVram(ISnesAddressSpace bus, SnesVram vram)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);

        // Samus has a dedicated DMA path instead of using the ordinary seven-byte VRAM
        // queue. The four destinations form two interleaved character regions selected by
        // the pose spritemaps' tile numbers under gameplay OBSEL=$03.
        if (TopTransferEnabled)
            ExecuteDefinition(bus, vram, TopDefinitionAddress, 0x6000, 0x6100);
        if (BottomTransferEnabled)
            ExecuteDefinition(bus, vram, BottomDefinitionAddress, 0x6080, 0x6180);

        // These flags are intentionally not cleared. The original NMI routine leaves them
        // set, and Samus_Draw refreshes the selected definitions during each main-loop pass.
    }

    /// <summary>Clears both transfer flags as the game-state reset routines do.</summary>
    public void ClearTransferFlags()
    {
        TopTransferEnabled = false;
        BottomTransferEnabled = false;
    }

    private static int ResolveDefinition(
        ISnesAddressSpace bus,
        int pointerTable,
        byte setIndex,
        byte position)
    {
        ushort listPointer = ReadWord(bus, AddWithinBank(pointerTable, setIndex * 2));
        return 0x920000 | unchecked((ushort)(listPointer + position * 7));
    }

    private static void ExecuteDefinition(
        ISnesAddressSpace bus,
        SnesVram vram,
        int definitionAddress,
        ushort part1Destination,
        ushort part2Destination)
    {
        // Definition layout: 24-bit source, 16-bit part-1 size, 16-bit part-2 size.
        ushort sourceOffset = ReadWord(bus, definitionAddress);
        byte sourceBank = bus.ReadByte(AddWithinBank(definitionAddress, 2));
        int sourceAddress = sourceBank << 16 | sourceOffset;
        ushort part1Size = ReadWord(bus, AddWithinBank(definitionAddress, 3));
        ushort part2Size = ReadWord(bus, AddWithinBank(definitionAddress, 5));

        // Retail definitions use nonzero part-1 sizes. On real DMA a zero DAS means 65536
        // bytes; reject it until a known frame needs that edge instead of silently doing no
        // work through SnesVram's narrower ushort-size API.
        if (part1Size == 0)
            throw new NotSupportedException("A zero-size Samus part-1 DMA requires the SNES 65536-byte DAS behavior.");

        vram.ExecuteQueuedWrite(bus, sourceAddress, part1Size, part1Destination);
        if (part2Size != 0)
        {
            // DMA increments only the 16-bit A-bus address. Adding in ushort space retains
            // the source bank if a definition ever crosses xx:FFFF.
            int part2Source = sourceBank << 16 | unchecked((ushort)(sourceOffset + part1Size));
            vram.ExecuteQueuedWrite(bus, part2Source, part2Size, part2Destination);
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | (bus.ReadByte(AddWithinBank(address, 1)) << 8));

}
