using MonoMod;
using UnityEngine;

#pragma warning disable CS0626
#pragma warning disable CS0649

class patch_Game : Game
{
    private extern void orig_Start();

    private void Start()
    {
        orig_Start();
        Debug.Log($"{nameof(Game)}: Start IsServer={ZNet.instance.IsServer()}");

        ZRoutedRpc.instance.Register("ValheimUtils_MapDataRequest", RPC_ValheimUtils_MapDataRequest);
        ZRoutedRpc.instance.Register<long>("ValheimUtils_MapDataSend", RPC_ValheimUtils_MapDataSend);
    }

    private void RPC_ValheimUtils_MapDataRequest(long sender)
    {
        Debug.Log($"{nameof(Game)}: RPC_ValheimUtils_MapDataRequest Sender={sender}");

        foreach (var peer in ZNet.instance.GetConnectedPeers())
        {
            Debug.Log($"{nameof(Game)}: RPC_ValheimUtils_MapDataRequest ConnectedPeer={peer.m_uid}");
            ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "ValheimUtils_MapDataSend", sender);
        }
    }

    private void RPC_ValheimUtils_MapDataSend(long sender, long requesterId)
    {
        Debug.Log($"{nameof(Game)}: RPC_ValheimUtils_MapDataSend Sender={sender} RequesterId={requesterId}");

        Minimap.instance.SaveMapData();

        ZRoutedRpc.instance.InvokeRoutedRPC(
            requesterId,
            "ValheimUtils_MapDataResponse",
            new ZPackage(GetPlayerProfile().GetMapData())
        );
    }
}

class patch_Player : Player
{
    private extern void orig_OnSpawned();

    public new void OnSpawned()
    {
        orig_OnSpawned();
        Debug.Log($"{nameof(Player)}: OnSpawned PlayerName={GetPlayerName()}");

        if (m_nview.IsOwner())
        {
            ZRoutedRpc.instance.InvokeRoutedRPC("ValheimUtils_MapDataRequest");
        }
    }
}

class patch_Minimap : Minimap
{
    // expose Minimap private fields
    [MonoModIgnore] private Texture2D m_fogTexture;
    [MonoModIgnore] private bool[] m_explored;

    // expose Minimap private methods
    [MonoModIgnore]
    private extern bool Explore(int x, int y);

    // patch Minimap methods
    private extern void orig_Start();

    private void Start()
    {
        orig_Start();
        Debug.Log($"{nameof(Minimap)}: Start");

        ZRoutedRpc.instance.Register<ZPackage>("ValheimUtils_MapDataResponse", RPC_ValheimUtils_MapDataResponse);
    }

    // new Minimap methods
    public void RPC_ValheimUtils_MapDataResponse(long sender, ZPackage mapData)
    {
        Debug.Log($"{nameof(Minimap)}: RPC_ValheimUtils_MapDataResponse Sender={sender} MapDataSize={mapData.Size()}");

        int mapVersion = mapData.ReadInt();
        int mapSize = mapData.ReadInt();

        for (int index = 0; index < m_explored.Length; ++index)
        {
            if (mapData.ReadBool())
            {
                Explore(index % mapSize, index / mapSize);
            }
        }

        m_fogTexture.Apply();
    }
}