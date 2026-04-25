using CoreService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreService.Infrastructure.Persistence.Seeding;

public static class DropDownSeeder
{
    private const string BoardProposalFlow = "BoardProposalRequest";

    public static async Task SeedAsync(CoreServiceDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var options = new[]
        {
            Option("MeetingType", "Regular", "Regular", 10),
            Option("MeetingType", "Extraordinary", "Extraordinary", 20),

            Option("MeetingFormat", "InPerson", "In person", 10),
            Option("MeetingFormat", "Remote", "Remote", 20),

            Option("Category", "Business", "Business", 10),
            Option("Category", "Regulatory", "Regulatory", 20),
            Option("Category", "Expense", "Expense", 30),
            Option("Category", "Organizational", "Organizational", 40),
            Option("Category", "Other", "Other", 50),

            Option("DecisionStatus", "Approved", "Approved", 10),
            Option("DecisionStatus", "Rejected", "Declined", 20),
            Option("DecisionStatus", "Postponed", "Postponed", 30),
            Option("DecisionStatus", "ForInformation", "For information", 40),
            Option("DecisionStatus", "Withdrawn", "Withdrawn", 50),
            Option("DecisionStatus", "EscalatedToSupervisoryBoard", "Escalated to supervisory board", 60),

            Option("VoteType", "Positive", "Positive", 10),
            Option("VoteType", "PositiveWithCondition", "Positive with condition", 20),
            Option("VoteType", "PositiveWithRecommendation", "Positive with recommendation", 30),
            Option("VoteType", "Negative", "Negative", 40),
            Option("VoteType", "NegativeWithComments", "Negative with comments", 50),
            Option("VoteType", "Abstained", "Abstained", 60),

            Option("TaskStatus", "ToDo", "To do", 10),
            Option("TaskStatus", "InProgress", "In progress", 20),
            Option("TaskStatus", "Completed", "Completed", 30),
            Option("TaskStatus", "Cancelled", "Cancelled", 40),
            Option("TaskStatus", "NotApplicable", "Not applicable", 50),
            Option("TaskStatus", "Extended", "Extended", 60),

            Option("DocumentType", "BoardMaterial", "Board material", 10),
        };

        var existingOptions = await dbContext.DropDownOptions
            .Where(x => x.Flow == BoardProposalFlow)
            .ToListAsync(cancellationToken);

        var existingByKey = existingOptions.ToDictionary(
            x => $"{x.Key}:{x.Code}",
            StringComparer.OrdinalIgnoreCase);

        foreach (var option in options)
        {
            if (existingByKey.TryGetValue($"{option.Key}:{option.Code}", out var existing))
            {
                existing.Label = option.Label;
                existing.SortOrder = option.SortOrder;
                existing.IsActive = true;
                existing.MetadataJson = option.MetadataJson;
                continue;
            }

            dbContext.DropDownOptions.Add(option);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DropDownOption Option(string key, string code, string label, int sortOrder)
        => new()
        {
            Flow = BoardProposalFlow,
            Key = key,
            Code = code,
            Label = label,
            SortOrder = sortOrder,
            IsActive = true
        };
}
