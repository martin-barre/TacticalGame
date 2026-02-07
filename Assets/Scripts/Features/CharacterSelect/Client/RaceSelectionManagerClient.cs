using UnityEngine;

public class RaceSelectionManagerClient : NetworkSingleton<RaceSelectionManagerClient>
{
    [SerializeField] private RaceSelectionUI ui;

    public void UpdateSelectionUI(RaceSelectionState[] states)
    {
        ui.UpdateInfo(states);
    }

    public void LockIn()
    {
        if (IsClient)
        {
            RaceSelectionManagerServer.Instance.LockInSelectionServerRpc();
        }
    }
}