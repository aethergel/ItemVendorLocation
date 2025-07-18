using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Interface.Components;
using Lumina.Excel.Sheets;
using System.Linq;
using Dalamud.Game.ClientState.Keys;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Dalamud.Interface.Style;
using Dalamud.Interface.Colors;

namespace ItemVendorLocation.GUI;

public class SettingsWindow : Window
{
    public SettingsWindow() : base("Item Vendor Location 설정")
    {
        RespectCloseHotkey = true;

        SizeCondition = ImGuiCond.FirstUseEver;
        Size = new Vector2(740, 200);
    }

    public override void Draw()
    {
#if DEBUG
        ImGui.SetNextItemWidth(200f);
        var num = Service.Configuration.BuildDebugVendorInfo;
        if (ImGui.InputInt("NPC ID", ref num))
        {
            Service.Configuration.BuildDebugVendorInfo = num;
            Service.Configuration.Save();
        }
        if (ImGui.Button("Build Debug Vendor Info"))
        {
            Service.Plugin.ItemLookup.BuildDebugVendorInfo((uint)num);
        }
        ImGui.SameLine();
        if (ImGui.Button("Build NPC location"))
        {
            Service.Plugin.ItemLookup.BuildDebugNpcLocation((uint)num);
        }
#endif
        var filterDuplicates = Service.Configuration.FilterDuplicates;
        if (ImGui.Checkbox("중복 필터", ref filterDuplicates))
        {
            Service.Configuration.FilterDuplicates = filterDuplicates;
            Service.Configuration.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker(@"각 지역마다 중복되는 상인은 제외합니다");

        var filterGCResults = Service.Configuration.FilterGCResults;
        if (ImGui.Checkbox("총사령부 필터", ref filterGCResults))
        {
            Service.Configuration.FilterGCResults = filterGCResults;
            Service.Configuration.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker(@"소속되어있는 총사령부 상인만 표시합니다");

        var filterNPCsWithNoLocation = Service.Configuration.FilterNPCsWithNoLocation;
        if (ImGui.Checkbox("위치 필터", ref filterNPCsWithNoLocation))
        {
            Service.Configuration.FilterNPCsWithNoLocation = filterNPCsWithNoLocation;
            Service.Configuration.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker(@"위치 좌표가 존재하는 상인만 표시합니다");

        var showShopName = Service.Configuration.ShowShopName;
        if (ImGui.Checkbox("상점 정보 표시", ref showShopName))
        {
            Service.Configuration.ShowShopName = showShopName;
            Service.Configuration.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker(@"상점 정보를 표시합니다 / 예: '방어구 구입(마법사) - 방어구 구입 (레벨 20~29)'");

        var highlightSelectedNpc = Service.Configuration.HighlightSelectedNpc;
        if (ImGui.Checkbox("선택한 상인 표시", ref highlightSelectedNpc))
        {
            Service.Configuration.HighlightSelectedNpc = highlightSelectedNpc;
            Service.Framework.Run(() => Service.HighlightObject.ToggleHighlight(highlightSelectedNpc));
            Service.Configuration.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker(@"검색한 항목을 판매하는 상인을 테두리로 표시합니다");
        ImGui.SameLine();
        var highlightColorNames = Enum.GetNames<ObjectHighlightColor>();
        var highlightColorValues = Enum.GetValues<ObjectHighlightColor>();
        var selectedHighlightColor = Array.IndexOf(highlightColorValues, Service.Configuration.HighlightColor);
        ImGui.SetNextItemWidth(150f);
        if (ImGui.Combo("표시 색상", ref selectedHighlightColor, highlightColorNames, highlightColorNames.Length))
        {
            Service.Configuration.HighlightColor = (ObjectHighlightColor)selectedHighlightColor;
            Service.Configuration.Save();
        }

        var highlightMenuSelections = Service.Configuration.HighlightMenuSelections;
        if (ImGui.Checkbox("선택 메뉴 표시", ref highlightMenuSelections))
        {
            Service.Configuration.HighlightMenuSelections = highlightMenuSelections;
            Service.Configuration.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker(@"선택 메뉴를 색으로 표시해 아이템을 찾기 쉽도록 만듭니다.

참조: 상점 창을 열어둔 채로 또 다른 검색을 할 경우 제대로 작동하지 않을 수 있습니다.");
        ImGui.SameLine();
        // this part seems dumb to me, but it works
        var selectedShopHighlightColor = Service.Configuration.ShopHighlightColor;
        ImGui.SetNextItemWidth(150f);
        selectedShopHighlightColor = ImGuiComponents.ColorPickerWithPalette(1, "표시 색상", selectedShopHighlightColor, ImGuiColorEditFlags.NoAlpha);
        if (selectedShopHighlightColor != Service.Configuration.ShopHighlightColor)
        {
            Service.Configuration.ShopHighlightColor = selectedShopHighlightColor;
            Service.Configuration.Save();
        }

        ImGui.SetNextItemWidth(200f);
        int maxSearchResults = Service.Configuration.MaxSearchResults;
        if (ImGui.InputInt("최대 검색결과 개수", ref maxSearchResults))
        {
            if (maxSearchResults is <= 50 and >= 1)
            {
                Service.Configuration.MaxSearchResults = (ushort)maxSearchResults;
                Service.Configuration.Save();
            }
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker(@"명령어를 사용해 검색했을때 표시할 검색결과의 최대 개수를 설정합니다.
최대로 설정할 수 있는 값은 50 입니다.");

        var resultsViewTypeNames = Enum.GetNames<ResultsViewType>();
        var resultsViewTypeValues = Enum.GetValues<ResultsViewType>();
        var selectedResultsViewType = Array.IndexOf(resultsViewTypeValues, Service.Configuration.ResultsViewType);
        ImGui.SetNextItemWidth(200f);
        if (ImGui.Combo("검색결과 표시 유형", ref selectedResultsViewType, resultsViewTypeNames, resultsViewTypeNames.Length))
        {
            Service.Configuration.ResultsViewType = resultsViewTypeValues[selectedResultsViewType];
            Service.Configuration.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker(@"플러그인이 상인 위치를 표시할 방식입니다.

Single은 하나의 결과만 메시지 창에 표시합니다.
Multiple은 팝업 창에 결과 목록을 표시합니다.");

        var uiColors = Service.DataManager.GetExcelSheet<UIColor>().DistinctBy(i => i.ClassicFF).ToList();
        int npcNameChatColor = Service.Configuration.NPCNameChatColor;
        ImGui.SetNextItemWidth(200f);
        // my lame way to allow selection of colors as defined in the UIColor sheet
        if (ImGui.BeginCombo("상인 이름 메시지 색상", ""))
        {
            foreach (var color in uiColors)
            {
                var isChecked = Service.Configuration.NPCNameChatColor == color.RowId;
                var reversedColors = ImGui.ColorConvertU32ToFloat4(color.ClassicFF);
                // Seems like the above function reverses the order of the bytes
                // There's got to be a better way to do this, but brain no working :P
                Vector4 correctColors = new()
                {
                    X = reversedColors.W,
                    Y = reversedColors.Z,
                    Z = reversedColors.Y,
                    W = reversedColors.X,
                };
                if (ImGui.Checkbox($"###{color.RowId}", ref isChecked))
                {
                    Service.Configuration.NPCNameChatColor = (ushort)uiColors.Find(i => i.ClassicFF == ImGui.ColorConvertFloat4ToU32(reversedColors)).RowId;
                    Service.Configuration.Save();
                }
                ImGui.SameLine();
                _ = ImGui.ColorEdit4($"", ref correctColors, ImGuiColorEditFlags.None | ImGuiColorEditFlags.NoInputs);
            }
            ImGui.EndCombo();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker(@"/pvendor 명령어를 사용해 검색했을때 메시지에서 상인 이름을 표시할 색상을 변경합니다.");

        var keyNames = Service.KeyState.GetValidVirtualKeys().Select(i => i.GetFancyName()).ToArray();
        keyNames = [.. keyNames.Prepend("None")];
        var keyValues = Service.KeyState.GetValidVirtualKeys().ToArray();
        keyValues = [.. keyValues.Prepend(VirtualKey.NO_KEY)];
        var selectedKey = Array.IndexOf(keyValues, Service.Configuration.SearchDisplayModifier);
        ImGui.SetNextItemWidth(200f);
        if (ImGui.Combo("검색결과 표시 유형 조합 키", ref selectedKey, keyNames, keyNames.Length))
        {
            Service.Configuration.SearchDisplayModifier = keyValues[selectedKey];
            Service.Configuration.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker(@"누르고 있는동안 검색결과 표시 유형을 변경합니다.");
    }
}