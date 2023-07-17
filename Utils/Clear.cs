namespace BetterAnime;

class Clear{
	static public void Lines(int left, int top, int columns){
		Console.SetCursorPosition(left, top);
			for (int i = 0; i < columns; i++)
				Console.WriteLine(new string(' ', Console.BufferWidth)); // WindowWidth | BufferWidth
	}
}