using System.Security.Cryptography;

namespace BetterAnime;

class Show{
	static public int Menu(string[] options){
		ConsoleKeyInfo key;
		int selectedOption = 0;
		bool isSelected = false;
		(int left, int top) = Console.GetCursorPosition();
		string space = "   ";
		string color = $"{space}{Color.Green.ToPattern()}";

		Console.WriteLine("Press Up/Down arrow keys to select!".ToColor(Color.Yellow));
		while(!isSelected){
			Console.SetCursorPosition(left,top + 1);

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
		for (int i = 0; i < options.Length + 1; i++)
			Console.Write(new string(' ', Console.BufferWidth)); // Console.WindowWidth BufferWidth
		Console.SetCursorPosition(left,top);
		Console.WriteLine($"{color}{options[selectedOption]}{Color.Reset.ToPattern()}\n");

		return selectedOption;
	}
	static public List<Episode> Episodes(List<Episode> episodes){
		ConsoleKeyInfo key;
		int selectedFrom = 0, selectedTo = 0;
		bool selectingFrom = true, isSelected = false;

		(int left, int top) = Console.GetCursorPosition();
		Console.WriteLine("Use ".ToColor(Color.Yellow) + "Left/Right".ToColor(Color.Cyan) +
			" arrow keys to change between From and To episodes\nUse ".ToColor(Color.Yellow)+
			"Up/Down".ToColor(Color.Cyan) + " arrow keys to change the episode".ToColor(Color.Yellow));
		
		while(!isSelected){
			Console.SetCursorPosition(left, top+2);
			Console.WriteLine("Number of episodes selected:".ToColor(Color.Yellow) + $" {selectedTo + 1}".ToColor(Color.Green));

			string from = episodes[selectedFrom].Title;
			from = selectingFrom ? from.ToColor(Color.Green) : from.ToColor(Color.Yellow);

			string to = selectedTo == 0 ? "Only this episode" : episodes[selectedFrom + selectedTo].Title;
			to = !selectingFrom ? to.ToColor(Color.Green) : to.ToColor(Color.Yellow);

			Console.WriteLine($"{from} TO {to}");

			key = Console.ReadKey(true);
			switch (key.Key){
				case ConsoleKey.DownArrow:
					if (selectingFrom){
						selectedFrom = selectedFrom == episodes.Count -1 ? 0 : selectedFrom + 1; // Jumps to first option if last option was selected
						selectedTo = 0;
					}
					else
						selectedTo = selectedTo == episodes.Count - selectedFrom -1 ? 0 : selectedTo + 1;
					break;
				case ConsoleKey.UpArrow:
					if (selectingFrom){
						selectedFrom = selectedFrom == 0 ? episodes.Count -1 : selectedFrom - 1; // Jumps to last option if first option was selected
						selectedTo = 0;
					}
					else
						selectedTo = selectedTo == 0 ? episodes.Count - selectedFrom -1 : selectedTo - 1;
					break;
				case ConsoleKey.RightArrow:
					selectingFrom = false;
					break;
				case ConsoleKey.LeftArrow:
					selectingFrom = true;
					break;
				case ConsoleKey.Enter:
					isSelected = true;
					break;
			}

			// clear
			Console.SetCursorPosition(left, top+2);
			for (int i = 0; i < 2 + 1; i++)
				Console.Write(new string(' ', Console.BufferWidth));
		}

		Console.SetCursorPosition(left, top);
		for (int i = 0; i < 2 + 1; i++)
				Console.Write(new string(' ', Console.BufferWidth));
		Console.SetCursorPosition(left, top);

		Console.WriteLine("Number of episodes selected:".ToColor(Color.Yellow) + $" {selectedTo + 1}".ToColor(Color.Green));

		string sfrom = episodes[selectedFrom].Title.ToColor(Color.Green);
		string sto = " TO ".ToColor(Color.Yellow) + episodes[selectedFrom + selectedTo].Title.ToColor(Color.Green);

		Console.WriteLine($"{sfrom}{(selectedTo == 0 ? "" : sto)}\n\n");

		return episodes.GetRange(selectedFrom, selectedTo + 1);
	}
	static public void DownloadProgress(int left, int top, int lastProgress, int progress, int total){
		if(lastProgress != progress){
			Console.SetCursorPosition(left,top);
			Console.Write($"{progress}".ToColor(Color.Green)+"/".ToColor(Color.Yellow) +$"{total}".ToColor(Color.Cyan));
		}
	}
}