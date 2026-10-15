using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Hardware;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>Walks each title card's native bank-$8B list beside its typed steps.</summary>
internal static class TitleCardOracle
{
    /// <summary>The four title cards, in authored order.</summary>
    internal static TitleTextSequenceDefinition[] Cards { get; } =
    [
        TitleSequenceRomData.TextSequences.Year, TitleSequenceRomData.TextSequences.Nintendo,
        TitleSequenceRomData.TextSequences.Presents, TitleSequenceRomData.TextSequences.MetroidThree,
    ];

    /// <summary>
    /// Requires every card's typed steps to match its native list word for word: each timed frame's duration and
    /// spritemap, then its scene trigger and deletion. Returns the number of steps compared.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the title cards' native lists")]
    internal static int CompareAll(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int compared = 0;
        foreach (TitleTextSequenceDefinition card in Cards)
        {
            int address = card.InstructionAddress;
            foreach (TitleCardStep step in TitleCardDefinitions.Steps(card.Phase))
            {
                ushort word = cartridge.ReadCartridgeWord(address);
                string at = $"{card.Phase} at ${address:X6}";
                if (step.Command is { } command)
                {
                    AssertEqual((ushort)command, word, $"{at}: command");
                    address += TitleSequenceRomData.TextSequences.InstructionWordByteCount;
                }
                else
                {
                    AssertEqual(step.Duration, word, $"{at}: duration");
                    AssertEqual(step.Spritemap, cartridge.ReadCartridgeWord(address + sizeof(ushort)), $"{at}: spritemap");
                    address += TitleSequenceRomData.TextSequences.TimedEntryByteCount;
                }
                compared++;
            }
        }
        return compared;
    }
}
