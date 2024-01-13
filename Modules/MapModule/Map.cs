using System.Numerics;

namespace CSurf.Modules
{
    public class Map
    {
        public string Name;
        public Tuple<Vector3, Vector3> StartZone, EndZone;

        public Map(string name, Tuple<Vector3, Vector3> startZone, Tuple<Vector3, Vector3> endZone) {
            this.Name = name;
            this.StartZone = startZone;
            this.EndZone = endZone;
        }

    }
}
