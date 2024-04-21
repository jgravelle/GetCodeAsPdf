using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Fonts;

namespace GetCodeAsPdf
{
    class Program
    {
        static void Main()
        {
            GlobalFontSettings.FontResolver = new CustomFontResolver();

            string currentDirectory = Directory.GetCurrentDirectory();
            string? solutionFile = Directory.GetFiles(currentDirectory, "*.sln").FirstOrDefault();

            string projectName;
            if (solutionFile != null)
            {
                projectName = Path.GetFileNameWithoutExtension(solutionFile);
            }
            else
            {
                projectName = new DirectoryInfo(currentDirectory).Name;
            }

            string outputFile = Path.Combine(currentDirectory, $"{projectName}.pdf");

            Document document = new();
            DefineStyles(document);
            Section section = document.AddSection();
            CreateTableOfContents(section);

            string[] fileExtensions = { "*.cs", "*.cshtml", "*.js", "*.css", "*.csproj", "*.json", "*.py" };
            string[] excludeDirectories = { "docs", "obj", "bin", "lib", "node_modules", "Migrations", ".vs", "Properties", "__pycache__", "venv" };

            foreach (string ext in fileExtensions)
            {
                foreach (string file in Directory.EnumerateFiles(currentDirectory, ext, SearchOption.AllDirectories))
                {
                    if (ShouldExcludeFile(file, excludeDirectories))
                    {
                        continue;
                    }

                    if (!IsFileContentSuitableForPdf(file))
                    {
                        Console.WriteLine($"Skipping file due to unsuitable content: {file}");
                        continue;
                    }

                    string relativeFilePath = file[(currentDirectory.Length + 1)..];
                    Console.WriteLine("Processing " + relativeFilePath);

                    try
                    {
                        string fileContent = File.ReadAllText(file);
                        if (string.IsNullOrWhiteSpace(fileContent))
                        {
                            Console.WriteLine($"Warning: The file '{relativeFilePath}' is empty or whitespace.");
                            continue;
                        }

                        Paragraph para = section.AddParagraph();
                        para.Style = "Heading1";
                        para.AddBookmark(relativeFilePath);
                        para.AddFormattedText(relativeFilePath, TextFormat.Bold);
                        Console.WriteLine($"Added heading for {relativeFilePath}");

                        section.AddParagraph(fileContent, "CodeStyle");
                        Console.WriteLine($"Added content for {relativeFilePath}");

                        section.AddParagraph("\n\n");
                        Console.WriteLine($"Added spacing after {relativeFilePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing file '{relativeFilePath}': {ex.Message}");
                    }
                }
            }

            CreatePdf(document, outputFile);
        }

        static void DefineStyles(Document document)
        {
            Style style = document.Styles["Normal"] ?? document.Styles.AddStyle("Normal", "Normal");
            style.Font.Name = "Arial";

            style = document.Styles.AddStyle("CodeStyle", "Normal");
            style.Font.Name = "Courier New";
            style.Font.Size = 10;

            style = document.Styles["Heading1"] ?? document.Styles.AddStyle("Heading1", "Normal");
            style.Font.Size = 14;
            style.Font.Bold = true;
        }

        static void CreateTableOfContents(Section section)
        {
            Paragraph paragraph = section.AddParagraph("Table of Contents");
            paragraph.Format.Font.Size = 14;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.SpaceAfter = 24;

            paragraph = section.AddParagraph();
            paragraph.Style = "TOC";
        }

        static void CreatePdf(Document document, string outputPath)
        {
            try
            {
                PdfDocumentRenderer renderer = new()
                {
                    Document = document
                };
                Console.WriteLine("Starting document rendering...");
                renderer.RenderDocument();
                renderer.PdfDocument.Save(outputPath);
                Console.WriteLine($"PDF saved to {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating PDF: " + ex.ToString());
                throw; // Rethrow the exception to preserve the original stack trace
            }
        }

        private static bool IsFileContentSuitableForPdf(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                // Check for overly long lines
                if (line.Length > 1000) // Adjust the threshold as needed
                {
                    Console.WriteLine($"Warning: Long line detected in {filePath}");
                    return false;
                }

                // Add other checks as needed, e.g., special characters, formatting issues, etc.
            }
            return true;
        }

        private static bool ShouldExcludeFile(string filePath, string[] excludeDirectories)
        {
            if (excludeDirectories.Any(dir => filePath.Contains(Path.DirectorySeparatorChar + dir + Path.DirectorySeparatorChar)))
            {
                return true;
            }

            string[] firstLines = File.ReadLines(filePath).Take(5).ToArray();
            if (firstLines.Any(line => line.Contains("Auto-generated")))
            {
                return true;
            }

            return false;
        }
    }
}
