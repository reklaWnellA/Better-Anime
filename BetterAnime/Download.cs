using RestSharp;

namespace BetterAnime;

class Download{

    public static CancellationTokenSource cts = new ();

	public static async Task Segments(List<Segment> list){
        int totalCount = list.Count;
        int runningThreadsCount = 0;
        CancellationToken ct = cts.Token;
        (int left, int top) = Console.GetCursorPosition();
        int progress = 0;
        int lastProgress = -1;
        // bool shortThreadsCreated = false;

        while(totalCount > 0){

            if(Volatile.Read(ref runningThreadsCount) < CONST.DOWNLOAD_THREADS){
                string url = list[totalCount - 1].Url;
                string segPath = list[totalCount - 1].Path;

                Interlocked.Increment(ref runningThreadsCount);
				Interlocked.Decrement(ref totalCount);

                Thread thread = new (async () => {
                    await FileRequest(url, segPath, ct);
                    Interlocked.Decrement(ref runningThreadsCount);
                    Interlocked.Increment(ref progress);
                });
                thread.Start();

                Show.DownloadProgress(left, top, lastProgress, progress, list.Count);
            }

            // wait some thread to finish
            else
                await Task.Delay(100);
        }

        // wait remaining threads
        while (Volatile.Read(ref runningThreadsCount) > 0){
            await Task.Delay(100);
            Show.DownloadProgress(left, top, lastProgress, progress, list.Count);

            // if (Volatile.Read(ref runningThreadsCount) <= 10 && !shortThreadsCreated){
            //     // create 5 threads for each segment with 2 max retries and 5 sec max timeout
                

            //     shortThreadsCreated = true;
            // }
            // else if (Volatile.Read(ref runningThreadsCount) <= 10){
            //     // check if any segment succesfully finished and cancel cts
            //     if()
            // }
        }

        Console.WriteLine();
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
                string message = "";
                message += new string('-', 10) + "\n";
                message += "Date: " + DateTime.Now.ToString() + "\n";
                message += $"Episode: {Program.currentlyDownloading}\n";
                message += $"SEGMENT MAX RETRIES REACHED, URL: {url}\n\n";
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








