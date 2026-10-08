using SuperMetroid.Core.Rendering;

internal static partial class CoreAccess
{
    extension(LatestRenderFrameMailbox mailbox)
    {
        /// <summary>
        /// The mailbox's current load/reset generation: publications must carry it, and a
        /// consumer-held packet is current only while its identity matches it.
        /// </summary>
        internal long Generation => PrivateState.Field<long>(mailbox, "generation");
    }
}
