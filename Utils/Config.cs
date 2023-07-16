namespace BetterAnime;

using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

class Config{


	public static void Read(){

		ParseConfigFile();

		Console.WriteLine($"{new string('-', 10)}\nConfigs:".ToColor(Color.Green));

		Console.WriteLine("DOWNLOAD PATH = ".ToColor(Color.Green)+ $"{CONST.DOWNLOAD_PATH}".ToColor(Color.Cyan));
		Console.WriteLine("DOWNLOAD THREADS = ".ToColor(Color.Green)+ $"{CONST.DOWNLOAD_THREADS}".ToColor(Color.Cyan));
		Console.WriteLine("DOWNLOAD SEGMENT MAX RETRIES = ".ToColor(Color.Green)+ $"{CONST.DOWNLOAD_SEGMENT_MAX_RETRIES}".ToColor(Color.Cyan));
		Console.WriteLine("CLIENT MAX TIMEOUT = ".ToColor(Color.Green)+ $"{CONST.RESTCLIENT_MAX_TIMEOUT}".ToColor(Color.Cyan));
		Console.WriteLine("SAVE COOKIES = ".ToColor(Color.Green)+ $"{CONST.RESTCLIENT_SAVE_COOKIES}".ToColor(Color.Cyan));

		Console.WriteLine($"{new string('-', 10)}\n\n".ToColor(Color.Green));

		if (CONST.RESTCLIENT_SAVE_COOKIES)
			Web.ReadCookiesFromFile();
	}
	public static void Set(){

		// Enable windows console to use color codes
		if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
			var handle = GetStdHandle( -11 );
			int mode;
			GetConsoleMode( handle, out mode );
			SetConsoleMode( handle, mode | 0x4 );
		}

		Console.OutputEncoding = System.Text.Encoding.UTF8;
	}

	public static void ParseConfigFile(){

		Regex configPattern = new (@"([\w_]+)=([^\n]+)");
		string configContent;
		MatchCollection matches;

		if(!File.Exists(CONST.CONFIG_PATH))
			return;

		configContent = File.ReadAllText(CONST.CONFIG_PATH).Replace("\r","");
		if(!configPattern.IsMatch(configContent))
			return;

		matches = configPattern.Matches(configContent);
		foreach(Match match in matches){
			string key = match.Groups[1].Value;
			switch(key.ToLower().Trim()){
				case "download_path":
					CONST.DOWNLOAD_PATH = match.Groups[2].Value;
				break;
				case "download_threads":
					int.TryParse(match.Groups[2].Value, out CONST.DOWNLOAD_THREADS);
				break;
				case "download_segment_max_retries":
					int.TryParse(match.Groups[2].Value, out CONST.DOWNLOAD_SEGMENT_MAX_RETRIES);
				break;
				case "client_max_timeout":
					int.TryParse(match.Groups[2].Value, out CONST.RESTCLIENT_MAX_TIMEOUT);
				break;
				case "save_cookies":
					bool.TryParse(match.Groups[2].Value, out CONST.RESTCLIENT_SAVE_COOKIES);
				break;
			}
		}

		if (CONST.DOWNLOAD_PATH[CONST.DOWNLOAD_PATH.Length-1] != '\\')
			CONST.DOWNLOAD_PATH += "\\";
	}

	[DllImport( "kernel32.dll", SetLastError = true )]
	public static extern bool SetConsoleMode( IntPtr hConsoleHandle, int mode );
	[DllImport( "kernel32.dll", SetLastError = true )]
	public static extern bool GetConsoleMode( IntPtr handle, out int mode );

	[DllImport( "kernel32.dll", SetLastError = true )]
	public static extern IntPtr GetStdHandle( int handle );
}