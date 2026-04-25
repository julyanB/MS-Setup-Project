namespace EmployeeManagementService.Domain.Constants;

public class Permissions
{
    //BoardProposal
    public const string BoardProposal_Read = "board-proposals.read";
    public const string BoardProposal_CreateMeeting = "board-proposals.meetings.create";
    public const string BoardProposal_AddAgendaItems = "board-proposals.agenda-items.add";
    public const string BoardProposal_UploadMaterials = "board-proposals.materials.upload";
    public const string BoardProposal_SendAgenda = "board-proposals.agenda.send";
    public const string BoardProposal_WorkflowNextStep = "board-proposals.workflow.next-step";
    public const string BoardProposal_RegisterDecisions = "board-proposals.decisions.register";
    public const string BoardProposal_CreateTasks = "board-proposals.tasks.create";
    public const string BoardProposal_ViewTasks = "board-proposals.tasks.view";
    public const string BoardProposal_UpdateTaskStatus = "board-proposals.tasks.status.update";

    public static readonly string[] All =
        typeof(Permissions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToArray();
}
