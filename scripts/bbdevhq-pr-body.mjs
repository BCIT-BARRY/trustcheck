#!/usr/bin/env node
// Prints the cut or ship pull request body from CHANGELOG sections.
import { readFileSync } from "node:fs";

const [kind, version, released] = process.argv.slice(2);
if (!["cut", "ship"].includes(kind) || !version) {
  console.error("usage: bbdevhq-pr-body.mjs <cut|ship> <version> [released-version]");
  process.exit(1);
}

const sections = readFileSync("CHANGELOG.md", "utf8")
  .replace(/^\[[^\]]+\]: .*$/gm, "")
  .split(/^(?=## \[\d)/m)
  .slice(1)
  .map((section) => section.trim());
const versionOf = (section) => section.match(/^## \[([^\]]+)\]/)[1];

const start = sections.findIndex((section) => versionOf(section) === version);
if (start === -1) {
  console.error(`No CHANGELOG section for ${version}`);
  process.exit(1);
}
const releasedIndex = sections.findIndex((section) => versionOf(section) === released);
let end = releasedIndex === -1 ? sections.length : releasedIndex;
if (kind === "cut") end = start + 1;

const summary =
  kind === "cut"
    ? [`Cuts ${version} on dev.`, "Squash merge only."]
    : [released ? `Ships ${version} to main, replacing ${released}.` : `Ships ${version} to main.`, "Merge commit only. Never squash."];
const changes = sections.slice(start, end).map((section) => section.replace(/^(#+) /gm, "#$1 "));

console.log(
  [
    "## Summary",
    summary.map((line) => `- ${line}`).join("\n"),
    "## What changed",
    ...changes,
    "## Test plan",
    "- [x] bbdevhq-policy on this PR gates the merge.",
  ].join("\n\n"),
);
