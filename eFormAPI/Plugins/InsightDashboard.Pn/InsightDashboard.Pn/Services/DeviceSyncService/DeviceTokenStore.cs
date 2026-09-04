namespace InsightDashboard.Pn.Services.DeviceSyncService;

using System;
using System.Collections.Concurrent;

// In-memory token -> siteId. Bevidst ikke DB-baseret: dette er en
// lokal-test-genvej, ikke den rigtige enhedsparring (se DeviceModels.cs).
// Nulstilles ved backend-genstart — det er fint til formålet.
public class DeviceTokenStore : IDeviceTokenStore
{
    private readonly ConcurrentDictionary<string, int> _tokenToSiteId = new();

    public string CreateToken(int siteId)
    {
        var token = Guid.NewGuid().ToString("N");
        _tokenToSiteId[token] = siteId;
        return token;
    }

    public bool TryGetSiteId(string token, out int siteId)
    {
        return _tokenToSiteId.TryGetValue(token ?? string.Empty, out siteId);
    }
}

public interface IDeviceTokenStore
{
    string CreateToken(int siteId);
    bool TryGetSiteId(string token, out int siteId);
}
