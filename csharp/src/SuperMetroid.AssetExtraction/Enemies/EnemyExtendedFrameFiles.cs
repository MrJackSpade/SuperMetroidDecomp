using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Extracts supported extended enemy visual components without exporting their
/// native hitbox pointers or any instruction/callback words.
/// </summary>
internal static partial class EnemyExtendedFrameFiles
{
    /// <summary>JSON options: Camel-case property names, indented output.</summary>
    private static readonly JsonSerializerOptions CamelCaseIndentedJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    /// <summary>Exports declared extended OAM roots as editable JSON and reloads the result to validate the produced document.</summary>
    internal static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var frames = new Dictionary<string, EnemyExtendedVisualComponent[]>(
            StringComparer.Ordinal);
        foreach (EnemyFrameName definition in
                 EnemyExtendedFrameDefinitions.Frames)
        {
            if (!frames.TryAdd(definition.Name, ExtractComponents(bus, definition)))
                throw new InvalidDataException($"Extended enemy frame {definition.Name} repeats an identity.");
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new EnemyExtendedFrameDocument
            {
                Version = EnemyExtendedFrameSchema.Revision,
                Frames = frames,
                DisplayFrames = EnemyFrameBindingRules.Identity(EnemyExtendedFrameDefinitions.Frames),
            }, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Enemies.EnemyExtendedFrameDocument>(CamelCaseIndentedJson));
        return json;
    }

    /// <summary>Decodes one declared visual root without importing its gameplay-owned hitboxes.</summary>
    internal static EnemyExtendedVisualComponent[] ExtractComponents(
        ISnesAddressSpace bus, EnemyFrameName definition)
    {
        Ensure.NotNull(bus);
        SnesAddress location = EnemyFrameCartridgeLookup.Of(definition.Frame);
        int bank = location.Bank;
        int pointer = location.Offset;
        int baseAddress = ((int)SnesAddress.InBank(bank, pointer));
        int count = bus.ReadCartridgeByte(baseAddress);
        int padding = bus.ReadCartridgeByte(((int)SnesAddress.InBank(bank, unchecked((ushort)(pointer + 1)))));
        // A header's byte after the component count is drawing metadata its run states (Ceres steam's header is $1001):
        // the generic drawing/collision walkers consume its low byte as one component and retain the high byte.
        EnemyExtendedFrameRun run = EnemyExtendedFrameFamilies.RunOf(definition.Frame);
        bool bg2Only = run.Layers == EnemyExtendedFrameLayers.Bg2Only;
        bool besideBg2 = run.Layers == EnemyExtendedFrameLayers.SpritesBesideBg2;
        if (count < 1 || count > run.MaximumComponents ||
            padding != run.HeaderHighByte)
            throw new InvalidDataException(
                $"Extended frame ${bank:X2}:{pointer:X4} has invalid component header.");
        var components = new List<EnemyExtendedVisualComponent>(count);
        for (int index = 0; index < count; index++)
        {
            int record = unchecked((ushort)(pointer + 2 + index * 8));
            int x = unchecked((short)CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)SnesAddress.InBank(bank, record))));
            int y = unchecked((short)CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)SnesAddress.InBank(bank, unchecked((ushort)(record + 2))))));
            int spritePointer = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)SnesAddress.InBank(bank, unchecked((ushort)(record + 4)))));
            if (CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)SnesAddress.InBank(bank, spritePointer))) ==
                EnemySourceDefinitions.Bg2StreamMarker)
            {
                // Multipart bodies combine ordinary OAM with BG2 streams
                // in one native extended root. Its BG2 half is extracted
                // separately, never misrepresented as sprite artwork.
                if (bg2Only || besideBg2)
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
