using MonoMod;
using UnityEngine;

#pragma warning disable CS0626

class patch_Game : Game
{
    private extern void orig_Start();

    private void Start()
    {
        orig_Start();
        Debug.Log($"{nameof(Game)}: Start IsServer={ZNet.instance.IsServer()}");

        ZRoutedRpc.instance.Register("ValheimUtils_MapDataRequest", RPC_ValheimUtils_MapDataRequest);
        ZRoutedRpc.instance.Register<long>("ValheimUtils_MapDataSend", RPC_ValheimUtils_MapDataSend);
        ZRoutedRpc.instance.Register<ZPackage>("ValheimUtils_MapDataResponse", ((patch_Minimap) Minimap.instance).RPC_ValheimUtils_MapDataResponse);
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
    private Texture2D m_fogTexture;
    private bool[] m_explored;

    private extern void orig_Explore(int x, int y);

    private void Explore(int x, int y)
    {
        orig_Explore(x, y);
    }

    public void RPC_ValheimUtils_MapDataResponse(long sender, ZPackage zpackage)
    {
        int num1 = zpackage.ReadInt();
        int num2 = zpackage.ReadInt();

        for (int index = 0; index < this.m_explored.Length; ++index)
        {
            if (zpackage.ReadBool())
            {
                Explore(index % num2, index / num2);
            }
        }
        this.m_fogTexture.Apply();
        Debug.Log($"{nameof(Game)}: RPC_ValheimUtils_MapDataResponse Sender={sender} MapDataSize={num2}");
    }
}