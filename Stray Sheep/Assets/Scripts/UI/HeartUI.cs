using UnityEngine;
using System.Collections.Generic;

public class HeartUI : MonoBehaviour
{
    public GameObject heartPrefab;
    public float spacing = 10f;     // Distance between hearts
    public int heartsPerRow = 10;   // How many hearts before starting a new row

    private List<GameObject> activeHearts = new List<GameObject>();

    public void UpdateHearts(int maxHealth)
    {
        // 1. Clear existing hearts
        foreach (GameObject heart in activeHearts)
        {
            Destroy(heart);
        }
        activeHearts.Clear();

        // 2. Instantiate and Position
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, transform);

            // Calculate grid position
            int row = i / heartsPerRow;
            int col = i % heartsPerRow;

            RectTransform rect = newHeart.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Set position based on row and column
                rect.anchoredPosition = new Vector2(col * spacing, -row * spacing);
            }

            activeHearts.Add(newHeart);
        }
    }
}