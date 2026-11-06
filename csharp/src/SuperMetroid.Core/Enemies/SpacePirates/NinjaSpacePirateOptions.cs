namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How a ninja Pirate's placement starts it. The values are arbitrary bits; the population entry names them.</summary>
[Flags]
public enum NinjaSpacePirateOptions
{
    /// <summary>It starts at its right post, facing left.</summary>
    None = 0,

    /// <summary>Starts at the left post and initially faces toward the right post.</summary>
    StartsAtLeftPostFacingRight = 1 << 0,
}
