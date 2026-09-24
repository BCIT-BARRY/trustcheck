# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [0.1.0] - 2026-09-24

### Added

- Initial TrustCheck verification backend.
- Verification create, list, get, and run API endpoints.
- Verification lifecycle with Submitted, Verifying, and Completed states.
- Verified and Rejected verification results.
- Request validation and API error responses.
- Background verification processing.
- Repository abstraction for verification persistence.
- DynamoDB persistence using the AWS SDK for .NET.
- DynamoDB Local support for development.
- AWS DynamoDB support in us-west-2.
- Status-CreatedAt-index global secondary index.
- HTTP request file for manual API testing.
- API contract documentation.

[Unreleased]: https://github.com/BCIT-BARRY/trustcheck/compare/0.1.0...HEAD
[0.1.0]: https://github.com/BCIT-BARRY/trustcheck/releases/tag/0.1.0