import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdtempSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";

const SCRIPT = join(import.meta.dirname, "bbdevhq-pr-body.mjs");
const CHANGELOG = `# Changelog

## [Unreleased]

## [0.3.0] - 2026-10-02

### Added

- Third feature.

## [0.2.0] - 2026-10-01

### Fixed

- Second fix.

## [0.1.0] - 2026-09-30

### Added

- First feature.

[0.1.0]: https://example.com/0.1.0
`;

function prBody(...args) {
  const dir = mkdtempSync(join(tmpdir(), "bbdevhq-pr-body-"));
  writeFileSync(join(dir, "CHANGELOG.md"), CHANGELOG);
  return spawnSync("node", [SCRIPT, ...args], { cwd: dir, encoding: "utf8" });
}

test("cut body lists only its own version", () => {
  const { stdout } = prBody("cut", "0.3.0");
  assert.match(stdout, /- Cuts 0\.3\.0 on dev\./);
  assert.match(stdout, /### \[0\.3\.0\]/);
  assert.match(stdout, /#### Added/);
  assert.doesNotMatch(stdout, /0\.2\.0/);
});

test("ship body lists every version since the last release", () => {
  const { stdout } = prBody("ship", "0.3.0", "0.1.0");
  assert.match(stdout, /- Ships 0\.3\.0 to main, replacing 0\.1\.0\./);
  assert.match(stdout, /### \[0\.3\.0\]/);
  assert.match(stdout, /### \[0\.2\.0\]/);
  assert.doesNotMatch(stdout, /### \[0\.1\.0\]/);
});

test("changelog link references stay out of the body", () => {
  const { stdout } = prBody("ship", "0.1.0");
  assert.doesNotMatch(stdout, /example\.com/);
});

test("unknown version fails", () => {
  const { status, stderr } = prBody("cut", "9.9.9");
  assert.equal(status, 1);
  assert.match(stderr, /No CHANGELOG section for 9\.9\.9/);
});
