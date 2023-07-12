namespace BetterAnime;

class Episode{
	public string Title, Url;

	public Episode(string title, string url){
		Title = title.Trim().DecodeHtmlAndUnicodes();
		Url = url.Replace("\\/", "/");
	}
}