"""Offline migration regressions. No network or real Discord messages."""
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

spec = importlib.util.spec_from_file_location("migration", Path(__file__).resolve().parents[1] / "tools/publication-migration.py")
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)


class FakeDiscord:
    def __init__(self, entry):
        self.receipt = {"id": entry["messageId"], "channel_id": entry["channelId"], "content": entry["content"]}
        self.calls = 0
        self.lose_receipt = False

    def get(self, message):
        return self.receipt.copy()

    def edit(self, message, content):
        self.calls += 1
        self.receipt["content"] = content
        if self.lose_receipt:
            raise m.MigrationError("simulated lost receipt")
        return self.receipt.copy()


class MigrationTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory(prefix="publication-test-")
        self.root = Path(self.tmp.name)
        self.old, self.new = "a" * 40, "b" * 40
        self.entry = {"commit": self.old, "messageId": "123", "channelId": "456",
                      "content": f"**Version {self.old[:7]}**\n```text\nOriginal summary.\n```\nhttps://github.com/example/repo/commit/{self.old}"}
        self.tracker = {"repositoryUrl": "https://github.com/example/repo", "history": [self.entry]}

    def tearDown(self):
        self.tmp.cleanup()

    def test_summary_preserved_exactly(self):
        content = m.mapped_content(self.entry, {self.old: self.new}, self.tracker["repositoryUrl"])
        self.assertEqual(content.split("\n")[1:-1], self.entry["content"].split("\n")[1:-1])
        self.assertTrue(content.startswith("**Version bbbbbbb**"))
        self.assertTrue(content.endswith(self.new))

    def test_lost_edit_receipt_reconciles_without_duplicate_patch(self):
        d = FakeDiscord(self.entry)
        d.lose_receipt = True
        p = {"edited": [], "pendingEdit": None}
        checkpoint = self.root / "progress.json"
        with self.assertRaises(m.MigrationError):
            m.migrate_messages(self.tracker, {self.old: self.new}, p, checkpoint, d)
        self.assertEqual(m.read(checkpoint)["pendingEdit"], "123")
        d.lose_receipt = False
        self.assertTrue(m.migrate_messages(self.tracker, {self.old: self.new}, p, checkpoint, d))
        self.assertEqual(d.calls, 1)
        self.assertEqual(p["edited"], ["123"])
        self.assertTrue(m.migrate_messages(self.tracker, {self.old: self.new}, p, checkpoint, d))
        self.assertEqual(d.calls, 1)

    def test_external_message_change_blocks_edit(self):
        d = FakeDiscord(self.entry)
        d.receipt["content"] = "Manually changed"
        with self.assertRaises(m.MigrationError):
            m.migrate_messages(self.tracker, {self.old: self.new}, {"edited": []}, self.root / "p.json", d)
        self.assertEqual(d.calls, 0)

    def test_wrong_channel_blocks_receipt(self):
        d = FakeDiscord(self.entry)
        d.receipt["channel_id"] = "999"
        with self.assertRaises(m.MigrationError):
            m.migrate_messages(self.tracker, {self.old: self.new}, {"edited": []}, self.root / "p.json", d)

    def test_missing_or_dropped_mapping_rejected(self):
        for mapping in ({}, {self.old: "0" * 40}):
            with self.assertRaises(m.MigrationError):
                m.mapped_content(self.entry, mapping, self.tracker["repositoryUrl"])

    def test_categories_keep_source_and_csv_archives(self):
        for p in ("Super Metroid.smc", "old/name.sfc", "csharp/test-fixtures/a/state.smstate",
                  "standalone-assets/raw/a.bin", "csharp/standalone-assets/a.png",
                  "csharp/test-fixtures/a/x.png", "input-recordings/a.smrec", "csharp/test-temp/a.cs"):
            self.assertTrue(m.purge_reason(p), p)
        for p in ("csharp/src/Runtime.cs", "csharp/test-fixtures/a/capture.zip", "tools/check.py", "docs/logo.png"):
            self.assertIsNone(m.purge_reason(p), p)

    def test_queue_lock_is_exclusive(self):
        with m.queue_lock(self.root):
            with self.assertRaises((m.MigrationError, BlockingIOError)):
                with m.queue_lock(self.root):
                    pass

    @unittest.skipUnless(__import__('os').environ.get("FILTER_REPO_SCRIPT"), "Set FILTER_REPO_SCRIPT for actual Git rewrite regression")
    def test_real_rewrite_preserves_empty_commits_renames_and_merge(self):
        import os
        repo, clone = self.root / "original", self.root / "clean.git"
        subprocess.run(["git", "init", "-q", "-b", "main", str(repo)], check=True)
        m.git(repo, "config", "user.name", "Migration Test")
        m.git(repo, "config", "user.email", "migration@example.invalid")
        (repo / "Super Metroid.smc").write_bytes(b"synthetic cartridge")
        m.git(repo, "add", ".")
        m.git(repo, "commit", "-qm", "ROM-only commit must remain empty")
        (repo / "source.cs").write_text("original code")
        m.git(repo, "add", ".")
        m.git(repo, "commit", "-qm", "Source survives")
        m.git(repo, "checkout", "-qb", "side")
        (repo / "side.cs").write_text("side branch")
        m.git(repo, "add", ".")
        m.git(repo, "commit", "-qm", "Side branch")
        m.git(repo, "checkout", "-q", "main")
        m.git(repo, "mv", "Super Metroid.smc", "renamed.smc")
        m.git(repo, "commit", "-qm", "Rename removed asset")
        m.git(repo, "merge", "--no-ff", "-qm", "Keep merge topology", "side")
        tip = m.git(repo, "rev-parse", "HEAD")
        rows = m.inventory(repo, self.root / "inventory")
        self.assertEqual({r["path"] for r in rows}, {"Super Metroid.smc", "renamed.smc"})
        m.git(repo, "clone", "--bare", "--no-local", str(repo), str(clone))
        subprocess.run([sys.executable, os.environ["FILTER_REPO_SCRIPT"], "--invert-paths",
                        "--paths-from-file", str(self.root / "inventory/purge-paths.txt"),
                        "--prune-empty", "never", "--prune-degenerate", "never",
                        "--preserve-commit-hashes", "--replace-refs", "delete-no-add"], cwd=clone, check=True, capture_output=True)
        mapping = m.load_map(clone / "filter-repo/commit-map")
        self.assertEqual(m.verify_rewrite(repo, clone, tip, m.git(clone, "rev-parse", "main"), mapping), 5)
        # A retained-file mutation must fail the same production verifier.
        m.git(clone, "update-ref", "refs/heads/main", mapping[tip])
        bad = dict(mapping)
        bad[tip] = mapping[m.git(repo, "rev-parse", "main~1")]
        with self.assertRaises(m.MigrationError):
            m.verify_rewrite(repo, clone, tip, m.git(clone, "rev-parse", "main"), bad)
        # Reconcile a dirty checkout without deleting its private cartridge copy.
        (repo / "source.cs").write_text("unfinished user edit")
        (repo / "renamed.smc").write_bytes(b"private local cartridge edit")
        m.git(repo, "remote", "add", "origin", str(clone))
        migration = m.Migration(repo)
        migration.directory.mkdir()
        migration.queue.mkdir()
        __import__('shutil').copyfile(clone / "filter-repo/commit-map", migration.directory / "commit-map")
        m.save(migration.checkpoint, {"stage": "complete", "oldTip": tip, "newTip": mapping[tip]})
        m.save(migration.marker, {})
        self.assertTrue(migration.reconcile()["dirtyFilesPreserved"])
        self.assertEqual((repo / "source.cs").read_text(), "unfinished user edit")
        self.assertEqual((repo / "renamed.smc").read_bytes(), b"private local cartridge edit")
        self.assertNotIn("renamed.smc", m.git(repo, "ls-files"))
        self.assertEqual(m.git(repo, "rev-parse", "side"), mapping[m.git(repo, "rev-parse", "main^2")]
                         if m.git(repo, "rev-parse", "main^2") in mapping else m.git(repo, "rev-parse", "main^2"))
        self.assertFalse(migration.marker.exists())


if __name__ == "__main__":
    unittest.main()
