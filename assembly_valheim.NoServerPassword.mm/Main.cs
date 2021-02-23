public class patch_FejdStartup : FejdStartup
{
    private bool IsPublicPasswordValid(string password, World world)
    {
        return true;
    }
}