using System;
using System.IO;
using System.Linq;
using System.Net;
using XmlDocMarkdown.Core;

namespace DocGen;

internal static class Program
{
    private static string OutputDir
    {
        get
        {
            var dir = Directory.GetCurrentDirectory();
            while (!File.Exists(Path.Combine(dir, $"{Libraries.First()}.sln")))
                dir = Path.GetFullPath(Path.Combine(dir, ".."));
            return dir;
        }
    }
    private static string SourceDir => Path.Combine(OutputDir, $"{Libraries.First()}");
    private static string FooterFile => Path.Combine(OutputDir, $"DocGen/footer.md");
    private static string Library => typeof(Tennisi.Xunit.ParallelTag).Assembly.FullName!.Split(',')[0];
    private static readonly string[] Libraries = {Library};
    private static readonly string[] ArgsArray = Libraries.Union(new[]
    {
        OutputDir,
        "--quiet",
        "--visibility", "public",
        "--clean"
    }).ToArray();

    public static void Main()
    {
        Console.WriteLine('_');
        Console.WriteLine(SourceDir);
        Console.WriteLine(OutputDir);
        XmlDocMarkdownApp.Run(ArgsArray);
        var indexPath = Path.Combine(OutputDir, Libraries.First() + ".md");
        Console.WriteLine($"indexPath: {indexPath}");
        var readmePath = Path.Combine(OutputDir, "README.md");
        File.Copy(indexPath, readmePath, overwrite: true);
        var footerContent = File.ReadAllText(FooterFile);
        File.AppendAllText(readmePath, Environment.NewLine + footerContent);
        File.Delete(indexPath);
    }
}