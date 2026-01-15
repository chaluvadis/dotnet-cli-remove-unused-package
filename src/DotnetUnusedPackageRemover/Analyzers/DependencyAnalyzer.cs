using System.Text.RegularExpressions;
using DotnetUnusedPackageRemover.Models;

namespace DotnetUnusedPackageRemover.Analyzers;

public class DependencyAnalyzer
{
    public AnalysisResult AnalyzeProject(ProjectInfo projectInfo)
    {
        var result = new AnalysisResult();

        try
        {
            var sourceFiles = GetSourceFiles(projectInfo.ProjectPath);
            var sourceContent = string.Join("\n", sourceFiles.Select(File.ReadAllText));

            // Analyze package references
            foreach (var package in projectInfo.PackageReferences)
            {
                if (!IsPackageUsed(package, sourceContent, projectInfo.ProjectPath))
                {
                    result.UnusedPackages.Add(package);
                }
            }

            // Analyze project references
            foreach (var projectRef in projectInfo.ProjectReferences)
            {
                if (!IsProjectReferenceUsed(projectRef, sourceContent, projectInfo.ProjectPath))
                {
                    result.UnusedProjectReferences.Add(projectRef);
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error analyzing project {projectInfo.ProjectPath}: {ex.Message}");
        }

        return result;
    }

    private List<string> GetSourceFiles(string projectPath)
    {
        var projectDir = Path.GetDirectoryName(projectPath) ?? string.Empty;
        var sourceFiles = new List<string>();

        if (Directory.Exists(projectDir))
        {
            sourceFiles.AddRange(Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("obj") && !f.Contains("bin")));
        }

        return sourceFiles;
    }

    private bool IsPackageUsed(PackageReference package, string sourceContent, string projectPath)
    {
        // Check for using directives or namespace references
        var packageNameParts = package.Name.Split('.');
        
        // Check if any part of the package name appears in the source code
        foreach (var part in packageNameParts)
        {
            if (part.Length > 2 && Regex.IsMatch(sourceContent, $@"\b{Regex.Escape(part)}\b"))
            {
                return true;
            }
        }

        // Check for the full package name
        if (Regex.IsMatch(sourceContent, $@"\b{Regex.Escape(package.Name)}\b"))
        {
            return true;
        }

        return false;
    }

    private bool IsProjectReferenceUsed(ProjectReference projectRef, string sourceContent, string projectPath)
    {
        var referencedProjectPath = projectRef.ReferencePath;
        var projectDir = Path.GetDirectoryName(projectPath) ?? string.Empty;
        var fullReferencePath = Path.GetFullPath(Path.Combine(projectDir, referencedProjectPath));

        if (!File.Exists(fullReferencePath))
        {
            return false;
        }

        // Get the namespace from the referenced project
        var referencedProjectName = Path.GetFileNameWithoutExtension(fullReferencePath);
        
        // Check if the referenced project's namespace is used
        if (Regex.IsMatch(sourceContent, $@"\b{Regex.Escape(referencedProjectName)}\b"))
        {
            return true;
        }

        return false;
    }
}
