"""Targeted, resumable history cleanup and in-place Discord link migration.

All recovery data lives in the Git common directory, never in a tracked file.
Requires Python 3.11+, Git, and an explicitly supplied git-filter-repo script.
See docs/publication-audit/README.md. No command changes repository visibility.
"""
from __future__ import annotations

import argparse
import contextlib
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import time
import urllib.error
import urllib.request


class MigrationError(RuntimeError):
    pass


def git(repo, *args, binary=False):
    p = subprocess.run(["git", "-C", str(repo), *args], capture_output=True)
    if p.returncode:
        # Never include credentials or arbitrary subprocess stderr in a failure.
        raise MigrationError(f"Git {args[0]} failed (exit {p.returncode}).")
    return p.stdout if binary else p.stdout.decode("utf-8").strip()


def save(path, data):
    path = Path(path)
    temporary = path.with_name(path.name + ".tmp")
    with temporary.open("w", encoding="utf-8", newline="\n") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)
        f.write("\n")
        f.flush()
        os.fsync(f.fileno())
    os.replace(temporary, path)


def read(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def purge_reason(path):
    """Conservative approved categories; CSV traces and source are retained."""
    low = path.lower()
    if low.endswith((".smc", ".sfc")):
        return "retail-rom"
    if re.search(r"(^|/)standalone-assets/", low):
        return "rom-derived-assets"
    if low.endswith(".smstate"):
        return "rom-containing-debug-state"
    if low.startswith("csharp/test-fixtures/") and low.endswith((".png", ".wav", ".spcu")):
        return "game-capture"
    if low.endswith((".smrec", ".srm")) or low.startswith(("debug-states/", "input-recordings/")):
        return "private-player-state"
    if low.endswith((".dll", ".exe", ".pdb")):
        return "compiled-artifact"
    if low.startswith("csharp/test-temp/"):
        return "temporary-diagnostics"
    return None


def historical_paths(repo, ref="HEAD"):
    raw = git(repo, "-c", "core.quotepath=false", "log", "--format=", "--name-only",
              "--no-renames", "-z", ref, binary=True)
    return sorted({p.decode("utf-8").strip("\n") for p in raw.split(b"\0") if p.strip(b"\n")})


def inventory(repo, destination):
    destination = Path(destination)
    destination.mkdir(parents=True, exist_ok=True)
    current = set(git(repo, "ls-files", "-z", binary=True).decode().split("\0"))
    paths = historical_paths(repo)
    rows = [{"path": p, "present_at_head": p in current, "reason": purge_reason(p)}
            for p in paths if purge_reason(p)]
    save(destination / "inventory.json", {"head": git(repo, "rev-parse", "HEAD"), "paths": rows})
    (destination / "purge-paths.txt").write_text("".join("literal:" + r["path"] + "\n" for r in rows), encoding="utf-8")
    return rows


@contextlib.contextmanager
def queue_lock(directory):
    directory = Path(directory)
    directory.mkdir(parents=True, exist_ok=True)
    path = directory / "queue.lock"
    if os.name == "nt":
        import ctypes
        from ctypes import wintypes
        kernel = ctypes.WinDLL("kernel32", use_last_error=True)
        kernel.CreateFileW.argtypes = [wintypes.LPCWSTR, wintypes.DWORD, wintypes.DWORD,
                                      wintypes.LPVOID, wintypes.DWORD, wintypes.DWORD, wintypes.HANDLE]
        kernel.CreateFileW.restype = wintypes.HANDLE
        kernel.CloseHandle.argtypes = [wintypes.HANDLE]
        handle = kernel.CreateFileW(str(path), 0xC0000000, 0, None, 4, 0x80, None)
        if handle == wintypes.HANDLE(-1).value:
            raise MigrationError("Discord queue lock is held; pause the other process.")
        try:
            yield
        finally:
            kernel.CloseHandle(handle)
    else:
        import fcntl
        with path.open("a+") as f:
            fcntl.flock(f, fcntl.LOCK_EX | fcntl.LOCK_NB)
            yield


def webhook_url():
    value = os.environ.get("DISCORD_WEBHOOK_URL", "")
    if not value and os.name == "nt":
        import winreg
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, "Environment") as key:
            value = winreg.QueryValueEx(key, "DISCORD_WEBHOOK_URL")[0]
    if not re.fullmatch(r"https://discord\.com/api(?:/v10)?/webhooks/\d+/[A-Za-z0-9._-]+", value):
        raise MigrationError("DISCORD_WEBHOOK_URL is missing or invalid.")
    return value


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, *args, **kwargs):
        return None


class Discord:
    def __init__(self):
        self.url = webhook_url()
        self.opener = urllib.request.build_opener(NoRedirect)

    def request(self, method, suffix="", body=None):
        for attempt in range(8):
            data = None if body is None else json.dumps(body).encode("utf-8")
            request = urllib.request.Request(self.url + suffix, data=data, method=method,
                                            headers={"Content-Type": "application/json", "User-Agent": "PublicationMigration/1.0"})
            try:
                with self.opener.open(request, timeout=30) as response:
                    result = json.load(response)
                    remaining = response.headers.get("X-RateLimit-Remaining")
                    delay = float(response.headers.get("X-RateLimit-Reset-After", "0"))
                if remaining == "0" and delay > 0:
                    time.sleep(min(delay + 0.1, 30))
                return result
            except urllib.error.HTTPError as error:
                if error.code == 429:
                    try:
                        delay = float(json.load(error).get("retry_after", 2))
                    except Exception:
                        delay = 2
                    time.sleep(min(max(delay + 0.1, 0.2), 30))
                    continue
                raise MigrationError(f"Discord HTTP {error.code}; checkpoint retained. Resume to reconcile.") from None
            except Exception:
                raise MigrationError("Discord delivery/read uncertain; checkpoint retained. Resume to reconcile.") from None
        raise MigrationError("Discord remained rate limited; checkpoint retained.")

    def validate(self, tracker):
        receipt = self.request("GET")
        if str(receipt.get("channel_id")) != tracker["channelId"] or str(receipt.get("guild_id")) != tracker["serverId"]:
            raise MigrationError("Webhook destination does not match the tracker.")
        return str(receipt["id"])

    def get(self, message):
        return self.request("GET", "/messages/" + message)

    def edit(self, message, content):
        return self.request("PATCH", "/messages/" + message,
                            {"content": content, "allowed_mentions": {"parse": []}, "flags": 4})


def validate_receipt(receipt, entry, expected):
    if (str(receipt.get("id")) != entry["messageId"] or
        str(receipt.get("channel_id")) != entry["channelId"] or receipt.get("content") != expected):
        raise MigrationError(f"Message {entry['messageId']} does not match the saved content/destination.")


def mapped_content(entry, mapping, repository_url):
    old = entry["commit"]
    new = mapping.get(old)
    if not new or new == "0" * 40:
        raise MigrationError("A Discord commit has no preserved replacement.")
    heading = "**Version " + old[:7] + "**"
    url = repository_url + "/commit/" + old
    content = entry["content"]
    if not content.startswith(heading + "\n") or not content.endswith("\n" + url):
        raise MigrationError(f"Unexpected announcement structure: {entry['messageId']}")
    # Preserve the entire authored summary, including incidental old hash references.
    return "**Version " + new[:7] + "**" + content[len(heading):-len(url)] + repository_url + "/commit/" + new


def migrate_messages(tracker, mapping, progress, checkpoint, discord, limit=0):
    completed = set(progress.setdefault("edited", []))
    count = 0
    for entry in tracker["history"]:
        message = entry["messageId"]
        if message in completed:
            continue
        expected = mapped_content(entry, mapping, tracker["repositoryUrl"])
        receipt = discord.get(message)
        if receipt.get("content") != expected:
            validate_receipt(receipt, entry, entry["content"])
            progress["pendingEdit"] = message
            save(checkpoint, progress)
            receipt = discord.edit(message, expected)
            validate_receipt(receipt, entry, expected)
        validate_receipt(receipt, entry, expected)
        # A GET after PATCH also confirms that the stored message actually changed.
        validate_receipt(discord.get(message), entry, expected)
        progress["edited"].append(message)
        progress["pendingEdit"] = None
        save(checkpoint, progress)
        completed.add(message)
        count += 1
        if count == 1 or count % 20 == 0:
            print(f"Verified Discord edits: {len(completed)}/{len(tracker['history'])}", flush=True)
        if limit and count >= limit:
            break
    return len(completed) == len(tracker["history"])


class BatchObjects:
    def __init__(self, repo):
        self.p = subprocess.Popen(["git", "-C", str(repo), "cat-file", "--batch"],
                                  stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL)
        self.trees = {}

    def get(self, sha):
        self.p.stdin.write(sha.encode() + b"\n")
        self.p.stdin.flush()
        header = self.p.stdout.readline().split()
        if len(header) != 3:
            raise MigrationError("Missing Git object during verification.")
        size = int(header[2])
        data = self.p.stdout.read(size)
        if len(data) != size or self.p.stdout.read(1) != b"\n":
            raise MigrationError("Incomplete Git object read.")
        return data

    def tree(self, sha, prefix=""):
        if sha not in self.trees:
            entries = []
            data = self.get(sha)
            offset = 0
            while offset < len(data):
                end = data.index(b"\0", offset)
                mode, name = data[offset:end].split(b" ", 1)
                oid = data[end + 1:end + 21].hex()
                entries.append((mode, name.decode("utf-8"), oid))
                offset = end + 21
            self.trees[sha] = entries
        for mode, name, oid in self.trees[sha]:
            path = prefix + name
            if mode == b"40000":
                yield from self.tree(oid, path + "/")
            else:
                yield path, (mode, oid)

    def close(self):
        self.p.stdin.close()
        self.p.stdout.close()
        self.p.wait()


def commit_parts(raw):
    header, message = raw.split(b"\n\n", 1)
    lines = header.split(b"\n")
    tree = next(x[5:].decode() for x in lines if x.startswith(b"tree "))
    parents = [x[7:].decode() for x in lines if x.startswith(b"parent ")]
    identity = [x for x in lines if x.startswith((b"author ", b"committer ", b"encoding "))]
    return tree, parents, identity, message


def load_map(path):
    result = {}
    for line in Path(path).read_text().splitlines()[1:]:
        old, new = line.split()
        if new == "0" * 40:
            raise MigrationError("Rewrite dropped a commit; pruning must be disabled.")
        result[old] = new
    return result


def verify_rewrite(original, rewritten, old_tip, new_tip, mapping):
    old_commits = git(original, "rev-list", old_tip).splitlines()
    new_commits = set(git(rewritten, "rev-list", new_tip).splitlines())
    if set(old_commits) != set(mapping) or set(mapping.values()) != new_commits or len(old_commits) != len(new_commits):
        raise MigrationError("Rewrite did not preserve a one-to-one commit mapping.")
    old_batch, new_batch = BatchObjects(original), BatchObjects(rewritten)
    try:
        for index, old in enumerate(old_commits, 1):
            ot, op, oi, om = commit_parts(old_batch.get(old))
            nt, np, ni, nm = commit_parts(new_batch.get(mapping[old]))
            if [mapping[p] for p in op] != np or oi != ni or om != nm:
                raise MigrationError("Commit identity, message, or parent topology changed unexpectedly.")
            expected = {p: v for p, v in old_batch.tree(ot) if not purge_reason(p)}
            actual = dict(new_batch.tree(nt))
            if expected != actual:
                raise MigrationError("Rewritten tree differs from the precise approved path removal.")
            if index % 200 == 0:
                print(f"Verified complete commit trees: {index}/{len(old_commits)}", flush=True)
    finally:
        old_batch.close()
        new_batch.close()
    return len(old_commits)


class Migration:
    def __init__(self, repo):
        self.repo = Path(repo).resolve()
        self.common = Path(git(repo, "rev-parse", "--path-format=absolute", "--git-common-dir"))
        self.directory = self.common / "publication-migration"
        self.queue = self.common / "discord-updates"
        self.marker = self.queue / "migration-active.json"
        self.checkpoint = self.directory / "progress.json"
        self.clone = self.directory / "rewritten.git"

    def remote_refs(self):
        lines = git(self.repo, "ls-remote", "origin", "refs/heads/*", "refs/tags/*").splitlines()
        return {line.split()[1]: line.split()[0] for line in lines}

    def prepare(self):
        if self.checkpoint.exists():
            progress = read(self.checkpoint)
            if not progress.get("localReconciled"):
                save(self.marker, {"checkpoint": str(self.checkpoint), "reason": "Publication migration is active."})
            return progress
        self.directory.mkdir(parents=True, exist_ok=True)
        tracker = read(self.queue / "state.json")
        if tracker["pending"]:
            raise MigrationError("Reconcile the existing pending announcement before migration.")
        refs = self.remote_refs()
        if set(refs) != {"refs/heads/main"}:
            raise MigrationError("Unexpected remote branches/tags; extend the audited rewrite scope first.")
        tip = refs["refs/heads/main"]
        if git(self.repo, "rev-parse", "HEAD") != tip:
            raise MigrationError("Migration checkout must match published main exactly.")
        if tracker["cursor"] != tip:
            raise MigrationError("Drain the normal Discord queue before indexing.")
        entries = tracker["history"]
        if len({e["messageId"] for e in entries}) != len(entries) or len({e["commit"] for e in entries}) != len(entries):
            raise MigrationError("Duplicate tracker identities.")
        discord = Discord()
        webhook = discord.validate(tracker)
        # Confirm access before doing any history work.
        for entry in (entries[0], entries[-1]):
            validate_receipt(discord.get(entry["messageId"]), entry, entry["content"])
        save(self.directory / "tracker.original.json", tracker)
        save(self.directory / "links.original.json", entries)
        if not (self.directory / "original.bundle").exists():
            git(self.repo, "bundle", "create", str(self.directory / "original.bundle"), "--all")
        git(self.repo, "bundle", "verify", str(self.directory / "original.bundle"))
        inventory(self.repo, self.directory)
        progress = {"version": 1, "oldTip": tip, "remote": git(self.repo, "remote", "get-url", "origin"),
                    "webhookId": webhook, "stage": "indexed", "indexed": [], "edited": [], "pendingEdit": None}
        save(self.checkpoint, progress)
        save(self.marker, {"checkpoint": str(self.checkpoint), "reason": "History/link migration in progress; normal posting is paused."})
        return progress

    def preflight(self, limit=0):
        p = read(self.checkpoint)
        tracker = read(self.directory / "tracker.original.json")
        discord = Discord()
        if discord.validate(tracker) != p["webhookId"]:
            raise MigrationError("Use the original indexed webhook.")
        done = set(p["indexed"])
        count = 0
        for entry in tracker["history"]:
            if entry["messageId"] in done:
                continue
            validate_receipt(discord.get(entry["messageId"]), entry, entry["content"])
            p["indexed"].append(entry["messageId"])
            save(self.checkpoint, p)
            count += 1
            if count == 1 or count % 25 == 0:
                print(f"Verified original Discord messages: {len(p['indexed'])}/{len(tracker['history'])}", flush=True)
            if limit and count >= limit:
                break
        return len(p["indexed"])

    def rewrite(self, filter_repo):
        p = read(self.checkpoint)
        if p["stage"] != "indexed":
            raise MigrationError("Rewrite already started/completed; use verify or resume, never overwrite it.")
        if self.clone.exists():
            raise MigrationError("Existing rewrite directory requires inspection; refusing to discard it.")
        if self.remote_refs() != {"refs/heads/main": p["oldTip"]}:
            raise MigrationError("Remote changed after indexing.")
        git(self.repo, "clone", "--bare", "--no-local", "--single-branch", "--branch", "main", p["remote"], str(self.clone))
        result = subprocess.run([sys.executable, str(Path(filter_repo).resolve()), "--invert-paths",
                                 "--paths-from-file", str(self.directory / "purge-paths.txt"),
                                 "--prune-empty", "never", "--prune-degenerate", "never",
                                 "--preserve-commit-hashes", "--preserve-commit-encoding", "--replace-refs", "delete-no-add"],
                                cwd=self.clone, capture_output=True)
        if result.returncode:
            raise MigrationError("git-filter-repo failed; inspect the private clone before resuming.")
        shutil.copyfile(self.clone / "filter-repo" / "commit-map", self.directory / "commit-map")
        p["newTip"] = git(self.clone, "rev-parse", "main")
        p["stage"] = "rewritten"
        save(self.checkpoint, p)
        return self.verify()

    def verify(self):
        p = read(self.checkpoint)
        mapping = load_map(self.directory / "commit-map")
        total = verify_rewrite(self.repo, self.clone, p["oldTip"], p["newTip"], mapping)
        tracker = read(self.directory / "tracker.original.json")
        links = [{"oldSha": e["commit"], "newSha": mapping[e["commit"]], "messageId": e["messageId"],
                  "channelId": e["channelId"], "oldContent": e["content"],
                  "newContent": mapped_content(e, mapping, tracker["repositoryUrl"])} for e in tracker["history"]]
        save(self.directory / "links.mapped.json", links)
        save(self.directory / "verification.json", {"commits": total, "oneToOne": True, "treesExact": True,
                                                    "authorsDatesMessagesParentsPreserved": True, "newTip": p["newTip"]})
        if p["stage"] == "rewritten":
            p["stage"] = "verified"
            save(self.checkpoint, p)
        return {"commits": total, "links": len(links), "newTip": p["newTip"]}

    def publish(self):
        p = read(self.checkpoint)
        if p["stage"] not in ("verified", "published"):
            raise MigrationError("Verify the rewrite before publishing.")
        tracker = read(self.directory / "tracker.original.json")
        if len(p["indexed"]) != len(tracker["history"]):
            raise MigrationError("Every existing Discord message must pass preflight before publishing.")
        if read(self.queue / "state.json") != tracker:
            raise MigrationError("Discord tracker changed after indexing.")
        refs = self.remote_refs()
        if refs != {"refs/heads/main": p["newTip"]}:
            if refs != {"refs/heads/main": p["oldTip"]}:
                raise MigrationError("Remote changed; refusing to overwrite another process's work.")
            self.verify()
            git(self.clone, "push", "--force-with-lease=refs/heads/main:" + p["oldTip"],
                p["remote"], "refs/heads/main:refs/heads/main")
        if self.remote_refs() != {"refs/heads/main": p["newTip"]}:
            raise MigrationError("Remote rewrite receipt is uncertain; resume publish to reconcile.")
        p["stage"] = "published"
        save(self.checkpoint, p)
        return {"stage": p["stage"], "newTip": p["newTip"]}

    def edit(self, limit=0):
        p = read(self.checkpoint)
        if p["stage"] == "complete":
            return {"stage": "complete", "edited": len(p["edited"])}
        if p["stage"] != "published":
            raise MigrationError("Publish the verified history before updating links.")
        if self.remote_refs() != {"refs/heads/main": p["newTip"]}:
            raise MigrationError("Published history changed during link migration.")
        tracker = read(self.directory / "tracker.original.json")
        mapping = load_map(self.directory / "commit-map")
        discord = Discord()
        if discord.validate(tracker) != p["webhookId"]:
            raise MigrationError("Use the original indexed webhook.")
        done = migrate_messages(tracker, mapping, p, self.checkpoint, discord, limit)
        if done:
            updated = json.loads(json.dumps(tracker))
            updated["cursor"] = mapping[tracker["cursor"]]
            for entry in updated["history"]:
                entry["content"] = mapped_content(entry, mapping, tracker["repositoryUrl"])
                entry["commit"] = mapping[entry["commit"]]
            live = read(self.queue / "state.json")
            if live not in (tracker, updated):
                raise MigrationError("Live tracker diverged; refusing to overwrite it.")
            save(self.queue / "state.json", updated)
            p["stage"] = "complete"
            save(self.checkpoint, p)
            # Local worktree reconciliation must finish before normal queue use resumes.
        return {"stage": p["stage"], "edited": len(p["edited"]), "total": len(tracker["history"])}

    def reconcile(self):
        p = read(self.checkpoint)
        if p["stage"] != "complete":
            raise MigrationError("Finish Discord edits before reconciling local checkouts.")
        mapping = load_map(self.directory / "commit-map")
        plan_path = self.directory / "local-worktrees.json"
        if plan_path.exists():
            plan = read(plan_path)
        else:
            plan = []
            blocks = git(self.repo, "worktree", "list", "--porcelain").split("\n\n")
            for block in blocks:
                values = dict(line.split(" ", 1) for line in block.splitlines() if " " in line)
                if "branch" not in values:
                    raise MigrationError("Detached worktree requires explicit reconciliation.")
                worktree, old = values["worktree"], values["HEAD"]
                if old not in mapping:
                    raise MigrationError("A local worktree has unindexed commits; preserve and reconcile them first.")
                if git(worktree, "diff", "--cached", "--name-only"):
                    raise MigrationError("Staged work needs explicit preservation before index migration.")
                dirty = git(worktree, "diff", "--name-only", "-z", binary=True).decode().split("\0")
                hashes = {name: hashlib.sha256((Path(worktree) / name).read_bytes()).hexdigest()
                          if (Path(worktree) / name).is_file() else None for name in dirty if name}
                index = git(worktree, "rev-parse", "--path-format=absolute", "--git-path", "index")
                shutil.copyfile(index, self.directory / f"worktree-{len(plan)}.index.backup")
                plan.append({"path": worktree, "branch": values["branch"], "old": old,
                             "new": mapping[old], "dirtyHashes": hashes})
            save(plan_path, plan)
        git(self.repo, "fetch", "--no-tags", "origin", "+refs/heads/main:refs/remotes/origin/main")
        if git(self.repo, "rev-parse", "origin/main") != p["newTip"]:
            raise MigrationError("Remote moved before local reconciliation.")
        for entry in plan:
            worktree = entry["path"]
            head = git(worktree, "rev-parse", "HEAD")
            if head not in (entry["old"], entry["new"], p["newTip"]):
                raise MigrationError("Local work advanced during reconciliation.")
            if head == entry["old"]:
                git(worktree, "update-ref", entry["branch"], entry["new"], entry["old"])
                head = entry["new"]
            # Index only: purged local files and all unstaged edits remain on disk.
            git(worktree, "read-tree", head)
            if entry["branch"] == "refs/heads/main" and head != p["newTip"]:
                git(worktree, "merge", "--ff-only", "origin/main")
            for name, expected in entry["dirtyHashes"].items():
                path = Path(worktree) / name
                actual = hashlib.sha256(path.read_bytes()).hexdigest() if path.is_file() else None
                if actual != expected:
                    raise MigrationError("A pre-existing dirty file changed; retain backups and investigate.")
        # Also migrate local branches that do not have a checked-out worktree.
        for line in git(self.repo, "for-each-ref", "--format=%(refname) %(objectname)", "refs/heads/").splitlines():
            ref, old = line.split()
            if old in mapping:
                git(self.repo, "update-ref", ref, mapping[old], old)
            elif old not in set(mapping.values()):
                raise MigrationError("An unindexed local branch needs explicit reconciliation.")
        p["localReconciled"] = True
        save(self.checkpoint, p)
        self.marker.unlink(missing_ok=True)
        return {"stage": "complete", "worktrees": len(plan), "dirtyFilesPreserved": True}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("command", choices=["inventory", "prepare", "preflight", "rewrite", "verify", "publish", "edit", "reconcile", "status"])
    parser.add_argument("--repository", default=str(Path(__file__).resolve().parent.parent))
    parser.add_argument("--output")
    parser.add_argument("--filter-repo")
    parser.add_argument("--limit", type=int, default=0, help="Maximum messages this invocation; zero means all.")
    args = parser.parse_args()
    migration = Migration(args.repository)
    if args.command == "inventory":
        if not args.output:
            parser.error("inventory requires --output")
        rows = inventory(args.repository, args.output)
        print(json.dumps({"paths": len(rows), "current": sum(r["present_at_head"] for r in rows)}))
        return
    if args.command == "status":
        p = read(migration.checkpoint)
        print(json.dumps({k: p.get(k) for k in ("stage", "oldTip", "newTip", "pendingEdit")} |
                         {"indexed": len(p["indexed"]), "edited": len(p["edited"])}))
        return
    with queue_lock(migration.queue):
        if args.command == "rewrite":
            if not args.filter_repo:
                parser.error("rewrite requires --filter-repo")
            result = migration.rewrite(args.filter_repo)
        elif args.command in ("preflight", "edit"):
            result = getattr(migration, args.command)(args.limit)
        else:
            result = getattr(migration, args.command)()
        if args.command == "prepare":
            result = {"stage": result["stage"], "oldTip": result["oldTip"]}
        print(json.dumps(result), flush=True)


if __name__ == "__main__":
    try:
        main()
    except (MigrationError, OSError, ValueError) as error:
        # OSError may include local paths, but request exceptions are sanitized above.
        print(f"Migration stopped: {error}", file=sys.stderr)
        sys.exit(1)
