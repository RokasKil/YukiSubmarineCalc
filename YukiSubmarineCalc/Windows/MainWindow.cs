using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace YukiSubmarineCalc.Windows;

public class MainWindow : Window, IDisposable
{
    private static readonly Vector4 Red = new Vector4(1, 0, 0, 1);
    private static readonly Vector4 Green = new Vector4(0, 1, 0, 1);
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin)
        : base("Yuki's Submarine Money", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.TextUnformatted("Total value:");
        ImGui.SameLine();
        using (ImRaii.PushColor(ImGuiCol.Text, plugin.GilSum == 0 ? Red : Green))
            ImGui.Text($"{plugin.GilSum:n0}{SeIconChar.Gil.ToIconString()}");
    }
}
