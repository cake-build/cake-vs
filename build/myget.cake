#addin "System.Net.Http"
using System.Net.Http;

public class MyGetClient : HttpClient 
{

    public string ApiKey { get; set; }
    public Uri FeedUri { get; set; }

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

    public static MyGetClient GetClient(MyGetFeed feed) 
    {
        return MyGetClient.GetClient(feed.Url, feed.Key);
    }

    public bool UploadVsix(IFile file)
    {
        using (var content = new MultipartFormDataContent())
        {
            content.Add(new StreamContent(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read)));
            DefaultRequestHeaders.Add("X-NuGet-ApiKey", ApiKey);
            using (var message =
                   PostAsync(FeedUri, content).Result)
            {
                return message.IsSuccessStatusCode;
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