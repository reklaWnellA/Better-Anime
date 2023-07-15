namespace BetterAnime;

public enum Color{
	Black = 30,
	Red = 31,
	Green = 32,
	Yellow = 33,
	Blue = 34,
	Magenta = 35,
	Cyan = 36,
	White = 37,
	Reset = 0
}

static class ColorExtensions{
	public static string ToPattern(this Color color) =>
		string.Format("\u001b[{0}m", (int)color);
	public static string ToColor(this string text, Color color) =>
		string.Format($"{color.ToPattern()}{text}{Color.Reset.ToPattern()}");
}