#r "Microsoft.WindowsAzure.Storage"

using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

public class AzureBlobDataReference
{
    // Storage connection string used for regular blobs. It has the following format:
    // DefaultEndpointsProtocol=https;AccountName=ACCOUNT_NAME;AccountKey=ACCOUNT_KEY
    // It's not used for shared access signature blobs.
    public string ConnectionString { get; set; }

    // Relative uri for the blob, used for regular blobs as well as shared access 
    // signature blobs.
    public string RelativeLocation { get; set; }

    // Base url, only used for shared access signature blobs.
    public string BaseLocation { get; set; }

    // Shared access signature, only used for shared access signature blobs.
    public string SasBlobToken { get; set; }
}

public enum BatchScoreStatusCode
{
    NotStarted,
    Running,
    Failed,
    Cancelled,
    Finished
}

public class BatchScoreStatus
{
    // Status code for the batch scoring job
    public BatchScoreStatusCode StatusCode { get; set; }


    // Locations for the potential multiple batch scoring outputs
    public IDictionary<string, AzureBlobDataReference> Results { get; set; }

    // Error details, if any
    public string Details { get; set; }
}

public class BatchExecutionRequest
{

    public IDictionary<string, AzureBlobDataReference> Inputs { get; set; }
    public IDictionary<string, string> GlobalParameters { get; set; }

    // Locations for the potential multiple batch scoring outputs
    public IDictionary<string, AzureBlobDataReference> Outputs { get; set; }
}

static async Task WriteFailedResponse(HttpResponseMessage response, TraceWriter log)
{
    log.Info(string.Format("The request failed with status code: {0}", response.StatusCode));

    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
    log.Info(response.Headers.ToString());

    string responseContent = await response.Content.ReadAsStringAsync();
    log.Info(responseContent);
}

static async Task RunBatch(TraceWriter log)
{
    string storageAccountName = "dwmduelstorage";
    string storageAccountKey = "WidIAN4brvj0ZZR09/vkSfeg3RjArVk55iPi1iZsXcFn/nVmt3D3peknJtHzPkYHETWlBnYlu4zc9Ky069bxOQ==";
    string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", storageAccountName, storageAccountKey);
    string storageContainerName = "input";
    string storageOutputContainerName = "output";

    using (HttpClient client = new HttpClient())
    {
        var request = new BatchExecutionRequest()
        {
            Inputs = new Dictionary<string, AzureBlobDataReference>()
            {
                {
                    "input1",
                    new AzureBlobDataReference()
                    {
                        ConnectionString = storageConnectionString,
                        RelativeLocation = string.Format("{0}/game_data_utf_8.csv", storageContainerName)
                    }
                },
            },

            Outputs = new Dictionary<string, AzureBlobDataReference>()
            {

                {
                    "output1",
                    new AzureBlobDataReference()
                    {
                        ConnectionString = storageConnectionString,
                        RelativeLocation = string.Format("/{0}/output1results.csv", storageOutputContainerName)
                    }
                },
            },
            GlobalParameters = new Dictionary<string, string>() { }
        };

        string apiKey = "cAxLThKeh+r+0k7C2Ev3EXRDCNqnaugKa1+8kPo8KatPhCUckJsoEeExQc3DPvz51fQGS6JFwFQMKj4Qdku1BA==";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
        // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
        // For instance, replace code such as:
        //      result = await DoSomeTask()
        // with the following:
        //      result = await DoSomeTask().ConfigureAwait(false)

        log.Info("Submitting the job...");

        // submit the job
        string BaseUrl = "https://ussouthcentral.services.azureml.net/workspaces/4e97d2a3fa7b405c8ba4dcf38cde3306/services/abbaf09c68c74a81b36f239a3ffd215b/jobs";
        var response = await client.PostAsJsonAsync(BaseUrl + "?api-version=2.0", request);
        if (!response.IsSuccessStatusCode)
        {
            await WriteFailedResponse(response, log);
            return;
        }

        string jobId = await response.Content.ReadAsAsync<string>();
        log.Info(string.Format("Job ID: {0}", jobId));

        // start the job
        Console.WriteLine("Starting the job...");
        response = await client.PostAsync(BaseUrl + "/" + jobId + "/start?api-version=2.0", null);
        if (!response.IsSuccessStatusCode)
        {
            await WriteFailedResponse(response, log);
            return;
        }
    }
}

public static void Run(string myBlob, TraceWriter log)
{
    //log.Info($"C# Blob trigger function processed: {myBlob}");

    RunBatch(log).Wait();
}