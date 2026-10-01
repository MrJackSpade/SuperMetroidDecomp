using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Extracts supported extended enemy visual components without exporting their
/// native hitbox pointers or any instruction/callback words.
/// </summary>
internal static class EnemyExtendedFrameFiles
{
    internal static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new Dictionary<string, EnemyExtendedVisualComponent[]>(
            StringComparer.Ordinal);
        foreach (EnemyExtendedFrameDefinition definition in
                 EnemyExtendedFrameDefinitions.Frames)
        {
            if (!frames.TryAdd(definition.Name, ExtractComponents(bus, definition)))
                throw new InvalidDataException($"Extended enemy frame {definition.Name} repeats an identity.");
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyExtendedFrameDocument
            {
                Version = EnemyExtendedFrameDefinitions.Version,
                Frames = frames,
                DisplayFrames = EnemyExtendedFrameDefinitions.Frames.ToArray()
                    .ToDictionary(frame => frame.Name, frame => frame.Name,
                        StringComparer.Ordinal),
            }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            });
        _ = EnemyExtendedFrameCatalog.Load(new MemoryStream(json, writable: false));
        return json;
    }

    /// <summary>Decodes one declared visual root without importing its gameplay-owned hitboxes.</summary>
    internal static EnemyExtendedVisualComponent[] ExtractComponents(
        ISnesAddressSpace bus, EnemyExtendedFrameDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte bank = definition.Bank;
        ushort pointer = definition.Pointer;
        int baseAddress = (bank << 16) | pointer;
        int count = bus.ReadCartridgeByte(baseAddress);
        byte padding = bus.ReadCartridgeByte((bank << 16) |
            unchecked((ushort)(pointer + 1)));
        // Ceres steam's header is $1001: the generic drawing/collision
        // walkers consume its low byte as one component and retain the
        // high byte as native presentation metadata.
        byte expectedPadding = definition.Name.StartsWith("ceres_steam_oam_",
            StringComparison.Ordinal) ? (byte)0x10 : (byte)0;
        bool motherBrainMixed = bank == MotherBrainBodyVisualDefinitions.Bank &&
            MotherBrainBodyVisualDefinitions.HasBg2(pointer);
        bool bg2Only = EnemyExtendedFrameDefinitions.IsBg2Only(definition);
        int maximumNativeComponents = motherBrainMixed
            ? MotherBrainBodyVisualDefinitions.MaximumNativeComponents
            : bg2Only ? bank == PhantoonBg2FrameDefinitions.Bank
                ? PhantoonBg2FrameDefinitions.MaximumComponents : DraygonBg2FrameDefinitions.MaximumComponents
                : EnemyExtendedFrameDefinitions.MaximumOamComponents(definition);
        if (count < 1 || count > maximumNativeComponents ||
            padding != expectedPadding)
            throw new InvalidDataException(
                $"Extended frame ${bank:X2}:{pointer:X4} has invalid component header.");
        var components = new List<EnemyExtendedVisualComponent>(count);
        for (int index = 0; index < count; index++)
        {
            ushort record = unchecked((ushort)(pointer + 2 + index * 8));
            short x = unchecked((short)ReadWord(bus, bank, record));
            short y = unchecked((short)ReadWord(bus, bank,
                unchecked((ushort)(record + 2))));
            ushort spritePointer = ReadWord(bus, bank,
                unchecked((ushort)(record + 4)));
            if (ReadWord(bus, bank, spritePointer) ==
                EnemyBg2FrameLayout.StreamMarker)
            {
                // Multipart bodies combine ordinary OAM with BG2 streams
                // in one native extended root. Its BG2 half is extracted
                // separately, never misrepresented as sprite artwork.
                if (bg2Only || motherBrainMixed || definition.Name.StartsWith("crocomire_body_oam_",
                        StringComparison.Ordinal))
                    continue;
                throw new InvalidDataException(
                    $"Extended frame ${bank:X2}:{pointer:X4} contains a BG2 command, not OAM.");
            }
            if (bg2Only)
                throw new InvalidDataException(
                    $"BG2-only frame ${bank:X2}:{pointer:X4} unexpectedly contains ordinary OAM.");
            components.Add(new EnemyExtendedVisualComponent
            {
                OffsetX = x,
                OffsetY = y,
                Parts = EnemySpritemapFiles.ExtractParts(bus, bank,
                    spritePointer),
            });
        }
        if (components.Count == 0 && !bg2Only)
            throw new InvalidDataException($"Extended enemy frame {definition.Name} has no OAM.");
        return components.ToArray();
    }

    private static ushort ReadWord(ISnesAddressSpace bus, byte bank, ushort address) =>
        (ushort)(bus.ReadCartridgeByte((bank << 16) | address) |
            bus.ReadCartridgeByte((bank << 16) | unchecked((ushort)(address + 1))) << 8);
}
