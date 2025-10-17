using System.Text;

namespace CodeAnalysis
{
    public static class RepoPrinter
    {
        private static readonly HashSet<string> IgnoredFolders = new()
        {
            "bin", "obj", "_site", ".git", ".vs", "node_modules", "PrinterLogs"
        };

        private static readonly HashSet<string> IgnoredFiles = new()
        {
            "RepoPrinter.cs", "Program.cs", "RepoStructure.txt"
        };

        public static void PrintRepo(string rootPath = "")
        {
            rootPath ??= Directory.GetCurrentDirectory();

            var sb = new StringBuilder();

            // Always print the root folder itself
            string rootName = new DirectoryInfo(rootPath).Name;
            sb.AppendLine($"[{rootName}]");

            // Start recursion with children only
            foreach (var dir in Directory.GetDirectories(rootPath))
            {
                PrintDirectory(dir, "  ", sb);
            }

            foreach (var file in Directory.GetFiles(rootPath))
            {
                string fileName = Path.GetFileName(file);
                if (!IgnoredFiles.Contains(fileName))
                    sb.AppendLine($"  {fileName}");
            }

            string outputPath = Path.Combine(rootPath, "RepoStructure.txt");
            File.WriteAllText(outputPath, sb.ToString());

            Console.WriteLine($"Repo structure written to {outputPath}");
        }

        private static void PrintDirectory(string path, string indent, StringBuilder sb)
        {
            string folderName = Path.GetFileName(path);
            if (IgnoredFolders.Contains(folderName))
                return;

            sb.AppendLine($"{indent}[{folderName}]");

            foreach (var file in Directory.GetFiles(path))
            {
                string fileName = Path.GetFileName(file);
                if (!IgnoredFiles.Contains(fileName))
                    sb.AppendLine($"{indent}  {fileName}");
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                PrintDirectory(dir, indent + "  ", sb);
            }
        }
    }
}
