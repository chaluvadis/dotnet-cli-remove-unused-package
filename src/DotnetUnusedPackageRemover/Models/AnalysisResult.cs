namespace DotnetUnusedPackageRemover.Models;

public class AnalysisResult
{
    public List<PackageReference> UnusedPackages { get; set; } = new();
    public List<ProjectReference> UnusedProjectReferences { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
