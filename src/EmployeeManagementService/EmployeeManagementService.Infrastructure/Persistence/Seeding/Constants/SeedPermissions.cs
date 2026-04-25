namespace EmployeeManagementService.Infrastructure.Persistence.Seeding.Constants;

public class SeedPermissions
{
    //BoardProposal
    public const string BoardProposal_Create = "board-proposals.create";
    public const string BoardProposal_AddAgendaItems = "board-proposals.agenda-items.add";
    public const string BoardProposal_UploadMaterials = "board-proposals.materials.upload";
    public const string BoardProposal_Review = "board-proposals.review";
    public const string BoardProposal_Send = "board-proposals.send";
    public const string BoardProposal_MarkHeld = "board-proposals.mark-held";
    public const string BoardProposal_RegisterDecisions = "board-proposals.decisions.register";
    public const string BoardProposal_ManageTasks = "board-proposals.tasks.manage";
    public const string BoardProposal_MonitorDeadlines = "board-proposals.deadlines.monitor";
    public const string BoardProposal_Close = "board-proposals.close";

    public static readonly string[] All =
        typeof(SeedPermissions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToArray();
}
