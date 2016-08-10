using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VowpalWabbitIncrementalTraining
{
    public class ModelUpdater
    {
        public static async Task InvokeService()
        {
            var resourceLocations = new ResourceLocations()
            {
                Resources = new ResourceLocation[] {
                    new ResourceLocation()
                    {
                        Name = "Trained model (saved from Train Vowpal Wabbit Version 8 Model)",
                        Location = new AzureBlobDataReference()
                        {
                            BaseLocation = Configuration.ModelUrlBaseLocation,
                            RelativeLocation = Configuration.ModelUrlRelativeLocation,
                            SasBlobToken = Configuration.ModelUrlSasBlobToken
                        }
                    }
                }
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Configuration.ModelUpdateApiKey);
                using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), Configuration.ModelUpdateUrl))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(resourceLocations), System.Text.Encoding.UTF8, "application/json");

                    // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
                    // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
                    // For instance, replace code such as:
                    //      result = await DoSomeTask()
                    // with the following:
                    //      result = await DoSomeTask().ConfigureAwait(false)
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Result: {0}", result);
                    }
                    else
                    {
                        Console.WriteLine("Failed with status code: {0}", response.StatusCode);
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseContent);
                    }
                }
            }
        }
    }
}

