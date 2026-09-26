#!/usr/bin/env node
// Runs `changeset version`, then rewrites its CHANGELOG entry into Keep a Changelog form.
import { execSync } from "node:child_process";
import { readdirSync, readFileSync, writeFileSync } from "node:fs";

const CHANGELOG = "CHANGELOG.md";
const HEADINGS = { Major: "Changed", Minor: "Added", Patch: "Fixed" };

const pending = readdirSync(".changeset").filter((file) => file.endsWith(".md") && file !== "README.md");
if (pending.length === 0) {
  console.log("No pending changesets; nothing to version.");
  process.exit(0);
}

const before = readFileSync(CHANGELOG, "utf8");
execSync("pnpm exec changeset version", { stdio: "inherit" });
const after = readFileSync(CHANGELOG, "utf8");

if (after === before) {
  console.log("No pending changesets; CHANGELOG unchanged.");
  process.exit(0);
}

// Changesets inserts its entry right after the file's first line.
const firstLineEnd = before.indexOf("\n");
const entry = after
  .slice(firstLineEnd, after.length - (before.length - firstLineEnd - 1))
  .trim();

const today = new Date().toISOString().slice(0, 10);
const formatted = entry
  .replace(/^## (\d+\.\d+\.\d+)$/m, `## [$1] - ${today}`)
  .replace(
    /^### (Major|Minor|Patch) Changes$/gm,
    (_, bump) => `### ${HEADINGS[bump]}`,
  );

const firstRelease = before.search(/^## \[\d/m);
const insertAt = firstRelease === -1 ? before.length : firstRelease;
const updated = `${before.slice(0, insertAt)}${formatted}\n\n${before.slice(insertAt)}`;

writeFileSync(CHANGELOG, updated);
console.log(`CHANGELOG.md updated with ${formatted.split("\n")[0]}`);
