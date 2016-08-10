﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace VowpalWabbitIncrementalTraining
{
    public static class Configuration
    {
        private static string modelUrlSasBlobToken;

        public const string InputScoreBlobName = "inputdatablob.csv";
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

        public static string ModelUrlSasBlobToken;
        public static string ModelUrlBaseLocation;
        public static string ModelUrlRelativeLocation;

        public static string StorageConnectionString
        {
            get
            {
                return string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);
            }
        }
    }
}
