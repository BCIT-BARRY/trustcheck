// Conventional Commits, so every commit states its change type.
module.exports = {
  extends: ["@commitlint/config-conventional"],
  rules: {
    "header-max-length": [2, "always", 72],
  },
};
