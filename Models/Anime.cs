namespace BetterAnime;

class Anime{
	public string Name, Url;

	public Anime(string name, string url){
		
		Name = name.Trim().DecodeHtmlAndUnicodes();
		Url = url.Replace("\\/", "/");
	}
}