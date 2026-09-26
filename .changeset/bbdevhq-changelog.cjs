// Changelog lines are the changeset summary only: no commit hashes, no dependency noise.
module.exports = {
  getReleaseLine: async (changeset) => {
    const [firstLine, ...rest] = changeset.summary.trim().split("\n");
    const continuation = rest.map((line) => `  ${line.trimEnd()}`);
    return [`- ${firstLine.trim()}`, ...continuation].join("\n");
  },
  getDependencyReleaseLine: async () => "",
};
