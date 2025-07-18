using System.Linq;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ItemVendorLocation.Models;
using System.Numerics;
using Dalamud.Interface.Components;
using Dalamud.Interface.ImGuiNotification;

namespace ItemVendorLocation.GUI;

public class VendorResultsWindow : Window
{
    private ItemInfo _itemToDisplay;

    public VendorResultsWindow() : base("Item Vendor Location")
    {
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new(409, 120),
            MaximumSize = new(-1, -1),
        };
    }

    private void DrawTableRow(NpcInfo npcInfo, string shopName, NpcLocation location, string costStr)
    {
        ImGui.TableNextRow();
        _ = ImGui.TableNextColumn();
#if DEBUG
        ImGui.Text(npcInfo.Id.ToString());
        _ = ImGui.TableNextColumn();
#endif
        ImGui.Text(npcInfo.Name);
        _ = ImGui.TableNextColumn();
        if (Service.Configuration.ShowShopName && _itemToDisplay.HasShopNames())
        {
            ImGui.Text(shopName ?? "");
            _ = ImGui.TableNextColumn();
        }

        if (location != null)
        {
            if (location.TerritoryType == 282)
            {
                ImGui.Text("플레이어 하우징");
            }
            else
            {
                // The <i>Endeavor</i> fix
                string placeString = location.TerritoryExcel.PlaceName.Value.Name.ExtractText();
                placeString = placeString.Replace("\u0002", "");
                placeString = placeString.Replace("\u001a", "");
                placeString = placeString.Replace("\u0003", "");
                placeString = placeString.Replace("\u0001", "");

                placeString = $"{placeString} ({location.MapX:F1}, {location.MapY:F1})";

                // need to use an ID here, the armorer/blacksmith vendors have the same location, resulting in a problem otherwise
                if (ImGui.Button($"{placeString}###{npcInfo.Id}"))
                {
                    Service.HighlightObject.SetNpcInfo([npcInfo]);
                    _ = Service.GameGui.OpenMapWithMapLink(new(location.TerritoryType, location.MapId, location.MapX, location.MapY, 0f));
                }

                var isHoveringButton = ImGui.IsItemHovered();

                if (isHoveringButton)
                {
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                    {
                        ImGui.SetClipboardText($"{_itemToDisplay.Name} -> {npcInfo.Name}@{placeString}, 가격: {costStr}");
                        Service.NotificationManager.AddNotification(new()
                        {
                            Content = "상인 정보를 복사했습니다",
                            Title = "ItemVendorLocation",
                            Type = NotificationType.Success,
                        });
                    }
                }
            }
        }
        else
        {
            ImGui.Text("No location");
        }

        _ = ImGui.TableNextColumn();

        ImGui.Text(costStr);

        if (_itemToDisplay.Type == ItemType.Achievement)
        {
            _ = ImGui.TableNextColumn();
            ImGui.Text(_itemToDisplay.AchievementDescription);
        }
    }

    public override void PreOpenCheck()
    {
        if (_itemToDisplay != null)
        {
            return;
        }

        IsOpen = false;
    }

    public override void Draw()
    {
        ImGui.Text($"{_itemToDisplay.Name} - 상인 목록:");
        ImGuiComponents.HelpMarker("위치 버튼을 우클릭하여 상인 정보를 복사할 수 있습니다");

        var columnCount = 3;
#if DEBUG
        columnCount++;
#endif
        if (_itemToDisplay.Type == ItemType.Achievement)
        {
            columnCount++;
        }

        if (Service.Configuration.ShowShopName && _itemToDisplay.HasShopNames())
        {
            columnCount++;
        }

        if (!ImGui.BeginChild("VendorListChild"))
            return;
        if (!ImGui.BeginTable("Vendors", columnCount, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY, new(-1, -1)))
            return;
        ImGui.TableSetupScrollFreeze(0, 1);
#if DEBUG
        ImGui.TableSetupColumn("NPC ID");
#endif
        ImGui.TableSetupColumn("상인 이름");
        if (Service.Configuration.ShowShopName && _itemToDisplay.HasShopNames())
        {
            ImGui.TableSetupColumn("상점 정보");
        }

        ImGui.TableSetupColumn("위치");
        ImGui.TableSetupColumn(_itemToDisplay.Type == ItemType.CollectableExchange ? "Exchange Rate" : "가격");

        if (_itemToDisplay.Type == ItemType.Achievement)
        {
            ImGui.TableSetupColumn("Obtain Requirement");
        }

        ImGui.TableHeadersRow();

        foreach (var npcInfo in _itemToDisplay.NpcInfos)
        {
            string costStr;
            if (_itemToDisplay.Type == ItemType.CollectableExchange)
            {
                costStr = npcInfo.Costs.Aggregate("", (current, cost) => current + $"{cost.Item2} will yield {cost.Item1}\n");
            }
            else
            {
                costStr = npcInfo.Costs.Aggregate("", (current, cost) => current + $"{cost.Item2} x{cost.Item1}, ");
                costStr = costStr.Length > 0 ? costStr[..^2] : "";
            }

            DrawTableRow(npcInfo, npcInfo.ShopName, npcInfo.Location, costStr);
        }

        ImGui.EndTable();
        ImGui.EndChild();
    }

    public void SetItemToDisplay(ItemInfo item)
    {
        _itemToDisplay = item;
    }
}