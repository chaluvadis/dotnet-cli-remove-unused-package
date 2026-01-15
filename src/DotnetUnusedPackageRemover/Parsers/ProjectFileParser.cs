using System.Xml.Linq;
using DotnetUnusedPackageRemover.Models;

namespace DotnetUnusedPackageRemover.Parsers;

public class ProjectFileParser
{
    public ProjectInfo ParseProjectFile(string projectPath)
    {
        var projectInfo = new ProjectInfo { ProjectPath = projectPath };

        if (!File.Exists(projectPath))
        {
            throw new FileNotFoundException($"Project file not found: {projectPath}");
        }

        var doc = XDocument.Load(projectPath);
        
        // Parse PackageReference elements
        var packageReferences = doc.Descendants("PackageReference")
            .Select(pr => new PackageReference(
                pr.Attribute("Include")?.Value ?? string.Empty,
                pr.Attribute("Version")?.Value ?? pr.Element("Version")?.Value,
                projectPath
            ))
            .Where(pr => !string.IsNullOrEmpty(pr.Name))
            .ToList();

        projectInfo.PackageReferences = packageReferences;

        // Parse ProjectReference elements
        var projectReferences = doc.Descendants("ProjectReference")
            .Select(pr => new ProjectReference(
                pr.Attribute("Include")?.Value ?? string.Empty,
                projectPath
            ))
            .Where(pr => !string.IsNullOrEmpty(pr.ReferencePath))
            .ToList();

        projectInfo.ProjectReferences = projectReferences;

        return projectInfo;
    }

    public List<string> GetAllProjectFiles(string solutionOrProjectPath)
    {
        var projectFiles = new List<string>();

        if (solutionOrProjectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            projectFiles.Add(Path.GetFullPath(solutionOrProjectPath));
        }
        else if (solutionOrProjectPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
        {
            var solutionDir = Path.GetDirectoryName(solutionOrProjectPath) ?? string.Empty;
            var solutionContent = File.ReadAllLines(solutionOrProjectPath);
            
            foreach (var line in solutionContent)
            {
                if (line.StartsWith("Project(") && line.Contains(".csproj"))
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 3)
                    {
                        var projectPath = parts[1].Trim().Trim('"').Replace('\\', Path.DirectorySeparatorChar);
                        var fullPath = Path.GetFullPath(Path.Combine(solutionDir, projectPath));
                        if (File.Exists(fullPath))
                        {
                            projectFiles.Add(fullPath);
                        }
                    }
                }
            }
        }
        else if (solutionOrProjectPath.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase))
        {
            var solutionDir = Path.GetDirectoryName(solutionOrProjectPath) ?? string.Empty;
            var doc = XDocument.Load(solutionOrProjectPath);
            
            var projects = doc.Descendants("Project")
                .Select(p => p.Attribute("Path")?.Value)
                .Where(p => p != null && p.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                .Select(p => Path.GetFullPath(Path.Combine(solutionDir, p!)))
                .Where(File.Exists)
                .ToList();
            
            projectFiles.AddRange(projects);
        }

        return projectFiles;
    }
}
