using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using System.IO;
using PdfSharp.Fonts;

namespace simposio.Services.PDF
{
    public class PDFEditor
    {

        private readonly IWebHostEnvironment _environment;

        public PDFEditor(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public void AddNameToCertificateAndSave(string userName)
        {
            
            string pdfTemplatePath = Path.Combine(_environment.ContentRootPath, "Resources", "Images", "certificado.pdf");
            Console.WriteLine($"Intentando abrir el archivo PDF en: {pdfTemplatePath}");
            string outputPath = Path.Combine(_environment.ContentRootPath, "Resources", "Images", "cm.pdf");

            // Cargar el PDF existente
            PdfDocument document = PdfReader.Open(pdfTemplatePath, PdfDocumentOpenMode.Modify);
            PdfPage page = document.Pages[0];

            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Roboto", 30, XFontStyleEx.Bold);

            // Escribir el nombre en el PDF
            gfx.DrawString(userName, font, XBrushes.Black, new XRect(0, 295, page.Width, page.Height), XStringFormats.TopCenter);

            // Guardar el documento modificado en el disco
            using (document)
            {
                document.Save(outputPath);
            }
        }

    }



    public class FontResolver : IFontResolver
    {
        string ContentRootPath { get; set; }

        public FontResolver(string _contentRootPath)
        {
            ContentRootPath = _contentRootPath;
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Asignar a la fuente según el nombre y los estilos
            string name = familyName.ToLower();
            switch (name)
            {
                case "roboto":
                    if (isBold)
                        return new FontResolverInfo("roboto#b");
                    return new FontResolverInfo("roboto#");
            }
            return null; // Retorna null para fuentes no encontradas
        }

        public byte[] GetFont(string faceName)
        {
            // Cargar el archivo de fuente desde un recurso incrustado o un archivo
            switch (faceName)
            {
                case "roboto#":
                    return File.ReadAllBytes(Path.Combine(ContentRootPath, "Resources", "Fonts", "Roboto-Regular.ttf"));
                case "roboto#b":
                    return File.ReadAllBytes(Path.Combine(ContentRootPath, "Resources", "Fonts", "Roboto-Bold.ttf"));
            }
            return null;
        }
    }
}
