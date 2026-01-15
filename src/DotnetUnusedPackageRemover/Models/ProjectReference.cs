namespace DotnetUnusedPackageRemover.Models;

public record ProjectReference(string ReferencePath, string SourceProjectPath)
{
    public override string ToString() => ReferencePath;
}
