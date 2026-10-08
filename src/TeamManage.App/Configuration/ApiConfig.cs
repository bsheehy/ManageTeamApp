namespace TeamManage.App.Configuration;

/// <summary>
/// Base API configuration. The default addresses target each platform's
/// loopback alias for a locally running TeamManage.Api during development
/// (Android emulator cannot reach "localhost" directly; it must use 10.0.2.2).
/// For a real device or production deployment, replace this with your
/// deployed API's HTTPS URL.
/// </summary>
public static class ApiConfig
{
    public static string BaseAddress =>
#if ANDROID
        "https://10.0.2.2:7060/";
#elif IOS
        "https://localhost:7060/";
#elif WINDOWS
        "https://localhost:7060/";
#else
        "https://localhost:7060/";
#endif
}
