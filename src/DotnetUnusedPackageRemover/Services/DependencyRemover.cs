using System.Xml.Linq;
using DotnetUnusedPackageRemover.Models;

namespace DotnetUnusedPackageRemover.Services;

public class DependencyRemover
{
    public bool RemoveUnusedPackages(List<PackageReference> unusedPackages)
    {
        if (unusedPackages.Count == 0)
        {
            return true;
        }

        var success = true;
        var projectGroups = unusedPackages.GroupBy(p => p.ProjectPath);

        foreach (var group in projectGroups)
        {
            var projectPath = group.Key;
            try
            {
                var doc = XDocument.Load(projectPath);
                
                foreach (var package in group)
                {
                    var elements = doc.Descendants("PackageReference")
                        .Where(pr => pr.Attribute("Include")?.Value == package.Name)
                        .ToList();

                    foreach (var element in elements)
                    {
                        element.Remove();
                    }
                }

                doc.Save(projectPath);
                Console.WriteLine($"Removed {group.Count()} package(s) from {projectPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing packages from {projectPath}: {ex.Message}");
                success = false;
            }
        }

        return success;
    }

    public bool RemoveUnusedProjectReferences(List<ProjectReference> unusedReferences)
    {
        if (unusedReferences.Count == 0)
        {
            return true;
        }

        var success = true;
        var projectGroups = unusedReferences.GroupBy(p => p.SourceProjectPath);

        foreach (var group in projectGroups)
        {
            var projectPath = group.Key;
            try
            {
                var doc = XDocument.Load(projectPath);
                
                foreach (var reference in group)
                {
                    var elements = doc.Descendants("ProjectReference")
                        .Where(pr => pr.Attribute("Include")?.Value == reference.ReferencePath)
                        .ToList();

                    foreach (var element in elements)
                    {
                        element.Remove();
                    }
                }

                doc.Save(projectPath);
                Console.WriteLine($"Removed {group.Count()} project reference(s) from {projectPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing project references from {projectPath}: {ex.Message}");
                success = false;
            }
        }

        return success;
    }
}
