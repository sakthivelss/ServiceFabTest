using PdfConversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AcbPdfConverter
{
    class Program
    {
        static void Main(string[] args)
        {

            Process("testHtm2l.html");
        }

        public static void Process(string htmlBlobName)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=iconingestmsgstoredev;AccountKey=+3gc0hIrMBDE9l8uJARYjlCZewppkXgoqcGSFVTRcjmSVRwbV6eXtkMFPDzTQK8A/Ksky9t6DzIh/RVPG9RKgw==";


            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("pdfgeneration");
            container.CreateIfNotExists();

            var pdfBlobName = Path.GetFileName(htmlBlobName) + ".pdf";
            CloudBlockBlob inputblockBlob = container.GetBlockBlobReference(htmlBlobName);
            CloudBlockBlob pdfBlockBlob = container.GetBlockBlobReference(pdfBlobName);
            pdfBlockBlob.Properties.ContentType = "application/pdf";

            var reportHtml = File.ReadAllText(@"Resource\Test.html");
            //var reportHtml = blockBlob.DownloadText();

            using (var memStream = new MemoryStream())
            {
                var pdfUtil = new PdfUtil();
                pdfUtil.WebURl = @"Resource";
                pdfUtil.GeneratePdf(reportHtml, memStream).Wait();
                var bytes = memStream.ToArray();
                pdfBlockBlob.UploadFromByteArray(bytes, 0, bytes.Length);
                //pdfBlockBlob.UploadFromStream(memStream);
                //File.WriteAllBytes(@"C:\temp\test.pdf", memStream.ToArray());
                memStream.Flush();

            }
        }
    }
}
