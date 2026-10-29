using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Editable Ceres Ridley, shared Norfair Ridley health, and private Baby draw colors.</summary>
public sealed class CeresRidleyColorCatalog
{
    /// <summary>The selected colour families.</summary>
    internal readonly CeresRidleyColorSelection selection;

    /// <summary>Retains independent palette edits and collapses unchanged retreat colors onto their startup and fade owners.</summary>
    internal CeresRidleyColorCatalog(CeresRidleyColorParts parts)
        : this(parts, new CeresRidleyStartColors(parts.Start, parts.Baby))
    {
    }

    /// <summary>Retains the compiled families, keeping only the retreat colours that differ from their owners.</summary>
    private CeresRidleyColorCatalog(CeresRidleyColorParts parts, CeresRidleyStartColors start) =>
        selection = new CeresRidleyColorSelection
        {
            Start = start,
            EyeFade = parts.EyeFade,
            BodyFade = parts.BodyFade,
            Health = new CeresRidleyHealthPaint(parts.Health, parts.BodyFade, parts.EyeFade),
            Alarm = parts.Alarm,
            RetreatBg = CeresRidleyColors.RetreatBgEdits(parts),
            RetreatShared = CeresRidleyColors.RetreatSharedEdits(parts.RetreatShared, start),
            Baby = parts.Baby,
        };

    /// <summary>Returns one BGR555 retreat BG color, index 0..14, preserving supplied edits over the composition of body-fade row one, eye-fade row fourteen, and the neutral tail colors.</summary>
    public RgbColor ResolveRetreatBg(int color) => CeresRidleyColors.RetreatBg(this, color);
    /// <summary>Returns one BGR555 retreat shared color, index 0..7; unedited values reuse startup colors 1..8, matching native $A6:AA01 and $A6:E171.</summary>
    public RgbColor ResolveRetreatShared(int color) => CeresRidleyColors.RetreatShared(this, color);

    /// <summary>Copies the 32 startup colors from native $A6:E16F into CGRAM entries 160..191, initializing the door/container and Baby OBJ palettes.</summary>
    public void ApplyStart(GameplayColours colours) => CeresRidleyColors.ApplyStart(this, colours);

    /// <summary>Shows eye-fade shade <paramref name="shade"/>, 0..15, in Ceres Ridley's eye colours; the three-colour shades fade from full eye paint to black.</summary>
    public void ApplyEyeFade(GameplayColours colours, int shade) => CeresRidleyColors.ApplyEyeFade(this, colours, shade);

    /// <summary>Shows body-fade shade <paramref name="shade"/>, 0..15, in Ceres Ridley's background and sprite body colours; the eleven-colour shades progress from black to full body paint.</summary>
    public void ApplyBodyFade(GameplayColours colours, int shade) => CeresRidleyColors.ApplyBodyFade(this, colours, shade);
    /// <summary>Shows health band <paramref name="band"/>, 0..2, in Ridley's fourteen health colours, shared by both encounters; encounter AI separately selects the band by Ceres shot count or Norfair health.</summary>
    public void ApplyHealth(GameplayColours colours, int band) => CeresRidleyColors.ApplyHealth(this, colours, band);

    /// <summary>Shows self-destruct alarm step <paramref name="step"/>, 0..15, in the three EMERGENCY-text colours, preserving the reflected three-color cycle.</summary>
    public void ApplyAlarm(GameplayColours colours, int step) => CeresRidleyColors.ApplyAlarm(this, colours, step);

    /// <summary>Installs the retreat's fifteen BG colors at CGRAM 81..95 and eight shared colors at both BG 33..40 and OBJ 241..248, matching $A6:A9E3/$AA01.</summary>
    public void ApplyRetreat(GameplayColours colours) => CeresRidleyColors.ApplyRetreat(this, colours);

    /// <summary>Shows the Baby/container's <paramref name="shade"/> in the Baby's colours; shade selection and pose timing remain owned by the Baby's animation.</summary>
    public void ApplyBaby(GameplayColours colours, CeresBabyShade shade) => CeresRidleyColors.ApplyBaby(this, colours, shade);
}
