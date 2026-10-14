namespace SuperMetroid.Core.Input;

/// <summary>The supported versions of the deterministic input-recording header, in format order.</summary>
internal enum ControllerInputRecordingVersion : uint
{
    /// <summary>Original recording format containing ROM, options, SRAM, and inputs.</summary>
    Legacy = 1,

    /// <summary>Recording format which adds installed-content identity.</summary>
    Identified = 2,

    /// <summary>Recording format adding a bounded named-catalog table after the fixed header.</summary>
    Current = 3,
}
