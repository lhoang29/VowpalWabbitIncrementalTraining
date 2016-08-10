using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace VowpalWabbitIncrementalTraining
{
    public class Trainer
    {
        public static async Task InvokeBatchExecutionService(string inputTrainFile, string inputScoreFile, string outputEvaluationBlobName, string outputPredictionBlobName)
        {
            // How this works:
            //
            // 1. Assume the input is present in a local file (if the web service accepts input)
            // 2. Upload the file to an Azure blob - you'd need an Azure storage account
            // 3. Call the Batch Execution Service to process the data in the blob. Any output is written to Azure blobs.
            // 4. Download the output blob, if any, to local file

            // set a time out for polling status
            const int TimeOutInMilliseconds = 120 * 1000; // Set a timeout of 2 minutes

            Helper.UploadFileToBlob(inputTrainFile, inputTrainFile, Configuration.StorageContainerName, Configuration.StorageConnectionString);
            Helper.UploadFileToBlob(inputScoreFile, inputScoreFile, Configuration.StorageContainerName, Configuration.StorageConnectionString);

            using (HttpClient client = new HttpClient())
            {
                var request = new BatchExecutionRequest()
                {
                    Outputs = new Dictionary<string, AzureBlobDataReference>()
                    {
                        {
                            "model",
                            new AzureBlobDataReference()
                            {
                                ConnectionString = Configuration.StorageConnectionString,
                                RelativeLocation = string.Format("/{0}/{1}", Configuration.StorageContainerName, Configuration.OutputModelBlobName)
                            }
                        },
                        {
                            "evaluation",
                            new AzureBlobDataReference()
                            {
                                ConnectionString = Configuration.StorageConnectionString,
                                RelativeLocation = string.Format("/{0}/{1}", Configuration.StorageContainerName, outputEvaluationBlobName)
                            }
                        },
                        {
                            "predictions",
                            new AzureBlobDataReference()
                            {
                                ConnectionString = Configuration.StorageConnectionString,
                                RelativeLocation = string.Format("/{0}/{1}", Configuration.StorageContainerName, outputPredictionBlobName)
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>() {
                        { "Path to container, directory or blob", Configuration.StorageContainerName + "/" + inputScoreFile },
                        { "Name of the input VW file", inputTrainFile },
                    }
                };

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Configuration.TrainerApiKey);

                // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)


                Console.WriteLine("Submitting the job...");

                // submit the job
                var response = await client.PostAsJsonAsync(Configuration.TrainerUrl + "?api-version=2.0", request);
                if (!response.IsSuccessStatusCode)
                {
                    await Helper.WriteFailedResponse(response);
                    return;
                }

                string jobId = await response.Content.ReadAsAsync<string>();
                Console.WriteLine(string.Format("Job ID: {0}", jobId));


                // start the job
                Console.WriteLine("Starting the job...");
                response = await client.PostAsync(Configuration.TrainerUrl + "/" + jobId + "/start?api-version=2.0", null);
                if (!response.IsSuccessStatusCode)
                {
                    await Helper.WriteFailedResponse(response);
                    return;
                }

                string jobLocation = Configuration.TrainerUrl + "/" + jobId + "?api-version=2.0";
                Stopwatch watch = Stopwatch.StartNew();
                bool done = false;
                while (!done)
                {
                    Console.WriteLine("Checking the job status...");
                    response = await client.GetAsync(jobLocation);
                    if (!response.IsSuccessStatusCode)
                    {
                        await Helper.WriteFailedResponse(response);
                        return;
                    }

                    BatchScoreStatus status = await response.Content.ReadAsAsync<BatchScoreStatus>();
                    if (watch.ElapsedMilliseconds > TimeOutInMilliseconds)
                    {
                        done = true;
                        Console.WriteLine(string.Format("Timed out. Deleting job {0} ...", jobId));
                        await client.DeleteAsync(jobLocation);
                    }
                    switch (status.StatusCode)
                    {
                        case BatchScoreStatusCode.NotStarted:
                            Console.WriteLine(string.Format("Job {0} not yet started...", jobId));
                            break;
                        case BatchScoreStatusCode.Running:
                            Console.WriteLine(string.Format("Job {0} running...", jobId));
                            break;
                        case BatchScoreStatusCode.Failed:
                            Console.WriteLine(string.Format("Job {0} failed!", jobId));
                            Console.WriteLine(string.Format("Error details: {0}", status.Details));
                            done = true;
                            break;
                        case BatchScoreStatusCode.Cancelled:
                            Console.WriteLine(string.Format("Job {0} cancelled!", jobId));
                            done = true;
                            break;
                        case BatchScoreStatusCode.Finished:
                            done = true;
                            Console.WriteLine(string.Format("Job {0} finished!", jobId));

                            ProcessResults(status);
                            break;
                    }

                    if (!done)
                    {
                        Thread.Sleep(1000); // Wait one second
                    }
                }
            }
        }

        public static void ProcessResults(BatchScoreStatus status)
        {
            foreach (var output in status.Results)
            {
                var blobLocation = output.Value;
                Console.WriteLine(string.Format("The result '{0}' is available at the following Azure Storage location:", output.Key));
                Console.WriteLine(string.Format("BaseLocation: {0}", blobLocation.BaseLocation));
                Console.WriteLine(string.Format("RelativeLocation: {0}", blobLocation.RelativeLocation));
                Console.WriteLine(string.Format("SasBlobToken: {0}", blobLocation.SasBlobToken));
                Console.WriteLine();
            }
        }
    }
}

