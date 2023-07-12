namespace BetterAnime;

using RestSharp;
using System.Net;
using HtmlAgilityPack;

class Web{
	static public RestClient Client = new RestClient(
		new RestClientOptions {
			MaxTimeout = CONST.RESTCLIENT_MAXTIMEOUT,
			UserAgent = CONST.RESTCLIENT_USER_AGENT,
			CookieContainer = new CookieContainer()
		}
	);

	// static public async Task<string> ReturnHtmlAsString(string url){
	// 	try{
	// 		HttpClient httpClient = new HttpClient();
	// 		return await httpClient.GetStringAsync(url);
	// 	}
	// 	catch (Exception){
	// 		return null;
	// 	}
	// }

	
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
		HtmlDocument htmlDocument = new HtmlDocument();
		htmlDocument.LoadHtml(htmlString);
		return htmlDocument.DocumentNode;
	}

}