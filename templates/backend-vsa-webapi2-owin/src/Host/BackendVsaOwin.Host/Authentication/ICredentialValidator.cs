namespace BackendVsaOwin.Host.Authentication;

internal interface ICredentialValidator
{
    bool Validate(string username, string password);
}
