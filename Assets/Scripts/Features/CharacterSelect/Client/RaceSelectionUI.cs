using UnityEngine;

public class RaceSelectionUI : MonoBehaviour
{
    [SerializeField] private RaceInfoUI raceInfoUI;

    [SerializeField] private Transform container;
    [SerializeField] private Transform containerPlayer1;
    [SerializeField] private Transform containerPlayer2;
    
    private void Start()
    {
        foreach (Race race in RaceDatabase.GetAll())
        {
            RaceInfoUI instance = Instantiate(raceInfoUI, container);
            instance.OnCharacterSelected += id => RaceSelectionManagerServer.Instance.RequestAddCharacterServerRpc(id);
            instance.SetRace(race);
        }
    }

    public void UpdateInfo(RaceSelectionState[] states)
    {
        // PLAYER 1
        foreach (Transform child in containerPlayer1)
        {
            Destroy(child.gameObject);
        }

        if (states.Length >= 1)
        {
            foreach (int raceId in states[0].CharacterIds)
            {
                Race race = RaceDatabase.GetById(raceId);
                RaceInfoUI instance = Instantiate(raceInfoUI, containerPlayer1);
                instance.OnCharacterSelected += id => RaceSelectionManagerServer.Instance.RequestRemoveCharacterServerRpc(id);
                instance.SetRace(race);
            }
        }
        
        // PLAYER 2
        foreach (Transform child in containerPlayer2)
        {
            Destroy(child.gameObject);
        }
        
        if (states.Length >= 2)
        {
            foreach (int raceId in states[1].CharacterIds)
            {
                Race race = RaceDatabase.GetById(raceId);
                RaceInfoUI instance = Instantiate(raceInfoUI, containerPlayer2);
                instance.OnCharacterSelected += id => RaceSelectionManagerServer.Instance.RequestRemoveCharacterServerRpc(id);
                instance.SetRace(race);
            }
        }
    }
}