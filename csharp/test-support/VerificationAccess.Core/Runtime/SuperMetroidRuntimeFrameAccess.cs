using SuperMetroid.Core.Input;
namespace SuperMetroid.Core.Runtime;

/// <summary>Verifier-facing step seam for one ordinary gameplay frame.</summary>
internal static class SuperMetroidRuntimeFrameAccess
{
    /// <summary>Advances one ordinary gameplay frame with the default runtime request.</summary>
    extension(SuperMetroid.Core.Runtime.SuperMetroidRuntime self)
    {
        /// <summary>
        /// One ordinary state-eight frame: departure allowed, game time advancing, deferred audio (#633). The frame is an
        /// update of the check's own, so it begins the check's record of the runtime's audio requests (#2137).
        /// </summary>
        internal void StepFrame(ControllerButton playerInput)
        {
            self.AudioRequests.BeginRecordedUpdate();
            self.StepFrame(playerInput, new SuperMetroid.Core.Runtime.RuntimeFrameRequest());
        }
    }
}
