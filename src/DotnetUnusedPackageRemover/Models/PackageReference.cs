namespace DotnetUnusedPackageRemover.Models;

public record PackageReference(string Name, string? Version, string ProjectPath)
{
    public override string ToString() => Version != null ? $"{Name} ({Version})" : Name;
}
