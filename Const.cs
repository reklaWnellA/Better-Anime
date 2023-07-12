namespace BetterAnime;

public class CONST {

	// Paths
	// private static string currentDir = System.IO.Directory.GetCurrentDirectory();
	// public static string DOWNLOADPATH = $"{currentDir}\\";

	// Downloads
	public static int DOWNLOAD_THREADS;

	// RestClient
	public static int RESTCLIENT_MAXTIMEOUT = 30_000;
	public static string RESTCLIENT_USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36 OPR/94.0.0.0";

	// BetterAnime
	public static string BETTERANIME_ROOT_ENDPOINT = "https://betteranime.net";
	public static string BETTERANIME_SEARCH_ENDPOINT = "https://betteranime.net/autocompleteajax?term={0}";
	public static string BETTERANIME_SEARCH_REGEX = "\"title\":\"([^\"]*)\",\"image\":\"[^\"]*\",\"url\":\"([^\"]*)\"";
	public static string BETTERANIME_CHANGE_PLAYER_ENDPOINT = "https://betteranime.net/changePlayer";

}