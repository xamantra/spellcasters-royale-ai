using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Player prefab;
    [SerializeField, Range(1, 10)] private int count;
    [Header("Spawn Range")]
    [SerializeField] private float minRange;
    [SerializeField] private float maxRange;

    private Player[] players;
    private int currentPlayerIndex;

    private void Start()
    {
        players = new Player[count];
        for (int i = 0; i < count; i++)
        {
            var player = Instantiate(prefab, new Vector3(Random.Range(minRange, maxRange), prefab.transform.position.y, Random.Range(minRange, maxRange)), Quaternion.identity);
            if (i == 0)
            {
                players[i] = player;
            }
        }
    }

    private void Update()
    {
        try
        {
            if (players[currentPlayerIndex] != null && playerCamera != null)
            {
                playerCamera.position = players[currentPlayerIndex].transform.position;
            }
        }
        catch
        {
            return;
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 70, 50, 30), "Next AI"))
        {
            currentPlayerIndex = Mathf.Clamp(currentPlayerIndex + 1, 0, players.Length - 1);
        }

        if (GUI.Button(new Rect(25, 70, 50, 30), "Previous AI"))
        {
            currentPlayerIndex = Mathf.Clamp(currentPlayerIndex - 1, 0, players.Length - 1);
        }
    }
}