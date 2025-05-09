using ArchiSteamFarm.Steam;
using ASFEnhance.Data;
using ASFEnhance.Data.WebApi;

namespace ASFEnhance.WishList;

/// <summary>
/// 网络请求
/// </summary>
public static class WebRequest
{
    /// <summary>
    /// 添加愿望单
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="gameId"></param>
    /// <param name="isAddWishlist"></param>
    /// <returns></returns>
    public static async Task<IgnoreGameResponse?> AddWishlist(this Bot bot, uint gameId, bool isAddWishlist)
    {
        var request = new Uri(SteamStoreURL, isAddWishlist ? "/api/addtowishlist" : "/api/removefromwishlist");
        var referer = new Uri(SteamStoreURL, "/app/" + gameId);

        var data = new Dictionary<string, string>(2, StringComparer.Ordinal)
        {
            { "appid", gameId.ToString() },
        };

        var response = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<IgnoreGameResponse>(request, data: data, referer: referer).ConfigureAwait(false);
        return response?.Content;
    }

    /// <summary>
    /// 关注游戏
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="gameId"></param>
    /// <param name="isFollow"></param>
    /// <returns></returns>
    public static async Task<bool> FollowGame(this Bot bot, uint gameId, bool isFollow)
    {
        var request = new Uri(SteamStoreURL, "/explore/followgame/");
        var referer = new Uri(SteamStoreURL, $"/app/{gameId}");

        var data = new Dictionary<string, string>(3, StringComparer.Ordinal)
        {
            { "appid", gameId.ToString() },
        };

        if (!isFollow)
        {
            data.Add("unfollow", "1");
        }

        var response = await bot.ArchiWebHandler.UrlPostToHtmlDocumentWithSession(request, data: data, referer: referer).ConfigureAwait(false);

        if (response == null)
        {
            return false;
        }

        return response?.Content?.Body?.InnerHtml.ToLowerInvariant() == "true";
    }

    /// <summary>
    /// 检查关注/愿望单情况
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="gameId"></param>
    /// <returns></returns>
    public static async Task<CheckGameResponse> CheckGame(this Bot bot, uint gameId)
    {
        var request = new Uri(SteamStoreURL, $"/app/{gameId}");

        var response = await bot.ArchiWebHandler.UrlPostToHtmlDocumentWithSession(request).ConfigureAwait(false);

        if (response == null)
        {
            return new(false, "网络错误");
        }

        if (response.FinalUri.LocalPath.Equals("/"))
        {
            return new(false, "商店页未找到");
        }

        return HtmlParser.ParseStorePage(response);
    }

    /// <summary>
    /// 忽略指定游戏
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="gameId"></param>
    /// <param name="isIgnore"></param>
    /// <returns></returns>
    internal static async Task<bool> IgnoreGame(this Bot bot, uint gameId, bool isIgnore)
    {
        var request = new Uri(SteamStoreURL, "/recommended/ignorerecommendation/");
        var referer = new Uri(SteamStoreURL, $"/app/{gameId}");

        var data = new Dictionary<string, string>(3, StringComparer.Ordinal)
        {
            { "appid", gameId.ToString() },
        };

        if (isIgnore)
        {
            data.Add("ignore_reason", "0");
        }
        else
        {
            data.Add("remove", "1");
        }

        var response = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<IgnoreGameResponse>(request, data: data, referer: referer).ConfigureAwait(false);

        if (response == null)
        {
            return false;
        }

        return response?.Content?.Result == true;
    }
}
