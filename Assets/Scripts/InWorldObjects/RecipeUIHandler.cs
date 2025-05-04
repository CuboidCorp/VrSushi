using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeUIHandler : MonoBehaviour
{
    [SerializeField] private Recipe[] recipes;

    [Header("UI Elements")]
    [SerializeField] private GameObject recipeListUI;
    [SerializeField] private Transform recipeListContent;

    [SerializeField] private GameObject recipeViewerUI;
    [SerializeField] private RectTransform recipeViewerContent;

    [SerializeField] private GameObject ingredientUIPrefab;
    [SerializeField] private GameObject transformationUIPrefab;

    [Header("Graph Settings")]
    [SerializeField] private float nodeHorizontalSpacing = 120f;
    [SerializeField] private float nodeVerticalSpacing = 100f;
    [SerializeField] private float nodeSize = 80f;
    [SerializeField] private float lineThickness = 3f;
    [SerializeField] private float graphScaleFactor = 1f;
    [SerializeField] private Vector2 initialContentOffset = new Vector2(20f, 20f);
    [SerializeField] private bool fitToScrollView = true;

    private class Node
    {
        public KitchenItem item;
        public List<Node> children = new();
        public List<Node> parents = new();
        public Vector2 position;
        public GameObject uiObject;
        public int depth;
        public int horizontalPosition;
    }

    private Dictionary<KitchenItem, Node> itemNodes = new();
    private Dictionary<KitchenItem, List<RecipeStep>> itemUsedInSteps = new();

    private void Start()
    {
        recipeViewerUI.SetActive(false);
        recipeListUI.SetActive(true);

        foreach (Recipe item in recipes)
        {
            KitchenItem resultItem = item.finalProduct;
            GameObject ingredientUI = Instantiate(ingredientUIPrefab, recipeListContent);
            Image image = ingredientUI.transform.GetChild(1).GetChild(2).GetComponent<Image>();
            image.sprite = resultItem.icon;

            TMP_Text text = ingredientUI.GetComponentInChildren<TMP_Text>();
            text.text = resultItem.name;

            Button button = ingredientUI.GetComponent<Button>();
            button.onClick.AddListener(() => ShowRecipe(item));
        }
    }

    private void ShowRecipe(Recipe recipe)
    {
        recipeListUI.SetActive(false);
        recipeViewerUI.SetActive(true);

        // Clear previous recipe view
        foreach (Transform child in recipeViewerContent)
            Destroy(child.gameObject);

        itemNodes.Clear();
        itemUsedInSteps.Clear();

        // Reset scroll position
        ScrollRect scrollRect = recipeViewerContent.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0.5f, 0); // Reset to center-top view
        }

        // Reset scale factor
        graphScaleFactor = 1f;

        // Map item -> steps that use it
        foreach (var step in recipe.steps)
        {
            foreach (var input in step.inputItems)
            {
                if (!itemUsedInSteps.ContainsKey(input))
                    itemUsedInSteps[input] = new List<RecipeStep>();
                itemUsedInSteps[input].Add(step);
            }
        }

        // Build graph connections
        foreach (var step in recipe.steps)
        {
            var resultNode = GetOrCreateNode(step.resultItem);
            foreach (var input in step.inputItems)
            {
                var inputNode = GetOrCreateNode(input);
                inputNode.children.Add(resultNode);
                resultNode.parents.Add(inputNode);
            }
        }

        // Find root and terminal nodes
        List<Node> rootNodes = new List<Node>();
        List<Node> terminalNodes = new List<Node>();

        foreach (var nodePair in itemNodes)
        {
            Node node = nodePair.Value;
            if (node.parents.Count == 0)
                rootNodes.Add(node);
            if (node.children.Count == 0)
                terminalNodes.Add(node);
        }

        // Compact the graph if there are many nodes
        CompactLayoutSettings(itemNodes.Count);

        // Layout the graph
        AssignNodeDepths(rootNodes);
        AssignHorizontalPositions(rootNodes);
        OptimizePositions();

        // Calculate the size needed for the content
        CalculateContentSize();

        // Create a container for all graph elements
        GameObject graphContainer = new GameObject("GraphContainer");
        graphContainer.transform.SetParent(recipeViewerContent, false);
        RectTransform containerRect = graphContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = Vector2.zero; // Fill the entire content area

        // Draw lines first so they appear behind nodes
        foreach (var node in itemNodes.Values)
        {
            foreach (var childNode in node.children)
            {
                GameObject edge = Instantiate(transformationUIPrefab, graphContainer.transform);
                DrawLineBetween(edge, node.position, childNode.position);
            }
        }

        // Instantiate UI nodes
        foreach (var node in itemNodes.Values)
        {
            GameObject ui = Instantiate(ingredientUIPrefab, graphContainer.transform);
            var rt = ui.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0); // Bottom-center anchoring
            rt.anchoredPosition = node.position;
            rt.sizeDelta = new Vector2(nodeSize * graphScaleFactor, nodeSize * graphScaleFactor);

            Image img = ui.transform.GetChild(1).GetChild(2).GetComponent<Image>();
            if (img != null) img.sprite = node.item.icon;

            TMP_Text txt = ui.GetComponentInChildren<TMP_Text>();
            if (txt != null)
            {
                txt.text = node.item.name;
                // Adjust text size based on scale factor
                txt.fontSize *= graphScaleFactor;
            }

            node.uiObject = ui;
        }
    }

    private void CompactLayoutSettings(int nodeCount)
    {
        // Dynamically adjust spacing based on number of nodes
        if (nodeCount > 15)
        {
            nodeHorizontalSpacing = Mathf.Min(nodeHorizontalSpacing, 100f);
            nodeVerticalSpacing = Mathf.Min(nodeVerticalSpacing, 80f);
            nodeSize = Mathf.Min(nodeSize, 70f);
        }

        if (nodeCount > 25)
        {
            nodeHorizontalSpacing = Mathf.Min(nodeHorizontalSpacing, 80f);
            nodeVerticalSpacing = Mathf.Min(nodeVerticalSpacing, 70f);
            nodeSize = Mathf.Min(nodeSize, 60f);
        }
    }

    private Node GetOrCreateNode(KitchenItem item)
    {
        if (!itemNodes.TryGetValue(item, out var node))
        {
            node = new Node { item = item };
            itemNodes[item] = node;
        }
        return node;
    }

    private void AssignNodeDepths(List<Node> rootNodes)
    {
        // Reset all depths
        foreach (var node in itemNodes.Values)
            node.depth = -1;

        // IMPORTANT: For our recipe tree, we want ingredients at the top,
        // and final products at the bottom - so we need to invert our depth logic

        // First find the terminal nodes (final products)
        List<Node> terminalNodes = new List<Node>();
        foreach (var nodePair in itemNodes)
        {
            if (nodePair.Value.children.Count == 0)
                terminalNodes.Add(nodePair.Value);
        }

        // If no terminal nodes found, fall back to roots
        if (terminalNodes.Count == 0)
            terminalNodes = rootNodes;

        // BFS to assign depths from terminal nodes upward
        Queue<Node> queue = new Queue<Node>();
        foreach (var terminalNode in terminalNodes)
        {
            terminalNode.depth = 0;
            queue.Enqueue(terminalNode);
        }

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();
            foreach (var parent in current.parents)
            {
                int newDepth = current.depth + 1;
                if (parent.depth < newDepth)
                {
                    parent.depth = newDepth;
                    queue.Enqueue(parent);
                }
            }
        }
    }

    private void AssignHorizontalPositions(List<Node> rootNodes)
    {
        // Group nodes by depth
        Dictionary<int, List<Node>> nodesByDepth = new Dictionary<int, List<Node>>();
        foreach (var nodePair in itemNodes)
        {
            Node node = nodePair.Value;
            if (!nodesByDepth.ContainsKey(node.depth))
                nodesByDepth[node.depth] = new List<Node>();
            nodesByDepth[node.depth].Add(node);
        }

        // Sort depths
        List<int> depths = nodesByDepth.Keys.ToList();
        depths.Sort();

        // Assign horizontal positions by depth
        foreach (int depth in depths)
        {
            List<Node> nodes = nodesByDepth[depth];
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].horizontalPosition = i;
            }
        }

        // Convert to actual positions - positive Y values (top = 0)
        foreach (var nodePair in itemNodes)
        {
            Node node = nodePair.Value;
            node.position = new Vector2(
                node.horizontalPosition * nodeHorizontalSpacing + nodeHorizontalSpacing / 2,
                node.depth * nodeVerticalSpacing + nodeVerticalSpacing
            );
        }
    }

    private void OptimizePositions()
    {
        // Group nodes by depth
        Dictionary<int, List<Node>> nodesByDepth = new Dictionary<int, List<Node>>();
        foreach (var nodePair in itemNodes)
        {
            Node node = nodePair.Value;
            if (!nodesByDepth.ContainsKey(node.depth))
                nodesByDepth[node.depth] = new List<Node>();
            nodesByDepth[node.depth].Add(node);
        }

        // Sort depths
        List<int> depths = nodesByDepth.Keys.ToList();
        depths.Sort();

        // Center child nodes under their parents
        foreach (int depth in depths)
        {
            if (depth == 0) continue; // Skip root nodes

            List<Node> nodes = nodesByDepth[depth];
            foreach (var node in nodes)
            {
                if (node.parents.Count > 0)
                {
                    float avgX = node.parents.Average(p => p.position.x);
                    node.position = new Vector2(avgX, node.position.y);
                }
            }
        }

        // Adjust positions to avoid overlaps
        foreach (int depth in depths)
        {
            List<Node> nodes = nodesByDepth[depth];
            nodes = nodes.OrderBy(n => n.position.x).ToList();

            for (int i = 1; i < nodes.Count; i++)
            {
                Node prev = nodes[i - 1];
                Node current = nodes[i];

                float minDistance = nodeHorizontalSpacing;
                if (current.position.x - prev.position.x < minDistance)
                {
                    float adjustment = minDistance - (current.position.x - prev.position.x);
                    current.position = new Vector2(current.position.x + adjustment, current.position.y);

                    // Adjust all subsequent nodes in this level
                    for (int j = i + 1; j < nodes.Count; j++)
                    {
                        nodes[j].position = new Vector2(nodes[j].position.x + adjustment, nodes[j].position.y);
                    }
                }
            }
        }
    }

    private void CalculateContentSize()
    {
        if (itemNodes.Count == 0) return;

        // Calculate the bounds of all nodes
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var node in itemNodes.Values)
        {
            minX = Mathf.Min(minX, node.position.x - nodeSize / 2);
            maxX = Mathf.Max(maxX, node.position.x + nodeSize / 2);
            minY = Mathf.Min(minY, node.position.y - nodeSize / 2);
            maxY = Mathf.Max(maxY, node.position.y + nodeSize / 2);
        }

        // Add padding
        float padding = nodeSize;
        minX -= padding;
        maxX += padding;
        minY -= padding;
        maxY += padding;

        float width = maxX - minX;
        float height = maxY - minY;

        // Get the scroll view rect for automatic fitting
        if (fitToScrollView)
        {
            // Get parent scroll rect if available
            ScrollRect scrollRect = recipeViewerContent.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                RectTransform scrollViewport = scrollRect.viewport;
                if (scrollViewport != null)
                {
                    float viewportWidth = scrollViewport.rect.width;
                    float viewportHeight = scrollViewport.rect.height;

                    // Calculate scale to fit within the viewport (with some margins)
                    float marginFactor = 0.8f; // 80% of viewport size
                    float widthScale = (viewportWidth * marginFactor) / width;

                    // For height, we want to ensure the recipe is fully visible
                    // but we don't want to shrink it too much if it's tall
                    float heightScale = (viewportHeight * 0.7f) / height;

                    // Use the width scale as primary, but don't go below minimum scale
                    graphScaleFactor = widthScale;

                    // Only apply height scaling if it's severely limiting visibility
                    if (heightScale < widthScale * 0.7f)
                        graphScaleFactor = Mathf.Lerp(heightScale, widthScale, 0.3f);

                    // Don't let it get too small
                    graphScaleFactor = Mathf.Max(graphScaleFactor, 0.4f);

                    // Don't enlarge if it already fits
                    if (graphScaleFactor > 1f)
                        graphScaleFactor = 1f;
                }
            }
        }

        // Apply the scale factor to all positions
        if (graphScaleFactor != 1f)
        {
            foreach (var node in itemNodes.Values)
            {
                node.position = new Vector2(
                    node.position.x * graphScaleFactor,
                    node.position.y * graphScaleFactor);
            }

            // Recalculate bounds after scaling
            minX *= graphScaleFactor;
            maxX *= graphScaleFactor;
            minY *= graphScaleFactor;
            maxY *= graphScaleFactor;
            width = maxX - minX;
            height = maxY - minY;
        }

        // Set content size
        recipeViewerContent.sizeDelta = new Vector2(
            Mathf.Max(width + initialContentOffset.x * 2, recipeViewerContent.parent.GetComponent<RectTransform>().rect.width),
            height + initialContentOffset.y * 2);

        // Center nodes horizontally and position from bottom
        Vector2 centerOffset = new Vector2(
            recipeViewerContent.sizeDelta.x / 2,
            initialContentOffset.y);

        // Adjust all nodes to be centered and positioned from bottom
        foreach (var node in itemNodes.Values)
        {
            node.position = new Vector2(
                node.position.x + centerOffset.x - (minX + maxX) / 2,
                node.position.y + centerOffset.y - minY
            );
        }
    }

    private void DrawLineBetween(GameObject lineObj, Vector2 from, Vector2 to)
    {
        var rt = lineObj.GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0); // Bottom-center anchoring

        Vector2 direction = to - from;
        float distance = direction.magnitude;

        // Scale line thickness based on graph scale factor
        float actualThickness = lineThickness * graphScaleFactor;
        rt.sizeDelta = new Vector2(distance, actualThickness);
        rt.anchoredPosition = from + direction / 2f;
        rt.localRotation = Quaternion.FromToRotation(Vector3.right, direction.normalized);

        // Add arrowhead
        AddArrowhead(lineObj, from, to);
    }

    private void AddArrowhead(GameObject lineObj, Vector2 from, Vector2 to)
    {
        // Create arrowhead
        GameObject arrowhead = new GameObject("Arrowhead");
        arrowhead.transform.SetParent(lineObj.transform, false);

        // Add image component
        Image arrowImage = arrowhead.AddComponent<Image>();
        arrowImage.color = lineObj.GetComponent<Image>().color;

        // Set up RectTransform
        RectTransform arrowRT = arrowhead.GetComponent<RectTransform>();
        arrowRT.pivot = new Vector2(0.5f, 0.5f);
        arrowRT.anchorMin = arrowRT.anchorMax = new Vector2(0.5f, 0.5f);

        // Size based on line thickness
        float arrowSize = lineThickness * 3f * graphScaleFactor;
        arrowRT.sizeDelta = new Vector2(arrowSize, arrowSize);

        // Position at end of line
        Vector2 direction = (to - from).normalized;
        arrowRT.anchoredPosition = direction * (Vector2.Distance(from, to) / 2f - arrowSize / 2);

        // Rotate to point in direction of line
        arrowRT.localRotation = Quaternion.FromToRotation(Vector2.up, direction);
    }

    public void ReturnToList()
    {
        recipeViewerUI.SetActive(false);
        recipeListUI.SetActive(true);
    }
}