using RestSharp;

namespace BetterAnime;

class Download{

    public static CancellationTokenSource cts = new ();

	public static async Task Segments(List<Segment> list){
        int totalCount = list.Count;
        int runningThreadsCount = 0;
        CancellationToken ct = cts.Token;

        while(totalCount > 0){

            if(Volatile.Read(ref runningThreadsCount) < CONST.DOWNLOAD_THREADS)
            {
                string url = list[totalCount - 1].Url;
                string segPath = list[totalCount - 1].Path;

                Interlocked.Increment(ref runningThreadsCount);
				Interlocked.Decrement(ref totalCount);

                Thread thread = new (async () =>
                {
                    await FileRequest(url, segPath, ct);

                    Interlocked.Decrement(ref runningThreadsCount);
                });
                thread.Start();
            }

            // wait some thread to finish
            else
                await Task.Delay(100);
        }

        // wait remaining threads
        while (Volatile.Read(ref runningThreadsCount) > 0)
            await Task.Delay(100);
    }

    static public async Task FileRequest(string url, string path, CancellationToken ct, int retry = 0){

        if (ct.IsCancellationRequested)
            return;
        
        RestResponse response;
        RestRequest request = new (url, Method.Get);
        request.AddHeader("origin", CONST.BETTERANIME_ROOT_ENDPOINT);

        response = await Web.AsyncRequest(request, ct);
        if (response.RawBytes is not null && response.IsSuccessful)
            File.WriteAllBytes(path, response.RawBytes);
        else{
            if (retry < CONST.DOWNLOAD_SEGMENT_MAX_RETRIES)
				await FileRequest(url, path, ct, ++retry);
			else{
                string message = $"SEGMENT MAX RETRIES REACHED, URL: {url}";
                File.WriteAllText(CONST.ERROR_LOG_PATH, message);
                Console.WriteLine(message.ToColor(Color.Red));
				cts.Cancel();
            }
        }
    }
    
    static public async Task Mp4(string url, string path, CancellationToken ct){
        
        if (ct.IsCancellationRequested)
            return;

        RestResponse response;
        RestRequest request = new (url, Method.Get);
        request.AddHeader("referer", CONST.BETTERANIME_ROOT_ENDPOINT);

        response = await Web.Client.HeadAsync(request, ct);
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
            cts.Cancel();

        request = new RestRequest(response.ResponseUri, Method.Get);
        request.AddHeader("referer", CONST.BETTERANIME_ROOT_ENDPOINT);

        using (var stream = await Web.Client.DownloadStreamAsync(request, ct))
        using (var output = new FileStream(path, FileMode.Create))
            await stream.CopyToAsync(output, ct);
    }
}








