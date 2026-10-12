# Security Policy

## Supported versions

Security fixes are currently intended for the latest version on the default branch. Older snapshots may not receive fixes.

## Reporting a vulnerability

Please do not publish credentials, private user data, or a weaponizable exploit in a public issue.

Use GitHub's private vulnerability reporting feature for this repository if it is enabled. If it is unavailable, contact the repository maintainer privately through a verified GitHub contact method and include:

- The affected version or commit.
- The affected file or component.
- A concise description of the impact.
- Reproduction steps or a minimal proof of concept.
- Any known mitigations.

Please allow time for the maintainer to investigate before publicly disclosing an unpatched issue.

## Security-sensitive areas

Changes involving update manifests, downloaded update packages, checksum verification, script execution, file exports, settings paths, or database access should be reviewed carefully. Do not store API keys, passwords, access tokens, or private connection strings in source control.
