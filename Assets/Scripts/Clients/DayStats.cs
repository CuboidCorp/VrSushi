using System.Collections.Generic;
using UnityEngine;

public class DayStats
{
    public int totalClients;
    public int satisfiedClients;
    public int unsatisfiedClients;
    public int notServedClients;
    public float totalSatisfaction;
    public int wastedIngredients;

    public List<KitchenItem> recipesServed = new();

    public float AverageSatisfaction =>
        satisfiedClients == 0 ? 0f : totalSatisfaction / satisfiedClients;


    public void RecordClient(ClientResult result, float satisfactionLevel, KitchenItem dish = null)
    {
        totalClients++;

        switch (result)
        {
            case ClientResult.Satisfied:
                satisfiedClients++;
                totalSatisfaction += satisfactionLevel;
                if (dish != null)
                    recipesServed.Add(dish);
                break;

            case ClientResult.Unsatisfied:
                unsatisfiedClients++;
                break;

            case ClientResult.NotServed:
                notServedClients++;
                break;
        }
    }

    public int GetScore()
    {
        return 30 * satisfiedClients - 30 * notServedClients - 10 * unsatisfiedClients - 5 * wastedIngredients;
    }

    public void PrintSummary()
    {
        Debug.Log($"--- Day Summary ---");
        Debug.Log($"Total Clients: {totalClients}");
        Debug.Log($"Satisfied: {satisfiedClients}, Unsatisfied: {unsatisfiedClients}, Not Served: {notServedClients}");
        Debug.Log($"Average Satisfaction: {AverageSatisfaction:F2}");

        if (recipesServed.Count > 0)
        {
            Debug.Log("Recipes Served:");
            foreach (var recipe in recipesServed)
            {
                Debug.Log($"- {recipe.name}");
            }
        }
        else
        {
            Debug.Log("No recipes served today.");
        }
    }
}
