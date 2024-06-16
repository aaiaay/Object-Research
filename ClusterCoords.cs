
namespace Object_Research
{
    internal class ClusterCoords
    {
        public int size;
        public double sumX;
        public double sumY;
        public int  avgX;
        public int avgY;
        public ClusterCoords(double X, double Y, int size, int avgY, int avgX)
        {
            this.sumX = X;
            this.sumY = Y;
            this.size = size;
            this.avgY = avgY;
            this.avgX = avgX;
        }
    }
}
