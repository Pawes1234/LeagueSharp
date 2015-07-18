using System;
using System.Collections.Generic;
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
        private static bool _isDetecting;
        private static bool _isDrawing = true;
        private static int _lastTick;
        private static List<Hero> _heroList;
        private static TimeSpan _ts;
        private static DateTime _start;
        private static int threshold = 10;

        private static float posX = 20.0f;
        private static float posY = 20.0f;
        private static float posChange = 2.5f;

        private const uint keyEnd = 0x23;
        private const uint keyLArrow = 0x25;
        private const uint keyUArrow = 0x26;
        private const uint keyRArrow = 0x27;
        private const uint keyDArrow = 0x28;
        private const uint keyDelete = 0x2E;
        private const uint keyPlus = 0x6B;
        private const uint keyMinus = 0x6D;

        private static void Main(string[] args)
        {
            Obj_AI_Base.OnNewPath += Obj_AI_Hero_OnNewPath;
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (_isDrawing && _heroList != null)
            {
                Drawing.DrawText(posX, posY, _isDetecting ? System.Drawing.Color.LawnGreen : System.Drawing.Color.Red, "Press Delete to toggle detection. Current threshold: {0}", threshold);
                Drawing.DrawText(posX, posY + 20.0f, System.Drawing.Color.AntiqueWhite, "Press End to toggle this drawing.");
                for (int i = 0; i < _heroList.Count; i++)
                {
                    Drawing.DrawText(posX, (posY + 40.0f) + (i * 20.0f), System.Drawing.Color.AntiqueWhite, "{0} - {1}", _heroList[i].Nickname, _heroList[i].Detections);
                }
            }
        }

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 0x100)
            {
                switch (args.WParam)
                {
                    case keyDelete:
                        _isDetecting = !_isDetecting;
                        break;

                    case keyEnd:
                        _isDrawing = !_isDrawing;
                        break;

                    case keyPlus:
                        threshold += 1;
                        break;

                    case keyMinus:
                        threshold -= 1;
                        break;

                    case keyLArrow:
                        MoveDrawing(Direction.Left);
                        break;

                    case keyUArrow:
                        MoveDrawing(Direction.Up);
                        break;

                    case keyRArrow:
                        MoveDrawing(Direction.Right);
                        break;

                    case keyDArrow:
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
                    if (posY >= posChange)
                        posY -= posChange;
                    break;

                case Direction.Left:
                    if (posX >= posChange)
                        posX -= posChange;
                    break;

                case Direction.Down:
                    if (posY <= Drawing.Height)
                        posY += posChange;
                    break;

                case Direction.Right:
                    if (posX <= Drawing.Height)
                        posX += posChange;
                    break;
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            Check();
        }

        private static void Check()
        {
            if (Environment.TickCount <= _lastTick + 200)
            {
                return;
            }

            if (!_lookUp)
            {
                _heroList = new List<Hero>();
                using (var enumerator = ObjectManager.Get<Obj_AI_Hero>().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        if (current == null || !current.IsValid)
                        {
                            continue;
                        }

                        _heroList.Add(new Hero {NetworkId = current.NetworkId, Count = 0, Detections = 0, Nickname = current.Name});
                    }
                }
                _lookUp = true;
            }

            if (!_isDetecting)
            {
                return;
            }

            _ts = DateTime.Now - _start;
            if (_ts.TotalMilliseconds > 1000.0)
            {
                WhoIsCheatingHuehue();
            }

            _lastTick = Environment.TickCount;
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

                    if (_heroList.Find(y => y.NetworkId == hero.NetworkId).Count >= threshold)
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