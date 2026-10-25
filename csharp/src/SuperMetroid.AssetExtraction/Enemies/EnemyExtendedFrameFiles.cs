using SuperMetroid.Core.World.Artwork;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Extracts supported extended enemy visual components without exporting their
/// native hitbox pointers or any instruction/callback words.
/// </summary>
[CpuBusBoundary("codec: ROM extended enemy spritemap component records → enemy extended frame files")]
internal static class EnemyExtendedFrameFiles
{
    /// <summary>JSON options: Camel-case property names, indented output.</summary>
    private static readonly JsonSerializerOptions CamelCaseIndentedJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    /// <summary>Exports declared extended OAM roots as editable JSON and reloads the result to validate the produced document.</summary>
    internal static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var frames = new Dictionary<string, EnemyExtendedVisualComponent[]>(
            StringComparer.Ordinal);
        foreach (EnemySpritemapDefinition definition in
                 EnemyExtendedFrameDefinitions.Frames)
        {
            if (!frames.TryAdd(definition.Name, ExtractComponents(bus, definition)))
                throw new InvalidDataException($"Extended enemy frame {definition.Name} repeats an identity.");
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyExtendedFrameDocument
            {
                Version = (int)EnemyExtendedFrameSchema.Current,
                Frames = frames,
                DisplayFrames = EnemyExtendedFrameDefinitions.Frames
                    .ToDictionary(frame => frame.Name, frame => frame.Name,
                        StringComparer.Ordinal),
            }, CamelCaseIndentedJson);
        _ = EnemyExtendedFrameDocuments.Load(new MemoryStream(json, writable: false));
        return json;
    }

    /// <summary>$10: the high byte of Ceres steam's spritemap headers, cartridge drawing metadata after the component count (#1415).</summary>
    private const byte CeresSteamHeaderHighByte = 16;

    /// <summary>Decodes one declared visual root without importing its gameplay-owned hitboxes.</summary>
    internal static EnemyExtendedVisualComponent[] ExtractComponents(
        ISnesAddressSpace bus, EnemySpritemapDefinition definition)
    {
        Ensure.NotNull(bus);
        SnesAddress location = EnemyFrameCartridgeLookup.Of(definition.Frame);
        byte bank = location.Bank;
        ushort pointer = location.Offset;
        int baseAddress = ((int)new SnesAddress(bank, pointer));
        int count = bus.ReadCartridgeByte(baseAddress);
        byte padding = bus.ReadCartridgeByte(((int)new SnesAddress(bank, unchecked((ushort)(pointer + 1)))));
        // Ceres steam's header is $1001: the generic drawing/collision
        // walkers consume its low byte as one component and retain the
        // high byte as native presentation metadata.
        byte expectedPadding = definition.Name.StartsWith("ceres_steam_oam_",
            StringComparison.Ordinal) ? CeresSteamHeaderHighByte : (byte)0;
        bool motherBrainMixed = MotherBrainBodyVisualDefinitions.HasBg2(definition.Frame);
        bool bg2Only = EnemyExtendedFrameDefinitions.IsBg2Only(definition);
        int maximumNativeComponents = motherBrainMixed
            ? MotherBrainBodyVisualDefinitions.MaximumComponents
            : bg2Only ? PhantoonBg2FrameDefinitions.IsFrame(definition.Frame)
                ? PhantoonBg2FrameDefinitions.MaximumComponents : DraygonBg2FrameDefinitions.MaximumComponents
                : EnemyExtendedFrameDefinitions.MaximumSpriteComponents(definition);
        if (count < 1 || count > maximumNativeComponents ||
            padding != expectedPadding)
            throw new InvalidDataException(
                $"Extended frame ${bank:X2}:{pointer:X4} has invalid component header.");
        var components = new List<EnemyExtendedVisualComponent>(count);
        for (int index = 0; index < count; index++)
        {
            ushort record = unchecked((ushort)(pointer + 2 + index * 8));
            short x = unchecked((short)CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, record))));
            short y = unchecked((short)CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, unchecked((ushort)(record + 2))))));
            ushort spritePointer = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, unchecked((ushort)(record + 4)))));
            if (CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, spritePointer))) ==
                EnemySourceDefinitions.Bg2StreamMarker)
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
}
