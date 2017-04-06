using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebSupergoo.ABCpdf10; // pagelayout
using WebSupergoo.ABCpdf10.Objects; // content 
using WebSupergoo.ABCpdf10.Atoms; // low level pdf structures
using WebSupergoo.ABCpdf10.Operations; //
using System.IO;

namespace PdfConversion
{
    public class PdfUtil
    {
        public string WebURl = @"Content\Images";
        public bool removeBlankPages = false;
        public string copyrightMessage = string.Empty;
        public bool iscancelledReport = false;
        public string waterMarkText = string.Empty;
        public bool isRotateWaterMarkText = false;


       

        public static void GeneratePdf()
        {
            Doc theDoc = new Doc();
            theDoc.FontSize = 96;

            theDoc.HtmlOptions.UseActiveX = true;
            theDoc.HtmlOptions.AutoTruncate = true;

            theDoc.HtmlOptions.Engine = EngineType.Gecko;
            theDoc.HtmlOptions.UseScript = true;
            theDoc.HtmlOptions.AddLinks = true;
            theDoc.HtmlOptions.AdjustLayout = false;
            


            theDoc.AddImageUrl(@"http://localhost:5095/app/index.html#/reports");
            //  theDoc.AddImageUrl(@"http://localhost:5095/app/components/reports/ReportOutput.html");

            
            theDoc.Save(@"C:\temp\testPdf.pdf");
        }

        public async Task GeneratePdf(string htmlContent, Stream outputStream)
        {

           

            string pageNumberFormat = string.Empty;
            int startingPageNumber = 1;

            string tempPath = Path.GetTempPath();

            string htmlPath = PdfHelper.CreateTempPDFHtml(htmlContent, tempPath);

            var pdfGenerator = new PdfHelper(WebURl);
            pdfGenerator.FormatPDFHtmlOptions();

           
            var frontPageSection = pdfGenerator.CreateFrontPageSection(htmlPath);
            frontPageSection.Wait();

            if (removeBlankPages)
            {
                await Task.Run(() => pdfGenerator.RemoveBlankPages());
            }

            //var headerSection = pdfGenerator.FormatHeaderSection(startingPageNumber , "", "", "",  false);
            //headerSection.Wait();

            //var footerSection = pdfGenerator.FormatFooterSection(pageNumberFormat, startingPageNumber, copyrightMessage);
            //footerSection.Wait();

            //if (iscancelledReport)
            //{
            //    var waterMarkSection = pdfGenerator.WaterMarkText(waterMarkText, isRotateWaterMarkText);
            //    //watermark method
            //    waterMarkSection.Wait();
            //}
          pdfGenerator.theDoc.Save(outputStream);
        //  pdfGenerator.theDoc.Save(@"C:\temp\test.pdf");

        }

    }
}
