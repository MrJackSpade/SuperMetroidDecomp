using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// Exact bus test double (#629) that proves which cartridge bytes a definition, renderer, importer or
/// gameplay path reads. Reads are checked against ordered rules: a forbid rule counts the attempt and
/// throws its configured failure; an observe rule records the read and continues. A read no rule rejects
/// is served by the wrapped source through the reference reader, or fails when the guard is unbacked.
/// Writes are checked against reject rules, then forwarded to the source or, when unbacked, recorded.
/// The guard is a full <see cref="ISnesMemoryBus"/> so production paths that require live memory can run
/// over it: WRAM and SRAM reads pass through to the source's mutable memory unchecked.
/// </summary>
[CpuBusBoundary("exact bus test: guard double that forbids, observes and counts reads of declared CPU-bus ranges")]
internal class CartridgeReadGuard : ISnesMemoryBus, IImportCartridgeSource
{
    /// <summary>Backing cartridge and mutable memory; absent for a guard that requires every read to be rejected.</summary>
    private readonly ISnesAddressSpace? source;
    /// <summary>Ordered forbid and observe rules applied before forwarding each read.</summary>
    private readonly List<ReadRule> readRules = [];
    /// <summary>Reject rules checked before any write reaches the backing bus.</summary>
    private readonly List<WriteRule> writeRules = [];

    /// <summary>Guards <paramref name="source"/>, which serves every read and write no rule rejects.</summary>
    internal CartridgeReadGuard(ISnesAddressSpace source) =>
        this.source = Ensure.NotNull(source);

    /// <summary>Creates the unbacked guard used to prove an installed path makes no accepted cartridge reads.</summary>
    private CartridgeReadGuard()
    {
    }

    /// <summary>A guard with no backing bus: every read must be rejected by a rule, and accepted writes are recorded.</summary>
    internal static CartridgeReadGuard Unbacked() => new();

    /// <summary>How many reads a forbid rule rejected.</summary>
    internal int ForbiddenReadAttempts { get; private set; }

    /// <summary>The table words whose bytes an <see cref="ObserveWords"/> rule saw read.</summary>
    internal HashSet<ushort> ObservedWords { get; } = [];

    /// <summary>Writes accepted by an unbacked guard, by CPU address.</summary>
    internal Dictionary<int, byte> Writes { get; } = [];

    /// <summary>Rejects every read <paramref name="match"/> selects with <paramref name="failure"/>.</summary>
    internal CartridgeReadGuard Forbid(Func<int, bool> match, Func<int, Exception> failure)
    {
        Ensure.NotNull(match);
        Ensure.NotNull(failure);
        readRules.Add(new(match, failure, null));
        return this;
    }

    /// <summary>Rejects every read <paramref name="match"/> selects with an <see cref="InvalidOperationException"/>.</summary>
    internal CartridgeReadGuard Forbid(Func<int, bool> match, Func<int, string> message)
    {
        Ensure.NotNull(message);
        return Forbid(match, address => new InvalidOperationException(message(address)));
    }

    /// <summary>Rejects reads of the <paramref name="count"/> bytes starting at <paramref name="start"/>.</summary>
    internal CartridgeReadGuard ForbidRange(int start, int count, Func<int, string> message) =>
        Forbid(address => address >= start && address < start + count, message);

    /// <summary>Rejects every read.</summary>
    internal CartridgeReadGuard ForbidAll(Func<int, string> message) => Forbid(_ => true, message);

    /// <summary>Rejects reads of either byte of any word in a bank-local word table.</summary>
    /// <param name="bankBase">The table's bank as a 24-bit base address, e.g. <c>0xA80000</c>.</param>
    /// <param name="count">Number of table words.</param>
    /// <param name="wordAddress">The bank-local address of word <c>index</c>.</param>
    /// <param name="message">The failure message for a rejected CPU address.</param>
    internal CartridgeReadGuard ForbidWords(int bankBase, int count, Func<int, ushort> wordAddress, Func<int, string> message) =>
        Forbid(address => FindWord(address, bankBase, count, wordAddress) is not null, message);

    /// <summary>Records every read <paramref name="match"/> selects and lets it continue.</summary>
    internal CartridgeReadGuard Observe(Func<int, bool> match, Action<int> observe)
    {
        Ensure.NotNull(match);
        Ensure.NotNull(observe);
        readRules.Add(new(match, null, observe));
        return this;
    }

    /// <summary>Adds to <see cref="ObservedWords"/> each bank-local table word whose low or high byte is read.</summary>
    internal CartridgeReadGuard ObserveWords(int bankBase, int count, Func<int, ushort> wordAddress) =>
        Observe(address => FindWord(address, bankBase, count, wordAddress) is not null,
            address => ObservedWords.Add((FindWord(address, bankBase, count, wordAddress) ?? throw new InvalidOperationException("Verification requires FindWord(address, bankBase, count, wordAddress)."))));

    /// <summary>Rejects every write <paramref name="match"/> selects with <paramref name="failure"/>.</summary>
    internal CartridgeReadGuard RejectWrites(Func<int, bool> match, Func<int, Exception> failure)
    {
        Ensure.NotNull(match);
        Ensure.NotNull(failure);
        writeRules.Add(new(match, failure));
        return this;
    }

    /// <summary>Rejects every write with an <see cref="InvalidOperationException"/>.</summary>
    internal CartridgeReadGuard RejectWrites(Func<int, string> message)
    {
        Ensure.NotNull(message);
        return RejectWrites(_ => true, address => new InvalidOperationException(message(address)));
    }

    /// <inheritdoc />
    public byte ReadCartridgeByte(int address) => ReadByte(address);

    /// <summary>The reference read: applies the rules, then reads the source's mapped byte.</summary>
    public byte ReadByte(int address)
    {
        Check(address);
        return CpuBusRegionRead.Read(Source, address);
    }

    /// <inheritdoc />
    public void WriteByte(int address, byte value)
    {
        foreach (WriteRule rule in writeRules)
        {
            if (rule.Match(address))
                throw rule.Failure(address);
        }
        if (source is null)
            Writes[address] = value;
        else
            source.WriteByte(address, value);
    }

    /// <inheritdoc />
    public virtual byte ReadWorkRamByte(int address) => Mutable.ReadWorkRamByte(address);

    /// <inheritdoc />
    public virtual byte ReadSaveRamByte(int address) => Mutable.ReadSaveRamByte(address);

    /// <summary>The source's WRAM/SRAM, which serves mutable-memory reads.</summary>
    private protected ISnesMutableMemory Mutable => Source as ISnesMutableMemory ?? throw new InvalidOperationException(
        $"Bus guard source {Source.GetType().Name} does not expose WRAM/SRAM.");

    /// <summary>Applies the read rules to <paramref name="address"/>.</summary>
    private protected void Check(int address)
    {
        foreach (ReadRule rule in readRules)
        {
            if (!rule.Match(address))
                continue;
            if (rule.Failure is not null)
            {
                ForbiddenReadAttempts++;
                throw rule.Failure(address);
            }
            (rule.Observe ?? throw new InvalidOperationException("Verification requires rule.Observe."))(address);
        }
    }

    /// <summary>The source serving reads no rule rejected.</summary>
    private protected ISnesAddressSpace Source => source ?? throw new InvalidOperationException(
        "Unbacked bus guard has no source for an unrejected read.");

    /// <summary>Finds the table word owning either byte of a bank-qualified CPU address.</summary>
    private static ushort? FindWord(int address, int bankBase, int count, Func<int, ushort> wordAddress)
    {
        if ((SnesAddressMath.BankBase(address)) != bankBase)
            return null;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < count; index++)
        {
            ushort word = wordAddress(index);
            if (bankAddress == word || bankAddress == unchecked((ushort)(word + 1)))
                return word;
        }
        return null;
    }

    /// <summary>Either rejects a selected read or records it before the backing source serves the byte.</summary>
    private sealed record ReadRule(Func<int, bool> Match, Func<int, Exception>? Failure, Action<int>? Observe);

    /// <summary>Rejects a selected bus write before it reaches mutable memory.</summary>
    private sealed record WriteRule(Func<int, bool> Match, Func<int, Exception> Failure);
}
