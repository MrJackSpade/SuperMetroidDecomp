# The asynchronous cleanup

Code cleanliness that a tool can fix must not cost the developer's time (owner decision, 2026-11-11). Anything a tool
fixes deterministically never blocks a build, a commit or a push: the cleanup fixes it after the push. The rules whose
every finding an existing code fix resolves, with no choice and no change of behaviour, sit at `none` in
`csharp/.editorconfig` under the comment that names this cleanup, so the build skips their analyzers and spends no
time on them. Dead code, stale inventories and stale architecture reviews never block a push either: CI runs no
dead-code check, and its architecture audit runs `--findings`, which fails only on what needs judgment. After every
push this cleanup takes the newest tip of `main`, applies those fixes, prunes dead code and stale reviews, regenerates
the inventories, confirms the result builds, and commits and pushes what changed. It runs on this machine, in its own
clone, at Idle priority, and the developer never waits for it.

Every other rule stays a full-severity build error, and every finding that needs judgment still blocks. Only the
owner moves a rule into the automatically fixed group (`docs/manifesto.md` sections 5, 6 and 8).

## What runs, and when

| Piece | Where | What it does |
| --- | --- | --- |
| `.githooks/reference-transaction` | the main checkout's hooks path | Git runs it for every ref update. When a push or fetch moves `refs/remotes/origin/<branch>` it runs the installed `trigger.ps1` and returns. It never fails a transaction or waits for the cleanup. |
| `trigger.ps1` | installed in `<dir>\bin` | Appends the request to `requests.log` and, if no service holds `service.lock`, starts `service.ps1` detached through WMI: Idle priority, a headless console, outside the caller's process tree. |
| `service.ps1` | installed in `<dir>\bin` | One at a time. Fetches, resets the clone hard to the remote tip, checks the installed scripts still match the tree, runs the tree's `tools/cleanup/run.ps1`, records the outcome, and repeats while requests are pending. |
| `run.ps1` | the clone's tree | One run: restore, the reachability audit's Release build, the fix and prune steps (code, dead code, stale reviews), the inventories, one build, commit, push. |
| `CodeFixRunner.cs` | the clone's tree | The code fixes, as a Roslyn workspace runner (below). |

Git has no `post-push` hook; a successful push updates the remote-tracking ref through a ref transaction, so
`reference-transaction` is where a push is seen after it has landed. A fetch that brings new commits triggers it too,
which also picks up pushes made from elsewhere. The hook costs a shell start per ref transaction and nothing more
until that ref moves.

`<dir>` is `G:\git\SuperMetroidDecomp-cleanup`. It holds `repo\` (a full clone of `origin`, never a worktree of the
developer's checkout, with its own `bin`/`obj` and its own `temp\`), `config.json`, `STATUS`, `state.json`,
`requests.log`, `service.log`, `trigger.log` and `logs\`.

### The run

1. **Restore** the solution, **build the reachability audit in Release** and **the repository's analyzer and code fixes`n   in Debug** (`csharp/src/SuperMetroid.EnsureAnalyzer.CodeFixes`) through `tools/build.ps1`. The code fixes load the
   repository's analyzer and fixers from the projects' Debug references, so they must be built first.
2. **Fix and prune, taking turns, until one of each in a row changes nothing:**
   - **Code fixes:** `dotnet run tools/cleanup/CodeFixRunner.cs -- csharp/SuperMetroid.Full.slnx <summary.json> CS0168,IDE0004,IDE0005,IDE0032,IDE0044,IDE0076,IDE0077,SME6391`.
     `run.ps1` reads the rules from `csharp/.editorconfig`: the `none` lines directly under the comment that names
     this cleanup, less IDE0079 (below). A rule there that `run.ps1` has no commit-message words for, or words for a
     rule not there, fails the run.
   - **Dead code:** `dotnet run --project csharp/src/SuperMetroid.ReachabilityAudit -c Release --no-build -- --solution csharp/SuperMetroid.Full.slnx --prune-keeping-verification-used unreachable-member,unreachable-type,unreferenced-field,unreferenced-enum-member,unreferenced-constant --path-prefix csharp/src --to csharp/test-support/VerificationAccess.Core --retired-fields <run>\retired-fields-<step>.txt`.
     It deletes dead code tests do not use and moves what they still use into test-support; the next code-fix step
     removes the usings it copies into moved files.
   - **Stale reviews:** `dotnet run --project csharp/src/SuperMetroid.ArchitectureAudit -c Release -- --solution csharp/SuperMetroid.Full.slnx --prune-stale-reviews`.
     It deletes every `[ArchitectureReview]` whose finding its type no longer has (a prune of dead code can remove a
     finding), through the attribute's own syntax reference, and keeps every other byte of the file. It validates
     every edit before writing any and writes nothing if one fails. The next code-fix step removes a using the
     attribute alone needed.
   - A step that returns the tree to an earlier state fails the run (the steps undo each other); so does a run that
     needs more than 6 rounds of the three steps.
3. **Inventories:** `dotnet run --project csharp/src/SuperMetroid.ArchitectureAudit -c Release -- --solution csharp/SuperMetroid.Full.slnx --write`.
4. **Nothing changed:** the run ends quietly. A change outside `csharp/` and `docs/` fails the run.
5. **Build once:** `pwsh -File tools/build.ps1 csharp/SuperMetroid.Full.slnx`. A failed build pushes nothing; its full
   output is kept as `logs\<run>\build.log`, STATUS says so and a notification is raised. A build failure is never
   fixed by judgment. The build changes no file; a tree it changed fails the run.
6. **Commit** as `Cleanup: <rule, what, count; ...>` (stale reviews as `stale architecture reviews removed <count>`),
   with the files, the prune's own summary and every removed review in the body, under
   the owner's identity from the main checkout and with no assistant attribution (the clone runs the repository's
   own `commit-msg` and `pre-push` guards), and **push** with a plain fast-forward push.

The service gives every tool its own temporary folder, no shared compiler server (`UseSharedCompilation=false`), no
reused MSBuild nodes and no MSBuild server, so nothing the cleanup starts serves the developer's builds or runs at the
cleanup's Idle priority for them. The NuGet package cache is shared read-mostly, as the brief allows. Git credential
prompts are disabled (`GCM_INTERACTIVE=never`), so a missing credential fails the push instead of opening a dialog.

### The code fixes

`CodeFixRunner.cs` loads every project into one MSBuildWorkspace, runs each project's own analyzers for the requested
rules with its own configuration, and applies each rule's fixer through its fix-all provider, given exactly the
findings chosen. `RuleOrder` names where each rule's one fixer comes from: Roslyn's code-style fixers (the same
fixers `dotnet format style` uses; on a fixture the runner's output for IDE0004, IDE0005, IDE0032, IDE0044, IDE0076
and IDE0077 is byte-identical to `dotnet format style --diagnostics ... --severity info`), or the repository's own
fixers in `SuperMetroid.EnsureAnalyzer.CodeFixes` (SME6391: `AdapterChainCodeFixProvider`; CS0168:
`UnusedVariableCodeFixProvider`). A finding its fixer offers no fix for, or cannot resolve without a choice, fails the
run, naming the site.

A compiler warning in the group (CS0168) has no analyzer: the compiler reports it while it binds. The runner reads it
from the compilation's own diagnostics, with the rule enabled in the per-file options like any other, and applies the
repository's fixer for it. Such a rule is also absent from `WarningsAsErrors` in `csharp/Directory.Build.props`;
since the compiler computes it during binding anyway, `none` saves the build its reporting, not its analysis. A
project that compiled when the run started and has compiler errors after a pass fails the run, naming them.

Since the group is at `none`, the runner enables exactly the requested rules, as warnings, in its own analysis: in
the compilation's per-file diagnostic options (where the `.editorconfig` severity would otherwise win over any
compilation-wide setting) and in the analyzer configuration the code-style analyzers read their severity from. It
enables each rule only where the project's configuration would, were the rule's group line `warning`: it reads the
project's analyzer configuration files again with that one line changed, so the sections that turn the SME rules off
outside Core and AssetExtraction keep SME6391 off there. Every other rule keeps the project's configuration, and the
build itself never runs these analyzers.

Roslyn skips an analyzer only when every rule it reports is off. A repository analyzer that reported a moved rule
beside one that stays an error would still run in every build, so a moved rule gets an analyzer of its own:
SME6391 is `SpanAdapterAnalyzer`, split from `AdapterChainAnalyzer` (SME6390).

It is a runner rather than `dotnet format` because of shared files. A file compiled into several projects
(`csharp/test-support` through `.projitems`, or a `<Compile Link>`) is edited only where every project compiling it
reports the same finding at the same place, and not at all while one of them has a compiler error. Where the projects
disagree about a file's unused usings, only the usings all of them report are removed. `dotnet format` judges a shared
file from one project and would remove usings another project needs. Disagreeing findings are left and counted in the
commit body.

The runner repeats its analysis until a pass finds nothing (one rule per file per pass, since other spans go stale
once a file changes). A file that returns to an earlier text fails the run, and so do more than 20 passes.

A fix may change several files, such as IDE0032 deleting a field in one part of a partial type and rewriting its
property in another. The runner accepts it when every file it changes is compiled by analyzed projects that have no
compiler errors (otherwise its findings are kept and counted), and when no other fix in the same pass touched one of
those files (otherwise it waits for the next pass, since its spans would be stale). Every project compiling one of
its files must still compile afterwards, or the run fails naming the fix. The cycle and pass checks cover every
changed file, and the summary lists them all.

## Fixtures

`tools/cleanup/fixtures/<rule>/` confirms the runner on one rule: `input/` is a small project (`Fixture.csproj`) with
the violating code of every shape the rule's ticket lists, and `expected/` is every file of that project after the
fix, byte for byte.

```powershell
pwsh -File tools/cleanup/check-fixtures.ps1                  # every fixture
pwsh -File tools/cleanup/check-fixtures.ps1 -Rule IDE0032    # only the named ones
```

It builds `SuperMetroid.EnsureAnalyzer.CodeFixes` (and so the analyzer) once in Debug, then for each fixture copies
`input/` into a new temporary directory beside the repository's `csharp/.editorconfig` and a `Directory.Build.props`
that imports `csharp/Directory.Build.props` and names the analyzer and code-fix assemblies (`$(CleanupAnalyzer)`,
`$(CleanupCodeFixes)`) for the fixture's own `<Analyzer>` items. It restores the copy, runs the runner on it for that
rule, compares every file (less `bin/`, `obj/` and the two added files) with `expected/`, including which files
exist, and builds the fixed copy with `tools/build.ps1`. A failed fixture keeps its copy and prints where; the script
exits nonzero when any fixture fails.

**IDE0079 (unnecessary suppression) is in the automatically fixed group but nothing at the command line can apply
it.** The code-style package the build and `dotnet format` load has no analyzer that reports it (only the IDE's
suppression analysis does), so neither `dotnet format style` nor `dotnet format analyzers` changes an unnecessary
`#pragma` or `[SuppressMessage]`, and the runner refuses to be asked for it, failing with that reason. `run.ps1`
leaves IDE0079 out of the rules it reads from `csharp/.editorconfig`. At `none` it costs the build nothing, and an
unnecessary suppression stays until someone removes it by hand.

## Seeing status

```powershell
pwsh -File tools/cleanup/status.ps1
```

prints `STATUS`: `idle`, `running`, `failed` or `stopped`, the tip, what happened, and the run's log. A failed run, a
stop and a trigger that cannot start the service also raise a Windows toast (`notify.ps1`, through Windows
PowerShell), which never takes focus. Every run's full output is `logs\<utc>-<tip>.log`, with its code-fix summaries,
retired fields, build logs and commit message in `logs\<utc>-<tip>\`. `service.log` and `trigger.log` record every
start, request and outcome.

A failed tip is not retried: the next push (a new tip) runs again, and a later successful run clears the failure.

A network step is retried before it fails the run (owner ruling, 2026-11-12). That covers every fetch and the push,
when a transient network or system failure stops them: a dropped connection, or a DNS thread that failed to start.
Each such step is tried up to 4 times in all, the first attempt and three retries, with a 30-second pause before each
retry (`Invoke-NetworkGit` in `common.ps1`). Each failed attempt is logged with git's output. A rejected push is an
outcome, not a failure, and is never retried (see below).

## Conflicts with the developer

The developer runs, before pushing:

```powershell
pwsh -File tools/cleanup/pull.ps1
```

It fetches the cleaned branch and, when it holds commits the branch lacks, merges them (`git merge --no-ff -X ours`),
so main's first-parent history stays the developer's and every conflicting line keeps the developer's side. Cleanup
output is regenerated, never hand-merged: a line the developer also changed is cleaned again by the next run. It
refuses to run over uncommitted changes, then prints the status.

If the developer pushes while a run is in progress, the run's push is rejected. The service then discards its commit,
resets to the new tip and runs again from the top: the cleanup is regenerated, never merged. It does this only when
the branch really moved; a rejection with the branch unmoved fails the run. After 10 consecutive rejections the
service stops and notifies (a count, not a time limit); the next push starts it again.

## The Discord tracker

`tools/discord-updates.ps1` skips commits whose subject starts with `Cleanup:` explicitly, by one switch at the top of
the script (`$CleanupCommits = 'skip'`). Setting it to `'post'` announces them like any other commit, as a short
"Cleanup" note. `tools/discord-updates.md` records the choice.

## Install, update, disable

```powershell
pwsh -File tools/cleanup/install.ps1            # install, or update after tools/cleanup changes
git config --unset cleanup.directory            # disable: the hook does nothing while this is unset
```

`install.ps1` verifies that the main checkout's `core.hooksPath` is its own absolute `.githooks`, clones `origin` into
`<dir>\repo` (or reuses the clone), points the clone's hooks at its own `.githooks`, writes `config.json`, copies
`common.ps1`, `notify.ps1`, `trigger.ps1` and `service.ps1` into `<dir>\bin`, and sets `cleanup.directory` and
`cleanup.branch` in the main checkout's git configuration. A change to any of those four scripts on `main` makes the
next run fail until `install.ps1` is run again, so a stale service never runs silently. To uninstall, unset
`cleanup.directory` and delete `<dir>`.

Inside the worker every process carries `SUPERMETROID_CLEANUP_WORKER=1`: the clone's own `reference-transaction` does
nothing, `post-commit` skips the agent time report and `pre-push` keeps its attribution guard but skips the Discord
reminder.
