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

            if (solutionFile == null)
            {
                Console.WriteLine("No .sln file found in the current directory.");
                return;
            }

            string solutionName = Path.GetFileNameWithoutExtension(solutionFile);
            string outputFile = Path.Combine(currentDirectory, $"{solutionName}.pdf");

            Document document = new();
            DefineStyles(document);
            Section section = document.AddSection();
            CreateTableOfContents(section);

            string[] fileExtensions = { "*.cs", "*.cshtml", "*.js", "*.css", "*.csproj", "*.json" }; // Extend as needed
            string[] excludeDirectories = { "docs", "obj", "bin", "lib", "node_modules", "Migrations", ".vs", "Properties" }; // Directories to exclude

            foreach (string ext in fileExtensions)
            {
                foreach (string file in Directory.EnumerateFiles(currentDirectory, ext, SearchOption.AllDirectories))
                {
                    if (ShouldExcludeFile(file, excludeDirectories))
                    {
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

                        section.AddParagraph("\n\n"); // Adding some space between files
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

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        static void DefineStyles(Document document)
        {
            Style style = document.Styles["Normal"];
            style.Font.Name = "Arial";


            style = document.Styles.AddStyle("CodeStyle", "Normal");
            style.Font.Name = "Courier New";
            style.Font.Size = 10;

            style = document.Styles["Heading1"];
            style.Font.Size = 14;
            style.Font.Bold = true;
        }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

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
            }
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
