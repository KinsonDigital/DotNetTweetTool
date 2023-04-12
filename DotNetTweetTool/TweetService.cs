using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace TweetTesting;

/// <summary>
/// Sends tweets to Twitter.
/// </summary>
public class TweetService
{
    private const string Url = "https://api.twitter.com/2/tweets";

    /// <summary>
    /// Sends a tweet to Twitter.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="consumerAPIKey">The consumer API key.</param>
    /// <param name="consumerAPISecret">The consumer API secret.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="accessTokenSecret">The access token secret.</param>
    /// <exception cref="Exception">
    ///     Thrown if an HTTP request other than <see cref="HttpStatusCode.OK"/> and <see cref="HttpStatusCode.Created"/>.
    /// </exception>
    public void SendTweet(
        string message,
        string consumerAPIKey,
        string consumerAPISecret,
        string accessToken,
        string accessTokenSecret)
    {
        SendTweetAsync(message, consumerAPIKey, consumerAPISecret, accessToken, accessTokenSecret).Wait();
    }

    /// <summary>
    /// Asynchronously sends a tweet to Twitter.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="consumerAPIKey">The consumer API key.</param>
    /// <param name="consumerAPISecret">The consumer API secret.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="accessTokenSecret">The access token secret.</param>
    /// <exception cref="Exception">
    ///     Thrown if an HTTP request other than <see cref="HttpStatusCode.OK"/> and <see cref="HttpStatusCode.Created"/>.
    /// </exception>
    public async Task SendTweetAsync(
        string message,
        string consumerAPIKey,
        string consumerAPISecret,
        string accessToken,
        string accessTokenSecret)
    {
        // Generate a timestamp and nonce
        var timestamp = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();
        var nonce = Guid.NewGuid().ToString("N");

        // Build the OAuth parameters
        var signatureMethod = "HMAC-SHA1";
        var version = "1.0";
        var signature = GenerateSignature("POST", Url, timestamp, nonce, signatureMethod, consumerAPIKey, consumerAPISecret, accessToken, accessTokenSecret);
        var oauthHeader = "OAuth " +
                          "oauth_consumer_key=\"" + consumerAPIKey + "\", " +
                          "oauth_nonce=\"" + nonce + "\", " +
                          "oauth_signature=\"" + Uri.EscapeDataString(signature) + "\", " +
                          "oauth_signature_method=\"" + signatureMethod + "\", " +
                          "oauth_timestamp=\"" + timestamp + "\", " +
                          "oauth_token=\"" + accessToken + "\", " +
                          "oauth_version=\"" + version + "\"";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", oauthHeader);

        var requestData = new
        {
            text = message,
        };

        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(Url, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
        {
            var errorMsg = $"Error sending tweet: {responseBody}";

            throw new Exception(errorMsg);
        }
    }

    /// <summary>
    /// Generates the OAuth signature from the given parameters.
    /// </summary>
    /// <param name="method">The type of request method.</param>
    /// <param name="url">The URL of the request.</param>
    /// <param name="timestamp">The timestamp of the request.</param>
    /// <param name="nonce"></param>
    /// <param name="signatureMethod"></param>
    /// <param name="consumerKey"></param>
    /// <param name="consumerSecret"></param>
    /// <param name="token"></param>
    /// <param name="tokenSecret"></param>
    /// <returns></returns>
    private static string GenerateSignature(string method, string url, string timestamp, string nonce, string signatureMethod, string consumerKey, string consumerSecret, string token, string tokenSecret)
    {
        // Generate the signature base string
        var signatureBaseString = method.ToUpper() + "&" +
                                  Uri.EscapeDataString(url) + "&" +
                                  Uri.EscapeDataString("oauth_consumer_key=" + consumerKey + "&" +
                                                       "oauth_nonce=" + nonce + "&" +
                                                       "oauth_signature_method=" + signatureMethod + "&" +
                                                       "oauth_timestamp=" + timestamp + "&" +
                                                       "oauth_token=" + token + "&" +
                                                       "oauth_version=1.0");

        // Generate the signing key
        var signingKey = Uri.EscapeDataString(consumerSecret) + "&" + Uri.EscapeDataString(tokenSecret);

        // Compute the signature
        var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
        var signatureBytes = hmac.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString));
        return Convert.ToBase64String(signatureBytes);
    }
}
