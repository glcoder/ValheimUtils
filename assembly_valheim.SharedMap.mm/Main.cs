using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using MonoMod;
using UnityEngine;

#pragma warning disable CS0626
#pragma warning disable CS0649

class patch_ZNet : ZNet
{
    [MonoModIgnore] private bool m_publicReferencePosition;

    private extern void orig_Awake();

    private void Awake()
    {
        Debug.Log($"{nameof(ZNet)}: Awake");
        orig_Awake();

        m_publicReferencePosition = true;
    }

    [MonoModReplace]
    public new void SetPublicReferencePosition(bool pub)
    {
    }
}

class patch_Game : Game
{
    [MonoModIgnore] private bool m_firstSpawn;

    private extern Player orig_SpawnPlayer(Vector3 spawnPoint);

    private Player SpawnPlayer(Vector3 spawnPoint)
    {
        Debug.Log($"{nameof(Game)}: SpawnPlayer FirstSpawn={m_firstSpawn} SpawnPoint={spawnPoint}");
        var player = orig_SpawnPlayer(spawnPoint);

        if (!ZNet.instance.IsServer() && m_firstSpawn)
        {
            ((patch_Minimap) Minimap.instance).SharedMap_Update();
        }

        return player;
    }
}

class patch_Minimap : Minimap
{
    [MonoModIgnore] private Texture2D m_fogTexture;
    [MonoModIgnore] private bool[] m_explored;

    private List<ZNet.PlayerInfo> m_SharedMap_playersInfo;
    private float m_SharedMap_exploreTimer;

    [MonoModIgnore]
    private extern bool Explore(int x, int y);

    [MonoModIgnore]
    private extern void Explore(Vector3 p, float radius);

    private extern void orig_Start();
    private extern void orig_Update();

    private void Start()
    {
        Debug.Log($"{nameof(Minimap)}: Start IsServer={ZNet.instance.IsServer()}");
        orig_Start();

        m_SharedMap_playersInfo = new List<ZNet.PlayerInfo>();
        m_SharedMap_exploreTimer = 0.0f;

        if (ZNet.instance.IsServer())
        {
            ZRoutedRpc.instance.Register<ZPackage>("SharedMap_Update", RPC_SharedMap_Update);
        }
        else
        {
            ZRoutedRpc.instance.Register<ZPackage>("SharedMap_Apply", RPC_SharedMap_Apply);
        }
    }

    private void Update()
    {
        orig_Update();

        m_SharedMap_exploreTimer += Time.deltaTime;
        if (m_SharedMap_exploreTimer <= m_exploreInterval)
            return;

        m_SharedMap_exploreTimer = 0.0f;

        m_SharedMap_playersInfo.Clear();
        ZNet.instance.GetOtherPublicPlayers(m_SharedMap_playersInfo);

        foreach (var playerInfo in m_SharedMap_playersInfo)
        {
            Explore(playerInfo.m_position, m_exploreRadius);
        }
    }

    public void SharedMap_Update()
    {
        var mapData = SharedMap_CompressMap(m_explored);
        ZRoutedRpc.instance.InvokeRoutedRPC("SharedMap_Update", mapData);
    }

    private ZPackage SharedMap_CompressMap(bool[] explored)
    {
        using (var memoryStream = new MemoryStream())
        {
            var bits = new BitArray(explored);
            var buffer = new byte[explored.Length / 8 + (explored.Length % 8 == 0 ? 0 : 1)];
            bits.CopyTo(buffer, 0);

            using (var stream = new DeflateStream(memoryStream, CompressionMode.Compress))
            {
                stream.Write(buffer, 0, buffer.Length);
                stream.Close();

                var mapData = new ZPackage();
                mapData.Write(explored.Length);
                mapData.Write(memoryStream.ToArray());

                return mapData;
            }
        }
    }

    private bool[] SharedMap_DecompressMap(ZPackage compressedMapData)
    {
        var exploredLength = compressedMapData.ReadInt();
        using (var memoryStream = new MemoryStream(compressedMapData.ReadByteArray()))
        {
            var buffer = new byte[exploredLength / 8 + (exploredLength % 8 == 0 ? 0 : 1)];

            using (var stream = new DeflateStream(memoryStream, CompressionMode.Decompress))
            {
                stream.Read(buffer, 0, buffer.Length);
                stream.Close();

                var bits = new BitArray(buffer);
                var explored = new bool[exploredLength];
                bits.CopyTo(explored, 0);

                return explored;
            }
        }
    }

    private void RPC_SharedMap_Update(long sender, ZPackage mapData)
    {
        Debug.Log($"{nameof(Minimap)}: RPC_SharedMap_Update Sender={sender} MapDataSize={mapData.Size()}");

        var explored = SharedMap_DecompressMap(mapData);
        if (explored.Length != m_explored.Length)
        {
            Debug.LogError($"{nameof(Minimap)}: RPC_SharedMap_Update invalid map data");
            return;
        }

        for (var index = 0; index < explored.Length; ++index)
        {
            // server side m_fogTexture can be ignored
            m_explored[index] = m_explored[index] || explored[index];
        }

        ZRoutedRpc.instance.InvokeRoutedRPC(
            ZRoutedRpc.Everybody,
            "SharedMap_Apply",
            SharedMap_CompressMap(m_explored)
        );
    }

    private void RPC_SharedMap_Apply(long sender, ZPackage mapData)
    {
        Debug.Log($"{nameof(Minimap)}: RPC_SharedMap_Apply Sender={sender} MapDataSize={mapData.Size()}");

        var explored = SharedMap_DecompressMap(mapData);
        if (explored.Length != m_explored.Length)
        {
            Debug.LogError($"{nameof(Minimap)}: RPC_SharedMap_Apply invalid map data");
            return;
        }

        for (var index = 0; index < explored.Length; ++index)
        {
            if (explored[index])
            {
                Explore(index % m_textureSize, index / m_textureSize);
            }
        }

        m_fogTexture.Apply();
    }
}