namespace SuperMetroid.Core.Assets;

/// <summary>The three channels of a colour's levels (<see cref="FadeLevelColour"/>), in the order a level-by-level change steps them.</summary>
public enum ColorChannel
{
    /// <summary>Red, stepped first.</summary>
    Red = 0,
    /// <summary>Green, stepped second.</summary>
    Green = 1,
    /// <summary>Blue, stepped last.</summary>
    Blue = 2,
}
