namespace eDom.Api.Controllers;

public sealed record CambiaPasswordRequest(
    string PasswordAttuale,
    string PasswordNuova);
