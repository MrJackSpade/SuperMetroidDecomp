namespace SuperMetroid.Core.Assets;

/// <summary>Exact stored relationships between the native cinematic and hurt-flash palettes; selected parameters remain REQUIRED.</summary>
internal static class SamusHurtColorDefinitions
{
    /// <summary>$9B:A3A2..A3BE: the intro blue channel is two below red above the darkest shade. This chosen tint remains REQUIRED.</summary>
    internal const int RequiredBlueOffset = 2;
    /// <summary>$9B:A3A6 and repeated shade: the darkest blue is four. This chosen floor remains REQUIRED.</summary>
    internal const int RequiredBlueFloor = 4;
    /// <summary>$9B:A382..A39E versus $9B:A3A2..A3BE: hurt blends five parts white to two parts source. The selected blend weight remains REQUIRED.</summary>
    internal const int RequiredWhiteWeight = 5;
    /// <summary>$9B:A382..A39E: the complementary source weight in the exact seven-part blend; REQUIRED with the white weight.</summary>
    internal const int RequiredSourceWeight = 2;

    /// <summary>Equal red/green channels and the bounded blue tint; each selected red level and slot role remain REQUIRED.</summary>
    internal static ushort IntroFromLevel(byte red)
    {
        int blue = Math.Max(RequiredBlueFloor, red - RequiredBlueOffset);
        return (ushort)(red | red << 5 | blue << 10);
    }

    /// <summary>Floor-scaled RGB5 blend toward maximum white; applies only to indices 1..15, excluding the independently supplied zero slot.</summary>
    internal static ushort HurtFromIntro(ushort intro)
    {
        int Blend(int channel) => (RequiredSourceWeight * channel + RequiredWhiteWeight * 31) / (RequiredSourceWeight + RequiredWhiteWeight);
        return (ushort)(Blend(intro & 31) | Blend((intro >> 5) & 31) << 5 | Blend((intro >> 10) & 31) << 10);
    }
}
