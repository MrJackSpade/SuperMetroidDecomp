namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>What the owner of a <see cref="CrocomireDeathStage"/> does on each of its cases (#2553).</summary>
/// <param name="Fighting">Reacts to a <see cref="CrocomireDeathStage.Fighting"/>.</param>
/// <param name="SinkThroughBridge">Reacts to a <see cref="CrocomireDeathStage.SinkThroughBridge"/>.</param>
/// <param name="Sink">Reacts to a <see cref="CrocomireDeathStage.Sink"/>.</param>
/// <param name="SubmergedPause">Reacts to a <see cref="CrocomireDeathStage.SubmergedPause"/>.</param>
/// <param name="Rise">Reacts to a <see cref="CrocomireDeathStage.Rise"/>.</param>
/// <param name="ShowMeltImage">Reacts to a <see cref="CrocomireDeathStage.ShowMeltImage"/>.</param>
/// <param name="BeginMeltPass">Reacts to a <see cref="CrocomireDeathStage.BeginMeltPass"/>.</param>
/// <param name="ShowMeltBands">Reacts to a <see cref="CrocomireDeathStage.ShowMeltBands"/>.</param>
/// <param name="PauseOneUpdate">Reacts to a <see cref="CrocomireDeathStage.PauseOneUpdate"/>.</param>
/// <param name="StartDissolve">Reacts to a <see cref="CrocomireDeathStage.StartDissolve"/>.</param>
/// <param name="Dissolve">Reacts to a <see cref="CrocomireDeathStage.Dissolve"/>.</param>
/// <param name="ClearMeltImage">Reacts to a <see cref="CrocomireDeathStage.ClearMeltImage"/>.</param>
/// <param name="FinalSink">Reacts to a <see cref="CrocomireDeathStage.FinalSink"/>.</param>
/// <param name="FlowDownRiver">Reacts to a <see cref="CrocomireDeathStage.FlowDownRiver"/>.</param>
/// <param name="WaitForSamusAtWall">Reacts to a <see cref="CrocomireDeathStage.WaitForSamusAtWall"/>.</param>
/// <param name="RumbleHiddenWall">Reacts to a <see cref="CrocomireDeathStage.RumbleHiddenWall"/>.</param>
/// <param name="BreakSpikeWall">Reacts to a <see cref="CrocomireDeathStage.BreakSpikeWall"/>.</param>
/// <param name="DelaySkeletonFall">Reacts to a <see cref="CrocomireDeathStage.DelaySkeletonFall"/>.</param>
/// <param name="ArcSkeletonIntoArena">Reacts to a <see cref="CrocomireDeathStage.ArcSkeletonIntoArena"/>.</param>
/// <param name="CollapseSkeleton">Reacts to a <see cref="CrocomireDeathStage.CollapseSkeleton"/>.</param>
/// <param name="ClearWallAndOpenScrolls">Reacts to a <see cref="CrocomireDeathStage.ClearWallAndOpenScrolls"/>.</param>
/// <param name="PublishDefeatAndRestoreMusic">Reacts to a <see cref="CrocomireDeathStage.PublishDefeatAndRestoreMusic"/>.</param>
/// <param name="InertCorpse">Reacts to a <see cref="CrocomireDeathStage.InertCorpse"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record CrocomireDeathStageReactions<TContext>(Action<CrocomireDeathStage.Fighting, TContext> Fighting, Action<CrocomireDeathStage.SinkThroughBridge, TContext> SinkThroughBridge, Action<CrocomireDeathStage.Sink, TContext> Sink, Action<CrocomireDeathStage.SubmergedPause, TContext> SubmergedPause, Action<CrocomireDeathStage.Rise, TContext> Rise, Action<CrocomireDeathStage.ShowMeltImage, TContext> ShowMeltImage, Action<CrocomireDeathStage.BeginMeltPass, TContext> BeginMeltPass, Action<CrocomireDeathStage.ShowMeltBands, TContext> ShowMeltBands, Action<CrocomireDeathStage.PauseOneUpdate, TContext> PauseOneUpdate, Action<CrocomireDeathStage.StartDissolve, TContext> StartDissolve, Action<CrocomireDeathStage.Dissolve, TContext> Dissolve, Action<CrocomireDeathStage.ClearMeltImage, TContext> ClearMeltImage, Action<CrocomireDeathStage.FinalSink, TContext> FinalSink, Action<CrocomireDeathStage.FlowDownRiver, TContext> FlowDownRiver, Action<CrocomireDeathStage.WaitForSamusAtWall, TContext> WaitForSamusAtWall, Action<CrocomireDeathStage.RumbleHiddenWall, TContext> RumbleHiddenWall, Action<CrocomireDeathStage.BreakSpikeWall, TContext> BreakSpikeWall, Action<CrocomireDeathStage.DelaySkeletonFall, TContext> DelaySkeletonFall, Action<CrocomireDeathStage.ArcSkeletonIntoArena, TContext> ArcSkeletonIntoArena, Action<CrocomireDeathStage.CollapseSkeleton, TContext> CollapseSkeleton, Action<CrocomireDeathStage.ClearWallAndOpenScrolls, TContext> ClearWallAndOpenScrolls, Action<CrocomireDeathStage.PublishDefeatAndRestoreMusic, TContext> PublishDefeatAndRestoreMusic, Action<CrocomireDeathStage.InertCorpse, TContext> InertCorpse);
