// Classe de União-Find para o Algoritmo de Kruskal
public class UnionFind
{
    private int[] parent;
    private int[] rank;

    public UnionFind(int size)
    {
        parent = new int[size];
        rank = new int[size];
        for(int i = 0; i < size; i++)
        {
            parent[i] = i;
            rank[i] = 0;
        }
    }

    public int Find(int x)
    {
        if(parent[x] != x)
            parent[x] = Find(parent[x]); // Path compression
        return parent[x];
    }

    public void Union(int x, int y)
    {
        int xRoot = Find(x);
        int yRoot = Find(y);

        if(xRoot == yRoot)
            return;

        // Union by rank
        if(rank[xRoot] < rank[yRoot])
        {
            parent[xRoot] = yRoot;
        }
        else if(rank[xRoot] > rank[yRoot])
        {
            parent[yRoot] = xRoot;
        }
        else
        {
            parent[yRoot] = xRoot;
            rank[xRoot]++;
        }
    }
}