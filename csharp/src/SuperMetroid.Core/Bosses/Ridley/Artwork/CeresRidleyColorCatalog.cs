using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Editable Ceres Ridley, shared Norfair Ridley health, and private Baby draw colors.</summary>
public sealed class CeresRidleyColorCatalog
{
    /// <summary>Startup colors also supply the unchanged retreat door/container entries.</summary>
    internal readonly CeresRidleyStartColorDefinitions start;
    /// <summary>Three-color fade rows used by the eyes and the retreat background composition.</summary>
    internal readonly CeresRidleyFadeColorDefinitions eyeFade;
    /// <summary>Eleven-color fade rows shared by the body shadow and retreat background.</summary>
    internal readonly CeresRidleyFadeColorDefinitions bodyFade;
    /// <summary>Health paint resolves its unchanged colors through the body and eye fade families.</summary>
    internal readonly CeresRidleyHealthPaintDefinitions health;
    /// <summary>The reflected self-destruct text cycle is independent of Ridley's body fades.</summary>
    internal readonly CeresRidleyAlarmColorDefinitions alarm;
    /// <summary>Only retreat BG colors that differ from the body, eye, and neutral composition.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> retreatBg = [];
    /// <summary>Only supplied differences from the first eight door paints: native $A6:AA01-AA10 repeats $A6:E171-E180.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> retreatShared = [];
    /// <summary>Private Baby draw shades retain their link to the body fade family.</summary>
    internal readonly CeresBabyPaintDefinitions baby;

    /// <summary>Builds the selected Ceres Ridley colours from their rows; a missing alarm or Baby family, which older overrides predate, is taken from <paramref name="stock"/>.</summary>
    /// <param name="rows">The selected colour rows.</param>
    /// <param name="stock">The current stock colours a missing family is inherited from.</param>
    internal static CeresRidleyColorCatalog Create(CeresRidleyColorRows rows, CeresRidleyColorCatalog? stock)
    {
        Ensure.NotNull(rows);
        var compiledBody = new CeresRidleyFadeColorDefinitions(CeresRidleyFadeKind.Body, rows.BodyFade);
        return new(new CeresRidleyColorParts
        {
            Start = rows.Start,
            EyeFade = new CeresRidleyFadeColorDefinitions(CeresRidleyFadeKind.Eyes, rows.EyeFade),
            BodyFade = compiledBody,
            Health = rows.Health,
            Alarm = rows.Alarm is null ? Ensure.NotNull(stock).alarm : new CeresRidleyAlarmColorDefinitions(rows.Alarm),
            RetreatBg = rows.RetreatBg,
            RetreatShared = rows.RetreatShared,
            Baby = rows.Baby is null ? Ensure.NotNull(stock).baby : new CeresBabyPaintDefinitions(rows.Baby, compiledBody),
        });
    }

    /// <summary>Retains independent palette edits and collapses unchanged retreat colors onto their startup and fade owners.</summary>
    internal CeresRidleyColorCatalog(CeresRidleyColorParts parts)
    {
        this.start = new(parts.Start, parts.Baby);
        this.eyeFade = parts.EyeFade;
        this.bodyFade = parts.BodyFade;
        this.health = new(parts.Health, parts.BodyFade, parts.EyeFade);
        this.alarm = parts.Alarm;
        for (int color = 0; color < parts.RetreatBg.Length; color++)
            if (parts.RetreatBg[color] != CalculateRetreatBg(color)) this.retreatBg.Add(new ColourCell(0, color), parts.RetreatBg[color]);
        // The retreat copies the normal door/container paints into both drawing domains.
        // Keep only independent supplied differences from the existing startup owner.
        for (int color = 0; color < parts.RetreatShared.Length; color++)
            if (parts.RetreatShared[color] != this.start.Resolve(color + 1))
                this.retreatShared.Add(new ColourCell(0, color), parts.RetreatShared[color]);
        this.baby = parts.Baby;
    }

    /// <summary>Returns one BGR555 retreat BG color, index 0..14, preserving supplied edits over the composition of body-fade row one, eye-fade row fourteen, and the neutral tail colors.</summary>
    public RgbColor ResolveRetreatBg(int color)
    {
        if ((uint)color >= CeresRidleyPaletteDefinitions.RetreatBgColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return retreatBg.TryGetValue(new ColourCell(0, color), out RgbColor edited) ? edited : CalculateRetreatBg(color);
    }
    /// <summary>Composes the native retreat background from body-fade row one, eye-fade row fourteen, and its neutral final color.</summary>
    private RgbColor CalculateRetreatBg(int color) => color < 11 ? bodyFade.Resolve(1, color)
        : color < 14 ? eyeFade.Resolve(14, color - 11) : CeresRidleyFloorPaintDefinitions.RetreatNeutral;
    /// <summary>Returns one BGR555 retreat shared color, index 0..7; unedited values reuse startup colors 1..8, matching native $A6:AA01 and $A6:E171.</summary>
    public RgbColor ResolveRetreatShared(int color)
    {
        if ((uint)color >= CeresRidleyPaletteDefinitions.RetreatSharedColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return retreatShared.TryGetValue(new ColourCell(0, color), out RgbColor edited) ? edited : start.Resolve(color + 1);
    }

    /// <summary>Copies the 32 startup colors from native $A6:E16F into CGRAM entries 160..191, initializing the door/container and Baby OBJ palettes.</summary>
    public void ApplyStart(GameplayColours colours)
    {
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.StartColorCount; color++)
            new RidleyColours(colours).CeresStart.Write(color, start.Resolve(color));
    }

    /// <summary>Copies one eye-fade row, index 0..15, to CGRAM entries 252..254; native $A6:E2AA's three-color rows fade from full eye paint to black.</summary>
    public void ApplyEyeFade(GameplayColours colours, int row)
    {
        eyeFade.ValidateRow(row);
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.EyeFadeColorCount; color++)
            new RidleyColours(colours).CeresEyeFade.Write(color, eyeFade.Resolve(row, color));
    }

    /// <summary>Copies one eleven-color body-fade row, index 0..15, to both BG CGRAM entries 145..155 and OBJ entries 241..251; native $A6:E30A progresses from black to full body paint.</summary>
    public void ApplyBodyFade(GameplayColours colours, int row)
    {
        bodyFade.ValidateRow(row);
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.BodyFadeColorCount; color++)
            new RidleyColours(colours).CeresBodyFadeShadow.Write(color, bodyFade.Resolve(row, color));
        for (int color = 0; color < CeresRidleyPaletteDefinitions.BodyFadeColorCount; color++)
            new RidleyColours(colours).CeresBodyFade.Write(color, bodyFade.Resolve(row, color));
    }
    /// <summary>Copies one fourteen-color health row, index 0..2, from the shared $A6:E46A palette family to CGRAM entries 241..254; encounter AI separately selects by Ceres shot count or Norfair health.</summary>
    public void ApplyHealth(GameplayColours colours, int row)
    {
        _ = health.Resolve(row, 0);
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.HealthColorCount; color++)
            new RidleyColours(colours).Health.Write(color, health.Resolve(row, color));
    }

    /// <summary>Copies one self-destruct EMERGENCY-text phase, index 0..15, to CGRAM entries 97..99, preserving the reflected three-color cycle at native $A6:C1DF.</summary>
    public void ApplyAlarm(GameplayColours colours, int row)
    {
        _ = CeresRidleyAlarmColorDefinitions.SourceRow(row);
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.AlarmColorCount; color++)
            new RidleyColours(colours).CeresAlarm.Write(color, alarm.Resolve(row, color));
    }

    /// <summary>Installs the retreat's fifteen BG colors at CGRAM 81..95 and eight shared colors at both BG 33..40 and OBJ 241..248, matching $A6:A9E3/$AA01.</summary>
    public void ApplyRetreat(GameplayColours colours)
    {
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.RetreatBgColorCount; color++)
            new RidleyColours(colours).CeresRetreatBackground.Write(color, ResolveRetreatBg(color));
        for (int color = 0; color < CeresRidleyPaletteDefinitions.RetreatSharedColorCount; color++)
        {
            RgbColor selected = ResolveRetreatShared(color);
            new RidleyColours(colours).CeresRetreatSharedBackground.Write(color, selected);
            new RidleyColours(colours).CeresRetreatSharedObject.Write(color, selected);
        }
    }

    /// <summary>Copies one private Baby/container draw shade, index 0..3, from native $A6:E1F1 to OBJ palette-three entries 177..191; row selection and pose timing remain owned by the Baby draw callback.</summary>
    public void ApplyBaby(GameplayColours colours, int row)
    {
        _ = baby.Resolve(row, 0);
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.BabyColorCount; color++)
            new RidleyColours(colours).CeresBaby.Write(color, baby.Resolve(row, color));
    }
}
