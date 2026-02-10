using System;
using Unity.Collections;
using Unity.Netcode;

namespace CardDuel.Networking.Messages
{
    public struct GameStartMessage : INetworkSerializable
{
    public FixedString32Bytes Action;
    public FixedString64Bytes PlayerIds;
    public int TotalTurns;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Action);
        serializer.SerializeValue(ref PlayerIds);
        serializer.SerializeValue(ref TotalTurns);
    }
}

public struct RevealCardMessage : INetworkSerializable
{
    public FixedString32Bytes Action;
    public ulong PlayerId;
    public int CardId;
    public int OrderIndex;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Action);
        serializer.SerializeValue(ref PlayerId);
        serializer.SerializeValue(ref CardId);
        serializer.SerializeValue(ref OrderIndex);
    }
}

public struct EndTurnMessage : INetworkSerializable
{
    public FixedString32Bytes Action;
    public ulong PlayerId;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Action);
        serializer.SerializeValue(ref PlayerId);
    }
}

public struct SyncBoardMessage : INetworkSerializable
{
    public FixedString32Bytes Action;
    public int OpponentCardCount;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Action);
        serializer.SerializeValue(ref OpponentCardCount);
    }
}

public struct SelectCardMessage : INetworkSerializable
{
    public FixedString32Bytes Action;
    public ulong PlayerId;
    public int CardId;
    public bool IsSelected;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Action);
        serializer.SerializeValue(ref PlayerId);
        serializer.SerializeValue(ref CardId);
        serializer.SerializeValue(ref IsSelected);
    }
}
}