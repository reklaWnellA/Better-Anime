namespace BetterAnime;

using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

static class StringHelper{
	static public Regex UnicodePattern = new Regex(@"\\[uU]([0-9A-F]{4})" /*,RegexOptions.Compiled*/);
	static public string DecodeHtmlAndUnicodes(this string input){
		var tmp = WebUtility.HtmlDecode(input);
		while (tmp != input){
			input = tmp;
			tmp = WebUtility.HtmlDecode(input);
		}

		if(UnicodePattern.IsMatch(input))
			input = UnicodePattern.Replace(input, match => ((char)Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString());

		return input; //completely decoded string
	}
}