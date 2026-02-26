# .NET 9.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 9.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 9.0 upgrade.
3. Upgrade PokemonSweeper.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

No projects are excluded from this upgrade.

### Project upgrade details

This section contains details about each project upgrade and modifications that need to be done in the project.

#### PokemonSweeper.csproj modifications

Project properties changes:
  - Project file needs to be converted from non-SDK-style to SDK-style format
  - Target framework should be changed from `net48` to `net9.0-windows`

