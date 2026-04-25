import { api } from "./api";

export type BoardProposalStatus =
  | "Draft"
  | "AgendaPreparation"
  | "SecretaryReview"
  | "ChairpersonReview"
  | "ReadyForSending"
  | "Sent"
  | "Held"
  | "DecisionsAndTasks"
  | "DeadlineMonitoring"
  | "Closed"
  | "Cancelled";

export type EmployeeRequestAction =
  | "Submit"
  | "Approve"
  | "Return"
  | "Reject"
  | "Send"
  | "MarkHeld"
  | "StartDecisionRegistration"
  | "StartMonitoring"
  | "Close"
  | "Cancel"
  | "Reopen";

export type BoardProposalDecisionStatus =
  | "Approved"
  | "Rejected"
  | "Postponed"
  | "ForInformation"
  | "Withdrawn"
  | "EscalatedToSupervisoryBoard";

export type BoardProposalTaskStatus =
  | "ToDo"
  | "InProgress"
  | "Completed"
  | "Cancelled"
  | "NotApplicable"
  | "Extended"
  | "Other";

export type BoardProposalAgendaItemDetails = {
  id: number;
  title: string;
  initiatorEmployeeId: string;
  responsibleBoardMemberEmployeeId: string;
  presenterEmployeeId: string;
  category: string;
  description?: string | null;
  order: number;
  decisionStatus?: BoardProposalDecisionStatus | null;
  decisionText?: string | null;
  finalVote?: string | null;
  notes?: string | null;
  votes: BoardProposalVoteDetails[];
  tasks: BoardProposalTaskDetails[];
};

export type BoardProposalVoteDetails = {
  id: number;
  boardMemberEmployeeId: string;
  voteType: string;
  notes?: string | null;
};

export type BoardProposalTaskDetails = {
  id: number;
  title: string;
  description?: string | null;
  responsibleEmployeeId: string;
  dueDate: string;
  status: BoardProposalTaskStatus;
  order?: number;
  extendedDueDate?: string | null;
  comment?: string | null;
};

export type AttachmentDetails = {
  id: number;
  fileName: string;
  documentType: string;
  documentName: string;
  section?: string | null;
  sectionEntityId?: number | null;
  sizeInBytes: number;
  createdAt: string;
};

export type BoardProposalRequestDetails = {
  id: number;
  meetingCode: string;
  meetingDate: string;
  meetingType: string;
  meetingFormat: string;
  secretaryEmployeeId: string;
  status: BoardProposalStatus;
  agendaItems: BoardProposalAgendaItemDetails[];
  attachments: AttachmentDetails[];
};

export const boardProposalApi = {
  create: (body: {
    meetingDate: string;
    meetingType: string;
    meetingFormat: string;
    secretaryEmployeeId: string;
  }) => api.post<number>("/board-proposal-requests", body),

  search: (id: number) =>
    api.get<BoardProposalRequestDetails>(
      `/board-proposal-requests/${id}/search`,
    ),

  nextStep: (id: number, action: EmployeeRequestAction, comment?: string) =>
    api.post<void>(`/board-proposal-requests/${id}/next-step`, {
      action,
      comment,
    }),

  addAgendaItem: (
    id: number,
    body: {
      title: string;
      initiatorEmployeeId: string;
      responsibleBoardMemberEmployeeId: string;
      presenterEmployeeId: string;
      category: string;
      description?: string;
    },
  ) => api.post<number>(`/board-proposal-requests/${id}/agenda-items`, body),

  setDecision: (
    agendaItemId: number,
    body: {
      decisionStatus: BoardProposalDecisionStatus;
      decisionText: string;
      finalVote?: string;
      notes?: string;
    },
  ) =>
    api.put<void>(
      `/board-proposal-requests/agenda-items/${agendaItemId}/decision`,
      body,
    ),

  addTask: (
    agendaItemId: number,
    body: {
      title: string;
      description?: string;
      responsibleEmployeeId: string;
      dueDate: string;
      status: BoardProposalTaskStatus;
    },
  ) =>
    api.post<number>(
      `/board-proposal-requests/agenda-items/${agendaItemId}/tasks`,
      body,
    ),

  reorderTasks: (
    agendaItemId: number,
    items: { id: number; order: number }[],
  ) =>
    api.put<void>(
      `/board-proposal-requests/agenda-items/${agendaItemId}/tasks/reorder`,
      { agendaItemId, items },
    ),

  updateTaskStatus: (
    taskId: number,
    body: {
      status: BoardProposalTaskStatus;
      extendedDueDate?: string;
      comment?: string;
    },
  ) =>
    api.put<void>(`/board-proposal-requests/tasks/${taskId}/status`, body),

  addVote: (
    agendaItemId: number,
    body: {
      boardMemberEmployeeId: string;
      voteType: string;
      notes?: string;
    },
  ) =>
    api.post<number>(
      `/board-proposal-requests/agenda-items/${agendaItemId}/votes`,
      body,
    ),

  deleteAttachment: (id: number) => api.del<void>(`/attachments/${id}`),
};

export function getStoredBoardProposalIds(): number[] {
  if (typeof window === "undefined") return [];
  const raw = window.localStorage.getItem("digi.boardProposalIds");
  if (!raw) return [];

  try {
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed)
      ? parsed.filter((item): item is number => typeof item === "number")
      : [];
  } catch {
    return [];
  }
}

export function storeBoardProposalId(id: number): void {
  const ids = getStoredBoardProposalIds();
  const next = [id, ...ids.filter((item) => item !== id)].slice(0, 20);
  window.localStorage.setItem("digi.boardProposalIds", JSON.stringify(next));
}
