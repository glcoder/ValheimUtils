using MonoMod;

public class patch_FejdStartup : FejdStartup
{
    [MonoModReplace]
    private bool IsPublicPasswordValid(string password, World world)
    {
        return true;
    }
}