namespace BetterAnime;

using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

class Config{


	public static void Read(){

		ParseConfigFile();

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
					string path = match.Groups[2].Value;
					// check if path is valid
					// string newPath = string.Join("#", path.Split(Path.GetInvalidPathChars()));
					// if (!newPath.Contains("#"))
					 	CONST.DOWNLOAD_PATH = path;
					// else
					// 	Console.WriteLine("Download_Path is not a valid path!".ToColor(Color.Red));
				break;
				case "download_threads":
					int.TryParse(match.Groups[2].Value, out CONST.DOWNLOAD_THREADS);
				break;
				case "save_cookies":
					bool.TryParse(match.Groups[2].Value, out CONST.RESTCLIENT_SAVE_COOKIES);
				break;
			}
		}
	}

	[DllImport( "kernel32.dll", SetLastError = true )]
	public static extern bool SetConsoleMode( IntPtr hConsoleHandle, int mode );
	[DllImport( "kernel32.dll", SetLastError = true )]
	public static extern bool GetConsoleMode( IntPtr handle, out int mode );

	[DllImport( "kernel32.dll", SetLastError = true )]
	public static extern IntPtr GetStdHandle( int handle );
}