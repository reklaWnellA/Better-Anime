namespace BetterAnime;

using RestSharp;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

class Web{
	static private RestClient client;
	static public RestClient Client {
		get {
			if (client is null)
				client = new (options);
			
			return client;
		}
		set { client = value; }
	}
	static private RestClientOptions options = new(){
		MaxTimeout = CONST.RESTCLIENT_MAX_TIMEOUT,
		UserAgent = CONST.RESTCLIENT_USER_AGENT,
		CookieContainer = new CookieContainer()
	};

	static public void ReadCookiesFromFile(){
		if (!File.Exists(CONST.COOKIES_PATH))
			return;
		
		string cookies = File.ReadAllText(CONST.COOKIES_PATH);
		Regex cookiesRegex = new (@"^([^=]*)=([^\n;]*);?\n?", RegexOptions.Multiline);
		var cookieContainer = new CookieContainer();
		MatchCollection matches;
		Uri target = new (CONST.BETTERANIME_ROOT_ENDPOINT);

		if (!cookiesRegex.IsMatch(cookies))
			return;
		
		cookies = cookies.Replace("\r\n","\n");
		matches = cookiesRegex.Matches(cookies);
		foreach (Match cookie in matches){
			string name = cookie.Groups[1].Value;
			string value = cookie.Groups[2].Value;
			cookieContainer.Add(new Cookie(name, value) {Domain = target.Host});
		}

		options.CookieContainer = cookieContainer;
	}

	static public void SaveCookiesToFile(){
		string cookies = "";
		foreach(Cookie cookie in Client.Options.CookieContainer?.GetAllCookies())
			cookies += cookie.Name + "=" + cookie.Value + "\n";
		if (!string.IsNullOrEmpty(cookies))
			File.WriteAllText(CONST.COOKIES_PATH, cookies);
	}

	static public async Task<RestResponse> AsyncRequest (RestRequest request) =>
		await Client.ExecuteAsync(request);
	static public async Task<RestResponse> AsyncRequest (RestRequest request, CancellationToken ct) =>
		await Client.ExecuteAsync(request, ct);
	static public async Task<HtmlNode> GetHtmlAsync (string url){
		RestResponse response = await Client.ExecuteAsync(new RestRequest(url, Method.Get));
		return ParseStringToHtml(response.Content);
	}
	static public HtmlNode ParseStringToHtml(string htmlString){
		if (string.IsNullOrEmpty(htmlString))
			return null;
		HtmlDocument htmlDocument = new ();
		htmlDocument.LoadHtml(htmlString);
		return htmlDocument.DocumentNode;
	}
}