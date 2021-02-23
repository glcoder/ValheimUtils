using UnityEngine;

#pragma warning disable CS0626

public class patch_Game : Game
{
    private extern void orig_Start();
    private extern Player orig_SpawnPlayer(Vector3 spawnPoint);

    private void Start()
    {
        orig_Start();
        Debug.Log($"{nameof(Game)}: Start IsServer={ZNet.instance.IsServer()}");

        if (ZNet.instance.IsServer())
        {
            ZRoutedRpc.instance.Register<float>("ValheimUtils_Ping", RPC_ValheimUtils_Ping);
        }
        else
        {
            ZRoutedRpc.instance.Register<float>("ValheimUtils_Pong", RPC_ValheimUtils_Pong);
        }
    }

    private Player SpawnPlayer(Vector3 spawnPoint)
    {
        var player = orig_SpawnPlayer(spawnPoint);
        Debug.Log($"{nameof(Game)}: SpawnPlayer SpawnPoint={spawnPoint}");

        ZRoutedRpc.instance.InvokeRoutedRPC("ValheimUtils_Ping", Time.time);
        return player;
    }

    private void RPC_ValheimUtils_Ping(long sender, float time)
    {
        Debug.Log($"{nameof(Game)}: RPC_ValheimUtils_Ping Sender={sender}");
        ZRoutedRpc.instance.InvokeRoutedRPC(sender, "ValheimUtils_Pong", time);
    }

    private void RPC_ValheimUtils_Pong(long sender, float time)
    {
        var pingTime = (int) ((Time.time - time) * 1000);
        Debug.Log($"{nameof(Game)}: RPC_ValheimUtils_Pong Sender={sender} PingTime={pingTime} ms");
    }
}