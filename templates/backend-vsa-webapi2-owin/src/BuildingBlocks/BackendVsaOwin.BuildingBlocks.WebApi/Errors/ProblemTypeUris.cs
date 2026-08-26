namespace BackendVsaOwin.BuildingBlocks.WebApi.Errors;

/// <summary>
/// Defines the stable URI namespace shared by this template's Problem Details contracts.
/// </summary>
public static class ProblemTypeUris
{
    public const string Prefix =
        "urn:backend-vsa-owin:problem";

    public const string ValidationFailed =
        Prefix + ":validation-failed";
}
