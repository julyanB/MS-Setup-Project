import { api } from "./api";

export type DropDownOption = {
  id: number;
  flow: string;
  key: string;
  code: string;
  label: string;
  sortOrder: number;
  isActive: boolean;
  metadataJson?: string | null;
};

export const BOARD_PROPOSAL_FLOW = "BoardProposalRequest";

export const fallbackDropDowns: Record<string, DropDownOption[]> = {
  MeetingType: [
    option("MeetingType", "Regular", "Regular", 10),
    option("MeetingType", "Extraordinary", "Extraordinary", 20),
  ],
  MeetingFormat: [
    option("MeetingFormat", "InPerson", "In person", 10),
    option("MeetingFormat", "Remote", "Remote", 20),
  ],
  Category: [
    option("Category", "Business", "Business", 10),
    option("Category", "Regulatory", "Regulatory", 20),
    option("Category", "Expense", "Expense", 30),
    option("Category", "Organizational", "Organizational", 40),
    option("Category", "Other", "Other", 50),
  ],
  DecisionStatus: [
    option("DecisionStatus", "Approved", "Approved", 10),
    option("DecisionStatus", "Rejected", "Declined", 20),
    option("DecisionStatus", "Postponed", "Postponed", 30),
    option("DecisionStatus", "ForInformation", "For information", 40),
    option("DecisionStatus", "Withdrawn", "Withdrawn", 50),
    option(
      "DecisionStatus",
      "EscalatedToSupervisoryBoard",
      "Escalated to supervisory board",
      60,
    ),
  ],
  VoteType: [
    option("VoteType", "Positive", "Positive", 10),
    option("VoteType", "PositiveWithCondition", "Positive with condition", 20),
    option(
      "VoteType",
      "PositiveWithRecommendation",
      "Positive with recommendation",
      30,
    ),
    option("VoteType", "Negative", "Negative", 40),
    option("VoteType", "NegativeWithComments", "Negative with comments", 50),
    option("VoteType", "Abstained", "Abstained", 60),
  ],
  TaskStatus: [
    option("TaskStatus", "ToDo", "To do", 10),
    option("TaskStatus", "Completed", "Completed", 20),
    option("TaskStatus", "Extended", "Extended", 30),
    option("TaskStatus", "Other", "Other", 40),
  ],
  DocumentType: [option("DocumentType", "BoardMaterial", "Board material", 10)],
};

export const dropDownApi = {
  get: (flow: string, key: string) =>
    api.get<DropDownOption[]>(
      `/drop-downs/${encodeURIComponent(flow)}/${encodeURIComponent(key)}`,
    ),
};

export async function getDropDownOptions(key: string): Promise<DropDownOption[]> {
  try {
    const options = await dropDownApi.get(BOARD_PROPOSAL_FLOW, key);
    return options.length > 0 ? options : (fallbackDropDowns[key] ?? []);
  } catch {
    return fallbackDropDowns[key] ?? [];
  }
}

export function getDropDownLabel(
  options: DropDownOption[],
  code?: string | null,
): string {
  if (!code) return "";
  return options.find((option) => option.code === code)?.label ?? code;
}

function option(
  key: string,
  code: string,
  label: string,
  sortOrder: number,
): DropDownOption {
  return {
    id: 0,
    flow: BOARD_PROPOSAL_FLOW,
    key,
    code,
    label,
    sortOrder,
    isActive: true,
  };
}
