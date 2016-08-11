
namespace LouvainCommunityPL
{
    /// <summary>
    /// Represents a weighted edge between nodes.
    /// </summary>
    public struct Edge
    {
        public int FromNode;
        public int ToNode;
        public double Weight;

        /// <summary>
        /// Constructs a weighted edge between two nodes.
        /// </summary>
        /// <param name="n1">The first node.</param>
        /// <param name="n2">The second node.</param>
        /// <param name="w">The edge's weight.</param>
        public Edge(int n1, int n2, double w)
        {
            FromNode = n1;
            ToNode = n2;
            Weight = w;
        }

        /// <summary>
        /// True iff the two nodes of the edge are the same.
        /// </summary>
        public bool SelfLoop
        {
            get { return FromNode == ToNode; }
        }
    }
}
