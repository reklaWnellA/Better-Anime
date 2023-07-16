using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace BetterAnime;

class BetterAnime{
    public static async Task<List<Anime>?> Search(string animeSearch){

        string url = "";
        var list = new List<Anime>();
        HtmlNode html;
        Regex parseAnimes = new (CONST.BETTERANIME_SEARCH_REGEX);
        MatchCollection animes;
        
        if (animeSearch.Contains("https://"))
            url = animeSearch;
        else
            url = string.Format(CONST.BETTERANIME_SEARCH_ENDPOINT, HttpUtility.UrlEncode(animeSearch));

        html = await Web.GetHtmlAsync(url);

        if (!parseAnimes.IsMatch(html.OuterHtml))
            return null;

        animes = parseAnimes.Matches(html.OuterHtml);
        foreach (Match anime in animes){

            string animeName = anime.Groups[1].Value;
            string animeUrl = anime.Groups[2].Value;

            if (animeUrl.StartsWith('/'))
                animeUrl = CONST.BETTERANIME_ROOT_ENDPOINT + animeUrl;

            list.Add(new Anime(animeName, animeUrl));
        }
        return list;
    }

    public static async Task<List<Episode>?> GetEpisodes(Anime serie){

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

            if (File.Exists(path)){
                Console.WriteLine("Episode already downloaded, skipping episode".ToColor(Color.Green));
                return true;
            }

            string playlist = await M3U8.GetPlaylist(episode.Url);
            bool isMp4 = playlist.Contains(".mp4");

            if (isMp4){
                Console.WriteLine("Downloading mp4 file".ToColor(Color.Yellow));
                await Download.Mp4(playlist, path, Download.cts.Token);
            }
            else{ // m3u8
                Directory.CreateDirectory(tempPath);

                Console.Write("Downloading m3u8 segments ".ToColor(Color.Yellow));
                var segmentList = await M3U8.ReplacePlaylist(playlist, tempPath);
                await Download.Segments(segmentList);
            }

            // m3u8
            if (!isMp4){
                if (!Download.cts.IsCancellationRequested){
                    // Join Segments
                    Console.WriteLine("Merging segments".ToColor(Color.Yellow));
                    {
                        var psi = new ProcessStartInfo{
                            UseShellExecute = false,
                            // #if !DEBUG
                                CreateNoWindow = true, //This hides the cmd prompt that usually shows
                            // #else
                            //     CreateNoWindow = false,
                            // #endif
                            FileName = "cmd.exe",
                            WorkingDirectory = CONST.CURRENT_DIR,
                            //Verb = "runas", //This runs the cmd as administrator
                            Arguments = "/c chcp 65001 &&" +
                                $"ffmpeg -allowed_extensions ALL -i \"{tempPath}playlist.m3u8\" -acodec copy -vcodec copy \"{path}\" && " +
                                // #if DEBUG
                                //     "pause"
                                // #else
                                    "exit"
                                // #endif
                        };

                        var process = new Process{
                            StartInfo = psi
                        };
                        process.Start();
                        process.WaitForExit();
                    }
                }
                
                // Delete folder
                // #if !DEBUG
                    Directory.Delete(tempPath, true);
                // #endif
            }

            if (Download.cts.IsCancellationRequested){
                Download.cts = new CancellationTokenSource();
                return false;
            }
            
            Console.WriteLine("Episode downloaded succesfully!".ToColor(Color.Green));
            return true;
        }
        catch(Exception ex){

            string message = "";
            message += new string('-', 10) + "\n";
            message += "Date: " + DateTime.Now.ToString() + "\n";
            message += ex.GetType().FullName + "\n";
            message += "Message: " + ex.Message + "\n";
            message += "StackTrace: " + ex.StackTrace + "\n\n";

            File.WriteAllText(CONST.ERROR_LOG_PATH, message);
            return false;
        }
    }
}