using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeUIHandler : MonoBehaviour
{
    [SerializeField, Tooltip("List of all recipes to show")] private Recipe[] recipes;

    [SerializeField] private RecipeStepPopup recipeStepPopup;

    [Header("UI Elements")]
    [SerializeField, Tooltip("Gameobject that shows the list of all recipes")] private GameObject recipeListUI;
    [SerializeField, Tooltip("Transform that will hold the recipes")] private Transform recipeListContent;

    [SerializeField, Tooltip("Gameobject that show the recipe graph for a recipe")] private GameObject recipeViewerUI;
    [SerializeField, Tooltip("Transform that will hold the recipe graph")] private Transform recipeViewerContent;

    [SerializeField, Tooltip("Prefab that shows the info for a recipe")] private GameObject recipeUiPrefab;
    [SerializeField, Tooltip("Prefab that shows the info for a ingredient or an item")] private GameObject nodeUIPrefab;



    [Header("Recipe Graph")]
    [SerializeField, Tooltip("Size of nodes")] private float nodeSize = 64f;
    [SerializeField, Tooltip("Margin between nodes")] private float margin = 6f;
    [SerializeField, Tooltip("Start pos of graph")] private Vector2 startPos = new(42f, -32f);
    [SerializeField] private Button btnBack;

    private void Start()
    {
        recipeViewerUI.SetActive(false);
        recipeListUI.SetActive(true);

        foreach (Recipe recipe in recipes)
        {
            KitchenItem resultItem = recipe.finalProduct;
            GameObject ingredientUI = Instantiate(recipeUiPrefab, recipeListContent);

            // Vérifions que la structure du prefab est correcte
            Transform iconTransform = ingredientUI.transform.GetChild(1).GetChild(2);
            if (iconTransform != null)
            {
                Image image = iconTransform.GetComponent<Image>();
                if (image != null && resultItem.icon != null)
                {
                    image.sprite = resultItem.icon;
                }
            }

            TMP_Text text = ingredientUI.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = resultItem.name;
            }

            Button button = ingredientUI.GetComponent<Button>();
            button.onClick.AddListener(() => ShowRecipe(recipe));
        }
    }

    private void ShowRecipe(Recipe recipe)
    {
        recipeListUI.SetActive(false);
        recipeViewerUI.SetActive(true);

        btnBack.onClick.AddListener(ReturnToList);

        foreach (Transform child in recipeViewerContent)
            Destroy(child.gameObject);



        // Calculate item depths - how far each item is from the final product
        Dictionary<KitchenItem, int> itemDepths = CalculateItemDepths(recipe);

        // Find the maximum depth to determine column layout
        int maxDepth = itemDepths.Values.Count > 0 ? itemDepths.Values.Max() : 0;

        // Calculate item positions by depth
        Dictionary<KitchenItem, Vector2> itemPositions = PositionItemsByDepth(recipe, itemDepths, maxDepth);

        // Create nodes for all items
        Dictionary<KitchenItem, GameObject> itemToNodeMap = CreateNodes(recipe, itemPositions);

        // Create connections between nodes
        CreateConnections(recipe, itemToNodeMap);
    }

    /// <summary>
    /// Calculates how many steps each item is from the final product
    /// </summary>
    private Dictionary<KitchenItem, int> CalculateItemDepths(Recipe recipe)
    {
        Dictionary<KitchenItem, int> depths = new Dictionary<KitchenItem, int>();

        // Set final product at depth 0
        depths[recipe.finalProduct] = 0;

        // Use BFS to determine depths starting from the final product
        Queue<KitchenItem> queue = new Queue<KitchenItem>();
        queue.Enqueue(recipe.finalProduct);

        while (queue.Count > 0)
        {
            KitchenItem current = queue.Dequeue();
            int currentDepth = depths[current];

            // Find all steps that produce this item
            var stepsProducingItem = recipe.steps.Where(s => s.resultItem == current).ToList();

            foreach (var step in stepsProducingItem)
            {
                // For each input item to this step
                if (step.inputItems != null)
                {
                    foreach (var input in step.inputItems)
                    {
                        // If we haven't assigned a depth yet, or if the new depth is greater
                        if (!depths.ContainsKey(input) || depths[input] < currentDepth + 1)
                        {
                            depths[input] = currentDepth + 1;
                            queue.Enqueue(input);
                        }
                    }
                }
            }
        }

        return depths;
    }

    /// <summary>
    /// Positions items based on their depth from the final product
    /// </summary>
    private Dictionary<KitchenItem, Vector2> PositionItemsByDepth(Recipe recipe, Dictionary<KitchenItem, int> itemDepths, int maxDepth)
    {
        Dictionary<KitchenItem, Vector2> positions = new Dictionary<KitchenItem, Vector2>();
        Dictionary<int, List<KitchenItem>> depthToItems = new Dictionary<int, List<KitchenItem>>();

        // Group items by depth
        foreach (var kvp in itemDepths)
        {
            if (!depthToItems.ContainsKey(kvp.Value))
                depthToItems[kvp.Value] = new List<KitchenItem>();

            depthToItems[kvp.Value].Add(kvp.Key);
        }

        // Position items from right to left (starting with final product)
        for (int depth = 0; depth <= maxDepth; depth++)
        {
            if (!depthToItems.ContainsKey(depth))
                continue;

            List<KitchenItem> itemsAtDepth = depthToItems[depth];

            // Calculate x position - rightmost for final product (depth 0)
            float xPos = startPos.x + (maxDepth - depth) * (nodeSize + margin);

            // Position items vertically within this column
            for (int i = 0; i < itemsAtDepth.Count; i++)
            {
                float yPos = startPos.y - i * (nodeSize + margin);
                positions[itemsAtDepth[i]] = new Vector2(xPos, yPos);
            }
        }

        return positions;
    }

    /// <summary>
    /// Creates UI nodes for all items using calculated positions
    /// </summary>
    private Dictionary<KitchenItem, GameObject> CreateNodes(Recipe recipe, Dictionary<KitchenItem, Vector2> itemPositions)
    {
        Dictionary<KitchenItem, GameObject> itemToNodeMap = new Dictionary<KitchenItem, GameObject>();

        // Créez un ensemble de tous les items dans la recette
        HashSet<KitchenItem> allItems = new HashSet<KitchenItem>();
        allItems.Add(recipe.finalProduct);

        foreach (var step in recipe.steps)
        {
            allItems.Add(step.resultItem);
            if (step.inputItems != null)
            {
                foreach (var input in step.inputItems)
                {
                    allItems.Add(input);
                }
            }
        }

        foreach (KitchenItem item in allItems)
        {
            if (!itemPositions.ContainsKey(item))
                continue;

            GameObject node = Instantiate(nodeUIPrefab, recipeViewerContent);
            node.name = item.itemName;
            node.transform.localPosition = itemPositions[item];

            if (node.TryGetComponent(out RectTransform rectTransform))
            {
                rectTransform.sizeDelta = new Vector2(nodeSize, nodeSize);
            }

            Image icon = node.transform.GetChild(0).GetChild(0).GetComponent<Image>();
            if (icon != null && item.icon != null)
            {
                icon.sprite = item.icon;
            }

            TMP_Text text = node.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = item.itemName;
            }

            // Ajoutez un événement onClick pour afficher la popup
            Button button = node.GetComponent<Button>();
            if (button != null)
            {
                // Trouvez l'étape associée à cet item
                RecipeStep associatedStep = recipe.steps.FirstOrDefault(s => s.resultItem == item);
                if (associatedStep != null)
                {
                    button.onClick.AddListener(() => recipeStepPopup.Show(associatedStep));
                }
            }

            itemToNodeMap[item] = node;
        }

        return itemToNodeMap;
    }


    /// <summary>
    /// Creates visual connections between nodes
    /// </summary>
    private void CreateConnections(Recipe recipe, Dictionary<KitchenItem, GameObject> itemToNodeMap)
    {
        foreach (var step in recipe.steps)
        {
            if (step.inputItems != null && step.inputItems.Count > 0)
            {
                foreach (KitchenItem inputItem in step.inputItems)
                {
                    if (itemToNodeMap.TryGetValue(inputItem, out GameObject fromNode) &&
                        itemToNodeMap.TryGetValue(step.resultItem, out GameObject toNode))
                    {
                        CreateConnection(fromNode, toNode);
                    }
                }
            }
        }
    }

    private void CreateConnection(GameObject fromNode, GameObject toNode)
    {
        GameObject connection = new("Connection");
        connection.transform.SetParent(recipeViewerContent, false);

        Image connectionImage = connection.AddComponent<Image>();
        connectionImage.color = Color.white;

        connectionImage.rectTransform.sizeDelta = new Vector2(2, 2);

        Vector3 fromPosition = fromNode.transform.localPosition;
        Vector3 toPosition = toNode.transform.localPosition;

        Vector3 direction = (toPosition - fromPosition).normalized;

        Vector3 adjustedFromPosition = fromPosition + direction * (nodeSize / 2);
        Vector3 adjustedToPosition = toPosition - direction * (nodeSize / 2);

        connection.transform.SetLocalPositionAndRotation(
            (adjustedFromPosition + adjustedToPosition) / 2,
            Quaternion.FromToRotation(Vector3.right, adjustedToPosition - adjustedFromPosition)
        );

        connection.GetComponent<RectTransform>().sizeDelta = new Vector2(Vector3.Distance(adjustedFromPosition, adjustedToPosition), 5);

        connection.transform.SetAsFirstSibling();
    }

    public void ReturnToList()
    {
        recipeViewerUI.SetActive(false);
        recipeListUI.SetActive(true);
    }
}