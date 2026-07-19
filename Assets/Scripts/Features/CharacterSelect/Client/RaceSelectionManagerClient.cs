using UnityEngine;

public class RaceSelectionManagerClient : NetworkSingleton<RaceSelectionManagerClient>
{
    [SerializeField] private RaceSelectionUI ui;

    public void UpdateSelectionUI(RaceSelectionState[] states)
              {
                  ui.UpdateInfo(states);
    }

    public void RequestAddCharacter(int characterId)
    {
        if (IsClient)
        {
            RaceSelectionManagerServer.Instance.RequestAddCharacterRpc(characterId);
        }
    }

    public void RequestRemoveCharacter(int characterId)
    {
        if (IsClient)
        {
            RaceSelectionManagerServer.Instance.RequestRemoveCharacterRpc(characterId);
        }
    }
}
