
namespace Simulation;

public class StateTree : StateTable
{
    abstract class Node
    {
        public bool IsLeaf { get; protected set; } = false;
        public abstract StateCounter Get(ArraySegment<State> config);
        public abstract Node GetBranch(int state);
        public abstract void Set(int branch, Node node);
    }

    class Leaf : Node
    {
        public StateCounter state;
        public Leaf(StateCounter state)
        {
            IsLeaf = true;
            this.state = state;
        }

        public override StateCounter Get(ArraySegment<State> config) => state;
        public override Node GetBranch(int state) => null;
        public override void Set(int branch, Node node) { }
    }

    class Branch : Node
    {
        public Node[] children;
        public StateCounter defaultCounter = null;

        public Branch(int stateCount) => this.children = new Node[stateCount];

        public override StateCounter Get(ArraySegment<State> config)
        {
            var state = config[0].Value;
            var arr = config.Array;
            var seg = new ArraySegment<State>(arr, config.Offset + 1, config.Count - 1);
            var node = children[state];
            if (node != null)
                return node.Get(seg);
            else
                return defaultCounter;
        }

        public override Node GetBranch(int state) => children[state];
        public override void Set(int branch, Node node) => children[branch] = node;
    }

    Branch root;
    int stateCount;
    Random rng;

    public StateTree(int stateCount = 2, Random rng = null)
    {
        this.stateCount = stateCount;
        this.rng = rng == null ? new Random() : rng;
        root = new(stateCount);
    }

    public override bool Contains(State[] configuration)
    {
        var state = root.Get(configuration);
        return state != null;
    }

    public override StateCounter Get(State[] configuration)
    {
        var state = root.Get(configuration);
        if (state != null)
            return state;
        else
        {
            var ret = new StateCounter(stateCount);
            ret.Set(DefaultState);
            return ret;
        }
    }

    void SetPath(State[] configuration, StateCounter counter)
    {
        Branch current = root;
        for (int i = 0; i < configuration.Length; i++)
        {
            var branch = configuration[i].Value;

            if (i == (configuration.Length - 1))
            {
                current.children[branch] = new Leaf(counter);
                return;
            }

            var node = current.children[branch];
            if (node == null)
                current.children[branch] = new Branch(stateCount);
            else if (!(node is Branch))
                throw new Exception("Trying to make a branch on a leaf");

            current = current.children[branch] as Branch;
        }
        throw new Exception("what the fuck");
    }

    public override void Set(State[] configuration, int stateValue)
    {
        var counter = new StateCounter(stateCount, rng);
        counter.Set(stateValue);
        SetPath(configuration, counter);
    }

    public override void Increment(State[] configuration, int stateValue, uint amount = 0)
    {
        if (Contains(configuration))
            Get(configuration).Increment(stateValue, amount);
        else
        {
            var counter = new StateCounter(stateCount, rng);
            counter.Increment(stateValue, amount);
            SetPath(configuration, counter);
        }
    }

    public override void Unset(State[] configuration)
    {
        SetPath(configuration, null);
    }

    public override void Clear()
    {
        for (int i = 0; i < stateCount; i++)
            root.children[i] = null;
    }

    IEnumerable<List<int>> EnumeratePaths(Branch root, List<int> path = null)
    {
        if (path == null) path = new();
        for (int i = 0; i < stateCount; i++)
        {
            if (root.children[i] == null) continue;

            path.Add(i);
            if (root.children[i] is Branch)
            {
                foreach (var p in EnumeratePaths(root.children[i] as Branch, path))
                    yield return p;
            }
            else
                yield return path;

            path.RemoveAt(path.Count - 1);
        }
        yield break;
    }

    public override IEnumerable<State[]> EnumerateConfigurations()
    {
        foreach (var p in EnumeratePaths(root))
        {
            yield return p.ConvertAll((i) =>
            {
                var state = new State((int)Math.Ceiling(Math.Log2(stateCount)));
                state.Value = i;
                return state;
            }).ToArray();
        }
        yield break;
    }

    public override int GetCount()
        => EnumeratePaths(root).Count();
}
