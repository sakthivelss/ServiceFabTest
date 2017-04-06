using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
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

            while (true)
            {

            }

        }
    }
}
