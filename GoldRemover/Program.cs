using System;
using System.Collections.Generic;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace GoldRemover
{
    static class Program
    {
        private const ushort DeleteKey = 0x2E;
        private const ushort InsertKey = 0x2D;
        private static bool IsDrawing = true;
        private static float TotalGoldDeleted = 0.0f;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (!IsDrawing) return;
            Drawing.DrawText(20.0f, 20.0f, Color.Red, "Hold Delete to delete your gold.");
            Drawing.DrawText(20.0f, 40.0f, Color.Red, "Press Insert to toggle this text");
            Drawing.DrawText(20.0f, 60.0f, Color.Red, "Total gold deleted: {0}", TotalGoldDeleted);
        }

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 0x100 && args.WParam == InsertKey)
                ToggleText();

            if (args.Msg == 0x100 && args.WParam == DeleteKey)
                DeleteGold();
        }

        private static void DeleteGold()
        {
            if (ObjectManager.Player.InShop())
            {
                var slot = GetFirstFreeSlot();
                if (slot != -1)
                {
                    if (ObjectManager.Player.Gold >= 35.0f)
                    {
                        ObjectManager.Player.BuyItem(ItemId.Health_Potion);
                        ObjectManager.Player.SellItem(slot);
                        TotalGoldDeleted += 21.0f;
                    }
                }
            }
        }

        private static int GetFirstFreeSlot()
        {
            Dictionary<int, ItemId> InventorySlots = new Dictionary<int, ItemId>(8);
            foreach (InventorySlot inventorySlot in ObjectManager.Player.InventoryItems)
            {
                InventorySlots.Add(inventorySlot.Slot, inventorySlot.Id);
            }
            for (int i = 0; i < 6; ++i)
            {
                try
                {
                    var x = InventorySlots[i];
                    if (x == ItemId.Health_Potion) return i;
                }
                catch (KeyNotFoundException)
                {
                    return i;
                }
            }
            return -1;
        }

        private static void ToggleText()
        {
            IsDrawing = !IsDrawing;
        }
    }
}
