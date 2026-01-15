# dotnet-cli-remove-unused-package

A .NET CLI tool to identify and remove unused NuGet packages and project references from .NET solutions.

## Features

- ✅ Scan `.sln`, `.slnx`, or `.csproj` files to identify unused dependencies
- ✅ Detect unused NuGet package references
- ✅ Detect unused project references
- ✅ Safe removal with user confirmation
- ✅ Support for multi-project solutions
- ✅ Cross-platform (Windows, macOS, Linux)

## Installation

### Build and Install Locally

```bash
# Clone the repository
git clone https://github.com/chaluvadis/dotnet-cli-remove-unused-package.git
cd dotnet-cli-remove-unused-package

# Build and pack the tool
dotnet pack src/DotnetUnusedPackageRemover/DotnetUnusedPackageRemover.csproj -c Release

# Install as a global tool
dotnet tool install --global --add-source ./src/DotnetUnusedPackageRemover/bin/Release DotnetUnusedPackageRemover
```

### Install from NuGet (when published)

```bash
dotnet tool install --global DotnetUnusedPackageRemover
```

## Usage

### Scan for Unused Dependencies

Scan a solution file:
```bash
dotnet-remove-unused path/to/solution.sln
```

Scan a project file:
```bash
dotnet-remove-unused path/to/project.csproj
```

Scan the current directory (auto-detects solution or project files):
```bash
dotnet-remove-unused
```

### Remove Unused Dependencies

Add the `--remove` flag to remove detected unused dependencies:
```bash
dotnet-remove-unused path/to/solution.sln --remove
```

The tool will prompt for confirmation before removing any dependencies.

### Skip Confirmation

Use `--skip-confirmation` to remove without prompting:
```bash
dotnet-remove-unused path/to/solution.sln --remove --skip-confirmation
```

### Verbose Output

Use `--verbose` for detailed analysis information:
```bash
dotnet-remove-unused path/to/solution.sln --verbose
```

## Examples

### Example 1: Analyze a Solution

```bash
$ dotnet-remove-unused MySolution.sln

Analyzing: MySolution.sln

Found 3 project(s) to analyze.

Project: MyWebApp.csproj
  Unused Packages (2):
    - Newtonsoft.Json (13.0.1)
    - Serilog (2.10.0)

Project: MyLibrary.csproj
  Unused Project References (1):
    - ..\UnusedProject\UnusedProject.csproj

Summary:
  Total unused packages: 2
  Total unused project references: 1

To remove these unused dependencies, run the command again with the --remove flag.
```

### Example 2: Remove Unused Dependencies

```bash
$ dotnet-remove-unused MySolution.sln --remove

Analyzing: MySolution.sln

Found 3 project(s) to analyze.

Project: MyWebApp.csproj
  Unused Packages (2):
    - Newtonsoft.Json (13.0.1)
    - Serilog (2.10.0)

Summary:
  Total unused packages: 2
  Total unused project references: 0

Do you want to remove these unused dependencies? (y/N): y
Removing unused dependencies...
Removed 2 package(s) from /path/to/MyWebApp.csproj
✓ Successfully removed unused dependencies!
```

## How It Works

The tool performs the following steps:

1. **Parse**: Reads solution/project files to identify all projects and their dependencies
2. **Analyze**: Scans source code files (`.cs`) to detect which packages and project references are actually used
3. **Report**: Lists all unused dependencies found
4. **Remove** (optional): With user consent, removes unused dependencies from project files

### Detection Logic

- **Package References**: A package is considered used if any of its namespace components appear in the source code
- **Project References**: A project reference is considered used if the referenced project's namespace appears in the source code

## Limitations

- The tool uses basic text-based analysis which may not catch all usage patterns
- Some packages may be used indirectly (as transitive dependencies) and could be marked as unused
- Always review the results and test your solution after removing dependencies
- The tool analyzes only `.cs` files; other file types are not currently supported

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

This project is licensed under the MIT License.

