using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Editable live-Shitroid RGB5 images; native timer and fade destinations stay compiled.</summary>
public sealed class ShitroidColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("ShitroidColorCatalog-v1", content =>
        {
            Span<RgbColor> selectedSidehopper = stackalloc RgbColor[ShitroidColorDefinitions.TargetColorCount];
            for (int color = 0; color < selectedSidehopper.Length; color++) selectedSidehopper[color] = sidehopper.Resolve(color);
            content.AppendColors("sidehopper", selectedSidehopper);
            Span<RgbColor> selectedShitroid = stackalloc RgbColor[ShitroidColorDefinitions.TargetColorCount];
            for (int color = 0; color < selectedShitroid.Length; color++) selectedShitroid[color] = TargetColor(ShitroidColorTarget.Shitroid, color);
            content.AppendColors("shitroid", selectedShitroid);
            Span<RgbColor> selectedCorpse = stackalloc RgbColor[ShitroidColorDefinitions.TargetColorCount];
            for (int color = 0; color < selectedCorpse.Length; color++) selectedCorpse[color] = deadSidehopper.Resolve(color);
            content.AppendColors("deadSidehopper", selectedCorpse);
            content.Append("normal", ShitroidColorDefinitions.NormalFrameCount);
            for (int frame = 0; frame < ShitroidColorDefinitions.NormalFrameCount; frame++)
            {
                var row = new RgbColor[ShitroidColorDefinitions.NormalColorsPerFrame];
                for (int color = 0; color < row.Length; color++) row[color] = NormalColor(frame, color);
                content.AppendColors("row", row);
            }
        });

    /// <summary>Uses the giant Baby Metroid's selected target inks when they reproduce every pulse frame; otherwise retains the supplied frames.</summary>
    internal readonly ColorPulse normal;
    /// <summary>Live victim colors copied into the Sidehopper palette during encounter setup.</summary>
    internal readonly SidehopperInitialPalette sidehopper;
    /// <summary>Visible giant Baby Metroid inks, excluding its retained transparent slot.</summary>
    private readonly BabyMetroidInitialPalette shitroid;
    /// <summary>$A9:F8E6 is copied by EF9F-EFA8 to target slot A0. Stock3800 is an exact
    /// transparent-slot compatibility payload: OBJ ink0 is skipped before color lookup.
    /// It has no visible hue to derive; independently supplied replacements remain exact.</summary>
    private readonly RgbColor shitroidTransparentSlot;
    /// <summary>Victim colors selected after the giant Baby Metroid drains the Sidehopper.</summary>
    internal readonly SidehopperCorpsePalette deadSidehopper;

    /// <summary>Separates the transparent Shitroid palette word before constructing the visible-ink resolver.</summary>
    internal ShitroidColorCatalog(RgbColor[][] normal, RgbColor[] sidehopper,
        RgbColor[] shitroid, RgbColor[] deadSidehopper)
    {
        this.shitroid = new BabyMetroidInitialPalette(shitroid.AsSpan(1).ToArray());
        this.normal = new ColorPulse(normal, this.shitroid);
        this.sidehopper = new(sidehopper);
        shitroidTransparentSlot = shitroid[0];
        this.deadSidehopper = new(deadSidehopper);
    }

    /// <summary>Gets a packed RGB5 ink from normal pulse phase 0–7 and ink 0–3, corresponding to $A9:F6D1 and written to CGRAM colors 165–168; phase cadence and cries remain actor behavior.</summary>
    public RgbColor NormalColor(int frame, int color) =>
        (uint)frame < ShitroidColorDefinitions.NormalFrameCount && (uint)color < ShitroidColorDefinitions.NormalColorsPerFrame
            ? normal.Resolve(frame, color)
            : throw new ArgumentOutOfRangeException(nameof(frame),
                $"Shitroid normal frame {frame}, color {color} is outside the authored image.");

    /// <summary>Gets packed RGB5 color 0–15 from a selected initialization target, including its retained transparent-slot word; the actor copies targets to its palette image and current CGRAM during setup.</summary>
    public RgbColor TargetColor(ShitroidColorTarget target, int color)
    {
        if (target == ShitroidColorTarget.Shitroid)
        {
            if ((uint)color >= ShitroidColorDefinitions.TargetColorCount) throw new ArgumentOutOfRangeException(nameof(color));
            return color == 0 ? shitroidTransparentSlot : shitroid.Resolve(color - 1);
        }
        if (target == ShitroidColorTarget.Sidehopper) return sidehopper.Resolve(color);
        return target == ShitroidColorTarget.DeadSidehopper ? deadSidehopper.Resolve(color) :
            throw new ArgumentOutOfRangeException(nameof(target));
    }

    /// <summary>Shares the target's symmetric dimming curve only when it matches all eight supplied pulse phases.</summary>
    internal sealed class ColorPulse
    {
        /// <summary>Visible target inks used to calculate the native pulse when the editable rows agree.</summary>
        private readonly BabyMetroidInitialPalette target;
        /// <summary>Independent pulse rows retained when an edited frame differs from the calculated curve.</summary>
        internal readonly RgbColor[][]? supplied;

        /// <summary>Compares each supplied ink with the target-derived pulse before deciding whether rows can be shared.</summary>
        public ColorPulse(RgbColor[][] frames, BabyMetroidInitialPalette target)
        {
            this.target = target;
            // Sharing the target's innard colors is valid only when every supplied
            // pulse sample agrees. Independently edited targets or frames stay exact.
            for (int frame = 0; frame < frames.Length; frame++)
            for (int color = 0; color < ShitroidColorDefinitions.NormalColorsPerFrame; color++)
                if (Resolve(frame, color) != frames[frame][color])
                { supplied = frames; return; }
        }

        /// <summary>Uses the supplied ink or mirrors the eight-phase dimming curve around its two darkest phases, retaining the organ's red floor.</summary>
        public RgbColor Resolve(int frame, int color)
        {
            if (supplied is not null) return supplied[frame][color];
            int phase = Math.Min(frame, ShitroidColorDefinitions.NormalFrameCount - 1 - frame);
            RgbColor paint = target.Resolve(ShitroidColorDefinitions.NormalFirstInitialColor + color);
            return paint.Map((channel, origin) =>
            {
                int floor = 0;
                if (channel == ColorChannel.Red)
                {
                    floor = ShitroidColorDefinitions.NormalOrganRedFloor;
                    if (origin == 31) origin += ShitroidColorDefinitions.NormalOrganRedHeadroom;
                }
                return Math.Clamp(origin - ShitroidColorDefinitions.NormalOrganDimmingStep * phase, floor, 31);
            });
        }
    }
}
