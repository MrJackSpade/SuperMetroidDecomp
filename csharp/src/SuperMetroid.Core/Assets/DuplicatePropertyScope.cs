namespace SuperMetroid.Core.Assets;

/// <summary>Which objects of a JSON document the duplicate-property check examines.</summary>
internal enum DuplicatePropertyScope
{
    /// <summary>Every object, including those reached through arrays.</summary>
    EveryObject,

    /// <summary>Only objects not reached through an array.</summary>
    OutsideArrays,
}
