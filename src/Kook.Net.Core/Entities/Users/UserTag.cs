using System.Diagnostics;

namespace Kook;

/// <summary>
///     Representing a tag an <see cref="IUser"/> can have.
/// </summary>
[DebuggerDisplay(@"{DebuggerDisplay,nq}")]
public class UserTag : IEquatable<UserTag>
{
    /// <summary>
    ///     Gets the color of the tag given to user.
    /// </summary>
    /// <returns>
    ///     A <see cref="Color"/> struct representing the color of this tag.
    /// </returns>
    public Color Color { get; }
    /// <summary>
    ///     Gets the text of the tag given to user.
    /// </summary>
    /// <returns>
    ///     A <see langword="string"/> representing the text of this tag.
    /// </returns>
    public string Text { get; }

    private UserTag(Color color, string text)
    {
        Color = color;
        Text = text;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserTag"/> class.
    /// </summary>
    /// <param name="color">
    ///     The color of the tag given to user.
    /// </param>
    /// <param name="text">
    ///     The text of the tag given to user.
    /// </param>
    /// <returns>
    ///     A <see cref="UserTag"/> representing the given parameters.
    /// </returns>
    public static UserTag Create(Color color, string text) => new UserTag(color, text);

    private string DebuggerDisplay => Text;

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(UserTag other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Text == other.Text;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((UserTag) obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return (Text != null ? Text.GetHashCode() : 0);
    }
    
    #endregion
    
}