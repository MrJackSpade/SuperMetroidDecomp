# Project Working Instructions

## Read the v1.0.0 manifesto first

- Read [the v1.0.0 manifesto](docs/v1.0.0-manifesto.md) first, then
  [the Godot architecture guide](docs/godot-architecture.md) and [the type system](docs/godot-types.md).

## The Godot destination

- v1.0.0 is the game written new in Godot. [The v1.0.0 manifesto](docs/v1.0.0-manifesto.md)
  states the goal and rules and wins over every other document for that work;
  [the Godot architecture guide](docs/godot-architecture.md) says how the project is built, and
  [the type system](docs/godot-types.md) lists every kind of type it is made of and how to decide
  where a thing belongs. Godot work extends that system.
- The C# port is legacy reference material: it documents what the game does. It is not a
  blueprint. The Godot project lives in its own directory with no references to the old
  code, reuses none of it, and does not copy its structure. The old code is not deleted.
- Both games are kept, on one branch, and a release carries a build of each: the old one named
  **Classic**, the Godot one named **Rebirth**. The owner's words, 2026-11-25: "The pipeline
  needs to now emit both the old (named classic) EXE, as well as the new (named Rebirth) exe,
  as well as a Rebirth APK. The APK should be properly signed and all that jazz, and the key
  should be stored up in the github actions", "since we're maintaining both the classic and the
  rebirth code bases"; and of the ticket that would have deleted the old code (#2910, closed):
  "I was going to keep them on different branches but two artifacts on the same branch is a lot
  easier."
- The Rebirth APK makes its assets on the device. The owner's words, 2026-11-25: "Youll also
  need to make sure the new APK handles the asset extraction properly on android". The Android
  build alone carries the asset extractor and calls it once, through `Game.RomAssets`; see the
  manifesto's section 1 for what that means for "no references to the old code". No other
  build carries it, and nothing else of the Godot project touches it.
- 0.9.x is still in testing: new 0.9.x bugs may arrive, in new milestones of their own, and
  they are fixed in `csharp/`. Apart from those the old code is frozen as a reference.
- The asset extractor reads the old code's definitions. The plan to make it self-contained
  by deleting the old code (#2910) was closed by the owner on 2026-11-25, with the words
  above. Whether a value is read from the old definitions or typed does not matter, so long
  as it is right: a typed value is not a difference from the cartridge and gets no parity
  ticket, and a small change to the old code made so that the extractor can read a value is
  not a question of the rules.
- For Godot work, parity means the same observable results from the same inputs, not the
  same internal order or state (v1.0.0 manifesto, section 4). The recorded movies are off
  limits until the owner opens testing.
- The engine holds nothing that nothing uses. The owner's words: "At this stage the engine
  shouldnt have unused resources in it. If generalize carrier doesnt work, then remove it.
  Leaving in unused entities during the architectural foundation just pollutes the design
  pattern". When a change leaves an engine kind, state or setting with no user, remove it in
  that change, with its checks, its writer in the extractor and its place in the docs; do
  not keep it as an offering to some other game. This covers an engine default behind a
  virtual that the game always overrides: if the game never reaches the default, it is dead
  and goes. The owner's words: "The engine at this point should contain no dead
  functionality at all. In the future it can, but right now its design isn't clear enough
  to justify vestigial data".
- The manifesto does not authorize starting Godot work.

## Full parity with the cartridge is required

- The owner's rule, in the owner's words: "Nothing can differ from cart behavior." The first
  test of the Godot game will be a recorded SMV movie played through it, which needs the game
  to be accurate frame by frame. That covers every update from power-on, in play and out of
  it: the title, the menus, the cinematics, door transitions, message boxes and pauses as
  much as Samus, enemies and rooms; positions, timings, the order things happen in, what
  fires and what is hit, random draws, energy and counts.
- The order of importance, in the owner's words, is "the most important way to determine what
  should or shouldn't be open/modified/reverted":
  1. "Parity of GAMEPLAY (old recordings play end to end, with no desync)"
  2. "Cleanliness of code, and idiomatic design"
  3. "Parity of presentation."

  A higher one wins over a lower one. In doubt between the first two, the second is the
  default. The owner's words: "when in doubt favor 2 over 1. 1 is more important, but failures
  of 1 can be identified by tests. So clean code and idiomatic design should be the default
  unless you have a reason to believe gameplay parity will break, because once tests are
  implemented all gameplay gaps will be identified, but cleanliness and idiomatic design can
  NOT be identified with tests."
- Presentation matters, and a presentation difference is a defect like any other. The order
  above says only which wins when two of them conflict. The owner's words: "code cleanliness
  and idiomatic design was more important than presentation. That means that when presentation
  does something weird, and that weird thing is resolved by making the code quality worse, you
  don't do that. Not that presentation doesn't matter at all." And: "you are not allowed to
  close presentation issues unless they lead to weird, unclean, or non-idiomatic design
  practices. You can't just ... ignore rendering bugs".
  - Something drawn, coloured, timed or heard differently from the cartridge gets a ticket and
    is worked, as a difference of play is.
  - A presentation ticket is closed unworked only when matching the cartridge there would need
    a weird, unclean or non-idiomatic design, and the ticket says which design that would be
    and why there is no clean way. "It changes nothing of play" is never a reason to close one.
  - No presentation ticket is closed, as done or as not to be done, without a comment that says
    whether the resolution broke the project's idiomatic design. The owner's words: "Every
    single ticket about a presentation level disparity needs to be closed with a comment about
    whether or not that resolution broke the idiomatic design of the project. You will not
    close any presentation parity issues as done, or wont do, without this justification."
    Done: the comment says how the look was got and that the design is whole, or what it cost.
    Not done: the comment names the design matching it would have needed and why that is
    unclean. The same holds when a finished one is labelled for validation.
  - An old mechanism is not copied to get a look: "You can't have idiomatic design if you're
    trying to port archaic design patterns." The look is matched in the Godot design, or the
    ticket is closed for the reason above.
- The old game's quirks of design are not carried forward for their own sake. The owner's
  words: "parity with the old code means parity of gameplay, not that all quirks of the game
  design need to be carried forward". That is about how the old game is built inside, not
  about what is seen or heard.
- A known gameplay difference is a defect, never an accepted end state. "Close enough",
  "simplified", "the Godot way" and "still different, not fixed" are not outcomes of a piece
  of work. A gameplay difference is fixed, or it stays open as a ticket.
- Every gameplay difference found gets its own ticket in the `v1.0.0` milestone, whether it was found
  by reading the old code, while building something else, or left behind by the work in
  hand. v1.0.0 is not complete while one is open.
- When migrating anything, read the old code for it line by line and carry over exactly what
  it does, in the Godot design. Numbers that agree are not enough: the order of an update,
  the rules that use the numbers, and the oddities of the cartridge's arithmetic are part of
  the behaviour.
- This does not open the recorded movies. They stay off limits until the owner opens testing
  (v1.0.0 manifesto, section 4); until then parity is reached by reading the old code.

## Which model takes a ticket

- The owner's standing rule: anything that requires architecture, design or new building, or
  that affects many systems, is for the newest Fable only. Anything that is isolated and has
  a clear fix may be taken by the newest Opus.
- The concern is not whether a fix turns out right without testing. It is changes that could
  poison the architecture: a quirk put in the engine, an engine member, format or order added
  to make one thing work, a structure copied from the old code, a second way of doing
  something the type system already does one way.
- Every ticket carries one of two labels, `high` or `low`, and is given it when it is created.
  An open ticket found with neither is given one before anything else is done with it.
  - `low`: the whole fix lies inside one thing of the game (its own files, its writer in the
    extractor, its place in the room generator), what the old game does there is stated or is
    found by a short read, and nothing new is asked of the engine.
  - `high`: everything else. That includes any ticket that needs an engine change however
    small, a decision about where something belongs, something new built, a reading of
    unknown size, or that waits on a `high` ticket. When in doubt, `high`.
- A model that is not the newest Fable takes only `low` tickets. If a `low` ticket turns out
  to need anything of the `high` kind, it stops there: it does not make that change, it says
  on the ticket what it found, and the ticket is relabelled `high`.
- The list of what the newest Opus may take is the open tickets labelled `low`.

## Rules come from the owner

- Do not change a rule, weaken or reinterpret one, or add a rule of a new kind (a check, a
  ban, a process), in code, tickets or documents. Those are the owner's.
- Building out what the Godot documents define is the agent's job and needs no asking: each
  section says why it is the way it is and how to extend it, and an extension is written
  into the document in the same change (v1.0.0 manifesto, section 6).

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

## Diagnostic references

- [InsaneFirebat Super Metroid disassembly](https://github.com/InsaneFirebat/sm_disassembly)
- [Patrick Johnston's bank reference](https://patrickjohnston.org/bank/index.html)

Use these references to cross-check what the game does. Confirm findings against the
project's ROM revision and pinned local sources; reference annotations do not replace
reproducing a reported bug.

## Process-boundary error handling

- No repository executable, verifier, diagnostic, replay tool, or temporary probe may
  allow an exception to escape to the CLR or Windows error reporter. This applies to
  one-off investigation programs under `test-temp` just as strictly as shipped hosts.
- Every console entry point must install a Windows no-dialog process policy
  before doing fallible work, wrap its outermost process boundary in `try`/`catch`, write
  `exception.ToString()` to standard error, and return a nonzero exit code. Do not create
  unguarded top-level statements that can throw.
- GUI entry points must keep failures visible in the console or diagnostic log without a
  modal dialog. Do not add `MessageBox`, Windows Error Reporting, or debugger-only UI as an
  error path.
- Failing loudly means a full exception and failing exit code or the configured GitHub
  recoverable-error report. It never means a focus-stealing dialog. Tests for expected
  failures must catch them inside the test harness rather than leaking them from the
  process.

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
- Running the Godot game headless to reproduce a bug the owner has specifically reported
  is allowed, as a standing rule. The owner's words: "you may create a standing rule that
  running a headless instance for a bug I've specifically reported is allowed". It covers
  the reported bug and confirming its fix, not looking for others; a one-off probe written
  for it is not committed.
- Source inspection and compiler/type-boundary work needed to implement the
  requested task remain appropriate. They do not authorize a test-driven search
  for additional work. Mark implemented player-facing fixes
  `awaiting-player-validation`; passing agent checks is not player confirmation.

## Regression tests

- Tests may use synthetic rooms and other focused fixtures. A regression test does not need to drive a real retail room when a smaller fixture faithfully reproduces the reported failure.
- Prefer the smallest deterministic fixture that exercises the real production code path and makes the failure observable.
- Match the assertion to the report. Visual, positioning, animation, timing, and state-transition bugs need assertions for those exact properties; a nearby endpoint or no-crash assertion is not sufficient evidence of a fix.
- Use a real room or recorded controller sequence when the behavior depends on retail room data, interactions across systems, or a sequence that a synthetic fixture cannot reproduce faithfully.

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
- `v1.0.0` is the game written new in Godot (umbrella #1393). `v1.1.0` collects low-hanging
  enhancements on top of the port, widescreen support (#1394) among them; more complex
  enhancements go to later `1.x` versions.

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

## Agent use

- An agent is never a model above the newest Opus.
- Whether work goes to an agent depends on which model is coordinating.
  - **The newest Fable coordinating.** Even counting an agent's start-up cost and the
    coordination, the newest Opus doing a worthwhile piece of work as an agent costs less
    than the coordinator doing it. That is not a reason to fire off an agent for every edit:
    use discretion. An agent is given a real piece of work, never a single small change.
    - Open `low` tickets that block nothing are batched, about five tickets to an agent, and
      a batch goes to an Opus agent.
    - One ticket by itself goes to an agent only when it is a longer, more mechanical task:
      the same change made across many files, say. A lone small `low` ticket is not worth an
      agent; it waits for a batch, or the coordinator makes the change itself.
    - Simple, low-effort boilerplate that has to be done to unblock `high` work may go to an
      Opus agent, when it is more than a few files' worth of work and needs no decisions.
    - Bulk investigation goes to an Opus agent. The owner's words: "for bulk investigation work, [an] Opus
      agent should be used when the Coordinator is Fable, as investigations as to the root cause of issues is
      low risk work that doesn't require any actual design work. The Fable coordinator would still decide how
      to fix the issue."
    - An edit the coordinator comes to in the course of its own work, it makes itself.
      "Low" describes who may safely make a change, not that every such change needs an agent.
    The coordinator keeps the `high` work for itself. Told to carry on with no ticket named,
    it first labels any open ticket that has no label, hands out the open `low` tickets that
    are not awaiting validation in batches as above, and works `high` tickets itself.
  - **The newest Opus coordinating.** An agent costs what the coordinator would cost doing the
    work itself, and its start-up cost and the coordination come on top, so for small tasks
    agents cost more. Spawn them only for work that splits intelligently into independent,
    long-running pieces; never fire off short-lived agents repeatedly.
- An agent never makes an architectural decision ("Which model takes a ticket"). Its prompt
  says so, and says what that means for the work it is given: nothing under `godot/engine/`
  changes; no engine member, format or order of update is added; nothing is decided about
  where something belongs. A ticket that turns out to need any of that is stopped, its
  changes undone, and what it needs reported.
- At most three agents may run at once.
- Every agent works in its own git worktree on its own branch and commits its work there, one
  commit a ticket, with `Ticket: #<number>` in the message's body. Agents never spawn further
  agents or forks, never push, and never write to GitHub.
- Agents do not build and do not run verification, tests, replays, generators or scripts that
  check behaviour. They only edit code and commit to their branches.
- An agent's report says, for each ticket: done, stopped or not done; its commit; what now
  agrees with the old code; exactly what is left; what must be regenerated; and any other
  difference from the old game it noticed, which it does not fix.
- While agents are running, the main session acts only as coordinator, not as a developer: it
  assigns and divides work, merges agent branches (`--no-ff`), regenerates what their changes
  need (`engine_defaults.py`, `generate_rooms.py`, `asset_manifest.py`), builds, runs the
  focused verification and the text checks in `tools/godot-rooms/`, fixes integration breakage,
  commits, pushes and drains the Discord queue. It then brings the tickets up to date: on each
  one, what was done and what is left; `awaiting-player-validation` on one that is finished;
  `high` in place of `low` on one that was stopped; and a new ticket, labelled, for each
  difference an agent found.
- That holds for agents that change code. While agents that only investigate are running (they change no file
  and commit nothing), the main session goes on developing on main. The owner's answer, 2026-11-25, to the
  question "May I keep fixing code on main ... while they read?", asked while three read-only investigation
  agents ran: "Yes, develop meanwhile".
- Every agent prompt includes the v1.0.0 manifesto's section 6 rules and this section.
- Before branching agents off for a ticket, merge that ticket's pre-existing WIP branches into main first, so
  agents start from all existing work.
- No branch pile-up: when a ticket's work is complete, every branch it used (pre-existing WIP and agent branches)
  is merged into main and deleted locally and on origin, and agent worktrees are removed. A branch whose work is
  already in main is deleted at once. WIP branches for other tickets wait until their ticket is taken.

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

### Run only the checks that cover the change

- A focused check is a command that covers only the changed code. A command that covers
  unrelated code is not focused, and neither is an unrelated suite.
- Run several focused commands in one process where the runner allows it, rather than one
  process per command.
- Run the change's suite set once and record its failures. After each fix, rerun ONLY the
  tests that failed, dropping each from the list as it passes. Run the whole set again
  only once, when every failure is believed resolved, as the final confirmation before the
  push. Never rerun the whole set after each change.
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
