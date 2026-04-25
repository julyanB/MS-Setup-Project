import { api } from "./api";

export type BoardProposalStatus =
  | "Draft"
  | "AgendaPreparation"
  | "MaterialsPreparation"
  | "ReadyForReview"
  | "Sent"
  | "Held"
  | "DecisionsRegistration"
  | "TaskMonitoring"
  | "Closed"
  | "Cancelled";

export type EmployeeRequestAction =
  | "Submit"
  | "MoveNext"
  | "Send"
  | "MarkHeld"
  | "RegisterDecisions"
  | "StartMonitoring"
  | "Close"
  | "Cancel"
  | "Reopen";

export type BoardProposalAgendaItemDetails = {
  id: number;
  title: string;
  initiatorEmployeeId: string;
  responsibleBoardMemberEmployeeId: string;
  presenterEmployeeId: string;
  category: string;
  description?: string | null;
  order: number;
  decisionStatus?: string | null;
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
  status: string;
  order?: number;
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
      decisionStatus: string;
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
      status: string;
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
