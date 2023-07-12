using RestSharp;

namespace BetterAnime;

class Download{

    public static CancellationTokenSource cts = new CancellationTokenSource();

	public static async Task Segments(List<Segment> list){
        int totalCount = list.Count;
        int runningThreadsCount = 0;

        while(totalCount > 0){

            if(Volatile.Read(ref runningThreadsCount) < CONST.DOWNLOAD_THREADS)
            {
                string url = list[totalCount - 1].Url;
                string segPath = list[totalCount - 1].Path;

                Interlocked.Increment(ref runningThreadsCount);
				Interlocked.Decrement(ref totalCount);

                Thread thread = new Thread(async () =>
                {
                    await FileRequest(url, segPath);

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

    static public async Task FileRequest(string url, string path, bool retry = false){

        RestResponse response;
        RestRequest request = new RestRequest(url, Method.Get);
        request.AddHeader("origin", CONST.BETTERANIME_ROOT_ENDPOINT);

        response = await Web.AsyncRequest(request, cts.Token);
        if (response.RawBytes is not null){
            File.WriteAllBytes(path, response.RawBytes);
        }
        else{
            if (!retry)
				await FileRequest(url, path, true);
			else
				throw new Exception();
        }
    }
}








