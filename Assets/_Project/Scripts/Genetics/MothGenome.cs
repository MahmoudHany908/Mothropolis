using System;
using UnityEngine;

namespace Mothropolis.Genetics
{
    [Serializable]
    public struct MothGenome 
    {
        [Range(0f, 1f)] public float speed;
        [Range(0f, 1f)] public float camouflage;
        [Range(0f, 1f)] public float lightAttraction;

        public float MovementSpeed => 60f + 80f * speed;
        public float Opacity => 1.00f - 0.55f * camouflage;
        public float PreferredLightRadius => 260f - 210f * lightAttraction;
    }
}
