using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace YukiSubmarineCalc.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin)
        : base("Yuki's Submarine Money", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        long sum = 0;
        foreach (var itemsCounts in plugin.Configuration.CharacterItems.Values)
        {
            foreach (var itemCount in itemsCounts)
            {
                var item = Plugin.DataManager.GetExcelSheet<Item>().GetRowOrDefault(itemCount.Key);
                if (item.HasValue)
                {
                    sum += item.Value.PriceLow * itemCount.Value;
                }
            }
        }
        ImGui.TextUnformatted("Total value:");
        ImGui.SameLine();
        using (ImRaii.PushColor(ImGuiCol.Text, new Vector4(0, 1, 0,1)))
            ImGui.Text(sum.ToString());
    }
}
