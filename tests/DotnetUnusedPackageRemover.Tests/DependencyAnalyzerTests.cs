using DotnetUnusedPackageRemover.Analyzers;
using DotnetUnusedPackageRemover.Models;
using Xunit;

namespace DotnetUnusedPackageRemover.Tests;

public class DependencyAnalyzerTests : IDisposable
{
    private readonly string _testDataDir;

    public DependencyAnalyzerTests()
    {
        _testDataDir = Path.Combine(Path.GetTempPath(), "DotnetUnusedPackageRemoverTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataDir))
        {
            Directory.Delete(_testDataDir, recursive: true);
        }
    }

    [Fact]
    public void AnalyzeProject_WithUsedPackage_ReturnsNoUnusedPackages()
    {
        // Arrange
        var projectPath = Path.Combine(_testDataDir, "test.csproj");
        var sourceFile = Path.Combine(_testDataDir, "Program.cs");
        
        File.WriteAllText(projectPath, "<Project></Project>");
        File.WriteAllText(sourceFile, @"
using Newtonsoft.Json;

public class Program
{
    public static void Main()
    {
        var obj = JsonConvert.SerializeObject(new { });
    }
}");

        var projectInfo = new ProjectInfo
        {
            ProjectPath = projectPath,
            PackageReferences = new List<PackageReference>
            {
                new PackageReference("Newtonsoft.Json", "13.0.1", projectPath)
            }
        };

        var analyzer = new DependencyAnalyzer();

        // Act
        var result = analyzer.AnalyzeProject(projectInfo);

        // Assert
        Assert.Empty(result.UnusedPackages);
    }

    [Fact]
    public void AnalyzeProject_WithUnusedPackage_ReturnsUnusedPackage()
    {
        // Arrange
        var projectPath = Path.Combine(_testDataDir, "test.csproj");
        var sourceFile = Path.Combine(_testDataDir, "Program.cs");
        
        File.WriteAllText(projectPath, "<Project></Project>");
        File.WriteAllText(sourceFile, @"
public class Program
{
    public static void Main()
    {
        Console.WriteLine(""Hello"");
    }
}");

        var projectInfo = new ProjectInfo
        {
            ProjectPath = projectPath,
            PackageReferences = new List<PackageReference>
            {
                new PackageReference("UnusedPackage", "1.0.0", projectPath)
            }
        };

        var analyzer = new DependencyAnalyzer();

        // Act
        var result = analyzer.AnalyzeProject(projectInfo);

        // Assert
        Assert.Single(result.UnusedPackages);
        Assert.Equal("UnusedPackage", result.UnusedPackages[0].Name);
    }

    [Fact]
    public void AnalyzeProject_WithNoPackages_ReturnsEmptyResult()
    {
        // Arrange
        var projectPath = Path.Combine(_testDataDir, "test.csproj");
        var sourceFile = Path.Combine(_testDataDir, "Program.cs");
        
        File.WriteAllText(projectPath, "<Project></Project>");
        File.WriteAllText(sourceFile, "public class Program { }");

        var projectInfo = new ProjectInfo
        {
            ProjectPath = projectPath,
            PackageReferences = new List<PackageReference>()
        };

        var analyzer = new DependencyAnalyzer();

        // Act
        var result = analyzer.AnalyzeProject(projectInfo);

        // Assert
        Assert.Empty(result.UnusedPackages);
        Assert.Empty(result.UnusedProjectReferences);
    }
}
