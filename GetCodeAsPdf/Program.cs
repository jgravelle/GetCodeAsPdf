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

            Console.WriteLine("Strap in.  This can take a while...");

            string solutionName = Path.GetFileNameWithoutExtension(solutionFile);
            string outputFile = Path.Combine(currentDirectory, $"{solutionName}.pdf");

            StringBuilder sb = new();
            string[] fileExtensions = { "*.cs", "*.cshtml", "*.js", "*.css" }; // Add more if needed

            foreach (string ext in fileExtensions)
            {
                foreach (string file in Directory.EnumerateFiles(currentDirectory, ext, SearchOption.AllDirectories))
                {
                    if (file.Contains("bootstrap") || file.Contains("jquery"))
                    {
                        continue;
                    }

                    Console.WriteLine("Processing " + file);
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

            // Break content into smaller chunks if necessary
            const int chunkSize = 5000;  // Adjust as needed
            int contentLength = content.Length;
            int startIndex = 0;

            while (startIndex < contentLength)
            {
                int length = Math.Min(chunkSize, contentLength - startIndex);
                string chunk = content.Substring(startIndex, length);

                Paragraph paragraph = section.AddParagraph();
                paragraph.Format.Font.Name = "Arial";
                paragraph.Format.Font.Size = 10;
                paragraph.AddText(chunk);

                startIndex += length;

                // Optionally add a new section for each chunk
                // section = document.AddSection();
            }

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
