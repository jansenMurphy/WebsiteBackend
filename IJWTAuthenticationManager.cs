namespace WebsiteBackend{
    public interface IJwtAuthenticationManager{
        string Authenticate(string username, string password, string galaxyName, int id);
    }
}