using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Comms;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    /// <summary>Who each bundled face is in the demo, with a line and a tint.</summary>
    private static readonly Dictionary<string, (string Name, string Text, string Style)> CommsVoices = new()
    {
        [CommsFaces.Warden] = ("THE WARDEN", "Nobody leaves this island. The zone closes in sixty seconds - run, little ones.", UiStyle.Danger),
        [CommsFaces.Jackal] = ("JACKAL", "I can see three of you from up here. Two of you will not make it to the next circle.", UiStyle.Warn),
        [CommsFaces.Doctor] = ("THE DOCTOR", "Your medkits are running low. I have plenty. Come and take them.", UiStyle.Success),
        [CommsFaces.Oracle] = ("ORACLE", "The storm remembers every step you take. Choose the next one carefully.", UiStyle.Purple),
        [CommsFaces.Villain] = ("THE BOSS", "You took my island. Now I take it back - one circle at a time.", UiStyle.Danger),
        [CommsFaces.Unknown] = ("UNKNOWN SIGNAL", "...can anyone hear me? The supply drop is a trap. Repeat: it is a trap...", UiStyle.Info),
    };

    /// <summary>
    /// A random villain on a radio transmission, or the face named, with the text given. Glitches harder on
    /// the warden. Needs the addon with the faces.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_comms", "Villain radio transmission.")]
    [CommandHelper(usage: "[face] [text...]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoComms(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        string face = CommsFaces.All[Random.Shared.Next(CommsFaces.All.Count)];
        if (command.ArgCount > 1)
        {
            face = command.GetArg(1).ToLowerInvariant();
        }

        if (!CommsVoices.TryGetValue(face, out (string Name, string Text, string Style) voice))
        {
            voice = CommsVoices[CommsFaces.Unknown];
        }

        string text = voice.Text;
        if (command.ArgCount > 2)
        {
            text = string.Join(' ', Enumerable.Range(2, command.ArgCount - 2).Select(command.GetArg));
        }

        command.ReplyToCommand($"[uikit] comms: {face}");
        kit.Comms.Show(target, new CommsOptions
        {
            Layout = DemoLayouts.Comms,
            Face = face,
            Name = voice.Name,
            Text = text,
            Footer = "SIGNAL 87%  //  ENCRYPTED",
            Style = voice.Style,
            Glitch = face == CommsFaces.Warden,
            Seconds = 7f,
        });
    }
}
