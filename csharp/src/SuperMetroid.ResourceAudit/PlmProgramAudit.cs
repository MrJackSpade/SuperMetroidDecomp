using SuperMetroid.Tooling;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit;

/// <summary>One missing or unresolved instruction word, operand, target, or artwork transfer.</summary>
/// <param name="Code">Stable category describing the audit failure.</param>
/// <param name="Owner">Instruction-list root from which the failure was discovered.</param>
/// <param name="Address">Bank-$84 address associated with the finding.</param>
/// <param name="Detail">Human-readable explanation of the missing data or unsupported boundary.</param>
internal sealed record PlmProgramFinding(string Code, string Owner, ushort Address, string Detail);
/// <summary>One instruction-list root delegated to a typed runtime owner.</summary>
internal sealed record PlmProgramClassification();

/// <summary>Finite closure of immutable PLM records. Never runs a PLM or opens a ROM.</summary>
internal sealed class PlmProgramAuditReport
{
    /// <summary>Number of instruction records decoded.</summary>
    public int Records { get; set; }
    /// <summary>Number of timed-draw records encountered.</summary>
    public int TimedDraws { get; set; }
    /// <summary>Number of 16-bit instruction operands read.</summary>
    public int WordOperands { get; set; }
    /// <summary>Number of 8-bit instruction operands read.</summary>
    public int ByteOperands { get; set; }
    /// <summary>Number of installed-artwork DMA transfers encountered.</summary>
    public int ArtworkTransfers { get; set; }
    /// <summary>Missing or unresolved instruction-program findings.</summary>
    public List<PlmProgramFinding> Findings { get; } = [];
    /// <summary>Program roots handled by identified typed runtime owners.</summary>
    public List<PlmProgramClassification> Classifications { get; } = [];
}

/// <summary>Static walker and entry points for the finite PLM instruction-program audit.</summary>
internal static class PlmProgramAudit
{
    /// <summary>Reads one compiled word at an address when present.</summary>
    internal delegate bool WordReader(ushort address, out ushort value);
    /// <summary>Reads one compiled byte at an address when present.</summary>
    internal delegate bool ByteReader(ushort address, out byte value);

    /// <summary>Audits every known PLM instruction-list root and writes optional findings JSON.</summary>
    internal static int Run(string root, string? jsonPath)
    {
        var report = new PlmProgramAuditReport();
        var source = new PlmProgramSource(root, report);
        source.VerifyInterpreter();
        PlmVramArtworkAudit.VerifySource(root, report);
        Dictionary<ushort, string> words = source.InventoryWords();
        var walker = new Walker(RoomPlmProgramDefinitions.TryReadWord,
            RoomPlmProgramDefinitions.TryReadByte, source.OwnsDraw, report,
            PlmVramArtworkAudit.OwnsInstalledTransfer);

        // Header/list identity inventories are independent of the definitions being
        // checked. A completely absent family is caught even if it exports no words.
        foreach (RoomPlmHeaderDefinition header in RoomPlmHeaderDefinitionsTooling.All)
        {
            string? route = TypedHeaderRoute(header.Header);
            if (route is not null)
                report.Classifications.Add(new());
            else walker.Visit(header.InitialInstruction, $"header ${header.Header:X4}");
        }
        FieldInfo[] instructionLists = [.. ToolingTypes.WithAdapter(typeof(RoomPlmInstructionLists))
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))];
        foreach (FieldInfo field in instructionLists)
        {
            if (!field.IsLiteral || field.FieldType != typeof(ushort)) continue;
            ushort address = (ushort)field.GetRawConstantValue()!;
            string? route = TypedListRoute(field.Name);
            if (route is not null) report.Classifications.Add(new());
            else walker.Visit(address, field.Name);
        }
        foreach ((ushort address, string owner) in source.AssignedConstantRoots())
        {
            // Reuse the same typed route for assignments to scroll/save identities.
            string? name = instructionLists.FirstOrDefault(field => field.IsLiteral && field.FieldType == typeof(ushort) &&
                    (ushort)field.GetRawConstantValue()! == address)?.Name;
            if (name is null || TypedListRoute(name) is null) walker.Visit(address, owner);
        }

        // Walk every disconnected compiled program component too: animation tails,
        // pre-handler wake continuations and linked lists need not appear in headers.
        // Mark byte spans, not only opcode words, because packed programs expose
        // overlapping unaligned word reads that are operands, not new instructions.
        foreach ((ushort address, string owner) in words.OrderBy(pair => pair.Key))
            if (!walker.Covered(address)) walker.Visit(address, owner);
        report.Findings.Sort((left, right) =>
        {
            int addressOrder = left.Address.CompareTo(right.Address);
            if (addressOrder != 0) return addressOrder;
            int ownerOrder = StringComparer.Ordinal.Compare(left.Owner, right.Owner);
            return ownerOrder != 0 ? ownerOrder : StringComparer.Ordinal.Compare(left.Detail, right.Detail);
        });
        Console.WriteLine($"Static PLM program audit: {words.Count} defined word positions; " +
            $"{report.Records} records; {report.TimedDraws} timed draws; " +
            $"{report.WordOperands} word / {report.ByteOperands} byte operands; " +
            $"{report.ArtworkTransfers} artwork transfers; " +
            $"{report.Classifications.Count} typed-owner roots; {report.Findings.Count} findings.");
        Console.WriteLine("No ROM, saves, inputs, gameplay or replay execution was used.");
        foreach (PlmProgramFinding finding in report.Findings.Take(40))
            Console.Error.WriteLine($"{finding.Code} $84:{finding.Address:X4} {finding.Owner}: {finding.Detail}");
        if (jsonPath is not null)
        {
            string absolute = Path.GetFullPath(jsonPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);
            File.WriteAllText(absolute, JsonSerializer.Serialize(report, AuditReport.JsonOptions) + Environment.NewLine);
            Console.WriteLine("Report: " + absolute);
        }
        return report.Findings.Count == 0 ? 0 : 1;
    }

    // These are actual alternative execution owners, not missing-data exemptions.
    // Their setup/step source is guarded alongside the generic interpreter.
    /// <summary>Identifies headers executed by specialized collectible, scroll or station handlers.</summary>
    private static string? TypedHeaderRoute(ushort header)
    {
        if (RoomPlmSystem.TryIdentifyPermanentCollectible(header, out _, out _))
            return "SetupCollectibleSlot / TryStepCollectible";
        return header switch
        {
            RoomPlmHeaders.ScrollTrigger or RoomPlmHeaders.RightwardsScrollExtension or
            RoomPlmHeaders.LeftwardsScrollExtension or RoomPlmHeaders.DownwardsScrollExtension or
            RoomPlmHeaders.UpwardsScrollExtension => "SetupScrollSlot / TryStepScrollPlm (extensions delete during setup)",
            RoomPlmHeaders.MapStation or RoomPlmHeaders.EnergyStation or
            RoomPlmHeaders.MissileStation or RoomPlmHeaders.SaveStation => "SetupStation / TryStepStation",
            _ => null,
        };
    }

    /// <summary>Identifies instruction lists owned by specialized scroll, station or treadmill handlers.</summary>
    private static string? TypedListRoute(string name) => name switch
    {
        nameof(RoomPlmInstructionLists.ScrollTriggerWaiting) or
        nameof(RoomPlmInstructionListsTooling.ScrollTriggerActivated) => "TryStepScrollPlm",
        nameof(RoomPlmInstructionLists.SaveStationIdleDraw) or
        nameof(RoomPlmInstructionLists.SaveStationAnimationFirstFrame) or
        nameof(RoomPlmInstructionLists.SaveStationAnimationSecondFrame) => "TryStepStation / SaveStationAnimationDefinitions",
        nameof(RoomPlmInstructionLists.WreckedShipEntranceTreadmillFromWest) or
        nameof(RoomPlmInstructionLists.WreckedShipEntranceTreadmillFromEast) => "TryStepWreckedShipTreadmill",
        _ => null,
    };

    /// <summary>Decodes linked PLM programs while tracking occupied bytes and checking resource operands.</summary>
    /// <param name="readWord">Reader for compiled 16-bit instruction words.</param>
    /// <param name="readByte">Reader for compiled byte operands.</param>
    /// <param name="ownsDraw">Predicate for production PLM draw definitions.</param>
    /// <param name="report">Mutable report receiving counts, classifications and findings.</param>
    /// <param name="ownsArtwork">Optional provider predicate for installed artwork transfers.</param>
    internal sealed class Walker(WordReader readWord, ByteReader readByte,
        Func<ushort, bool> ownsDraw, PlmProgramAuditReport report,
        Func<int, int, bool>? ownsArtwork = null)
    {
        /// <summary>Instruction entry addresses already decoded by this walker.</summary>
        private readonly HashSet<ushort> visited = [];
        /// <summary>Every byte in a decoded instruction or operand span.</summary>
        private readonly HashSet<ushort> covered = [];
        /// <summary>Whether an address has already been claimed by a decoded instruction span.</summary>
        internal bool Covered(ushort address) => covered.Contains(address);

        /// <summary>Walks an instruction-list root and records missing words, bad links and absent resources.</summary>
        internal void Visit(ushort root, string owner)
        {
            var pending = new Stack<ushort>();
            pending.Push(root);
            while (pending.TryPop(out ushort cursor))
            {
                if (!visited.Add(cursor)) continue;
                if (covered.Contains(cursor))
                {
                    Missing("invalid-target", cursor, "Execution enters a previously decoded operand, not a record boundary.");
                    continue;
                }
                if (!readWord(cursor, out ushort opcode))
                {
                    Missing("missing-word", cursor, "Instruction/branch target has no compiled word.");
                    continue;
                }
                report.Records++;
                if ((opcode & RoomPlmMemoryLayoutTooling.RoutineWordMask) == 0)
                {
                    report.TimedDraws++;
                    if (RequireWord(2, out ushort draw) && !ownsDraw(draw))
                        Missing("missing-draw", At(2), $"Timed draw ${draw:X4} has no production draw provider.");
                    Mark(4);
                    pending.Push(At(4));
                    continue;
                }
                PlmInstructionFormat? format = PlmInstructionFormat.Get(opcode);
                if (format is null)
                {
                    Missing("unknown-opcode", cursor, $"No guarded record format for opcode ${opcode:X4}.");
                    Mark(2);
                    continue;
                }
                foreach (int offset in format.Words)
                    if (RequireWord(offset, out ushort operand) && format.Targets.Contains(offset)) pending.Push(operand);
                foreach (int offset in format.Bytes)
                {
                    report.ByteOperands++;
                    if (!readByte(At(offset), out _)) Missing("missing-byte", At(offset), "Opcode byte operand is absent.");
                }
                if (opcode == RoomPlmInstructionCodes.CopyFromRamToVram &&
                    readWord(At(2), out ushort count) && readWord(At(4), out ushort sourceOffset) &&
                    readByte(At(6), out byte sourceBank))
                {
                    var address = new SnesAddress(sourceBank, sourceOffset);
                    SnesDmaSourceKind kind = SnesDmaSourceMap.Classify(address);
                    if (kind == SnesDmaSourceKind.Cartridge)
                    {
                        report.ArtworkTransfers++;
                        if (!(ownsArtwork ?? PlmVramArtworkAudit.OwnsInstalledTransfer)((int)address, count))
                            Missing("missing-artwork-transfer", cursor,
                                $"DMA ${(int)address:X6}, {count} bytes has no reviewed installed-artwork provider.");
                    }
                    else if (count == 0 || kind == SnesDmaSourceKind.Unmapped ||
                        Enumerable.Range(0, count).Any(offset =>
                            SnesDmaSourceMap.Classify(address.AddWithinBank(offset)) != kind))
                        Missing("unresolved-memory-transfer", cursor, "DMA is empty, unmapped or crosses memory-domain boundaries.");
                }
                Mark(format.Length);
                if (format.FallThrough) pending.Push(At(format.Length));

                ushort At(int offset) => unchecked((ushort)(cursor + offset));
                void Mark(int length) { for (int index = 0; index < length; index++) covered.Add(At(index)); }
                bool RequireWord(int offset, out ushort value)
                {
                    report.WordOperands++;
                    if (readWord(At(offset), out value)) return true;
                    Missing("missing-word", At(offset), $"Operand of record ${cursor:X4} is absent.");
                    return false;
                }
                void Missing(string code, ushort address, string detail) =>
                    report.Findings.Add(new(code, owner, address, detail));
            }
        }
    }
}
