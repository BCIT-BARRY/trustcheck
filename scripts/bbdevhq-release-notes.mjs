#!/usr/bin/env node
// Prints one version's CHANGELOG section body, for use as GitHub Release notes.
import { readFileSync } from "node:fs";

import { FOOTER } from "./bbdevhq-footer.mjs";

const version = process.argv[2];
if (!version) {
  console.error("usage: bbdevhq-release-notes.mjs <version>");
  process.exit(1);
}

const changelog = readFileSync("CHANGELOG.md", "utf8");
const header = `## [${version}]`;
const start = changelog.indexOf(header);
if (start === -1) {
  console.error(`No CHANGELOG section for ${version}`);
  process.exit(1);
}

const bodyStart = changelog.indexOf("\n", start) + 1;
const nextSection = changelog.slice(bodyStart).search(/^## \[/m);
const body =
  nextSection === -1
    ? changelog.slice(bodyStart)
    : changelog.slice(bodyStart, bodyStart + nextSection);
console.log(`${body.trim()}\n\n${FOOTER}`);
