# Contributing

Every contribution goes through a pull request. Open work is listed in the [Todo](README.md#todo) of the README.

## Before opening the pull request

```bash
./build.sh
```

This checks every layout against its contract, then builds. A layout that does not follow its contract fails the build.

Contract rules: [docs/contracts](docs/contracts/README.md). Do not edit `docs/contracts/*.md` or `docs/CONTRACTS.md` by hand. They are generated:

```bash
cd tools && python3 -m uikit docs
```

## Design

A design change, or a new component that comes with a design, has to be testable in game without rebuilding the layouts locally.

Publish the layouts on a **public Workshop item**. The item must be mountable on a server, so the layouts can be tried directly. Put the Workshop addon ID in the pull request, with a link to the item.

The layout still has to follow its component's contract: the same panel ids, the same `{s:...}` text slots and the same classes. Colours, spacing and animation are yours. A screenshot or a short video in the pull request helps, but the Workshop item is what makes the change testable.

## A new component

A component is more than a layout. The pull request needs:

- a contract in `contracts/<name>.json`
- the layout and the stylesheet under `panorama/`
- the C# API in `PanoramaUiKit.Shared`, implemented by the plugin
- a demo command in `PanoramaUiKit.Demo`, and the layout named in `DemoLayouts.cs`
- the generated contract page (`python3 -m uikit docs`) and a page in `docs/components/`

The same Workshop rule applies: the design has to be on a public Workshop item, and the pull request has to give its addon ID.