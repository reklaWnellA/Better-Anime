namespace BetterAnime;

using System.Runtime.InteropServices;

class Config{
	public static void Set(){

		// Enable windows console to use color codes
		if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
			var handle = GetStdHandle( -11 );
			int mode;
			GetConsoleMode( handle, out mode );
			SetConsoleMode( handle, mode | 0x4 );
		}

		Console.OutputEncoding = System.Text.Encoding.UTF8;

		if (CONST.RESTCLIENT_SAVE_COOKIES)
			Web.ReadCookiesFromFile();
	}

	[DllImport( "kernel32.dll", SetLastError = true )]
	public static extern bool SetConsoleMode( IntPtr hConsoleHandle, int mode );
	[DllImport( "kernel32.dll", SetLastError = true )]
	public static extern bool GetConsoleMode( IntPtr handle, out int mode );

	[DllImport( "kernel32.dll", SetLastError = true )]
	public static extern IntPtr GetStdHandle( int handle );
}