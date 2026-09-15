# BounceOrBreak

## Project Information
**Unity Version**: 6000.3.9f1

---

## Getting Started

### Script Template
To follow the project's coding conventions, apply the custom script template.

**Windows**:
```
Unity\Hub\Editor\6000.3.9f1\Editor\Data\Resources\ScriptTemplates\1-Scripting__MonoBehaviour Script-NewMonoBehaviourScript.cs.txt
```

Replace the file at the above path with [`/Documents/Development/Conventions/1-Scripting__MonoBehaviour Script-NewMonoBehaviourScript.cs.txt`](</Documents/Development/Conventions/1-Scripting__MonoBehaviour Script-NewMonoBehaviourScript.cs.txt>).

### External Assets

Assets listed in [`asset-dependencies.json`](/asset-dependencies.json) are not included in this repository due to licensing restrictions.
Download and import each asset manually from the Unity Asset Store before running the project.

Each entry contains the following fields:

- `name`: Asset name
- `installPath`: Where the asset should be placed in the project
- `version`: Currently used version
- `storeUrl`: Unity Asset Store link
- `privateRepo`: Private repository name containing the asset

**Note**: All other packages are managed automatically via `Packages/manifest.json`.

---

## Development Guidelines

### Coding Conventions

For coding conventions, refer to [`/Documents/Development/Conventions/CodingConvention.md`](/Documents/Development/Conventions/CodingConvention.md).

### CI/CD

For CI/CD workflow, refer to [`/Documents/Development/CICDWorkflow.md`](/Documents/Development/CICDWorkflow.md).

### Asset Management

Paid assets are tracked in [`asset-dependencies.json`](/asset-dependencies.json).
When modifying paid assets, update this file manually:

- **Adding an asset**: Add an entry to `asset-dependencies.json` and add the `installPath` to `.gitignore`. If the asset emits an analyzer warning that cannot be fixed, add a rule for it to `Assets/Default.ruleset`
- **Updating an asset**: Update the `version` field in `asset-dependencies.json`
- **Removing an asset**: Remove the entry from `asset-dependencies.json` and remove the `installPath` from `.gitignore`

The private repository must mirror the `installPath` structure so assets can be placed at the correct location.