#!/usr/bin/env node
// Prints the version the pending changesets will produce, or nothing when none bump it.
import { readdirSync, readFileSync } from "node:fs";

const RANK = { patch: 1, minor: 2, major: 3 };
const { name, version } = JSON.parse(readFileSync("package.json", "utf8"));

let highest = 0;
for (const file of readdirSync(".changeset")) {
  if (!file.endsWith(".md") || file === "README.md") continue;
  const frontmatter =
    readFileSync(`.changeset/${file}`, "utf8").split("---")[1] ?? "";
  const match = frontmatter.match(
    new RegExp(`["']?${name}["']?\\s*:\\s*(major|minor|patch)`),
  );
  if (match) highest = Math.max(highest, RANK[match[1]]);
}

if (highest === 0) process.exit(0);

const [major, minor, patch] = version.split(".").map(Number);
const next = {
  3: `${major + 1}.0.0`,
  2: `${major}.${minor + 1}.0`,
  1: `${major}.${minor}.${patch + 1}`,
}[highest];
console.log(next);
