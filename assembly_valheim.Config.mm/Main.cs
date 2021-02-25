using System.IO;
using BepInEx;
using BepInEx.Configuration;
using MonoMod;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace ValheimConfig
{
    [MonoModPatch("global::WorldGenerator")]
    public class ModConfig
    {
        public static ConfigFile Config;

        // ExtendedStorage
        public static ConfigEntry<bool> ExtendedStorageEnabled;

        // NoPassword
        public static ConfigEntry<bool> NoPasswordEnabled;

        private extern void orig_ctor(World world);

        [MonoModConstructor]
        private void ctor(World world)
        {
            Config = new ConfigFile(Path.Combine(Paths.ConfigPath, "com.glcoder.ValheimUtils.cfg"), true);

            ExtendedStorageEnabled = Config.Bind("ExtendedStorage", "Enabled", false,
                "Enable ExtendedStorage mod."
            );

            NoPasswordEnabled = Config.Bind("NoPassword", "Enabled", true,
                "Enable NoPassword mod."
            );

            orig_ctor(world);
        }
    }
}