namespace EmployeeManagementService.Infrastructure.Persistence.Seeding.Constants;

public static class SeedRoles
{
    // BoardProposal
    public const string BoardProposal_SecretaryAdmin = "BoardProposalSecretaryAdmin";
    public const string BoardProposal_BoardMember = "BoardProposalBoardMember";
    public const string BoardProposal_TaskOwner = "BoardProposalTaskOwner";
    public const string BoardProposal_UserObserver = "BoardProposalUserObserver";
    public const string BoardProposal_AuditObserver = "BoardProposalAuditObserver";

    public static readonly string[] All =
        typeof(SeedRoles)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToArray();
}
