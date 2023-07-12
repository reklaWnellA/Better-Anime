using RestSharp;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace BetterAnime;

class BetterAnime{
    public static async Task<List<Serie>?> Search(string animeSearch){

        string url = string.Format(CONST.BETTERANIME_SEARCH_ENDPOINT, HttpUtility.UrlEncode(animeSearch));
        var list = new List<Serie>();
        HtmlNode html = await Web.GetHtmlAsync(url);
        Regex parseAnimes = new Regex(CONST.BETTERANIME_SEARCH_REGEX);
        MatchCollection animes;

        if (!parseAnimes.IsMatch(html.OuterHtml))
            return null;

        animes = parseAnimes.Matches(html.OuterHtml);
        foreach (Match anime in animes){

            string animeName = anime.Groups[1].Value;
            string animeUrl = anime.Groups[2].Value;

            if (animeUrl.StartsWith('/'))
                animeUrl = CONST.BETTERANIME_ROOT_ENDPOINT + animeUrl;

            list.Add(new Serie(animeName, animeUrl));
        }
        return list;
    }

    public static async Task<List<Episode>?> GetEpisodes(Serie serie){

        HtmlNode html = await Web.GetHtmlAsync(serie.Url);
        var list = new List<Episode>();
        HtmlNode? episodesPanel;
        List<HtmlNode>? episodes;

        if (string.IsNullOrWhiteSpace(html.OuterHtml))
            return null;

        episodesPanel = html.Descendants("ul")
            .Where(a => a.GetAttributeValue("id", "")
            .Equals("episodesList")).FirstOrDefault();

        if (episodesPanel is null) return null;
        
        episodes = episodesPanel.Descendants("li")
            .Where(a => a.GetAttributeValue("class", null)
            .Contains("list-group-item-action")).ToList();

        foreach (var episode in episodes){

            HtmlNode? anchor = episode.Descendants("a").FirstOrDefault();

            if (anchor is null) continue;

            string episodeTitle = anchor.InnerText;
            string episodeUrl = anchor.GetAttributeValue("href", "");

            if (string.IsNullOrEmpty(episodeUrl)) continue;

            if (episodeUrl.StartsWith('/'))
                episodeUrl = CONST.BETTERANIME_ROOT_ENDPOINT + episodeUrl;

            list.Add(new Episode(episodeTitle, episodeUrl));
        }

        return list;
    }

    public static async Task<bool> DownloadEpisode(Episode episode, string path){

        try{

            string TempPath = path.Substring(0, path.LastIndexOf("\\")) + "\\TempFolder\\";
            Directory.CreateDirectory(TempPath);

            string playlist = await M3U8.GetPlaylist(episode.Url);
		    var segmentList = await M3U8.ReplacePlaylist(playlist, TempPath);
            
		    await Download.Segments(segmentList);

            // Join Segments
            {
                var psi = new ProcessStartInfo();
                psi.UseShellExecute = false;
                psi.CreateNoWindow = false; //This hides the cmd prompt that usually shows
                psi.FileName = "cmd.exe";
                psi.WorkingDirectory = TempPath;
                //psi.Verb = "runas"; //This runs the cmd as administrator
                psi.Arguments = "/c chcp 65001 &&" +
                    "ffmpeg -allowed_extensions ALL -i playlist.m3u8 -acodec copy -vcodec copy \"" + path + ".mkv\" && "+
                    "pause";

                var process = new Process();
                process.StartInfo = psi;
                process.Start();
                process.WaitForExit();
            }

            // Delete folder
            Directory.Delete(TempPath, true);
            return true;
        }
        catch(Exception e){
            return false;
        }
    }
}