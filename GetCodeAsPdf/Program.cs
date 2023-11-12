using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using System.Text;

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

            StringBuilder sb = new();
            string[] fileExtensions = { "*.cs", "*.cshtml", "*.js", "*.css" }; // Add more if needed

            foreach (string ext in fileExtensions)
            {
                foreach (string file in Directory.EnumerateFiles(currentDirectory, ext, SearchOption.AllDirectories))
                {
                    sb.AppendLine($"File: {file}");
                    sb.AppendLine(File.ReadAllText(file));
                    sb.AppendLine("\n\n"); // Adding some space between files
                }
            }

            CreatePdf(sb.ToString(), outputFile);
        }

        static void CreatePdf(string content, string outputPath)
        {
            Document document = new();
            Section section = document.AddSection();
            Paragraph paragraph = section.AddParagraph();
            paragraph.Format.Font.Name = "Arial";
            paragraph.Format.Font.Size = 10;
            paragraph.AddText(content);

            PdfDocumentRenderer renderer = new()
            {
                Document = document
            };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(outputPath);

            Console.WriteLine($"PDF saved to {outputPath}");
        }
    }
}
