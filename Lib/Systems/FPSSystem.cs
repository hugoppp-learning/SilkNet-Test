﻿using System;
using Leopotam.Ecs;
using Lib.Render;

namespace Lib.Systems
{
    public class FpsProcessor : IEcsRunSystem
    {
        private int _count;
        private float _fps;
        private double _sum;

        public float UpdateTime { get; set; } = 1f;
        public bool PrintToConsole { get; set; } = true;

        private IRenderInfo _renderInfo = null!;

        public void Run()
        {
            _sum += _renderInfo.Delta;
            _count++;

            if (_sum > UpdateTime)
            {
                _fps = (float) (_count / _sum);
                _sum = 0;
                _count = 0;
                if (PrintToConsole)
                    Console.WriteLine($" [FPS] {_fps.ToString("F1")}");
            }
        }
    }
}