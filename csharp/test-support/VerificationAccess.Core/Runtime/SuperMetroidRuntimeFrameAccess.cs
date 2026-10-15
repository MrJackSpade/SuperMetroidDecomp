namespace SuperMetroid.Core.Runtime;

/// <summary>Verifier-facing step seam for one ordinary gameplay frame.</summary>
internal static class SuperMetroidRuntimeFrameAccess
{
    /// <summary>Advances one ordinary gameplay frame with the default runtime request.</summary>
    extension(SuperMetroid.Core.Runtime.SuperMetroidRuntime self)
    {
        /// <summary>One ordinary state-eight frame: departure allowed, game time advancing, deferred audio (#633).</summary>
        internal SuperMetroid.Core.Runtime.RuntimeFrameResult StepFrame(ushort controller1Input) =>
            self.StepFrame(controller1Input, new SuperMetroid.Core.Runtime.RuntimeFrameRequest());
    }
}
