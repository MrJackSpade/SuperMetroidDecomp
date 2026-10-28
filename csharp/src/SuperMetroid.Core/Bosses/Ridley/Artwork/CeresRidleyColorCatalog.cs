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

    /// <summary>Copies one eye-fade row, index 0..15, to CGRAM entries 252..254; native $A6:E2AA's three-color rows fade from full eye paint to black.</summary>
    public void ApplyEyeFade(GameplayColours colours, int row) => CeresRidleyColors.ApplyEyeFade(this, colours, row);

    /// <summary>Copies one eleven-color body-fade row, index 0..15, to both BG CGRAM entries 145..155 and OBJ entries 241..251; native $A6:E30A progresses from black to full body paint.</summary>
    public void ApplyBodyFade(GameplayColours colours, int row) => CeresRidleyColors.ApplyBodyFade(this, colours, row);
    /// <summary>Copies one fourteen-color health row, index 0..2, from the shared $A6:E46A palette family to CGRAM entries 241..254; encounter AI separately selects by Ceres shot count or Norfair health.</summary>
    public void ApplyHealth(GameplayColours colours, int row) => CeresRidleyColors.ApplyHealth(this, colours, row);

    /// <summary>Copies one self-destruct EMERGENCY-text phase, index 0..15, to CGRAM entries 97..99, preserving the reflected three-color cycle at native $A6:C1DF.</summary>
    public void ApplyAlarm(GameplayColours colours, int row) => CeresRidleyColors.ApplyAlarm(this, colours, row);

    /// <summary>Installs the retreat's fifteen BG colors at CGRAM 81..95 and eight shared colors at both BG 33..40 and OBJ 241..248, matching $A6:A9E3/$AA01.</summary>
    public void ApplyRetreat(GameplayColours colours) => CeresRidleyColors.ApplyRetreat(this, colours);

    /// <summary>Copies one private Baby/container draw shade, index 0..3, from native $A6:E1F1 to OBJ palette-three entries 177..191; row selection and pose timing remain owned by the Baby draw callback.</summary>
    public void ApplyBaby(GameplayColours colours, int row) => CeresRidleyColors.ApplyBaby(this, colours, row);
}
