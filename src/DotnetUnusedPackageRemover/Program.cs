using System.CommandLine;
using DotnetUnusedPackageRemover.Analyzers;
using DotnetUnusedPackageRemover.Models;
using DotnetUnusedPackageRemover.Parsers;
using DotnetUnusedPackageRemover.Services;

var rootCommand = new RootCommand("A .NET CLI tool to identify and remove unused NuGet packages and project references");

var pathArgument = new Argument<string>(
    name: "path",
    description: "Path to the solution (.sln, .slnx) or project (.csproj) file",
    getDefaultValue: () => Directory.GetCurrentDirectory()
);

var removeOption = new Option<bool>(
    name: "--remove",
    description: "Remove unused packages and project references (requires confirmation)",
    getDefaultValue: () => false
);

var verboseOption = new Option<bool>(
    name: "--verbose",
    description: "Show detailed output",
    getDefaultValue: () => false
);

var skipConfirmationOption = new Option<bool>(
    name: "--skip-confirmation",
    description: "Skip confirmation prompt when removing dependencies",
    getDefaultValue: () => false
);

rootCommand.AddArgument(pathArgument);
rootCommand.AddOption(removeOption);
rootCommand.AddOption(verboseOption);
rootCommand.AddOption(skipConfirmationOption);

rootCommand.SetHandler((string path, bool remove, bool verbose, bool skipConfirmation) =>
{
    try
    {
        // Determine the file to analyze
        string targetFile = path;
        
        if (Directory.Exists(path))
        {
            // Look for solution or project files in the directory
            var slnFiles = Directory.GetFiles(path, "*.sln").Concat(Directory.GetFiles(path, "*.slnx")).ToArray();
            var csprojFiles = Directory.GetFiles(path, "*.csproj");

            if (slnFiles.Length > 0)
            {
                targetFile = slnFiles[0];
            }
            else if (csprojFiles.Length > 0)
            {
                targetFile = csprojFiles[0];
            }
            else
            {
                Console.WriteLine("Error: No solution or project file found in the specified directory.");
                Environment.Exit(1);
            }
        }

        if (!File.Exists(targetFile))
        {
            Console.WriteLine($"Error: File not found: {targetFile}");
            Environment.Exit(1);
        }

        Console.WriteLine($"Analyzing: {targetFile}");
        Console.WriteLine();

        var parser = new ProjectFileParser();
        var analyzer = new DependencyAnalyzer();
        var allUnusedPackages = new List<PackageReference>();
        var allUnusedProjectReferences = new List<ProjectReference>();
        var projectFiles = parser.GetAllProjectFiles(targetFile);

        if (projectFiles.Count == 0)
        {
            Console.WriteLine("No projects found to analyze.");
            Environment.Exit(0);
        }

        Console.WriteLine($"Found {projectFiles.Count} project(s) to analyze.");
        Console.WriteLine();

        foreach (var projectFile in projectFiles)
        {
            if (verbose)
            {
                Console.WriteLine($"Analyzing project: {projectFile}");
            }

            var projectInfo = parser.ParseProjectFile(projectFile);
            var result = analyzer.AnalyzeProject(projectInfo);

            if (result.Errors.Count > 0)
            {
                Console.WriteLine($"Errors in {projectFile}:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
                Console.WriteLine();
            }

            if (result.UnusedPackages.Count > 0 || result.UnusedProjectReferences.Count > 0)
            {
                Console.WriteLine($"Project: {Path.GetFileName(projectFile)}");
                
                if (result.UnusedPackages.Count > 0)
                {
                    Console.WriteLine($"  Unused Packages ({result.UnusedPackages.Count}):");
                    foreach (var package in result.UnusedPackages)
                    {
                        Console.WriteLine($"    - {package}");
                        allUnusedPackages.Add(package);
                    }
                }

                if (result.UnusedProjectReferences.Count > 0)
                {
                    Console.WriteLine($"  Unused Project References ({result.UnusedProjectReferences.Count}):");
                    foreach (var reference in result.UnusedProjectReferences)
                    {
                        Console.WriteLine($"    - {reference}");
                        allUnusedProjectReferences.Add(reference);
                    }
                }

                Console.WriteLine();
            }
        }

        if (allUnusedPackages.Count == 0 && allUnusedProjectReferences.Count == 0)
        {
            Console.WriteLine("✓ No unused packages or project references found!");
            Environment.Exit(0);
        }

        Console.WriteLine($"Summary:");
        Console.WriteLine($"  Total unused packages: {allUnusedPackages.Count}");
        Console.WriteLine($"  Total unused project references: {allUnusedProjectReferences.Count}");
        Console.WriteLine();

        if (remove)
        {
            var shouldRemove = skipConfirmation;
            
            if (!skipConfirmation)
            {
                Console.Write("Do you want to remove these unused dependencies? (y/N): ");
                var response = Console.ReadLine()?.Trim().ToLower();
                shouldRemove = response == "y" || response == "yes";
            }

            if (shouldRemove)
            {
                Console.WriteLine("Removing unused dependencies...");
                var remover = new DependencyRemover();
                
                var packagesRemoved = remover.RemoveUnusedPackages(allUnusedPackages);
                var referencesRemoved = remover.RemoveUnusedProjectReferences(allUnusedProjectReferences);

                if (packagesRemoved && referencesRemoved)
                {
                    Console.WriteLine("✓ Successfully removed unused dependencies!");
                }
                else
                {
                    Console.WriteLine("⚠ Some dependencies could not be removed. Check the output above for details.");
                    Environment.Exit(1);
                }
            }
            else
            {
                Console.WriteLine("Operation cancelled.");
            }
        }
        else
        {
            Console.WriteLine("To remove these unused dependencies, run the command again with the --remove flag.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fatal error: {ex.Message}");
        if (verbose)
        {
            Console.WriteLine(ex.StackTrace);
        }
        Environment.Exit(1);
    }
}, pathArgument, removeOption, verboseOption, skipConfirmationOption);

return await rootCommand.InvokeAsync(args);
