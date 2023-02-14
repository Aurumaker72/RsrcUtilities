using System.Collections;

namespace RsrcUtilities;

public class TreeNode<T> : IEnumerable<TreeNode<T>>
{
    public TreeNode(T data)
    {
        Data = data;
        Children = new LinkedList<TreeNode<T>>();

        ElementsIndex = new LinkedList<TreeNode<T>>();
        ElementsIndex.Add(this);
    }

    public T Data { get; set; }
    public TreeNode<T> Parent { get; set; }
    public ICollection<TreeNode<T>> Children { get; set; }

    /// <summary>
    ///     Whether the node has no parent
    /// </summary>
    public bool IsRoot => Parent == null;


    /// <summary>
    ///     Whether the node has no children
    /// </summary>
    public bool IsLeaf => Children.Count == 0;

    public int Level
    {
        get
        {
            if (IsRoot)
                return 0;
            return Parent.Level + 1;
        }
    }


    private ICollection<TreeNode<T>> ElementsIndex { get; }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<TreeNode<T>> GetEnumerator()
    {
        yield return this;
        foreach (var directChild in Children)
        foreach (var anyChild in directChild)
            yield return anyChild;
    }

    public event Action<T>? CollectionChanged;

    public TreeNode<T> AddChild(T child)
    {
        var childNode = new TreeNode<T>(child) { Parent = this };
        Children.Add(childNode);

        RegisterChildForSearch(childNode);

        CollectionChanged?.Invoke(child);

        return childNode;
    }

    public override string ToString()
    {
        return Data != null ? Data.ToString() : "null";
    }

    private void RegisterChildForSearch(TreeNode<T> node)
    {
        ElementsIndex.Add(node);
        if (Parent != null)
            Parent.RegisterChildForSearch(node);
    }

    public TreeNode<T>? FindTreeNode(Func<TreeNode<T>, bool> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        return ElementsIndex.FirstOrDefault(predicate, null);
    }

    public List<TreeNode<T>> GetParents()
    {
        var parents = new List<TreeNode<T>>();

        var currentNode = Parent;

        while (true)
            if (currentNode != null && currentNode.Data != null)
            {
                parents.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            else
            {
                // no more parents, reached top
                break;
            }

        return parents;
    }


    public IEnumerable<T> Flatten()
    {
        return new[] { Data }.Concat(Children.SelectMany(x => x.Flatten()));
    }
}