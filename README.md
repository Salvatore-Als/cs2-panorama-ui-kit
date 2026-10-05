# Panorama UI Kit

**Version 1.0.0.** I built this out of necessity: I needed UI for my own projects. It turned out to be practical enough to share.

Shared Panorama HUD components for [CounterStrikeSharp](https://docs.cssharp.dev/) plugins. Built with [PanoramaManager](https://github.com/Next-il/PanoramaManager).

The `PanoramaUiKit` plugin runs on the server and exposes an API. Your plugins call it to show toasts, menus, votes and more. Every call names the layout file that draws the component. The bundled ones are `uikit_*`.

## Your own style

You can ship your own look. Colours, spacing and animation are yours, as long as the layout follows the contract of its component: the same panel ids, the same `{s:...}` text slots and the same classes. The calls do not change.

Each contract, generated from `contracts/*.json`: [docs/contracts](docs/contracts/README.md).

## Validate a layout

A layout names its contract in a comment, for example `<!-- uikit:toast placements=tc,bl depth=3 -->`. From `tools/`:

```bash
python3 -m uikit check ../panorama/layout/custom_game/neon_toast.xml
python3 -m uikit check-all
```

`check` validates one file. `check-all` validates every layout under `panorama/layout/custom_game` that carries a marker. `./build.sh` runs `check-all` before compiling. A missing id or a wrong `{s:...}` slot fails the check.

## Components


| Component                                    | What it is                                       |
| -------------------------------------------- | ------------------------------------------------ |
| [Toast](docs/components/toast.md)            | Stacked notifications that dismiss themselves    |
| [Banner](docs/components/banner.md)          | One sticky line across the top                   |
| [Announce](docs/components/announce.md)      | Big centred card: reveal, objective, round intro |
| [Ability bar](docs/components/abilities.md)  | Ability tiles with cooldown and hold gauges      |
| [Match bar](docs/components/match.md)        | Top status bar: two sides and a timer            |
| [Kill feed](docs/components/killfeed.md)     | Feed of short events, newest on top              |
| [Victory screen](docs/components/victory.md) | End-of-round screen with chips and a countdown   |
| [Roulette](docs/components/roulette.md)      | Case-opening strip on a weighted draw            |
| [Vote](docs/components/vote.md)              | Question, choices with live counts, countdown    |
| [Panel](docs/components/panel.md)            | Modal with pages and typed rows                  |
| [Menu](docs/components/menu.md)              | Paginated menu with sub-menus                    |


Concepts shared by every component: [docs/components](docs/components/README.md#common-concepts).

## Install

```bash
./build.sh
```

Copy the content of `dist/` onto `addons/counterstrikesharp/`:

- `plugins/PanoramaUiKit/`: the plugin
- `plugins/PanoramaUiKit.Demo/`: demo commands, optional
- `shared/PanoramaUiKit.Shared/`: the API DLL, once for the whole server

A Workshop addon with the default UI kit already exists: [Panorama UI Kit](https://steamcommunity.com/sharedfiles/filedetails/?id=3813908138). It is waiting for Steam validation. Once it is public, mount it on the server to use the bundled `uikit_*` layouts.

To ship your own look, compile the `panorama/` folder into a Workshop addon. Keep the paths `panorama/layout/custom_game` and `panorama/styles/custom_game`.

Requires CounterStrikeSharp 1.0.376 or newer, and [PanoramaManager](https://github.com/Next-il/PanoramaManager). The kit plugin depends on it and ships `PanoramaManager.dll` next to itself. A plugin that only calls the kit references `PanoramaUiKit.Shared`, not PanoramaManager.

## Use

The demo plugin is a consumer: it references only `PanoramaUiKit.Shared` ([csproj](src/PanoramaUiKit.Demo/PanoramaUiKit.Demo.csproj)), resolves the capability and preloads every layout ([DemoPlugin.cs](src/PanoramaUiKit.Demo/DemoPlugin.cs)), and names each layout file ([DemoLayouts.cs](src/PanoramaUiKit.Demo/DemoLayouts.cs)). One file per component:


| Component      | Demo                                                                         | Command                    |
| -------------- | ---------------------------------------------------------------------------- | -------------------------- |
| Toast          | [DemoPlugin.Toast.cs](src/PanoramaUiKit.Demo/UI/DemoPlugin.Toast.cs)         | `css_uikit_demo_toast`     |
| Banner         | [DemoPlugin.Banner.cs](src/PanoramaUiKit.Demo/UI/DemoPlugin.Banner.cs)       | `css_uikit_demo_banner`    |
| Announce       | [DemoPlugin.Announce.cs](src/PanoramaUiKit.Demo/UI/DemoPlugin.Announce.cs)   | `css_uikit_demo_announce`  |
| Ability bar    | [DemoPlugin.Abilities.cs](src/PanoramaUiKit.Demo/UI/DemoPlugin.Abilities.cs) | `css_uikit_demo_abilities` |
| Match bar      | [DemoPlugin.Match.cs](src/PanoramaUiKit.Demo/UI/DemoPlugin.Match.cs)         | `css_uikit_demo_match`     |
| Kill feed      | [DemoPlugin.KillFeed.cs](src/PanoramaUiKit.Demo/UI/DemoPlugin.KillFeed.cs)   | `css_uikit_demo_killfeed`  |
| Victory screen | [DemoPlugin.Victory.cs](src/PanoramaUiKit.Demo/UI/DemoPlugin.Victory.cs)     | `css_uikit_demo_victory`   |
| Roulette       | [DemoPlugin.Roulette.cs](src/PanoramaUiKit.Demo/UI/DemoPlugin.Roulette.cs)   | `css_uikit_demo_roulette`  |
| Vote           | [DemoPlugin.Vote.cs](src/PanoramaUiKit.Demo/UI/DemoPlugin.Vote.cs)           | `css_uikit_demo_vote`      |
| Panel          | [DemoPlugin.Panel.cs](src/PanoramaUiKit.Demo/UI/DemoPlugin.Panel.cs)         | `css_uikit_demo_panel`     |
| Menu           | [DemoPlugin.Menu.cs](src/PanoramaUiKit.Demo/UI/DemoPlugin.Menu.cs)           | `css_uikit_demo_menu`      |




## Todo

- [ ] Improve the roulette design. It is quite slow and ugly.
- [ ] Improve the ability bar design.
- [ ] Support images where they are useful: panel, menu, kill feed, and others.
- [ ] Support sounds where they are useful: a panel click, a button click, and others.
- [ ] Add more components: a progress bar, a compass, and anything else that would be useful.
- [ ] Add one image per component on the repo to show the result, or a video.
- [ ] Add Claude skills for creating and working with UI kit layouts.



## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).