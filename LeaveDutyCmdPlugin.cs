using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using static Dalamud.Game.Command.CommandInfo;
using static Dalamud.Game.Framework;

namespace LeaveDutyCmdPlugin;

internal class LeaveDutyCmdPlugin : IDalamudPlugin, IDisposable
{
    private delegate void LeaveDutyDelegate(char is_timeout);

    private const string commandName = "/ql";

    private LeaveDutyDelegate leaveDungeon;

    private readonly AddressResolver AddressResolver;

    private bool requesting = false;

    public string Name => "DutyHop";

    [PluginService]
    private static DalamudPluginInterface Interface { get; set; }

    [PluginService]
    private static CommandManager CommandManager { get; set; }

    [PluginService]
    private static Framework Framework { get; set; }

    public LeaveDutyCmdPlugin()
    {
        
        AddressResolver = new AddressResolver();
        ((BaseAddressResolver)AddressResolver).Setup();
        leaveDungeon = Marshal.GetDelegateForFunctionPointer<LeaveDutyDelegate>(AddressResolver.LeaveDuty);
        Framework.Update += new OnUpdateDelegate(OnFrameworkUpdate);
        CommandManager.AddHandler("/ql", new CommandInfo(new HandlerDelegate(OnCommand))
        {
            HelpMessage = "Immediately leave duty without confirmation window."
        });
    }

    public void Dispose()
    {
        
        CommandManager.RemoveHandler("/ql");
        Framework.Update -= new OnUpdateDelegate(OnFrameworkUpdate);
    }

    private void OnCommand(string command, string args)
    {
        requesting = true;
    }

    private void OnFrameworkUpdate(Framework framework)
    {
        try
        {
            if (requesting)
            {
                leaveDungeon('\0');
                requesting = false;
            }
        }
        catch (Exception ex)
        {
            PluginLog.LogError(ex.Message, Array.Empty<object>());
        }
    }
}
