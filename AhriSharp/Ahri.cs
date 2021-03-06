﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;



namespace AhriSharp
{
    internal class Ahri
    {
        private Menu _menu;

        private Spell _spellQ, _spellW, _spellE, _spellR;

        const float _spellQSpeed = 2600;
        const float _spellQSpeedMin = 400;
        const float _spellQFarmSpeed = 1600;
        const float _spellQAcceleration = -3200; 

        private static Orbwalking.Orbwalker _orbwalker;

        public Ahri()
        {
            if (ObjectManager.Player.ChampionName != "Ahri")
                return;

            (_menu = new Menu("AhriSharp", "AhriSharp", true)).AddToMainMenu();

            var targetSelectorMenu = new Menu("Target Selector", "Chọn mục tiêu");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _menu.AddSubMenu(targetSelectorMenu);

            _orbwalker = new Orbwalking.Orbwalker(_menu.AddSubMenu(new Menu("Orbwalking", "Fam lính")));

            var comboMenu = _menu.AddSubMenu(new Menu("Combo", "Combo"));
            comboMenu.AddItem(new MenuItem("comboQ", "Dùng Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboW", "Dùng W").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboE", "Dùng E").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboR", "Dùng R").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboROnlyUserInitiate", "Chỉ dùng R khi bạn bắt đầu").SetValue(false));

            var harassMenu = _menu.AddSubMenu(new Menu("Harass", "Rỉa máu"));
            harassMenu.AddItem(new MenuItem("harassQ", "Dùng Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("harassE", "Dùng E").SetValue(true));
            harassMenu.AddItem(new MenuItem("harassPercent", "rỉa máu cho đến khi Mana").SetValue(new Slider(20)));

            var farmMenu = _menu.AddSubMenu(new Menu("Lane Clear", "Đẩy đường"));
            farmMenu.AddItem(new MenuItem("farmQ", "dùng Q").SetValue(true));
            farmMenu.AddItem(new MenuItem("farmW", "dùng W").SetValue(false));
            farmMenu.AddItem(new MenuItem("farmPercent", "dùng skill cho đến khi mana %").SetValue(new Slider(20)));
            farmMenu.AddItem(new MenuItem("farmStartAtLevel", "chỉ AA cho đến khi level").SetValue(new Slider(8, 1, 18)));

            var drawMenu = _menu.AddSubMenu(new Menu("Drawing", "Vòng hiển thị"));
            drawMenu.AddItem(new MenuItem("drawQE", "Vòng Q, E Phạm vi").SetValue(new Circle(true, System.Drawing.Color.FromArgb(125, 0, 255, 0))));
            drawMenu.AddItem(new MenuItem("drawW", "Vòng W Phạm vi").SetValue(new Circle(false, System.Drawing.Color.FromArgb(125, 0, 0, 255))));
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Vòng Dame sau khi combo").SetValue(true); //copied from esk0r Syndra
            drawMenu.AddItem(dmgAfterComboItem);

            var miscMenu = _menu.AddSubMenu(new Menu("Misc", "Linh Tinh"));
            miscMenu.AddItem(new MenuItem("autoE", "Tự động E để làm gián đoạn mục tiêu").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoEI", "Tự động dùng E để ngắt").SetValue(true));


            _spellQ = new Spell(SpellSlot.Q, 1000);
            _spellW = new Spell(SpellSlot.W, 795 - 95);
            _spellE = new Spell(SpellSlot.E, 1000);
            _spellR = new Spell(SpellSlot.R, 1000 - 100);

            _spellQ.SetSkillshot(0.25f, 50, 1600f, false, SkillshotType.SkillshotLine);
            _spellW.SetSkillshot(0.70f, _spellW.Range, float.MaxValue, false, SkillshotType.SkillshotCircle);
            _spellE.SetSkillshot(0.25f, 60, 1550f, true, SkillshotType.SkillshotLine);

            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs) { Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>(); };

            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnEndScene += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            
            Game.PrintChat("<font color=\"#1eff00\">Việt Hóa By Tekachi</font> - <font color=\"#00BFFF\">Loaded</font>");
        }

        void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!_menu.Item("autoE").GetValue<bool>()) return;
            if (ObjectManager.Player.Distance(gapcloser.Sender, true) < _spellE.Range * _spellE.Range)
            {
                _spellE.Cast(gapcloser.Sender);
            }
        }

        void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!_menu.Item("autoEI").GetValue<bool>()) return;

            if (ObjectManager.Player.Distance(sender, true) < _spellE.Range * _spellE.Range)
            {
                _spellE.Cast(sender);
            }
        }

        void Game_OnUpdate(EventArgs args)
        {
            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                default:
                    break;
            }
        }

        void Harass()
        {
            if (_menu.Item("harassE").GetValue<bool>() && ObjectManager.Player.ManaPercent >= _menu.Item("harassPercent").GetValue<Slider>().Value)
                CastE();

            if (_menu.Item("harassQ").GetValue<bool>() && ObjectManager.Player.ManaPercent >= _menu.Item("harassPercent").GetValue<Slider>().Value)
                CastQ();
        }

        void LaneClear()
        {
            _spellQ.Speed = _spellQFarmSpeed;
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellQ.Range, MinionTypes.All, MinionTeam.NotAlly);

            bool jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

            if ((_menu.Item("farmQ").GetValue<bool>() && ObjectManager.Player.ManaPercent >= _menu.Item("farmPercent").GetValue<Slider>().Value && ObjectManager.Player.Level >= _menu.Item("farmStartAtLevel").GetValue<Slider>().Value) || jungleMobs)
            {
                MinionManager.FarmLocation farmLocation = _spellQ.GetLineFarmLocation(minions);

                if (farmLocation.Position.IsValid())
                    if (farmLocation.MinionsHit >= 2 || jungleMobs)
                        CastQ(farmLocation.Position);
            }

            minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellW.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minions.Count() > 0)
            {
                jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

                if ((_menu.Item("farmW").GetValue<bool>() && ObjectManager.Player.ManaPercent >= _menu.Item("farmPercent").GetValue<Slider>().Value && ObjectManager.Player.Level >= _menu.Item("farmStartAtLevel").GetValue<Slider>().Value) || jungleMobs)
                    CastW(true);
            }
        }

        bool CastE()
        {
            if (!_spellE.IsReady())
            {
                return false;    
            }
               
            var target = TargetSelector.GetTarget(_spellE.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                return _spellE.Cast(target) == Spell.CastStates.SuccessfullyCasted;
            }
             
            return false;
        }

        void CastQ()
        {
            if (!_spellQ.IsReady())
            {
                return;    
            }    

            var target = TargetSelector.GetTarget(_spellQ.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                var predictedPos = Prediction.GetPrediction(target, _spellQ.Delay * 1.5f); //correct pos currently not possible with spell acceleration
                if (predictedPos.Hitchance >= HitChance.High)
                {
                    _spellQ.Speed = GetDynamicQSpeed(ObjectManager.Player.Distance(predictedPos.UnitPosition));
                    if (_spellQ.Speed > 0f)
                    {
                        _spellQ.Cast(target);
                    }  
                }
            }
        }

        void CastQ(Vector2 pos)
        {
            if (!_spellQ.IsReady())
                return;

            _spellQ.Cast(pos);
        }

        void CastW(bool ignoreTargetCheck = false)
        {
            if (!_spellW.IsReady())
            {
                return;    
            }  

            var target = TargetSelector.GetTarget(_spellW.Range, TargetSelector.DamageType.Magical);

            if (target != null || ignoreTargetCheck)
            {
                _spellW.CastOnUnit(ObjectManager.Player);    
            }   
        }

        void Combo()
        {
            if (_menu.Item("comboE").GetValue<bool>())
            {
                if (CastE())
                {
                    return;
                }
            }

            if (_menu.Item("comboQ").GetValue<bool>())
            {
                CastQ();    
            }


            if (_menu.Item("comboW").GetValue<bool>())
            {
                CastW();    
            }


            if (_menu.Item("comboR").GetValue<bool>() && _spellR.IsReady())
            {
                if (OkToUlt())
                {
                    _spellR.Cast(Game.CursorPos);      
                }   
            }
        }

        List<SpellSlot> GetSpellCombo()
        {
            var spellCombo = new List<SpellSlot>();

            if (_spellQ.IsReady())
                spellCombo.Add(SpellSlot.Q);
            if (_spellW.IsReady())
                spellCombo.Add(SpellSlot.W);
            if (_spellE.IsReady())
                spellCombo.Add(SpellSlot.E);
            if (_spellR.IsReady())
                spellCombo.Add(SpellSlot.R);
            return spellCombo;
        }

        float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = (float)ObjectManager.Player.GetComboDamage(target, GetSpellCombo());

            return (float)(comboDamage + ObjectManager.Player.GetAutoAttackDamage(target));
        }

        bool OkToUlt()
        {
            if (Program.Helper.EnemyTeam.Any(x => x.Distance(ObjectManager.Player) < 500)) //any enemies around me?
                return true;

            Vector3 mousePos = Game.CursorPos;

            var enemiesNearMouse = Program.Helper.EnemyTeam.Where(x => x.Distance(ObjectManager.Player) < _spellR.Range && x.Distance(mousePos) < 650);

            if (enemiesNearMouse.Count() > 0)
            {
                if (IsRActive()) //R already active
                    return true;

                bool enoughMana = ObjectManager.Player.Mana > ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost + ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ManaCost + ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;

                if (_menu.Item("comboROnlyUserInitiate").GetValue<bool>() || !(_spellQ.IsReady() && _spellE.IsReady()) || !enoughMana) //dont initiate if user doesnt want to, also dont initiate if Q and E isnt ready or not enough mana for QER combo
                    return false;

                var friendsNearMouse = Program.Helper.OwnTeam.Where(x => x.IsMe || x.Distance(mousePos) < 650); //me and friends near mouse (already in fight)

                if (enemiesNearMouse.Count() == 1) //x vs 1 enemy
                {
                    Obj_AI_Hero enemy = enemiesNearMouse.FirstOrDefault();

                    bool underTower = Utility.UnderTurret(enemy);

                    return GetComboDamage(enemy) / enemy.Health >= (underTower ? 1.25f : 1); //if enemy under tower, only initiate if combo damage is >125% of enemy health
                }
                else //fight if enemies low health or 2 friends vs 3 enemies and 3 friends vs 3 enemies, but not 2vs4
                {
                    int lowHealthEnemies = enemiesNearMouse.Count(x => x.Health / x.MaxHealth <= 0.1); //dont count low health enemies

                    float totalEnemyHealth = enemiesNearMouse.Sum(x => x.Health);

                    return friendsNearMouse.Count() - (enemiesNearMouse.Count() - lowHealthEnemies) >= -1 || ObjectManager.Player.Health / totalEnemyHealth >= 0.8;
                }
            }

            return false;
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                var drawQE = _menu.Item("drawQE").GetValue<Circle>();
                var drawW = _menu.Item("drawW").GetValue<Circle>();

                if (drawQE.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _spellQ.Range, drawQE.Color);

                if (drawW.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _spellW.Range, drawW.Color);
            }
        }

        float GetDynamicQSpeed(float distance)
        {
            var a = 0.5f * _spellQAcceleration;
            var b = _spellQSpeed;
            var c = - distance;

            if (b * b - 4 * a * c <= 0f)
            {
                return 0;    
            }

            var t = (float) (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
            return distance / t;
        }

        bool IsRActive()
        {
            return ObjectManager.Player.HasBuff("AhriTumble");
        }

        int GetRStacks()
        {
            return ObjectManager.Player.GetBuffCount("AhriTumble");
        }
    }
}
