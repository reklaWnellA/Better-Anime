namespace BetterAnime;

class Show{
	static public int Menu(string[] options){
		ConsoleKeyInfo key;
		int selectedOption = 0;
		bool isSelected = false;
		(int left, int top) = Console.GetCursorPosition();
		string space = "   ";
		string color = $"{space}{Color.Green.ToPattern()}";

		while(!isSelected){
			Console.SetCursorPosition(left,top);

			for (int i = 0; i < options.Length; i++)
				Console.WriteLine($"{(selectedOption == i ? color : space)}{options[i]}{Color.Reset.ToPattern()}");

			key = Console.ReadKey(true);
			switch (key.Key){
				case ConsoleKey.DownArrow:
					selectedOption = selectedOption == options.Length -1 ? 0 : selectedOption + 1; // Jumps to first option if last option was selected
					break;
				case ConsoleKey.UpArrow:
					selectedOption = selectedOption == 0 ? options.Length -1 : selectedOption - 1; // Jumps to last option if first option was selected
					break;
				case ConsoleKey.Enter:
					isSelected = true;
					break;
			}
		}

		// clear console
		Console.SetCursorPosition(left,top);
		for (int i = 0; i < options.Length; i++)
			Console.Write(new string(' ', Console.BufferWidth)); // Console.WindowWidth
		Console.SetCursorPosition(left,top);
		Console.WriteLine($"{(color)}{options[selectedOption]}{Color.Reset.ToPattern()}");

		return selectedOption;
	}
}