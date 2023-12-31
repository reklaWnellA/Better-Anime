using RestSharp;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Web;

namespace BetterAnime;

class M3U8{
	static string baseUrl = "";
	static string refererUrl = "";

    static public async Task<string> GetPlaylist(string url){

        HtmlNode html = await Web.GetHtmlAsync(url);
        Regex parseIframe = new ("<iframe src=\"([^\"]*)\"");
		string playerUrl, newPlayerUrl, playlistUrl, playlist;
		refererUrl = url;
		Resolution bestQuality;

        if (!parseIframe.IsMatch(html.OuterHtml))
            return null;

        Console.WriteLine("Getting direct url".ToColor(Color.Yellow));
        playerUrl = parseIframe.Match(html.OuterHtml).Groups[1].Value;

        // get cookies
        await Web.AsyncRequest(new RestRequest(playerUrl, Method.Get));
        await Web.AsyncRequest(new RestRequest(url, Method.Head));

		// changePlayer
        bestQuality = SelectBestResolution(html.OuterHtml);
        newPlayerUrl = await ChangePlayerQuality(bestQuality.Token1, bestQuality.Token2);

        Console.WriteLine("Quality selected: ".ToColor(Color.Yellow) +
            $"{bestQuality.Quality}".ToColor(Color.Cyan));

        // get playlist m3u8
        playlistUrl = await GetNewPlaylistUrl(newPlayerUrl);
        if (playlistUrl.Contains(".m3u8"))
            playlist = await ParsePaylist(playlistUrl);
        else if (playlistUrl.Contains(".mp4"))
            return playlistUrl;
        else
            throw new Exception("GetPlaylist exception, direct url doesnt contain .m3u8 or .mp4");

        return playlist;
    }

	record Resolution (int Quality, string Token1, string Token2);
    static private Resolution SelectBestResolution(string html){

        Regex QualityRegex = new ("qualityString\\[\\\"(\\d+)p\\\"\\] = \\\"([^\\\"]*)\\\"");
        Regex TokenRegex = new ("_token:\"([^\"]*)\"");
		MatchCollection qualities;
		string token2;
		Resolution? selectedResolution = null;
		int q = 0;

        if (!QualityRegex.IsMatch(html) || !TokenRegex.IsMatch(html))
            throw new Exception("SelectBestResolution exception");

        qualities = QualityRegex.Matches(html);
        token2 = TokenRegex.Match(html).Groups[1].Value;

        foreach (Match quality in qualities){
            var token1 = quality.Groups[2].Value;
            var qq = int.Parse(quality.Groups[1].Value);
            if (q < qq && qq <= CONST.BETTERANIME_MAX_QUALITY)
                selectedResolution = new Resolution(
                    qq,
                    HttpUtility.UrlEncode(token1),
                    HttpUtility.UrlEncode(token2)
                );
        }

        return selectedResolution;
    }

    static private async Task<string> ChangePlayerQuality(string quality, string token){

        RestRequest request = new(CONST.BETTERANIME_CHANGE_PLAYER_ENDPOINT, Method.Post);
		RestResponse response;
		Regex urlRegex = new ("\"frameLink\":\"([^\"]*)\"");
		string newPlayerUrl;

        request.AddHeader("origin", CONST.BETTERANIME_ROOT_ENDPOINT);
        request.AddHeader("referer", refererUrl);
        request.AddHeader("content-type", "application/x-www-form-urlencoded; charset=UTF-8");
        request.AddHeader("x-requested-with", "XMLHttpRequest");

        request.AddParameter("application/x-www-form-urlencoded",
			$"_token={token}&info={quality}", ParameterType.RequestBody);

        response = await Web.AsyncRequest(request);

        if (!urlRegex.IsMatch(response.Content))
            throw new ("ChangePlayerQuality exception");

		newPlayerUrl = urlRegex.Match(response.Content).Groups[1].Value.Replace("\\/", "/");
        return newPlayerUrl;
    }

    static private async Task<string> GetNewPlaylistUrl(string playerUrl){

        RestRequest request = new (playerUrl, Method.Get);
		RestResponse response;
		Regex urlParser;
		string m3u8;

        request.AddHeader("referer", refererUrl);
        response = await Web.AsyncRequest(request);

        urlParser = new Regex("\"file\":\"([^\"]*)\"");

        if (!urlParser.IsMatch(response.Content))
            throw new ("GetNewPlaylistUrl exception");

        m3u8 = urlParser.Match(response.Content).Groups[1].Value.Replace("\\/", "/"); // keep parameters
        //m3u8 = m3u8.Substring(0, m3u8.IndexOf(".m3u8") + 5); // skip parameters
        m3u8 = m3u8.DecodeHtmlAndUnicodes();

        baseUrl = m3u8.Substring(0, m3u8.LastIndexOf("/") + 1);

        return m3u8;
    }
    static private async Task<string> ParsePaylist(string m3u8Url){

        RestResponse response;
		Regex playlistRegex = new ("#EXTINF:[^\n]*\n([^\n]*)");
		Regex masterRegex = new ("RESOLUTION=\\d+x(\\d+)\n([^\n]*)");
		RestRequest request = new (m3u8Url, Method.Get);
		string m3u8;

		request.AddHeader("referer", refererUrl);

        response = await Web.AsyncRequest(request);

        // if(!response.IsSuccessful && 
        //   (response.ErrorMessage == "The SSL connection could not be established, see inner exception.")){
        //     request = new (m3u8Url.Replace("https://","http://"), Method.Get);
        //     request.AddHeader("referer", refererUrl);
        //     response = await Web.AsyncRequest(request);
        // }

		m3u8 = response.Content;

        if (masterRegex.IsMatch(m3u8)){
            MatchCollection matches = masterRegex.Matches(m3u8);
            int quality = 0;
            string link = "";

            // Select Best Quality
            foreach (Match match in matches){
                int q = int.Parse(match.Groups[1].Value);
                if (q > quality && q <= CONST.BETTERANIME_MAX_QUALITY){
                    quality = q;
                    link = match.Groups[2].Value.Replace("\\/", "/");
                    if (!link.Contains("https://"))
                        link = baseUrl + link;
                }
            }

            var playlist = await Web.GetHtmlAsync(link);
            return playlist.OuterHtml;
        }
        else if (playlistRegex.IsMatch(m3u8))
            return m3u8;
        else
            throw new ("ParsePaylist exception");
    }
	
    static public async Task<List<Segment>> ReplacePlaylist(string playlist, string path){

        string replacedplaylist = playlist;
        Regex KeyRegex = new ("URI=\"([^\"]*)\"");
        Regex PlaylistRegex = new ("#EXTINF:[^\n]*\n([^\n]*)");
		MatchCollection matches;
		List<Segment> list;

        // Download .KEY
        if (KeyRegex.IsMatch(playlist)){
            string keyUrl = KeyRegex.Match(playlist).Groups[1].Value;
            replacedplaylist = playlist.Replace(keyUrl, path.Replace("\\","\\\\") +  "key.key");
            
			await Download.FileRequest(keyUrl, path + "key.key", Download.cts.Token);
        }

        // Replace playlist.m3u8
        if (!PlaylistRegex.IsMatch(playlist))
            throw new ("ReplacePlaylist exception");

        matches = PlaylistRegex.Matches(playlist);
        list = new List<Segment>();

        // Set EXT and parameters if any
        //string firstUrl = matches[0].Groups[1].Value;
        //string ext = "";    // some episodes dont have extensions (.ts and etc)
        //string parameters = "";

        //if (firstUrl.Contains("?"))
        //{
        //    parameters = firstUrl.Substring(firstUrl.IndexOf("?"));
        //    ext = firstUrl.Substring(firstUrl.IndexOf("."), firstUrl.IndexOf("?") - firstUrl.IndexOf("."));
        //}
        //else
        //ext = firstUrl.Substring(firstUrl.IndexOf("."));

        // Adds every segment to segmentList
        foreach (Match match in matches){

            string oldUrlToReplace = match.Groups[1].Value;
            string url = match.Groups[1].Value;
            string segment = "";

            // remove parameters (maybe not a good idea?)
            //if (!string.IsNullOrEmpty(parameters))
            //    url = url.Replace(parameters, "");

            if (url.Contains("/")){
                if (url.LastIndexOf(".") > url.LastIndexOf("/"))         // last '.' is from extension (".mp4")
                    segment = url.Substring(url.LastIndexOf("/") + 1, url.LastIndexOf(".") - url.LastIndexOf("/") - 1) /*+ ext*/;
                else
                    segment = url.Substring(url.LastIndexOf("/") + 1);  // last '.' is from domain (".com")
            }
            else{
                segment = url;
                if (url.Contains("."))
                    segment = url.Substring(0, url.LastIndexOf(".")) /*+ ext*/;
                url = baseUrl + url;
            }

            // bypass googleusercontent proxy
            if (url.Contains("https://"))
                url = url.Substring(url.LastIndexOf("https://"));

            replacedplaylist = replacedplaylist.Replace(oldUrlToReplace, path + segment);
            list.Add(new Segment(url, path + segment));
        }

        File.WriteAllText(path + "playlist.m3u8", replacedplaylist);
		return list;
    }
}