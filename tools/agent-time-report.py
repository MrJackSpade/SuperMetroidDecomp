"""Reports, for every commit in a window, how the agent's time between commits split between working and waiting.

Reads the Claude Code session transcripts (one JSON record per line, each timestamped) and the repository's commit
times. Every gap between consecutive transcript records is classified:

  working    the agent was producing output: the gap ends at an assistant record and no tool call is in flight
  tool       a foreground tool call was in flight (the gap lies between a tool call and its result)
  idle       the agent's turn had ended and the gap ends at a task notification, wake-up, goal check-in or hook
             prompt, i.e. the agent was sitting on background work or a timer
  user       the agent's turn had ended and the gap ends at a genuine human message (the user's time, excluded)
  system     anything else, such as context compaction (excluded)

"Waiting" is tool + idle. Tool and idle time are also broken down by what was running (verify-all, build, focused
checks, audit, and so on), taken from the command text of the in-flight foreground call or the running background job.

Usage: python tools/agent-time-report.py [--hours 24] [--transcripts DIR]
"""

import argparse
import collections
import glob
import json
import os
import re
import subprocess
import sys
from datetime import datetime, timedelta, timezone


def parse_time(text):
    return datetime.fromisoformat(text.replace("Z", "+00:00"))


def classify_command(name, tool_input):
    """Names what a tool call ran, from its tool name and command text."""
    if name not in ("Bash", "PowerShell"):
        return name
    command = tool_input.get("command", "")
    for marker, label in (("verify-all", "verify-all"), ("CartridgeEncodingAudit", "encoding audit"),
                          ("slice.ps1", "slice script"), ("bl.sh", "build"), ("dotnet build", "build"),
                          ("runv.sh", "focused checks"), ("SuperMetroid.Verification.dll", "focused checks"),
                          ("enemy_typed_gen.py", "generator"), ("_wire.py", "edit scripts"), ("_tests.py", "edit scripts"),
                          ("_rest", "edit scripts"), ("discord-updates", "discord"), ("git push", "git"),
                          ("git commit", "git")):
        if marker in command:
            return label
    return name + " (other)"


def load_records(transcript_dir, start):
    """All transcript records from the start time on, in time order, across every session file."""
    records = []
    for path in glob.glob(os.path.join(transcript_dir, "*.jsonl")):
        with open(path, encoding="utf-8") as handle:
            for line in handle:
                record = json.loads(line)
                stamp = record.get("timestamp")
                # Only conversation records bound a gap; housekeeping records (turn durations, queue operations,
                # attachments) sit inside gaps and would otherwise split an idle wait into "working".
                if stamp is None or record.get("isSidechain") or record.get("type") not in ("assistant", "user"):
                    continue
                when = parse_time(stamp)
                if when >= start - timedelta(hours=6):
                    records.append((when, record))
    records.sort(key=lambda item: item[0])
    return records


def content_parts(record):
    content = (record.get("message") or {}).get("content")
    return content if isinstance(content, list) else []


def text_of(record):
    content = (record.get("message") or {}).get("content")
    if isinstance(content, str):
        return content
    return " ".join(part.get("text", "") for part in content_parts(record) if isinstance(part, dict))


def background_jobs(records):
    """Each background job's (start, end, label), matching launches to their completion notifications."""
    launches = {}
    by_tool_use = {}
    for when, record in records:
        for part in content_parts(record):
            if isinstance(part, dict) and part.get("type") == "tool_use" and part.get("input", {}).get("run_in_background"):
                by_tool_use[part["id"]] = (when, classify_command(part["name"], part["input"]))
            if isinstance(part, dict) and part.get("type") == "tool_result" and part.get("tool_use_id") in by_tool_use:
                result = part.get("content")
                result_text = result if isinstance(result, str) else " ".join(
                    item.get("text", "") for item in result or [] if isinstance(item, dict))
                match = re.search(r"with ID: (\w+)", result_text)
                if match:
                    launches[match.group(1)] = by_tool_use[part["tool_use_id"]]
    jobs = []
    for when, record in records:
        match = re.search(r"<task-id>(\w+)</task-id>", text_of(record))
        if match and match.group(1) in launches and "<task-notification>" in text_of(record):
            started, label = launches.pop(match.group(1))
            jobs.append((started, when, label))
    return jobs


def classify_gaps(records, jobs):
    """Yields (start, end, kind, label) for every gap between consecutive records."""
    pending = {}
    turn_ended = False
    for (start, record), (end, following) in zip(records, records[1:]):
        for part in content_parts(record):
            if not isinstance(part, dict):
                continue
            if part.get("type") == "tool_use" and not part.get("input", {}).get("run_in_background"):
                pending[part["id"]] = classify_command(part["name"], part.get("input", {}))
            if part.get("type") == "tool_result":
                pending.pop(part.get("tool_use_id"), None)
        if record.get("type") == "assistant":
            turn_ended = not any(isinstance(p, dict) and p.get("type") == "tool_use" for p in content_parts(record))
        elif record.get("type") == "user" and any(isinstance(p, dict) and p.get("type") == "tool_result" for p in content_parts(record)):
            turn_ended = False

        origin = (following.get("origin") or {}).get("kind")
        if pending:
            yield start, end, "tool", next(iter(pending.values()))
        elif following.get("isCompactSummary"):
            yield start, end, "system", "compaction"
        elif turn_ended and following.get("type") == "user" and origin == "human":
            yield start, end, "user", "user"
        elif turn_ended and following.get("type") == "user":
            running = [label for began, finished, label in jobs if began <= start and finished >= end]
            yield start, end, "idle", running[0] if running else "timer or hook"
        else:
            yield start, end, "working", "working"


def commits(hours):
    """Commit (time, short hash, subject) pairs for the window, plus the commit just before it as a starting edge."""
    since = (datetime.now(timezone.utc) - timedelta(hours=hours)).isoformat()
    output = subprocess.run(["git", "log", "--first-parent", "--format=%cI%x09%h%x09%s", f"--since={since}"],
                            check=True, capture_output=True, text=True).stdout
    window = [(parse_time(line.split("\t")[0]), line.split("\t")[1], line.split("\t")[2])
              for line in output.splitlines() if line]
    window.sort()
    if not window:
        raise SystemExit("No commits in the window.")
    before = subprocess.run(["git", "log", "-1", "--format=%cI%x09%h%x09%s", f"{window[0][1]}^"],
                            check=True, capture_output=True, text=True).stdout.strip().split("\t")
    return [(parse_time(before[0]), before[1], before[2])] + window


def overlap(a_start, a_end, b_start, b_end):
    return max(0.0, (min(a_end, b_end) - max(a_start, b_start)).total_seconds())


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--hours", type=float, default=24)
    parser.add_argument("--transcripts", default=os.path.expanduser(
        r"~\.claude\projects\C--Users-Service-Account-SuperMetroidDecomp"))
    args = parser.parse_args()

    edges = commits(args.hours)
    records = load_records(args.transcripts, edges[0][0])
    jobs = background_jobs(records)
    gaps = list(classify_gaps(records, jobs))

    def minutes(seconds):
        return f"{seconds / 60:6.1f}"

    header = f"{'commit':9} {'subject':46} {'span':>6} {'work':>6} {'tool':>6} {'idle':>6} {'user':>6} {'wait%':>6}  top waits (min)"
    print(header)
    print("-" * len(header))
    totals = collections.Counter()
    wait_totals = collections.Counter()
    for (previous, _, _), (when, short, subject) in zip(edges, edges[1:]):
        kinds = collections.Counter()
        waits = collections.Counter()
        for start, end, kind, label in gaps:
            seconds = overlap(start, end, previous, when)
            if seconds:
                kinds[kind] += seconds
                if kind in ("tool", "idle"):
                    waits[label] += seconds
        totals.update(kinds)
        wait_totals.update(waits)
        counted = kinds["working"] + kinds["tool"] + kinds["idle"]
        share = 100 * (kinds["tool"] + kinds["idle"]) / counted if counted else 0
        top = ", ".join(f"{label} {seconds / 60:.1f}" for label, seconds in waits.most_common(3))
        span = (when - previous).total_seconds()
        print(f"{short:9} {subject[:46]:46} {minutes(span)} {minutes(kinds['working'])} {minutes(kinds['tool'])} "
              f"{minutes(kinds['idle'])} {minutes(kinds['user'])} {share:5.0f}%  {top}")

    counted = totals["working"] + totals["tool"] + totals["idle"]
    print("-" * len(header))
    print(f"Totals over {len(edges) - 1} commits: working {totals['working'] / 60:.0f} min, "
          f"waiting {(totals['tool'] + totals['idle']) / 60:.0f} min "
          f"(foreground tools {totals['tool'] / 60:.0f}, idle on background or timers {totals['idle'] / 60:.0f}); "
          f"waiting is {100 * (totals['tool'] + totals['idle']) / counted:.0f}% of agent time. "
          f"Excluded: user {totals['user'] / 60:.0f} min, system {totals['system'] / 60:.0f} min.")
    print("Waiting by cause (min): " + ", ".join(f"{label} {seconds / 60:.0f}" for label, seconds in wait_totals.most_common()))


if __name__ == "__main__":
    try:
        main()
    except Exception:
        import traceback
        traceback.print_exc(file=sys.stderr)
        sys.exit(1)
