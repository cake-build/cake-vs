#addin "System.Net.Http"
using System.Net.Http;

public class MyGetClient : HttpClient 
{

    public string ApiKey { get; set; }
    public Uri FeedUri { get; set; }
    private Action<string> Log { get; set; }

    public static MyGetClient GetClient(string uri, string key)
    {
        return new MyGetClient
        {
            FeedUri = uri.TrimEnd('/').EndsWith("/upload")
            ? new Uri(uri)
            : new Uri(uri.TrimEnd('/') + "/upload"),
            ApiKey = key
        };
    }

    public static MyGetClient GetClient(string uri, string key, Action<string> log) {
        Log = log;
        return GetClient(uri, key);
    }

    public static MyGetClient GetClient(MyGetFeed feed) 
    {
        return MyGetClient.GetClient(feed.Url, feed.Key);
    }

    public HttpResponseMessage UploadVsix(IFile file)
    {
        Log = Log ?? s => { };
        using (var content = new MultipartFormDataContent())
        {
            Log.Invoke("Preparing API request");
            content.Add(new StreamContent(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read)));
            DefaultRequestHeaders.Add("X-NuGet-ApiKey", ApiKey);
            Log.Invoke(string.Format("Issuing POST request to {0}", FeedUri));
            using (var message =
                   PostAsync(FeedUri, content).Result)
            {
                return message;
            }
        }
    }
}

public class MyGetFeed 
{
    public string Url { get; private set; }
    public string Key { get; private set; }

    public MyGetFeed(string feedUrl, string apiKey) 
    {
        Url = feedUrl;
        Key = apiKey;
    }
}

public IFile GetFile(FilePath path) {
    return Context.FileSystem.GetFile(path);
}