using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;

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
    internal readonly Dictionary<ColourCell, Bgr555> retreatBg = [];
    /// <summary>Only supplied differences from the first eight door paints: native $A6:AA01-AA10 repeats $A6:E171-E180.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> retreatShared = [];
    /// <summary>Private Baby draw shades retain their link to the body fade family.</summary>
    internal readonly CeresBabyPaintDefinitions baby;

    /// <summary>Builds the selected Ceres Ridley colours from their rows; a missing alarm or Baby family, which older overrides predate, is taken from <paramref name="stock"/>.</summary>
    /// <param name="start">The startup colours.</param>
    /// <param name="eyeFade">The eye fade rows.</param>
    /// <param name="bodyFade">The body fade rows.</param>
    /// <param name="health">The health rows.</param>
    /// <param name="alarm">The alarm rows, or null to inherit the stock alarm colours.</param>
    /// <param name="retreatBg">The retreat background colours.</param>
    /// <param name="retreatShared">The retreat colours shared with the startup door paints.</param>
    /// <param name="baby">The Baby Metroid rows, or null to inherit the stock Baby colours.</param>
    /// <param name="stock">The current stock colours a missing family is inherited from.</param>
    internal static CeresRidleyColorCatalog Create(Bgr555[] start, Bgr555[][] eyeFade, Bgr555[][] bodyFade,
        Bgr555[][] health, Bgr555[][]? alarm, Bgr555[] retreatBg, Bgr555[] retreatShared, Bgr555[][]? baby,
        CeresRidleyColorCatalog? stock)
    {
        var compiledBody = new CeresRidleyFadeColorDefinitions(CeresRidleyFadeKind.Body, bodyFade);
        return new(new CeresRidleyColorParts
        {
            Start = start,
            EyeFade = new CeresRidleyFadeColorDefinitions(CeresRidleyFadeKind.Eyes, eyeFade),
            BodyFade = compiledBody,
            Health = health,
            Alarm = alarm is null ? Ensure.NotNull(stock).alarm : new CeresRidleyAlarmColorDefinitions(alarm),
            RetreatBg = retreatBg,
            RetreatShared = retreatShared,
            Baby = baby is null ? Ensure.NotNull(stock).baby : new CeresBabyPaintDefinitions(baby, compiledBody),
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
    public Bgr555 ResolveRetreatBg(int color)
    {
        if ((uint)color >= CeresRidleyPaletteRomData.RetreatBgColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return retreatBg.TryGetValue(new ColourCell(0, color), out Bgr555 edited) ? edited : CalculateRetreatBg(color);
    }
    /// <summary>Composes the native retreat background from body-fade row one, eye-fade row fourteen, and its neutral final color.</summary>
    private Bgr555 CalculateRetreatBg(int color) => color < 11 ? bodyFade.Resolve(1, color)
        : color < 14 ? eyeFade.Resolve(14, color - 11) : CeresRidleyMode7PaintDefinitions.RetreatNeutral;
    /// <summary>Returns one BGR555 retreat shared color, index 0..7; unedited values reuse startup colors 1..8, matching native $A6:AA01 and $A6:E171.</summary>
    public Bgr555 ResolveRetreatShared(int color)
    {
        if ((uint)color >= CeresRidleyPaletteRomData.RetreatSharedColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return retreatShared.TryGetValue(new ColourCell(0, color), out Bgr555 edited) ? edited : start.Resolve(color + 1);
    }

    /// <summary>Copies the 32 startup colors from native $A6:E16F into CGRAM entries 160..191, initializing the door/container and Baby OBJ palettes.</summary>
    public void ApplyStart(GameplayColours cgram)
    {
        Ensure.NotNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.StartColorCount; color++)
            new RidleyColours(cgram).CeresStart.Write(color, start.Resolve(color));
    }

    /// <summary>Copies one eye-fade row, index 0..15, to CGRAM entries 252..254; native $A6:E2AA's three-color rows fade from full eye paint to black.</summary>
    public void ApplyEyeFade(GameplayColours cgram, int row)
    {
        eyeFade.ValidateRow(row);
        Ensure.NotNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.EyeFadeColorCount; color++)
            new RidleyColours(cgram).CeresEyeFade.Write(color, eyeFade.Resolve(row, color));
    }

    /// <summary>Copies one eleven-color body-fade row, index 0..15, to both BG CGRAM entries 145..155 and OBJ entries 241..251; native $A6:E30A progresses from black to full body paint.</summary>
    public void ApplyBodyFade(GameplayColours cgram, int row)
    {
        bodyFade.ValidateRow(row);
        Ensure.NotNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.BodyFadeColorCount; color++)
            new RidleyColours(cgram).CeresBodyFadeShadow.Write(color, bodyFade.Resolve(row, color));
        for (int color = 0; color < CeresRidleyPaletteRomData.BodyFadeColorCount; color++)
            new RidleyColours(cgram).CeresBodyFade.Write(color, bodyFade.Resolve(row, color));
    }
    /// <summary>Copies one fourteen-color health row, index 0..2, from the shared $A6:E46A palette family to CGRAM entries 241..254; encounter AI separately selects by Ceres shot count or Norfair health.</summary>
    public void ApplyHealth(GameplayColours cgram, int row)
    {
        _ = health.Resolve(row, 0);
        Ensure.NotNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.HealthColorCount; color++)
            new RidleyColours(cgram).Health.Write(color, health.Resolve(row, color));
    }

    /// <summary>Copies one self-destruct EMERGENCY-text phase, index 0..15, to CGRAM entries 97..99, preserving the reflected three-color cycle at native $A6:C1DF.</summary>
    public void ApplyAlarm(GameplayColours cgram, int row)
    {
        _ = CeresRidleyAlarmColorDefinitions.SourceRow(row);
        Ensure.NotNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.AlarmColorCount; color++)
            new RidleyColours(cgram).CeresAlarm.Write(color, alarm.Resolve(row, color));
    }

    /// <summary>Installs the retreat's fifteen BG colors at CGRAM 81..95 and eight shared colors at both BG 33..40 and OBJ 241..248, matching $A6:A9E3/$AA01.</summary>
    public void ApplyRetreat(GameplayColours cgram)
    {
        Ensure.NotNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.RetreatBgColorCount; color++)
            new RidleyColours(cgram).CeresRetreatBackground.Write(color, ResolveRetreatBg(color));
        for (int color = 0; color < CeresRidleyPaletteRomData.RetreatSharedColorCount; color++)
        {
            Bgr555 selected = ResolveRetreatShared(color);
            new RidleyColours(cgram).CeresRetreatSharedBackground.Write(color, selected);
            new RidleyColours(cgram).CeresRetreatSharedObject.Write(color, selected);
        }
    }

    /// <summary>Copies one private Baby/container draw shade, index 0..3, from native $A6:E1F1 to OBJ palette-three entries 177..191; row selection and pose timing remain owned by the Baby draw callback.</summary>
    public void ApplyBaby(GameplayColours cgram, int row)
    {
        _ = baby.Resolve(row, 0);
        Ensure.NotNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.BabyColorCount; color++)
            new RidleyColours(cgram).CeresBaby.Write(color, baby.Resolve(row, color));
    }
}
