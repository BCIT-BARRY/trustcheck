#!/usr/bin/env node
// Fails a pull request into dev that adds no changeset, so every change reaches the CHANGELOG.
import { execFileSync } from "node:child_process";

const [base, head] = process.argv.slice(2);
if (!base || !head) {
  console.error("usage: bbdevhq-changeset-required.mjs <base-sha> <head-sha>");
  process.exit(1);
}

const added = execFileSync(
  "git",
  ["diff", "--name-only", "--diff-filter=A", `${base}...${head}`],
  { encoding: "utf8" },
)
  .split("\n")
  .filter(
    (file) =>
      /^\.changeset\/[^/]+\.md$/.test(file) && !file.endsWith("README.md"),
  );

if (added.length === 0) {
  console.error(
    "No changeset found. Run `pnpm changeset` and commit the generated file.",
  );
  process.exit(1);
}
console.log(`Changeset found: ${added.join(", ")}`);
