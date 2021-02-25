using MonoMod;

#pragma warning disable CS0626

namespace ValheimNoServerPassword
{
    [MonoModPatch("global::FejdStartup")]
    internal class ModFejdStartup : FejdStartup
    {
        private extern bool orig_IsPublicPasswordValid(string password, World world);

        private bool IsPublicPasswordValid(string password, World world)
        {
            if (ValheimConfig.ModConfig.NoPasswordEnabled.Value)
            {
                return true;
            }
            return orig_IsPublicPasswordValid(password, world);
        }
    }
}
