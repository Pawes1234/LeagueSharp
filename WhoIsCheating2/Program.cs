using System;
using System.Collections.Generic;
using System.Drawing;
using LeagueSharp;

namespace WhoIsCheating2
{
    internal enum Direction
    {
        Left,
        Up,
        Right,
        Down
    };
    internal class Hero
    {
        public int Count;
        public int Detections;
        public int NetworkId;
        public string Nickname;
    }

    internal class Program
    {
        private static bool _lookUp;
        private static bool _isBlocked = true;
        private static bool _isDetecting;
        private static bool _isDrawing = true;
        private static bool _isDrawingCounts;
        private static List<Hero> _heroList;
        private static TimeSpan _ts;
        private static DateTime _start;
        private static int _threshold = 10;

        private static float _posX = 20.0f;
        private static float _posY = 20.0f;
        private static float _posChange = 2.5f;

        private const uint KeyEnd = 0x23;
        private const uint KeyLArrow = 0x25;
        private const uint KeyUArrow = 0x26;
        private const uint KeyRArrow = 0x27;
        private const uint KeyDArrow = 0x28;
        private const uint KeyInsert = 0x2D;
        private const uint KeyDelete = 0x2E;
        private const uint KeyPlus = 0x6B;
        private const uint KeyMinus = 0x6D;

        private static void Main()
        {
            LeagueSharp.Common.CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Obj_AI_Base.OnNewPath += Obj_AI_Hero_OnNewPath;
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            _isBlocked = false;
            _isDetecting = true;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (_isDrawing)
            {
                Drawing.DrawText(_posX, _posY, _isDetecting ? Color.LawnGreen : Color.Red, "Press Delete to toggle detection. Current threshold: {0}", _threshold);
                Drawing.DrawText(_posX, _posY + 20.0f, Color.AntiqueWhite, "Press End to toggle this drawing.");
                if (_heroList != null)
                {
                    for (int i = 0; i < _heroList.Count; i++)
                    {
                        Drawing.DrawText(_posX, (_posY + 40.0f) + (i * 20.0f), (_heroList[i].Detections > 20) ? Color.Red : (_heroList[i].Detections > 5) ? Color.Yellow : Color.AntiqueWhite, "{0} - {1}{2}", _heroList[i].Nickname, _heroList[i].Detections, _isDrawingCounts ? String.Format(" - {0}", _heroList[i].Count) : "");
                    }
                }
                else
                {
                    Drawing.DrawText(_posX, _posY + 40.0f, Color.Aqua, "Please wait for the game to load.");
                }
            }
        }

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 0x100)
            {
                switch (args.WParam)
                {
                    case KeyDelete:
                        if (!_isBlocked)
                            _isDetecting = !_isDetecting;
                        break;

                    case KeyEnd:
                        _isDrawing = !_isDrawing;
                        break;

                    case KeyInsert:
                        _isDrawingCounts = !_isDrawingCounts;
                        break;

                    case KeyPlus:
                        _threshold += 1;
                        break;

                    case KeyMinus:
                        if (_threshold > 1)
                            _threshold -= 1;
                        break;

                    case KeyLArrow:
                        MoveDrawing(Direction.Left);
                        break;

                    case KeyUArrow:
                        MoveDrawing(Direction.Up);
                        break;

                    case KeyRArrow:
                        MoveDrawing(Direction.Right);
                        break;

                    case KeyDArrow:
                        MoveDrawing(Direction.Down);
                        break;
                }
            }
        }

        private static void MoveDrawing(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    if (_posY >= _posChange)
                        _posY -= _posChange;
                    break;

                case Direction.Left:
                    if (_posX >= _posChange)
                        _posX -= _posChange;
                    break;

                case Direction.Down:
                    if (_posY <= Drawing.Height)
                        _posY += _posChange;
                    break;

                case Direction.Right:
                    if (_posX <= Drawing.Width)
                        _posX += _posChange;
                    break;
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            Check();
        }

        private static void Check()
        {
            if (!_lookUp)
            {
                _heroList = new List<Hero>();
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                    _heroList.Add(new Hero { NetworkId = hero.NetworkId, Count = 0, Detections = 0, Nickname = hero.Name });

                _lookUp = true;
            }

            if (!_isDetecting)
                return;

            _ts = DateTime.Now - _start;

            if (_ts.TotalMilliseconds > 1000.0)
                WhoIsCheatingHuehue();
        }

        private static void WhoIsCheatingHuehue()
        {
            using (var enumerator = ObjectManager.Get<Obj_AI_Hero>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var hero = enumerator.Current;
                    if (hero == null || !hero.IsValid)
                    {
                        continue;
                    }

                    if (_heroList.Find(y => y.NetworkId == hero.NetworkId).Count >= _threshold)
                    {
                        ++_heroList.Find(y => y.NetworkId == hero.NetworkId).Detections;
                    }
                    _heroList.Find(y => y.NetworkId == hero.NetworkId).Count = 0;
                }
            }
            _start = DateTime.Now;
        }

        private static void Obj_AI_Hero_OnNewPath(Obj_AI_Base sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Hero) || !_lookUp || !_isDetecting)
            {
                return;
            }

            ++_heroList.Find(hero => hero.NetworkId == sender.NetworkId).Count;
        }
    }
}