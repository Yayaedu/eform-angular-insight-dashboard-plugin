namespace InsightDashboard.Pn.Services.DeviceSyncService;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;

// In-memory pairing-code -> siteId, samme lokal-test-genvej-filosofi som
// DeviceTokenStore. Mønsteret er lånt fra eform-angular-frontends rigtige
// Unit-OTP-flow (Device Users-siden, "New OTP"): en admin genererer en
// kort, tidsbegrænset kode for et site, som tastes ind på enheden. Her
// valideres koden dog lokalt i pluginet i stedet for mod Microtings cloud,
// da insight_app ikke er en Microting-branded enhed og derfor ikke kan
// gennemføre den rigtige Unit-cloud-handshake.
public class DevicePairingCodeStore : IDevicePairingCodeStore
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

    private record PendingCode(int SiteId, DateTime ExpiresAtUtc);

    private readonly ConcurrentDictionary<string, PendingCode> _codes = new();

    public (string Code, DateTime ExpiresAtUtc) GenerateCode(int siteId)
    {
        // At génerere en ny kode for et site ugyldiggør en evt. tidligere
        // udestående kode for samme site — samme adfærd som "New OTP"
        // i eform-angular-frontends Device Users-side.
        foreach (var existing in _codes.Where(kvp => kvp.Value.SiteId == siteId).ToList())
        {
            _codes.TryRemove(existing.Key, out _);
        }

        string code;
        do
        {
            code = GenerateNumericCode();
        } while (_codes.ContainsKey(code));

        var expiresAtUtc = DateTime.UtcNow.Add(CodeLifetime);
        _codes[code] = new PendingCode(siteId, expiresAtUtc);

        return (code, expiresAtUtc);
    }

    public bool TryConsumeCode(string code, out int siteId)
    {
        siteId = 0;

        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        if (!_codes.TryRemove(code, out var pending))
        {
            return false;
        }

        if (pending.ExpiresAtUtc < DateTime.UtcNow)
        {
            return false;
        }

        siteId = pending.SiteId;
        return true;
    }

    private static string GenerateNumericCode()
    {
        // 6 cifre, kryptografisk tilfældig (ikke System.Random) — samme
        // krav man ville stille til en rigtig engangskode.
        const int digits = 6;
        var max = (int)Math.Pow(10, digits);
        var value = RandomNumberGenerator.GetInt32(max);
        return value.ToString(new string('0', digits));
    }
}

public interface IDevicePairingCodeStore
{
    (string Code, DateTime ExpiresAtUtc) GenerateCode(int siteId);
    bool TryConsumeCode(string code, out int siteId);
}
