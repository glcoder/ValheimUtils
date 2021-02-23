using System.Collections.Generic;
using MonoMod;
using UnityEngine;

#pragma warning disable CS0626
#pragma warning disable CS0649

class patch_Player : Player
{
    private bool m_enterGame = true;

    public extern void orig_OnSpawned();

    public new void OnSpawned()
    {
        Debug.Log($"{nameof(Player)}: OnSpawned PlayerName={GetPlayerName()}");
        orig_OnSpawned();

        if (m_enterGame)
        {
            foreach (var mapScanline in ((patch_Minimap) Minimap.instance).SharedMap_GetMapScanlines())
            {
                ZRoutedRpc.instance.InvokeRoutedRPC("SharedMap_Update", mapScanline);
            }

            m_enterGame = false;
        }
    }
}

class patch_Minimap : Minimap
{
    // expose Minimap fields
    [MonoModIgnore] private Texture2D m_fogTexture;
    [MonoModIgnore] private bool[] m_explored;

    [MonoModIgnore]
    private extern bool Explore(int x, int y);

    private extern void orig_Start();

    private void Start()
    {
        Debug.Log($"{nameof(Minimap)}: Start IsServer={ZNet.instance.IsServer()}");
        orig_Start();

        if (ZNet.instance.IsServer())
        {
            ZRoutedRpc.instance.Register<ZPackage>("SharedMap_Update", RPC_SharedMap_Update);
        }
        else
        {
            ZRoutedRpc.instance.Register<ZPackage>("SharedMap_Apply", RPC_SharedMap_Apply);
        }
    }

    public IEnumerable<ZPackage> SharedMap_GetMapScanlines()
    {
        for (int y = 0; y < m_textureSize; ++y)
        {
            var scanline = new ZPackage();

            scanline.Write(m_textureSize);
            scanline.Write(y);

            for (int x = 0; x < m_textureSize; ++x)
            {
                scanline.Write(m_explored[y * m_textureSize + x]);
            }

            yield return scanline;
        }
    }

    private void RPC_SharedMap_Update(long sender, ZPackage mapScanline)
    {
        Debug.Log($"{nameof(Minimap)}: RPC_SharedMap_Update Sender={sender} MapScanlineSize={mapScanline.Size()}");


        int mapSize = mapScanline.ReadInt();
        if (m_textureSize != mapSize)
            return;

        var y = mapScanline.ReadInt();

        var tempScanline = new ZPackage();
        tempScanline.Write(mapSize);
        tempScanline.Write(y);

        for (int x = 0; x < m_textureSize; ++x)
        {
            if (mapScanline.ReadBool())
            {
                Debug.Log($"{nameof(Minimap)}: SharedMap Explore({x}, {y})");
                m_explored[y * m_textureSize + x] = true;
            }

            tempScanline.Write(m_explored[y * m_textureSize + x]);
        }

        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SharedMap_Apply", tempScanline);
    }

    private void RPC_SharedMap_Apply(long sender, ZPackage mapScanline)
    {
        Debug.Log($"{nameof(Minimap)}: RPC_SharedMap_Apply Sender={sender} MapScanlineSize={mapScanline.Size()}");

        int mapSize = mapScanline.ReadInt();
        if (m_textureSize != mapSize)
            return;

        var y = mapScanline.ReadInt();
        for (int x = 0; x < m_textureSize; ++x)
        {
            if (mapScanline.ReadBool())
            {
                Debug.Log($"Explore({x}, {y})");
                Explore(x, y);
            }
        }

        m_fogTexture.Apply();
    }
}