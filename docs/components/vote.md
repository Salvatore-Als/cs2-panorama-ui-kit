# Vote

The screen of a vote: a question, choices with live counts and percentages, and a countdown. It takes the mouse while open.

The kit only draws what it is given and reports clicks. It keeps no tally: who voted for what, when the vote ends and who wins are yours.

Bundled layout: `uikit_vote`. Demo command: `css_uikit_demo_vote`.

## Declare the layout

```csharp
static readonly VoteLayout Votes = new() { Name = "uikit_vote" };
```

| Property | Default | Meaning |
|---|---|---|
| `Choices` | `5` | Choices per page. A longer vote is paged. |

## Run a vote

The loop: show the vote, record each click in `OnVote`, redraw everyone with `UpdateAll`, hide it when it is over.

```csharp
string[] maps = ["de_dust2", "de_mirage", "de_inferno"];
int[] counts = new int[maps.Length];
Dictionary<ulong, int> ballots = new();

VoteOptions Build(int? selected) => new()
{
    Layout = Votes,
    Title = "Next map?",
    Choices = maps.Select((map, i) => new VoteChoice(map, counts[i])).ToList(),
    Selected = selected,
    Seconds = 20,
    OnVote = vote =>
    {
        ulong id = vote.Player.SteamID;
        if (ballots.TryGetValue(id, out int previous)) counts[previous]--;
        ballots[id] = vote.Index;
        counts[vote.Index]++;
        kit.Votes.UpdateAll(Build(null));
        kit.Votes.Update(vote.Player, Build(vote.Index));
    },
};

kit.Votes.ShowAll(Build(null));
AddTimer(20, () => kit.Votes.HideAll(Votes));
```

| Option | Default | Meaning |
|---|---|---|
| `Title` | required | The question. |
| `Choices` | required | `VoteChoice(Label, Votes, Sub, Style)`. Percentages and the total count every choice, on every page. |
| `Kicker` | none | Small line above the title. |
| `Selected` | none | Index of the choice this viewer picked, highlighted. |
| `Seconds` | `0` | Countdown, ticked by the kit from `Show`. At zero, clicks stop. `Update` does not restart it. |
| `Closable` | `true` | `false` hides the close button: close it yourself when the vote is over. |
| `Locked` | `false` | Shows the choices but takes no click. |
| `TimeFormat` / `TotalFormat` | `"{0}s"` / `"{0} votes"` | Formats of the countdown and the total. |
| `OnVote` | none | A click on a choice, on the server thread. `VoteEvent.Index` is the position in `Choices`. |
| `OnClose` | none | Called once when the screen leaves this player. The vote itself goes on. |

## Other calls

- `kit.Votes.Show(player, options)` opens it for one player, or redraws it if it is already open.
- `kit.Votes.Update(player, options)` redraws an open vote. It does nothing for a player who closed it.
- `kit.Votes.Hide(player, layout)` and `kit.Votes.HideAll(layout)` close it.

See [Common concepts](README.md#common-concepts) for `Style`, `Texts` and `Classes`.
