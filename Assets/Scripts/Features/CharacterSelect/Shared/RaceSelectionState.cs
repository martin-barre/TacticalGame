using System.Collections.Generic;
using Unity.Netcode;

public struct RaceSelectionState : INetworkSerializable
{
    public ulong ClientId;
    public List<int> CharacterIds;
    public bool IsLockedIn;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref IsLockedIn);
        
        int count = CharacterIds?.Count ?? 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            CharacterIds = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                int id = 0;
                serializer.SerializeValue(ref id);
                CharacterIds.Add(id);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                int id = CharacterIds[i];
                serializer.SerializeValue(ref id);
            }
        }
    }
}