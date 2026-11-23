namespace SuperMetroid.Core.Input;

/// <summary>Verification access to <see cref="ControllerBindings"/>: a check's bindings with one action rebound.</summary>
internal static class ControllerBindingsAccess
{
    /// <summary>Rebinds one action of a check's bindings.</summary>
    extension(ControllerBindings self)
    {
        /// <summary>These bindings with <paramref name="action"/> bound to <paramref name="value"/> and the other six kept.</summary>
        internal ControllerBindings With(ControllerAction action, ControllerButton value) =>
            new(Enum.GetValues<ControllerAction>().ToDictionary(bound => bound, bound => bound == action ? value : self[bound]));
    }
}
