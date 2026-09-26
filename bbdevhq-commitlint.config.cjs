// Conventional Commits, so every commit states its change type.
module.exports = {
  extends: ["@commitlint/config-conventional"],
  // Dependabot bodies carry long release note links that break body line limits.
  ignores: [(message) => message.includes("Signed-off-by: dependabot[bot]")],
  rules: {
    "header-max-length": [2, "always", 72],
  },
};
