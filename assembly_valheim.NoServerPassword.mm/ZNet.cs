using MonoMod;
using UnityEngine;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace ValheimNoServerPassword
{
    [MonoModPatch("global::ZNet")]
    internal class ModZNet : ZNet
    {
        [MonoModIgnore] private static string m_serverPassword;
        [MonoModIgnore] private SyncedList m_permittedList;

        [MonoModIgnore]
        private extern void ClearPlayerData(ZNetPeer peer);

        [MonoModIgnore]
        private extern ZNetPeer GetPeer(ZRpc rpc);

        private extern void orig_RPC_ServerHandshake(ZRpc rpc);

        private void RPC_ServerHandshake(ZRpc rpc)
        {
            if (!ValheimConfig.ModConfig.NoPasswordEnabled.Value)
            {
                orig_RPC_ServerHandshake(rpc);
                return;
            }

            ZNetPeer peer = this.GetPeer(rpc);
            if (peer == null)
                return;
            ZLog.Log((object)("Got handshake from client " + peer.m_socket.GetEndPointString()));
            this.ClearPlayerData(peer);
            bool needPassword = !(string.IsNullOrEmpty(ModZNet.m_serverPassword) || this.m_permittedList.Contains(peer.m_socket.GetHostName()));
            peer.m_rpc.Invoke("ClientHandshake", (object)needPassword);
        }

        private extern void orig_RPC_PeerInfo(ZRpc rpc, ZPackage pkg);

        private void RPC_PeerInfo(ZRpc rpc, ZPackage pkg)
        {
            if (!ValheimConfig.ModConfig.NoPasswordEnabled.Value)
            {
                orig_RPC_PeerInfo(rpc, pkg);
                return;
            }

            ZNetPeer peer = this.GetPeer(rpc);
            if (peer == null)
                return;

            // Repackage data with changed password for simplicity
            ZPackage zpackage = new ZPackage();
            zpackage.Write(pkg.ReadLong());
            zpackage.Write(pkg.ReadString());
            zpackage.Write(pkg.ReadVector3());
            zpackage.Write(pkg.ReadString());

            if (this.IsServer())
            {
                string password = pkg.ReadString();
                if (this.m_permittedList.Contains(peer.m_socket.GetHostName()))
                {
                    password = ModZNet.m_serverPassword;
                }
                zpackage.Write(password);
                zpackage.Write(pkg.ReadByteArray());
            } else
            {
                zpackage.Write(pkg.ReadString());
                zpackage.Write(pkg.ReadInt());
                zpackage.Write(pkg.ReadString());
                zpackage.Write(pkg.ReadLong());
                zpackage.Write(pkg.ReadInt());
                zpackage.Write(pkg.ReadDouble());
            }

            orig_RPC_PeerInfo(rpc, new ZPackage(zpackage.GetArray()));
        }
    }
}