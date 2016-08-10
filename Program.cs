using System;
using System.Threading;

namespace VowpalWabbitIncrementalTraining
{
    class Program
    {
        static void Configure()
        {
            Configuration.StorageAccountName = ""; // Replace this with your Azure Storage Account name
            Configuration.StorageAccountKey = ""; // Replace this with your Azure Storage Key
            Configuration.StorageContainerName = ""; // Replace this with your Azure Storage Container name

            Configuration.TrainerUrl = ""; // Replace this with the URL of Training web service
            Configuration.TrainerApiKey = ""; // Replace this with the API key for the web service

            Configuration.ScorerUrl = ""; // Replace this with the URL of Scoring web service
            Configuration.ScorerApiKey = ""; // Replace this with the API key for the web service

            Configuration.ModelUpdateUrl = ""; // Replace this with the URL of Update Resource endpoint
            Configuration.ModelUpdateApiKey = ""; // Replace this with the API key for the endpoint
        }

        static void Main(string[] args)
        {
            Configure();

            // Workflow is similar to here: https://azure.microsoft.com/en-us/documentation/articles/machine-learning-retrain-models-programmatically/

            // Trigger Training Web Service
            Trainer.InvokeBatchExecutionService(
                inputTrainFile: "sample-5-train.vw", // train file
                inputScoreFile: "sample-5-test.vw", // test file
                outputEvaluationBlobName: "train-eval.csv", // output evaluation file
                outputPredictionBlobName: "train-pred.csv") // output prediction file
            .Wait();

            // Update Model
            ModelUpdater.InvokeService().Wait();

            Thread.Sleep(TimeSpan.FromMinutes(2));

            // Trigger Scoring Web Service
            Scorer.InvokeBatchExecutionService(
                inputDataFile: "sample-5-test.vw", // input data file for scoring
                outputEvaluationBlobName: "test-eval.csv", // output evaluation file
                outputPredictionBlobName: "test-pred.csv") // output prediction file
            .Wait();
        }
    }
}
