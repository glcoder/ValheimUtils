using MonoMod;
using ValheimConfig;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace ValheimExtendedStorage
{
    [MonoModPatch("global::Container")]
    class ModContainer : Container
    {
        private extern void orig_Awake();

        private void Awake()
        {
            if (ModConfig.ExtendedStorageEnabled.Value)
            {
                m_width = m_width + 1;
                m_height = m_height + 1;
            }

            orig_Awake();
        }
    }
}