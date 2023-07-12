namespace BetterAnime;

class Serie{
	public string Name, Url;

	public Serie(string name, string url){
		
		Name = name.Trim().DecodeHtmlAndUnicodes();
		Url = url.Replace("\\/", "/");
	}
}