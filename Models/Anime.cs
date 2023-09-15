namespace BetterAnime;

class Anime{
	public string Name, Url;
	public Anime(string name, string url){
		
		if (name is not null)
			Name = name.Trim().DecodeHtmlAndUnicodes();
		Url = url.Replace("\\/", "/");
	}
}