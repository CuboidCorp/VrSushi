using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeUIHandler : MonoBehaviour
{
    [SerializeField, Tooltip("List of all recipes to show")] private Recipe[] recipes;

    [Header("UI Elements")]
    [SerializeField, Tooltip("Gameobject that shows the list of all recipes")] private GameObject recipeListUI;
    [SerializeField, Tooltip("Transform that will hold the recipes")] private Transform recipeListContent;

    [SerializeField, Tooltip("Gameobject that show the recipe graph for a recipe")] private GameObject recipeViewerUI;
    [SerializeField, Tooltip("Transform that will hold the recipe graph")] private Transform recipeViewerContent;

    [SerializeField, Tooltip("Prefab that shows the info for a recipe")] private GameObject recipeUiPrefab;
    [SerializeField, Tooltip("Prefab that shows the info for a ingredient or an item")] private GameObject nodeUIPrefab;

    [SerializeField, Tooltip("Sprites of all spawn locations")] private Sprite[] spawnLocations;
    [SerializeField, Tooltip("Sprite of ustensils")] private Sprite[] utensils;

    [Header("Recipe Graph")]
    [SerializeField, Tooltip("Size of nodes")] private float nodeSize = 64f;
    [SerializeField, Tooltip("Margin between nodes")] private float margin = 6f;
    [SerializeField, Tooltip("Start pos of graph")] private Vector2 startPos = new(42f, -32f);

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

        foreach (Transform child in recipeViewerContent)
            Destroy(child.gameObject);

        List<List<KitchenItem>> branches = GetBranches(recipe.steps);

        Dictionary<KitchenItem, GameObject> itemToNodeMap = new();
        Dictionary<KitchenItem, Vector2> itemToPositionMap = new();

        float branchSpacingY = nodeSize + margin;
        float currentX = startPos.x;
        float currentY = startPos.y;

        for (int branchIndex = 0; branchIndex < branches.Count; branchIndex++)
        {
            List<KitchenItem> branch = branches[branchIndex];

            foreach (KitchenItem item in branch)
            {
                GameObject node = Instantiate(nodeUIPrefab, recipeViewerContent);
                node.name = item.name;
                node.transform.localPosition = new Vector2(currentX, currentY);

                if (node.TryGetComponent(out RectTransform rectTransform))
                {
                    rectTransform.sizeDelta = new Vector2(nodeSize, nodeSize);
                }

                Image icon = node.GetComponentInChildren<Image>();
                if (icon != null && item.icon != null)
                {
                    icon.sprite = item.icon;
                }

                TMP_Text text = node.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = item.name;
                }

                itemToNodeMap[item] = node;
                itemToPositionMap[item] = new Vector2(currentX, currentY);

                currentX += nodeSize + margin;
            }

            currentX = startPos.x;
            currentY += -branchSpacingY;
        }

        //Création des connexions
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

    /// <summary>
    /// Renvoie les branches de la recette sous forme de liste de listes d'items de cuisine.
    /// Une branche represente la suite des transformations sur un ingrédient que l'on spawn.
    /// </summary>
    /// <param name="recipeSteps">La recette dont on veut les branches</param>
    /// <returns>Une liste de liste de kitchen item donc la liste des branches</returns>
    private List<List<KitchenItem>> GetBranches(List<RecipeStep> recipeSteps)
    {
        List<List<KitchenItem>> branches = new(); // Liste des branches
        foreach (var step in recipeSteps)
        {
            switch (step.method)
            {
                case ObtentionMethod.SPAWN:
                    // Créer une nouvelle branche pour SPAWN
                    branches.Add(new List<KitchenItem> { step.resultItem });
                    break;

                case ObtentionMethod.COMBINE:
                    var branchesContainingInputs = branches
                        .Where(branch => step.inputItems.Any(item => branch.Contains(item)))
                        .ToList();

                    if (branchesContainingInputs.Count > 0)
                    {
                        var targetBranch = branchesContainingInputs
                            .OrderByDescending(branch => branch.Count)
                            .First();

                        //On ajoute le resultat a la branche la plus longue qui a les inputs
                        targetBranch.Add(step.resultItem);
                    }
                    else
                    {
                        // Si aucune branche ne contient les inputItems, on fait une au cas ou mais ce sera jamais utilisé
                        branches.Add(new List<KitchenItem> { step.resultItem });
                    }
                    break;

                default:
                    // Par defaut on ajoute le resultat a la suite logique
                    var targetBranchDefault = branches
                        .FirstOrDefault(branch => step.inputItems.All(item => branch.Contains(item)));

                    if (targetBranchDefault != null)
                    {
                        targetBranchDefault.Add(step.resultItem);
                    }
                    else
                    {
                        // Si aucune branche ne correspond, on crée une nouvelle mais là aussi jamais utilisé
                        branches.Add(new List<KitchenItem> { step.resultItem });
                    }
                    break;
            }
        }
        return branches;
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




    private Sprite GetSprite(ObtentionMethod method, SpawnLocation? spawn)
    {
        if (method == ObtentionMethod.SPAWN)
            return spawnLocations[(int)spawn];
        return utensils[(int)method];
    }

    public void ReturnToList()
    {
        recipeViewerUI.SetActive(false);
        recipeListUI.SetActive(true);
    }


}