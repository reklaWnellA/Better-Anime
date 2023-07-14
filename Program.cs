namespace BetterAnime;

class Program{
	static async Task Main(){

		string path = $"{Directory.GetCurrentDirectory()}\\aaa\\";
		CONST.DOWNLOAD_THREADS = 100;
		CONST.RESTCLIENT_SAVE_COOKIES = true;

		// Set exit event handler
		AppDomain.CurrentDomain.ProcessExit += OnApplicationExit;

		// Set cookies if any
		if (CONST.RESTCLIENT_SAVE_COOKIES)
			Web.ReadCookiesFromFile();

		List<Serie>? search;
		List<Episode>? episodes;

		// Search
		search = await BetterAnime.Search("Bleach: Sennen Kessen-hen");
		if (search is null || search.Count == 0){
			Console.WriteLine("nothing was found");
			return;
		}

		// Get Episodes
		episodes = await BetterAnime.GetEpisodes(search.FirstOrDefault());
		if (episodes is null){
			Console.WriteLine("was not possible to get episodes");
			return;
		}

		// Download
		Console.WriteLine($"Episodes Count: {episodes.Count}");
		foreach (var episode in episodes){
			if (episode is not null){
				Console.WriteLine($"Downloading: {episode.Title}");
				bool success = await BetterAnime.DownloadEpisode(episode, path + episode.Title);

				if (!success)
					Console.WriteLine("an error occurred while downloading the episode!\nerror was saved to log file, skipping episode...");
			}
		}

		Console.WriteLine("acabo!");
	}

	static void OnApplicationExit(object sender, EventArgs e){
		Download.cts.Cancel();
		if (CONST.RESTCLIENT_SAVE_COOKIES)
			Web.SaveCookiesToFile();
	}
}