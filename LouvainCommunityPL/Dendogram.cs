using System.Collections.Generic;
using System.Linq;

namespace LouvainCommunityPL
{
    /// <summary>
    /// A dendrogram is a tree, and each level is a partition of the graph nodes. Level 0 is the first partition, which contains the smallest communities,
    /// and the largest (best) are in dendrogram.Length - 1.
    /// </summary>
    public class Dendrogram
    {
        private List<IDictionary<int, int>> Partitions;

        /// <summary>
        /// Creates a dendrogram with one level.
        /// </summary>
        /// <param name="part">The partition for the one level.</param>
        public Dendrogram(IDictionary<int, int> part)
        {
            Partitions = new List<IDictionary<int, int>>();
            Partitions.Add(part);
        }

        /// <summary>
        /// Creates a dendrogram with multiple levels.
        /// </summary>
        /// <param name="parts"></param>
        public Dendrogram(IEnumerable<IDictionary<int, int>> parts)
        {
            Partitions = new List<IDictionary<int, int>>(parts);
        }

        public int Length
        {
            get { return Partitions.Count; }
        }

        /// <summary>
        /// Return the partition of the nodes at the given level.
        /// </summary>
        /// <param name="level">The level to retrieve, [0..dendrogram.Length-1].</param>
        /// <returns>A dictionary where keys are nodes and values the set to which it belongs.</returns>
        public Dictionary<int, int> PartitionAtLevel(int level)
        {
            Dictionary<int, int> partition = new Dictionary<int, int>(Partitions[0]);
            for (int index = 1; index <= level; index++)
            {
                foreach (int node in partition.Keys.ToArray())
                {
                    int com = partition[node];
                    partition[node] = Partitions[index][com];
                }
            }
            return partition;
        }
    }
}