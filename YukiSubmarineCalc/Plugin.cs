using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Inventory;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using YukiSubmarineCalc.Windows;

namespace YukiSubmarineCalc;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IGameInventory GameInventory { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandName = "/yuki";
    private static readonly uint[] ItemIds = [22500, 22501, 22502, 22503, 22504, 22505, 22506, 22507];
    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("YukiSubmarineCalc");
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();


        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Show main window"
        });

        // Tell the UI system that we want our windows to be drawn through the window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
        
        ClientState.Login += OnLogin;
        GameInventory.InventoryChanged += InventoryChanged;
        UpdateCurrentCharacter();

    }


    private void UpdateCurrentCharacter()
    {
        if (!ClientState.IsLoggedIn || PlayerState.ContentId == 0)
        {
            Log.Debug("Character not logged in");
            return;
        }
        Dictionary<uint, int> itemCounts = [];
        var filteredItemEnumerator = GameInventory.GetInventoryItems(GameInventoryType.Inventory1).ToArray()
            .Concat(GameInventory.GetInventoryItems(GameInventoryType.Inventory2).ToArray())
            .Concat(GameInventory.GetInventoryItems(GameInventoryType.Inventory3).ToArray())
            .Concat(GameInventory.GetInventoryItems(GameInventoryType.Inventory4).ToArray())
            .Where(item => ItemIds.Contains(item.ItemId)).AsEnumerable();
        foreach (var gameInventoryItem in filteredItemEnumerator)
        {
            itemCounts[gameInventoryItem.ItemId] = itemCounts.GetValueOrDefault(gameInventoryItem.ItemId) + gameInventoryItem.Quantity;
        }
        Configuration.CharacterItems[PlayerState.ContentId] = itemCounts;
        Configuration.Save();
        Log.Debug($"Updated {PlayerState.CharacterName} inventory");
    }
    
    private void InventoryChanged(IReadOnlyCollection<InventoryEventArgs> events)
    {
        UpdateCurrentCharacter();
    }

    private void OnLogin()
    {
        UpdateCurrentCharacter();
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anything during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        ClientState.Login -= OnLogin;
        GameInventory.InventoryChanged -= InventoryChanged;
        
        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        if (args == "clear")
        {
            Configuration.CharacterItems = [];
            Configuration.Save();
            UpdateCurrentCharacter();
        }
        else
        {
            // In response to the slash command, toggle the display status of our main ui
            MainWindow.Toggle();
        }
    }
    
    public void ToggleMainUi() => MainWindow.Toggle();
}
