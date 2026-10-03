namespace SuperMetroid.Core.Assets;

/// <summary>Calculated repeated-row identities in the three suit-loading palettes.</summary>
/// <remarks>Supported NTSC J/U programs $8D:DB62/DCC8/DE2E contain nine
/// sixteen-color records. Every even record repeats the normal palette; record3
/// repeats record1. Records5 and7 contain distinct shades. The mapping is checked
/// against complete original rows, including transparent entries, for #1165.
/// This removes repeated rows, without assigning a retention disposition to
/// the four remaining artwork rows or caching generated colors.</remarks>
public static class LoadingPaletteColorDefinitions
{
    /// <summary>Maps a color address to its earliest identical native row.</summary>
    /// <remarks>First color words are $8D:DB6B/DCD1/DE37. Counted groups
    /// advance79 bytes with records36 bytes apart; the final row is313 bytes
    /// after the first. Only the32 color bytes in each record are owned.
    /// Non-color addresses return false and zero, including wrong byte parity.</remarks>
    public static bool TryCanonicalPointer(ushort pointer, out ushort canonical) =>
        TryProgram(pointer, 0xdb6b, out canonical) ||
        TryProgram(pointer, 0xdcd1, out canonical) ||
        TryProgram(pointer, 0xde37, out canonical);

    /// <summary>Resolves an explicit edit before following a calculated loading alias.</summary>
    internal static bool TryReadColor(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value) =>
        colors.TryGetValue(pointer, out value) ||
        (TryCanonicalPointer(pointer, out ushort canonical) &&
         (colors.TryGetValue(canonical, out value) || TryCalculatedColor(canonical, colors, out value)));

    /// <summary>Calculates the shared blue/green loading tint from normal suit colors.</summary>
    /// <remarks>Power slots3..8/13..15 and Gravity slots10/11 preserve red.
    /// Bright, middle and dim shades add green15/5/0 and blue20/20/10,
    /// saturating each RGB5 channel at31. All33 original words independently
    /// confirm this channel transformation. Varia slots1/2 use the weaker green
    /// and stronger blue tint; slots10/11 share its middle and dim shades only.
    /// Their brightest blue29 differs and remains a separate pending input.
    /// Original Power dim slots10/11 preserve normal red/green and share dim
    /// slot2's blue13. Slot9's dim follows the ordinary blue+10 tint; its middle
    /// preserves brightest blue21 while applying normal green+5. These four
    /// additional words use shared channels; the blue source inputs remain
    /// editable and pending their own disposition.
    /// Other slots have separate pending
    /// artwork/channel reviews and are not silently forced into this rule.</remarks>
    internal static bool TryCalculatedColor(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value)
    {
        value = 0;
        int first = pointer >= 0xde37 ? 0xde37 : pointer >= 0xdcd1 ? 0xdcd1 : 0xdb6b;
        int offset = pointer - first;
        if ((uint)offset >= 4 * 79) return false;
        int group = offset / 79;
        int within = offset % 79 - 36;
        if (group == 1 || (uint)within >= 32 || (within & 1) != 0) return false;
        int slot = within / 2;
        int shade = group == 0 ? 0 : group - 1;
        if (first == 0xdb6b && group == 3 && slot is 10 or 11)
        {
            if (!TryReadColor((ushort)(first + 2 * slot), colors, out ushort normal) ||
                !TryReadColor((ushort)(first + 3 * 79 + 36 + 2 * 2), colors, out ushort blueSource)) return false;
            value = (ushort)((normal & 0x03ff) | (blueSource & 0x7c00));
            return true;
        }
        if (first == 0xdb6b && slot == 9 && group != 0)
        {
            if (!TryReadColor((ushort)(first + 2 * slot), colors, out ushort normal)) return false;
            ushort tint = TintColor(normal, shade);
            if (group == 2)
            {
                if (!TryReadColor((ushort)(first + 36 + 2 * slot), colors, out ushort bright)) return false;
                tint = (ushort)((tint & 0x03ff) | (bright & 0x7c00));
            }
            value = tint;
            return true;
        }
        // These tracks brighten an independently chosen dim tint, rather than
        // the normal suit color. The dim endpoint remains an editable input.
        if (group != 3 && (first == 0xdb6b ? slot is 1 or 2 or 10 or 11 or 12 :
            first == 0xdcd1 ? slot == 12 : slot == 2))
        {
            if (!TryReadColor((ushort)(first + 3 * 79 + 36 + 2 * slot), colors, out ushort dim)) return false;
            bool plateau = first == 0xde37 || (first == 0xdb6b && slot is 1 or 12);
            int greenPeak = plateau ? 15 : first == 0xdb6b && slot is 10 or 11 ? 5 : 0;
            value = BrightenDimColor(dim, shade, greenPeak, plateau);
            return true;
        }
        bool varia = first == 0xdcd1;
        if (!(first == 0xdb6b ? slot is >= 3 and <= 8 or >= 13 and <= 15 :
            varia ? slot is 1 or 2 || (group != 0 && slot is 10 or 11) : slot is 10 or 11)) return false;
        if (!TryReadColor((ushort)(first + 2 * slot), colors, out ushort original)) return false;
        value = varia ? VariaTintColor(original, shade) : TintColor(original, shade);
        return true;
    }

    /// <summary>Brightens an independently chosen dim loading tint.</summary>
    /// <remarks>Original Power slots1/12 and Gravity slot2 add green15/5 and
    /// blue10/10 for bright/middle. Power slots2/10/11 and Varia slot12 add
    /// blue10/5; only Power slots10/11 also add green5/0. Red is unchanged,
    /// sums saturate at31. These14 words derive from seven editable dim
    /// endpoints; this does not exempt those endpoints from further review.
    /// Shade0/1 is bright/middle; greenPeak is an RGB5 additive amount.</remarks>
    internal static ushort BrightenDimColor(ushort dim, int shade, int greenPeak, bool bluePlateau)
    {
        if ((uint)shade >= 2) throw new ArgumentOutOfRangeException(nameof(shade));
        if ((uint)greenPeak > 31) throw new ArgumentOutOfRangeException(nameof(greenPeak));
        int green = Math.Min(31, (dim >> 5 & 31) + Math.Max(0, greenPeak - 10 * shade));
        int blue = Math.Min(31, (dim >> 10 & 31) + (bluePlateau ? 10 : 10 - 5 * shade));
        return (ushort)((dim & 31) | green << 5 | blue << 10);
    }

    /// <summary>Applies Varia's blue-dominant loading tint to one RGB5 input.</summary>
    /// <remarks>Shade0..2 preserves red and adds green=max(0,5-5*shade),
    /// blue=30-10*shade, saturating at31. Original Varia slots1/2 follow all
    /// three levels; slots10/11 follow levels1/2. The brightest slots10/11
    /// are excluded because their blue is29 rather than30. All ten included
    /// words match the original program; no correction table is used.</remarks>
    internal static ushort VariaTintColor(ushort original, int shade)
    {
        if ((uint)shade >= 3) throw new ArgumentOutOfRangeException(nameof(shade));
        int green = Math.Min(31, (original >> 5 & 31) + Math.Max(0, 5 - 5 * shade));
        int blue = Math.Min(31, (original >> 10 & 31) + 30 - 10 * shade);
        return (ushort)((original & 31) | green << 5 | blue << 10);
    }

    /// <summary>Applies the three saturating loading tint levels to one RGB5 input.</summary>
    /// <remarks>Shade0..2 runs bright to dim. Green=max(0,15-10*shade),
    /// blue=min(20,30-10*shade); channel sums saturate at31, red is unchanged.</remarks>
    internal static ushort TintColor(ushort original, int shade)
    {
        if ((uint)shade >= 3) throw new ArgumentOutOfRangeException(nameof(shade));
        int green = Math.Min(31, (original >> 5 & 31) + Math.Max(0, 15 - 10 * shade));
        int blue = Math.Min(31, (original >> 10 & 31) + Math.Min(20, 30 - 10 * shade));
        return (ushort)((original & 31) | green << 5 | blue << 10);
    }

    /// <summary>Shares identical suit slots with the Power palette for the same shade.</summary>
    /// <remarks>Original complete RGB5 words establish each equality. Varia's
    /// normal row differs at0/2/10/11 and bright rows at1/2/10/11/12. Gravity's
    /// normal row differs at2/10/11/12 and bright rows at2/10/11. All other slots
    /// share the Power input, including transparent entries where equal.
    /// Differing custom colors remain explicit overrides before this mapping.</remarks>
    private static ushort ShareSuit(int first, int rowOffset, int colorOffset)
    {
        int slot = colorOffset / 2;
        bool distinct = first == 0xdb6b || (first == 0xdcd1
            ? (rowOffset == 0 ? slot is 0 or 2 or 10 or 11 : slot is 1 or 2 or 10 or 11 or 12)
            : (rowOffset == 0 ? slot is 2 or 10 or 11 or 12 : slot is 2 or 10 or 11));
        return (ushort)((distinct ? first : 0xdb6b) + rowOffset + colorOffset);
    }

    private static bool TryProgram(ushort pointer, int first, out ushort canonical)
    {
        canonical = 0;
        int offset = pointer - first;
        int finalColor = offset - 313;
        if ((uint)finalColor < 32 && (finalColor & 1) == 0)
        {
            canonical = ShareSuit(first, 0, finalColor);
            return true;
        }
        if ((uint)offset >= 4 * 79) return false;
        int group = offset / 79;
        int within = offset % 79;
        int record = within / 36;
        int color = within % 36;
        if (record >= 2 || color >= 32 || (color & 1) != 0) return false;
        canonical = ShareSuit(first, record == 0 ? 0 :
            (group < 2 ? 36 : group * 79 + 36), color);
        return true;
    }
}
