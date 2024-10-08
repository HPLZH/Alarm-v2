namespace Alarm_v2
{
    public class Tree : TreeNode
    {
        readonly Dictionary<string, TreeNode> children = [];
        int count = -1;
        //public override int Count => Children.Sum(kvp => kvp.Value.Count);

        private int GetCount()
        {
            if(count == -1)
            {
                count = Children.Sum(kvp => kvp.Value.Count);
            }
            return count;
        }

        public override int Count => GetCount();
        public override Dictionary<string, TreeNode> Children => children;

        public void AddSubTree(string name)
        {
            children.Add(name, new Tree());
        }

        public void AddEndPoint(string name, int value = 1)
        {
            children.Add(name, new TreeEnd { Value = value });
        }

        public void AddEndPath(string[] path, int value = 1)
        {
            if (path.Length == 0) return;
            else if (Children.TryGetValue(path[0], out TreeNode? v))
            {
                if (v is Tree t)
                    t.AddEndPath(path[1..], value);
            }
            else if (path.Length == 1)
            {
                AddEndPoint(path[0], value);
            }
            else
            {
                AddSubTree(path[0]);
                if (children[path[0]] is Tree t)
                    t.AddEndPath(path[1..], value);
            }
        }

        public override IEnumerable<TreeNode> EnumerateNodes()
        {
            yield return this;
            foreach (var (_, node) in children)
            {
                foreach (var child in node.EnumerateNodes())
                {
                    yield return child;
                }
            }
        }

        public IEnumerable<Tree> EnumerateTrees()
        {
            yield return this;
            foreach (var (_, node) in children)
            {
                foreach (var child in node.EnumerateNodes())
                {
                    if (child is Tree t)
                        yield return t;
                }
            }
        }
    }

    public abstract class TreeNode
    {
        public abstract Dictionary<string, TreeNode> Children { get; }
        public abstract int Count { get; }

        public TreeNode? GetNode(string[] path)
        {
            if (path.Length == 0) return this;
            else if (Children.TryGetValue(path[0], out TreeNode? value))
                return value.GetNode(path[1..]);
            else
                return null;
        }

        public abstract IEnumerable<TreeNode> EnumerateNodes();
    }

    public class TreeEnd : TreeNode
    {
        public int Value { get; set; } = 1;
        public override int Count => Value;
        public override Dictionary<string, TreeNode> Children => [];

        public override IEnumerable<TreeNode> EnumerateNodes()
        {
            yield return this;
        }
    }

    public static class TreeExtensions
    {
        public static int DirectChildrenCount(this Tree tree)
        {
            int c = 0;
            foreach (var kvp in tree.Children)
            {
                if (kvp.Value is TreeEnd child)
                {
                    c += child.Count;
                }
            }
            return c;
        }

        public static double ADCC(this Tree tree)
        {
            int n = 0;
            int c = 0;
            foreach (var t in tree.EnumerateTrees())
            {
                int dcc = t.DirectChildrenCount();
                if (dcc != 0)
                {
                    n++;
                    c += dcc;
                }
            }
            if (n == 0)
                return 0;
            else
                return (double)c / n;
        }

        public static void PrintTree(this Tree tree,string name, Action<Tree>? action = null, int sp = 2)
        {
            string pth = name;
            if(tree.Children.Count == 1 && tree.Children.Count(kvp => kvp.Value is Tree) == 1)
            {
                foreach(var kvp in tree.Children)
                {
                    (kvp.Value as Tree)?.PrintTree(Path.Combine(pth, kvp.Key), action, sp);
                }
            }
            else
            {
                IO.CWrite(pth);
                action?.Invoke(tree);
                int x = Console.CursorLeft;
                Console.WriteLine();
                Console.CursorLeft = x + sp;
                foreach (var kvp in tree.Children.OrderByDescending(kvp => kvp.Value.Count))
                {
                    (kvp.Value as Tree)?.PrintTree(kvp.Key, action, sp);
                }
                Console.CursorLeft = x;
            }
        }
    }
}
