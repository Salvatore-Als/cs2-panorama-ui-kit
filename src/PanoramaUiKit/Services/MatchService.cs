using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Match;

namespace PanoramaUiKit.Services;

internal sealed class MatchService(LayoutRegistry registry) : ComponentService<MatchChannel, MatchLayout>(registry), IMatchService
{
    public void Update(MatchOptions options) => WhenWarm(Channel(options.Layout), channel => channel.Update(options));

    public void Hide(MatchLayout? layout = null)
    {
        foreach (MatchChannel channel in Targets(layout))
        {
            channel.Hide();
        }
    }
}
