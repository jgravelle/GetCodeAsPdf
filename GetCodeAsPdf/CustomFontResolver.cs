using PdfSharp.Fonts;
using System;
using System.IO;
using System.Reflection;

namespace GetCodeAsPdf
{
    public class CustomFontResolver : IFontResolver
    {
        public static string DefaultFontName => "Arial";

        public byte[] GetFont(string faceName)
        {
            switch (faceName)
            {
                case "Arial":
                    return LoadFontData("GetCodeAsPdf.arial.ttf"); // Replace with your actual namespace
                default:
#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo("Arial");
        }

        private static byte[] LoadFontData(string name)
        {
            using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name) ?? throw new FileNotFoundException($"Resource not found: {name}");
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            return data;
        }
    }
}
