using MonoMod;
using UnityEngine;

#pragma warning disable CS0626, CS0649

namespace ValheimSharedMap
{
    [MonoModPatch("global::ZNet")]
    internal class ModZNet : ZNat
    {
#pragma warning disable CS0414
        [MonoModIgnore] private bool m_publicReferencePosition;
#pragma warning restore CS0414

        private extern void orig_Awake();

        private void Awake()
        {
            Debug.Log($"{nameof(ZNet)}: Awake");
            orig_Awake();

            m_publicReferencePosition = true;
        }

        [MonoModReplace]
        public void SetPublicReferencePosition(bool pub)
        {
        }
    }
}