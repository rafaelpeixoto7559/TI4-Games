
// Classe representando uma aresta entre dois vértices
public class Edge
{
    public int Source { get; private set; }
    public int Destination { get; private set; }

    public Edge(int source, int destination)
    {
        Source = source;
        Destination = destination;
    }

    // Sobrescrever Equals e GetHashCode para evitar duplicatas na lista de arestas
    public override bool Equals(object obj)
    {
        if (obj is Edge other)
        {
            return (Source == other.Source && Destination == other.Destination) ||
                   (Source == other.Destination && Destination == other.Source);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Source.GetHashCode() ^ Destination.GetHashCode();
    }
}


