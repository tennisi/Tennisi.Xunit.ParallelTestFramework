using System;
using System.IO;
using System.Linq;
using XmlDocMarkdown.Core;

namespace DocGen;

internal static class Program
{
    private static string OutputDir
    {
        get
        {
            var dir = Directory.GetCurrentDirectory();
            while (!File.Exists(Path.Combine(dir, "Tennisi.Xunit.v2.ParallelTestFramework.sln")))
                dir = Path.GetFullPath(Path.Combine(dir, ".."));
            return dir;
        }
    }
    private static string SourceDir => Path.Combine(OutputDir, $"{Libraries.First()}");
    private static string FooterFile => Path.Combine(OutputDir, $"DocGen/footer.md");
    private static readonly string[] Libraries = {"Tennisi.Xunit","Tennisi.Xunit.v2.ParallelTestFramework"};

    private static string[] ArgsArray(string library)
    {
        var result = new[]
        {
            library,
            OutputDir,
            "--quiet",
            "--visibility", "public",
            "--clean"
        };
        return result;
    }

    public static void Main()
    {
        Console.WriteLine('_');
        Console.WriteLine(SourceDir);
        Console.WriteLine(OutputDir);
        var readmePath = Path.Combine(OutputDir, "README.md");
        File.WriteAllText(readmePath, string.Empty);
        foreach (var libary in Libraries)
        {
            XmlDocMarkdownApp.Run(ArgsArray(libary));
            var indexPath = Path.Combine(OutputDir, libary + ".md");
            var indexContent = File.ReadAllText(indexPath);
            File.AppendAllText(readmePath, Environment.NewLine + indexContent);
        }
        var footerContent = File.ReadAllText(FooterFile);
        File.AppendAllText(readmePath, Environment.NewLine + footerContent);
    }
}