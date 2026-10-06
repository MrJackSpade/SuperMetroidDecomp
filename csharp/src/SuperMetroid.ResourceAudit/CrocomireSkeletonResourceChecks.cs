using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified skeleton export and component-capacity omissions, not the battle.</summary>
internal static class CrocomireSkeletonResourceChecks
{
    internal static void Run()
    {
        ExtendedEnemyCompositionResourceChecks.Run("Crocomire skeleton",
            CrocomireSkeletonVisualDefinitions.Frames.ToArray(),
            EnemyExtendedFrameDefinitions.PreCrocomireSkeletonVersion,
            EnemyExtendedFrameDefinitions.PreCrocomireSkeletonFrameCount);
        ConfirmFragmentationCapacity();
    }

    private static void ConfirmFragmentationCapacity()
    {
        EnemyExtendedFrameDefinition frame = CrocomireSkeletonVisualDefinitions.Frames.ToArray()
            .Single(item => item.Pointer == CrocomireSkeletonVisualDefinitions.FirstThirteenComponentFrame);
        int count = CrocomireSkeletonVisualDefinitions.MaximumComponents;
        var source = new ConstructedComponentSource(frame, count);
        EnemyExtendedVisualComponent[] extracted = EnemyExtendedFrameFiles.ExtractComponents(source, frame);
        ExtendedEnemyCompositionResourceChecks.Require(extracted.Length == count,
            "the importer must retain all thirteen collapse components, not truncate to eight");
        EnemyExtendedFrameDocument document = ExtendedEnemyCompositionResourceChecks.CreateDocument();
        document.Frames[frame.Name] = extracted;
        EnemyExtendedFrameCatalog installed = ExtendedEnemyCompositionResourceChecks.Load(document);
        ExtendedEnemyCompositionResourceChecks.Require(installed.TryGetDisplay(frame.Bank, frame.Pointer,
            out var components) && components.Length == count, "all thirteen components must survive installation");
        for (int index = 0; index < count; index++)
        {
            var component = components.Span[index];
            ExtendedEnemyCompositionResourceChecks.Require(component.OffsetX == index && component.OffsetY == -index &&
                component.Parts.Length == 1 && component.Parts[0].X.SignedOffset == 3 && component.Parts[0].Y == 4,
                "the importer/loader must preserve component order, signed offsets and OAM pieces");
        }
        Reject(() => EnemyExtendedFrameFiles.ExtractComponents(new ConstructedComponentSource(frame, count + 1), frame),
            "the skeleton importer must still reject a fourteenth component");
        document.Frames[frame.Name] = [.. extracted, extracted[0]];
        Reject(() => ExtendedEnemyCompositionResourceChecks.Load(document),
            "the skeleton loader must still reject a fourteenth component");
        var ordinary = EnemyExtendedFrameDefinitions.Frames[0];
        int ordinaryCount = EnemyExtendedFrameDefinitions.MaximumComponents + 1;
        Reject(() => EnemyExtendedFrameFiles.ExtractComponents(new ConstructedComponentSource(ordinary, ordinaryCount), ordinary),
            "other native OAM families must retain their existing component bound");
        document.Frames[frame.Name] = extracted;
        document.Frames[ordinary.Name] = Enumerable.Repeat(extracted[0], ordinaryCount).ToArray();
        Reject(() => ExtendedEnemyCompositionResourceChecks.Load(document),
            "other installed OAM families must retain their existing component bound");
        Console.WriteLine("Crocomire collapse: importer and loader preserve thirteen ordered components; family-specific limits remain enforced.");
    }

    private static void Reject(Action action, string message)
    {
        bool rejected = false;
        try { action(); }
        catch (InvalidDataException) { rejected = true; }
        ExtendedEnemyCompositionResourceChecks.Require(rejected, message);
    }

    /// <summary>Synthetic import bytes for exactly one root; unspecified reads, including hitboxes, fail.</summary>
    private sealed class ConstructedComponentSource : ISnesAddressSpace, IImportCartridgeSource
    {
        private readonly Dictionary<int, byte> bytes = [];

        public ConstructedComponentSource(EnemyExtendedFrameDefinition frame, int count)
        {
            int address = (frame.Bank << 16) | frame.Pointer;
            int sprite = (frame.Bank << 16) | 0x8000;
            Word(address, count);
            for (int index = 0; index < count; index++)
            {
                int record = address + 2 + index * 8;
                Word(record, index);
                Word(record + 2, -index);
                Word(record + 4, sprite & 0xffff);
                // No hitbox bytes: extracting editable art must not read native mechanics.
            }
            Word(sprite, 1);
            Word(sprite + 2, 3);
            bytes[sprite + 4] = 4;
            Word(sprite + 5, 0);
        }

        private void Word(int address, int value)
        {
            bytes[address] = unchecked((byte)value);
            bytes[address + 1] = unchecked((byte)(value >> 8));
        }

        public byte ReadCartridgeByte(int cpuAddress) => bytes.TryGetValue(cpuAddress, out byte value)
            ? value : throw new InvalidDataException($"Unexpected synthetic composition read {cpuAddress:X6}.");
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Static import must not mutate emulated state.");
    }
}
