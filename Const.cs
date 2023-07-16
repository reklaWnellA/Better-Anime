namespace BetterAnime;

public class CONST {

	// Paths
	public static string CURRENT_DIR = Directory.GetCurrentDirectory();
	public static string CONFIG_PATH = $"{CURRENT_DIR}\\config.ini";
	public static string COOKIES_PATH = $"{CURRENT_DIR}\\cookies.txt";
	public static string DOWNLOAD_PATH = $"{CURRENT_DIR}\\Animes\\";
	public static string ANIME_PATH = "";
	public static string ERROR_LOG_PATH { get => $"{ANIME_PATH}errorlog.txt"; }

	// Downloads
	public static int DOWNLOAD_THREADS = 100;
	public static int DOWNLOAD_SEGMENT_MAX_RETRIES = 5;

	// RestClient
	public static int RESTCLIENT_MAX_TIMEOUT = 60_000;
	public static string RESTCLIENT_USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36 OPR/94.0.0.0";
	public static bool RESTCLIENT_SAVE_COOKIES = true;

	// BetterAnime
	public static string BETTERANIME_ROOT_ENDPOINT = "https://betteranime.net";
	public static string BETTERANIME_SEARCH_ENDPOINT = "https://betteranime.net/autocompleteajax?term={0}";
	public static string BETTERANIME_SEARCH_REGEX = "\"title\":\"([^\"]*)\",\"image\":\"[^\"]*\",\"url\":\"([^\"]*)\"";
	public static string BETTERANIME_CHANGE_PLAYER_ENDPOINT = "https://betteranime.net/changePlayer";
}