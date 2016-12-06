using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.ComponentModel;

namespace GraphSort
{

    public static class Extensions
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
        public static bool TryParse<T>(this T instance, string input, out T output)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    output = (T)converter.ConvertFromString(input);
                    return true;
                }
                else
                {
                    output = default(T);
                    return false;
                }
            }
            catch (NotSupportedException)
            {
                output = default(T);
                return false;
            }
        }
    }

    public class Vertex<T>
    {
        T _item;
        public LinkedList<Vertex<T>> Adjacent { get; set; }
        public T Data { get { return _item; } }
        public Vertex(T data)
        {
            _item = data;
            Adjacent = new LinkedList<Vertex<T>>();
        }
    }


    public class Graph<T>
    {
        int vCount = 0;
        List<Vertex<T>> vertices;

        public Graph() { vertices = new List<Vertex<T>>(); }
        public int Count { get { return vCount; } }
        public List<Vertex<T>> Vertices { get { return vertices; } }

        public void Add(Vertex<T> vertex)
        {
            vertices.Add(vertex);
            vCount++;
        }

        public void AddDirectedEdge(Vertex<T> source, Vertex<T> destination)
        {
            source.Adjacent.AddLast(destination);
        }
        public void RemoveDirectedEdge(Vertex<T> source, Vertex<T> destination)
        {
            source.Adjacent.Remove(destination);
        }

        public void GetOutDegrees()
        {
            Vertices.ForEach(v => Console.WriteLine(string.Format("{0} Out Degree of: {1}",
                v.Data,
                v.Adjacent.Count)));
        }
        public void GetInDegrees()
        {
            int aCount;
            Vertices.ForEach(v =>
            {
                aCount = 0;
                Vertices.ForEach(u =>
                {
                    aCount += u.Adjacent.Where(x => x.Data.Equals(v.Data)).Count();
                });
                Console.WriteLine(string.Format("{0} In Degree of: {1}",
                    v.Data,
                    aCount));
            });
        }

        public void DepthFirstSearch(T Key)
        {
            var start = this.vertices.First();
            var destinaton = this.vertices.Where(v => v.Data.Equals(Key)).FirstOrDefault();

            if (start == null)
                throw new ArgumentException("Graph is empty");

            if (destinaton == null)
                throw new ArgumentException("Key does not exist in graph");

            DepthFirstSearch(start, destinaton);
        }
        public void DepthFirstSearch(Vertex<T> Source, Vertex<T> Destination)
        {
            HashSet<Vertex<T>> visited = new HashSet<Vertex<T>>();
            Stack<Vertex<T>> processed = new Stack<Vertex<T>>();
            LinkedListNode<Vertex<T>> neighbor;

            //add source the the queue
            processed.Push(Source);
            visited.Add(Source);

            //process the queue until its empy
            while (processed.Count > 0)
            {
                //remove the first item in queue and process adjacent vertices
                var vertex = processed.Pop();

                //check if we found key, stop if so
                if(vertex.Data.Equals(Destination.Data))
                {
                    Console.WriteLine();
                    Console.WriteLine(string.Format("Path from {0} -> {1} exists", Source.Data, vertex.Data));
                    break;
                }

                //process item
                Console.Write(string.Format("-> {0} ", vertex.Data));

                //iterate over adjacent vertices reverse
                neighbor = vertex.Adjacent.Last;
                for (int i = 0; i < vertex.Adjacent.Count; i++)
                {
                    //check if visited already
                    if (!visited.Contains(neighbor.Value))
                    {
                        //enqueue adjacent vertices
                        processed.Push(neighbor.Value);
                        visited.Add(neighbor.Value);
                    }
                    neighbor = neighbor.Previous;
                }
            }
        }

        public void BreadthFirstSearch(T Key)
        {
            var start = this.vertices.First();
            var destinaton = this.vertices.Where(v => v.Data.Equals(Key)).FirstOrDefault();

            if (start == null)
                throw new ArgumentException("Graph is empty");

            if (destinaton == null)
                throw new ArgumentException("Key does not exist in graph");

            BreathFirstSearch(start, destinaton);
        }
        public void BreathFirstSearch(Vertex<T> Source, Vertex<T> Destination)
        {
            HashSet<Vertex<T>> visited = new HashSet<Vertex<T>>();
            Queue<Vertex<T>> processed = new Queue<Vertex<T>>();

            //add source the the queue
            processed.Enqueue(Source);
            visited.Add(Source);

            //process the queue until its empy
            while (processed.Count > 0)
            {
                //remove the first item in queue and process adjacent vertices
                var vertex = processed.Dequeue();

                //check if we found key, stop if so
                if (vertex.Data.Equals(Destination.Data))
                {
                    Console.WriteLine();
                    Console.WriteLine(string.Format("Path from {0} -> {1} exists", Source.Data, vertex.Data));
                    break;
                }

                //process item
                Console.Write(string.Format("-> {0} ", vertex.Data));

                //iterate over adjacent vertices
                foreach (var v in vertex.Adjacent)
                {
                    //check if visited already
                    if (!visited.Contains(v))
                    {
                        //enqueue adjacent vertices
                        processed.Enqueue(v);
                        visited.Add(v);
                    }
                }
            }
        }

        public void Print()
        {
            vertices.ForEach(v =>
            {
                Console.Write(string.Format("{0} ", v.Data));
                v.Adjacent.AsEnumerable().ToList().ForEach(n =>
                {
                    Console.Write(string.Format("-> {0}", n.Data));
                });
                Console.WriteLine();
            });
        }
    }

    public class GraphBuilder<T> where T : new()
    {
        public static List<Vertex<T>> GetVertices(string[] Vertices)
        {
            List<Vertex<T>> vertices = new List<Vertex<T>>();
            T vertex;

            for(int i = 0; i < Vertices.Length; i++)
            {
                if ((new T()).TryParse<T>(Vertices[i], out vertex))
                {
                    vertices.Add(new Vertex<T>(vertex));
                }
            }
            return vertices;
        }

        public static List<Tuple<Vertex<T>,Vertex<T>>> GetEdges(string[] Edges, List<Vertex<T>> Vertices)
        {
            List<Tuple<Vertex<T>,Vertex<T>>> edges = new List<Tuple<Vertex<T>, Vertex<T>>>();
            T vertexA;
            T vertexB;

            for (int i = 0; i < Edges.Length; i++)
            {
                var edgeVertices = Edges[i].Split(' ');
                if (edgeVertices.Count() > 1
                    && (new T()).TryParse(edgeVertices[0], out vertexA)
                    && (new T()).TryParse(edgeVertices[1], out vertexB)
                    )
                {
                    var A = Vertices.Where(x => x.Data.Equals(vertexA)).First();

                    edges.Add(new Tuple<Vertex<T>, Vertex<T>>(
                        Vertices.Where(x => x.Data.Equals(vertexA)).First(),
                        Vertices.Where(x => x.Data.Equals(vertexB)).First()
                        ));
                }
            }
            return edges;
        }

        public static void GenerateGraph(Graph<T> Graph, List<Vertex<T>> Vertices , List<Tuple<Vertex<T>, Vertex<T>>> Edges)
        {
            foreach (Vertex<T> v in Vertices)
                Graph.Add(v);

            foreach (var e in Edges)
                Graph.AddDirectedEdge(e.Item1, e.Item2);
        } 
    } 

    class Program
    {
        static void Main(string[] args)
        {
            var inputFile = @"";
            var graphData = File.ReadAllLines(inputFile);
            var Vertices = GraphBuilder<char>.GetVertices(graphData.SubArray(0, 17));
            var Edges = GraphBuilder<char>.GetEdges(graphData.SubArray(17, 16), Vertices);

            Graph<char> graph = new Graph<char>();
            GraphBuilder<char>.GenerateGraph(graph, Vertices, Edges);

            graph.DepthFirstSearch('L');

            graph.BreadthFirstSearch('L');

            Console.ReadKey();
        }
    }
}