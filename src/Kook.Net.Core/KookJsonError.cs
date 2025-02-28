using System.Collections.Immutable;

namespace Kook;

/// <summary>
///     Represents a generic parsed json error received from Kook after performing a rest request.
/// </summary>
public struct KookJsonError
{
    /// <summary>
    ///     Gets the json path of the error.
    /// </summary>
    public string Path { get; }

    /// <summary>
    ///     Gets a collection of errors associated with the specific property at the path.
    /// </summary>
    public IReadOnlyCollection<KookError> Errors { get; }

    internal KookJsonError(string path, KookError[] errors)
    {
        Path = path;
        Errors = errors.ToImmutableArray();
    }
}

/// <summary>
///     Represents an error with a property.
/// </summary>
public struct KookError
{
    /// <summary>
    ///     Gets the code of the error.
    /// </summary>
    public string Code { get; }

    /// <summary>
    ///     Gets the message describing what went wrong.
    /// </summary>
    public string Message { get; }

    internal KookError(string code, string message)
    {
        Code = code;
        Message = message;
    }
}