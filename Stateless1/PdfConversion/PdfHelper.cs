using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSupergoo.ABCpdf10;
using WebSupergoo.ABCpdf10.Objects;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PdfConversion
{



    public class PdfHelper
    {
        public string BackgroundColor ="Blue";
        public string BorderColor="Black";
     
        private const string LogoImage = "lennoxlogo-120px.png";
        private const string HeaderImage = "pdfHeaderStrip.png";

        private string LogoUrl = string.Empty;


        public Doc theDoc;
        private string FolderPath;
        public void FormatPDFHtmlOptions()
        {
            theDoc.HtmlOptions.Engine = EngineType.Gecko;
            theDoc.HtmlOptions.UseScript = true;
            //theDoc.HtmlOptions.OnLoadScript = "(function(){ window.ABCpdf_go = false; setTimeout(function(){ window.ABCpdf_go = true; }, 1000); })();";
            theDoc.HtmlOptions.ImageQuality = 33;
            theDoc.HtmlOptions.Timeout = 300000;
            theDoc.HtmlOptions.BrowserWidth = 1500;
            theDoc.HtmlOptions.AddLinks = true;
            // theDoc.HtmlOptions.BreakMethod = HtmlBreakMethodType.NoOptimization;
            //  theDoc.HtmlOptions.BreakZoneSize = 400;

            // header footer custom font

            theDoc.HtmlOptions.FontEmbed = true;
            theDoc.HtmlOptions.FontSubstitute = true;
            theDoc.HtmlOptions.FontProtection = false;
            theDoc.HtmlOptions.FontSubset = false;
            theDoc.Font = theDoc.AddFont("Arial");
        }

        public PdfHelper(string webUrl)
        {
            theDoc = new Doc();
            String path = Path.GetTempPath();
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            FolderPath = path;
            LogoUrl = Path.Combine(webUrl, LogoImage);

        }

        public string ImageResize(string pathString, int Width, int Height, string webPath = null)
        {
            Image imgPhoto = Image.FromFile(pathString);
            Guid guid = Guid.NewGuid();
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            if (imgPhoto.Width <= Width && imgPhoto.Height <= Height)
            {
                return pathString;
            }
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float divPercent = 0;
            float divPercentW = 0;
            float divPercentH = 0;

            divPercentW = ((float)Width / (float)sourceWidth);
            divPercentH = ((float)Height / (float)sourceHeight);
            if (divPercentH < divPercentW)
            {
                divPercent = divPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * divPercent)) / 2);
            }
            else
            {
                divPercent = divPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * divPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * divPercent);
            int destHeight = (int)(sourceHeight * divPercent);

            Bitmap bitmapPhoto = new Bitmap(Width, Height,
                              PixelFormat.Format24bppRgb);
            bitmapPhoto.SetResolution(imgPhoto.HorizontalResolution,
                             imgPhoto.VerticalResolution);

            Graphics graphPhoto = Graphics.FromImage(bitmapPhoto);
            graphPhoto.Clear(Color.White);
            graphPhoto.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

            graphPhoto.DrawImage(imgPhoto,
                new Rectangle(0, 0, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            graphPhoto.Dispose();
            string path = string.Empty;

            if (webPath == null)
            {
                path = this.CreateFilePathIfNotExists( guid.ToString());
            }
            else
            {
                path = this.CreateFilePathIfNotExists( guid.ToString());
            }
            using (var newImage = bitmapPhoto)
            {
                newImage.Save(path);
            }

            return path;
        }

        public async Task RemoveBlankPages()
        {
            //ImageOperation op = null;
            for (int i = theDoc.PageCount; i > 0; i--)
            {
                theDoc.PageNumber = i;

                //get the pdf content
                //op = new ImageOperation(theDoc);
                //op.PageContents.AddPages();
                //var imageCollection = op.GetImageProperties();
                string textContent = theDoc.GetText("Text");

                //delete the page if it is blank
                //&& imageCollection.Count == 0)
                if (string.IsNullOrEmpty(textContent))
                {
                    theDoc.Delete(theDoc.Page);
                }
            }
            await Task.FromResult(0);
            return;
        }

        public async Task WaterMarkText(string WaterMarkText, bool IsRotate = false)
        {
            theDoc.TextStyle.HPos = 0;
            theDoc.TextStyle.VPos = 0;
            theDoc.Rect.SetRect(250, 265, 500, 80);
            if (IsRotate)
            {
                theDoc.Transform.Rotate(55, 250, 265);
            }
            theDoc.FontSize = 80;
            theDoc.Color.String = "0 0 0 a70";
            Page[] pages = theDoc.ObjectSoup.Catalog.Pages.GetPageArray();
            foreach (Page p in pages)
            {
                theDoc.Page = p.ID;
                theDoc.AddText(WaterMarkText);
            }

            await Task.FromResult(0);
        }
        private bool disposedValue = false; // To detect redundant calls

        public static string LogoImage1
        {
            get
            {
                return LogoImage;
            }
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    theDoc.Dispose();
                }

                disposedValue = true;
            }
        }
      


        public static string CreateTempPDFHtml(string HtmlData, string outputFolder)
        {

            var HtmlValue = RemoveSpecialCharacters(HtmlData);
            Guid guid = Guid.NewGuid();
            string outputFile = Path.Combine(outputFolder, guid.ToString() + ".Html");
            System.IO.File.WriteAllText(outputFile, HtmlValue);
           
            return outputFile;
        }

        public static string RemoveSpecialCharacters(string PdfData)
        {
            var HtmlValue = PdfData;
            HtmlValue = HtmlValue.Replace("\\n", " ");
            HtmlValue = HtmlValue.Replace(@"\", "");
            HtmlValue = HtmlValue.Trim('"');
            HtmlValue = HtmlValue.Replace("”", "\"");
            HtmlValue = HtmlValue.Replace("“", "\"");
            return HtmlValue;
        }

        public static string CreatePDFfile(string fileServerPath, byte[] theData, out string filePath)
        {
            string fileName;
            string pathString;
            //string fileServerPath = FileHelper.GetFileServerPath();

            fileName = Guid.NewGuid().ToString();
            pathString = System.IO.Path.Combine(fileServerPath, fileName);
            filePath = pathString;
            FileStream fs = new FileStream(pathString, FileMode.Create);
            fs.Write(theData, 0, theData.Length);
            fs.Close();
            return fileName;
        }

        public async Task CreateFrontPageSection(string htmlPath)
        {
            try
            {
                int theID;

                theDoc.Rect.String = "40 70 570 700";
                theID = theDoc.AddImageUrl(htmlPath);

                theDoc.Color.String = "0 255 0";
                while (true)
                {
                    if (!theDoc.Chainable(theID))
                    {
                        theID = theDoc.AddImageToChain(theID);
                        break;
                    }
                    theDoc.Page = theDoc.AddPage();
                    theID = theDoc.AddImageToChain(theID);
                }
                await Task.FromResult(0);
                return;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void CreatePDFThambNail(string filename)
        {
            theDoc.Read(filename);
            theDoc.Rendering.DotsPerInch = 18;
            Page[] pages = theDoc.ObjectSoup.Catalog.Pages.GetPageArrayAll();
            theDoc.PageNumber = 1;
            theDoc.Rect.String = theDoc.CropBox.String;
            theDoc.Rendering.Save(filename + "thumb.png");

        }

        private string CreateFilePathIfNotExists(string relativeUrl)
        {
            return FolderPath + @"\"+ relativeUrl;
        }



        public async Task FormatHeaderSection(int startingPageNumber,  string firstHeaderField,
            string secondHeaderField, string thirdHeaderField,   
            bool isHeaderLogoRequired = false)
        {
            try
            {
                //theDoc.Rect.String = "36 770 700 800";
                //theDoc.TextStyle.HPos = 0.5;
                //theDoc.TextStyle.VPos = 0.5;
                //theDoc.Color.String = "0 255 0";

                theDoc.Rect.String = "39 732 0 0";
                theDoc.Color.String = "0 255 0";

                Color backgroundColour = ColorTranslator.FromHtml(BackgroundColor);
                Color borderColor = ColorTranslator.FromHtml(BorderColor);

                int headerID = 0;

                for (int page = startingPageNumber; page <= theDoc.PageCount; page++)
                {
                    theDoc.PageNumber = page;
                    theDoc.Flatten();
                    if (page == startingPageNumber)
                    {
                        theDoc.Rect.Top = 200;
                        if (isHeaderLogoRequired)
                        {
                            theDoc.TextStyle.HPos = 0;

                            headerID = theDoc.AddImageUrl(this.ImageResize(this.LogoUrl, 200, 40));

                            //if (logoURL != null)
                            //{
                               
                            //}
                            //else
                            //{
                            //    headerID = theDoc.AddImageUrl(WebURl + (string.IsNullOrWhiteSpace(logoURL) == true ? ImageURl : logoURL));
                            //}

                        }
                    }
                    else
                    {
                        theDoc.TextStyle.HPos = 0;
                        //headerID = theDoc.AddImageUrl(WebURl + (string.IsNullOrWhiteSpace(logoURL) == true ? ImageURl : logoURL));
                        XImage image = new XImage();
                        string tempImageUrl = "";
                        //if (logoURL != null)
                        //{

                        tempImageUrl = this.ImageResize(LogoUrl, 200, 40);

                        
                        //headerID = theDoc.AddImageUrl(this.ImageResize(logoURL, 200, 40));
                        //theDoc.Rect.Width = 200;
                        //theDoc.Rect.Height = 40;
                        image.SetFile(tempImageUrl);
                        theDoc.Rect.Width = image.Width;
                        theDoc.Rect.Height = image.Height;
                        theDoc.AddImageObject(image, true);
                        image.Clear();
                        //}
                        //else
                        //{
                        //    headerID = theDoc.AddImageUrl(WebURl + (string.IsNullOrWhiteSpace(logoURL) == true ? ImageURl : logoURL));
                        //}
                    }
                }

                theDoc.Rect.String = "36 735 866 920";
                theDoc.TextStyle.HPos = 0.5;
                theDoc.TextStyle.VPos = 0.5;

                for (int page = startingPageNumber; page <= theDoc.PageCount; page++)
                {
                    theDoc.PageNumber = page;
                    if (page == startingPageNumber)
                    {
                        theDoc.Rect.Top = 200;
                        if (isHeaderLogoRequired)
                        {
                            theDoc.TextStyle.HPos = 0;
                            //theDoc.AddImageUrl(WebURl + (string.IsNullOrWhiteSpace(headerURL) == true ? HeaderURl : headerURL));
                            if (BackgroundColor != null)
                            {
                                Rectangle rc = theDoc.MediaBox.Rectangle;
                                rc.Inflate(-40, -61);
                                rc.Height = 20;
                                rc.Width = 530;
                                theDoc.Rect.Rectangle = rc;
                                theDoc.Color.String = string.Format("{0} {1} {2} ", backgroundColour.R, backgroundColour.G, backgroundColour.B);
                                theDoc.FillRect();
                            }

                            if (BorderColor != null)
                            {
                                Rectangle rc1 = theDoc.MediaBox.Rectangle;
                                rc1.Inflate(-40, -81);
                                rc1.Height = 3;
                                rc1.Width = 530;
                                theDoc.Rect.Rectangle = rc1;
                                theDoc.Color.String = string.Format("{0} {1} {2} ", borderColor.R, borderColor.G, borderColor.B);
                                theDoc.FillRect();
                            }
                        }
                    }
                    else
                    {
                        //if (isHeaderLogoRequired)
                        //{
                        theDoc.TextStyle.HPos = 0;
                        //theDoc.AddImageUrl(WebURl + (string.IsNullOrWhiteSpace(headerURL) == true ? HeaderURl : headerURL));
                        if (BackgroundColor != null)
                        {
                            Rectangle rc = theDoc.MediaBox.Rectangle;
                            rc.Inflate(-40, -61);
                            rc.Height = 20;
                            rc.Width = 530;
                            theDoc.Rect.Rectangle = rc;
                            theDoc.Color.String = string.Format("{0} {1} {2} ", backgroundColour.R, backgroundColour.G, backgroundColour.B);
                            theDoc.FillRect();
                        }

                        if (BorderColor != null)
                        {
                            Rectangle rc1 = theDoc.MediaBox.Rectangle;
                            rc1.Inflate(-40, -81);
                            rc1.Height = 3;
                            rc1.Width = 530;
                            theDoc.Rect.Rectangle = rc1;
                            theDoc.Color.String = string.Format("{0} {1} {2} ", borderColor.R, borderColor.G, borderColor.B);
                            theDoc.FillRect();
                        }
                        //}
                    }
                }
                int previewOrderID = 0;
                if (previewOrderID != 0)
                {
                    string headerInfo = string.Empty;
                    headerInfo = firstHeaderField + "                  ";
                    if (!string.IsNullOrEmpty(secondHeaderField))
                    {
                        headerInfo = headerInfo + secondHeaderField + "                  ";
                    }
                    if (!string.IsNullOrEmpty(thirdHeaderField))
                    {
                        headerInfo = headerInfo + thirdHeaderField;
                    }

                    theDoc.Rect.String = "42 172 866 1270";
                    theDoc.FontSize = 8;
                    for (int page = 1; page <= theDoc.PageCount; page++)
                    {
                        theDoc.PageNumber = page;

                        //if (documentType == OrderConstants.DocumentType.Invoice)
                        //{
                        if (page != startingPageNumber || startingPageNumber == theDoc.PageCount)
                        {
                            theDoc.Color.String = "245 245 245";
                            theDoc.TextStyle.HPos = 0;
                            theDoc.AddText(headerInfo);
                        }
                        //}
                        //else
                        //{
                        //if (documentType == OrderConstants.DocumentType.Report)
                        //{
                        //    theDoc.Color.String = "255 0 0";
                        //    theDoc.TextStyle.HPos = 1.0;
                        //    theDoc.AddText(headerInfo);
                        //}
                        //}
                    }
                }

                await Task.FromResult(0);
                return;
                //await Task.Delay(1000);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task FormatFooterSection(string pageNumberString, int startingPageNumber, string copyrightMessage)
        {
            try
            {
                theDoc.Rect.String = "36 30 570 50";
                if (pageNumberString != null)
                {
                    string pageValues = pageNumberString;
                    for (int page = startingPageNumber; page <= theDoc.PageCount; page++)
                    {
                        //pageNumber.FormulaString = pageValues;
                        theDoc.PageNumber = page;
                       // if (page != startingPageNumber || theDoc.PageCount == startingPageNumber)
                        {
                            theDoc.Rendering.SaveAlpha = true;
                            Bitmap alphaBitmap = theDoc.Rendering.GetBitmap();
                            // Create a blue PDF

                            theDoc.Rect.String = "36 30 570 48";
                            theDoc.Color.String = "210 211 213";
                            // Add the transparent Bitmap into the PDF
                            // so that the underlying blue can show through
                            theDoc.AddImageBitmap(alphaBitmap, true);
                            theDoc.FillRect();
                            theDoc.Rect.String = "42 30 490 43";
                            theDoc.Color.String = "108 89 90";
                            //if (OrderConstants.DocumentType.Report == documentType)
                            //{
                            //    theDoc.FontSize = 8;
                            //    theDoc.AddText(copyrightMessage);
                            //}
                            theDoc.Rect.String = "515 30 565 43";
                            theDoc.Color.String = "108 89 90";

                            //if (OrderConstants.DocumentType.Report == documentType)
                            //{
                            //    // theDoc.Rect.String = "36 30 570 50";
                            //    theDoc.TextStyle.HPos = 0;
                            //    theDoc.TextStyle.VPos = 0;
                            //    theDoc.FontSize = 8;
                            //    pageNumber.FormulaString = pageNumber.FormulaString.ToLower().Replace("pageno", page.ToString());
                            //    pageNumber.FormulaString = pageNumber.FormulaString.ToLower().Replace("totalpages", theDoc.PageCount.ToString());
                            //    pageNumber.FormulaString = pageNumber.FormulaString.Replace("'", "");
                            //    pageNumber.FormulaString = pageNumber.FormulaString.Substring(0, 1).ToUpper() + pageNumber.FormulaString.Substring(1);

                            //    theDoc.AddText(pageNumber.FormulaString);
                            //}
                            //else
                            {
                                theDoc.Rect.String = "36 30 570 50";
                                theDoc.TextStyle.HPos = 0.5;
                                theDoc.TextStyle.VPos = 1.0;
                                theDoc.FontSize = 8;
                                string footerTextString = @"Thank you for your business!" + Environment.NewLine + " if you have any questions you may call Customer Service at 877-996-2620 or Cds.clearcs@firstam.com";
                                theDoc.AddText(footerTextString);
                            }
                        }
                    }
                }

                await Task.FromResult(0);
                return;
            }
            catch (Exception)
            {

                throw;
            }
        }




       

    }
}
