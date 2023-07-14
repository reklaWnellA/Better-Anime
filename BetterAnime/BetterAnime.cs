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
        Regex parseAnimes = new (CONST.BETTERANIME_SEARCH_REGEX);
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

            string pathNoTitle = path.Substring(0, path.LastIndexOf("\\"));
            string tempPath = $"{pathNoTitle}\\TempFolder\\";
            if (!Directory.Exists(pathNoTitle))
                Directory.CreateDirectory(pathNoTitle);

            string playlist = await M3U8.GetPlaylist(episode.Url);
            bool isMp4 = playlist.Contains(".mp4");

            if (isMp4){
                Console.WriteLine("Downloading mp4 file");
                await Download.Mp4(playlist, path, Download.cts.Token);
            }
            else{ // m3u8
                Directory.CreateDirectory(tempPath);

                Console.WriteLine("Downloading m3u8 segments");
                var segmentList = await M3U8.ReplacePlaylist(playlist, tempPath);
                await Download.Segments(segmentList);
            }

            if (Download.cts.IsCancellationRequested){
                Download.cts = new CancellationTokenSource();
                return false;
            }


            // m3u8
            if (!isMp4){
                // Join Segments
                System.Console.WriteLine("Merging segments");
                {
                    var psi = new ProcessStartInfo{
                        UseShellExecute = false,
                        CreateNoWindow = true, //This hides the cmd prompt that usually shows
                        FileName = "cmd.exe",
                        WorkingDirectory = CONST.CURRENT_DIR,
                        //Verb = "runas", //This runs the cmd as administrator
                        Arguments = "/c chcp 65001 &&" +
                            "ffmpeg -allowed_extensions ALL -i \"" + tempPath + "playlist.m3u8\" -acodec copy -vcodec copy \"" + path + ".mkv\" && " +
                            "exit"
                    };

                    var process = new Process{
                        StartInfo = psi
                    };
                    process.Start();
                    process.WaitForExit();
                }

                // Delete folder
                Directory.Delete(tempPath, true);
            }
            return true;
        }
        catch(Exception ex){

            string message = "";
            message += new string('-', 10) + "\n";
            message += "Date : " + DateTime.Now.ToString() + "\n";
            message += ex.GetType().FullName + "\n";
            message += "Message : " + ex.Message + "\n";
            message += "StackTrace : " + ex.StackTrace + "\n\n";

            File.WriteAllText(CONST.ERROR_LOG_PATH, message);
            return false;
        }
    }
}