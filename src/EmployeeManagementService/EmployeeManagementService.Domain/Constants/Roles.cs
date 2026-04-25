namespace EmployeeManagementService.Domain.Constants;

public static class Roles
{
    // BoardProposal
    public const string BoardProposal_SecretaryAdmin = "BoardProposalSecretaryAdmin";
    public const string BoardProposal_BoardMember = "BoardProposalBoardMember";
    public const string BoardProposal_TaskOwner = "BoardProposalTaskOwner";
    public const string BoardProposal_UserObserver = "BoardProposalUserObserver";
    public const string BoardProposal_AuditObserver = "BoardProposalAuditObserver";

    public static readonly string[] All =
        typeof(Roles)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToArray();
}
