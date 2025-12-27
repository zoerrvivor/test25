using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Test25.Core;

namespace Test25.Core.Gameplay

{
    [Serializable]
    public class LevelData
    {
        public string Name { get; set; } = "New Level";
        public int Seed { get; set; }
        public WallType WallType { get; set; } = WallType.Solid;
        public float TerrainRoughness { get; set; } = 0.5f;
        public float TerrainDisplacement { get; set; } = 350f;
        public List<PlacedEntity> Entities { get; set; } = new List<PlacedEntity>();
    }

    [Serializable]
    public class PlacedEntity
    {
        public string Type { get; set; } // "Tank", "Tree", "Building", "Crate"
        public Vector2 Position { get; set; }
        public Color? Color { get; set; }
    }
}
