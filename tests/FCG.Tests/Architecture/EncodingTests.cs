using System.Text;

namespace FCG.Tests.Architecture;

[Trait("Category", "Architecture")]
public sealed class EncodingTests
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private static readonly string[] TextFileExtensions =
    [
        ".cs",
        ".csproj",
        ".json",
        ".md",
        ".slnx",
        ".props",
        ".targets",
        ".config"
    ];

    private static readonly string[] IgnoredDirectories =
    [
        ".git",
        ".vs",
        "bin",
        "obj"
    ];

    private static readonly string[] MojibakePatterns =
    [
        BuildPattern(0x00C3, 0x00A1),
        BuildPattern(0x00C3, 0x00A9),
        BuildPattern(0x00C3, 0x00AD),
        BuildPattern(0x00C3, 0x00B3),
        BuildPattern(0x00C3, 0x00BA),
        BuildPattern(0x00C3, 0x00A3),
        BuildPattern(0x00C3, 0x00B5),
        BuildPattern(0x00C3, 0x00A7),
        BuildPattern(0x00C3, 0x00AA),
        BuildPattern(0x00C3, 0x00B4),
        BuildPattern(0x00C2, 0x00BA),
        BuildPattern(0xFFFD)
    ];

    [Fact]
    public void Arquivos_Texto_Do_Projeto_Nao_Devem_Conter_Mojibake()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var filesWithEncodingProblems = GetProjectTextFiles(repositoryRoot)
            .Select(FindEncodingProblem)
            .Where(problem => problem is not null)
            .ToArray();

        // Assert
        filesWithEncodingProblems.ShouldBeEmpty();
    }

    private static string? FindEncodingProblem(string filePath)
    {
        string content;

        try
        {
            content = File.ReadAllText(filePath, StrictUtf8);
        }
        catch (DecoderFallbackException)
        {
            return $"{filePath} is not valid UTF-8.";
        }

        var mojibakePattern = MojibakePatterns.FirstOrDefault(content.Contains);

        return mojibakePattern is null
            ? null
            : $"{filePath} contains possible mojibake: {ToCodePoints(mojibakePattern)}.";
    }

    private static IEnumerable<string> GetProjectTextFiles(string repositoryRoot)
    {
        return Directory.EnumerateFiles(repositoryRoot, "*", SearchOption.AllDirectories)
            .Where(filePath => TextFileExtensions.Contains(Path.GetExtension(filePath)))
            .Where(filePath => !HasIgnoredDirectory(repositoryRoot, filePath));
    }

    private static bool HasIgnoredDirectory(string repositoryRoot, string filePath)
    {
        var relativePath = Path.GetRelativePath(repositoryRoot, filePath);
        var directories = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return directories.Any(directory => IgnoredDirectories.Contains(directory));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "FiapCloudGames.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private static string BuildPattern(params int[] codePoints)
    {
        return new string(codePoints.Select(codePoint => (char)codePoint).ToArray());
    }

    private static string ToCodePoints(string value)
    {
        return string.Join(
            " ",
            value.Select(character => $"U+{(int)character:X4}"));
    }
}
