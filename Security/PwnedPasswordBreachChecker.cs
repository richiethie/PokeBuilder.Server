using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace PokeBuilder.Server.Security;

/// <summary>
/// k-anonymity check against Have I Been Pwned (SHA-1 range API). Does not send the full password.
/// </summary>
public sealed class PwnedPasswordBreachChecker(
    IHttpClientFactory httpClientFactory,
    ILogger<PwnedPasswordBreachChecker> logger) : IPasswordBreachChecker
{
    public const string HttpClientName = "PwnedPasswords";

    private const string RangeUrl = "https://api.pwnedpasswords.com/range/";

    public async Task<bool> IsBreachedAsync(string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);

            var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes(password));
            var hex = Convert.ToHexString(sha1);
            var prefix = hex[..5];
            var suffix = hex[5..];

            using var request = new HttpRequestMessage(HttpMethod.Get, RangeUrl + prefix);
            request.Headers.TryAddWithoutValidation("Add-Padding", "true");

            using var response = await client.SendAsync(request, cancellationToken);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                logger.LogWarning("Pwned password API returned {Status}. Skipping breach check.", response.StatusCode);
                return false;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            foreach (var line in body.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var colon = line.IndexOf(':');
                if (colon <= 0) continue;
                var lineSuffix = line[..colon];
                if (string.Equals(lineSuffix, suffix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Pwned password check failed; allowing password.");
            return false;
        }
    }
}
