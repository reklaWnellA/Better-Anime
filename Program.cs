namespace BetterAnime;

class Program{
	static async Task Main(string[] args){

		// Set exit event handler
		AppDomain.CurrentDomain.ProcessExit += OnApplicationExit;

		List<Serie>? search;
		List<Episode>? episodes;

		string path = @"C:\Users\Allen\Desktop\aaa\";
		CONST.DOWNLOAD_THREADS = 100;
		
		// Search
		search = await BetterAnime.Search("Bleach: Sennen Kessen-hen");
		if (search is null || search.Count() == 0){
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
				await BetterAnime.DownloadEpisode(episode, path + episode.Title);
			}
		}

		Console.WriteLine("acabo!");
	}

	static void OnApplicationExit(object sender, EventArgs e){
		Download.cts.Cancel();
	 }
}