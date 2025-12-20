using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.Parser.Interfaces;
using Dalamud.Utility;
using HtmlAgilityPack;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AcquisitionDate.Parser.Elements;

internal class LodestonePageLanguageParser : IAcquistionParserElement<LodestoneRegion>
{
    static readonly Regex ldst_subdomain_regex = new Regex(@"var\s+ldst_subdomain\s*=\s*'([^']+)';", RegexOptions.Compiled);

    public void Parse(HtmlNode rootNode, Action<LodestoneRegion> onSuccess, Action<Exception> onFailure)
    {
        HtmlNode[] scriptNodes = rootNode.Descendants("script").ToArray();
        if (scriptNodes.Length == 0)
        {
            onFailure?.Invoke(new Exception("No script nodes found!"));
            return;
        }

        for (int i = 0; i < scriptNodes.Length; i++)
        {
            HtmlNode scriptNode = scriptNodes[i];

            string innerHtml = scriptNode.InnerHtml;
            if (!innerHtml.Contains("var ldst_subdomain")) continue;

            string? extractedLocale = ExtractLdstSubdomain(innerHtml);
            if (extractedLocale.IsNullOrWhitespace())
            {
                onFailure?.Invoke(new Exception("Couldn't extract the locale"));
                return;
            }

            LodestoneRegion region = GetRegion(extractedLocale);
            onSuccess?.Invoke(region);
            break;
        }
    }

    string? ExtractLdstSubdomain(string scriptContent)
    {
        Match match = ldst_subdomain_regex.Match(scriptContent);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }

    LodestoneRegion GetRegion(string regionText) => regionText switch
    {
        "na" => LodestoneRegion.America,
        "jp" => LodestoneRegion.Japan,
        "eu" => LodestoneRegion.Europe,
        "de" => LodestoneRegion.Germany,
        "fr" => LodestoneRegion.France,
        _ => LodestoneRegion.Europe,
    };
}
