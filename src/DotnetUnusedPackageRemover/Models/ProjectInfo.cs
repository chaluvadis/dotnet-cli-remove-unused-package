namespace DotnetUnusedPackageRemover.Models;

public class ProjectInfo
{
    public string ProjectPath { get; set; } = string.Empty;
    public List<PackageReference> PackageReferences { get; set; } = new();
    public List<ProjectReference> ProjectReferences { get; set; } = new();
}
