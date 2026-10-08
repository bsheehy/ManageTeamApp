namespace TeamManage.App.Services;

/// <summary>
/// Thrown when an API call returns a non-success status code, carrying a
/// user-presentable message extracted from the response body where possible.
/// </summary>
public class ApiException : Exception
{
    public ApiException(string message) : base(message)
    {
    }
}
