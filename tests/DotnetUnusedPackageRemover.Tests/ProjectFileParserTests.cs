using DotnetUnusedPackageRemover.Parsers;
using Xunit;

namespace DotnetUnusedPackageRemover.Tests;

public class ProjectFileParserTests : IDisposable
{
    private readonly string _testDataDir;

    public ProjectFileParserTests()
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
    public void ParseProjectFile_WithPackageReferences_ReturnsCorrectPackages()
    {
        // Arrange
        var projectPath = Path.Combine(_testDataDir, "test.csproj");
        var projectContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Newtonsoft.Json"" Version=""13.0.1"" />
    <PackageReference Include=""Serilog"" Version=""2.10.0"" />
  </ItemGroup>
</Project>";
        File.WriteAllText(projectPath, projectContent);

        var parser = new ProjectFileParser();

        // Act
        var result = parser.ParseProjectFile(projectPath);

        // Assert
        Assert.Equal(projectPath, result.ProjectPath);
        Assert.Equal(2, result.PackageReferences.Count);
        Assert.Contains(result.PackageReferences, p => p.Name == "Newtonsoft.Json" && p.Version == "13.0.1");
        Assert.Contains(result.PackageReferences, p => p.Name == "Serilog" && p.Version == "2.10.0");
    }

    [Fact]
    public void ParseProjectFile_WithProjectReferences_ReturnsCorrectReferences()
    {
        // Arrange
        var projectPath = Path.Combine(_testDataDir, "test.csproj");
        var projectContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include=""..\OtherProject\OtherProject.csproj"" />
  </ItemGroup>
</Project>";
        File.WriteAllText(projectPath, projectContent);

        var parser = new ProjectFileParser();

        // Act
        var result = parser.ParseProjectFile(projectPath);

        // Assert
        Assert.Single(result.ProjectReferences);
        Assert.Equal(@"..\OtherProject\OtherProject.csproj", result.ProjectReferences[0].ReferencePath);
    }

    [Fact]
    public void GetAllProjectFiles_WithCsprojPath_ReturnsSingleProject()
    {
        // Arrange
        var projectPath = Path.Combine(_testDataDir, "test.csproj");
        File.WriteAllText(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        var parser = new ProjectFileParser();

        // Act
        var result = parser.GetAllProjectFiles(projectPath);

        // Assert
        Assert.Single(result);
        Assert.Equal(Path.GetFullPath(projectPath), result[0]);
    }

    [Fact]
    public void ParseProjectFile_FileNotFound_ThrowsException()
    {
        // Arrange
        var parser = new ProjectFileParser();
        var nonExistentPath = Path.Combine(_testDataDir, "nonexistent.csproj");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => parser.ParseProjectFile(nonExistentPath));
    }
}
