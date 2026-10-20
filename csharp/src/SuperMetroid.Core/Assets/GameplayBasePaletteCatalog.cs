using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable starting CGRAM image and common sprite colors restored on room entry.</summary>
public sealed class GameplayBasePaletteCatalog
{
    /// <summary>Owned 256-color CGRAM image loaded at gameplay startup.</summary>
    internal readonly Bgr555[] initial;
    /// <summary>Owned sixteen-color palette reused by common room sprites.</summary>
    internal readonly Bgr555[] commonSprites;

    /// <summary>Retains validated RGB5 arrays detached from the editable document.</summary>
    private GameplayBasePaletteCatalog(Bgr555[] initial, Bgr555[] commonSprites)
    {
        this.initial = initial;
        this.commonSprites = commonSprites;
    }

    /// <summary>Identity of the decoded selected colors, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(GameplayBasePaletteCatalog), content =>
    {
        content.AppendColors("initial", initial);
        content.AppendColors("common sprites", commonSprites);
    });

    /// <summary>Loads the complete selected 256-color starting image into CGRAM.</summary>
    public void LoadInitial(IPaletteColors cgram)
    {
        Ensure.NotNull(cgram);
        new CgramPalettes(cgram).LoadImage(initial);
    }

    /// <summary>Loads the selected 16-color common-sprite palette at a CGRAM color index.</summary>
    [PpuLayoutBoundary("codec: room-load sprite palette copies at their native CGRAM index operands")]
    public void LoadCommonSprites(IPaletteColors cgram, int destination)
    {
        Ensure.NotNull(cgram);
        for (int color = 0; color < commonSprites.Length; color++)
            new CgramPalettes(cgram).All.Write(destination + color, commonSprites[color]);
    }

    /// <summary>Copies the 16 enemy-projectile colors from the initial image to a CGRAM color index.</summary>
    [PpuLayoutBoundary("codec: room-load sprite palette copies at their native CGRAM index operands")]
    public void LoadEnemyProjectileSprites(IPaletteColors cgram, int destination)
    {
        Ensure.NotNull(cgram);
        for (int color = 0; color < GameplayBasePaletteFormat.SpriteColorCount; color++)
            new CgramPalettes(cgram).All.Write(destination + color,
                initial[GameplayBasePaletteFormat.EnemyProjectileInitialColor + color]);
    }

    /// <summary>Loads and validates the full initial and common-sprite RGB5 palettes.</summary>
    public static GameplayBasePaletteCatalog Load(Stream json)
    {
        GameplayBasePaletteDocument document = JsonAssetDocument.Read<GameplayBasePaletteDocument>(
            json, GameplayBasePaletteFormat.JsonOptions, "gameplay base palette");
        if (document.Version != GameplayBasePaletteFormat.Version)
            throw new InvalidDataException("Gameplay base palette has an unsupported version.");
        return new GameplayBasePaletteCatalog(
            Compile(document.Initial, CgramPalettes.ColorCount, "initial"),
            Compile(document.CommonSprites, GameplayBasePaletteFormat.SpriteColorCount,
                "commonSprites"));
    }

    /// <summary>Validates and serializes a gameplay base-palette document as JSON.</summary>
    public static byte[] Write(GameplayBasePaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            GameplayBasePaletteFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Checks palette length and channel bounds before converting authoring colors to packed CGRAM values.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? source, int expected, string name)
    {
        if (source is null || source.Length != expected)
            throw new InvalidDataException($"Gameplay {name} palette requires {expected} RGB5 colors.");
        var result = new Bgr555[expected];
        for (int color = 0; color < expected; color++)
        {
            PaletteRgb5 entry = source[color] ??
                throw new InvalidDataException($"Gameplay {name} color {color} is null.");
            if ((uint)entry.Red > 31 || (uint)entry.Green > 31 || (uint)entry.Blue > 31)
                throw new InvalidDataException($"Gameplay {name} color {color} exceeds RGB5.");
            result[color] = entry.ToBgr555();
        }
        return result;
    }
}
