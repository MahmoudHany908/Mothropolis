using System.Collections.Generic;
using UnityEngine;
using Mothropolis.Genetics;

namespace Mothropolis.Moths
{
    public interface IMothSpawner
    {
        void SpawnMoths(List<MothGenome> population, GameObject mothPrefab);
        void ClearMoths();
    }
}
