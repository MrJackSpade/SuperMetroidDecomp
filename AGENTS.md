# Project Working Instructions

## Read the manifesto first

- [docs/manifesto.md](docs/manifesto.md) is the reference for every design decision in
  this repository: what the project is, where it is going, why the analyzers exist, why
  their findings are never excused, and what a correct change looks like. Read it before
  changing code, and include its section 8 rules in every subagent prompt. Where a shorter
  rule here or in another document conflicts with it, the manifesto wins.

## The Godot destination

- [The architectural plan](docs/godot-architecture.md) describes the shape the
  simulation is converging on: phase dispatch through entities into domain services,
  shared state as explicit relationships with real lifetimes, one owner of simulation
  advancement, a display snapshot boundary, semantic audio and input contracts. That
  shape is the design reference for every change in the C# product, not only for the
  migration itself: a change that moves code toward it is progress, a change that
  moves away from it is not, whatever ticket it serves.
- What is not started is the Godot host, scene authoring and engine integration. The
  plan does not authorize beginning that work, switching away from the active ticket,
  or treating its illustrative names as a completed design. For migration work,
  establish the applicable source constraints before choosing a pattern and record the
  execution, state-ownership and lifetime contracts for the affected slice.

## Discord version updates

- Use `pwsh -File tools/discord-updates.ps1 next` to obtain the single next
  unposted published commit. Follow `tools/discord-updates.md` for connection and
  recovery. The queue starts at the first commit and targets server
  `1547270817312809112`, channel `1547271407476408411`.
- After every successful push, drain all unposted published commits before
  handing control back to the user. The user has authorized this recurring
  posting workflow and repo text other than API keys; do not ask again.
- Run `next`, inspect only the exposed commit, write a concise plain-language
  summary of that version, preview it, and post with its full SHA. While historical
  commits remain, announce the oldest unposted commit, even when it differs from
  the commit just pushed. After each confirmed post, run `next` again and repeat
  this loop until it returns `caught-up`. Do not stop after one update. Catch-up
  requests resume the existing tracker; never restart from the first commit.
  If already caught up, no post is needed.
- Install the tracked push reminder with `core.hooksPath` set to the main checkout's absolute `.githooks` path
  as described in `tools/discord-updates.md`. Its `pre-push` output reminds the
  model to perform this loop AFTER the push succeeds; it does not post itself.
- Do not enumerate later commits, skip ahead, or use raw webhooks to bypass the
  tracker. Verify the delivery receipt and report any posting failure at handoff;
  do not treat a failed or uncertain send as a completed announcement.
- Keep webhook credentials in `DISCORD_WEBHOOK_URL` and tracker state in the Git
  common directory. Never commit either. If delivery is uncertain, reconcile the
  existing attempt before posting again.

## Gameplay parity and hardware lag

- The standing project goal is functionally exact cartridge gameplay with hardware
  lag eliminated. Preserve the cartridge's gameplay-update order, state changes,
  collision rules, RNG behavior, and actual input consumption. Smooth execution is
  an intended improvement, not a parity defect.
- Compare every gameplay update across a supplied recording, including entry,
  transitions, combat, and the recorded ending. Do not require reproducing SNES
  CPU stalls, repeated video refreshes, or elapsed wall-clock slowdown. Do not add
  artificial lag merely to match an SMV's video-frame numbers.
- Preserve behaviorally meaningful timers, deliberate waits, animation cadence,
  and input edges. Hardware lag elimination does not authorize speeding up or
  skipping authored gameplay behavior.
- Keep original SMVs immutable. A reusable converter may derive a separate port
  replay and source-frame-to-update map from a verified native input-consumption
  trace. Never infer removable frames merely from duplicate inputs, unchanged
  images, or unchanged actor positions. See [the replay contract](docs/smv-gameplay-parity.md).
- Full-movie parity requires a single initial-state import followed by the
  converted input timeline. Do not feed recorded Samus state, RNG, enemy state,
  or expected results back into the port during replay. Isolated subsystem
  comparisons remain useful diagnostics but do not establish full-movie parity.
- Report the original movie coverage, compared update count, input alignment,
  compared properties, and any remaining mismatch or unverified coverage.

## Diagnostic references

- [InsaneFirebat Super Metroid disassembly](https://github.com/InsaneFirebat/sm_disassembly)
- [Patrick Johnston's bank reference](https://patrickjohnston.org/bank/index.html)

Use these references during cartridge diagnostics to cross-check routines, symbols,
addresses, and behavior. Confirm findings against the project's ROM revision and
pinned local sources; reference annotations do not replace reproducing a reported bug.

## Process-boundary error handling

- No repository executable, verifier, diagnostic, replay tool, or temporary probe may
  allow an exception to escape to the CLR or Windows error reporter. This applies to
  one-off investigation programs under `test-temp` just as strictly as shipped hosts.
- Every console entry point must install the existing Windows no-dialog process policy
  before doing fallible work, wrap its outermost process boundary in `try`/`catch`, write
  `exception.ToString()` to standard error, and return a nonzero exit code. Follow
  `SuperMetroid.Verification` or `SuperMetroid.DebugRunner`; do not create unguarded
  top-level statements that can throw.
- GUI entry points must use the established `UnhandledExceptionConsole`/dispatcher
  boundary so failures remain visible in the console or diagnostic log without a modal
  dialog. Do not add `MessageBox`, Windows Error Reporting, or debugger-only UI as an
  error path.
- Failing loudly means a full exception and failing exit code or the configured GitHub
  recoverable-error report. It never means a focus-stealing dialog. Tests for expected
  failures must catch them inside the test harness rather than leaking them from the
  process.

## Analyzer findings are addressed, never grandfathered

- Analyzers exist to find bad code so it gets resolved. Every build error and every
  analyzer finding must be ADDRESSED: fix the code, or put a justification on that one
  site stating why it cannot be fixed or why it is a false positive that does not violate
  the spirit of the rule (for example `SuppressMessage` with a real `Justification`, a
  same-line `#pragma warning disable` reason, or the rule's own boundary attribute with
  its reason).
- NEVER grandfather existing code. Do not create or extend any mechanism that excuses
  findings in bulk: per-file allowed counts, baseline files, "may only shrink" lists,
  accepted-fingerprint files, allow-lists kept as hooks, tools that regenerate allowances,
  assembly- or namespace-wide suppressions, placeholder justifications such as
  `<Pending>`, or severity downgrades (`NoWarn`, `WarningsNotAsErrors`, lowering a rule
  below error) to keep a build green.
- A new or tightened rule ships at full severity. If it fires on existing code, those
  sites are fixed or individually justified as part of the same work; the rule does not
  ship with exemptions for them.
- The size of the work is never a reason to stop, ask, or excuse findings. A fix that
  touches a whole subsystem, every family or hundreds of sites is done, in committed
  batches, like any other fix.

## Recorded-movie acceptance scope

- For requested SMV/movie parity, replay the entire supplied recording through
  production gameplay without desynchronization, excluding evidenced hardware
  lag. Initialize once; never inject later native state to repair divergence.
- A passing end-to-end replay is the requested game-logic check. Do not turn it
  into an exhaustive internal-state audit, rendering/audio parity campaign, or
  unrelated whole-game validation. Missing additional coverage is not a blocker.
- Apply this scope to larger future recordings as well. Diagnose actual replay
  divergences and finish when the full replay passes; expand scope only when asked.
- See [the replay contract](docs/smv-gameplay-parity.md) for timing and conversion
  details. Passing one movie does not establish correctness of every game path.

## Testing ownership and scope

- The agent is the DEVELOPER, not the TESTER. The user is the project tester.
  The user owns exploratory playtesting, bug discovery, and final whole-game
  validation; the agent implements requested work
  and confirms specific changes. Do not take over the user's testing role.
- NEVER USE TEST CASES TO SEARCH FOR PROBLEMS. Automated tests, replays, frame
  probes, room sweeps, parameter searches, and expanded test matrices must not
  be used to hunt for unreported defects or choose the next implementation task.
- Test cases exist ONLY to confirm an identified change or reproduce/confirm a
  reported problem. Before adding or running a case, identify the requested
  change or reported defect and the exact property the case confirms. If it is
  being run to discover what else might be broken, do not run it.
- A focused reproduction of a reported failure, including the mandatory
  reproduction after a failed fix, is confirmation work, not exploratory testing.
  Keep it scoped to that report; do not expand it into a playthrough or new bug hunt.
- Whole-game validation is an END GOAL, not a checkpoint for every fix. Do not
  hold a completed change or usable build for exhaustive parity, every-room,
  every-effect, or whole-game acceptance. Perform proportionate confirmation of
  the change, commit/push it, and hand it back for the user's testing.
- Ticket goals and acceptance text must describe the specific implementation or
  reported defect. Never add whole-game validation as an individual ticket's goal
  or per-fix gate. Scope confirmation to the changed contract and reported property;
  do not turn unspecified "remaining acceptance" into an agent-owned test campaign.
- Source inspection and compiler/type-boundary work needed to implement the
  requested task remain appropriate. They do not authorize a test-driven search
  for additional work. Mark implemented player-facing fixes
  `awaiting-player-validation`; passing agent checks is not player confirmation.

## Regression tests

- Tests may use synthetic rooms, constructed cartridge data, fake address spaces, and other focused fixtures. A regression test does not need to drive a real retail room when a smaller fixture faithfully reproduces the reported failure.
- Prefer the smallest deterministic fixture that exercises the real production code path and makes the failure observable.
- Match the assertion to the report. Visual, positioning, animation, timing, and state-transition bugs need assertions for those exact properties; a nearby endpoint or no-crash assertion is not sufficient evidence of a fix.
- Use a real room or recorded controller sequence when the behavior depends on retail room data, interactions across systems, or a sequence that a synthetic fixture cannot reproduce faithfully.

## ROM-free migration

- Do not discover remaining cartridge reads by running gameplay, replays, unit tests,
  or progressively longer frame probes. A passing path cannot establish that other
  paths are ROM-free, and finding one read at a time is not a completion strategy.
- Inventory cartridge access statically from the source and its call graph. Separate
  extraction/startup imports from executable gameplay dependencies, and use API or
  type-boundary changes that make unresolved gameplay reads visible at build time.
- Tests may confirm a specifically identified conversion and its behavior, but must
  never search for remaining reads or unrelated defects. Establish ROM-capability
  removal through source/type ownership and compiler boundaries. A focused host
  check may confirm a particular integration change; whole-game validation remains
  the user's end goal, not a prerequisite for each conversion or build handoff.

## Issue report versions

- Every player bug report must record the affected game version in the GitHub
  issue body under `Affected version`. Include it when creating or reopening an
  issue, or when adding a new report to an existing issue.
- Use the version supplied by the player; carry it forward for subsequent reports
  in the same testing session until the player specifies a different version.
  Never infer the affected version from the current repository or latest release.
- The player explicitly uses the latest test build handed off by this agent unless
  they say otherwise. Carry that supplied build forward; do not repeatedly ask
  whether they used an older build. This does not mean the latest GitHub release.
- If the version is unknown, record `Unknown (awaiting player version)` and ask
  for it. Update the issue when supplied, preserving earlier reported versions.

## Milestones

- Every open ticket belongs to a GitHub milestone named for the release that ships it
  (`v0.6.0`, `v1.0.0`, ...). Place every new ticket in the appropriate milestone when
  creating it, including child tickets and source-inspection findings. A ticket left
  without a milestone is an error to correct.
- Minor versions (`0.x.0`) are planned release bands; read each milestone's description
  for its scope. Patch versions (`0.x.y`) are reserved for bugfix releases.
- `v1.0.0` is the full Godot port (umbrella #1393); `v1.1.0` adds widescreen support.
- Work is ordered by milestone: take tickets from the earliest open milestone, subject
  to "Single ticket development".

## Issue-fix workflow

- On the first attempt to fix an issue, exact reproduction is optional when the report is clear and the defect is small or mechanically obvious. Reproduce first whenever diagnosis is uncertain, the change is risky, or the reported property cannot otherwise be verified confidently.
- If the user reports that a first fix failed, exact reproduction becomes mandatory before making another fix attempt. Do not apply a second speculative fix. Capture the failing behavior with a real-room sequence or a faithful synthetic fixture, make the relevant assertion fail, and only then change production code.
- Treat each repeated report as evidence that the earlier test or diagnosis was insufficient. Expand the reproduction to cover the user's exact trigger, trajectory, timing, or visual property rather than rerunning a nearby test.
- Do not make the user report the same defect a third time because the second attempt was not tested against the actual failure.
- Do not close a reported issue solely because a synthetic test passes. After reproducing as required, add an appropriate regression test, verify the fix against that reproduction, and leave the issue open for player confirmation unless explicitly instructed otherwise.
- As soon as an implemented fix is ready for player confirmation, add the
  `awaiting-player-validation` label to its GitHub issue. Remove that label if
  player validation fails, and close the issue when the player confirms the fix.
  An open issue without this label must not be assumed to be awaiting validation.
- A ticket with an unanswered question for the owner is blocked on the owner, not
  awaiting validation. Never label it `awaiting-player-validation`; label it
  `question` so the owner can see it needs input, and implement neither option
  until the owner answers. Remove `question` when the answer is applied; only then
  can the ticket move to `awaiting-player-validation`. Before labeling any ticket,
  check its comments for unanswered owner questions.

## Single ticket development

- Work on only ONE ticket at a time. The GitHub label `in-progress` is the
  repository's active development status and must be assigned before beginning
  a ticket's diagnosis, implementation, or confirmation work.
- Before starting a ticket, query the repository for open tickets marked
  `in-progress`. If any exists, continue that ticket; do not start another.
  If multiple active tickets already exist, report the conflict rather than
  selecting another ticket or silently clearing their status.
- While an `in-progress` ticket exists, taking work on any other ticket is
  FORBIDDEN. Do not investigate, implement, or confirm another ticket, including
  a related child or follow-up, until the active ticket is development complete.
  Difficulty, waiting, a blocker, or convenient adjacent work does not permit
  switching tickets or clearing the active status.
- Development complete means the requested implementation is finished, its
  specific change has passed proportionate confirmation, and the completed work
  is committed and pushed. Do not invent whole-game coverage requirements or
  use exploratory tests to delay this status.
- Remove `in-progress` only when development is complete. For a player-facing
  change awaiting the user's confirmation, replace it with
  `awaiting-player-validation`; pending player validation does not prevent
  beginning the next ticket. Closure still follows the existing player-confirmation
  rule or the user's explicit instruction.
- Tracking umbrellas and their implementation children are not simultaneous
  active tickets. Finish the current ticket before taking a different ticket,
  and keep the status accurate across turns and handoffs.

## Emulation scope limits

- Techniques and reported behavior that require unbounded memory corruption are
  outside the project's implementation scope. This exemption applies specifically
  when the behavior cannot be reproduced with bounded modeled state or a faithful
  deterministic fixture and would instead require full cartridge-compatible memory,
  CPU-state, and native-side-effect emulation.
- Do not approximate, hard-code, or otherwise implement the observed outcome of an
  exempt technique. Once investigation establishes this boundary, stop implementation,
  document the architectural reason on the GitHub issue, and close it as `not planned`.
- Do not treat memory corruption by itself as sufficient grounds for exemption. If its
  relevant effects can be bounded and reproduced faithfully without full memory
  emulation, follow the normal issue-fix and regression-test workflow. Keep any
  separable bounded behavior in scope, using a separate issue when appropriate.

## Batch handoff summaries

- At the end of every batch containing multiple fixes, provide a self-contained summary before returning control to the user. The user must not need to reconstruct the results from intermediate progress messages or earlier conversation history.
- Include one entry for every fix in the batch. For each entry, state the reported problem, the diagnosed root cause, the implemented solution, and how the result was verified.
- Clearly distinguish fixes that were reproduced and verified from first-pass fixes that did not require reproduction, and identify anything still awaiting player confirmation.
- Also summarize relevant tests, commits, pushes, and remaining open or deferred work once for the batch.

## Commit and push discipline

- Commit and push every completed change as soon as it has passed its proportionate
  verification. Do not leave recoverable work only in the local working tree between
  handoffs.
- Keep commits logically sized and scoped. A focused fix and its regression test belong
  together. Prefer one commit per reported ticket, containing that ticket's production fix
  and regression test. Use a shared commit only when multiple tickets have the same root
  cause or cannot be separated without leaving a broken intermediate state, and identify
  every covered ticket in the commit message or body.
- Never rewrite or discard the user's history to accomplish this. A committed regression
  can be reverted, while uncommitted lost work cannot be recovered.
- Proportionate verification for a commit is one clean build plus the focused checks that
  cover the code that changed. Nothing else gates a commit. See "Agent turnaround time".

## Agent turnaround time

Waiting on builds, checks and scripts has been the largest cost on this project: whole days
spent watching the agent wait on tools it chose to run. Ticket turnaround is a primary
constraint, not a side concern.

### The commit path stays cheap

- A commit needs one clean build and the focused checks for what changed. Do not add any
  other step to the commit path unless the user asks for it.
- `tools/verify-all.ps1` is not a commit gate, a batch gate or a per-ticket gate. Run it
  only when the user asks. The script refuses a second start within 120 minutes of the
  previous one; a refused start is the rule working, not an obstacle to route around.
- Do not invent pre-commit or per-batch checks of your own (for example a private "slice"
  or audit script run before every commit). Writing a new script is fine. Making it a
  recurring step is the user's decision.

### Slow commands must not become routine

- Any command that takes minutes to finish is slow. A slow command may run once when there
  is a specific reason. Running it again within the same ticket needs the user's approval.
  This applies to scripts the agent writes as much as to existing tools.
- Never chain slow steps into a fixed sequence that runs for every commit or batch. Each
  slow step multiplied by every commit is how ticket turnaround grew by hours.
- At handoff, report what was run and how long it took, so growing turnaround is visible
  before it costs hours.
- The `post-commit` hook runs `tools/agent-time-report.py --head`, which measures how much of
  the agent's time since the previous commit was spent waiting on tools. Above 20% it warns;
  above 50% it demands the waste be found and fixed. Treat either warning as an instruction:
  before continuing, name the commands that caused the waiting and stop the pattern. Run
  `python tools/agent-time-report.py --hours N` for the per-commit history.

### Build once, fix everything once

- Make all the edits for a body of work, then build once and fix every reported error in
  a single pass. Never build after each unit or rebuild after each individual fix.
- Build only the projects the change needs. The full solution builds in CI.

### Run only the checks that cover the change

- A focused check is a command that covers only the changed code. A command that runs
  every family's assertions (for example the whole enemy-artwork suite) is not focused for
  a change to one family, and neither is an unrelated suite.
- Put each family's assertions in that family's own verification class and command. Never
  add a family's assertions to a shared suite. If the only command covering a change is a
  suite-wide one, split the suite first and run only the split piece.
- Run several focused commands in one process where the runner allows it, rather than one
  process per command.
- Never re-run a command to read its output. Test runners must print the full failure
  message the first time; fix a runner that truncates output instead of re-running.
- Never guess command names. Look up the command that covers a test once, rather than
  launching guesses that only print usage.

### Never idle while something runs

- Background work is for working in parallel, not for waiting. While a command runs,
  continue with the next unit of work. Do not end a turn, schedule a wake-up or poll just
  to wait for a command to finish.
- If nothing can proceed without the result, say what is blocked instead of waiting
  silently.

### Edit tooling must not fail halfway

- Scripted multi-file edits validate every edit before writing any of them, and write
  nothing if any edit does not apply. A script that stops partway leaves the tree
  half-edited and costs a reconstruction plus another build.
- Do not embed scripts in shell heredocs with nested quoting. Write the script to a file
  and run it.
- Do not chain dependent edit steps with `&&` or pipes whose exit status can hide a
  failure (`| tail`, `grep -v`). A step that silently skips or silently continues leaves
  partial state.
- Make content checks in edit scripts specific. A check that matches legitimate text
  (a class's own name, a longer identifier containing the searched one) aborts the run.
- When a shared helper's assumption fails for one case, fix the helper once instead of
  working around it per case.
- Never regenerate generated files over hand edits. Put the fix in the generator or its
  configuration so regeneration reproduces it.

### Tests must not guess runtime behavior

- Do not hand-write assertions about engine behavior that has not been checked (cursor
  positions after a helper runs, the update a cue fires on, which program holds a pose).
  Each wrong guess costs a build, a run and a diagnosis.
- Prefer generated or shared standard checks whose behavior is already established, and
  hand-write only checks for logic unique to the change.
- Replace an obsolete test file wholesale rather than rewriting it line by line through
  replacement scripts.

### Reports

- Per-batch progress reports are brief. The full self-contained summary required by
  "Batch handoff summaries" is for handoff.

## Identities, definition data and cartridge addresses

- Identities are semantic names: enum members, types, behaviour-object instances. Their
  backing values are arbitrary and nothing reads them. An enum or constant whose value is a
  ROM address, a code pointer or an instruction-list pointer, and whose value is read,
  compared, indexed or switched on, is cartridge coupling and is removed, not extended.
- Definition data (durations, speeds, shapes, damage, frame lists, names) lives in
  domain-named catalog types with the meaning on each member's XML summary. Catalog files
  hold data only: no `if`, `switch`, loops or decisions. Call sites read as named domain
  operations.
- The ROM is known only to the import and extraction pipeline. The mapping from an address
  to a semantic identity is that pipeline's private knowledge, decoded once. An address
  catalog inside the simulation is scaffolding on the way out: it is never the destination,
  and no new address is added to the simulation because a catalog exists for it.
- Use an enum only when values are a proven mutually exclusive domain. Use `[Flags]` only
  when the cartridge demonstrably treats the values as composable bits; do not infer flag
  semantics from suggestive values alone. The flags stay typed; one codec at the import or
  export boundary converts them to a word.
- Comments in functional code explain behaviour, ordering and non-obvious side effects,
  never what a numeric value denotes. If a comment is needed to say what a number means,
  the number should be a name.
