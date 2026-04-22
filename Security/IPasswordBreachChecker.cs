namespace PokeBuilder.Server.Security;

/// <summary>
/// Returns true if the password appears in a known breach corpus (e.g. HIBP k-anonymity API).
/// </summary>
public interface IPasswordBreachChecker
{
    /// <summary>
    /// When the remote check fails (network, timeout), returns <c>false</c> so registration is not blocked.
    /// </summary>
    Task<bool> IsBreachedAsync(string password, CancellationToken cancellationToken = default);
}
