namespace SuperMetroid.Core.Assets;

/// <summary>The three channels of a colour's levels (<see cref="FadeLevelColour"/>), in the order a level-by-level change steps them.</summary>
public enum ColorChannel
{
    /// <summary>Red, stepped first.</summary>
    Red,
    /// <summary>Green, stepped second.</summary>
    Green,
    /// <summary>Blue, stepped last.</summary>
    Blue,
}
