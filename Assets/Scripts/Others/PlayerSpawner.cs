using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    private const string nextStr = "Next AI";
    private const string prevStr = "Previous AI";

    [SerializeField] private Transform playerCamera;
    [SerializeField] private float lerpSpeed;
    [SerializeField] private Player prefab;
    [SerializeField, Range(1, 20)] private int count;
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
            players[i] = player;
        }
        ChangePlayer("", 0);
    }

    private void Update()
    {
        try
        {
            if (players[currentPlayerIndex] != null && playerCamera != null)
            {
                playerCamera.position = Vector3.Lerp(playerCamera.position, players[currentPlayerIndex].transform.position, Time.deltaTime * lerpSpeed);
            }
        }
        catch
        {
            return;
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 80, 30), nextStr))
        {
            //currentPlayerIndex = ProperClamp(currentPlayerIndex + 1, 0, players.Length - 1);
            ChangePlayer(nextStr);
        }

        if (GUI.Button(new Rect(10, 40, 80, 30), prevStr))
        {
            //currentPlayerIndex = ProperClamp(currentPlayerIndex - 1, 0, players.Length - 1);
            ChangePlayer(prevStr);
        }
    }

    private int ProperClamp(int value, int min, int max)
    {
        if (value < min)
        {
            value = max;
        }

        if (value > max)
        {
            value = min;
        }
        return value;
    }

    private void ChangePlayer(string action, int? customIndex = null)
    {
        players = players.Where(x => x != null).ToArray();
        var isNext = action == nextStr ? true : false;
        var index = isNext ? ProperClamp(currentPlayerIndex + 1, 0, players.Length - 1) : ProperClamp(currentPlayerIndex - 1, 0, players.Length - 1);
        currentPlayerIndex = customIndex.HasValue ? customIndex.Value : index;
        tc.Run(() => { Selection.SetActiveObjectWithContext(players[index], Selection.activeContext); });
    }
}