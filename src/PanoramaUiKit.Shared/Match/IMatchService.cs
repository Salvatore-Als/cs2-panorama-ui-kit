namespace PanoramaUiKit.Shared.Match;

/// <summary>
/// Server-wide top bar. <see cref="Update"/> shows it to every human and is safe every tick: text is
/// only written when it changes, and anyone who does not have the bar yet (late joiner, round
/// restart) gets it on the next call.
/// </summary>
public interface IMatchService
{
    void Update(MatchOptions options);

    /// <summary>Hides for everyone until the next Update. Null layout = every match layout.</summary>
    void Hide(MatchLayout? layout = null);
}
