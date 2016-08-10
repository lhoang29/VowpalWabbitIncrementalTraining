using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace VowpalWabbitIncrementalTraining
{
    public static class Configuration
    {
        private static string modelUrlSasBlobToken;

        public const string InputDataBlobName = "inputdatablob.csv";
        public const string OutputModelBlobName = "modelresults.ilearner";

        public static string StorageAccountName;
        public static string StorageAccountKey;
        public static string StorageContainerName;

        public static string ScorerUrl;
        public static string ScorerApiKey;

        public static string TrainerUrl;
        public static string TrainerApiKey;

        public static string ModelUpdateUrl;
        public static string ModelUpdateApiKey;

        public static string ModelUrlSasBlobToken
        {
            get
            {
                if (!string.IsNullOrEmpty(modelUrlSasBlobToken))
                {
                    return modelUrlSasBlobToken;
                }
                var sasPolicy = new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-1),
                    SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1)
                };
                var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(StorageContainerName);
                var modelBlob = blobContainer.GetBlockBlobReference(OutputModelBlobName);
                var blobTokenUri = modelBlob.GetSharedAccessSignature(sasPolicy);
                modelUrlSasBlobToken = blobTokenUri.Substring(blobTokenUri.IndexOf('?'));

                Console.WriteLine("Generated SAS Token for model blob: {0}", modelUrlSasBlobToken);

                return modelUrlSasBlobToken;
            }
            set
            {
                modelUrlSasBlobToken = value;
            }
        }

        public static string ModelUrlBaseLocation
        {
            get
            {
                return string.Format("http://{0}.blob.core.windows.net/", StorageAccountName);
            }
        }

        public static string ModelUrlRelativeLocation
        {
            get
            {
                return string.Format("{0}/modelresults.ilearner", StorageContainerName);
            }
        }


        public static string StorageConnectionString
        {
            get
            {
                return string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);
            }
        }
    }
}
