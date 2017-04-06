using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using PdfConversion;

namespace Stateless1
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                ServiceRuntime.RegisterServiceAsync("Stateless1Type",
                    context => new Stateless1(context)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(Stateless1).Name);

                //// Prevents this host process from terminating so services keep running.

               // UploadText();
                ProcessPdf("testHtm2l.html");

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        public static void UploadText()
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=iconingestmsgstoredev;AccountKey=+3gc0hIrMBDE9l8uJARYjlCZewppkXgoqcGSFVTRcjmSVRwbV6eXtkMFPDzTQK8A/Ksky9t6DzIh/RVPG9RKgw==";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("pdfgeneration");
            container.CreateIfNotExists();
            var pdfBlobName = "test.txt";
            CloudBlockBlob pdfBlockBlob = container.GetBlockBlobReference(pdfBlobName);
            pdfBlockBlob.Properties.ContentType = "application/pdf";
            var reportHtml = "test case";
            pdfBlockBlob.UploadText(reportHtml);
        }

        public static void ProcessPdf(string htmlBlobName)
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

            //var reportHtml = File.ReadAllText(@"Resource\Test.html");
            var reportHtml = inputblockBlob.DownloadText();

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
