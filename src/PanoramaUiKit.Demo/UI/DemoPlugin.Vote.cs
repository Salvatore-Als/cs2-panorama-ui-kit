using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Votes;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private static readonly string[] DemoVoteLabels = ["Choice A", "Choice B", "Choice C", "Choice D", "Choice E", "Choice F", "Choice G"];

    private static readonly string[] DemoVoteStyles =
    [UiStyle.Info, UiStyle.Success, UiStyle.Warn, UiStyle.Danger, UiStyle.Purple, UiStyle.Gold, UiStyle.Neutral];

    private bool _demoClosable = true;

    /// <summary>Votes of the demo: what a mode would keep on its side. The kit only draws them.</summary>
    private readonly int[] _demoVotes = new int[DemoVoteLabels.Length];

    private int _demoPick = -1;

    /// <summary>
    /// The screen of a vote for the caller: seven choices over two pages, 12 seconds. The tally is the demo's own (a click
    /// moves your vote, three other voters pick at random every second): the kit draws it and reports the
    /// click. Click a choice, the arrows to change page, the cross to close
    /// the screen. <c>noclose</c>: no close button, the demo closes the screen itself 3 seconds after the end.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_vote", "Vote screen for the caller: seven choices, 12 s, live counts. Argument: noclose.")]
    [CommandHelper(usage: "[noclose]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoVote(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        Array.Clear(_demoVotes);
        _demoPick = -1;
        _demoClosable = !command.GetArg(1).Equals("noclose", StringComparison.OrdinalIgnoreCase);

        kit.Votes.Show(target, BuildDemoVote(kit));

        int left = 12;
        RunEverySecond("vote", () =>
        {
            left--;
            if (left <= 0 || !target.IsValid)
            {
                if (!_demoClosable)
                {
                    Later(3f, () => kit.Votes.Hide(target, DemoLayouts.Vote));
                }

                return false;
            }

            _demoVotes[Random.Shared.Next(_demoVotes.Length)]++;
            kit.Votes.Update(target, BuildDemoVote(kit));
            return true;
        });
    }

    private VoteOptions BuildDemoVote(IPanoramaUiKit kit)
    {
        List<VoteChoice> choices = [];
        for (int i = 0; i < DemoVoteLabels.Length; i++)
        {
            choices.Add(new VoteChoice(DemoVoteLabels[i], _demoVotes[i], $"Sub line {i + 1}", DemoVoteStyles[i]));
        }

        return new VoteOptions
        {
            Layout = DemoLayouts.Vote,
            Kicker = "Vote",
            Title = "Which one next?",
            Choices = choices,
            Selected = PickOrNull(),
            Seconds = 12f,
            Closable = _demoClosable,
            Style = UiStyle.Gold,
            OnVote = vote =>
            {
                // A mode would record this against the player; the demo keeps a single vote.
                if (_demoPick >= 0)
                {
                    _demoVotes[_demoPick]--;
                }

                _demoPick = vote.Index;
                _demoVotes[vote.Index]++;
                kit.Votes.Update(vote.Player, BuildDemoVote(kit));
            },
        };
    }

    private int? PickOrNull()
    {
        if (_demoPick < 0)
        {
            return null;
        }

        return _demoPick;
    }
}
