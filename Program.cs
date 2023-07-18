namespace BetterAnime;

class Program{
	static async Task Main(){

		// Set exit event handler
		AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
		Console.CancelKeyPress += OnCancelKeyPress;

		Anime animeSelected;
		List<Episode>? episodes;

		// Configs
		Config.Set();
		Config.Read();
		
		(int left, int top) = Console.GetCursorPosition();
		while(true){
			Console.SetCursorPosition(left, top);

			// Search
			animeSelected = await Search();

			// Get Episodes
			episodes = await GetEpisodes(animeSelected);
			if (episodes is null) return;
			episodes = Show.Episodes(episodes);

			// Download
			await DownloadEpisodes(animeSelected, episodes);

			// Retry?
			Console.WriteLine("\nNothing more to download!\n".ToColor(Color.Green) +
				$"Press ".ToColor(Color.Yellow) + "R".ToColor(Color.Cyan) +
				" to search another anime to download.".ToColor(Color.Yellow));
			var key = Console.ReadKey();
			if (key.KeyChar != 'r' && key.KeyChar != 'R')
				break;

			// Clear console
			Clear.Lines(left, top, Console.GetCursorPosition().Top - top + 2);
		}
	}

	static async Task<Anime> Search(){

		string? search;
		List<Anime>? results;
		int selectedOption;

		while(true){
			Console.Write($"Search: ".ToColor(Color.Cyan));
			search = Console.ReadLine();
			if(!string.IsNullOrWhiteSpace(search))
				break;
			else
				Console.WriteLine("Search a valid anime!".ToColor(Color.Red));
		}

		results = await BetterAnime.Search(search);

		if(!(results?.Any() ?? false)){
			Console.WriteLine("No anime was found with this name!".ToColor(Color.Red));
			return await Search();
		}

		selectedOption = Show.Menu(results.Select(a => a.Name).ToArray());
		return results[selectedOption];
	}

	static async Task<List<Episode>?> GetEpisodes(Anime serie){

		List<Episode>? episodes;

		Console.WriteLine("Getting ".ToColor(Color.Cyan) +
			$"{serie.Name.ToColor(Color.Yellow)}" + " episodes...".ToColor(Color.Cyan));
		episodes = await BetterAnime.GetEpisodes(serie);

		if(!(episodes?.Any() ?? false)){
			Console.WriteLine("An error occurred trying get anime episodes".ToColor(Color.Red));
			return null;
		}

		Console.WriteLine($"{episodes.Count} episodes found\n".ToColor(Color.Cyan));

		return episodes;
	}

	static async Task DownloadEpisodes(Anime anime, List<Episode> episodes){

		CONST.ANIME_PATH = CONST.DOWNLOAD_PATH + string.Join("#", anime.Name.Split(Path.GetInvalidFileNameChars())) + "\\";

		foreach (var episode in episodes){

			Console.WriteLine($"{new string('-', 10)}\n".ToColor(Color.Yellow));
			Console.WriteLine($"Downloading: {episode.Title}".ToColor(Color.Cyan));

			string episodePath = CONST.ANIME_PATH + string.Join("#", episode.Title.Split(Path.GetInvalidFileNameChars())) + ".mp4";
			bool success = await BetterAnime.DownloadEpisode(episode, episodePath);

			if (disposing) break;
			if (!success)
				Console.WriteLine("an error occurred while downloading the episode!\n".ToColor(Color.Red) +
					"stack trace was saved to errorlog.txt, skipping episode...".ToColor(Color.Yellow));
			
			Console.WriteLine();
		}
	}

	static void OnProcessExit(object sender, EventArgs e) =>
		Dispose();
	static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e) =>
		Environment.Exit(1);
	
	static bool disposing = false;
	static void Dispose(){
		disposing = true;
		Console.WriteLine($"\n\n\n{new string('-', 10)}\nDisposing\n{new string('-', 10)}".ToColor(Color.Red));
		Download.cts.Cancel();
		Thread.Sleep(1000); // wait for all threads to finish
		Download.cts.Dispose();
		if (CONST.RESTCLIENT_SAVE_COOKIES)
			Web.SaveCookiesToFile();
	}
}