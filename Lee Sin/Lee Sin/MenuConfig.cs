using LeagueSharp.Common;

namespace Lee_Sin
{
   internal class MenuConfig : Helper
    {
       public static readonly string[] Names =
        {
            "Krug",
            "Razorbeak",
            "Murkwolf",
            "Gromp",
            "Crab",
            "Blue",
            "Red",
            "Dragon",
            "Baron"
        };
        public static void OnLoad()
        {
            Config = new Menu(Menuname, Menuname, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            var combos = new Menu("Các Phím", "Các Phím");
            {
                AddKeyBind(combos, "StarCombo", "starcombo", 'Z', KeyBindType.Press);
                AddKeyBind(combos, "Cắm mắt -> W", "Cắm mắt -> W", 'A', KeyBindType.Press);
                AddKeyBind(combos, "Insec", "Insec = mắt", 'G', KeyBindType.Press);
                AddBool(combos, "Dùng Tốc biến trong lúc Insec", "Dùng Tốc biến");
                AddBool(combos, "Sử dụng đối tượng insec", "Sử dụng đối tượng");
                AddBool(combos, "Click chọn Insec", "Click vào");
                AddBool(combos, "Insec về đồng đội hoặc Trụ", "Sử dụng đối tượng");
                AddBool(combos, "Ưu Tiên Tốc biến nếu không có Mắt", "Ưu tiên tốc biến");
                AddBool(combos, "Ward -> Tốc biến Insec", "expwardflash", false);
                AddBool(combos, "Dùng Trừng phạt khi Insec", "Dugnf Trừng phạt");
            }

            var combo = new Menu("Combo Settings", "Combo Settings");
            {
                AddBool(combo, "Dùng [Q]", "dùngq");
                AddBool(combo, "Dùng [Q2]", "dùngq2");
                AddValue(combo, "Dùng [Q2] Trễ", "q2 trễ", 500, 0, 2500);
                AddBool(combo, "Dùng [E]", "dùnge");
                AddBool(combo, "Dùng [R]", "dùngr");
                AddValue(combo, "Tự động [R] tren X mục tiêu", "tudongRtren", 1, 1, 5);
                //var rmenu = new Menu("Auto R on X Enemies", "autorxenemies");
                //{
                //    AddBool(rmenu, "Use ward", "xeward");
                //    AddBool(rmenu, "Use flash", "xeflash");
                //    AddValue(rmenu, "Min enemies hit", "xeminhit", 3, 2, 5);
                //    combo.AddSubMenu(rmenu);
                //}
                AddBool(combo, "dùng [W]", "wardjumpcombo");
                AddBool(combo, "dùng [M]ắt -> W", "combomatW");
                AddBool(combo, "dùng [Trừng phạt]", "dungtrungphat");
                var items = new Menu("Dùng Items", "Dùng Items");
                {
                    items.AddItem(
                        new MenuItem("hydrati", "Chế độ Hydra/RìuMX`").SetValue(
                            new StringList(new[] {"Combo", "Đẩy đường", "Cả 2", "không"})));
                    AddBool(items, "Kiếm ma Youmuu", "youm");
                    AddBool(items, "Khiên Băng", "omen");
                    AddValue(items, "Mục tiêu ít nhất để dùng Khiên băng", "minrand", 3, 1, 5);
                }
                combo.AddSubMenu(items);
            }

            var harass = new Menu("Cài đặt Rỉa máu", "Cài đặt rỉa máu");
            {
                AddBool(harass, "dùng [Q]", "dùngqriamau");
                AddBool(harass, "dùng Second [Q]", "dùngq2riamau");
                AddValue(harass, "dùng [Q2] trễ", "Q2trêriamau", 500, 0, 2500);
                AddBool(harass, "dùng [E]", "dùngEriamau");
                AddValue(harass, "Nội năng nhỏ nhất", "noinangnhonhat", 100, 0, 200);
            }
            var laneclear = new Menu("Cài đặt đẩy đường", "cài đặt đẩy đường");
            {
                AddBool(laneclear, "dùng [Q]", "dungq1");
                AddBool(laneclear, "dùng [E]", "dunge1");
                AddValue(laneclear, "[E] khi X lính", "dunge", 3, 1, 10);
                AddValue(laneclear, "Nội năng nhỏ nhất", "nnnhonhat", 100, 0, 200);
            }

            var lasthit = new Menu("Cài đặt lasthit", "Cài đặt lasthit");
            {
                AddBool(lasthit, "dùng [Q] Last Hit", "dungq");
                AddValue(lasthit, "Nội năng nhỏ nhất", "nnnhonhat", 100, 0, 200);
            }
            var jungleClear = new Menu("Cài đặt Fam rừng", "Cài đặt Fam rừng");
            {
                AddBool(jungleClear, "dùng [Q]", "dungq");
                AddBool(jungleClear, "dùng [W]", "dungw");
                AddBool(jungleClear, "dùng [E]", "dunge");
                AddBool(jungleClear, "Sử dụng skill thông minh", "dungtm");
            }

            var Smite = new Menu("Cài đặt trừng phạt", "Cài đặt trừng phạt");
            {
                var smtiekill = new Menu("Trừng phạt để giết", "Trừng phạt để giết");
                {
                    foreach (var name in Names)
                    {
                        AddBool(smtiekill, "Dùng trừng phạt trên " + name, "Dùng trừng phạt trên" + name);
                    }
                    AddBool(smtiekill, "Dùng trừng phạt có thể giết quái rừng", "trungphatgietquai");
                }
                AddBool(Smite, "Tính dame Q trển trừng phạt", "qcalcsmite");
                AddBool(Smite, "Trừng phạt kẻ thù, "collsmite");
                AddKeyBind(Smite, "Enable Smite", "smiteenable", 'M', KeyBindType.Toggle);
                Smite.AddSubMenu(smtiekill);
            }
            
            var drawings = new Menu("Drawings menu", "Drawings menu");
            {
                var jungle = new Menu("Jungle Drawings", "Jungle Drawings");
                {
                    AddBool(jungle, "Enable Drawings", "jungledraws");
                    AddBool(jungle, "Camp HP Bar", "jungledraw");
                    AddBool(jungle, "Killable Mob", "killmob");
                    AddBool(jungle, "Smite Enabled/Disabled", "enabledisablesmite");
                }
                drawings.AddSubMenu(jungle);
                var spells = new Menu("Spell Ranges", "Spell Ranges");
                {
                    AddBool(spells, "Show Drawings", "spellsdraw");
                    AddBool(spells, "Show Expected Target Position After Insec", "targetexpos");
                    AddBool(spells, "[Q] Range", "qrange");
                    AddBool(spells, "[W] Range", "wrange");
                    AddBool(spells, "[E] Range", "erange");
                    AddBool(spells, "[R] Range", "rrange");
                }
                var misc = new Menu("Misc Drawings", "Misc Drawings");
                {
                    AddBool(misc, "Show Expected Target Position After Insec", "targetexpos");
                    AddBool(misc, "Show Ward Position", "wardpositionshow");
                    AddBool(misc, "Display [R] Polygon", "rpolygon");
                    AddBool(misc, "Show target count hit by R", "counthitr");
                    AddBool(misc, "Show Line Between Objects", "linebetween");
                }
                drawings.AddSubMenu(misc);
                drawings.AddSubMenu(spells);
                AddBool(drawings, "Enable Drawings", "ovdrawings");
            }

            //todo Anti Dash/Gap Closer (Ward jump/R)

            Config.AddSubMenu(combos);
            Config.AddSubMenu(combo);
            Config.AddSubMenu(harass);
            Config.AddSubMenu(laneclear);
            Config.AddSubMenu(jungleClear);
            Config.AddSubMenu(lasthit);
            Config.AddSubMenu(Smite);
            Config.AddSubMenu(drawings);
            Config.AddToMainMenu();
        }
    }
}
